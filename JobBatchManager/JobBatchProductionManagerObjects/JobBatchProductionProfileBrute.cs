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
    public class JobBatchProductionProfileBrut : JobBatchProductionManager, IJobBatchProductionManager
    {
        public JobBatchProductionProfileBrut(CustomScriptManager csm) : base(csm) { }
        protected override void SetJobBatchJobNum()
        {
            jobBatchJobNum = GetJobFictiveForType("ProfileBrut");
        }
        public string JobBatchTitle { get { return "JobBatch Profilé brut"; } }
        protected override void SetPanels()
        {
            try
            {
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "Production");
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "RebutProduction");
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
        public override Dictionary<string, string> GetProductionLayout()
        {
            return new Dictionary<string, string>() {
                { "ChildKey3", "ID" },
                { "Character07", "No BT" },
                { "Character09", "OpCode" },
                { "Character05", "No lot" },
                { "Character01", "No pièce" },
                { "Number01", "BarLength" },
                { "Number02", "Qte à produire" },
                { "Number04", "Entré de qte" },
                { "Number05", "Qte produite" }
            };
        }
        public override Dictionary<string, string> GetRebutProductionLayout()
        {
            return new Dictionary<string, string>() {
                { "ChildKey3", "ID" },
                { "Character07", "No BT" },
                { "Character09", "OpCode" },
                { "Character05", "No lot" },
                { "ShortChar03", "Raison de rejet" },
                { "Character01", "No pièce" },
                { "Number01", "BarLength" },
                { "Number05", "Qte rejettée" }
            };
        }
        public override string BeforeProductionKeyChange(string proposedValue)
        {
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            return ((BarlistManager)this.adapterManager.GetManager("BarlistManager")).ValidateBarlistIDProposedValue(proposedValue, refPoincon);
        }
        public override bool AfterProductionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow)
        {
            DataRow barlistRow;
            //DataTable lots;
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            string opCode;
            if (((BarlistManager)this.adapterManager.GetManager("BarlistManager")).GetBarlistLineByID(keyValue, refPoincon, out barlistRow, out opCode))
            {
                string jobNum = barlistRow["Key3"].ToString();
                string noLot = "";
                if (((GestionLotManager)this.adapterManager.GetManager("GestionLotManager")).GetLotForJobNum(jobNum, out noLot))
                {

                }
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Number01"] = barlistRow["Number01"];
                newRow["Number02"] = barlistRow["Number02"];
                newRow["Character05"] = noLot;
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                //newRow["CheckBox02"] = autoSetLot;

                return true;
            }
            return false;
        }
        public override string BeforeRebutProductionKeyChange(string proposedValue, EpiTransaction oTrans)
        {
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            return ((BarlistManager)this.adapterManager.GetManager("BarlistManager")).ValidateBarlistIDProposedValue(proposedValue, refPoincon);
        }
        public override bool AfterRebutProductionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow)
        {
            DataRow barlistRow;
            //DataTable lots;
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            string opCode;
            if (((BarlistManager)this.adapterManager.GetManager("BarlistManager")).GetBarlistLineByID(keyValue, refPoincon, out barlistRow, out opCode))
            {
                string jobNum = barlistRow["Key3"].ToString();
                string noLot = "";
                if (((GestionLotManager)this.adapterManager.GetManager("GestionLotManager")).GetLotForJobNum(jobNum, out noLot))
                {

                }
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Number01"] = barlistRow["Number01"];
                newRow["Character05"] = noLot;
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                //newRow["CheckBox02"] = autoSetLot;

                return true;
            }
            return false;
        }
        public override string BeforeProductionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans)
        {
            try
            {
                switch (fieldName)
                {
                    case "Number04":
                        decimal qteProduite = decimal.Parse(GetView("Production").CurrentDataRow["Number05"].ToString());
                        decimal qteEntree = decimal.Parse(proposedValue);
                        if ((qteEntree + qteProduite) < 0m)
                        {
                            throw new Exception("La quantité produite ne peut pas être négative");
                        }
                        /*string lotNum = GetView("Production").CurrentDataRow["Character05"].ToString();
						if( (qteEntree + qteProduite) > 0m )
						{
							if( string.IsNullOrEmpty(lotNum) )
							{
								throw new Exception("Veuillez sélectionner un numéro de lot avant d'entrer une quantité");
							}
							GetView("Production").CurrentDataRow["CheckBox03"] = true;
						}*/
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
        public override bool AfterProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange, out bool refreshAfterChange)
        {
            saveAfterChange = false;
            refreshAfterChange = false;
            switch (fieldName)
            {
                case "Number04":
                    saveAfterChange = true;
                    break;
            }
            return true;
        }
        public override string BeforeRebutProductionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans)
        {
            try
            {
                switch (fieldName)
                {
                    case "Number05":
                        string raison = GetView("RebutProduction").CurrentDataRow["ShortChar03"].ToString();
                        decimal qteEntree = decimal.Parse(proposedValue);
                        if (qteEntree < 0m)
                        {
                            throw new Exception("La quantité rejettée ne peut pas être négative");
                        }
                        if (string.IsNullOrEmpty(raison))
                        {
                            throw new Exception("Vous devez sélectionner une raison de rejet.");
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
        public override bool AfterRebutProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
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
            RowRule disableProduction = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "Production", new string[3]{
                    "ChildKey3",
                    "ShortChar01",
                    "Number04"
                })
            });
            edv.AddRowRule(disableProduction);
            RowRule disableProduction2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.ChildKey3", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction2);
            RowRule disableRebutProduction = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "RebutProduction", new string[3]{
                    "ChildKey3",
                    "ShortChar03",
                    "Number05"
                })
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction);
            RowRule disableRebutProduction2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ChildKey3", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction2);
            RowRule disableRebutProduction3 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar03", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction3);
            /*RowRule disableProduction3 = new RowRule("CheckBox02", RuleCondition.Equals, true, new RuleAction[]{
				RuleAction.AddControlSettings(oTrans, "Production.Character05", SettingStyle.ReadOnly)
			});
			edv.AddRowRule(disableProduction3);
			RowRule disableProduction4 = new RowRule("CheckBox03", RuleCondition.Equals, true, new RuleAction[]{
				RuleAction.AddControlSettings(oTrans, "Production.Character05", SettingStyle.ReadOnly)
			});
			edv.AddRowRule(disableProduction4);*/
        }
    }
}
