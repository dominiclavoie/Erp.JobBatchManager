using System.Data;
using System.Collections.Generic;
using Ice.Lib.Framework;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchValidationManagerObjects
{
    interface IJobBatchValidationManager
    {
        string JobBatchJobNum { get; }
        string JobBatchTitle { get; }
        void SetAdapterManager(AdapterManager _adapterManager);
        void AddViewReference(string key, ref EpiDataView edv);
        void CalculateLabor(DataSet dsJobBatch, EpiTransaction oTrans);
        void UpdateTempsRepartirView(Validator validator, EpiTransaction oTrans);
        void CalculerRepartition(Validator validator);
        void SoumettreRepartition(Validator validator);
        void MainSetupRowRule(EpiTransaction oTrans, ref EpiDataView edv);
        void Clear();
        void ShowPanels();
        Dictionary<string, string> GetPoinconLayout();
        Dictionary<string, string> GetRepartitionLayout();
        Dictionary<string, string> GetRebutProductionLayout();
    }
}
