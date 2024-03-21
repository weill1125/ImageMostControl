using System.Collections.Generic;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate
{
    /// <summary>
    /// 最终业务XML生成接口
    /// 描述：根据内存中的数据结构生成交互的业务XML文档
    /// </summary>
    interface IXmlDataExport
    {
        ///// <summary>
        ///// 获取业务交互XML文档
        ///// </summary>
        ///// <param name="version">版本</param>
        ///// <param name="encoding">编码格式</param>
        ///// <param name="description">描述与注释</param>
        ///// <returns></returns>
        //string CreateIndexXml(string version, string encoding, string description, bool delIgnore);

        ///// <summary>
        ///// 获取业务交互XML文档
        ///// </summary>
        ///// <param name="xmlHead">xml头部信息</param>
        ///// <returns></returns>
        //string CreateIndexXml(string xmlHead, bool delIgnore);

        ///// <summary>
        ///// 获取业务交互XML文档(默认XML版本"1.0",编码"UTF-8")
        ///// </summary>
        ///// <returns></returns>
        //string CreateIndexXml(bool delIgnore);

        /// <summary>
        /// 获取index.xml的头
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="description">描述与注释</param>
        /// <returns></returns>
        string CreateIndexXmlHead(string version, string encoding, string description);

        /// <summary>
        /// 获取index.xml的头(默认"1.0", "UTF-8")
        /// </summary>
        /// <returns></returns>
        string CreateIndexXmlHead();

        ///// <summary>
        ///// 按交易批量生成index xml
        ///// </summary>
        ///// <param name="version">版本</param>
        ///// <param name="encoding">编码格式</param>
        ///// <param name="description">描述与注释</param>
        ///// <returns>交易码，该交易对应的index xml</returns>
        //Dictionary<string, string> BatchCreateIndexXml(string version, string encoding, string description, bool delIgnore);

        /// <summary>
        /// 按交易批量生成index xml
        /// </summary>
        /// <returns>交易码，该交易对应的index xml</returns>
        Dictionary<string, string> BatchCreateIndexXml(bool delIgnore);

        ///// <summary>
        ///// 获取字段值为空的节点名称
        ///// </summary>
        ///// <returns></returns>
        //Dictionary<int, string> GetErrorControlInfo();

        /// <summary>
        /// 判断元数据是否符合上传规则
        /// </summary>
        /// <param name="dic">pkuuid,模板名键值对</param>
        /// <returns>成功返回1,否则pkuuid或者返回错误消息</returns>
        Dictionary<string, List<string>> CheckAllMetadata(Dictionary<int, Dictionary<string, string>> dic);

        ///// <summary>
        ///// 获取数据结构的XML
        ///// </summary>
        ///// <returns></returns>
        //string GetDataXml();
    }
}
