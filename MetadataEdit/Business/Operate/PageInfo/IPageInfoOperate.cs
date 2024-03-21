using System;
using System.Collections.Generic;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.PageInfo
{
    /// <summary>
    /// 图像操作类接口
    /// </summary>
    interface IPageInfoOperate
    {
        /// <summary>
        /// 初始化pages_info的XML
        /// 
        /// 描述：图像添加到文档节点之后，生成图像XML，此处进行数据缓存。
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="xml">xml字符串</param>
        void InitXml(int doc_index, string xml);
        /// <summary>
        /// 添加文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="docXml">xml字符串</param>
        void AddDoc(int doc_index, string docXml);
        /// <summary>
        /// 添加图像批注文件
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="remark">批注信息</param>
        void AddImagePostil(int doc_index, int page_index, string remark);
        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="pageXml">xml字符串</param>
        void AddImageXml(int doc_index, int page_index, string pageXml);
        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="file_name">图像文件名</param>
        void AddImageXml(int doc_index, string file_name);
        /// <summary>
        /// 增加图像节点集合
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="fileNames">图像文件名</param>
        void AddImageXml(int doc_index, string[] fileNames);
        /// <summary>
        /// 删除批注
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        void DeleteImagePostil(int doc_index, int page_index);
        /// <summary>
        /// 删除图像节点
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        void DeleteImageXml(int doc_index, int page_index);
        /// <summary>
        /// 取得图像排序的索引键值对 
        /// 
        /// 描述：补扫、重扫，取得预加载的排序图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <returns>已排序好的(索引,文件名)键值对</returns>
        Dictionary<int, string> GetImageOrder(int doc_index);
        /// <summary>
        /// 获取所有改变的文件名
        /// </summary>
        /// <returns>改变的文件名列表</returns>
        List<string> GetChangedFileNames();

        /// <summary>
        /// 获取Control类型的子节点信息
        /// </summary>
        /// <param name="docIndex">文件类型编号</param>
        /// <param name="nodeNames">子节点名称</param>
        /// <returns>子节点名称跟子节点值的组合</returns>
        Dictionary<string, string> GetChildNodeValue(int docIndex, List<string> nodeNames);
        
        /// <summary>
        /// 重扫补扫时获取整个业务图像信息<!--<doc index, <page index, file name>>-->
        /// </summary>
        /// <returns>返回结构(文档索引, (图像索引, 图像名)) </returns>
        Dictionary<int, Dictionary<int, string>> GetImageOrder();
        /// <summary>
        /// 移动图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        /// <param name="old_page_index">初始图像索引</param>
        void MoveImage(int doc_index, int page_index, int old_doc_index, int old_page_index);
        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        /// <param name="old_page_index">初始图像索引</param>
        void CopyImage(int doc_index, int old_doc_index, int old_page_index);
        /// <summary>
        /// 替换图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="file_name">图想文件名</param>
        void ReplaceFileName(int doc_index, int page_index, string file_name);
        /// <summary>
        /// 修改图像正反面
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="page_flag">当前正反面</param>
        void UpdateImageFlag(int doc_index, int page_index, string page_flag);
        /// <summary>
        /// 修改图像批注信息
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="remark">批注信息</param>
        void UpdateImagePostil(int doc_index, int page_index, string remark);
    }
}
