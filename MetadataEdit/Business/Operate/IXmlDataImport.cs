using Com.Boc.Icms.MetadataEdit.Models;
using System.Xml;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate
{
    /// <summary>
    /// 交互XML文件的处理类接口
    /// </summary>
    interface IXmlDataImport
    {
        /// <summary>
        /// XML根节点
        /// </summary>
        XmlElement Root { get; }

        ///// <summary>
        ///// 父级节点在数据缓存中的自增编号
        ///// </summary>
        //int ParentID { get; set; }

        /// <summary>
        /// 保存个业务的xml文档(补扫、重扫)
        /// </summary>
        /// <param name="xml">xml字符串</param>
        bool SaveAllXml(string xml);

        /// <summary>
        /// 保存业务的部分xml到数据缓存
        /// 描述：根据外部接口分开传递的xml文档(包括buessness_info、docs_info、Pages)
        /// </summary>
        /// <param name="xml">xml字符串</param>
        bool SaveXml(string xml);

        /// <summary>
        /// 保存业务的部分xml到数据缓存
        /// 描述：只缓存到xml对象，不将数据存入缓存
        /// </summary>
        /// <param name="xml">xml字符串</param>
        void LoadXml(string xml);

        /// <summary>
        /// 把xml中的doc内容添加到doc内存表中
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="businessId"></param>
        void AddDocs(string xml, string businessId);

        /// <summary>
        /// 添加单个doc
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="businessId"></param>
        /// <returns></returns>
        string AddDoc(string xml, string businessId);

        /// <summary>
        /// 添加单个page
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="pkuuid"></param>
        /// <returns></returns>
        Page AddOnePage(string xml, string pkuuid, string realName);

    }
}
