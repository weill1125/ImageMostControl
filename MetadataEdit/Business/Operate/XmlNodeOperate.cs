using Com.Boc.Icms.MetadataEdit.Base.Xml;
using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;
using Com.Boc.Icms.MetadataEdit.Services;


namespace Com.Boc.Icms.MetadataEdit.Business.Operate
{
    /// <summary>
    /// XML节点在数据缓存中操作的基类
    /// </summary>
    public class XmlNodeOperate : XmlOperate
    {
        protected DataServies dataServies;


        public XmlNodeOperate(DataServies dataServies)
            : base()
        {
            this.dataServies = dataServies;
        }


        /// <summary>
        /// 修改节点的属性值(同时修改节点中的modi_time时间值)
        /// </summary>
        /// <param name="xml">xml值</param>
        /// <param name="attribute">属性名</param>
        /// <param name="value">值</param>
        public virtual string UpdateNodePropertyAndTime(string xml, string attribute, string value)
        {
            string result = "";
            result = UpdateNodeProperty(xml, attribute, value);
            result = UpdateNodeProperty(result, "modi_time", this.Now);
            return result;
        }

        ///// <summary>
        ///// 添加单个节点到父节点的最后一个索引位置
        ///// </summary>
        ///// <param name="parentId">父节点编号</param>
        ///// <param name="child">节点名</param>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="type">节点类型</param>
        //public virtual int Add(int parentId, string child, string xml, SupportEnumType.EnumNodeType type)
        //{
        //    int index = this.DataManage.GetMaxIndex(parentId, type) + 1;

        //    return this.Add(parentId, child, xml, type, index);
        //}

        ///// <summary>
        ///// 添加单个节点到父节点的指定索引位置
        ///// </summary>
        ///// <param name="parentId">父节点编号</param>
        ///// <param name="child">节点名</param>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="type">节点类型</param>
        ///// <param name="index">索引位置</param>
        //public virtual int Add(int parentId, string child, string xml, SupportEnumType.EnumNodeType type, int index)
        //{
        //    this.DataManage.SeizeByIndex(parentId, index);
        //    return this.DataManage.AddRow(parentId, child, xml, type, index);
        //}

        ///// <summary>
        ///// 移动节点位置
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        ///// <param name="parentId">目标父节点编号</param>
        ///// <param name="index">目标子节点索引</param>
        //public virtual void MoveNode(int nodeId, int parentId, int index)
        //{
        //    //为保证效率，或者移动的节点包含有迭代子节点的情况
        //    //将不执行删除并添加的操作，而直接进行父节点、索引位置的更新
        //    DataRow dr = this.DataManage.SelectNodeRow(nodeId);
        //    int oldParentId = (int)dr["parentID"];
        //    int srcIndex = (int)dr["Index"];

        //    //节点在目标位置占位
        //    this.DataManage.SeizeByIndex(parentId, index);

        //    //修改目标节点Index索引列值
        //    dr.BeginEdit();
        //    dr["ParentID"] = parentId;
        //    dr["Index"] = index;
        //    try
        //    {
        //        dr.EndEdit();
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(9131,ex,System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
        //        throw ex;
        //    }

        //    //判断节点是否在当前父节点下面移动
        //    if (oldParentId == parentId)
        //    {
        //        this.UpdateNodePropertyAndTime(nodeId, "oper_type", "E");
        //    }
        //    else
        //    {
        //        //重新排序初始父节点下面的子节点
        //        this.DataManage.ReseizeByIndex(oldParentId, srcIndex);

        //        this.UpdateNodePropertyAndTime(nodeId, "oper_type", "A");
        //    }
        //}

        ///// <summary>
        ///// 删除节点及其后代节点，并重新排序同级别的其他节点
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        //public virtual void Delete(int nodeId)
        //{
        //    DataRow dr = this.DataManage.SelectNodeRow(nodeId);
        //    int parentId = (int)dr["ParentID"];
        //    int index = (int)dr["Index"];

        //    this.DataManage.DeleteByParentId(nodeId);
        //    this.DataManage.ReseizeByIndex(parentId, index);
        //}

        ///// <summary>
        ///// 修改节点名
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        ///// <param name="value">节点值</param>
        //public virtual void UpdateNodeName(int nodeId, string value)
        //{
        //    this.DataManage.UpdateChild(nodeId, value);

        //    this.UpdateNodePropertyAndTime(nodeId, "oper_type", "E");
        //}

        ///// <summary>
        ///// 修改节点的属性值(同时修改节点中的modi_time时间值)
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        ///// <param name="attribute">属性名</param>
        ///// <param name="value">值</param>
        //public virtual void UpdateNodePropertyAndTime(int nodeId, string attribute, string value)
        //{
        //    this.UpdateNodeProperty(nodeId, attribute, value);
        //    this.UpdateNodeProperty(nodeId, "modi_time", this.Now);
        //}

        ///// <summary>
        ///// 修改节点的属性值
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        ///// <param name="attribute">属性名</param>
        ///// <param name="value">值</param>
        //public virtual void UpdateNodeProperty(int nodeId, string attribute, string value)
        //{
        //    string xml = this.DataManage.SelectNodeValue(nodeId);
        //    value = XmlOperate.UpdateNodeProperty(xml, attribute, value);
        //    this.DataManage.UpdateValue(nodeId, value);
        //}

        ///// <summary>
        ///// 查询节点的属性值
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        ///// <param name="attribute">属性名</param>
        ///// <returns>值</returns>
        //public virtual string GetNodeProperty(int nodeId, string attribute)
        //{
        //    string value = this.DataManage.SelectNodeValue(nodeId);
        //    return GetNodePropertyValue(value, attribute);
        //}

        ///// <summary>
        ///// 根据指定的父节点检索包含有多个子节点的子节点编号
        ///// 描述：根据业务XML的情况，
        ///// buessiness_info包含docs_info，docs_info包含doc_info，doc_info包含有pages，pages包含有page
        ///// 需要取得docs_info节点，pages节点在进行文档和图像的节点查找
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        //public virtual int GetSpecialNode(int nodeId)
        //{
        //    return this.DataManage.GetSpecialIdByType(nodeId, SupportEnumType.EnumNodeType.Node);
        //}

        ///// <summary>
        ///// 同步指定父节点的后代子节点的指定属性的值
        ///// </summary>
        ///// <param name="nodeId">父节点编号</param>
        ///// <param name="property">属性名</param>
        ///// <param name="value">值</param>
        //public virtual void SynNodeProperty(int nodeId, string property, string value)
        //{
        //    DataRow[] rows = this.DataManage.GetRowsByType(nodeId, SupportEnumType.EnumNodeType.Node);

        //    rows.ToList().ForEach(a =>
        //    {
        //        nodeId = (int)a["ID"];
        //        this.UpdateNodePropertyAndTime(nodeId, property, value);

        //        this.SynNodeProperty(nodeId, property, value);
        //    });
        //}

        ///// <summary>
        ///// 判断节点是否存在，不存在则进行添加
        ///// 注：添加的值为<!--<name></name>-->
        ///// </summary>
        ///// <param name="nodeId">父节点编号</param>
        ///// <param name="name">节点限定名</param>
        //public virtual int CheckNode(int nodeId, string name)
        //{
        //    return this.DataManage.CheckChild(nodeId,
        //          name,
        //          "<" + name + "></" + name + ">",
        //          SupportEnumType.EnumNodeType.Node, 0);
        //}
    }
}
