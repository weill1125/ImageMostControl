
namespace Com.Boc.Icms.MetadataEdit.Support.Template
{
    /// <summary>
    /// XML控件模板缓存类接口
    /// </summary>
    public interface IXmlTemplateCache
    {
        ///// <summary>
        ///// 填充控件的值
        ///// </summary>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="nodeId">控件节点在缓存中对应的父节点编号</param>
        ///// <param name="dataManage">数据管理类</param>
        //string FillTemplate(string xml, int nodeId, DataManage dataManage);

        /// <summary>
        /// 查找XML业务控件模板中对应的节点模板
        /// </summary>
        /// <param name="name">模板名</param>
        /// <returns></returns>
        string FindTemplate(string name);

        /// <summary>
        /// 加载XML控件模板
        /// </summary>
        /// <param name="xml">xml字符串</param>
        bool LoadXml(string xml);
    }
}
