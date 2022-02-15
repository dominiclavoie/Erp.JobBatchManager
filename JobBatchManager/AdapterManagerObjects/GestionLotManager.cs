using System;
using System.Collections;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Ice.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class GestionLotManager : AdapterBase<UD11Adapter>, IAdapterBase
    {
        public GestionLotManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }
        private EpiDataView edvUD105;

        public void SetViewReference(EpiDataView view)
        {
            this.edvUD105 = view;
        }

        private bool GenerateNewLotByJobNum(string jobNum, out string noLot)
        {
            noLot = "";
            string resID = this.edvUD105.CurrentDataRow["Character02"].ToString();
            this.adapter.ClearData();
            this.adapter.GetaNewUD11();
            DataRow newRow = this.adapter.UD11Data.UD11.Rows[this.adapter.UD11Data.UD11.Rows.Count - 1];
            newRow["Key1"] = "NoLot";
            newRow["Key2"] = resID;
            newRow["Key3"] = jobNum;
            newRow["RowMod"] = "A";
            this.adapter.Update();
            noLot = this.adapter.UD11Data.UD11.Rows[this.adapter.UD11Data.UD11.Rows.Count - 1]["Character01"].ToString();
            return true;
        }

        public bool GenerateLotForJobNumFromProfile(string jobNum, string opCode, string noLot, string partNumProfile, int ratio, int mtlSeq)
        {
            try
            {
                string resID = this.edvUD105.CurrentDataRow["Character02"].ToString();
                this.adapter.ClearData();
                this.adapter.GetaNewUD11();
                DataRow newRow = this.adapter.UD11Data.UD11.Rows[this.adapter.UD11Data.UD11.Rows.Count - 1];
                newRow["Key1"] = "NoLot";
                newRow["Key2"] = resID;
                newRow["Key3"] = jobNum;
                newRow["Character01"] = noLot;
                newRow["ShortChar01"] = partNumProfile;
                newRow["Number01"] = Convert.ToDecimal(ratio);
                newRow["ShortChar02"] = opCode;
                newRow["ShortChar04"] = mtlSeq;
                newRow["RowMod"] = "A";
                bool flag = this.adapter.Update();
                noLot = this.adapter.UD11Data.UD11.Rows[this.adapter.UD11Data.UD11.Rows.Count - 1]["Character01"].ToString();
                return flag;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        public bool GetLotForJobNum(string jobNum, out string noLot)
        {
            noLot = string.Empty;
            if (!this.edvUD105.HasRow)
            {
                return false;
            }
            string resID = this.edvUD105.CurrentDataRow["Character02"].ToString();
            string whereClause = string.Format("Key1 = 'NoLot' and Key2 = '{0}' and Key3 = '{1}' by cast(Key5 as int) DESC", resID, jobNum);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("UD11", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = this.adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["UD11"].Rows.Count > 0)
            {
                noLot = ds.Tables["UD11"].Rows[0]["Character01"].ToString();
                return true;
            }
            else
            {
                if (GenerateNewLotByJobNum(jobNum, out noLot))
                {
                    return true;
                }
            }
            return false;
        }

        public bool GetLotForJobNumFromProfile(string jobNum, string opCode, out string noLot, out string partNumProfile, out decimal ratio, out int mtlSeq)
        {
            noLot = string.Empty;
            partNumProfile = string.Empty;
            ratio = 0m;
            mtlSeq = -1;
            if (!this.edvUD105.HasRow)
            {
                return false;
            }
            string resID = this.edvUD105.CurrentDataRow["Character02"].ToString();
            string whereClause = string.Format("Key1 = 'NoLot' and Key2 = '{0}' and Key3 = '{1}' and ShortChar02 = '{2}' by cast(Key5 as int) DESC", resID, jobNum, opCode);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("UD11", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = this.adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["UD11"].Rows.Count > 0)
            {
                noLot = ds.Tables["UD11"].Rows[0]["Character01"].ToString();
                partNumProfile = ds.Tables["UD11"].Rows[0]["ShortChar01"].ToString();
                ratio = decimal.Parse(ds.Tables["UD11"].Rows[0]["Number01"].ToString());
                mtlSeq = int.Parse(ds.Tables["UD11"].Rows[0]["ShortChar04"].ToString());
                return true;
            }
            else
            {
                if (GenerateNewLotByJobNum(jobNum, out noLot))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasCurrentLot()
        {
            if (!this.edvUD105.HasRow)
            {
                return false;
            }
            string resID = this.edvUD105.CurrentDataRow["Character02"].ToString();
            string whereClause = string.Format("Key1 = 'NoLot' and Key2 = '{0}'", resID);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("UD11", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = this.adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["UD11"].Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        public bool GetCurrentLot(out string currentLot, out string partNum)
        {
            currentLot = string.Empty;
            partNum = string.Empty;
            if (!this.edvUD105.HasRow)
            {
                return false;
            }
            string resID = this.edvUD105.CurrentDataRow["Character02"].ToString();
            string whereClause = string.Format("Key1 = 'NoLot' and Key2 = '{0}' by cast(Key5 as int) DESC", resID);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("UD11", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = this.adapter.GetRows(searchOptions, out morePages);
            if (ds.Tables["UD11"].Rows.Count > 0)
            {
                currentLot = ds.Tables["UD11"].Rows[0]["Character01"].ToString();
                partNum = ds.Tables["UD11"].Rows[0]["Key3"].ToString();
                return true;
            }
            return false;
        }
    }
}
