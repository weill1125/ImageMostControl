using Com.Boc.Icms.MetadataEdit;
using System.Collections.Generic;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo
{
    /// <summary>
    /// 文档操作类接口
    /// </summary>
    interface INewDocInfocOperate
    {
        ///// <summary>
        ///// 初始化加载文档类型docs_info的XML
        ///// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        ///// </summary>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="isAddUniqMetadata">是否自动添加uniq_metadata节点</param>
        ///// <param name="isAddGuid">是否自动添加uniq_metadata节点值guid</param>
        ///// <returns>pkuuid列表</returns>
        //string[] InitXml(string xml, bool isAddUniqMetadata, bool isAddGuid, int index);

        ///// <summary>
        ///// 初始化加载文档类型docs_info的XML
        ///// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        ///// </summary>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="isAddUniqMetadata">是否自动添加uniq_metadata节点</param>
        ///// <param name="isAddGuid">是否自动添加uniq_metadata节点值guid</param>
        ///// <param name="pkuuidList">pkuuid列表</param>
        ///// <returns>是否操作成功</returns>
        //bool InitXml(string xml, string businessId);
        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="xml">文档XML</param>
        string AddDoc(string xml, string businessId);

        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="xml">文档XML</param>
        bool AddDocs(string xml, string businessId);

        /// <summary>
        /// 切换节点对应的XML控件模板
        /// 描述：根据指定的模板名进行模板检索，并生成控件。根据指定的文档索引查找控件值，并填充控件。
        /// </summary>
        /// <param name="nodeType">文档种类</param>
        /// <param name="key">文档标识</param>
        /// <param name="templateName">模板名</param>
        void ChangeXmlNode(BusinessData.EnumType.TableType tableType, string key, string templateName);

        /// <summary>
        /// 删除单个文档
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        bool DeleteDoc(string pkuuid);

        /// <summary>
        /// 删除交易
        /// </summary>
        /// <param name="businessId">交易编号</param>
        /// <returns>删除结果</returns>
        bool DeleteBusinessInfo(string businessId);

        ///// <summary>
        ///// 删除当前交易
        ///// </summary>
        ///// <param name="businessIndex">交易节点序号</param>
        ///// <returns>删除结果</returns>
        //bool DeleteCurBusinessInfo();

        /// <summary>
        /// 根据指定的文档pkuuid检索字段名为field的值
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        string GetValueByField(string pkuuid, string field);

        ///// <summary>
        ///// 根据指定的数据类型检索文档的PKUUID列表
        ///// </summary>
        ///// <param name="type">数据类型</param>
        ///// <returns></returns>
        //string[] GetGuidByType(string type);

        ///// <summary>
        ///// 根据指定的PKUID检索属性值
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="attribute">属性名</param>
        //string GetDocAttribute(string pkuuid, string attribute);

        /// <summary>
        /// 检索批次的字段名为field的值
        /// </summary>
        /// <param name="field">字段名</param>
        string GetValueByBusinessInfoField(string bussnessId, string field);

        /// <summary>
        /// 设置businessinfo的节点值
        /// </summary>
        /// <param name="busInfo">要设置的值<节点名字，节点值></param>
        /// <returns>是否修改成功</returns>
        bool SetValueByBusinessInfoField(string bussnessId, Dictionary<string, string> busInfo);

        /*/// <summary>
        /// 检索控件名为field的模板控件标签
        /// </summary>
        /// <param name="field">控件名</param>
        string GetTemplateControlCaption(string field);*/

        /// <summary>
        /// 更新元数据 IndexMetadata 属性
        /// 描述：只是对Index_Metadata属性进行更新
        /// </summary>
        /// <param name="pkuuid">文档的唯一标识</param>
        /// <returns></returns>
        void UpdateDocIndexMetadata(string pkuuid, Dictionary<string, string> dicIndexMetadata);

        /// <summary>
        /// 根据datatype或者对应的pkuuid
        /// </summary>
        /// <param name="dataType">datatype</param>
        /// <returns>pkuuid</returns>
        string GetPkuuidByDatatype(string dataType);

        /// <summary>
        /// 在验证控件文本时发生
        /// </summary>
        event ShowMessage TextValidated;

        /// <summary>
        /// 在验证控件文本组成的XML时发生
        /// </summary>
        event ValidateXml XmlValidated;

        /// <summary>
        /// 在更改交易编号时发生
        /// </summary>
        event UpdateDirectory BizMetadata1Changed;

        /// <summary>
        /// 改变TreeNode前景色
        /// </summary>
        event UpdateDirectory ChangeTreeNodeForeColor;
    }
}
