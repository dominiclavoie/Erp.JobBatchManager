using System;
using System.Collections.Generic;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Customization;
using Infragistics.Win.UltraWinDock;
using JobBatchManagerObjects.JobBatchValidationManagerObjects;
using JobBatchManagerObjects.AdapterManagerObjects;
using AdapterManagerObjects;

namespace JobBatchManagerObjects
{
	public class ValidationJobBatchManager
	{
		private JobBatchType jobBatchType;
		private IJobBatchValidationManager jobBatchValidationManager;
		private EpiTransaction oTrans;
		private List<DockablePaneBase> panels;
		public ValidationJobBatchManager(JobBatchType _jobBatchType, EpiTransaction _oTrans, CustomScriptManager csm)
		{
			jobBatchType = _jobBatchType;
			oTrans = _oTrans;
			this.panels = new List<DockablePaneBase>();
			SetAllPanels(csm);
			SetupJobBatch(csm);
		}
		public string JobBatchJobNum { get { return jobBatchValidationManager.JobBatchJobNum; } }
		public string JobBatchTitle { get { return jobBatchValidationManager.JobBatchTitle; } }
		public JobBatchType JobBatchType { get { return jobBatchType; } }
		public bool BatchTypeIs(JobBatchType _jobBatchType)
		{
			return jobBatchType == _jobBatchType;
		}
		private void SetupJobBatch(CustomScriptManager csm)
		{
			switch (jobBatchType)
			{
				case JobBatchType.VRODDroite:
					//jobBatchValidationManager = new JobBatchValidationVRODDroite(csm);
					break;
				case JobBatchType.ProfileBrut:
					//jobBatchValidationManager = new JobBatchValidationProfileBrut(csm);
					break;
				case JobBatchType.Usinage:
					jobBatchValidationManager = new JobBatchValidationUsinage(csm);
					break;
				case JobBatchType.Assemblage:
					jobBatchValidationManager = new JobBatchValidationAssemblage(csm);
					break;
				case JobBatchType.Melange:
					//jobBatchValidationManager = new JobBatchValidationMelange(csm);
					break;
			}
		}
		private void SetAllPanels(CustomScriptManager csm)
		{
			try
			{
				AddPanel(csm, "e15f2050-131d-40fb-b9d6-8ee6ee8fc915", "ValidRepartition");
				AddPanel(csm, "e15f2050-131d-40fb-b9d6-8ee6ee8fc915", "ValidRepartitionMelange");
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
				{ "JobEntryManager", new JobEntryManager(oTrans, _adapterManager) },
				{ "PartManager", new PartManager(oTrans, _adapterManager) },
				{ "OperationInteractionsManager", new OperationInteractionsManager(oTrans, _adapterManager) },
				{ "CRTIMiscActionManager", new CRTIMiscActionManager(oTrans, _adapterManager) }
			});
			_adapterManager.Connect();
			((CRTIMiscActionManager)_adapterManager.GetManager("CRTIMiscActionManager")).InitDataSet(jobBatchType.ToString());
			jobBatchValidationManager.SetAdapterManager(_adapterManager);
		}
		public void CalculateLabor(DataSet dsJobBatch)
		{
			jobBatchValidationManager.CalculateLabor(dsJobBatch, this.oTrans);
		}
		public void UpdateTempsRepartirView(Validator validator)
		{
			jobBatchValidationManager.UpdateTempsRepartirView(validator, this.oTrans);
		}
		public void CalculerRepartition(Validator validator)
		{
			jobBatchValidationManager.CalculerRepartition(validator);
		}
		public void SoumettreRepartition(Validator validator)
		{
			jobBatchValidationManager.SoumettreRepartition(validator);
		}
		public void OnGetRow(DataRowView row, bool forceRefresh = false)
		{
			//jobBatchValidationManager.OnGetRow(row, oTrans, forceRefresh);
		}
		public void AddViewReference(string key, ref EpiDataView edv)
		{
			jobBatchValidationManager.AddViewReference(key, ref edv);
		}
		public string BeforeProductionKeyChange(string proposedValue)
		{
			return "";// jobBatchValidationManager.BeforeProductionKeyChange(proposedValue);
		}
		public bool AfterProductionKeyChange(string keyValue, ref DataRow newRow)
		{
			return true;//jobBatchValidationManager.AfterProductionKeyChange(keyValue, oTrans, ref newRow);
		}
		public bool AfterProductionFieldChange(string fieldName, string fieldValue, out bool saveAfterChange)
		{
			saveAfterChange = false;
			return true;// jobBatchValidationManager.AfterProductionFieldChange(fieldName, fieldValue, oTrans, out saveAfterChange);
		}
		public void SetupRowRule(ref EpiDataView edv)
		{
			jobBatchValidationManager.MainSetupRowRule(oTrans, ref edv);
		}
		public void ShowPanels()
		{
			ClosePanels();
			jobBatchValidationManager.ShowPanels();
		}
		public void Clear()
		{
			jobBatchValidationManager.Clear();
		}
		public Dictionary<string, string> GetPoinconLayout()
		{
			return jobBatchValidationManager.GetPoinconLayout();
		}
		public Dictionary<string, string> GetRepartitionLayout()
		{
			return jobBatchValidationManager.GetRepartitionLayout();
		}
		public Dictionary<string, string> GetRebutProductionLayout()
		{
			return jobBatchValidationManager.GetRebutProductionLayout();
		}
	}
}
