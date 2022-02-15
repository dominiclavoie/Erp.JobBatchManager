using System.Collections;
using System.Data;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class OperationManager : AdapterBase<OpMasterAdapter>, IAdapterBase
    {
        public OperationManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public string GetDescriptionByOpCode(string opCode)
        {
            string opDesc = "";
            string whereClause = string.Format("OpCode = '{0}'", opCode);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("OpMaster", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["OpMaster"].Rows.Count > 0)
            {
                opDesc = ds.Tables["OpMaster"].Rows[0]["OpDesc"].ToString();
            }
            return opDesc;
        }
    }
}
