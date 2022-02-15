using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Ice.Adapters;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class OperationInteractionsManager : AdapterBase<UD12Adapter>, IAdapterBase
    {
        private Dictionary<string, IOperationInteraction> opInteractions;
        public OperationInteractionsManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager)
        {
            opInteractions = new Dictionary<string, IOperationInteraction>();
        }
        public override void AfterConnect()
        {
            GetOperationInteractions();
        }
        private void GetOperationInteractions()
        {
            string whereClause = "Company = '01'";
            Hashtable hashtable = new Hashtable();
            hashtable.Add("UD12", whereClause);
            SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.RowsDataSet);
            searchOptions.DataSetMode = DataSetMode.RowsDataSet;
            bool morePages;
            DataSet ds = adapter.GetRows(searchOptions, out morePages);
            foreach (DataRow row in ds.Tables["UD12"].Rows)
            {
                string type = row["Key2"].ToString();
                if (!opInteractions.ContainsKey(type))
                {
                    switch (type)
                    {
                        case "NoFGF":
                            opInteractions[type] = new NoFGFInteraction();
                            break;
                        case "IsOperGeneral":
                            opInteractions[type] = new OperGeneralInteraction();
                            break;
                        case "NoLot":
                            opInteractions[type] = new NoLotInteraction();
                            break;
                        case "IsSetup":
                            opInteractions[type] = new IsSetupInteraction();
                            break;
                        case "ApplyOnFirstJob":
                            opInteractions[type] = new ApplyOnFirstJobInteraction();
                            break;
                        case "UseCurrentLot":
                            opInteractions[type] = new UseCurrentLotInteraction();
                            break;
                        case "UseMtlLot":
                            opInteractions[type] = new UseMtlLotInteraction();
                            break;
                        case "UsePrevOpLot":
                            opInteractions[type] = new UsePrevOpLotInteraction();
                            break;
                        case "DenyConcurrentClockIn":
                            opInteractions[type] = new DenyConcurrentClockInInteraction();
                            break;
                        case "ScrapMtlLot":
                            opInteractions[type] = new ScrapMtlLotInteraction();
                            break;
                    }
                }
                opInteractions[type].Add(row["Key1"].ToString(), row["Character01"].ToString());
            }
        }
        public bool OperationIs(string key, string opCode)
        {
            return ((CRTIMiscActionManager)this.adapterManager.GetManager("CRTIMiscActionManager")).OperationIs(key, opCode);
            /*f( !opInteractions.ContainsKey(key) )
			{
				return false;
			}
			return opInteractions[key].ContainsOperation(opCode);*/
        }
        public bool GetOperationSetupRef(string opCode, out string opRef)
        {
            opRef = "";
            if (!opInteractions.ContainsKey("IsSetup"))
            {
                return false;
            }
            bool flag = ((IsSetupInteraction)opInteractions["IsSetup"]).GetOperationSetupRef(opCode, out opRef);
            return flag;
        }
        public List<string> GetPunchOperationConcurrentRestrictions(string opCode)
        {
            if (!opInteractions.ContainsKey("DenyConcurrentClockIn"))
            {
                return new List<string>();
            }
            return ((DenyConcurrentClockInInteraction)opInteractions["DenyConcurrentClockIn"]).GetPunchOperationConcurrentRestrictions(opCode);
        }
        public string GetScrapPlaceHolder(string opCode)
        {
            if (!opInteractions.ContainsKey("ScrapMtlLot"))
            {
                return string.Empty;
            }
            return ((ScrapMtlLotInteraction)opInteractions["ScrapMtlLot"]).GetScrapPlaceHolder(opCode);
        }
        public interface IOperationInteraction
        {
            void Add(string key1, string key2);
            bool ContainsOperation(string opCode);
        }
        public abstract class BaseInteraction
        {
            protected List<string> opCodes;
            public BaseInteraction()
            {
                opCodes = new List<string>();
            }
            public virtual void Add(string key1, string key2)
            {
                opCodes.Add(key1);
            }
            public bool ContainsOperation(string opCode)
            {
                return opCodes.Contains(opCode);
            }
        }
        public class NoFGFInteraction : BaseInteraction, IOperationInteraction
        {
            public NoFGFInteraction() : base() { }
        }
        public class OperGeneralInteraction : BaseInteraction, IOperationInteraction
        {
            public OperGeneralInteraction() : base() { }
        }
        public class NoLotInteraction : BaseInteraction, IOperationInteraction
        {
            public NoLotInteraction() : base() { }
        }
        public class IsSetupInteraction : BaseInteraction, IOperationInteraction
        {
            private Dictionary<string, string> setupReferences;
            public IsSetupInteraction() : base()
            {
                setupReferences = new Dictionary<string, string>();
            }
            public override void Add(string key1, string key2)
            {
                base.Add(key1, key2);
                setupReferences[key1] = key2;
            }
            public bool GetOperationSetupRef(string opCode, out string opRef)
            {
                opRef = "";
                if (!setupReferences.ContainsKey(opCode))
                {
                    return false;
                }
                opRef = setupReferences[opCode];
                return true;
            }
        }
        public class ApplyOnFirstJobInteraction : BaseInteraction, IOperationInteraction
        {
            public ApplyOnFirstJobInteraction() : base() { }
        }
        public class UseCurrentLotInteraction : BaseInteraction, IOperationInteraction
        {
            public UseCurrentLotInteraction() : base() { }
        }
        public class UseMtlLotInteraction : BaseInteraction, IOperationInteraction
        {
            public UseMtlLotInteraction() : base() { }
        }
        public class UsePrevOpLotInteraction : BaseInteraction, IOperationInteraction
        {
            public UsePrevOpLotInteraction() : base() { }
        }
        public class DenyConcurrentClockInInteraction : BaseInteraction, IOperationInteraction
        {
            private Dictionary<string, List<string>> concurrentRestrictions;
            public DenyConcurrentClockInInteraction() : base()
            {
                concurrentRestrictions = new Dictionary<string, List<string>>();
            }
            public override void Add(string key1, string key2)
            {
                base.Add(key1, key2);
                if (!concurrentRestrictions.ContainsKey(key1))
                {
                    concurrentRestrictions[key1] = new List<string>();
                }
                concurrentRestrictions[key1].Add(key2);
            }
            public List<string> GetPunchOperationConcurrentRestrictions(string opCode)
            {
                List<string> list = new List<string>();
                if (concurrentRestrictions.ContainsKey(opCode))
                {
                    list = concurrentRestrictions[opCode];
                }
                return list;
            }
        }
        public class ScrapMtlLotInteraction : BaseInteraction, IOperationInteraction
        {
            private Dictionary<string, List<string>> scrapPlaceHolder;
            public ScrapMtlLotInteraction() : base()
            {
                scrapPlaceHolder = new Dictionary<string, List<string>>();
            }
            public override void Add(string key1, string key2)
            {
                base.Add(key1, key2);
                if (!scrapPlaceHolder.ContainsKey(key1))
                {
                    scrapPlaceHolder[key1] = new List<string>();
                }
                scrapPlaceHolder[key1].Add(key2);
            }
            public string GetScrapPlaceHolder(string opCode)
            {
                List<string> list = new List<string>();
                if (scrapPlaceHolder.ContainsKey(opCode))
                {
                    list = scrapPlaceHolder[opCode];
                }
                if (list.Any())
                {
                    return list[0];
                }
                return "";
            }
        }
    }
}
