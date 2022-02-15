using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class JobEntryManager : AdapterBase<JobEntryAdapter>, IAdapterBase
    {
        public JobEntryManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public bool ValidateJobNum(string jobNum)
        {
            return adapter.ValidateJobNum(jobNum);
        }

        public bool GetJobOperOpCode(string jobNum, int oprSeq, out string opCode, out bool qtyEntry)
        {
            qtyEntry = false;
            opCode = string.Empty;
            Hashtable hashtable = new Hashtable();
            hashtable.Add("JobHead", "JobNum = '" + jobNum.Replace("'", "''") + "' and JobClosed = false and JobComplete = false and JobEngineered = true and JobReleased = true");
            hashtable.Add("JobOper", "JobNum = '" + jobNum.Replace("'", "''") + "' and AssemblySeq = 0 and OprSeq = " + oprSeq.ToString());
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages = false;
            DataSet ds = adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["JobOper"].Rows.Count > 0)
            {
                opCode = ds.Tables["JobOper"].Rows[0]["OpCode"].ToString();
                string[] qtyEntryMethod = new string[] { "Q", "T" };
                qtyEntry = qtyEntryMethod.Contains(ds.Tables["JobOper"].Rows[0]["LaborEntryMethod"].ToString());
                return true;
            }
            return false;
        }

        public bool GetJobDetailsForMelange(string jobNum, out DataSet dsJob)
        {
            dsJob = null;
            try
            {
                List<string> melangesPartNums;
                if (!((PartManager)this.adapterManager.GetManager("PartManager")).GetMelanges(out melangesPartNums))
                {
                    throw new Exception("Aucune pièce «Melange» dans le système.\r\nVeuillez contacter l'administrateur du système.");
                }
                string melangesSearch = string.Join("','", melangesPartNums);
                Hashtable hashtable = new Hashtable();
                hashtable.Add("JobHead", "JobNum = '" + jobNum.Replace("'", "''") + "' and JobClosed = false and JobComplete = false and JobEngineered = true and JobReleased = true");
                hashtable.Add("JobAsmbl", "JobNum = '" + jobNum.Replace("'", "''") + "' and PartNum in ('" + melangesSearch + "')");
                hashtable.Add("JobOper", "JobNum = '" + jobNum.Replace("'", "''") + "' and OpCode = 'MELANGE'");
                SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
                searchOptions.DataSetMode = DataSetMode.RowsDataSet;
                bool morePages = false;
                DataSet ds = adapter.GetRows(searchOptions, out morePages);
                if (ds.Tables["JobHead"].Rows.Count == 0)
                {
                    throw new Exception("Le bon de travail est introuvable.");
                }
                dsJob = ds;
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        public bool GetJobOperDetails(string jobNum, string opCode, out int oprSeq)
        {
            oprSeq = -1;
            Hashtable hashtable = new Hashtable();
            hashtable.Add("JobHead", "JobNum = '" + jobNum.Replace("'", "''") + "' and JobClosed = false and JobComplete = false and JobEngineered = true and JobReleased = true");
            hashtable.Add("JobOper", "JobNum = '" + jobNum.Replace("'", "''") + "' and AssemblySeq = 0 and OpCode = '" + opCode.Replace("'", "''") + "'");
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages = false;
            DataSet ds = adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["JobOper"].Rows.Count > 0)
            {
                oprSeq = int.Parse(ds.Tables["JobOper"].Rows[0]["OprSeq"].ToString());
                return true;
            }
            return false;
        }

        public bool GetMtlSeqDetails(string jobNum, string partNum, out int asmSeq, out int mtlSeq)
        {
            asmSeq = -1;
            mtlSeq = -1;
            Hashtable hashtable = new Hashtable();
            hashtable.Add("JobHead", "JobNum = '" + jobNum.Replace("'", "''") + "' and JobClosed = false and JobComplete = false and JobEngineered = true and JobReleased = true");
            hashtable.Add("JobMtl", "JobNum = '" + jobNum.Replace("'", "''") + "' and PartNum = '" + partNum.Replace("'", "''") + "'");
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages = false;
            DataSet ds = adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["JobMtl"].Rows.Count > 0)
            {
                asmSeq = int.Parse(ds.Tables["JobMtl"].Rows[0]["AssemblySeq"].ToString());
                mtlSeq = int.Parse(ds.Tables["JobMtl"].Rows[0]["MtlSeq"].ToString());
                return true;
            }
            return false;
        }
    }
}
