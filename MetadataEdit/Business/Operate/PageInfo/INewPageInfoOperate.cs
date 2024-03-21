using Com.Boc.Icms.MetadataEdit;
using Com.Boc.Icms.MetadataEdit.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.PageInfo
{
    /// <summary>
    /// 图像操作类接口
    /// </summary>
    interface INewPageInfoOperate
    {
        ///// <summary>
        ///// 初始化pages_info的XML
        ///// 描述：图像添加到文档节点之后，生成图像XML，此处进行数据缓存。
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="xml">xml字符串</param>
        //bool InitXml(string pkuuid, string xml);

        /// <summary>
        /// 添加图像批注文件
        /// </summary>
        /// <param name="realName">图像索引</param>
        /// <param name="postilName">批注文件名</param>
        /// <param name="remark">批注信息</param>
        bool AddImagePostil(string realName, string postilName, string remark);

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="pageXml">xml字符串</param>
        bool AddImageByXml(string pkuuid, string pageXml, string realName);

        /// <summary>
        /// 增加图像节点(默认全部正面)
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="file_name">图像文件名</param>
        bool AddImage(string pkuuid, string fileName, string realName);

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileName">图像文件名</param>
        /// <param name="pageFlag">图像正反面(True正-Flase反)</param>
        bool AddImage(string pkuuid, string fileName, bool pageFlag, string realName);

        /// <summary>
        /// 增加图像节点集合
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileNames">图像文件名</param>
        bool AddImage(string pkuuid, string[] fileNames, string[] realNames);

        /// <summary>
        /// 删除批注
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        bool DeleteImagePostil(string realname);

        /// <summary>
        /// 删除图像节点
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        bool DeleteImage(string realName);

        ///// <summary>
        ///// 取得图像排序的索引键值对 
        ///// 描述：补扫、重扫，取得预加载的排序图像
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <returns>已排序好的(索引,文件名)键值对</returns>
        //IDictionary<int, string> GetImageOrder(string pkuuid);

        /// <summary>
        /// 获取所有改变的文件名
        /// </summary>
        /// <returns>改变的文件名列表</returns>
        Collection<string> GetChangedFileNames();

        ///// <summary>
        ///// 获取Control类型的子节点信息
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="nodeNames">子节点名称</param>
        ///// <returns>子节点名称跟子节点值的组合</returns>
        //IDictionary<string, string> GetChildNodeValue(string pkuuid, string[] nodeNames);

        ///// <summary>
        ///// 重扫补扫时获取整个业务图像信息<!--<doc index, <page index, file name>>-->
        ///// </summary>
        ///// <returns>返回结构(文档索引, (图像索引, 图像名)) </returns>
        //IDictionary<string, Dictionary<int, string>> GetImageOrder();


        /// <summary>
        /// 重扫补扫时获取整个业务图像信息和版本信息<!--<doc index, <page index, file name>>-->
        /// </summary>
        /// <param name="docFileInfo">图像信息</param>
        /// <param name="hizFileInfo">版本信息</param> 
        void GetImageOrderAndHizVersion(ref IDictionary<string, List<string>> hizFileInfo);

        /// <summary>
        /// 移动图像
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="docIndex">目的节点的docIndex -1</param>
        /// <param name="dragRealName">拖拽文件的索引</param>
        string MoveImage(string pkuuid, int docIndex, string dragRealName,bool ifChangeRealName);

        /// <summary>
        /// page排序后修改pkuuid下文件的pageIndex
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="newPagesIndex">文件要设置的Index字典(文件名，pageIndex)</param>
        /// <returns></returns>
        bool UpdatePagesIndex(string pkuuid, Dictionary<string, int> newPagesIndex);

        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="pkuuid">要拷贝到的文档唯一标识</param>
        /// <param name="copyRealName">初始文档唯一标识</param>
        /// <param name="realName">拷贝后文件的唯一标识</param>
        bool CopyImage(string pkuuid, string copyRealName, ref string realName);

        /// <summary>
        /// 替换图像
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        /// <param name="fileName">图想文件名</param>
        bool ReplaceFileName(string realName, string fileName);

        /// <summary>
        /// 只替换图像，不修改元数据
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        /// <param name="fileName">图想文件名</param>
        bool ReplaceOnlyFileName(string realName, string fileName);

        /// <summary>
        /// 修改图像正反面
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        /// <param name="pageFlag">当前正反面</param>
        bool UpdateImageFlag(string realName, string pageFlag);

        /// <summary>
        /// 修改oper_type和modi_range
        /// </summary>
        /// <param name="realName">page唯一标识</param>
        /// <param name="opertype"></param>
        /// <param name="modirange"></param>
        /// <returns></returns>
        bool UpdateImageOperTypeAndModiRange(string realName, string opertype, string modirange);

        /// <summary>
        /// 修改图像批注信息
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="remark">批注信息</param>
        bool UpdateImagePostil(string realname, string remark);

        /// <summary>
        /// 获取page节点属性
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        string GetPageAttribute(string realname, string attributeName);

        ///// <summary>
        ///// 重扫补扫时获取整个业务图像信息<!--<pkuuid, filename>-->
        ///// </summary>
        ///// <returns>返回结构(文档pkuid, 图像名)</returns>
        //IDictionary<string, string[]> GetImageList();

        /// <summary>
        /// 图像正反面排序
        /// 描述：将pkuuid所在的对象下面的所有图像执行正反排序
        /// 即1F1B2F2B...
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        bool OrderByPageFlag(string pkuuid);

        ///// <summary>
        ///// 处理下载模式正反面标志对应的DocIndex属性相同
        ///// 描述：目前下载模式，page的排序是按page_index索引顺序排列
        ///// 处理后要按Doc_index与正反面的顺序排序
        ///// </summary>
        //bool ProccessDocIndexByPageFlag();

        /// <summary>
        /// 在修改图像之后，更新modi_range属性
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        bool UpdateModiRangeByEditImage(string realName);

        ///// <summary>
        ///// 切换图像，更改图像的正反面
        ///// </summary>
        ///// <param name="realName">文件的唯一标识</param>
        //void ChangeImageNode(string realName, string laguage = "CN");

        ///// <summary>
        ///// 判断在同一个对象内部是否已存在指定的文件名
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        //bool IsExistsFileName(string fileName);

        /// <summary>
        /// 全部图像正反面排序
        /// 描述：处理正反面相同的doc_index，
        /// 将相同正反面doc_index的值设置一致，
        /// 第二个page的doc_index等于第一个page的doc_index
        /// </summary>
        bool OrderByPageFlagAll();

        /// <summary>
        /// 图像正反面排序
        /// 描述：处理正反面相同的doc_index，
        /// 将相同正反面doc_index的值设置一致，
        /// 第二个page的doc_index等于第一个page的doc_index
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        void OrderByPageFlag1(string pkuuid);

        /// <summary>
        /// 在验证控件文本时发生
        /// </summary>
        event ShowMessage TextValidated;

        /// <summary>
        /// 获取Version节点属性
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        string GetVersionAttribute(string pkuuid, string attributeName);

        ////2019-0619 ljf
        ///// <summary>
        ///// 获取data_type节点属性
        ///// </summary>
        ///// <param name="pkuuid"></param>
        ///// <param name="attribyteName"></param>
        ///// <returns></returns>
        //string GetDataTypeAttribute(string pkuuid,string attribyteName);

        /// <summary>
        /// 获取pkuuid下所有page节点属性
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        /// <returns>返回realname为key，属性值为value的字典</returns>
        Dictionary<string, string> GetPageAttributes(string pkuuid, string attributeName);

        ///// <summary>
        ///// 根据pkuuid跟page_index获取对应的批注文件名
        ///// </summary>
        ///// <param name="pkuuid"></param>
        ///// <param name="pageIndex"></param>
        ///// <returns>批注文件名</returns>
        //string GetPostName(string pkuuid, int pageIndex);

        /// <summary>
        /// 根据pkuuid跟page_index获取对应的批注文件名
        /// </summary>
        /// <param name="realName"></param>
        /// <returns>批注文件名</returns>
        string GetPostName(string realName);

        bool IfGetAllImageInfo { set; get; }

        Dictionary<string, List<Page>> ImageInfo { get; }

        /// <summary>
        /// 根据realname获取批注的oper_type
        /// </summary>
        /// <param name="realname"></param>
        /// <returns></returns>
        string GetPostOperType(string realname);
    }
}
