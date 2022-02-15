using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class TimeExpenseManager : AdapterBase<LaborAdapter>, IAdapterBase
    {
        public TimeExpenseManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public void CreateJobBatchLabor(int hedSeq, DateTime clockDate, string jobNum, int oprSeq, string resID, string refJobBatch, string refBatchLigneOperateur, string idLigne, bool isSetup, bool isOperGeneral, string operCompl)
        {
            Clear();
            adapter.GetByID(hedSeq);
            adapter.GetNewLaborDtlWithHdr(clockDate, 0m, clockDate, 0.01m, hedSeq);
            DataRow newRow = adapter.LaborData.LaborDtl.Rows[adapter.LaborData.LaborDtl.Rows.Count - 1];
            adapter.DefaultJobNum(jobNum);
            adapter.DefaultAssemblySeq(0);
            string vMessage = string.Empty;
            adapter.DefaultOprSeq(oprSeq, out vMessage);
            if (isSetup)
            {
                adapter.DefaultLaborType("S");
                adapter.ChangeLaborType();
            }

            adapter.CheckResourceGroup(resID, out vMessage);
            adapter.OverridesResource(resID);
            adapter.LaborRateCalc();
            adapter.CheckWarnings(out vMessage);
            if (isOperGeneral)
            {
                newRow["LaborRate"] = 0m;
            }
            newRow["UD_RefJobBatch_c"] = refJobBatch;
            newRow["UD_RefBatchLigneOperateur_c"] = refBatchLigneOperateur;
            newRow["UD_IDLigneProduction_c"] = idLigne;
            newRow["UD_OpComplementaire_c"] = operCompl;
            adapter.Update();
        }

        public void CreateJobBatchLabor(int hedSeq, DateTime clockDate, string jobBatchJobNum, int oprSeq, string resID, string refJobBatch, string jobNum, string refBatchLigneOperateur, bool isSetup)
        {
            adapter.ClearData();
            adapter.ClearList();
            adapter.GetByID(hedSeq);
            adapter.GetNewLaborDtlWithHdr(clockDate, 0m, clockDate, 0.01m, hedSeq);
            DataRow newRow = adapter.LaborData.LaborDtl.Rows[adapter.LaborData.LaborDtl.Rows.Count - 1];
            adapter.DefaultJobNum(jobBatchJobNum);
            adapter.DefaultAssemblySeq(0);
            string vMessage = string.Empty;
            adapter.DefaultOprSeq(oprSeq, out vMessage);
            adapter.CheckResourceGroup(resID, out vMessage);
            adapter.OverridesResource(resID);
            adapter.LaborRateCalc();
            adapter.CheckWarnings(out vMessage);
            newRow["LaborRate"] = 0m;
            newRow["UD_RefJobBatch_c"] = refJobBatch;
            if (isSetup)
            {
                newRow["UD_SetupJobNum_c"] = jobNum;
            }
            else
            {
                newRow["UD_OperJobNum_c"] = jobNum;
            }
            newRow["UD_RefBatchLigneOperateur_c"] = refBatchLigneOperateur;
            adapter.Update();
            newRow["RowMod"] = "U";
            adapter.SubmitForApproval(false, out vMessage);
        }

        public string CreateLaborDtl(string batchSysRowID, int hedSeq, DateTime clockDate, string jobNum, string resID, string opCode, bool isSetup, decimal tempsAttribueEmployeJob, decimal hrsFGF, decimal prodQty = 0m)
        {
            string sysRowID = string.Empty;
            adapter.ClearData();
            adapter.ClearList();
            int assSeq = 0;
            int oprSeq;
            bool flag2 = ((JobEntryManager)this.adapterManager.GetManager("JobEntryManager")).GetJobOperDetails(jobNum, opCode, out oprSeq);
            if (!flag2)
            {
                throw new UIException();
            }
            adapter.GetByID(hedSeq);
            decimal tempsEmploye = tempsAttribueEmployeJob == 0m ? 0.01m : tempsAttribueEmployeJob;
            adapter.GetNewLaborDtlWithHdr(clockDate, 0m, clockDate, tempsEmploye, hedSeq);
            if (isSetup)
            {
                adapter.DefaultLaborType("S");
                adapter.ChangeLaborType();
            }
            DataRow newRow = adapter.LaborData.LaborDtl.Rows[adapter.LaborData.LaborDtl.Rows.Count - 1];
            newRow.BeginEdit();
            if (tempsAttribueEmployeJob == 0m)
            {
                newRow["LaborHrs"] = 0.1m;
            }
            newRow["BurdenHrs"] = hrsFGF;
            newRow["RowMod"] = "A";
            newRow.EndEdit();
            adapter.DefaultJobNum(jobNum);
            adapter.DefaultAssemblySeq(assSeq);
            string vMessage = string.Empty;
            adapter.DefaultOprSeq(oprSeq, out vMessage);
            if (!string.IsNullOrEmpty(vMessage))
            {
                EpiMessageBox.Show(vMessage);
            }
            if (!string.IsNullOrEmpty(resID))
            {
                string vMessage2 = "";
                bool flag3 = adapter.CheckResourceGroup(resID, out vMessage2);
                adapter.OverridesResource(resID);
            }

            adapter.LaborRateCalc();
            if (prodQty != 0m)
            {
                string vMessage3 = "";
                adapter.DefaultLaborQty(prodQty, out vMessage3);
                if (vMessage3 != "")
                {
                    EpiMessageBox.Show(vMessage3);
                }
                newRow["LaborQty"] = prodQty;
            }
            string vMessage4 = "";
            adapter.CheckWarnings(out vMessage4);
            if (!string.IsNullOrEmpty(vMessage4) && EpiMessageBox.Show(vMessage4, EpiString.GetString("warningMsg_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                throw new UIException();
            }
            newRow["UD_RefBatchSubmission_c"] = batchSysRowID;
            adapter.Update();
            newRow = adapter.LaborData.LaborDtl.Rows[adapter.LaborData.LaborDtl.Rows.Count - 1];
            newRow["RowMod"] = "U";
            string cMessageText = string.Empty;
            adapter.SubmitForApproval(false, out cMessageText);
            if (!string.IsNullOrEmpty(cMessageText))
            {
                EpiMessageBox.Show(cMessageText);
            }
            sysRowID = newRow["SysRowID"].ToString();
            return sysRowID;
        }

        public bool DeleteLaborDtl(string empID, DateTime payrollDateTime, int laborDtlSeq)
        {
            try
            {
                string payrollDate = FormFunctions.GetDateString(payrollDateTime);
                GetLaborData(empID, payrollDate);
                int laborRow;
                if (!GetRowByLaborDtlSeq(laborDtlSeq, out laborRow))
                {
                    throw new Exception("L'entrée de temps est introuvable.");
                }
                if (this.adapter.LaborData.LaborDtl.Rows[laborRow]["TimeStatus"].ToString() == "A")
                {
                    this.adapter.LaborData.LaborDtl.Rows[laborRow]["RowMod"] = "U";
                    string cMessageText = string.Empty;
                    this.adapter.RecallFromApproval(false, out cMessageText);
                    GetLaborData(empID, payrollDate);
                    if (!GetRowByLaborDtlSeq(laborDtlSeq, out laborRow))
                    {
                        throw new Exception("L'entrée de temps est introuvable.");
                    }
                }
                this.adapter.Delete(this.adapter.LaborData.LaborDtl.Rows[laborRow]);
                this.adapter.Update();
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
            finally
            {
                adapter.ClearData();
                adapter.ClearList();
            }
        }

        private void GetLaborData(string empID, string payrollDate)
        {
            adapter.ClearData();
            adapter.ClearList();
            string whereClauseLH = string.Format("EmployeeNum = '{0}' AND PayrollDate = '{1}'", empID, payrollDate);
            string whereClauseLD = "ActiveTrans = 0";
            Hashtable hashtable = new Hashtable();
            hashtable.Add("LaborHed", whereClauseLH);
            hashtable.Add("LaborDtl", whereClauseLD);
            hashtable.Add("LaborDtlAttach", string.Empty);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.CustomArgs.Add("EmployeeNum", empID);
            searchOptions.CustomArgs.Add("WeekBeginDate", payrollDate);
            searchOptions.CustomArgs.Add("WeekEndDate", payrollDate);
            searchOptions.SearchMethod = adapter.GetRowsCalendarView;
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            adapter.InvokeSearch(searchOptions);
        }

        private bool GetRowByLaborDtlSeq(int laborDtlSeq, out int row)
        {
            row = -1;
            for (int i = 0; i < this.adapter.LaborData.LaborDtl.Rows.Count; i++)
            {
                if (int.Parse(this.adapter.LaborData.LaborDtl.Rows[i]["LaborDtlSeq"].ToString()) == laborDtlSeq)
                {
                    row = i;
                    return true;
                }
            }
            return false;
        }

        public bool OperationHasMOD(string refPoincon, string opCode, out List<string> jobNums)
        {
            jobNums = new List<string>();
            string whereClause = string.Format("OpCode = '{0}' and UD_RefJobBatch_c = '{1}' and LaborType = 'P'", opCode, refPoincon);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("LaborDtl", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = adapter.GetRows(searchOptions, out morePages);
            foreach (DataRow row in ds.Tables["LaborDtl"].Rows)
            {
                string jobNum = row["JobNum"].ToString();
                if (!jobNums.Contains(jobNum))
                {
                    jobNums.Add(jobNum);
                }
            }
            return jobNums.Any();
        }

        public bool IsClockInSystem(string empID)
        {
            Clear();
            Hashtable hashtable = new Hashtable();
            hashtable.Add("LaborHed", "EmployeeNum = '" + empID.Replace("'", "''") + "' and ActiveTrans = yes");
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            this.adapter.InvokeSearch(searchOptions);
            return this.adapter.LaborData.LaborHed.Rows.Count > 0;
        }

        public bool GetCurrentPayrollDate(string empID, out DateTime payrollDate)
        {
            payrollDate = DateTime.Now;
            if (this.adapter.LaborData.LaborHed.Rows.Count == 0 || !this.adapter.LaborData.LaborHed.Rows[0]["EmployeeNum"].ToString().Equals(empID))
            {
                return false;
            }
            payrollDate = DateTime.Parse(this.adapter.LaborData.LaborHed.Rows[0]["PayrollDate"].ToString());
            return true;
        }

        public bool GetCurrentHedSeq(string empID, out int hedSeq)
        {
            hedSeq = -1;
            if (this.adapter.LaborData.LaborHed.Rows.Count == 0 || !this.adapter.LaborData.LaborHed.Rows[0]["EmployeeNum"].ToString().Equals(empID))
            {
                return false;
            }
            hedSeq = int.Parse(this.adapter.LaborData.LaborHed.Rows[0]["LaborHedSeq"].ToString());
            return true;
        }
    }
}
