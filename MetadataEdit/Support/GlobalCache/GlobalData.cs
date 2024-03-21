using System.Data;

namespace Com.Boc.Icms.MetadataEdit.Support.GlobalCache
{
    /// <summary>
    /// 全局数据存取类
    /// </summary>
    sealed class GlobalData : DataTable
    {
        //private static GlobalData instanceData = null;
        //private static readonly object _object = new object();

        /// <summary>
        /// 数据结构
        /// </summary>
        //public static GlobalData DataSturct
        //{
        //    get { return GetInstance(); }
        //    set { instanceData = null; }
        //}

        public GlobalData()
        {
            //加入表的名称，解决无法转换XML的问题
            this.TableName = "DataSturct";

            DataColumn col = new DataColumn("ID", typeof(int));
            col.AutoIncrement = true;
            col.AutoIncrementSeed = 1;
            col.AutoIncrementStep = 1;
            col.Caption = "ID";
            col.ReadOnly = true;
            this.Columns.Add(col);
            this.PrimaryKey = new[] { col };
        }

        public GlobalData(string name)
            : this()
        {
            this.TableName = name;
        }

        /// <summary>
        /// 取得单实例模式的本类唯一实例
        /// </summary>
        /// <returns></returns>
        //public static GlobalData GetInstance()
        //{
        //    if (instanceData == null)
        //    {
        //        //保证一个线程进入
        //        lock (_object)
        //        {
        //            instanceData = new GlobalData();
        //        }
        //    }

        //    return instanceData;
        //}
    }
}
