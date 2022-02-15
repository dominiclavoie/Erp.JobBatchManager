using System;
using System.Collections.Generic;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Customization;
using Infragistics.Win.UltraWinDock;
using JobBatchManagerObjects.JobBatchProductionManagerObjects;
using JobBatchManagerObjects.AdapterManagerObjects;
using AdapterManagerObjects;

namespace JobBatchManagerObjects
{
    public class JobBatchManager
    {
        private JobBatchType jobBatchType;
        private IJobBatchProductionManager jobBatchProductionManager;
        private EpiTransaction oTrans;
        private List<DockablePaneBase> panels;
        public JobBatchManager(JobBatchType _jobBatchType, EpiTransaction _oTrans, CustomScriptManager csm)
        {
            jobBatchType = _jobBatchType;
            oTrans = _oTrans;
            this.panels = new List<DockablePaneBase>();
            SetAllPanels(csm);
            SetupJobBatch(csm);
        }
        public string JobBatchJobNum { get { return jobBatchProductionManager.JobBatchJobNum; } }
        public string JobBatchTitle { get { return jobBatchProductionManager.JobBatchTitle; } }
        public string EmployeeListFilter { get { return jobBatchProductionManager.EmployeeListFilter; } }
        public bool BatchTypeIs(JobBatchType _jobBatchType)
        {
            return jobBatchType == _jobBatchType;
        }
        private void SetupJobBatch(CustomScriptManager csm)
        {
            switch (jobBatchType)
            {
                case JobBatchType.VRODDroite:
                    //jobBatchProductionManager = new JobBatchProductionVRODDroite(csm);
                    break;
                case JobBatchType.ProfileBrut:
                    //jobBatchProductionManager = new JobBatchProductionProfileBrut(csm);
                    break;
                case JobBatchType.Usinage:
                    jobBatchProductionManager = new JobBatchProductionUsinage(csm);
                    break;
                case JobBatchType.Assemblage:
                    jobBatchProductionManager = new JobBatchProductionAssemblage(csm);
                    break;
                case JobBatchType.Melange:
                    //jobBatchProductionManager = new JobBatchProductionMelange(csm);
                    break;
                case JobBatchType.Pultrusion:
                    jobBatchProductionManager = new JobBatchProductionPultrusion(csm);
                    break;
            }
        }
        public DataRow GetOperateurInfos()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("JobBatchJobNum", typeof(string)),
                new DataColumn("EmployeeListFilter", typeof(string))
            });
            dt.Rows.Add(JobBatchJobNum, EmployeeListFilter);
            return dt.Rows[0];
        }
        private void SetAllPanels(CustomScriptManager csm)
        {
            try
            {
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "Production");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "EnregistrementMelange");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "RecuperationMelange");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "RebutMelange");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "RebutProduction");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "ConsommationProfile");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "DecoupeBrut");
                AddPanel(csm, "65ad0f2d-e5e4-4f95-b7cf-52342f6058f6", "Pultrusion");
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
        private void AddPanel(CustomScriptManager csm, string epiGuid, string panelKey)
        {
            EpiDockManagerPanel mp = (EpiDockManagerPanel)csm.GetNativeControlReference(epiGuid);
            DockablePaneBase panel = mp.baseDockManager.PaneFromKey(panelKey);
            if (panel == null)
            {
                throw new Exception(string.Format("L'onglet {0} est manquant.", panelKey));
            }
            this.panels.Add(panel);
        }
        private void ClosePanels()
        {
            foreach (DockablePaneBase panel in this.panels)
            {
                if (!panel.Closed)
                {
                    panel.Close();
                }
            }
        }
        public void SetAdapterManager(AdapterManager _adapterManager)
        {
            _adapterManager.AddAdapterRange(new Dictionary<string, IAdapterBase>()
            {
                { "TimeExpenseManager", new TimeExpenseManager(oTrans, _adapterManager) },
                { "InvQtyManager", new InvQtyManager(oTrans, _adapterManager) },
                { "CostAdjustmentManager", new CostAdjustmentManager(oTrans, _adapterManager) },
                { "IssueMaterialManager", new IssueMaterialManager(oTrans, _adapterManager) },
                { "WarehouseBinManager", new WarehouseBinManager(oTrans, _adapterManager) },
                { "PartPlantWarehouseManager", new PartPlantWarehouseManager(oTrans, _adapterManager) },
                { "PartManager", new PartManager(oTrans, _adapterManager) },
                { "JobEntryManager", new JobEntryManager(oTrans, _adapterManager) },
                { "GestionLotManager", new GestionLotManager(oTrans, _adapterManager) },
                { "BarlistManager", new BarlistManager(oTrans, _adapterManager) },
                { "OperationInteractionsManager", new OperationInteractionsManager(oTrans, _adapterManager) },
                { "OperationManager", new OperationManager(oTrans, _adapterManager) },
                { "CRTIMiscActionManager", new CRTIMiscActionManager(oTrans, _adapterManager) },
            });
            _adapterManager.Connect();
            jobBatchProductionManager.SetAdapterManager(_adapterManager);
        }
        public void SetCodeChangeData(bool value)
        {
            jobBatchProductionManager.SetCodeChangeData(value);
        }
        public Dictionary<string, string> GetProductionLayout()
        {
            return jobBatchProductionManager.GetProductionLayout();
        }
        public Dictionary<string, string> GetRebutProductionLayout()
        {
            return jobBatchProductionManager.GetRebutProductionLayout();
        }
        public Dictionary<string, string> GetRebutProfileLayout()
        {
            return jobBatchProductionManager.GetRebutProfileLayout();
        }
        public Dictionary<string, string> GetDecoupeBrutLayout()
        {
            return jobBatchProductionManager.GetDecoupeBrutLayout();
        }
        public Dictionary<string, string> GetPultrusionLayout()
        {
            return jobBatchProductionManager.GetPultrusionLayout();
        }
        public string[] GetRequiredFields()
        {
            return jobBatchProductionManager.GetRequiredFields();
        }
        public void ClockIn(int hedSeq, int oprSeq, string resID, string empID)
        {
            jobBatchProductionManager.ClockIn(hedSeq, oprSeq, resID, empID, oTrans);
        }
        public void OnGetRow(string viewName, DataRowView row, bool forceRefresh = false)
        {
            jobBatchProductionManager.OnGetRow(viewName, row, oTrans, forceRefresh);
        }
        public void AddViewReference(string key, ref EpiDataView edv)
        {
            jobBatchProductionManager.AddViewReference(key, ref edv);
        }
        public string BeforeProductionKeyChange(string proposedValue)
        {
            return jobBatchProductionManager.BeforeProductionKeyChange(proposedValue);
        }
        public bool AfterProductionKeyChange(string keyValue, ref DataRow newRow)
        {
            return jobBatchProductionManager.AfterProductionKeyChange(keyValue, oTrans, ref newRow);
        }
        public string BeforeRebutProductionKeyChange(string proposedValue)
        {
            return jobBatchProductionManager.BeforeRebutProductionKeyChange(proposedValue, oTrans);
        }
        public bool AfterRebutProductionKeyChange(string keyValue, ref DataRow newRow)
        {
            return jobBatchProductionManager.AfterRebutProductionKeyChange(keyValue, oTrans, ref newRow);
        }
        public string BeforeRebutProfileKeyChange(string proposedValue)
        {
            return jobBatchProductionManager.BeforeRebutProfileKeyChange(proposedValue);
        }
        public bool AfterRebutProfileKeyChange(string keyValue, ref DataRow newRow)
        {
            return jobBatchProductionManager.AfterRebutProfileKeyChange(keyValue, oTrans, ref newRow);
        }
        public string BeforeDecoupeBrutKeyChange(string proposedValue)
        {
            return jobBatchProductionManager.BeforeDecoupeBrutKeyChange(proposedValue);
        }
        public bool AfterDecoupeBrutKeyChange(string keyValue, ref DataRow newRow)
        {
            return jobBatchProductionManager.AfterDecoupeBrutKeyChange(keyValue, oTrans, ref newRow);
        }
        public string BeforePultrusionKeyChange(string proposedValue)
        {
            return jobBatchProductionManager.BeforePultrusionKeyChange(proposedValue);
        }
        public bool AfterPultrusionKeyChange(string keyValue, ref DataRow newRow)
        {
            return jobBatchProductionManager.AfterPultrusionKeyChange(keyValue, oTrans, ref newRow);
        }
        public string BeforeProductionFieldChange(string fieldName, string currentValue, string proposedValue)
        {
            return jobBatchProductionManager.BeforeProductionFieldChange(fieldName, currentValue, proposedValue, oTrans);
        }
        public bool AfterProductionFieldChange(string fieldName, string fieldValue, out bool saveAfterChange, out bool refreshAfterChange)
        {
            return jobBatchProductionManager.AfterProductionFieldChange(fieldName, fieldValue, oTrans, out saveAfterChange, out refreshAfterChange);
        }
        public string BeforeRebutProductionFieldChange(string fieldName, string currentValue, string proposedValue)
        {
            return jobBatchProductionManager.BeforeRebutProductionFieldChange(fieldName, currentValue, proposedValue, oTrans);
        }
        public bool AfterRebutProductionFieldChange(string fieldName, string fieldValue, out bool saveAfterChange)
        {
            return jobBatchProductionManager.AfterRebutProductionFieldChange(fieldName, fieldValue, oTrans, out saveAfterChange);
        }
        public string BeforeRebutProfileFieldChange(string fieldName, string currentValue, string proposedValue)
        {
            return jobBatchProductionManager.BeforeRebutProfileFieldChange(fieldName, currentValue, proposedValue, oTrans);
        }
        public bool AfterRebutProfileFieldChange(string fieldName, string fieldValue, out bool saveAfterChange)
        {
            return jobBatchProductionManager.AfterRebutProfileFieldChange(fieldName, fieldValue, oTrans, out saveAfterChange);
        }
        public string BeforeDecoupeBrutFieldChange(string fieldName, string currentValue, string proposedValue)
        {
            return jobBatchProductionManager.BeforeDecoupeBrutFieldChange(fieldName, currentValue, proposedValue, oTrans);
        }
        public bool AfterDecoupeBrutFieldChange(string fieldName, string fieldValue, out bool saveAfterChange)
        {
            return jobBatchProductionManager.AfterDecoupeBrutFieldChange(fieldName, fieldValue, oTrans, out saveAfterChange);
        }
        public string BeforePultrusionFieldChange(string fieldName, string currentValue, string proposedValue)
        {
            return jobBatchProductionManager.BeforePultrusionFieldChange(fieldName, currentValue, proposedValue, oTrans);
        }
        public bool AfterPultrusionFieldChange(string fieldName, string fieldValue, out bool saveAfterChange)
        {
            return jobBatchProductionManager.AfterPultrusionFieldChange(fieldName, fieldValue, oTrans, out saveAfterChange);
        }
        public void SetupRowRule(ref EpiDataView edv)
        {
            jobBatchProductionManager.MainSetupRowRule(oTrans, ref edv);
        }
        public void ShowPanels()
        {
            ClosePanels();
            jobBatchProductionManager.ShowPanels();
        }
        public void Clear()
        {
            jobBatchProductionManager.Clear();
        }
    }
    
}
