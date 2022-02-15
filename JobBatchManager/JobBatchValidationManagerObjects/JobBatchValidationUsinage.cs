using System;
using System.Collections.Generic;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Customization;
using Ice.Lib.ExtendedProps;
using Infragistics.Win.UltraWinGrid;
using JobBatchManagerObjects.AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchValidationManagerObjects
{
    public class JobBatchValidationUsinage : JobBatchValidationManager, IJobBatchValidationManager
    {
        public JobBatchValidationUsinage(CustomScriptManager csm) : base(csm) { }
        protected override void SetJobBatchJobNum()
        {
            jobBatchJobNum = GetJobFictiveForType("Volvo");
        }
        public string JobBatchTitle { get { return "Validation JobBatch Sur mesure"; } }
        protected override void SetPanels()
        {
            try
            {
                AddPanel("e15f2050-131d-40fb-b9d6-8ee6ee8fc915", "ValidRepartition");
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }
        protected override void AfterCalculateLabor(EpiTransaction oTrans)
        {
            EpiDataView edvTempsRepartirMOD = this.GetView("TempsRepartirMOD");
            int row = -1;
            if (((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).TableHasRow("TempsRepartir"))
            {
                row = 0;
            }
            edvTempsRepartirMOD.Notify(new EpiNotifyArgs(oTrans, row, EpiTransaction.NotifyType.Initialize));
        }
        public void UpdateTempsRepartirView(Validator validator, EpiTransaction oTrans)
        {
            try
            {
                EpiDataView edvTempsRepartirMOD = this.GetView("TempsRepartirMOD");
                edvTempsRepartirMOD.dataView.Table.Rows.Clear();
                validator.GetMODView(ref edvTempsRepartirMOD);
                int r = -1;
                if (edvTempsRepartirMOD.dataView.Table.Rows.Count > 0)
                {
                    r = 0;
                }
                edvTempsRepartirMOD.Notify(new EpiNotifyArgs(oTrans, r, EpiTransaction.NotifyType.Initialize));

                EpiDataView edvTempsRepartirFGF = this.GetView("TempsRepartirFGF");
                edvTempsRepartirFGF.dataView.Table.Rows.Clear();
                validator.GetFGFView(ref edvTempsRepartirFGF);
                int r2 = -1;
                if (edvTempsRepartirFGF.dataView.Table.Rows.Count > 0)
                {
                    r2 = 0;
                }
                edvTempsRepartirFGF.Notify(new EpiNotifyArgs(oTrans, r2, EpiTransaction.NotifyType.Initialize));

                EpiDataView edvTempsRepartirOth = this.GetView("TempsRepartirOth");
                edvTempsRepartirOth.dataView.Table.Rows.Clear();
                validator.GetOthView(ref edvTempsRepartirOth);
                int r3 = -1;
                if (edvTempsRepartirOth.dataView.Table.Rows.Count > 0)
                {
                    r3 = 0;
                }
                edvTempsRepartirOth.Notify(new EpiNotifyArgs(oTrans, r3, EpiTransaction.NotifyType.Initialize));
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }

        public override Dictionary<string, string> GetPoinconLayout()
        {
            return new Dictionary<string, string>() {
                { "EmployeeNum", "Opérateur" },
                { "ResourceID", "Équipement" },
                { "JobNum", "No BT" },
                { "OpCode", "Opération" },
                { "LaborType", "Type" },
                { "ClockInDate", "Date d'entrée" },
                { "DspClockInTime", "Poinçon entrée" },
                { "DspClockOutTime", "Poinçon sortie" }
            };
        }
        public override Dictionary<string, string> GetRepartitionLayout()
        {
            return new Dictionary<string, string>() {
                { "Character06", "Équipement" },
                { "Character07", "No BT" },
                { "Character09", "Opération" },
                { "Character05", "No lot" },
                { "Character01", "No pièce" },
                { "Number05", "Qte produite" }
            };
        }
        public override Dictionary<string, string> GetRebutProductionLayout()
        {
            return new Dictionary<string, string>() {
                { "Character06", "Équipement" },
                { "Character07", "No BT" },
                { "Character09", "Opération" },
                { "Character05", "No lot" },
                { "Character01", "No pièce" },
                { "ShortChar03", "Raison" },
                { "Number05", "Qte jetée" }
            };
        }
    }
}
