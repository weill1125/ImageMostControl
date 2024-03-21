using System;
using System.Data;
using System.Linq;
using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;
using Com.Boc.Icms.LogDLL;

namespace Com.Boc.Icms.MetadataEdit.Support.ProviderEffect
{
    /// <summary>
    /// 数据管理类
    /// 描述：
    ///      将业务接口的XML文档用数据缓存表来表达（个人不建议，建议直接在缓存中操作XML）
    ///      UniqueID:节点唯一标识，对应于业务XML规则来说，目前都是第一个子节点（批次XML、文档XML）。
    ///      ParentID:父节点
    ///      Child:子节点
    ///      Value:值（控件类型为用户输入值，节点类型为节点的自身XML）
    ///      Type:0节点，1控件
    ///      Index:节点在父节点下面的索引位置
    /// </summary>
    public class DataManage
    {
        private readonly GlobalData _gData = new GlobalData();
        internal GlobalData GData
        {
            get { return this._gData; }
        }

        private readonly string _proIdAndThreadId = string.Empty;

        public DataManage()
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            //this._gData = GlobalData.DataSturct;
            if (this._gData == null)
            {
                this._gData = new GlobalData();
            }

            //判断是否已经构建缓存数据结构
            if (this._gData.Columns.Count > 1) return;

            DataColumn parentIdColumn = new DataColumn("ParentID");
            parentIdColumn.AllowDBNull = false;
            parentIdColumn.DataType = typeof(int);
            parentIdColumn.DefaultValue = 0;

            DataColumn typeColumn = new DataColumn("Type");
            typeColumn.AllowDBNull = false;
            typeColumn.DataType = typeof(int);
            typeColumn.DefaultValue = 0;

            DataColumn indexColumn = new DataColumn("Index");
            indexColumn.AllowDBNull = false;
            indexColumn.DataType = typeof(int);
            indexColumn.DefaultValue = 0;

            this._gData.Columns.AddRange(new[] {
                parentIdColumn,
                new DataColumn("Child"),
                new DataColumn("Value"),
                typeColumn,
                indexColumn});
        }

