using System;
using System.Data;
using System.Linq;
using Ice.Lib;
using Ice.Lib.Framework;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class CRTIMiscActionManager : AdapterBase<CRTI_MiscActionAdapter>, IAdapterBase
    {
        private string dsMiscActionXML;
        private DataSet dsMiscAction;
        public DataSet DsMiscAction { get { return dsMiscAction; } }
        public CRTIMiscActionManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public override void AfterConnect()
        {
            InitDataSet();
        }

        private DataSet LoadDataSet(string dataSetJson)
        {
            return dataSetJson.DeserializeFromString<DataSet>();
        }

        private void SetDataSet(string dataSetXml)
        {
            this.dsMiscActionXML = dataSetXml;
            this.dsMiscAction.Clear();
            this.dsMiscAction.Merge(LoadDataSet(dataSetXml), true, MissingSchemaAction.Ignore);
        }

        public void InitDataSet()
        {
            object result;
            adapter.CallCustomMethod("Initializer.InitDataSet", out result);
            this.dsMiscAction = ((string)result).DeserializeFromString<DataSet>();
            this.dsMiscAction.Clear();
            this.dsMiscAction.AcceptChanges();
            this.dsMiscActionXML = this.dsMiscAction.SerializeObject();
        }

        public void InitDataSet(string type)
        {
            object result;
            adapter.CallCustomMethod("Initializer.InitDataSet", out result, (object)type);
            this.dsMiscAction = ((string)result).DeserializeFromString<DataSet>();
            ClearDataSet();
        }

        public void ClearDataSet()
        {
            string[] except = new string[] { "JobBatch", "EquipementJobBatch", "OperationJobBatch" };
            foreach (DataTable dt in this.dsMiscAction.Tables)
            {
                if (!except.Contains(dt.TableName))
                {
                    dt.Rows.Clear();
                }
            }
            this.dsMiscAction.AcceptChanges();
            this.dsMiscActionXML = this.dsMiscAction.SerializeObject();
        }

        public void ClearRows(string tableName)
        {
            if (this.dsMiscAction.Tables.Contains(tableName))
            {
                this.dsMiscAction.Tables[tableName].Rows.Clear();
                this.dsMiscAction.AcceptChanges();
                this.dsMiscActionXML = this.dsMiscAction.SerializeObject();
            }
        }

        public bool TableHasRow(string tableName)
        {
            if (!this.dsMiscAction.Tables.Contains(tableName))
            {
                return false;
            }
            return this.dsMiscAction.Tables[tableName].Rows.Count > 0;
        }

        public bool ItemInList(string tableName, string colName, string value)
        {
            bool inList = false;
            if (!this.dsMiscAction.Tables.Contains(tableName))
            {
                return false;
            }
            if (!this.dsMiscAction.Tables[tableName].Columns.Contains(colName))
            {
                return false;
            }
            foreach (DataRow row in this.dsMiscAction.Tables[tableName].Rows)
            {
                if (row[colName].ToString() == value)
                {
                    inList = true;
                    break;
                }
            }
            return inList;
        }

        public void GetBTJobFictive()
        {
            object dataset;
            adapter.CallCustomMethod("GestionJobBatch.GetBTJobFictive", out dataset, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public bool OperationIs(string key, string opCode)
        {
            object result;
            object dataset;
            adapter.CallCustomMethod("OperationInteraction.OperationIs", out result, out dataset, (object)key, (object)opCode, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
            return (bool)result;
        }

        public string[] GetInteractionsForOpCode(string opCode)
        {
            object result;
            adapter.CallCustomMethod("OperationInteraction.GetInteractionsForOpCode", out result, (object)opCode);
            string interactions = result.ToString();
            return interactions.Split('~');
        }

        public void GetJobOperStockProfileBrut(string jobNum, int assemblySeq, int oprSeq)
        {
            object dataset;
            adapter.CallCustomMethod("StockProfileBrut.GetJobOperStockProfileBrut", out dataset, (object)jobNum, (object)assemblySeq, (object)oprSeq, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetStockProfileBrut(string partNum)
        {
            object dataset;
            adapter.CallCustomMethod("StockProfileBrut.GetStockProfileBrut", out dataset, (object)partNum, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetProfileDecoupeBrut()
        {
            object dataset;
            adapter.CallCustomMethod("StockProfileBrut.GetProfileDecoupeBrut", out dataset, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetPartFIFOCost(string partNum, string lotNum, string partTranPKs)
        {
            object dataset;
            adapter.CallCustomMethod("StockProfileBrut.GetPartFIFOCost", out dataset, (object)partNum, (object)lotNum, (object)partTranPKs, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetListLotForJobNumFromProfile(string resId, string jobNum, string opCode)
        {
            object dataset;
            adapter.CallCustomMethod("GestionLotProduction.GetListForJobNumFromProfile", out dataset, (object)resId, (object)jobNum, (object)opCode, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetListEquipementForJobBatch(string jobNum)
        {
            object dataset;
            adapter.CallCustomMethod("GestionJobBatch.GetListForJobBatch", out dataset, (object)jobNum, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetProfileBrutLocalisation(string partNum, string lotNum)
        {
            object dataset;
            adapter.CallCustomMethod("StockProfileBrut.GetProfileBrutLocalisation", out dataset, (object)partNum, (object)lotNum, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetLotNumForLastOperation(string jobNum, string opCode)
        {
            object dataset;
            adapter.CallCustomMethod("GestionJobBatch.GetLotNumForLastOperation", out dataset, (object)jobNum, (object)opCode, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public bool CheckCanDeleteLotNumForOperation(string idLigne, string lotNum, string rowID, out string message)
        {
            object result;
            object oMessage;
            adapter.CallCustomMethod("GestionJobBatch.CheckCanDeleteLotNumForOperation", out result, out oMessage, (object)idLigne, (object)lotNum, (object)rowID);
            message = oMessage.ToString();
            return Convert.ToBoolean(result.ToString());
        }

        public void GetListScrapReasonForOperation(string opCode)
        {
            object dataset;
            adapter.CallCustomMethod("GestionOperation.GetListScrapReasonForOperation", out dataset, (object)opCode, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public void GetLaborHedSeqForPayrollDate(string empID, DateTime payrollDate, out int hedSeq)
        {
            hedSeq = -1;
            object hSeq;
            string strPayrollDate = FormFunctions.GetDateString(payrollDate);
            adapter.CallCustomMethod("ValidationJobBatch.GetLaborHedSeqForPayrollDate", out hSeq, (object)empID, (object)strPayrollDate);
            hedSeq = int.Parse(hSeq.ToString());
        }

        public string GetRefBatchLigneOperateur(string refPoincon, string empID, string oprSeq)
        {
            object refBatchLigneOperateur;
            adapter.CallCustomMethod("ValidationJobBatch.GetRefBatchLigneOperateur", out refBatchLigneOperateur, (object)refPoincon, (object)empID, (object)oprSeq);
            return refBatchLigneOperateur.ToString();
        }

        public void GetAllOperationComplementaire()
        {
            object dataset;
            adapter.CallCustomMethod("GestionJobBatch.GetAllOperationComplementaire", out dataset, (object)this.dsMiscActionXML);
            SetDataSet((string)dataset);
        }

        public bool GetOperSeq(string jobNum, string opCode, out int oprSeq)
        {
            object result;
            object opSeq;
            adapter.CallCustomMethod("GestionJobBatch.GetOperSeq", out result, out opSeq, (object)jobNum, (object)opCode);
            oprSeq = int.Parse(opSeq.ToString());
            return Convert.ToBoolean(result);
        }

        public bool Reserver(string idLigneFrom, string idLigneTo, string jobNum, string mtlSeq, string partNum, string lotNum, decimal quantity)
        {
            try
            {
                adapter.CallCustomMethod("GestionReservationWIP.Reserver", idLigneFrom, idLigneTo, jobNum, mtlSeq, partNum, lotNum, quantity);
                return true;
            }
            catch(Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        public bool Annuler(string idLigneFrom, string idLigneTo, string mtlSeq, string partNum, string lotNum, decimal quantity)
        {
            try
            {
                adapter.CallCustomMethod("GestionReservationWIP.Annuler", idLigneFrom, idLigneTo, mtlSeq, partNum, lotNum, quantity);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        public void GetWeekJobBatchs(string jobBatchJobNum, DateTime fromDate, DateTime toDate)
        {
            object result;
            string strFromDate = FormFunctions.GetDateString(fromDate);
            string strToDate = FormFunctions.GetDateString(toDate);
            adapter.CallCustomMethod("ValidationJobBatch.GetWeekJobBatchs", out result, (object)this.dsMiscActionXML, (object)jobBatchJobNum, (object)strFromDate, (object)strToDate);
            SetDataSet((string)result);
        }

        public void CalculateLabor(DataSet dsLabor)
        {
            string xmlLabor = dsLabor.SerializeObject();
            object result;
            adapter.CallCustomMethod("ValidationJobBatch.CalculateLabor", out result, (object)xmlLabor, (object)this.dsMiscActionXML);
            SetDataSet((string)result);
        }

        public void SubmitLabor(DataSet dsLabor)
        {
            string xmlLabor = dsLabor.SerializeObject();
            object result;
            adapter.CallCustomMethod("ValidationJobBatch.SubmitLabor", out result, (object)xmlLabor, (object)this.dsMiscActionXML);
            SetDataSet((string)result);
        }

        public void GetValidationSelection(string jobBatchJobNum, string dateSelection, string shift)
        {
            adapter.CallCustomMethod("ValidationJobBatch.GetValidationSelection", (object)jobBatchJobNum, (object)dateSelection, (object)shift);
        }

        public string GetRefPoinconForNewPunch(string jobBatchJobNum, DateTime dateJobBatch, string resourceID, string quart)
        {
            object refPoincon;
            string strDateJobBatch = FormFunctions.GetDateString(dateJobBatch);
            adapter.CallCustomMethod("ValidationJobBatch.GetRefPoinconForNewPunch", out refPoincon, (object)jobBatchJobNum, (object)strDateJobBatch, (object)resourceID, (object)quart);
            return refPoincon.ToString();
        }

        public bool CheckAllClockedOut(string refJobBatchs, out string message)
        {
            object clockedOut;
            object msg;
            adapter.CallCustomMethod("ValidationJobBatch.CheckAllClockedOut", out clockedOut, out msg, (object)refJobBatchs);
            message = msg.ToString();
            return (bool)clockedOut;
        }
    }
}
