using System.Data;
using System.Collections.Generic;
using Ice.Lib.Framework;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchProductionManagerObjects
{
    public interface IJobBatchProductionManager
    {
        string JobBatchJobNum { get; }
        string JobBatchTitle { get; }
        string EmployeeListFilter { get; }
        void SetAdapterManager(AdapterManager _adapterManager);
        void AddViewReference(string key, ref EpiDataView edv);
        void SetCodeChangeData(bool value);
        Dictionary<string, string> GetProductionLayout();
        Dictionary<string, string> GetRebutProductionLayout();
        Dictionary<string, string> GetRebutProfileLayout();
        Dictionary<string, string> GetDecoupeBrutLayout();
        Dictionary<string, string> GetPultrusionLayout();
        void ClockIn(int hedSeq, int oprSeq, string resID, string empID, EpiTransaction oTrans);
        void OnGetRow(string viewName, DataRowView row, EpiTransaction oTrans, bool forceRefresh);
        void MainSetupRowRule(EpiTransaction oTrans, ref EpiDataView edv);
        void Clear();
        void ShowPanels();
        string[] GetRequiredFields();
        string BeforeProductionKeyChange(string proposedValue);
        bool AfterProductionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow);
        string BeforeRebutProductionKeyChange(string proposedValue, EpiTransaction oTrans);
        bool AfterRebutProductionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow);
        string BeforeRebutProfileKeyChange(string proposedValue);
        bool AfterRebutProfileKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow);
        string BeforeDecoupeBrutKeyChange(string proposedValue);
        bool AfterDecoupeBrutKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow);
        string BeforePultrusionKeyChange(string proposedValue);
        bool AfterPultrusionKeyChange(string keyValue, EpiTransaction oTrans, ref DataRow newRow);
        string BeforeProductionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans);
        bool AfterProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange, out bool refreshAfterChange);
        string BeforeRebutProductionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans);
        bool AfterRebutProductionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange);
        string BeforeRebutProfileFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans);
        bool AfterRebutProfileFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange);
        string BeforeDecoupeBrutFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans);
        bool AfterDecoupeBrutFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange);
        string BeforePultrusionFieldChange(string fieldName, string currentValue, string proposedValue, EpiTransaction oTrans);
        bool AfterPultrusionFieldChange(string fieldName, string fieldValue, EpiTransaction oTrans, out bool saveAfterChange);
    }
}
