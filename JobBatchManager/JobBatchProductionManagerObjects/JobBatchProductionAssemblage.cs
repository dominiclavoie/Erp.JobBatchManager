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
    public class JobBatchProductionAssemblage : JobBatchProductionManager, IJobBatchProductionManager
    {
        public JobBatchProductionAssemblage(CustomScriptManager csm) : base(csm) { }
        protected override void SetJobBatchJobNum()
        {
            jobBatchJobNum = GetJobFictiveForType("Hydro");
        }
        public string JobBatchTitle { get { return "Production Hydro"; } }
        public override string EmployeeListFilter { get { return "JCDept = 'Hydro' AND ShopSupervisor = 0"; } }
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
                { "Character09", "Opération" },
                { "Character01", "No pièce" },
                { "Character05", "No lot" },
                { "Number02", "Qte à produire" },
                { "Number05", "Qte produite" },
                { "Number20", "Qte restante" }
            };
        }
        public override Dictionary<string, string> GetRebutProductionLayout()
        {
            return new Dictionary<string, string>() {
                { "ChildKey3", "ID" },
                { "Character07", "No BT" },
                { "Character09", "Opération" },
                { "Character01", "No pièce" },
                { "Character05", "No lot" },
                { "ShortChar03", "Raison de rejet" },
                { "Number05", "Qte rejettée" }
            };
        }
        private void GetLotNumForLastOperation(string jobNum, string opCode)
        {
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetLotNumForLastOperation(jobNum, opCode);
        }
        private void GetListScrapReasonForOperation(string opCode)
        {
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetListScrapReasonForOperation(opCode);
        }
        public override void OnGetRow(string viewName, DataRowView row, EpiTransaction oTrans, bool forceRefresh)
        {
            if (!this.codeChangeData && GetView("JobBatch").HasRow)
            {
                string jobNum = row["Character07"].ToString();
                string opCode = row["Character09"].ToString();
                if (((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UsePrevOpLot", opCode))
                {
                    GetLotNumForLastOperation(jobNum, opCode);
                    switch (viewName)
                    {
                        case "Production":
                            RefreshComboDataSource("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LastOperationLotNum"], false);
                            break;
                        case "RebutProduction":
                            RefreshComboDataSource("02ca5353-9363-45f3-8e52-4dc8173389d8", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LastOperationLotNum"], false);
                            break;
                    }
                }
                if (viewName.Equals("RebutProduction"))
                {
                    GetListScrapReasonForOperation(opCode);
                    RefreshComboDataSource("96a1d1bf-cbe1-458c-b3a0-0d087594741b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["RebutProductionRaisonRejet"], false);
                }
            }
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
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            if (((BarlistManager)this.adapterManager.GetManager("BarlistManager")).GetBarlistLineByID(keyValue, refPoincon, out DataRow barlistRow, out string opCode))
            {
                string jobNum = barlistRow["Key3"].ToString();
                bool usePrevOpLot = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UsePrevOpLot", opCode);

                int oprSeq = int.Parse(barlistRow["Key4"].ToString());
                this.codeChangeData = true;
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Number02"] = barlistRow["Number02"];
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                newRow["Number20"] = barlistRow["Number20"];
                newRow["CheckBox03"] = !usePrevOpLot;
                this.codeChangeData = false;
                if (usePrevOpLot)
                {
                    GetLotNumForLastOperation(jobNum, opCode);
                    RefreshComboDataSource("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LastOperationLotNum"], false);
                }

                return true;
            }
            return false;
        }
        public override string BeforeRebutProductionKeyChange(string proposedValue, EpiTransaction oTrans)
        {
            if (string.IsNullOrEmpty(proposedValue))
            {
                return "";
            }
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            string validValue = ((BarlistManager)this.adapterManager.GetManager("BarlistManager")).ValidateBarlistIDProposedValue(proposedValue, refPoincon, out string opCode);
            bool flag = false;
            string[] opInteractions = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetInteractionsForOpCode(opCode);
            string[] requireIssueParts = new string[] { "UseMtlLot" };
            string[] requireQtyEntry = new string[] { "UsePrevOpLot", "ScrapMtlLot" };
            if (!string.IsNullOrEmpty(validValue) && opInteractions.Any(x => requireIssueParts.Contains(x)))
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>() { { "View", "RebutProduction" }, { "OpCode", opCode }, { "IDLigne", validValue } };
                LaunchForm(oTrans, "UDPRLTAS", FormParameters(parameters));
                flag = !string.IsNullOrEmpty(GetView("RebutProduction").CurrentDataRow["Character10"].ToString());
            }
            else if (!string.IsNullOrEmpty(validValue) && opInteractions.Any(x => requireQtyEntry.Contains(x)))
            {
                flag = true;
            }
            else
            {
                ExceptionBox.Show(new Exception("Les rejets ne sont pas admissibles pour cette opération"));
            }
            if (!flag)
            {
                return "";
            }
            return validValue;
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
                string[] opInteractions = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetInteractionsForOpCode(opCode);
                string[] requireQtyEntry = new string[] { "UsePrevOpLot", "ScrapMtlLot" };
                bool qtyEnabled = opInteractions.Any(x => requireQtyEntry.Contains(x));
                bool lotLocked = !opInteractions.Contains("UsePrevOpLot");
                string jobNum = barlistRow["Key3"].ToString();
                int oprSeq = int.Parse(barlistRow["Key4"].ToString());
                this.codeChangeData = true;
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                newRow["CheckBox03"] = lotLocked;
                newRow["CheckBox04"] = !qtyEnabled;
                this.codeChangeData = false;
                if (opInteractions.Contains("UseMtlLot"))
                {
                    try
                    {
                        OnBeforeTranQtyChange("RebutProduction", new string[] { "ScrapMtlLot", "UsePrevOpLot" }, new string[] { "ScrapMtlLot", "UseMtlLot" }, decimal.Parse(GetView("RebutProduction").CurrentDataRow["Number05"].ToString()), false, true, true);
                    }
                    catch (Exception ex)
                    {
                        ExceptionBox.Show(ex);
                    }
                }
                if (opInteractions.Contains("UsePrevOpLot"))
                {
                    GetLotNumForLastOperation(jobNum, opCode);
                    RefreshComboDataSource("02ca5353-9363-45f3-8e52-4dc8173389d8", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LastOperationLotNum"], false);
                }
                GetListScrapReasonForOperation(opCode);
                RefreshComboDataSource("96a1d1bf-cbe1-458c-b3a0-0d087594741b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["RebutProductionRaisonRejet"], false);
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
                    case "Number05":
                        decimal qteEntree = GetPositiveProposedQty(proposedValue);
                        string idLigne = GetView("Production").CurrentDataRow["ChildKey3"].ToString();
                        string opCode = GetView("Production").CurrentDataRow["Character09"].ToString();
                        string lotNum = GetView("Production").CurrentDataRow["Character05"].ToString();
                        if (((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UseMtlLot", opCode))
                        {
                            if (string.IsNullOrEmpty(GetView("Production").CurrentDataRow["Character05"].ToString()))
                            {
                                Dictionary<string, string> parameters = new Dictionary<string, string>() { { "View", "Production" }, { "OpCode", opCode }, { "IDLigne", idLigne } };
                                LaunchForm(oTrans, "UDPRLTAS", FormParameters(parameters));
                            }
                        }
                        if (decimal.Parse(proposedValue) == 0m && !string.IsNullOrEmpty(lotNum))
                        {
                            string rowID = GetView("Production").CurrentDataRow["SysRowID"].ToString();
                            string message;
                            if (!((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).CheckCanDeleteLotNumForOperation(idLigne, lotNum, rowID, out message))
                            {
                                throw new Exception(message);
                            }
                        }

                        OnBeforeTranQtyChange("Production", new string[] { "UseMtlLot", "UsePrevOpLot" }, new string[] { "UseMtlLot" }, qteEntree, true);
                        if (decimal.Parse(proposedValue) == 0m)
                        {
                            this.codeChangeData = true;
                            GetView("Production").CurrentDataRow["Character05"] = "";
                            GetView("Production").CurrentDataRow["Character10"] = "";
                            GetView("Production").CurrentDataRow["ShortChar10"] = "";
                            this.codeChangeData = false;
                        }
                        break;

                    case "Character05":
                        if (this.codeChangeData)
                        {
                            return proposedValue;
                        }
                        string opCode2 = GetView("Production").CurrentDataRow["Character09"].ToString();
                        bool usePrevOpLot = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UsePrevOpLot", opCode2);
                        if (usePrevOpLot)
                        {
                            if (!((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).ItemInList("LastOperationLotNum", "LotNum", proposedValue))
                            {
                                throw new Exception("Veuillez sélectionner un numéro de lot valide.");
                            }
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
        public override bool AfterProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange, out bool refreshAfterChange)
        {
            saveAfterChange = false;
            refreshAfterChange = false;
            switch (fieldName)
            {
                case "Number05":
                    saveAfterChange = true;
                    refreshAfterChange = true;
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
                        if (this.codeChangeData)
                        {
                            return proposedValue;
                        }
                        decimal qteEntree = GetPositiveProposedQty(proposedValue);
                        string raison = GetView("RebutProduction").CurrentDataRow["ShortChar03"].ToString();
                        if (string.IsNullOrEmpty(raison))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner une raison de rejet.");
                        }
                        string idLigne = GetView("RebutProduction").CurrentDataRow["ChildKey3"].ToString();
                        string opCode = GetView("RebutProduction").CurrentDataRow["Character09"].ToString();
                        string[] opInteractions = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetInteractionsForOpCode(opCode);
                        decimal oldQty = decimal.Parse(GetView("RebutProduction").CurrentDataRow["Number05"].ToString());
                        string jobNum = GetView("RebutProduction").CurrentDataRow["Character07"].ToString();
                        string company = GetView("RebutProduction").CurrentDataRow["Company"].ToString();
                        decimal tranQty = qteEntree - oldQty;
                        if (opInteractions.Contains("ScrapMtlLot"))
                        {
                            if (string.IsNullOrEmpty(GetView("RebutProduction").CurrentDataRow["Character05"].ToString()))
                            {
                                Dictionary<string, string> parameters = new Dictionary<string, string>() { { "View", "RebutProduction" }, { "OpCode", opCode }, { "IDLigne", idLigne } };
                                LaunchForm(oTrans, "UDPRLTAS", FormParameters(parameters));
                            }
                        }
                        OnBeforeTranQtyChange("RebutProduction", new string[] { "ScrapMtlLot", "UsePrevOpLot" }, new string[] { "ScrapMtlLot", "UseMtlLot" }, qteEntree, true);
                        if (decimal.Parse(proposedValue) == 0m)
                        {
                            this.codeChangeData = true;
                            GetView("RebutProduction").CurrentDataRow["Character05"] = "";
                            GetView("RebutProduction").CurrentDataRow["Character10"] = "";
                            GetView("RebutProduction").CurrentDataRow["ShortChar10"] = "";
                            this.codeChangeData = false;
                        }
                        break;

                    case "Character05":
                        if (this.codeChangeData)
                        {
                            return proposedValue;
                        }
                        string opCode2 = GetView("RebutProduction").CurrentDataRow["Character09"].ToString();
                        bool usePrevOpLot = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UsePrevOpLot", opCode2);
                        if (usePrevOpLot)
                        {
                            if (!((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).ItemInList("LastOperationLotNum", "LotNum", proposedValue))
                            {
                                throw new Exception("Veuillez sélectionner un numéro de lot valide.");
                            }
                        }
                        break;

                    case "ShortChar03":
                        if (this.codeChangeData)
                        {
                            return proposedValue;
                        }
                        if (!((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).ItemInList("RebutProductionRaisonRejet", "CodeRaison", proposedValue))
                        {
                            throw new Exception("Veuillez sélectionner un code de raison valide.");
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
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;
                    }
                    break;
            }
            return true;
        }
        protected override void FormCallBack(DataRow response)
        {
            string view = response["SenderView"].ToString();
            this.SetCodeChangeData(true);

            GetView(view).CurrentDataRow["Character10"] = response["InfoLot"].ToString();
            string[] opInteractions = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetInteractionsForOpCode(response["OpCode"].ToString());
            bool scrapMtlLot = opInteractions.Contains("ScrapMtlLot");
            if (view.Equals("RebutProduction"))
            {
                if (scrapMtlLot)
                {
                    GetView(view).CurrentDataRow["Character05"] = response["LotNum"].ToString();
                    GetView(view).CurrentDataRow["ShortChar10"] = response["LotNum"].ToString();
                }
                else
                {
                    GetView(view).CurrentDataRow["Number05"] = response["ScrapQty"].ToString();
                }
            }
            else
            {
                GetView(view).CurrentDataRow["Character05"] = response["LotNum"].ToString();
                GetView(view).CurrentDataRow["ShortChar10"] = response["LotNum"].ToString();
            }
            this.SetCodeChangeData(false);
        }
        protected override void SetupRowRule(EpiTransaction oTrans, ref EpiDataView edv)
        {
            RowRule disableProduction = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "Production", new string[]{
                    "ChildKey3",
                    "Number05",
                    "Character05",
                    "ShortChar10"
                })
            });
            edv.AddRowRule(disableProduction);
            RowRule disableProduction2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.ChildKey3", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction2);
            RowRule disableProduction3 = new RowRule("RowMod", RuleCondition.Equals, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.Number05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "Production.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "Production.ShortChar10", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction3);
            RowRule disableProduction4 = new RowRule("CheckBox03", RuleCondition.Equals, true, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "Production.ShortChar10", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction4);
            RowRule disableProduction5 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "Production.ShortChar10", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction5);

            RowRule disableRebutProduction = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "RebutProduction", new string[]{
                    "ChildKey3",
                    "ShortChar03",
                    "Number05",
                    "Character05",
                    "ShortChar10"
                })
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction);
            RowRule disableRebutProduction2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ChildKey3", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction2);
            RowRule disableRebutProduction3 = new RowRule("RowMod", RuleCondition.Equals, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar03", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.Number05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar10", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction3);
            RowRule disableRebutProduction4 = new RowRule("CheckBox04", RuleCondition.Equals, true, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.Number05", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction4);
            RowRule disableRebutProduction5 = new RowRule("CheckBox03", RuleCondition.Equals, true, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar10", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction5);
            RowRule disableRebutProduction6 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar10", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction6);
            RowRule disableRebutProduction7 = new RowRule(null, delegate (RowRuleDelegateArgs args)
            {
                return !Convert.ToBoolean(args.Row["CheckBox04"].ToString()) &&
                       decimal.Parse(args.Row["Number05"].ToString()) > 0m;
            }, null, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar03", SettingStyle.ReadOnly)
            });

            GetView("RebutProduction").AddRowRule(disableRebutProduction7);
        }
    }
}
