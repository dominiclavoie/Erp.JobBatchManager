using System;
using System.Windows.Forms;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class IssueMaterialManager : AdapterBase<IssueReturnAdapter>, IAdapterBase
    {
        public IssueMaterialManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }
        private bool plNegQtyAction;
        private string tranType;
        private string callProcess;

        public bool IssueMaterial(string job, string company, string mtlSeq, decimal tranQty, string lotNum, string issuedPartNum = "")
        {
            this.tranType = "STK-MTL";
            this.callProcess = "IssueMaterial";
            if (!GetNew())
            {
                return false;
            }
            if (!GetNewJobAsmbl(job, company))
            {
                return false;
            }
            if (!SetMtlSeq(mtlSeq))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(issuedPartNum))
            {
                if (!SetIssuedPartNum(issuedPartNum))
                {
                    return false;
                }
            }
            if (!SetTranQty(tranQty))
            {
                return false;
            }
            if (!SetLotNum(lotNum))
            {
                return false;
            }
            if (!PerformMaterialMovement())
            {
                return false;
            }
            return true;
        }

        public bool ReturnMaterial(string job, string company, string mtlSeq, decimal tranQty, string lotNum, string issuedPartNum = "")
        {
            this.tranType = "MTL-STK";
            this.callProcess = "ReturnMaterial";
            if (!GetNew())
            {
                return false;
            }
            if (!GetNewJobAsmbl(job, company))
            {
                return false;
            }
            if (!SetMtlSeq(mtlSeq))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(issuedPartNum))
            {
                if (!SetIssuedPartNum(issuedPartNum))
                {
                    return false;
                }
            }
            if (!SetTranQty(tranQty))
            {
                return false;
            }
            if (!SetLotNum(lotNum))
            {
                return false;
            }
            if (!PerformMaterialMovement())
            {
                return false;
            }
            return true;
        }

        private bool GetNew()
        {
            try
            {
                Guid empty = Guid.Empty;
                adapter.GetNewIssueReturn(this.tranType, empty, this.callProcess);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool GetNewJobAsmbl(string job, string company)
        {
            try
            {
                adapter.SelectedJobAsmblDataSet.SelectedJobAsmbl.Clear();
                adapter.ClearData();
                DataRow dataRow = adapter.SelectedJobAsmblDataSet.SelectedJobAsmbl.NewRow();
                dataRow["JobNum"] = job;
                dataRow["Company"] = company;
                dataRow["AssemblySeq"] = "0";
                adapter.SelectedJobAsmblDataSet.SelectedJobAsmbl.Rows.Add(dataRow);
                Guid empty = Guid.Empty;
                string pcMessage = "";
                adapter.GetNewJobAsmblMultiple(this.tranType, empty, this.callProcess, out pcMessage);
                if (pcMessage != "")
                {
                    EpiMessageBox.Show(pcMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool SetMtlSeq(string mtlSeq)
        {
            try
            {
                if (this.callProcess == "IssueMaterial" && !adapter.OnChangingToJobSeq(int.Parse(mtlSeq)))
                {
                    throw new UIException();
                }
                if (this.callProcess == "ReturnMaterial" && !adapter.OnChangingJobSeq(int.Parse(mtlSeq), "From", this.callProcess))
                {
                    throw new UIException();
                }
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                string colName = this.callProcess == "IssueMaterial" ? "ToJobSeq" : "FromJobSeq";
                row[colName] = int.Parse(mtlSeq);
                row["RowMod"] = "U";
                string pcMessage = "";
                if (this.callProcess == "IssueMaterial")
                {
                    adapter.OnChangeToJobSeq(this.callProcess, out pcMessage);
                }
                else
                {
                    adapter.OnChangeFromJobSeq(this.callProcess, out pcMessage);
                }
                if (!string.IsNullOrEmpty(pcMessage))
                {
                    EpiMessageBox.Show(pcMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool SetIssuedPartNum(string issuedPartNum)
        {
            try
            {
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                row["PartNum"] = issuedPartNum;
                row["RowMod"] = "U";
                adapter.OnChangePartNum(this.callProcess);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool SetTranQty(decimal tranQty)
        {
            try
            {
                if (!adapter.OnChangeTranQty(tranQty))
                {
                    throw new UIException();
                }
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                row["TranQty"] = tranQty;
                row["RowMod"] = "U";
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool SetLotNum(string lotNum)
        {
            try
            {
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                if (!Convert.ToBoolean(row["PartTrackLots"].ToString()))
                {
                    return true;
                }
                if (!adapter.OnChangeLotNum(lotNum))
                {
                    throw new UIException();
                }

                row["LotNum"] = lotNum;
                row["RowMod"] = "U";
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool SetWarehouseCode(string whseCode)
        {
            try
            {
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                row["RowMod"] = "U";
                if (this.callProcess == "IssueMaterial")
                {
                    row["FromWarehouseCode"] = whseCode;
                    adapter.onChangeFromWarehouse(this.callProcess);
                }
                if (this.callProcess == "ReturnMaterial")
                {
                    row["ToWarehouseCode"] = whseCode;
                    adapter.onChangeToWarehouse(this.callProcess);
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool SetBinNum(string binNum)
        {
            try
            {
                string pcMessage;
                bool plOverrideBinChange = false;
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                row["RowMod"] = "U";
                if (this.callProcess == "IssueMaterial")
                {
                    adapter.OnChangingFromBinNum(out pcMessage);
                    if (pcMessage != "")
                    {
                        plOverrideBinChange = true;
                    }
                    row["FromBinNum"] = binNum;
                    adapter.OnChangeFromBinNum(plOverrideBinChange);
                }
                if (this.callProcess == "ReturnMaterial")
                {
                    row["ToBinNum"] = binNum;
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool PerformMaterialMovement()
        {
            try
            {
                DataRow row = adapter.IssueReturnData.IssueReturn.Rows[0];
                string partNum = row["PartNum"].ToString();
                string lotNum = row["LotNum"].ToString();
                ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetProfileBrutLocalisation(partNum, lotNum);
                if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LocalisationProfileBrut"].Rows.Count == 0)
                {
                    throw new Exception("La localisation de la pièce est introuvable. Veuillez contacter l'administrateur du système.");
                }
                DataRow locRow = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LocalisationProfileBrut"].Rows[0];
                string whse = locRow["WarehouseCode"].ToString();
                string bin = locRow["BinNum"].ToString();
                SetWarehouseCode(whse);
                SetBinNum(bin);
                bool requiresUserInput = false;
                if (!adapter.PrePerformMaterialMovement(out requiresUserInput))
                {
                    throw new UIException();
                }
                if (!MasterInventoryBinTests())
                {
                    throw new UIException();
                }
                string legalNumberMessage = "";
                string partTranPKs;
                adapter.PerformMaterialMovement(plNegQtyAction, out legalNumberMessage, out partTranPKs);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        private bool MasterInventoryBinTests()
        {
            bool flag = true;
            string pcNeqQtyAction = string.Empty;
            string pcNeqQtyMessage = string.Empty;
            string pcPCBinAction = string.Empty;
            string pcPCBinMessage = string.Empty;
            string pcOutBinAction = string.Empty;
            string pcOutBinMessage = string.Empty;
            adapter.MasterInventoryBinTests(out pcNeqQtyAction, out pcNeqQtyMessage, out pcPCBinAction, out pcPCBinMessage, out pcOutBinAction, out pcOutBinMessage);
            if (pcNeqQtyMessage != "")
            {
                switch (pcNeqQtyAction.ToUpper())
                {
                    case "STOP":
                        EpiMessageBox.Show(pcNeqQtyMessage, "Error", MessageBoxButtons.OK);
                        plNegQtyAction = false;
                        flag = false;
                        break;
                    case "NONE":
                        plNegQtyAction = false;
                        flag = true;
                        break;
                    case "ASK USER":
                        if (EpiMessageBox.Show(pcNeqQtyMessage, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                        {
                            plNegQtyAction = true;
                            flag = true;
                        }
                        else
                        {
                            plNegQtyAction = false;
                            flag = false;
                        }
                        break;
                }
                if (!flag)
                {
                    return flag;
                }
            }
            if (pcPCBinMessage != "")
            {
                switch (pcPCBinAction.ToUpper())
                {
                    case "STOP":
                        EpiMessageBox.Show(pcPCBinMessage, "Error", MessageBoxButtons.OK);
                        flag = false;
                        break;
                    case "NONE":
                        flag = true;
                        break;
                    case "WARN":
                        flag = ((EpiMessageBox.Show(pcPCBinMessage, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes) ? true : false);
                        break;
                }
                if (!flag)
                {
                    return flag;
                }
            }
            if (pcOutBinMessage != "")
            {
                switch (pcOutBinAction.ToUpper())
                {
                    case "STOP":
                        EpiMessageBox.Show(pcOutBinMessage, "Error", MessageBoxButtons.OK);
                        flag = false;
                        break;
                    case "NONE":
                        flag = true;
                        break;
                    case "WARN":
                        flag = ((EpiMessageBox.Show(pcOutBinMessage, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes) ? true : false);
                        break;
                }
            }
            return flag;
        }

    }
}
