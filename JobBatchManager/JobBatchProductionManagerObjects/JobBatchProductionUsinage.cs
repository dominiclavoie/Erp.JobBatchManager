using System;
using System.Collections.Generic;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Infragistics.Win.UltraWinGrid;
using JobBatchManagerObjects.AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchProductionManagerObjects
{
    public class JobBatchProductionUsinage : JobBatchProductionManager, IJobBatchProductionManager
    {
        public JobBatchProductionUsinage(CustomScriptManager csm) : base(csm) { }
        protected override void SetJobBatchJobNum()
        {
            jobBatchJobNum = GetJobFictiveForType("Volvo");
        }
        public string JobBatchTitle { get { return "Production Usinage"; } }
        public override string EmployeeListFilter { get { return "(JCDept = 'USI' AND ShopSupervisor = 0) OR EmpID IN('1630', '10042')"; } }
        protected override void SetPanels()
        {
            try
            {
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "Production");
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "RebutProduction");
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "ConsommationProfile");
                AddPanel("65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "DecoupeBrut");
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
                { "ShortChar01", "Profilé consommé" },
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
                { "ShortChar01", "Profilé consommé" },
                { "Character05", "No lot" },
                { "ShortChar03", "Raison de rejet" },
                { "Number05", "Qte rejettée" }
            };
        }
        public override Dictionary<string, string> GetRebutProfileLayout()
        {
            return new Dictionary<string, string>() {
                { "ChildKey3", "ID" },
                { "Character07", "No BT" },
                { "Character09", "Opération" },
                { "Character01", "No pièce" },
                { "ShortChar01", "Profilé jeté" },
                { "Character05", "No lot" },
                { "Number05", "Qte jetée" }
            };
        }
        public override Dictionary<string, string> GetDecoupeBrutLayout()
        {
            return new Dictionary<string, string>() {
                { "ShortChar01", "Profilé récupéré" },
                { "Character05", "No lot" },
                { "Number05", "Qte récupérée" }
            };
        }
        private void GetJobOperStockProfileBrut(string jobNum, int assemblySeq, int oprSeq, out string partNum, out int mtlSeq)
        {
            partNum = "";
            mtlSeq = -1;
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetJobOperStockProfileBrut(jobNum, assemblySeq, oprSeq);
            if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows.Count > 0)
            {
                partNum = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows[0]["PartNum"].ToString();
            }
            if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["JobRefProfileBrut"].Rows.Count > 0)
            {
                mtlSeq = int.Parse(((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["JobRefProfileBrut"].Rows[0]["MtlSeq"].ToString());
            }
        }
        private void GetStockProfileBrut(string partNum)
        {
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetStockProfileBrut(partNum);
        }
        private void GetListScrapReasonForOperation(string opCode)
        {
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetListScrapReasonForOperation(opCode);
        }
        public override void OnGetRow(string viewName, DataRowView row, EpiTransaction oTrans, bool forceRefresh)
        {
            if (!this.codeChangeData && GetView("JobBatch").HasRow)
            {
                //string resID = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                string jobNum = row["Character07"].ToString();
                int oprSeq = int.Parse(row["Character08"].ToString());
                string partNumProfile;
                int mtlSeq;
                GetJobOperStockProfileBrut(jobNum, 0, oprSeq, out partNumProfile, out mtlSeq);
                partNumProfile = row["ShortChar01"].ToString();
                GetStockProfileBrut(partNumProfile);
                //((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetListLotForJobNumFromProfile(resID, jobNum, opCode);
                switch (viewName)
                {
                    case "Production":
                        RefreshComboDataSource("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                        break;
                    case "RebutProduction":
                        RefreshComboDataSource("02ca5353-9363-45f3-8e52-4dc8173389d8", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                        string opCode = row["Character09"].ToString();
                        GetListScrapReasonForOperation(opCode);
                        RefreshComboDataSource("96a1d1bf-cbe1-458c-b3a0-0d087594741b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["RebutProductionRaisonRejet"], false);
                        break;
                    case "RebutProfile":
                        RefreshComboDataSource("5b199b25-1081-4b30-b906-088d269ebb7b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                        break;
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
            DataRow barlistRow;
            //DataTable lots;
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            string opCode;
            if (((BarlistManager)this.adapterManager.GetManager("BarlistManager")).GetBarlistLineByID(keyValue, refPoincon, out barlistRow, out opCode))
            {
                string jobNum = barlistRow["Key3"].ToString();
                int oprSeq = int.Parse(barlistRow["Key4"].ToString());
                string partNumProfile = "";
                decimal ratio = 1m;
                int mtlSeq = -1;
                GetJobOperStockProfileBrut(jobNum, 0, oprSeq, out partNumProfile, out mtlSeq);
                string noLot = "";
                string infoLot = "";
                string refComboLot = "";
                GetFirstStockProfileBrut(mtlSeq, out noLot, out infoLot, out refComboLot);
                this.codeChangeData = true;
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Number02"] = barlistRow["Number02"];
                newRow["Character05"] = noLot;
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                newRow["Character10"] = infoLot;
                newRow["ShortChar01"] = partNumProfile;
                newRow["ShortChar10"] = refComboLot;
                newRow["Number11"] = ratio;
                newRow["ShortChar04"] = mtlSeq;
                newRow["Number20"] = barlistRow["Number20"];

                //string resID = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                //((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetListLotForJobNumFromProfile(resID, jobNum, opCode);
                //RefreshComboDataSource("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LotProductionProfile"], true);
                RefreshComboDataSource("b1adb7a8-9d71-41a8-b07d-7e42f6f72c8d", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["ProfileBrut"], true);
                RefreshComboDataSource("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                this.codeChangeData = false;

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
                int oprSeq = int.Parse(barlistRow["Key4"].ToString());
                string partNumProfile = "";
                decimal ratio = 0m;
                int mtlSeq = -1;
                GetJobOperStockProfileBrut(jobNum, 0, oprSeq, out partNumProfile, out mtlSeq);
                string noLot = "";
                string infoLot = "";
                string refComboLot = "";
                GetFirstStockProfileBrut(mtlSeq, out noLot, out infoLot, out refComboLot);
                this.codeChangeData = true;
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Character05"] = noLot;
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                newRow["Character10"] = infoLot;
                newRow["ShortChar01"] = partNumProfile;
                newRow["ShortChar10"] = refComboLot;
                newRow["Number11"] = ratio;
                newRow["ShortChar04"] = mtlSeq;

                //string resID = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                //((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetListLotForJobNumFromProfile(resID, jobNum, opCode);
                //RefreshComboDataSource("02ca5353-9363-45f3-8e52-4dc8173389d8", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LotProductionProfile"], true);
                RefreshComboDataSource("824a0425-d5af-45f1-a01c-48622d816b1b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["ProfileBrut"], true);
                RefreshComboDataSource("02ca5353-9363-45f3-8e52-4dc8173389d8", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                GetListScrapReasonForOperation(opCode);
                RefreshComboDataSource("96a1d1bf-cbe1-458c-b3a0-0d087594741b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["RebutProductionRaisonRejet"], false);
                this.codeChangeData = false;

                return true;
            }
            return false;
        }
        public override string BeforeRebutProfileKeyChange(string proposedValue)
        {
            string refPoincon = string.Format("{0}-{1}",
                                    GetView("JobBatch").CurrentDataRow["Key1"].ToString(),
                                    GetView("JobBatch").CurrentDataRow["Key2"].ToString());
            return ((BarlistManager)this.adapterManager.GetManager("BarlistManager")).ValidateBarlistIDProposedValue(proposedValue, refPoincon);
        }
        public override bool AfterRebutProfileKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow)
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
                int oprSeq = int.Parse(barlistRow["Key4"].ToString());
                string noLot = "";
                string partNumProfile = "";
                decimal ratio = 0m;
                int mtlSeq = -1;
                GetJobOperStockProfileBrut(jobNum, 0, oprSeq, out partNumProfile, out mtlSeq);
                if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows.Count > 0)
                {
                    noLot = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows[0]["LotNum"].ToString();
                }
                /*if( ((GestionLotManager)this.adapterManager.GetManager("GestionLotManager")).GetLotForJobNumFromProfile(jobNum, opCode, out noLot, out partNumProfile, out ratio, out mtlSeq) )
				{
					
				}*/
                this.codeChangeData = true;
                newRow["Character01"] = barlistRow["Character01"];
                newRow["Character05"] = noLot;
                newRow["Character06"] = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                newRow["Character07"] = barlistRow["Key3"];
                newRow["Character08"] = barlistRow["Key4"];
                newRow["Character09"] = opCode;
                newRow["ShortChar01"] = partNumProfile;
                newRow["Number11"] = ratio;
                newRow["ShortChar04"] = mtlSeq;

                //string resID = GetView("JobBatch").CurrentDataRow["Character02"].ToString();
                //((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetListLotForJobNumFromProfile(resID, jobNum, opCode);
                //RefreshComboDataSource("5b199b25-1081-4b30-b906-088d269ebb7b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["LotProductionProfile"], true);
                RefreshComboDataSource("b82408a5-d449-4474-a2d2-6c2f996e9b58", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["ProfileBrut"], true);
                RefreshComboDataSource("5b199b25-1081-4b30-b906-088d269ebb7b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                this.codeChangeData = false;

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
                        OnBeforeTranQtyChange("Production", new string[] { "UseMtlLot" }, new string[] { "UseMtlLot" }, qteEntree, true);
                        /*decimal qteEntree = decimal.Parse(proposedValue);
                        if (qteEntree < 0m)
                        {
                            proposedValue = currentValue;
                            throw new Exception("La quantité produite ne peut pas être négative");
                        }
                        decimal oldQty = decimal.Parse(GetView("Production").CurrentDataRow["Number05"].ToString());
                        string jobNum = GetView("Production").CurrentDataRow["Character07"].ToString();
                        string company = GetView("Production").CurrentDataRow["Company"].ToString();
                        string profile = GetView("Production").CurrentDataRow["ShortChar01"].ToString();
                        if (string.IsNullOrEmpty(profile))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un profilé à consommer");
                        }
                        string lotNum = GetView("Production").CurrentDataRow["Character05"].ToString();
                        if (string.IsNullOrEmpty(lotNum))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un numéro de lot");
                        }
                        string mtlSeq = GetView("Production").CurrentDataRow["ShortChar04"].ToString();
                        decimal ratio = decimal.Parse(GetView("Production").CurrentDataRow["Number11"].ToString());
                        decimal tranQty = qteEntree - oldQty;
                        bool canUpdateBL = false;
                        canUpdateBL = ((BarlistManager)this.adapterManager.GetManager("BarlistManager")).UpdateBarlistLineQty(GetView("Production").CurrentDataRow["ChildKey3"].ToString(), tranQty);
                        if (!canUpdateBL)
                        {
                            proposedValue = currentValue;
                            return currentValue;
                        }
                        decimal qteProfileConsomme = ratio > 0m ? tranQty / ratio : tranQty;
                        bool flag = false;
                        if (qteProfileConsomme > 0m)
                        {
                            flag = ((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).IssueMaterial(jobNum, company, mtlSeq, qteProfileConsomme, lotNum, profile);
                        }
                        if (qteProfileConsomme < 0m)
                        {
                            qteProfileConsomme = qteProfileConsomme * -1m;
                            flag = ((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).ReturnMaterial(jobNum, company, mtlSeq, qteProfileConsomme, lotNum, profile);
                        }
                        if (!flag)
                        {
                            proposedValue = currentValue;
                            decimal revertTranQty = tranQty * -1m;
                            ((BarlistManager)this.adapterManager.GetManager("BarlistManager")).UpdateBarlistLineQty(GetView("Production").CurrentDataRow["ChildKey3"].ToString(), revertTranQty);
                        }*/
                        //((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).IssueMaterial(job, company, mtlSeq, tranQty, lotNum);
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
                case "Number05":
                    saveAfterChange = true;
                    refreshAfterChange = true;
                    break;

                case "ShortChar01":
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;
                        UltraGridRow selectedRow = GetComboSelectedRow("b1adb7a8-9d71-41a8-b07d-7e42f6f72c8d");
                        string profilePartNum = selectedRow.Cells["PartNum"].Value.ToString();
                        GetStockProfileBrut(profilePartNum);
                        this.codeChangeData = true;
                        string noLot = "";
                        string infoLot = "";
                        string refComboLot = "";
                        int mtlSeq = int.Parse(GetView("Production").CurrentDataRow["ShortChar04"].ToString());
                        GetFirstStockProfileBrut(mtlSeq, out noLot, out infoLot, out refComboLot);
                        GetView("Production").CurrentDataRow.BeginEdit();
                        GetView("Production").CurrentDataRow["Character05"] = noLot;
                        GetView("Production").CurrentDataRow["Character10"] = infoLot;
                        GetView("Production").CurrentDataRow["ShortChar10"] = refComboLot;
                        GetView("Production").CurrentDataRow.EndEdit();
                        this.codeChangeData = false;
                        RefreshComboDataSource("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                    }
                    break;

                case "Character05":
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;
                        
						UltraGridRow selectedRow = GetComboSelectedRow("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461");
						string partNum = selectedRow.Cells["PartNum"].Value.ToString();
						string lotNum = selectedRow.Cells["LotNum"].Value.ToString();
                        string estWIP = Convert.ToBoolean(selectedRow.Cells["EstWIP"].Value.ToString()) ? "1" : "0";
                        string idLigne = selectedRow.Cells["IDLigne"].Value.ToString();
                        string mtlSeq = GetView("Production").CurrentDataRow["ShortChar04"].ToString();
                        IssuePart ip = new IssuePart
                        {
                            PartNum = partNum,
                            LotNum = lotNum,
                            SetTrackLots = "1",
                            SetQtyPer = "1",
                            MtlSeq = mtlSeq,
                            SetEstWIP = estWIP,
                            IdLigneProdFrom = idLigne
                        };
                        GetView("Production").CurrentDataRow.BeginEdit();
                        GetView("Production").CurrentDataRow["Character10"] = ip.SerializeInfoLot();
						GetView("Production").CurrentDataRow.EndEdit();
                    }
                    break;

                case "ShortChar10":
                    if (!this.codeChangeData)
                    {
                        UltraGridRow selRow = GetComboSelectedRow("d1aaa9d7-1433-47a9-bddf-3dbd33cd2461");
                        GetView("Production").CurrentDataRow.BeginEdit();
                        GetView("Production").CurrentDataRow["Character05"] = selRow.Cells["LotNum"].Value.ToString();
                        GetView("Production").CurrentDataRow.EndEdit();
                    }
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
                        if (string.IsNullOrEmpty(raison))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner une raison de rejet.");
                        }
                        decimal qteEntree = GetPositiveProposedQty(proposedValue);
                        OnBeforeTranQtyChange("RebutProduction", new string[] { "UseMtlLot" }, new string[] { "UseMtlLot" }, qteEntree, true);
                        /*decimal qteEntree = decimal.Parse(proposedValue);
                        if (qteEntree < 0m)
                        {
                            proposedValue = currentValue;
                            throw new Exception("La quantité rejettée ne peut pas être négative");
                        }
                        string profile = GetView("RebutProduction").CurrentDataRow["ShortChar01"].ToString();
                        if (string.IsNullOrEmpty(profile))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un profilé à consommer");
                        }
                        string lotNum = GetView("RebutProduction").CurrentDataRow["Character05"].ToString();
                        if (string.IsNullOrEmpty(lotNum))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un numéro de lot");
                        }
                        decimal oldQty = decimal.Parse(GetView("RebutProduction").CurrentDataRow["Number05"].ToString());
                        string jobNum = GetView("RebutProduction").CurrentDataRow["Character07"].ToString();
                        string company = GetView("RebutProduction").CurrentDataRow["Company"].ToString();
                        string mtlSeq = GetView("RebutProduction").CurrentDataRow["ShortChar04"].ToString();
                        decimal tranQty = qteEntree - oldQty;
                        decimal ratio = decimal.Parse(GetView("RebutProduction").CurrentDataRow["Number11"].ToString());
                        decimal qteProfileConsomme = ratio > 0m ? tranQty / ratio : tranQty;
                        bool flag = false;
                        if (qteProfileConsomme > 0m)
                        {
                            flag = ((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).IssueMaterial(jobNum, company, mtlSeq, qteProfileConsomme, lotNum, profile);
                        }
                        if (qteProfileConsomme < 0m)
                        {
                            qteProfileConsomme = qteProfileConsomme * -1m;
                            flag = ((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).ReturnMaterial(jobNum, company, mtlSeq, qteProfileConsomme, lotNum, profile);
                        }
                        if (!flag)
                        {
                            proposedValue = currentValue;
                        }*/
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
                    saveAfterChange = true;
                    break;

                case "ShortChar01":
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;
                        UltraGridRow selectedRow = GetComboSelectedRow("824a0425-d5af-45f1-a01c-48622d816b1b");
                        string profilePartNum = selectedRow.Cells["PartNum"].Value.ToString();
                        GetStockProfileBrut(profilePartNum);
                        this.codeChangeData = true;
                        string noLot = "";
                        string infoLot = "";
                        string refComboLot = "";
                        int mtlSeq = int.Parse(GetView("Production").CurrentDataRow["ShortChar04"].ToString());
                        GetFirstStockProfileBrut(mtlSeq, out noLot, out infoLot, out refComboLot);
                        GetView("RebutProduction").CurrentDataRow.BeginEdit();
                        GetView("RebutProduction").CurrentDataRow["Character05"] = noLot;
                        GetView("RebutProduction").CurrentDataRow["Character10"] = infoLot;
                        GetView("RebutProduction").CurrentDataRow["ShortChar10"] = refComboLot;
                        GetView("RebutProduction").CurrentDataRow.EndEdit();
                        this.codeChangeData = false;
                        RefreshComboDataSource("02ca5353-9363-45f3-8e52-4dc8173389d8", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                    }
                    break;

                case "Character05":
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;

                        UltraGridRow selectedRow = GetComboSelectedRow("02ca5353-9363-45f3-8e52-4dc8173389d8");
                        string partNum = selectedRow.Cells["PartNum"].Value.ToString();
                        string lotNum = selectedRow.Cells["LotNum"].Value.ToString();
                        string estWIP = Convert.ToBoolean(selectedRow.Cells["EstWIP"].Value.ToString()) ? "1" : "0";
                        string idLigne = selectedRow.Cells["IDLigne"].Value.ToString();
                        string mtlSeq = GetView("RebutProduction").CurrentDataRow["ShortChar04"].ToString();
                        IssuePart ip = new IssuePart
                        {
                            PartNum = partNum,
                            LotNum = lotNum,
                            SetTrackLots = "1",
                            SetQtyPer = "1",
                            MtlSeq = mtlSeq,
                            SetEstWIP = estWIP,
                            IdLigneProdFrom = idLigne
                        };
                        GetView("RebutProduction").CurrentDataRow.BeginEdit();
                        GetView("RebutProduction").CurrentDataRow["Character10"] = ip.SerializeInfoLot();
                        GetView("RebutProduction").CurrentDataRow.EndEdit();
                    }
                    break;

                case "ShortChar10":
                    if (!this.codeChangeData)
                    {
                        UltraGridRow selRow = GetComboSelectedRow("02ca5353-9363-45f3-8e52-4dc8173389d8");
                        GetView("RebutProduction").CurrentDataRow.BeginEdit();
                        GetView("RebutProduction").CurrentDataRow["Character05"] = selRow.Cells["LotNum"].Value.ToString();
                        GetView("RebutProduction").CurrentDataRow.EndEdit();
                    }
                    break;
            }
            return true;
        }
        public override string BeforeRebutProfileFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans)
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
                            throw new Exception("La quantité jetée ne peut pas être négative");
                        }
                        string profile = GetView("RebutProfile").CurrentDataRow["ShortChar01"].ToString();
                        if (string.IsNullOrEmpty(profile))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un profilé à consommer");
                        }
                        string lotNum = GetView("RebutProfile").CurrentDataRow["Character05"].ToString();
                        if (string.IsNullOrEmpty(lotNum))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un numéro de lot");
                        }
                        decimal oldQty = decimal.Parse(GetView("RebutProfile").CurrentDataRow["Number05"].ToString());
                        decimal tranQty = (qteEntree - oldQty) * -1m;
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
                        bool flag = ((InvQtyManager)this.adapterManager.GetManager("InvQtyManager")).AdjQty(profile, lotNum, whse, bin, ium, dateJobBatch, tranQty, out partTran, "PULT");
                        if (!flag)
                        {
                            proposedValue = currentValue;
                            throw new Exception("Une erreur est survenue.");
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
        public override bool AfterRebutProfileFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
        {
            saveAfterChange = false;
            switch (fieldName)
            {
                case "Number05":
                    saveAfterChange = true;
                    break;

                case "ShortChar01":
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;
                        UltraGridRow selectedRow = GetComboSelectedRow("b82408a5-d449-4474-a2d2-6c2f996e9b58");
                        string profilePartNum = selectedRow.Cells["PartNum"].Value.ToString();
                        GetStockProfileBrut(profilePartNum);
                        this.codeChangeData = true;
                        string noLot = "";
                        if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows.Count > 0)
                        {
                            noLot = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows[0]["LotNum"].ToString();
                        }
                        GetView("RebutProfile").CurrentDataRow.BeginEdit();
                        GetView("RebutProfile").CurrentDataRow["Character05"] = noLot;
                        GetView("RebutProfile").CurrentDataRow.EndEdit();
                        this.codeChangeData = false;
                        RefreshComboDataSource("5b199b25-1081-4b30-b906-088d269ebb7b", ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"], true);
                    }
                    break;

                case "Character05":
                    if (!this.codeChangeData)
                    {
                        saveAfterChange = true;
                        /*
						UltraGridRow selectedRow = GetComboSelectedRow("5b199b25-1081-4b30-b906-088d269ebb7b");
						string IDLot = selectedRow.Cells["IDLot"].Value.ToString();
						string profile = selectedRow.Cells["Profilé consommé"].Value.ToString();
						GetView("RebutProfile").CurrentDataRow.BeginEdit();
						GetView("RebutProfile").CurrentDataRow["ShortChar01"] = profile;
						GetView("RebutProfile").CurrentDataRow["ShortChar02"] = IDLot;
						GetView("RebutProfile").CurrentDataRow.EndEdit();
						*/
                    }
                    break;
            }
            return true;
        }
        public override string BeforeDecoupeBrutFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans)
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
                            throw new Exception("La quantité récupérée ne peut pas être négative");
                        }
                        string profile = GetView("DecoupeBrut").CurrentDataRow["ShortChar01"].ToString();
                        if (string.IsNullOrEmpty(profile))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez sélectionner un profilé récupéré");
                        }
                        string lotNum = GetView("DecoupeBrut").CurrentDataRow["Character05"].ToString();
                        if (string.IsNullOrEmpty(lotNum))
                        {
                            proposedValue = currentValue;
                            throw new Exception("Vous devez entrer un numéro de lot");
                        }
                        decimal oldQty = decimal.Parse(GetView("DecoupeBrut").CurrentDataRow["Number05"].ToString());
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
                        bool flag = ((InvQtyManager)this.adapterManager.GetManager("InvQtyManager")).AdjQty(profile, lotNum, whse, bin, ium, dateJobBatch, tranQty, out partTran, "DECOUP");
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

                }
                return proposedValue;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return currentValue;
            }
        }
        public override bool AfterDecoupeBrutFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
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
                RuleAction.DisableRow(oTrans, "Production", new string[]{
                    "ChildKey3",
                    "Number05",
                    "ShortChar01",
                    "Character05",
                    "ShortChar10"
                })
            });
            edv.AddRowRule(disableProduction);
            RowRule disableProduction2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.ChildKey3", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction2);
            RowRule disableProduction3 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "Production.ShortChar01", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "Production.Character05", SettingStyle.ReadOnly)
            });
            edv.AddRowRule(disableProduction3);
            RowRule disableRebutProduction = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "RebutProduction", new string[]{
                    "ChildKey3",
                    "ShortChar03",
                    "Number05",
                    "ShortChar01",
                    "Character05",
                    "ShortChar10"
                })
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction);
            RowRule disableRebutProduction2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ChildKey3", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction2);
            RowRule disableRebutProduction3 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar01", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProduction.ShortChar03", SettingStyle.ReadOnly)
            });
            GetView("RebutProduction").AddRowRule(disableRebutProduction3);

            RowRule disableRebutProfile = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "RebutProfile", new string[5]{
                    "ChildKey3",
                    "ShortChar03",
                    "Number05",
                    "ShortChar01",
                    "Character05"
                })
            });
            GetView("RebutProfile").AddRowRule(disableRebutProfile);
            RowRule disableRebutProfile2 = new RowRule("RowMod", RuleCondition.NotEqual, "A", new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProfile.ChildKey3", SettingStyle.ReadOnly)
            });
            GetView("RebutProfile").AddRowRule(disableRebutProfile2);
            RowRule disableRebutProfile3 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "RebutProfile.ShortChar01", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProfile.Character05", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "RebutProfile.ShortChar03", SettingStyle.ReadOnly)
            });
            GetView("RebutProfile").AddRowRule(disableRebutProfile3);

            RowRule disableDecoupeBrut = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, new RuleAction[]{
                RuleAction.DisableRow(oTrans, "DecoupeBrut", new string[3]{
                    "Number05",
                    "ShortChar01",
                    "Character05"
                })
            });
            GetView("DecoupeBrut").AddRowRule(disableDecoupeBrut);
            RowRule disableDecoupeBrut2 = new RowRule("Number05", RuleCondition.NotEqual, 0m, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "DecoupeBrut.ShortChar01", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "DecoupeBrut.Character05", SettingStyle.ReadOnly)
            });
            GetView("DecoupeBrut").AddRowRule(disableDecoupeBrut2);
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
