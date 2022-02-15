using System;
using System.Collections;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Erp.Adapters;
using Erp.BO;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class WarehouseBinManager : AdapterBase<WhseBinAdapter>, IAdapterBase
    {
        public WarehouseBinManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public bool ValidateBinNum(string whsCode, string binNum)
        {
            try
            {
                Hashtable hashtable = new Hashtable();
                SearchOptions opts = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.ListDataSet);
                hashtable.Add("WhseBin", string.Format("WareHouseCode='{0}' AND BinNum='{1}'", whsCode, binNum));
                bool morePages;
                WhseBinDataSet whseBinDataSet = (WhseBinDataSet)adapter.GetRows(opts, out morePages);
                if (whseBinDataSet.WhseBin.Rows.Count != 1)
                {
                    throw new Exception(string.Format("La localisation de l'entrepôt {0} est invalide", whsCode));
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
