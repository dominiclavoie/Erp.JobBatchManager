using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Infragistics.Win.UltraWinDock;
using Infragistics.Win.UltraWinGrid;
using JobBatchManagerObjects.AdapterManagerObjects;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchProductionManagerObjects
{
    public abstract class JobBatchProductionManager
    {
        protected string jobBatchJobNum;
        protected AdapterManager adapterManager;
        protected List<DockablePaneBase> panels;
        protected Dictionary<string, EpiDataView> views;
        protected CustomScriptManager csm;
        protected bool codeChangeData;
        public JobBatchProductionManager(CustomScriptManager _csm)
        {
            this.views = new Dictionary<string, EpiDataView>();
            this.panels = new List<DockablePaneBase>();
            csm = _csm;
            codeChangeData = false;
            SetPanels();
        }
        private void GetJobFictive()
        {
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetBTJobFictive();
        }
        protected string GetJobFictiveForType(string type)
        {
            if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["BTJobFictive"].Rows.Count == 0)
            {
                GetJobFictive();
            }
            string jobFictive = string.Empty;
            foreach (DataRow row in ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["BTJobFictive"].Rows)
            {
                if (type.Equals(row["Type"].ToString()))
                {
                    jobFictive = row["No BT"].ToString();
                    break;
                }
            }
            return jobFictive;
        }
        protected virtual void SetJobBatchJobNum() { }
        public string JobBatchJobNum
        {
            get
            {
                if (string.IsNullOrEmpty(jobBatchJobNum))
                {
                    SetJobBatchJobNum();
                }
                return jobBatchJobNum;
            }
        }
        public virtual string EmployeeListFilter { get { return ""; } }
        public void SetAdapterManager(AdapterManager _adapterManager)
        {
            adapterManager = _adapterManager;
        }
        protected virtual string[] GetFormViewInvisibleFields()
        {
            return new string[] { };
        }
        public string[] GetRequiredFields() { return GetFormViewRequiredFields(); }
        protected virtual string[] GetFormViewRequiredFields()
        {
            return new string[] { "DateJobBatch", "Quart", "Equipement" };
        }
        public void MainSetupRowRule(EpiTransaction oTrans, ref EpiDataView edv)
        {
            string[] invisibleFields = this.GetFormViewInvisibleFields();
            if (invisibleFields.Any())
            {
                List<RuleAction> raInvisibleFields = new List<RuleAction>();
                foreach (string invisibleField in invisibleFields)
                {
                    string viewName = string.Format("FormView.{0}", invisibleField);
                    raInvisibleFields.Add(RuleAction.AddControlSettings(oTrans, viewName, SettingStyle.Invisible));
                }
                RowRule rrInvisibleFields = new RowRule(null, delegate (RowRuleDelegateArgs args) { return true; }, null, raInvisibleFields.ToArray());
                this.GetView("FormView").AddRowRule(rrInvisibleFields);
            }
            RowRule disableButtons = new RowRule("ButtonsEnabled", RuleCondition.Equals, false, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonsEnabled", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonsEnabled2", SettingStyle.ReadOnly)
            });
            this.GetView("FormView").AddRowRule(disableButtons);
            RowRule disableClosed = new RowRule("UD105.CheckBox01", RuleCondition.Equals, true, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonsEnabled", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonsEnabled2", SettingStyle.ReadOnly)
            });
            this.GetView("FormView").AddRowRule(disableClosed);
            this.SetupRowRule(oTrans, ref edv);
            string[] disableClosedViews = new string[] { "Production", "RebutProduction", "RebutProfile", "Operateur" };
            foreach (string disableClosedView in disableClosedViews)
            {
                RowRule rrDisableClosed = new RowRule("UD105.CheckBox01", RuleCondition.Equals, true, new RuleAction[]{
                    RuleAction.DisableRow(oTrans, disableClosedView)
                });
                this.GetView(disableClosedView).AddRowRule(rrDisableClosed);
            }
        }
        public virtual void ClockIn(int hedSeq, int oprSeq, string resID, string empID, EpiTransaction oTrans) { }
        public virtual void OnGetRow(string viewName, DataRowView row, EpiTransaction oTrans, bool forceRefresh) { }
        public virtual void Clear() { }
        protected virtual void SetupRowRule(EpiTransaction oTrans, ref EpiDataView edv) { }
        public void SetCodeChangeData(bool value)
        {
            this.codeChangeData = value;
        }
        protected virtual void SetPanels() { }
        public virtual Dictionary<string, string> GetProductionLayout()
        {
            return new Dictionary<string, string>() { };
        }
        public virtual Dictionary<string, string> GetRebutProductionLayout()
        {
            return new Dictionary<string, string>() { };
        }
        public virtual Dictionary<string, string> GetRebutProfileLayout()
        {
            return new Dictionary<string, string>() { };
        }
        public virtual Dictionary<string, string> GetDecoupeBrutLayout()
        {
            return new Dictionary<string, string>() { };
        }
        public virtual Dictionary<string, string> GetPultrusionLayout()
        {
            return new Dictionary<string, string>() { };
        }
        public virtual string BeforeProductionKeyChange(string proposedValue) { return ""; }
        public virtual bool AfterProductionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow) { return false; }
        public virtual string BeforeRebutProductionKeyChange(string proposedValue, EpiTransaction oTrans) { return ""; }
        public virtual bool AfterRebutProductionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow) { return false; }
        public virtual string BeforeRebutProfileKeyChange(string proposedValue) { return ""; }
        public virtual bool AfterRebutProfileKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow) { return false; }
        public virtual string BeforeDecoupeBrutKeyChange(string proposedValue) { return ""; }
        public virtual bool AfterDecoupeBrutKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow) { return false; }
        public virtual string BeforePultrusionKeyChange(string proposedValue) { return ""; }
        public virtual bool AfterPultrusionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow) { return false; }
        public virtual string BeforeProductionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans) { return ""; }
        public virtual bool AfterProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange, out bool refreshAfterChange)
        {
            saveAfterChange = false;
            refreshAfterChange = false;
            return false;
        }
        public virtual string BeforeRebutProductionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans) { return ""; }
        public virtual bool AfterRebutProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
        {
            saveAfterChange = false;
            return false;
        }
        public virtual string BeforeRebutProfileFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans) { return ""; }
        public virtual bool AfterRebutProfileFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
        {
            saveAfterChange = false;
            return false;
        }
        public virtual string BeforeDecoupeBrutFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans) { return ""; }
        public virtual bool AfterDecoupeBrutFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
        {
            saveAfterChange = false;
            return false;
        }
        public virtual string BeforePultrusionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans) { return ""; }
        public virtual bool AfterPultrusionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange)
        {
            saveAfterChange = false;
            return false;
        }
        protected virtual void OnBeforeTranQtyChange(string viewName, string[] requireLotNum, string[] requireIssueParts, decimal qteEntree, bool updateRemainingQty, bool isTranQty = false, bool isScrap = false)
        {
            string idLigne = GetView(viewName).CurrentDataRow["ChildKey3"].ToString();
            string opCode = GetView(viewName).CurrentDataRow["Character09"].ToString();
            string[] opInteractions = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).GetInteractionsForOpCode(opCode);
            decimal oldQty = decimal.Parse(GetView(viewName).CurrentDataRow["Number05"].ToString());
            string jobNum = GetView(viewName).CurrentDataRow["Character07"].ToString();
            string company = GetView(viewName).CurrentDataRow["Company"].ToString();
            decimal tranQty = qteEntree - oldQty;
            if (isTranQty)
            {
                tranQty = qteEntree;
            }
            if (opInteractions.Any(x => requireLotNum.Contains(x)))
            {
                string lotNum = GetView(viewName).CurrentDataRow["Character05"].ToString();
                if (string.IsNullOrEmpty(lotNum))
                {
                    throw new Exception("Vous devez entrer un numéro de lot");
                }
            }
            if (opInteractions.Any(x => requireIssueParts.Contains(x)))
            {
                if (updateRemainingQty)
                {
                    UpdateRemainingQty(GetView(viewName).CurrentDataRow["ChildKey3"].ToString(), tranQty);
                }
                if (!IssueParts(company, jobNum, GetView(viewName).CurrentDataRow["Character10"].ToString(), idLigne, tranQty, isScrap))
                {
                    decimal revertTranQty = tranQty * -1m;
                    if (updateRemainingQty)
                    {
                        UpdateRemainingQty(GetView(viewName).CurrentDataRow["ChildKey3"].ToString(), revertTranQty);
                    }
                    throw new Exception("Une erreur est survenue en consommant les matières.");
                }
            }
            else
            {
                if (updateRemainingQty)
                {
                    UpdateRemainingQty(GetView(viewName).CurrentDataRow["ChildKey3"].ToString(), tranQty);
                }
            }
        }
        protected virtual void UpdateRemainingQty(string idLigne, decimal tranQty)
        {
            if (!((BarlistManager)this.adapterManager.GetManager("BarlistManager")).UpdateBarlistLineQty(idLigne, tranQty))
            {
                throw new Exception("La quantité restante n'a pas être mise à jour. Veuillez contacter l'administrateur.");
            }
        }
        protected virtual bool TryIssueParts(string company, string jobNum, string infoLot, string idLigne, decimal tranQty, bool isScrap, out int issuedPartIndex, int endI = -1)
        {
            bool flag = false;
            issuedPartIndex = -1;
            List<IssuePart> issueParts = isScrap ? GetIssuePartsWithScrap(infoLot) : GetIssueParts(infoLot);
            int endIndex = endI == -1 ? issueParts.Count : endI;
            for (int i = 0; i < endIndex; i++)
            {
                IssuePart issuePart = issueParts[i];
                if (tranQty > 0m)
                {
                    decimal qte = isScrap ? issuePart.ScrapQty : (tranQty * issuePart.QtyPer);
                    if (issuePart.EstWIP)
                    {
                        flag = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).Reserver(issuePart.IdLigneProdFrom, idLigne, jobNum, issuePart.MtlSeq, issuePart.PartNum, issuePart.LotNum, qte);
                    }
                    else
                    {
                        flag = ((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).IssueMaterial(jobNum, company, issuePart.MtlSeq, qte, issuePart.LotNum, issuePart.PartNum);
                    }
                }
                if (tranQty < 0m)
                {
                    decimal qte = isScrap ? issuePart.ScrapQty : (tranQty * issuePart.QtyPer * -1m);
                    if (issuePart.EstWIP)
                    {
                        flag = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).Annuler(issuePart.IdLigneProdFrom, idLigne, issuePart.MtlSeq, issuePart.PartNum, issuePart.LotNum, qte);
                    }
                    else
                    {
                        flag = ((IssueMaterialManager)this.adapterManager.GetManager("IssueMaterialManager")).ReturnMaterial(jobNum, company, issuePart.MtlSeq, qte, issuePart.LotNum, issuePart.PartNum);

                    }
                }
                if (!flag)
                {
                    issuedPartIndex = i - 1;
                    break;
                }
            }
            return flag;
        }
        protected virtual bool IssueParts(string company, string jobNum, string infoLot, string idLigne, decimal tranQty, bool isScrap)
        {
            int issuedPartIndex;
            if (!TryIssueParts(company, jobNum, infoLot, idLigne, tranQty, isScrap, out issuedPartIndex))
            {
                if (issuedPartIndex > -1)
                {
                    decimal revertTranQty = tranQty * -1m;
                    issuedPartIndex += 1;
                    int i;
                    TryIssueParts(company, jobNum, infoLot, idLigne, tranQty, isScrap, out i, issuedPartIndex);
                }
                return false;
            }
            return true;
        }
        protected List<IssuePart> GetIssueParts(string infoLot)
        {
            return (from il in infoLot.Split('|')
                    let pt = il.Split('~')
                    where pt.Length == 5 || pt.Length == 7
                    select new IssuePart
                    {
                        PartNum = pt[0],
                        LotNum = pt[1],
                        SetTrackLots = pt[2],
                        SetQtyPer = pt[3],
                        MtlSeq = pt[4],
                        SetEstWIP = pt.Length == 5 ? "0" : pt[5],
                        IdLigneProdFrom = pt.Length == 5 ? "" : pt[6]
                    }).ToList();
        }
        protected List<IssuePart> GetIssuePartsWithScrap(string infoLot)
        {
            return (from il in infoLot.Split('|')
                    let pt = il.Split('~')
                    where pt.Length == 6 || pt.Length == 8
                    select new IssuePart
                    {
                        PartNum = pt[0],
                        LotNum = pt[1],
                        SetTrackLots = pt[2],
                        SetQtyPer = pt[3],
                        MtlSeq = pt[4],
                        SetScrapQty = pt[5],
                        SetEstWIP = pt.Length == 6 ? "0" : pt[6],
                        IdLigneProdFrom = pt.Length == 6 ? "" : pt[7]
                    }).ToList();
        }
        protected decimal GetPositiveProposedQty(string proposedValue)
        {
            decimal qteEntree;
            if (!decimal.TryParse(proposedValue, out qteEntree))
            {
                throw new Exception("La valeur entrée doit être un nombre.");
            }
            if (qteEntree < 0m)
            {
                throw new Exception("La quantité entrée ne peut pas être négative");
            }
            return qteEntree;
        }
        protected void LaunchForm(EpiTransaction oTrans, string menuID, object valueIn)
        {
            try
            {
                LaunchFormOptions lfo = new LaunchFormOptions();
                lfo.SuppressFormSearch = true;
                lfo.IsModal = true;
                lfo.CallBackMethod = LaunchFormCallBackHandler;
                lfo.ValueIn = valueIn;
                ProcessCaller.LaunchForm(oTrans, menuID, lfo);
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
        protected void LaunchFormCallBackHandler(object sender, object CallBackArgs)
        {
            if (CallBackArgs == null) return;
            try
            {
                DataRow response = (DataRow)CallBackArgs;
                FormCallBack(response);
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
        protected DataRow FormParameters(Dictionary<string, string> parameters)
        {
            DataTable dt = new DataTable();
            foreach (string col in parameters.Keys)
            {
                dt.Columns.Add(new DataColumn(col, typeof(string)));
            }
            DataRow newRow = dt.NewRow();
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                newRow[entry.Key] = entry.Value;
            }
            return newRow;
        }
        protected virtual void FormCallBack(DataRow response) { }
        protected void AddPanel(string epiGuid, string panelKey)
        {
            EpiDockManagerPanel mp = (EpiDockManagerPanel)csm.GetNativeControlReference(epiGuid);
            DockablePaneBase panel = mp.baseDockManager.PaneFromKey(panelKey);
            if (panel == null)
            {
                throw new Exception(string.Format("L'onglet {0} est manquant.", panelKey));
            }
            this.panels.Add(panel);
        }
        public void ShowPanels()
        {
            foreach (DockablePaneBase panel in this.panels)
            {
                if (panel.Closed)
                {
                    panel.Show();
                }
            }
        }
        public void AddViewReference(string key, ref EpiDataView edv)
        {
            this.views.Add(key, edv);
        }
        protected EpiDataView GetView(string key)
        {
            if (!this.views.ContainsKey(key))
            {
                throw new Exception(string.Format("La référence à {0} est manquante.", key));
            }
            return this.views[key];
        }
        public void RefreshComboDataSource(string epiGuid, DataTable dt, bool displayColumnHeaders = false)
        {
            EpiUltraCombo cmb = (EpiUltraCombo)csm.GetNativeControlReference(epiGuid);
            cmb.DataSource = dt;
            cmb.DisplayLayout.Bands[0].ColHeadersVisible = displayColumnHeaders;
            cmb.ForceRefreshList();
        }
        public UltraGridRow GetComboSelectedRow(string epiGuid)
        {
            EpiUltraCombo cmb = (EpiUltraCombo)csm.GetNativeControlReference(epiGuid);
            return cmb.SelectedRow;
        }

        public bool GetFirstStockProfileBrut(int mtlSeq, out string noLot, out string infoLot, out string refComboLot)
        {
            noLot = string.Empty;
            infoLot = string.Empty;
            refComboLot = string.Empty;
            if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows.Count > 0)
            {
                DataRow dr = ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).DsMiscAction.Tables["StockProfileBrut"].Rows[0];
                IssuePart ip = new IssuePart
                {
                    PartNum = dr["PartNum"].ToString(),
                    LotNum = dr["LotNum"].ToString(),
                    SetTrackLots = "1",
                    SetQtyPer = "1",
                    MtlSeq = mtlSeq.ToString(),
                    SetEstWIP = Convert.ToBoolean(dr["EstWIP"].ToString()) ? "1" : "0",
                    IdLigneProdFrom = dr["IDLigne"].ToString()
                };
                noLot = ip.LotNum;
                infoLot = ip.SerializeInfoLot();
                refComboLot = dr["InfoLot"].ToString();
                return true;
            }
            return false;
        }
    }
}
