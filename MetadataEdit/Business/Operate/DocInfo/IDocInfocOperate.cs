using System;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo
{
    /// <summary>
    /// 文档操作类接口
    /// </summary>
    interface IDocInfocOperate
    {
        /// <summary>
        /// 初始化加载文档类型docs_info的XML
        /// 
        /// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        /// <param name="type">业务工作类型</param>
        void InitXml(string xml, EnumType.EnumWorkType type);
        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="data_type">数据类型</param>
        void AddDoc(string data_type);
        /// <summary>
        /// 切换节点对应的XML控件模板
        /// 
        /// 描述：根据指定的模板名进行模板检索，并生成控件。根据指定的文档索引查找控件值，并填充控件。
        /// </summary>
        /// <param name="doc_index">文档索引(如何小于0的数都代表Business_info根节点)</param>
        /// <param name="templateName">模板名</param>
        void ChangeXmlNode(int doc_index, string templateName);
        /// <summary>
        /// 删除单个文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        void DeleteDoc(int doc_index);
        /// <summary>
        /// 移动文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        void MoveDoc(int doc_index, int old_doc_index);
        /// <summary>
        /// 根据指定的文档索引检索字段名为field的值
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        string GetValueByField(int doc_index, string field);
        /// <summary>
        /// 根据指定的数据类型检索文档的PKUUID列表
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        string[] GetGuidByType(string type);
        ///<summary>
        /// 在验证控件文本时发生
        /// </summary>
        event ShowMessage TextValidated;
        /// <summary>
        /// 在验证控件文本组成的XML时发生
        /// </summary>
        event ValidateXml XmlValidated;
    }
}
