using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using Com.Boc.Icms.LogDLL;

namespace Com.Boc.Icms.MetadataEdit.DataTables
{
    public class BaseDateTable<T> : DataTable, IBaseTableOperate<T>
    {
        private readonly string _proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        public BaseDateTable()
        {
            Type t = typeof(T);
            foreach (System.Reflection.PropertyInfo info in t.GetProperties())//datatable列不区分大小写
            {
                Type type = info.PropertyType;
                DataColumn dataColumn = new DataColumn(info.Name, type);

                if (type.Name.ToLower() == "string")
                {
                    dataColumn.DefaultValue = "";
                }
                else if (type.Name.ToLower() == "int")
                {
                    dataColumn.DefaultValue = -1;
                }

                this.Columns.Add(new DataColumn(info.Name, type));
                if ("modi_time".Equals(info.Name.ToLower()))
                {
                    this.RowChanged += BaseDateTable_RowChanged; //在EndEdit时更改时间
                }
            }

        }

        bool isTiemChanged = false;
        private void BaseDateTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (!isTiemChanged && e.Action == DataRowAction.Change)
            {
                isTiemChanged = true;
                try
                {
                    e.Row["modi_time"] = this.Now;
                }
                catch
                {

                }
                isTiemChanged = false;
            }
        }

        /// <summary>
        /// 取得当前日期和时间的字符串(格式：yyyy-MM-ddTHH:mm:ssZ)
        /// </summary>
        /// <returns></returns>
        public virtual string Now
        {
            get { return DateTime.Now.ToString("s") + "Z"; }
        }

        public DataRow AddRow(T t)
        {
            DataRow dr = this.NewRow();
            var newObjs = typeof(T);

            try
            {
                foreach (PropertyInfo newObjInfo in newObjs.GetProperties())
                {
                    dr[newObjInfo.Name] = newObjInfo.GetValue(t, null);
                }
                this.Rows.Add(dr);
                return dr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <returns></returns>
        public void Delete(string filterExpression)
        {
            DataRow[] rowArray = this.Select(filterExpression);
            if (rowArray.Length <= 0) return;

            try
            {
                rowArray[0].Delete();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T FindbyKey(object key)
        {
            DataRow dataRow = this.Rows.Find(key);

            if (dataRow != null)
            {
                Type type = typeof(T);
                T t = Activator.CreateInstance<T>();

                foreach (var property in type.GetProperties())
                {
                    //datatable列不区分大小写
                    object value = dataRow[property.Name];
                    if (property.CanWrite && !(value is DBNull))
                    {
                        property.SetValue(t, value, null);
                    }
                }

                return t;
            }
            else
            {
                return default(T);
            }

        }

        /// <summary>
        /// 查询节点
        /// </summary>
        /// <returns></returns>
        public T[] Find(params string[] parameters)
        {
            DataRow[] selectedNodes;
            if (parameters.Length == 0)
            {
                selectedNodes = this.Select();
            }
            else if (parameters.Length == 1)
            {
                selectedNodes = this.Select(parameters[0]);
            }
            else
            {
                selectedNodes = this.Select(parameters[0], parameters[1]);
            }
            if (selectedNodes.Length <= 0)
            {
                return new T[0];
            }
            else
            {
                T[] returnArray = new T[selectedNodes.Length];
                Type type = typeof(T);
                for (int rowIndex = 0; rowIndex < selectedNodes.Length; rowIndex++)
                {
                    T t = Activator.CreateInstance<T>();

                    foreach (var property in type.GetProperties())
                    {
                        //datatable列不区分大小写
                        object value = selectedNodes[rowIndex][property.Name];
                        if (property.CanWrite && !(value is DBNull))
                        {
                            property.SetValue(t, value, null);
                        }
                    }
                    returnArray[rowIndex] = t;
                }

                return returnArray;
            }
        }

        public void UpdateCellValue(string filterExpression, string column, object value)
        {
            DataRow[] rowArray = this.Select(filterExpression);
            if (rowArray.Count() <= 0) return;

            rowArray[0].BeginEdit();
            rowArray[0][column] = value;
            try
            {
                rowArray[0].EndEdit();
            }
            catch (Exception ex)
            {
                SysLog.Write(7108, ex, this._proIdAndThreadId);
                throw;
            }
        }
    }
}
