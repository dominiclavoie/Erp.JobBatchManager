using System;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class CostAdjustmentManager : AdapterBase<CostAdjustmentAdapter>, IAdapterBase
    {
        public CostAdjustmentManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public bool AdjustCost(string partNum, string lotNum, DateTime fifoDate, int fifoSeq, int fifoSubSeq, decimal materialCost, decimal modCost, decimal fgfCost, string reasonCode = "CorrFIFO")
        {
            if (!GetCostAdjustment(partNum))
            {
                return false;
            }
            if (!GetLotCosts(lotNum))
            {
                return false;
            }
            if (!GetFIFOCosts(fifoDate, fifoSeq, fifoSubSeq))
            {
                return false;
            }
            if (!SetReasonCode(reasonCode))
            {
                return false;
            }
            bool noChangeMtlCost;
            if (!ChangeFIFOMaterialCost(materialCost, out noChangeMtlCost))
            {
                return false;
            }
            bool noChangeOtherCost;
            if (!ChangeOtherFIFOCost(modCost, fgfCost, out noChangeOtherCost))
            {
                return false;
            }
            if (noChangeMtlCost && noChangeOtherCost)
            {
                adapter.ClearData();
                return true;
            }
            if (!ProcessAdjustCost())
            {
                return false;
            }
            return true;
        }

        private bool GetCostAdjustment(string partNum)
        {
            this.adapter.ClearData();
            try
            {
                this.adapter.GetCostAdjustment(partNum);
                if (this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count == 0)
                {
                    throw new Exception("Le coût de la pièce est introuvable. Veuillez contacter l'administrateur du système.");
                }
                DataRow row = this.adapter.CostAdjustmentData.CostAdjustment.Rows[this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count - 1];
                if (row["CostMethod"].ToString() != "O")
                {
                    throw new Exception("La méthode de coût de la pièce est mal configuré. Veuillez contacter l'administrateur du système.");
                }
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool GetLotCosts(string lotNum)
        {
            try
            {
                DataRow row = this.adapter.CostAdjustmentData.CostAdjustment.Rows[this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count - 1];
                row["RowMod"] = "U";
                row["LotNum"] = lotNum;
                this.adapter.GetLotCosts();
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool GetFIFOCosts(DateTime fifoDate, int fifoSeq, int fifoSubSeq)
        {
            try
            {
                DataRow row = this.adapter.CostAdjustmentData.CostAdjustment.Rows[this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count - 1];
                row["RowMod"] = "U";
                row["FIFODate"] = fifoDate;
                row["TransDate"] = fifoDate;
                row["FIFOSeq"] = fifoSeq;
                row["FIFOSubSeq"] = fifoSubSeq;
                this.adapter.GetFIFOCosts();
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool SetReasonCode(string reasonCode)
        {
            try
            {
                DataRow row = this.adapter.CostAdjustmentData.CostAdjustment.Rows[this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count - 1];
                row["RowMod"] = "U";
                row["ReasonCode"] = reasonCode;
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool ChangeFIFOMaterialCost(decimal materialCost, out bool noChange)
        {
            noChange = false;
            try
            {
                DataRow row = this.adapter.CostAdjustmentData.CostAdjustment.Rows[this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count - 1];
                decimal currentCost = decimal.Parse(row["FIFOMaterialCost"].ToString());
                if (currentCost == materialCost)
                {
                    noChange = true;
                    return true;
                }
                row["RowMod"] = "U";
                if (!this.adapter.OnChangeFIFOMaterialCost(materialCost))
                {
                    throw new Exception("Le coût de la pièce est erroné. Veuillez contacter l'administrateur du système.");
                }
                row["FIFOMaterialCost"] = materialCost;
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool ChangeOtherFIFOCost(decimal modCost, decimal fgfCost, out bool noChange)
        {
            noChange = false;
            try
            {
                DataRow row = this.adapter.CostAdjustmentData.CostAdjustment.Rows[this.adapter.CostAdjustmentData.CostAdjustment.Rows.Count - 1];
                decimal currentLaborCost = decimal.Parse(row["FIFOLaborCost"].ToString());
                decimal currentBurdenCost = decimal.Parse(row["FIFOBurdenCost"].ToString());
                if (currentLaborCost == modCost && currentBurdenCost == fgfCost)
                {
                    noChange = true;
                    return true;
                }
                row["RowMod"] = "U";
                row["FIFOLaborCost"] = modCost;
                row["FIFOBurdenCost"] = fgfCost;
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }

        private bool ProcessAdjustCost()
        {
            try
            {
                bool requiresUserInput;
                if (!this.adapter.PreSetCostAdjustment(out requiresUserInput))
                {
                    throw new Exception("Une erreur est survenue dans l'ajustement du coût de la pièce. Veuillez contacter l'administrateur du système.");
                }
                string partTranPKs;
                this.adapter.SetCostAdjustment(out partTranPKs);
                return true;
            }
            catch (Exception e)
            {
                ExceptionBox.Show(e);
                return false;
            }
        }
    }
}
