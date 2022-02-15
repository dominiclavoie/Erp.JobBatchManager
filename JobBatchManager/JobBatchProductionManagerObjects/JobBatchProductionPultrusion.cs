using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Infragistics.Win.UltraWinGrid;
using JobBatchManagerObjects.AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchProductionManagerObjects
{
    public class JobBatchProductionPultrusion : JobBatchProductionManager, IJobBatchProductionManager
    {
        public JobBatchProductionPultrusion(CustomScriptManager csm) : base(csm) { }
        protected override void SetJobBatchJobNum()
        {
            jobBatchJobNum = GetJobFictiveForType("Pultrusion");
        }
        public string JobBatchTitle { get { return "Production Pultrusion"; } }
        protected override void SetPanels()
        {
            try
            {
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "Pultrusion");
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
        public override Dictionary<string, string> GetPultrusionLayout()
        {
            return new Dictionary<string, string>() {
                { "ShortChar01", "Profilé" },
                { "Character05", "No lot" },
                { "Number05", "Qte produite" }
            };
        }
        public override string BeforePultrusionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans)
        {
            try
            {
                switch (fieldName)
                {
                    case "Number05":
                        decimal qteEntree = decimal.Parse(proposedValue);
                        if (qteEntree < 0m)
                        {
                            proposedValue = currentValue;
                            throw new Exception("La quantité produite ne peut pas être négative");
                        }
                        string profile = GetView("Pultrusion").CurrentDataRow["ShortChar01"].ToString();
                        if (string.IsNullOrEmpty(profile))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un profilé");
                        }
                        string lotNum = GetView("Pultrusion").CurrentDataRow["Character05"].ToString();
                        if (string.IsNullOrEmpty(lotNum))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez entrer un numéro de lot");
                        }
                        decimal oldQty = decimal.Parse(GetView("Pultrusion").CurrentDataRow["Number05"].ToString());
                        decimal tranQty = (qteEntree - oldQty);
                        ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetProfileBrutLocalisation(profile, lotNum);
                        if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LocalisationProfileBrut"].Rows.Count == 0)
                        {
                            proposedValue = currentValue;
                            throw new Exception("La localisation de la pièce est introuvable. Veuillez contacter l'administrateur du système.");
                        }
                        DataRow locRow = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LocalisationProfileBrut"].Rows[0];
                        string whse = locRow["WarehouseCode"].ToString();
                        string bin = locRow["BinNum"].ToString();
                        string ium = locRow["IUM"].ToString();
                        DateTime dateJobBatch;
                        if (!DateTime.TryParse(GetView("JobBatch").CurrentDataRow["Date01"].ToString(), out dateJobBatch))
                        {
                            proposedValue = currentValue;
                            throw new Exception("La date est invalide.");
                        }
                        string partTran;
                        bool flag = ((InvQtyManager)this.adapterManager.GetManager("InvQtyManager")).AdjQty(profile, lotNum, whse, bin, ium, dateJobBatch, tranQty, out partTran, "Prod");
                        if (!flag)
                        {
                            proposedValue = currentValue;
                            throw new Exception("Une erreur est survenue.");
                        }
                        if (tranQty > 0m)
                        {
                            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetPartFIFOCost(profile, lotNum, partTran);
                            if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["ProfilePartFIFOCost"].Rows.Count == 0)
                            {
                                proposedValue = currentValue;
                                throw new Exception("Le coût de la pièce est introuvable. Veuillez contacter l'administrateur du système.");
                            }
                            DataRow costRow = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["ProfilePartFIFOCost"].Rows[0];
                            DateTime fifoDate = DateTime.Parse(costRow["FIFODate"].ToString());
                            int fifoSeq = int.Parse(costRow["FIFOSeq"].ToString());
                            int fifoSubSeq = int.Parse(costRow["FIFOSubSeq"].ToString());
                            decimal materialCost = decimal.Parse(costRow["MaterialCost"].ToString());
                            decimal laborCost = decimal.Parse(costRow["LaborCost"].ToString());
                            decimal burdenCost = decimal.Parse(costRow["BurdenCost"].ToString());
                            ((CostAdjustmentManager)this.adapterManager.GetManager("CostAdjustmentManager")).AdjustCost(profile, lotNum, fifoDate, fifoSeq, fifoSubSeq, materialCost, laborCost, burdenCost);
                        }
                        break;

                    case "Character05":
                        if(proposedValue.Length != 7)
                        {
                            throw new Exception("Le numéro de lot doit contenir 7 chiffres");
                        }
                        int n;
                        if(!int.TryParse(proposedValue, out n))
                        {
                            throw new Exception("Le numéro de lot doit contenir 7 chiffres");
                        }
                        if (proposedValue[2] != '0')
                        {
                            throw new Exception("Le 3e chiffre doit être un 0");
                        }
                        if (proposedValue[3] == '0')
                        {
                            throw new Exception("Le 4e chiffre ne peut pas être un 0");
                        }
                        if (proposedValue[6] == '0')
                        {
                            throw new Exception("Le 7e chiffre ne peut pas être un 0");
                        }
                        int year = int.Parse(DateTime.Today.ToString("yy"));
                        int[] years = new int[] { year-1, year, year+1 };
                        int lotYear = int.Parse(proposedValue.Substring(0, 2));
                        if (!years.Contains(lotYear))
                        {
                            throw new Exception("Les 2 premiers chiffres entrés référant à l'année ne sont pas valides.");
                        }
                        break;

                }
                return proposedValue;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return currentValue;
            }
        }
        public override bool AfterPultrusionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
        {
            saveAfterChange = false;
            switch (fieldName)
            {
                case "Number05":
                    saveAfterChange = true;
                    break;
            }
            return true;
        }
        protected override void SetupRowRule(EpiTransaction oTrans, ref EpiDataView edv)
        {
            RowRule disablePultrusion = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "Pultrusion", new string[3]{
                    "Number05",
                    "ShortChar01",
                    "Character05"
                })
            });
            GetView("Pultrusion").AddRowRule(disablePultrusion);
            RowRule disablePultrusion2 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Pultrusion.ShortChar01", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "Pultrusion.Character05", SettingStyle.ReadOnly)
            });
            GetView("Pultrusion").AddRowRule(disablePultrusion2);

            RowRule disableButtons = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonsEnabled", SettingStyle.ReadOnly)
            });
            GetView("FormView").AddRowRule(disableButtons);
        }
    }
}
