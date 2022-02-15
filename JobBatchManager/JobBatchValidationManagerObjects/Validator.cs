using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Ice.Lib.Framework;
using AdapterManagerObjects;
using JobBatchManagerObjects.AdapterManagerObjects;

namespace JobBatchManagerObjects.JobBatchValidationManagerObjects
{
    public class Validator
    {
        private AdapterManager adapterManager;
        private ValidatorMOD validMOD;
        private ValidatorFGF validFGF;
        public Validator(AdapterManager _adapterManager)
        {
            adapterManager = _adapterManager;
            validMOD = new ValidatorMOD(_adapterManager);
            validFGF = new ValidatorFGF(_adapterManager);
        }
        public void Add(DataRow row)
        {
            validMOD.Add(row);
            validFGF.Add(row);
        }
        public void Calculate()
        {
            validMOD.SplitTime();
            validFGF.CountFGFTime();
            validMOD.RegroupTime();
        }
        public void GetMODView(ref EpiDataView edv)
        {
            validMOD.GetMODView(ref edv);
        }
        public void GetFGFView(ref EpiDataView edv)
        {
            validFGF.GetFGFView(ref edv);
        }
        public void GetOthView(ref EpiDataView edv)
        {
            validMOD.GetOthView(ref edv);
        }
        public void GetMODMelView(ref EpiDataView edv)
        {
            validMOD.GetMODMelView(ref edv);
            validFGF.GetFGFMelView(ref edv);
        }
        public decimal GetMODHours(string resID, string opCode, string noLot)
        {
            return validMOD.GetMODHours(resID, opCode, noLot);
        }
        public decimal GetMODMelHours(string opCode)
        {
            return validMOD.GetMODMelHours(opCode);
        }
        public decimal GetResWithFGFMODHours(string resID)
        {
            return validMOD.GetResWithFGFMODHours(resID);
        }
        public decimal GetFGFHours(string resID)
        {
            return validFGF.GetFGFHours(resID);
        }
        public decimal GetTotalFGFHours()
        {
            return validFGF.GetTotalFGFHours();
        }
        public decimal GetResMODHours(string resID)
        {
            return validMOD.GetResMODHours(resID);
        }
        public Dictionary<string, ValidEmployee> GetEmployees()
        {
            return validMOD.Employees;
        }
        public Dictionary<string, ValidResource> GetMODResources()
        {
            return validMOD.GetMODResources();
        }
        public Dictionary<string, ValidResource> GetOthResources()
        {
            return validMOD.GetOthResources();
        }
    }
    public class ValidatorFGF
    {
        private AdapterManager adapterManager;
        private Dictionary<string, ValidResource> resources;
        private decimal totalHours;
        public ValidatorFGF(AdapterManager _adapterManager)
        {
            adapterManager = _adapterManager;
            resources = new Dictionary<string, ValidResource>();
            totalHours = 0m;
        }
        public void Add(DataRow row)
        {
            string opCode = row["OpCode"].ToString();
            if (((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("NoFGF", opCode))
            {
                return;
            }
            string resID = row["ResourceID"].ToString();
            if (!resources.ContainsKey(resID))
            {
                resources[resID] = new ValidResource(resID, adapterManager);
            }
            resources[resID].Add(row);
        }
        public void CountFGFTime()
        {
            decimal totalMinutes = 0m;
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                totalMinutes += resource.Value.CountFGFTime();
            }
            totalHours = totalMinutes / 60m;
        }
        public void GetFGFView(ref EpiDataView edv)
        {
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                DataRow row = edv.dataView.Table.NewRow();
                row["Équipement"] = resource.Key == "Empty" ? "" : resource.Key;
                row["FGF"] = resource.Value.TotalHours;
                edv.dataView.Table.Rows.Add(row);
            }
        }
        public void GetFGFMelView(ref EpiDataView edv)
        {
            decimal totalMOD = 0m;
            foreach (DataRow row in edv.dataView.Table.Rows)
            {
                string opCode = row["Opération"].ToString();
                if (!((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("NoFGF", opCode))
                {
                    totalMOD += decimal.Parse(row["MOD"].ToString());
                }
            }
            if (totalMOD == 0m)
            {
                return;
            }
            foreach (DataRow row in edv.dataView.Table.Rows)
            {
                decimal mod = decimal.Parse(row["MOD"].ToString());
                decimal fgf = 0m;
                string opCode = row["Opération"].ToString();
                if (!((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("NoFGF", opCode))
                {
                    fgf = totalHours * mod / totalMOD;
                }
                row["FGF"] = fgf;
            }
        }
        public decimal GetFGFHours(string resID)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].TotalHours;
        }
        public decimal GetTotalFGFHours()
        {
            return totalHours;
        }
    }
    public class ValidatorMOD
    {
        private AdapterManager adapterManager;
        private Dictionary<string, ValidEmployee> employees;
        private Dictionary<string, ValidResource> resources;
        private decimal totalHours;
        public ValidatorMOD(AdapterManager _adapterManager)
        {
            adapterManager = _adapterManager;
            employees = new Dictionary<string, ValidEmployee>();
            resources = new Dictionary<string, ValidResource>();
            totalHours = 0m;
        }
        public Dictionary<string, ValidEmployee> Employees { get { return employees; } }
        public Dictionary<string, ValidResource> GetMODResources()
        {
            return GetResources(false);
        }
        public Dictionary<string, ValidResource> GetOthResources()
        {
            return GetResources(true);
        }
        private Dictionary<string, ValidResource> GetResources(bool isOth)
        {
            Dictionary<string, ValidResource> res = new Dictionary<string, ValidResource>();
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                foreach (KeyValuePair<string, ValidOperation> operation in resource.Value.Items)
                {
                    bool isOthOper = (operation.Value.IsSetup || operation.Value.IsApplyOnFirstJob);
                    if (isOth == isOthOper)
                    {
                        if (!res.ContainsKey(resource.Key))
                        {
                            res[resource.Key] = new ValidResource(resource.Key, adapterManager);
                        }
                        res[resource.Key].Items[operation.Key] = operation.Value;
                    }
                }
            }
            return res;
        }
        public void Add(DataRow row)
        {
            string employeeNum = row["EmployeeNum"].ToString();
            if (!employees.ContainsKey(employeeNum))
            {
                employees[employeeNum] = new ValidEmployee(employeeNum, adapterManager);
            }
            employees[employeeNum].Add(row);
        }
        public void SplitTime()
        {
            decimal totalMinutes = 0m;
            foreach (KeyValuePair<string, ValidEmployee> employee in employees)
            {
                totalMinutes += employee.Value.SplitTime();
            }
            totalHours = totalMinutes / 60m;
        }
        public void RegroupTime()
        {
            resources.Clear();
            foreach (KeyValuePair<string, ValidEmployee> employee in employees)
            {
                foreach (KeyValuePair<string, ValidClockDate> clockDate in employee.Value.Items)
                {
                    foreach (KeyValuePair<string, ValidResource> resource in clockDate.Value.Items)
                    {
                        if (!resources.ContainsKey(resource.Key))
                        {
                            resources[resource.Key] = new ValidResource(resource.Key, adapterManager);
                        }
                        resources[resource.Key].Merge(resource.Value);
                        if (!employee.Value.Resources.ContainsKey(resource.Key))
                        {
                            employee.Value.Resources[resource.Key] = new ValidResource(resource.Key, adapterManager);
                        }
                        employee.Value.Resources[resource.Key].Merge(resource.Value);
                    }
                }
            }
        }
        public void GetMODView(ref EpiDataView edv)
        {
            GetView(ref edv, false);
        }
        public void GetOthView(ref EpiDataView edv)
        {
            GetView(ref edv, true);
        }
        public void GetView(ref EpiDataView edv, bool isOthView)
        {
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                foreach (KeyValuePair<string, ValidOperation> operation in resource.Value.Items)
                {
                    bool isOthOper = (operation.Value.IsSetup || operation.Value.IsApplyOnFirstJob);
                    if (isOthView != isOthOper)
                    {
                        continue;
                    }
                    string colName = isOthView ? "No Job" : "No lot";
                    foreach (KeyValuePair<string, ValidLot> lot in operation.Value.Items)
                    {
                        DataRow row = edv.dataView.Table.NewRow();
                        row["Équipement"] = resource.Key == "Empty" ? "" : resource.Key;
                        row["Opération"] = operation.Key;
                        row[colName] = lot.Value.Key;
                        row["MOD"] = lot.Value.TotalHours;
                        edv.dataView.Table.Rows.Add(row);
                    }
                }
            }
        }
        public void GetMODMelView(ref EpiDataView edv)
        {
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                foreach (KeyValuePair<string, ValidOperation> operation in resource.Value.Items)
                {
                    foreach (KeyValuePair<string, ValidLot> lot in operation.Value.Items)
                    {
                        DataRow row = edv.dataView.Table.NewRow();
                        row["Opération"] = operation.Key;
                        row["MOD"] = lot.Value.TotalHours;
                        row["FGF"] = 0m;
                        edv.dataView.Table.Rows.Add(row);
                    }
                }
            }
        }
        public decimal GetResMODHours(string resID)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].TotalHours;
        }
        public decimal GetResWithFGFMODHours(string resID)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].GetResWithFGFMODHours();
        }
        public decimal GetMODHours(string resID, string opCode, string noLot)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].GetMODHours(opCode, noLot);
        }
        public decimal GetMODMelHours(string opCode)
        {
            decimal modHours = 0m;
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                foreach (KeyValuePair<string, ValidOperation> operation in resource.Value.Items)
                {
                    if (operation.Key != opCode)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<string, ValidLot> lot in operation.Value.Items)
                    {
                        modHours += lot.Value.TotalHours;
                    }
                }
            }
            return modHours;
        }
    }
    public abstract class ValidObject<T>
    {
        protected AdapterManager adapterManager;
        protected string key;
        protected string itemsKey;
        protected decimal totalHours;
        protected Dictionary<string, T> items;
        public string Key { get { return key; } }
        public Dictionary<string, T> Items { get { return items; } }
        public decimal TotalHours { get { return totalHours; } set { totalHours = value; } }
        public ValidObject(string _key, AdapterManager _adapterManager)
        {
            key = _key;
            adapterManager = _adapterManager;
            items = new Dictionary<string, T>();
            totalHours = 0m;
        }
        public void Add(DataRow row, int clockIn, int clockOut)
        {
            string itemsKeyValue = row[itemsKey].ToString();
            if (!items.ContainsKey(itemsKeyValue))
            {
                items[itemsKeyValue] = (T)Activator.CreateInstance(typeof(T), new object[] { itemsKeyValue, this.adapterManager });
            }
            items[itemsKeyValue].GetType().GetMethod("Add").Invoke(items[itemsKeyValue], new object[] { row, clockIn, clockOut });
        }
        public decimal SplitTime(decimal[] clockMinutes)
        {
            decimal totalMinutes = 0m;
            foreach (KeyValuePair<string, T> item in items)
            {
                totalMinutes += (decimal)item.Value.GetType().GetMethod("SplitTime").Invoke(item.Value, new object[] { clockMinutes });
            }
            totalHours = totalMinutes / 60m;
            return totalMinutes;
        }
    }
    public class ValidEmployee : ValidObject<ValidClockDate>
    {
        private Dictionary<string, ValidResource> resources;
        public Dictionary<string, ValidResource> Resources { get { return resources; } }
        public ValidEmployee(string _key, AdapterManager _adapterManager) : base(_key, _adapterManager)
        {
            itemsKey = "ClockInDate";
            resources = new Dictionary<string, ValidResource>();
        }
        public void Add(DataRow row)
        {
            int clockIn = (int)Math.Round((decimal.Parse(row["ClockinTime"].ToString()) * 60m), MidpointRounding.ToEven);
            int startMin = int.Parse(row["ClockInMInute"].ToString());
            int endMin = int.Parse(row["ClockOutMinute"].ToString());
            int clockOut = clockIn + endMin - startMin;
            int nextDay = 0;
            if (clockOut > 1440)
            {
                nextDay = clockOut - 1440;
                clockOut = 1440;
            }

            DateTime clockDate = DateTime.ParseExact(row["ClockInDate"].ToString(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            string clockInDate = clockDate.ToString("yyyy-MM-dd");
            if (!items.ContainsKey(clockInDate))
            {
                items[clockInDate] = new ValidClockDate(clockInDate, adapterManager);
            }
            items[clockInDate].Add(row, clockIn, clockOut);

            if (nextDay > 0)
            {
                if (nextDay > 1440)
                {
                    nextDay = 1440;
                }

                clockDate = clockDate.AddDays(1);
                string nextDate = clockDate.ToString("yyyy-MM-dd");
                if (!items.ContainsKey(nextDate))
                {
                    items[nextDate] = new ValidClockDate(nextDate, adapterManager);
                }
                items[nextDate].Add(row, 1, nextDay);
            }
        }
        public decimal SplitTime()
        {
            decimal totalMinutes = 0m;
            foreach (KeyValuePair<string, ValidClockDate> clockDate in items)
            {
                totalMinutes += clockDate.Value.SplitTime();
            }
            totalHours = totalMinutes / 60m;
            return totalMinutes;
        }
        public decimal GetMODHours(string resID, string opCode, string noLot)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].GetMODHours(opCode, noLot);
        }
        public decimal GetMODMelHours(string opCode)
        {
            decimal modHours = 0m;
            foreach (KeyValuePair<string, ValidResource> resource in resources)
            {
                foreach (KeyValuePair<string, ValidOperation> operation in resource.Value.Items)
                {
                    if (operation.Key != opCode)
                    {
                        continue;
                    }
                    foreach (KeyValuePair<string, ValidLot> lot in operation.Value.Items)
                    {
                        modHours += lot.Value.TotalHours;
                    }
                }
            }
            return modHours;
        }
        public decimal GetResMODHours(string resID)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].TotalHours;
        }
        public decimal GetResWithFGFMODHours(string resID)
        {
            if (!resources.ContainsKey(resID))
            {
                return 0m;
            }
            return resources[resID].GetResWithFGFMODHours();
        }
        public bool HasResource(string resID)
        {
            return resources.ContainsKey(resID);
        }
        public bool HasOperation(string resID, string opCode)
        {
            if (!resources.ContainsKey(resID))
            {
                return false;
            }
            return resources[resID].HasOperation(opCode);
        }
        public bool HasLot(string resID, string opCode, string noLot)
        {
            if (!resources.ContainsKey(resID))
            {
                return false;
            }
            return resources[resID].HasLot(opCode, noLot);
        }
    }
    public class ValidClockDate : ValidObject<ValidResource>
    {
        private decimal[] minutes;
        public ValidClockDate(string _key, AdapterManager _adapterManager) : base(_key, _adapterManager)
        {
            itemsKey = "ResourceID";
            minutes = new decimal[1440];
        }
        public void Add(DataRow row, int clockIn, int clockOut, bool isFGF = false)
        {
            if (isFGF)
            {
                for (int i = clockIn + 1; i <= clockOut; i++)
                {
                    minutes[i - 1] = 1m;
                }
            }
            else
            {
                string resID = row["ResourceID"].ToString();
                if (!items.ContainsKey(resID))
                {
                    items[resID] = new ValidResource(resID, adapterManager);
                }
                items[resID].Add(row, clockIn, clockOut);

                for (int i = clockIn + 1; i <= clockOut; i++)
                {
                    minutes[i - 1] += 1m;
                }
            }
        }
        public decimal SplitTime()
        {
            decimal totalMinutes = 0m;
            foreach (KeyValuePair<string, ValidResource> resource in items)
            {
                totalMinutes += resource.Value.SplitTime(minutes);
            }
            totalHours = totalMinutes / 60m;
            return totalMinutes;
        }
        public decimal CountFGFTime()
        {
            decimal totalMinutes = (decimal)minutes.Count(s => s == 1m);
            totalHours = totalMinutes / 60m;
            return totalMinutes;
        }
    }
    public class ValidResource : ValidObject<ValidOperation>
    {
        private Dictionary<string, ValidClockDate> clockDates;
        public ValidResource(string _key, AdapterManager _adapterManager) : base(_key, _adapterManager)
        {
            itemsKey = "OpCode";
            clockDates = new Dictionary<string, ValidClockDate>();
        }
        public void Add(DataRow row)
        {
            int clockIn = (int)Math.Round((decimal.Parse(row["ClockinTime"].ToString()) * 60m), MidpointRounding.ToEven);
            int startMin = int.Parse(row["ClockInMInute"].ToString());
            int endMin = int.Parse(row["ClockOutMinute"].ToString());
            int clockOut = clockIn + endMin - startMin;
            int nextDay = 0;
            if (clockOut > 1440)
            {
                nextDay = clockOut - 1440;
                clockOut = 1440;
            }

            DateTime clockDate = DateTime.ParseExact(row["ClockInDate"].ToString(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            string clockInDate = clockDate.ToString("yyyy-MM-dd");
            if (!clockDates.ContainsKey(clockInDate))
            {
                clockDates[clockInDate] = new ValidClockDate(clockInDate, adapterManager);
            }
            clockDates[clockInDate].Add(row, clockIn, clockOut, true);

            if (nextDay > 0)
            {
                if (nextDay > 1440)
                {
                    nextDay = 1440;
                }

                clockDate = clockDate.AddDays(1);
                string nextDate = clockDate.ToString("yyyy-MM-dd");
                if (!clockDates.ContainsKey(nextDate))
                {
                    clockDates[nextDate] = new ValidClockDate(nextDate, adapterManager);
                }
                clockDates[nextDate].Add(row, 1, nextDay, true);
            }
        }
        public decimal CountFGFTime()
        {
            decimal totalMinutes = 0m;
            foreach (KeyValuePair<string, ValidClockDate> clockDate in clockDates)
            {
                totalMinutes += clockDate.Value.CountFGFTime();
            }
            totalHours = totalMinutes / 60m;
            return totalMinutes;
        }
        public void Merge(ValidResource other)
        {
            totalHours += other.TotalHours;
            foreach (KeyValuePair<string, ValidOperation> item in other.Items)
            {
                if (!items.ContainsKey(item.Key))
                {
                    items[item.Key] = new ValidOperation(item.Key, adapterManager);
                }
                items[item.Key].Merge(item.Value);
            }
        }
        public decimal GetMODHours(string opCode, string noLot)
        {
            if (!items.ContainsKey(opCode))
            {
                return 0m;
            }
            return items[opCode].GetMODHours(noLot);
        }
        public decimal GetResWithFGFMODHours()
        {
            decimal MODHours = 0m;
            foreach (KeyValuePair<string, ValidOperation> item in items)
            {
                if (!item.Value.IsNoFGF)
                {
                    MODHours += item.Value.TotalHours;
                }
            }
            return MODHours;
        }
        public bool HasOperation(string opCode)
        {
            return items.ContainsKey(opCode);
        }
        public bool HasLot(string opCode, string noLot)
        {
            if (!items.ContainsKey(opCode))
            {
                return false;
            }
            return items[opCode].HasLot(noLot);
        }
    }
    public class ValidOperation : ValidObject<ValidLot>
    {
        public bool IsSetup;
        public bool IsApplyOnFirstJob;
        public bool IsNoFGF;
        public ValidOperation(string _key, AdapterManager _adapterManager) : base(_key, _adapterManager)
        {
            itemsKey = "UD_RefCurrentLot_c";
            IsSetup = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("IsSetup", key);
            IsApplyOnFirstJob = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("ApplyOnFirstJob", key);
            IsNoFGF = ((OperationInteractionsManager)this.adapterManager.GetManager("OperationInteractionsManager")).OperationIs("NoFGF", key);
            if (IsSetup || IsApplyOnFirstJob)
            {
                itemsKey = "UD_SetupJobNum_c";
            }
        }
        new public void Add(DataRow row, int clockIn, int clockOut)
        {
            string itemsKeyValue = row[itemsKey].ToString();
            if (IsSetup || IsApplyOnFirstJob)
            {
                itemsKeyValue = "Job" + itemsKeyValue;
            }
            if (!items.ContainsKey(itemsKeyValue))
            {
                items[itemsKeyValue] = new ValidLot(row[itemsKey].ToString(), adapterManager);
            }
            items[itemsKeyValue].Add(row, clockIn, clockOut);
        }
        public void Merge(ValidOperation other)
        {
            totalHours += other.TotalHours;
            foreach (KeyValuePair<string, ValidLot> item in other.Items)
            {
                if (!items.ContainsKey(item.Key))
                {
                    items[item.Key] = new ValidLot(item.Value.Key, adapterManager);
                }
                items[item.Key].Merge(item.Value);
            }
        }
        public decimal GetMODHours(string noLot)
        {
            if (!items.ContainsKey(noLot))
            {
                return 0m;
            }
            return items[noLot].TotalHours;
        }
        public bool HasLot(string noLot)
        {
            return items.ContainsKey(noLot);
        }
    }
    public class ValidLot : ValidObject<object>
    {
        private decimal[] minutes;
        public ValidLot(string _key, AdapterManager _adapterManager) : base(_key, _adapterManager)
        {
            itemsKey = "";
            minutes = new decimal[1440];
        }
        new public void Add(DataRow row, int clockIn, int clockOut)
        {
            for (int i = clockIn + 1; i <= clockOut; i++)
            {
                minutes[i - 1] = 1m;
            }
        }
        new public decimal SplitTime(decimal[] clockMinutes)
        {
            decimal totalMinutes = 0m;
            for (int i = 0; i < 1440; i++)
            {
                if (clockMinutes[i] > 0m)
                {
                    totalMinutes += (minutes[i] / clockMinutes[i]);
                }
            }
            totalHours = totalMinutes / 60m;
            return totalMinutes;
        }
        public void Merge(ValidLot other)
        {
            decimal otherTotalHours = (decimal)other.TotalHours;
            totalHours += otherTotalHours;
        }
    }
}
