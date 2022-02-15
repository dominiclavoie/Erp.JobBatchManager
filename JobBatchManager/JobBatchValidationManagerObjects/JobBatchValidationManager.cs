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

namespace JobBatchManagerObjects.JobBatchValidationManagerObjects
{
    public abstract class JobBatchValidationManager
    {
        protected string jobBatchJobNum;
        protected AdapterManager adapterManager;
        protected List<DockablePaneBase> panels;
        protected Dictionary<string, EpiDataView> views;
        protected CustomScriptManager csm;
        public JobBatchValidationManager(CustomScriptManager _csm)
        {
            this.views = new Dictionary<string, EpiDataView>();
            this.panels = new List<DockablePaneBase>();
            csm = _csm;
            SetPanels();
        }
        public void SetAdapterManager(AdapterManager _adapterManager)
        {
            adapterManager = _adapterManager;
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
        public void MainSetupRowRule(EpiTransaction oTrans, ref EpiDataView edv)
        {
            RowRule disableButtonSubmit = new RowRule("ButtonSubmit", RuleCondition.Equals, false, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonSubmit", SettingStyle.ReadOnly)
            });
            this.GetView("FormView").AddRowRule(disableButtonSubmit);
            RowRule disableButtonRecall = new RowRule("ButtonRecall", RuleCondition.Equals, false, new RuleAction[]{
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonRecall", SettingStyle.ReadOnly),
                RuleAction.AddControlSettings(oTrans, "FormView.ButtonRecall", SettingStyle.Invisible)
            });
            this.GetView("FormView").AddRowRule(disableButtonRecall);
            this.SetupRowRule(oTrans, ref edv);
        }
        protected virtual void SetupRowRule(EpiTransaction oTrans, ref EpiDataView edv) { }
        protected virtual void SetPanels() { }
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
        public void CalculateLabor(DataSet dsJobBatch, EpiTransaction oTrans)
        {
            ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).CalculateLabor(dsJobBatch);
            AfterCalculateLabor(oTrans);
        }
        protected virtual void AfterCalculateLabor(EpiTransaction oTrans) { }
        public virtual void CalculerRepartition(Validator validator) { }
        public virtual void SoumettreRepartition(Validator validator) { }
        public virtual void Clear() { }
        public virtual Dictionary<string, string> GetPoinconLayout() { return null; }
        public virtual Dictionary<string, string> GetRepartitionLayout() { return null; }
        public virtual Dictionary<string, string> GetRebutProductionLayout() { return null; }
    }
}
