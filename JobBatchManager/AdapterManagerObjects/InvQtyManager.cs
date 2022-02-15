extern alias Erp_Contracts_BO_InventoryQtyAdj;

using System;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class InvQtyManager : AdapterBase<InventoryQtyAdjAdapter>, IAdapterBase
    {
        public InvQtyManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public bool AdjQty(string partNum, string lotNum, string whsCode, string binNum, string uomCode, DateTime tranDate, decimal quantity, out string partTran, string reason = "Prod")
        {
            partTran = string.Empty;
            if (!ValidateDatas(partNum, ref uomCode, quantity))
            {
                return false;
            }
            if (!SetDefaultDatas(partNum, lotNum, whsCode, binNum, tranDate, quantity, reason))
            {
                return false;
            }
            if (!ProcessInvAdj(partNum, lotNum, whsCode, binNum, uomCode, quantity, out partTran))
            {
                return false;
            }
            return true;
        }

        private bool ValidateDatas(string partNum, ref string uomCode, decimal quantity)
        {
            try
            {
                if (quantity == decimal.Zero)
                {
                    throw new Exception("Impossible de faire un ajustement de 0 quantité.");
                }
                this.adapter.ClearData();
                if (!GetPartXRefInfo(ref partNum, ref uomCode))
                {
                    return false;
                }
                if (!KitStatusVerification(partNum))
                {
                    return false;
                }
                if (!PartExists(partNum))
                {
                    return false;
                }
                if (!GetPartAdjustmentInfo(partNum, uomCode))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool SetDefaultDatas(string partNum, string lotNum, string whsCode, string binNum, DateTime tranDate, decimal quantity, string reason)
        {
            if (!SetWarehouseCode(partNum, whsCode))
            {
                return false;
            }
            if (!SetBinNum(whsCode, binNum))
            {
                return false;
            }
            DataRow rw = this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows[0];
            rw.BeginEdit();
            rw["TransDate"] = tranDate;
            rw["ReasonCode"] = reason;
            rw["AdjustQuantity"] = quantity;
            rw["LotNum"] = lotNum;
            rw.EndEdit();
            return true;
        }

        private bool ProcessInvAdj(string partNum, string lotNum, string whsCode, string binNum, string uomCode, decimal quantity, out string partTran)
        {
            partTran = "";
            if (!NegativeInventoryTest(partNum, lotNum, whsCode, binNum, uomCode, quantity))
            {
                return false;
            }
            bool requiresUserInput = false;
            string partTranPKs;
            try
            {
                this.adapter.PreSetInventoryQtyAdj(out requiresUserInput);
                this.adapter.SetInventoryQtyAdj(this.adapter.InventoryQtyAdjData, out partTranPKs);
                partTran = partTranPKs;
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool GetPartXRefInfo(ref string proposedPartNum, ref string uomCode)
        {
            bool flag = true;
            bool multipleMatch = false;
            string serialWarning = "";
            string questionString = "";
            string sysRowID = "";
            string rowType = "";
            try
            {
                flag = this.adapter.GetPartXRefInfo(ref proposedPartNum, sysRowID, rowType, ref uomCode, out serialWarning, out questionString, out multipleMatch);
                if (!flag)
                {
                    return flag;
                }
                if (multipleMatch)
                {
                    throw new Exception("Plusieurs matchs pour la pièce, l'ajustement d'inventaire doit être fait manuellement.");
                }
                return flag;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool KitStatusVerification(string partNum)
        {
            try
            {
                string kitMessage = "";
                this.adapter.KitPartStatus(partNum, out kitMessage);
                if (kitMessage.Length > 0)
                {
                    throw new Exception("La pièce est un kit");
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool PartExists(string partNum)
        {
            try
            {
                if (!((PartManager)this.adapterManager.GetManager("PartManager")).PartExists(partNum))
                {
                    throw new Exception("La pièce n'existe pas");
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool GetPartAdjustmentInfo(string partNum, string uomCode)
        {
            Erp_Contracts_BO_InventoryQtyAdj::Erp.BO.InventoryQtyAdjDataSet inventoryQtyAdjDataSet = null;
            try
            {
                inventoryQtyAdjDataSet = this.adapter.GetInventoryQtyAdj(partNum, uomCode);
                this.adapter.InventoryQtyAdjData.Clear();
                this.adapter.InventoryQtyAdjData.Merge(inventoryQtyAdjDataSet, false, MissingSchemaAction.Ignore);
                inventoryQtyAdjDataSet = null;
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                inventoryQtyAdjDataSet = null;
                return false;
            }
        }

        private bool SetWarehouseCode(string partNum, string whsCode)
        {
            try
            {
                if (this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows.Count == 0)
                {
                    throw new UIException();
                }
                DataRow rw = this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows[0];
                string w = rw["WareHseCode"].ToString();
                if (!whsCode.Equals(w))
                {
                    if (((PartPlantWarehouseManager)this.adapterManager.GetManager("PartPlantWarehouseManager")).ValidateWarehouseCode(partNum, whsCode))
                    {
                        string primaryBin = "";
                        Erp_Contracts_BO_InventoryQtyAdj::Erp.BO.InventoryQtyAdjBrwDataSet inventoryQtyAdjBrwDS = this.adapter.GetInventoryQtyAdjBrw(partNum, whsCode, out primaryBin);
                        if (inventoryQtyAdjBrwDS.InventoryQtyAdjBrw.Rows.Count > 0)
                        {
                            rw.BeginEdit();
                            rw["WareHseCode"] = whsCode;
                            rw["BinNum"] = primaryBin;
                            rw.EndEdit();
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool SetBinNum(string whsCode, string binNum)
        {
            try
            {
                if (this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows.Count == 0)
                {
                    throw new UIException();
                }
                DataRow rw = this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows[0];
                string b = rw["BinNum"].ToString();
                if (!binNum.Equals(b))
                {
                    if (((WarehouseBinManager)this.adapterManager.GetManager("WarehouseBinManager")).ValidateBinNum(whsCode, binNum))
                    {
                        rw.BeginEdit();
                        rw["BinNum"] = binNum;
                        rw.EndEdit();
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool NegativeInventoryTest(string partNum, string lotNum, string whsCode, string binNum, string uomCode, decimal qty)
        {
            try
            {
                if (this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows.Count == 0)
                {
                    throw new UIException();
                }
                DataRow rw = this.adapter.InventoryQtyAdjData.InventoryQtyAdj.Rows[0];

                string pcID = "";
                decimal one = decimal.One;
                string pcNeqQtyAction;
                string pcMessage;
                this.adapter.NegativeInventoryTest(partNum, whsCode, binNum, lotNum, pcID, uomCode, one, -qty, out pcNeqQtyAction, out pcMessage);

                if (string.Compare(pcNeqQtyAction, "Ask User", true) == 0)
                {
                    rw.BeginEdit();
                    rw["AllowNegQty"] = true;
                    rw.EndEdit();
                    return true;
                }
                else
                {
                    if (string.Compare(pcNeqQtyAction, "Stop", true) != 0)
                    {
                        return true;
                    }
                }
                throw new Exception(string.Format("Erreur d'inventaire négatif pour la pièce {0}", partNum));
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }
    }
}