        /// <summary>
        /// 添加行
        /// </summary>
        /// <param name="parentID">父节点编号</param>
        /// <param name="child">子节点</param>
        /// <param name="value">子节点值</param>
        /// <param name="type">节点类型</param>
        /// <returns>返回行自增编号</returns>
        public int AddRow(int nodeId, string child, string value, SupportEnumType.EnumNodeType type, int index)
        {
            DataRow dr = this._gData.NewRow();
            dr.ItemArray = new[] { dr[0], nodeId, child, value, (int)type, index };
            try
            {
                this._gData.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (int)dr[0];
        }

        /// <summary>
        /// 检测节点
        /// 检测节点是否存在 不存在则添加
        /// </summary>
        /// <param name="child">子节点</param>
        /// <param name="parentID">父节点编号</param>
        /// <param name="value">子节点值</param>
        /// <param name="type">节点类型</param>
        public int CheckChild(int nodeId, string child, string value, SupportEnumType.EnumNodeType type, int index)
        {
            DataRow[] rowArray = this.SelectNodeRows(nodeId, child, type);
            if (rowArray.Count() <= 0)
            {
                //2016-03-31 dingyao 添加节点version_list的index跟pages都是0，所以对version_list节点做特例，不再添加。
                //以后有时间会将检查节点是否存在跟添加节点的功能分成两个函数
                if (child == "version_list")
                {
                    return -1;
                }
                return this.AddRow(nodeId, child, value, type, index);
            }
            return (int)rowArray[0]["ID"];
        }

        /// <summary>
        /// 修改节点值
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="child">子节点名称</param>
        /// <param name="value">值</param>
        /// <param name="type">节点类型</param>
        public void UpdateValue(int nodeId, string child, string value, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rowArray = this.SelectNodeRows(nodeId, child, type);
            if (rowArray.Count() <= 0) return;

            //存在相同的行 执行修改
            rowArray[0].BeginEdit();
            rowArray[0]["Value"] = value;
            try
            {
                rowArray[0].EndEdit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 修改节点值
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <param name="value">值</param>
        public void UpdateValue(int nodeId, string value)
        {
            this.UpdateCellValue(nodeId, "Value", value);
        }

        /// <summary>
        /// 修改在指定节点编号的节点上面指定字段名的值
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <param name="column">列名</param>
        /// <param name="value">值</param>
        public void UpdateCellValue(int nodeId, string column, object value)
        {
            DataRow[] rowArray = this.Select("[ID] = " + nodeId);
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

        /// <summary>
        /// 修改节点名
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <param name="value">新节点名</param>
        public void UpdateChild(int nodeId, string value)
        {
            this.UpdateCellValue(nodeId, "Child", value);
        }

        /// <summary>
        /// 修改节点所属父节点
        /// </summary>
        /// <param name="child">子节点</param>
        /// <param name="srcParentId">源父节点</param>
        /// <param name="descParentId">目标父节点</param>
        /// <param name="type">节点类型</param>
        public void UpdateChild(string child, int srcParentId, int descParentId, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rowArray = this.SelectNodeRows(srcParentId, child, type);
            if (rowArray.Count() <= 0) return;

            rowArray[0].BeginEdit();
            rowArray[0]["ParentID"] = descParentId;
            try
            {
                rowArray[0].EndEdit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据节点索引位置检索编号(index代表索引字段值)
        /// </summary>
        /// <param name="parentID">父节点编号</param>
        /// <param name="index">索引位置</param>
        /// <param name="type">节点类型</param>
        /// <returns>节点编号</returns>
        public int GetIDByIndex(int nodeId, int index, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rowArray = this.Select("[ParentID] = " +
                nodeId +
                " and [Index] = " +
                index +
                " and [Type] = " +
                type);
            return rowArray.Length > 0 ? (int)rowArray[0][0] : 0;
        }

        /// <summary>
        /// 根据节点索引检索编号(index代表索引字段值)
        /// </summary>
        /// <param name="parentID">父节点编号</param>
        /// <param name="index">索引位置</param>
        /// <returns>节点编号</returns>
        public int GetIDByIndex(int nodeId, int index)
        {
            DataRow[] rowArray = this.Select("[ParentID] = " +
                nodeId +
                " and [Index] = " +
                index);
            return rowArray.Length > 0 ? (int)rowArray[0][0] : 0;
        }

        /// <summary>
        /// 查询节点值
        /// </summary>
        /// <param name="parentID">父节点编号</param>
        /// <param name="child">节点名</param>
        /// <param name="type">节点类型</param>
        /// <returns>节点值</returns>
        public string SelectNodeValue(int nodeId, string child, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rowArray = this.SelectNodeRows(nodeId, child, type);
            if (rowArray.Count() > 0)
                return rowArray[0]["Value"].ToString();
            return string.Empty;
        }

        /// <summary>
        /// 查询节点值
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <returns></returns>
        public string SelectNodeValue(int nodeId)
        {
            DataRow dr = this.SelectNodeRow(nodeId);
            if (dr != null)
                return dr["Value"].ToString();
            return string.Empty;
        }

        /// <summary>
        /// 查询与筛选条件相匹配的所有 DataRow 对象的数组
        /// </summary>
        /// <param name="filterExpression">筛选条件</param>
        /// <returns>DataRow对象数组</returns>
        public DataRow[] Select(string filterExpression)
        {
            try
            {
                return this._gData.Select(filterExpression);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 查询节点
        /// </summary>
        /// <param name="parentId">父节点编号</param>
        /// <param name="child">节点名</param>
        /// <param name="type">节点类型</param>
        /// <returns></returns>
        public DataRow[] SelectNodeRows(int parentId, string child, SupportEnumType.EnumNodeType type)
        {
            return this.Select("[ParentID] = " + parentId +
                " and [Child] = '" + child +
                "' and [Type] = " + (int)type);
        }

        /// <summary>
        /// 根据指定的节点编号查询节点
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <returns></returns>
        public DataRow SelectNodeRow(int nodeId)
        {
            DataRow[] rowArray = this.Select("[ID] = " + nodeId);
            return rowArray.Length > 0 ? rowArray[0] : null;
        }

        /// <summary>
        /// 删除父节点及其后代子节点
        /// </summary>
        /// <param name="parentID">父节点编号</param>
        public void DeleteByParentId(int nodeId)
        {
            this.ClearChilds(nodeId);
            this.Delete(nodeId);
        }

        /// <summary>
        /// 清空父节点下面的所有后代子节点
        /// 注:父节点下面的所有子节点将删除，但父节点保留
        /// </summary>
        /// <param name="parentID"></param>
        public void ClearChilds(int nodeId)
        {
            DataRow[] rowArray = this.Select("[ParentID] = " + nodeId);

            foreach (var row in rowArray)
            {
                this.ClearChilds((int)row[0]);

                try
                {
                    row.Delete();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <returns></returns>
        public void Delete(int nodeId)
        {
            DataRow[] rowArray = this.Select("[ID] = " + nodeId);
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

        /// <summary>
        /// 根据指定的父节点编号重新编号子节点(重新从0开始编号)
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        public void OrderBy(int nodeId)
        {
            DataRow[] rowArray = this.Select("[parentID] = " + nodeId);
            OrderByRows(rowArray, "Index", this._proIdAndThreadId);
        }

        /// <summary>
        /// 根据指定的父节点编号与节点类型重新编号子节点(重新从0开始编号)
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="type">节点类型</param>
        public void OrderBy(int nodeId, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rowArray = this.GetRowsByType(nodeId, type);
            OrderByRows(rowArray, "Index", this._proIdAndThreadId);
        }

        /// <summary>
        /// 根据指定的父节点编号在index处占位子节点
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="index">占位索引</param>
        public void SeizeByIndex(int nodeId, int index)
        {
            DataRow[] rowArray = this.Select("[parentID] = " + nodeId);
            SeizeRowsByIndex(rowArray, "Index", index, this._proIdAndThreadId);
        }

        /// <summary>
        /// 根据指定的父节点编号与节点类型在index处占位子节点
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="type">节点类型</param>
        /// <param name="index">占位索引</param>
        public void SeizeByIndex(int nodeId, SupportEnumType.EnumNodeType type, int index)
        {
            DataRow[] rowArray = this.GetRowsByType(nodeId, type);
            SeizeRowsByIndex(rowArray, "Index", index, this._proIdAndThreadId);
        }

        /// <summary>
        /// 根据指定的父节点编号在index处复位子节点
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="index">复位索引</param>
        public void ReseizeByIndex(int nodeId, int index)
        {
            DataRow[] rowArray = this.Select("[parentID] = " + nodeId);
            ReseizeRowsByIndex(rowArray, "Index", index, this._proIdAndThreadId);
        }

        /// <summary>
        /// 根据指定的父节点编号与节点类型在index处复位子节点
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="type">节点类型</param>
        /// <param name="index">复位索引</param>
        public void ReseizeByIndex(int nodeId, SupportEnumType.EnumNodeType type, int index)
        {
            DataRow[] rowArray = this.GetRowsByType(nodeId, type);
            ReseizeRowsByIndex(rowArray, "Index", index, this._proIdAndThreadId);
        }

        /// <summary>
        /// 根据指定的父节点编号检索类型为type的子节点编号
        /// 注：如果子节点有多个，将返回第一个节点的编号
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="type">节点类型</param>
        /// <returns>节点编号</returns>
        public int GetSpecialIdByType(int nodeId, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rowArray = this.GetRowsByType(nodeId, type);
            return rowArray.Length > 0 ? (int)rowArray[0][0] : 0;
        }

        /// <summary>
        /// 根据指定的父节点编号检索类型为EnumType.EnumNodeType的子节点
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="type">节点类型</param>
        /// <returns>行数组</returns>
        public DataRow[] GetRowsByType(int nodeId, SupportEnumType.EnumNodeType type)
        {
            return this.Select("[ParentID] = " +
                nodeId +
                " and [Type] = " +
                (int)type);
        }

        /// <summary>
        /// 根据指定的子节点的名称找到相关的行的所有值
        /// </summary>
        /// <param name="child">子节点的值</param>
        /// <returns></returns>
        public DataRow[] GetRowsbyChild(string child)
        {
            DataRow[] drs = this.Select("[Child] = " + "'" + child + "'");
            return drs;
        }

        /// <summary>
        /// 根据指定的父节点编号、子节点类型检索最大的子节点索引值
        /// </summary>
        /// <param name="nodeId">父节点编号</param>
        /// <param name="type">子节点类型</param>
        /// <returns>子节点最大索引值</returns>
        public int GetMaxIndex(int nodeId, SupportEnumType.EnumNodeType type)
        {
            DataRow[] rows = this.GetRowsByType(nodeId, type);
            if (rows.Length <= 0) return -1;

            rows = rows.OrderByDescending(a => int.Parse(a["Index"].ToString())).ToArray();
            return (int)rows[0]["Index"];
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            try
            {
                //GlobalData.DataSturct = null;
                if (this._gData != null)
                {
                    this._gData.Clear();
                    //this._gData = null;
                }
            }
            catch (Exception ex)
            {
                SysLog.Write(7145, ex, this._proIdAndThreadId);
                throw;
            }
        }

        /// <summary>
        /// 数据提交
        /// </summary>
        public void Commit()
        {
            try
            {
                this._gData.AcceptChanges();
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
                this._gData.RejectChanges();
            }
            catch (Exception ex)
            {
                SysLog.Write(7149, ex, this._proIdAndThreadId);
                throw;
            }
        }

        /// <summary>
        /// 根据指定的节点编号检索是否存在任意子节点
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        public bool HasChilds(int nodeId)
        {
            DataRow[] rows = this.Select("[ParentID] = " + nodeId);
            return rows.Length > 0 ? true : false;
        }

        /// <summary>
        /// 根据指定的节点编号检索是否存在指定子节点类型的节点
        /// </summary>
        /// <param name="nodeId">节点编号</param>
        /// <param name="type">子节点类型</param>
        public bool HasChilds(int nodeId, SupportEnumType.EnumNodeType type)
        {
            return this.GetRowsByType(nodeId, type).Length > 0 ? true : false;
        }

        /// <summary>
        /// 根据指定的节点编号检索是否存在指定子节点类型与子节点名的节点
        /// </summary>
        /// <param name="parentID">节点编号</param>
        /// <param name="child">子节点名</param>
        /// <param name="type">子节点类型</param>
        /// <returns></returns>
        public bool HasChilds(int nodeId, string child, SupportEnumType.EnumNodeType type)
        {
            return this.SelectNodeRows(nodeId, child, type).Length > 0 ? true : false;
        }

        /// <summary>
        /// 获取数据结构XML
        /// </summary>
        /// <returns></returns>
        public string GetDataXml()
        {
            try
            {
                System.IO.StringWriter writer = new System.IO.StringWriter();
                this._gData.WriteXml(writer, false);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region 静态方法
        /// <summary>
        /// 将DataRow数组中指定列名的列重新编号(重新从0开始编号)
        /// </summary>
        /// <param name="rows">DataRow数组</param>
        /// <param name="columnName">列名(索引列)</param>
        public static void OrderByRows(DataRow[] rows, string columnName, string strLogId)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i][columnName].Equals(i)) continue;

                rows[i].BeginEdit();
                rows[i][columnName] = i;
                try
                {
                    rows[i].EndEdit();
                }
                catch (Exception ex)
                {
                    SysLog.Write(7136, ex, strLogId);
                    throw;
                }
            }
        }

        /// <summary>
        /// 将DataRow数组中指定列名的列用index占位(所有大于等于index的记录都后移一位)
        /// </summary>
        /// <param name="rows">DataRow数组</param>
        /// <param name="columnName">列名(索引列)</param>
        /// <param name="index">占位索引</param>
        public static void SeizeRowsByIndex(DataRow[] rows, string columnName, int index,string strLogId)
        {
            SerialRowsByIndex(rows, columnName, index, true, strLogId);
        }

        /// <summary>
        /// 将DataRow数组中指定列名的列在index复位(所有大于等于index的记录都前移一位)
        /// </summary>
        /// <param name="rows">DataRow数组</param>
        /// <param name="columnName">列名(索引列)</param>
        /// <param name="index">复位索引</param>
        public static void ReseizeRowsByIndex(DataRow[] rows, string columnName, int index, string strLogId)
        {
            SerialRowsByIndex(rows, columnName, index, false, strLogId);
        }

        #region 静态私有方法
        /// <summary>
        /// 将DataRow数组中指定列名的列在index执行占复位操作
        /// </summary>
        /// <param name="rows">DataRow数组</param>
        /// <param name="columnName">列名(索引列)</param>
        /// <param name="index">占复位索引</param>
        /// <param name="isSeize">是否占位</param>
        private static void SerialRowsByIndex(DataRow[] rows, string columnName, int index, bool isSeize,string strLogId)
        {
            int currentIndex = 0;

            //排序
            int value = 0;
            rows = rows.OrderBy(a => int.TryParse(a[columnName].ToString(), out value) ? value : a[columnName]).ToArray();

            //占复位
            foreach (var row in rows)
            {
                currentIndex = (int)row[columnName];

                row.BeginEdit();
                if (currentIndex >= index)
                    row[columnName] = isSeize ? ++currentIndex : --currentIndex;
                try
                {
                    row.EndEdit();
                }
                catch (Exception ex)
                {
                    SysLog.Write(7137, ex, strLogId);
                    throw;
                }
            }
        }
        #endregion
        #endregion
    }
}
