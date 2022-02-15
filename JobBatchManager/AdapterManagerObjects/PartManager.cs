using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class PartManager : AdapterBase<PartAdapter>, IAdapterBase
    {
        public PartManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public bool GetMelanges(out List<string> partNums)
        {
            return GetPartsByClassID("FMel", out partNums);
        }

        public bool GetProfiles(out List<string> partNums)
        {
            return GetPartsByClassID("FPro", out partNums);
        }

        public bool GetPartsByClassID(string classID, out List<string> partNums)
        {
            partNums = new List<string>();
            string whereClause = string.Format("ClassID = '" + classID + "' and InActive = false");
            Hashtable hashtable = new Hashtable();
            hashtable.Add("Part", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = this.adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["Part"].Rows.Count > 0)
            {
                partNums = ds.Tables["Part"].AsEnumerable().Select(x => x["PartNum"].ToString()).ToList();
                return true;
            }
            return false;
        }

        public bool GetPartRev(string partNum, out string revisionNum)
        {
            revisionNum = "";
            string dateToday = DateTime.Today.ToShortDateString();
            string whereClause = string.Format("PartNum = '" + partNum + "' and EffectiveDate <= '" + dateToday + "' and Approved = true by EffectiveDate DESC");
            Hashtable hashtable = new Hashtable();
            hashtable.Add("PartRev", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = this.adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["PartRev"].Rows.Count > 0)
            {
                revisionNum = ds.Tables["PartRev"].Rows[0]["RevisionNum"].ToString();
                return true;
            }
            return false;
        }

        public bool PartExists(string partNum)
        {
            return this.adapter.PartExists(partNum);
        }

    }
}
