using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.LogDLL;

namespace Com.Boc.Icms.MetadataEdit.Services
{
    public class DataServies
    {
        private DataSet dataSet = null;
        private readonly string _proIdAndThreadId = string.Empty;

        public DataServies()
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            dataSet = new DataSet();

            dataSet.Tables.AddRange(
                new DataTable[]
                {
                    new BusinessTable(),
                    new DocTable(),
                    new PageTable(),
                    new VersionTable(),
                    new DeletedPageTable()
                });
        }

        internal void Clear()
        {
            dataSet.Clear();
        }
        public DataTable GetDataTable(EnumType.TableType tableType)
        {
            return dataSet.Tables[tableType.ToString()];
        }


        public DataRow GetRowByKey(EnumType.TableType tableType, string keyValue)
        {
            return dataSet.Tables[tableType.ToString()].Rows.Find(keyValue);
        }


        public void DelectRowByKey(EnumType.TableType tableType, string keyValue)
        {
            dataSet.Tables[tableType.ToString()].Rows.Find(keyValue).Delete();
        }

        public DataRow[] GetRowsByType(EnumType.TableType tableType)
        {
            return dataSet.Tables[tableType.ToString()].Select();
        }


        public Models.Doc[] GetAllDocs()
        {
            return ((DocTable)dataSet.Tables["DocTable"]).Find();
        }


        /// <summary>
        /// 数据提交
        /// </summary>
        public void Commit()
        {
            try
            {
                this.dataSet.AcceptChanges();
            }
            catch (Exception ex)
            {
                SysLog.Write(7148, ex, this._proIdAndThreadId);
                throw;
            }
        }

        /// <summary>
        /// 数据回滚
        /// </summary>
        public void RollBack()
        {
            try
            {
                this.dataSet.RejectChanges();
            }
            catch (Exception ex)
            {
                SysLog.Write(7149, ex, this._proIdAndThreadId);
                throw;
            }
        }

    }
}
