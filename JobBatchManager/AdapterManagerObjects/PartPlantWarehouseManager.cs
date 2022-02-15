using System;
using System.Collections;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Erp.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class PartPlantWarehouseManager : AdapterBase<PartPlantWhseSearchAdapter>, IAdapterBase
    {
        public PartPlantWarehouseManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public bool ValidateWarehouseCode(string partNum, string whsCode)
        {
            try
            {
                Hashtable hashtable = new Hashtable();
                SearchOptions opts = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.ListDataSet);
                hashtable.Add("PartNum", partNum);
                bool morePages;
                DataSet rows = adapter.GetRows(opts, out morePages);
                if (rows == null || rows.Tables["PartPlantWhseSearch"].Rows.Count == 0 || rows.Tables["PartPlantWhseSearch"].Select(string.Format("WarehouseCode = '{0}' ", whsCode.Replace("'", "''"))).Length == 0)
                {
                    throw new Exception(string.Format("L'entrepôt de la pièce {0} est invalide", partNum));
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }
    }
}
