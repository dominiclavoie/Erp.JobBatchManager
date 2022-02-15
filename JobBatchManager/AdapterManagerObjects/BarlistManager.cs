using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Ice.Lib;
using Ice.Lib.Framework;
using Ice.Lib.Searches;
using Ice.Adapters;
using Ice.BO;
using AdapterManagerObjects;

namespace JobBatchManagerObjects.AdapterManagerObjects
{
    public class BarlistManager : AdapterBase<UD104Adapter>, IAdapterBase
    {
        public BarlistManager(EpiTransaction oTrans, AdapterManager _adapterManager) : base(oTrans, _adapterManager) { }

        public string ValidateBarlistIDProposedValue(string proposedValue, string refPoincon)
        {
            string opCode;
            return ValidateBarlistIDProposedValue(proposedValue, refPoincon, out opCode);
        }

        public string ValidateBarlistIDProposedValue(string proposedValue, string refPoincon, out string opCode)
        {
            opCode = "";
            string result = "";
            string ID;
            if (ValidateBarlistID(proposedValue, out ID))
            {
                result = ID;
                DataRow barlistRow;
                if (!GetBarlistLineByID(ID, refPoincon, out barlistRow, out opCode))
                {
                    return "";
                }
                try
                {
                    bool lotGeneratedOnClockIn = !((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UseMtlLot", opCode);
                    bool noLotForOperation = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("NoLot", opCode);
                    bool usePrevOpLot = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UsePrevOpLot", opCode);
                    if (lotGeneratedOnClockIn && !noLotForOperation && !usePrevOpLot)
                    {
                        string jobNum = barlistRow["Key3"].ToString();
                        string noLot;
                        if (!((GestionLotManager)this.adapterManager.GetManager("GestionLotManager")).GetLotForJobNum(jobNum, out noLot))
                        {
                            throw new Exception("Aucun lot pour le bon de travail sélectionné.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionBox.Show(ex);
                    return "";
                }
            }
            return result;
        }

        private bool ValidateBarlistID(string value, out string ID)
        {
            ID = string.Empty;
            int barlistID = 0;
            try
            {
                bool flag = int.TryParse(value, out barlistID);
                if (!flag)
                {
                    throw new Exception("L'ID doit être un nombre.");
                }
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
            ID = barlistID.ToString("D6");
            return true;
        }

        public bool GetBarlistLineByID(string ID, string refPoincon, out DataRow row, out string opCode)
        {
            //lots = null;
            row = null;
            opCode = string.Empty;
            try
            {
                this.adapter.ClearData();
                this.adapter.ClearList();
                Hashtable hashtable = new Hashtable();
                hashtable.Add("BaseList", "Key2 = 'JobOper' AND ShortChar01 = '" + ID.Replace("'", "''") + "'");
                SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.ListDataSet);
                searchOptions.DataSetMode = DataSetMode.ListDataSet;
                bool morePages = false;
                UD104ListDataSet ud104ListDataSet = (UD104ListDataSet)this.adapter.GetList(searchOptions, out morePages);
                if (ud104ListDataSet.UD104List.Rows.Count != 1)
                {
                    throw new Exception("La ligne de barlist est introuvable");
                }
                string jobNum = ud104ListDataSet.UD104List.Rows[0]["Key3"].ToString();
                int oprSeq = int.Parse(ud104ListDataSet.UD104List.Rows[0]["Key4"].ToString());
                bool qtyEntry;
                bool flag = ((JobEntryManager)this.adapterManager.GetManager("JobEntryManager")).GetJobOperOpCode(jobNum, oprSeq, out opCode, out qtyEntry);
                if (!flag)
                {
                    throw new Exception("Le bon de travail associé est introuvable");
                }
                if (!qtyEntry)
                {
                    string opDesc = ((OperationManager)this.adapterManager.GetManager("OperationManager")).GetDescriptionByOpCode(opCode);
                    throw new Exception(string.Format("L'entrée de quantité produite n'est pas requise pour l'opération {0} avec le bon de travail {1}", opDesc, jobNum));
                }
                /*if( ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("UseCurrentLot", opCode) )
				{
					string linePartNum = ud104ListDataSet.UD104List.Rows[0]["Character01"].ToString();
					string currentLot;
					string partNum;
					bool flag2 = ((GestionLotManager)this.adapterManager.GetManager("GestionLotManager")).GetCurrentLot(out currentLot, out partNum);
					if( !flag2 )
					{
						throw new Exception("Le lot courant est introuvable");
					}
					if( partNum != linePartNum )
					{
						throw new Exception("La pièce sélectionnée ne correspond pas à celle du lot courant");
					}
				}
				else
				{*/
                List<string> jobNums;
                if (!((TimeExpenseManager)this.adapterManager.GetManager("TimeExpenseManager")).OperationHasMOD(refPoincon, opCode, out jobNums))
                {
                    string opDesc = ((OperationManager)this.adapterManager.GetManager("OperationManager")).GetDescriptionByOpCode(opCode);
                    throw new Exception(string.Format("Vous devez d'abord poinçonner dans l'opération {0}", opDesc));
                }
                if (!jobNums.Contains(jobNum))
                {
                    string opDesc = ((OperationManager)this.adapterManager.GetManager("OperationManager")).GetDescriptionByOpCode(opCode);
                    throw new Exception(string.Format("Vous devez d'abord poinçonner dans l'opération {0} avec le bon de travail {1}", opDesc, jobNum));
                }
                /*lots = ((BAQManager)this.adapterManager.GetManager("BAQManager")).GetLotsOperationPrecedente(ID);
                if( lots.Rows.Count == 0 )
                {
                    throw new Exception("Aucun lot de produit pour l'opération précédente de cette ligne.");
                }*/
                //}
                row = ud104ListDataSet.UD104List.Rows[0];
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        public bool GetBarlistRowByID(string ID, out DataRow row)
        {
            row = null;
            try
            {
                this.adapter.ClearData();
                this.adapter.ClearList();
                Hashtable hashtable = new Hashtable();
                hashtable.Add("BaseList", "Key2 = 'JobOper' AND ShortChar01 = '" + ID.Replace("'", "''") + "'");
                SearchOptions searchOptions = SearchOptions.CreateRuntimeSearch(hashtable, DataSetMode.ListDataSet);
                searchOptions.DataSetMode = DataSetMode.ListDataSet;
                bool morePages = false;
                UD104ListDataSet ud104ListDataSet = (UD104ListDataSet)this.adapter.GetList(searchOptions, out morePages);
                if (ud104ListDataSet.UD104List.Rows.Count != 1)
                {
                    throw new Exception("La ligne de barlist est introuvable");
                }
                row = ud104ListDataSet.UD104List.Rows[0];
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
        }

        public bool UpdateBarlistLineQty(string ID, decimal tranQty)
        {
            try
            {
                DataRow barlistRow;
                if (!GetBarlistRowByID(ID, out barlistRow))
                {
                    return false;
                }
                this.adapter.ClearData();
                this.adapter.ClearList();
                this.adapter.GetByID(barlistRow["Key1"].ToString(), barlistRow["Key2"].ToString(), barlistRow["Key3"].ToString(), barlistRow["Key4"].ToString(), barlistRow["Key5"].ToString());
                if (this.adapter.UD104Data.UD104.Rows.Count != 1)
                {
                    throw new Exception("La ligne de barlist est introuvable");
                }
                DataRow row = this.adapter.UD104Data.UD104.Rows[0];
                row["RowMod"] = "U";
                decimal currQty = decimal.Parse(row["Number19"].ToString());
                row["Number19"] = currQty + tranQty;
                if (!this.adapter.Update())
                {
                    throw new Exception("La quantité n'a pas être mise à jour");
                }
                return true;
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
                return false;
            }
            finally
            {
                this.adapter.ClearData();
                this.adapter.ClearList();
            }
        }

    }
}
