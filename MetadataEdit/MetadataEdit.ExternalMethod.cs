using System.Collections.Generic;
using Com.Boc.Icms .MetadataEdit.Business.BusinessData;
using Com.Boc.Icms .MetadataEdit.Business.Operate.DocInfo;
using Com.Boc.Icms .MetadataEdit.Business.Operate;
using Com.Boc.Icms .MetadataEdit.Business.Operate.PageInfo;
using Com.Boc.Icms .MetadataEdit.Support.Template;
using System.Collections.ObjectModel;
using Com.Boc.Icms .MetadataEdit.Services;
using Com.Boc.Icms .MetadataEdit.Models;
using Com.Boc.Icms .GlobalResource;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo;
using Com.Boc.Icms.MetadataEdit.Business.Operate.PageInfo;
using Com.Boc.Icms.MetadataEdit.Business.Operate;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.Support.Template;
using Com.Boc.Icms.MetadataEdit.GetDataSource;

namespace Com.Boc.Icms .MetadataEdit
{
    public struct IcmsFileInfo
    {
        //内容文件对应的docIndex
        private int _docIndex;

        public int DocIndex
        {
            get { return this._docIndex; }
            set
            {
                this._docIndex = value;
            }
        }

        //内容文件对应的pageIndex
        private int _pageIndex;

        public int PageIndex
        {
            get { return this._pageIndex; }
            set
            {
                this._pageIndex = value;
            }
        }

        //内容文件对应的文件名
        private string _fileName;

        public string FileName
        {
            get { return this._fileName; }
            set
            {
                this._fileName = value;
            }
        }

        private string operType;

        public string OperType
        {
            get { return operType; }
            set { operType = value; }
        }
    }

    /// <summary>
    /// 外部调用方法
    /// </summary>
    public partial class MetadataEdit
    {
        IXmlDataImport _ixmlfe = null;
        //保留，doc_index修改原意之前。
        //IPageInfoOperate ixmlio = null;
        //IDocInfocOperate ixmldo = null;
        INewPageInfoOperate _ixmlio = null;
        INewDocInfocOperate _ixmldo = null;
        XmlTemplateCache _xmlct = null;
        IXmlDataExport _ibxmle = null;
        DataServies _dataServies = null;
        CommFunc _commFunc = null;
        XmlNodeOperate _xmlNodeOpeirate = null;
        Property _property = null;
		public GetSource _getSource;
		public MetadataEdit()
        {

            _getSource = new GetSource();
			this._property = new Property();
            this._dataServies = new DataServies();
            this._commFunc = new CommFunc(this._property);
            this._xmlct = new XmlTemplateCache();
            this._ibxmle = new XmlDataExport(this._xmlct, this._dataServies);
            this._ixmlfe = new XmlDataImport(this._dataServies);
            //保留，doc_index修改原意之前。
            //ixmlio = new PageInfoOperate(dataManage, property, ixmlfe);
            //ixmldo = new DocInfoOperate(this, dataManage, property, ixmlfe, ixmlct);
            this._ixmlio = new NewPageInfoOperate(this._dataServies, this._ixmlfe, this._property);
            this._ixmldo = new NewDocInfoOperate(this._dataServies, this._property, this._ixmlfe, this._xmlct, _getSource);

            this._xmlNodeOpeirate = new XmlNodeOperate(this._dataServies);
        }

        #region add by dingyao 20160106,添加元数据重新初始化数据的方法
        public void ReSetProperty()
        {
			_getSource = new GetSource();
			this._property = new Property();
            this._dataServies = new DataServies();
            this._commFunc = new CommFunc(this._property);
            this._xmlct = new XmlTemplateCache();
            this._ibxmle = new XmlDataExport(this._xmlct, this._dataServies);
            this._ixmlfe = new XmlDataImport(this._dataServies);
            //保留，doc_index修改原意之前。
            //ixmlio = new PageInfoOperate(dataManage, property, ixmlfe);
            //ixmldo = new DocInfoOperate(this, dataManage, property, ixmlfe, ixmlct);
            this._ixmlio = new NewPageInfoOperate(this._dataServies, this._ixmlfe, this._property);
            this._ixmldo = new NewDocInfoOperate(this._dataServies, this._property, this._ixmlfe, this._xmlct, _getSource);

            this._xmlNodeOpeirate = new XmlNodeOperate(this._dataServies);

            if (!string.IsNullOrEmpty(this._businessControlXml))
            {
                this.InitBusinessControlXml(this._businessControlXml);
            }
        }
        #endregion

        #region //保留，doc_index修改原意之前。
        /**
        #region 初始化XML文档
        /// <summary>
        /// 初始化加载业务控件的XML模板
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public void InitBusinessControlXml(string xml)
        {
            ixmlct.LoadXml(xml);
        }

        /// <summary>
        /// 初始化加载businessinfo的XML
        /// 
        /// 描述：businessinfo代表业务批次根目录。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public void InitBusinessXml(string xml)
        {
            ixmlfe.SaveXml(xml);

            commFunc.ChangeWorkType(this.WorkType);
        }

        /// <summary>
        /// 初始化加载文档类型docs_info的XML
        /// 
        /// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public void InitDocXml(string xml)
        {
            ixmldo.InitXml(xml, this.WorkType);
        }

        /// <summary>
        /// 初始化pages_info的XML
        /// 
        /// 描述：图像添加到文档节点之后，生成图像XML，此处进行数据缓存。
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="xml">xml字符串</param>
        public void InitDocImageXml(int doc_index, string xml)
        {
            ixmlio.InitXml(doc_index, xml);
        }

        /// <summary>
        ///  初始化加载整个业务的XML（包含businessinfo，docs_info）
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public void InitAllXml(string xml)
        {
            ixmlfe.SaveAllXml(xml);

            commFunc.ChangeWorkType(this.WorkType);
        }
        #endregion

        #region 文档操作
        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="data_type">数据类型</param>
        public void AddDoc(string data_type)
        {
            ixmldo.AddDoc(data_type);
        }

        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="docXml">xml字符串</param>
        public void AddDoc(int doc_index, string docXml)
        {
            ixmlio.AddDoc(doc_index, docXml);
        }

        /// <summary>
        /// 删除单个文档
        /// </summary>
        /// <param name="doc_index"></param>
        public void DeleteDoc(int doc_index)
        {
            ixmldo.DeleteDoc(doc_index);
        }

        /// <summary>
        /// 拖动文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        public void DragDoc(int doc_index, int old_doc_index)
        {
            ixmldo.MoveDoc(doc_index, old_doc_index);
        }
        #endregion

        #region 图像操作

        /// <summary>
        /// 添加单个图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="file_name">图像文件名</param>
        public void AddImage(int doc_index, string file_name)
        {
            ixmlio.AddImageXml(doc_index, file_name);
        }

        /// <summary>
        /// 添加单个图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="pageXml">图像xml字符串</param>
        public void AddImage(int doc_index, int page_index, string pageXml)
        {
            ixmlio.AddImageXml(doc_index, page_index, pageXml);
        }

        /// <summary>
        /// 添加图像集合
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="fileNames">图像文件名集合</param>
        public void AddImage(int doc_index, string[] fileNames)
        {
            ixmlio.AddImageXml(doc_index, fileNames);
        }

        /// <summary>
        /// 删除图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        public void DeleteImage(int doc_index, int page_index)
        {
            //文档增加删除节点 添加删除索引号
            ixmlio.DeleteImageXml(doc_index, page_index);
        }

        /// <summary>
        /// 拖动图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        /// <param name="old_page_index">初始图像索引</param>
        public void DragImage(int doc_index, int page_index, int old_doc_index, int old_page_index)
        {
            //原始图像删除 目标文档添加
            //原文档增加删除节点 添加删除索引号
            ixmlio.MoveImage(doc_index, page_index, old_doc_index, old_page_index);
        }

        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        /// <param name="old_page_index">初始图像索引</param>
        public void CopyImage(int doc_index, int old_doc_index, int old_page_index)
        {
            ixmlio.CopyImage(doc_index, old_doc_index, old_page_index);
        }

        /// <summary>
        /// 替换图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="file_name"></param>
        public void ReplaceFileName(int doc_index, int page_index, string file_name)
        {
            ixmlio.ReplaceFileName(doc_index, page_index, file_name);
        }

        /// <summary>
        /// 修改图像正反面
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="page_flag">当前正反面</param>
        public void UpdateImageFlag(int doc_index, int page_index, string page_flag)
        {
            ixmlio.UpdateImageFlag(doc_index, page_index, page_flag);
        }

        /// <summary>
        /// 添加图像批注文件
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="remark">批注信息</param>
        public void AddImagePostil(int doc_index, int page_index, string remark)
        {
            ixmlio.AddImagePostil(doc_index, page_index, remark);
        }

        /// <summary>
        /// 修改图像批注信息
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="remark">批注信息</param>
        public void UpdateImagePostil(int doc_index, int page_index, string remark)
        {
            ixmlio.UpdateImagePostil(doc_index, page_index, remark);
        }

        /// <summary>
        /// 删除批注
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        public void DeleteImagePostil(int doc_index, int page_index)
        {
            ixmlio.DeleteImagePostil(doc_index, page_index);
        }
        #endregion

        #region 基本操作
        /// <summary>
        /// 切换节点对应的XML控件模板
        /// 
        /// 描述：根据指定的模板名进行模板检索，并生成控件。根据指定的文档索引查找控件值，并填充控件。
        /// </summary>
        /// <param name="doc_index">文档索引(如何小于0的数都代表Business_info根节点)</param>
        /// <param name="templateName">模板名</param>
        public void ChangeXmlNode(int doc_index, string templateName)
        {
            //警告：防止多次绑定同一事件
            ixmldo.TextValidated -= this.TextValidated;
            ixmldo.TextValidated += this.TextValidated;

            ixmldo.XmlValidated -= this.XmlValidated;
            ixmldo.XmlValidated += this.XmlValidated;

            ixmldo.ChangeXmlNode(doc_index, templateName);
        }

        /// <summary>
        /// 取得图像排序的索引键值对 
        /// 
        /// 描述：补扫、重扫，取得预加载的排序图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <returns>已排序好的(索引,文件名)键值对</returns>
        public Dictionary<int, string> GetImageOrder(int doc_index)
        {
            return ixmlio.GetImageOrder(doc_index);
        }

        /// <summary>
        /// 获取所有改变的文件名
        /// </summary>
        /// <returns>改变的文件名列表</returns>
        public List<string> GetChangedFileNames()
        {
            return ixmlio.GetChangedFileNames();
        }

        /// <summary>
        /// 获取Control类型的子节点信息
        /// </summary>
        /// <param name="docIndex">文件类型编号</param>
        /// <param name="nodeNames">子节点名称</param>
        /// <returns>子节点名称跟子节点值的组合</returns>
        public Dictionary<string, string> GetChildNodeValue(int docIndex, List<string> nodeNames)
        {
            return ixmlio.GetChildNodeValue(docIndex, nodeNames);
        }

        /// <summary>
        /// 重扫补扫时获取整个业务图像信息<!--<doc index, <page index, file name>>-->
        /// </summary>
        /// <returns>返回结构(文档索引, (图像索引, 图像名)) </returns>
        public Dictionary<int, Dictionary<int, string>> GetImageOrder()
        {
            return ixmlio.GetImageOrder();
        }

        /// <summary>
        /// 取得需要交互的整个业务XML文档
        /// 
        /// 描述：默认XML版本1.0、编码UTF-8
        /// </summary>
        public string GetAllBusinessXml()
        {
            return ibxmle.CreateIndexXml();
        }

        /// <summary>
        /// 获取业务交互XML文档
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="description">描述与注释</param>
        /// <returns></returns>
        string GetAllBusinessXml(string version, string encoding, string description)
        {
            return ibxmle.CreateIndexXml(version, encoding, description);
        }

        /// <summary>
        /// 根据指定的数据类型检索文档的PKUUID
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public string[] GetGuidByType(string type)
        {
            return ixmldo.GetGuidByType(type);
        }

        /// <summary>
        /// 根据指定的文档索引检索字段名为field的值
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        public string GetValueByField(int doc_index, string field)
        {
            return ixmldo.GetValueByField(doc_index, field);
        } 
        **/
        #endregion

        #region 初始化XML文档
        /// <summary>
        /// 初始化加载业务控件的XML模板
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public bool InitBusinessControlXml(string xml)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._xmlct.LoadXml(xml);
        }

        /// <summary>
        /// 初始化加载businessinfo的XML
        /// 描述：businessinfo代表业务批次根目录。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public bool InitBusinessXml(string xml)
        {
            /*
            return ixmlfe.SaveXml(xml) &
                commFunc.ChangeWorkType(this.WorkType);
             */
            //this._ixmlfe.ParentID = 0;
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlfe.SaveXml(xml);
        }

        ///// <summary>
        ///// 初始化加载文档类型docs_info的XML
        ///// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        ///// </summary>
        ///// <param name="xml">xml字符串</param>
        ///// <param name="isAddUniqMetadata">是否自动添加uniq_metadata节点</param>
        ///// <param name="isAddGuid">是否自动添加uniq_metadata节点值guid</param>
        ///// <returns>pkuuid列表</returns>
        //public string[] InitDocXml(string xml, bool isAddUniqMetadata, bool isAddGuid, int index)
        //{
        //    this._ixmlio.IfGetAllImageInfo = true;
        //    return this._ixmldo.InitXml(xml, isAddUniqMetadata, isAddGuid, index);
        //}
        /// <summary>
        /// 初始化加载文档类型docs_info的XML
        /// 描述：docs_info包含很多doc_info，解析并保存到数据结构中。
        /// </summary>
        /// <param name="xml">xml字符串</param>
        /// <param name="isAddUniqMetadata">是否自动添加uniq_metadata节点</param>
        /// <param name="isAddGuid">是否自动添加uniq_metadata节点值guid</param>
        /// <param name="pkuuidList">pkuuid列表</param>
        /// <returns>是否操作成功</returns>
        public bool InitDocXml(string xml, string businessId)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmldo.AddDocs(xml, businessId);
        }

        ///// <summary>
        ///// 初始化pages_info的XML
        ///// 描述：图像添加到文档节点之后，生成图像XML，此处进行数据缓存。
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="xml">xml字符串</param>
        //public bool InitDocImageXml(string pkuuid, string xml)
        //{
        //    this._ixmlio.IfGetAllImageInfo = true;
        //    return this._ixmlio.InitXml(pkuuid, xml);
        //}

        /// <summary>
        ///  初始化加载整个业务的XML（包含businessinfo，docs_info）
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public bool InitAllXml(string xml)
        {
            /*
            return ixmlfe.SaveAllXml(xml) &
                ixmlio.ProccessDocIndexByPageFlag() &
                commFunc.ChangeWorkType(this.WorkType);
             */
            //return this._ixmlfe.SaveAllXml(xml) & this._ixmlio.ProccessDocIndexByPageFlag();
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlfe.SaveAllXml(xml);
        }

      

        #endregion



        #region 文档操作
        /// <summary>
        /// 添加单个文档
        /// </summary>
        /// <param name="xml">文档XML</param>
        /// <param name="businessId">交易编号</param>
        public string AddDoc(string xml, string businessId)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmldo.AddDoc(businessId, xml);
        }

        /// <summary>
        /// 删除单个文档
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        public bool DeleteDoc(string pkuuid)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmldo.DeleteDoc(pkuuid);
        }

        /// <summary>
        /// 删除指定交易节点
        /// </summary>
        /// <param name="businessIndex">交易编号</param>
        /// <returns>删除结果</returns>
        public bool DeleteBusinessInfo(string businessId)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmldo.DeleteBusinessInfo(businessId);
        }

        /*  delete by dy,删除此方法，新内存表没有当前交易一说，删除均走交易编号删除
        /// <summary>
        /// 删除当前交易节点
        /// </summary>
        /// <returns>删除结果</returns>
        public bool DeleteCurBusinessInfo()
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmldo.DeleteCurBusinessInfo();
        }
        */
        #endregion

        #region 图像操作
        /// <summary>
        /// 添加单个图像
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="pageIndex">图像索引</param>
        /// <param name="pageXml">图像xml字符串</param>
        public bool AddImageByXml(string pkuuid, string pageXml, string realName)
        {
            if (string.IsNullOrEmpty(pkuuid))
            {
                return false;
            }

            //if (this._ixmlio.AddImageByXml(pkuuid, pageXml,ref realName))
            //{
            //    this._dataServies.Commit();
            //    return true;
            //}

            return this._ixmlio.AddImageByXml(pkuuid, pageXml, realName);
        }

        /// <summary>
        /// 增加图像节点(默认全部正面)
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="file_name">图像文件名</param>
        public bool AddImage(string pkuuid, string fileName, string realName)
        {
            return this._ixmlio.AddImage(pkuuid, fileName, realName);
        }

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileName">图像文件名</param>
        /// <param name="pageFlag">图像正反面(True正-Flase反)</param>
        /// <param name="realName">文件唯一标识</param>
        public bool AddImage(string pkuuid, string fileName, bool pageFlag, string realName)
        {
            return this._ixmlio.AddImage(pkuuid, fileName, pageFlag, realName);
        }

        /// <summary>
        /// 增加图像节点集合
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileNames">图像文件名</param>
        public bool AddImage(string pkuuid, string[] fileNames, string[] realNames)
        {
            if (fileNames == null || fileNames.Length == 0)
                return false;
            return this._ixmlio.AddImage(pkuuid, fileNames, realNames);
        }

        /// <summary>
        /// 删除图像
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        public bool DeleteImage(string realName)
        {
            if (string.IsNullOrEmpty(realName))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.DeleteImage(realName);
        }

        /// <summary>
        /// 移动图像
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="docIndex">目的节点的docIndex -1</param>
        /// <param name="dragRealName">拖拽文件的索引</param>
        public string DragImage(string pkuuid, int pageIndex, string dragRealName,bool ifChangeRealName)
        {
            if (string.IsNullOrEmpty(pkuuid))
            {
                return "";
            }
            //原始图像删除 目标文档添加
            //原文档增加删除节点 添加删除索引号
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.MoveImage(pkuuid, pageIndex, dragRealName, ifChangeRealName);
        }

        /// <summary>
        /// page排序后修改pkuuid下文件的pageIndex
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="newPagesIndex">文件要设置的Index字典(文件名，pageIndex)</param>
        /// <returns></returns>
        public bool UpdatePagesIndex(string pkuuid, Dictionary<string, int> newPagesIndex)
        {
            if (string.IsNullOrEmpty(pkuuid))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.UpdatePagesIndex(pkuuid, newPagesIndex);
        }
        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="pkuuid">要拷贝到的文档唯一标识</param>
        /// <param name="copyRealName">初始文档唯一标识</param>
        /// <param name="realName">拷贝后文件的唯一标识</param>
        public bool CopyImage(string pkuuid, string copyRealName, ref string realName)
        {
            if (string.IsNullOrEmpty(pkuuid))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.CopyImage(pkuuid, copyRealName, ref realName);
        }

        /// <summary>
        /// 替换图像
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        /// <param name="fileName">图想文件名</param>
        public bool ReplaceFileName(string realName, string fileName)
        {
            if (string.IsNullOrEmpty(realName))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.ReplaceFileName(realName, fileName);
        }

        ///// <summary>
        ///// 替换图像
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="pageIndex">图像索引</param>
        ///// <param name="fileName"></param>
        //public bool ReplaceOnlyFileName(string realName, string fileName)
        //{
        //    if (string.IsNullOrEmpty(realName))
        //    {
        //        return false;
        //    }
        //    this._ixmlio.IfGetAllImageInfo = true;
        //    return this._ixmlio.ReplaceOnlyFileName(realName, fileName);
        //}

        /// <summary>
        /// 修改图像正反面
        /// </summary>
        /// <param name="realName">page唯一标识</param>
        /// <param name="pageFlag">当前正反面</param>
        public bool UpdateImageFlag(string realName, string pageFlag)
        {
            if (string.IsNullOrEmpty(realName))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.UpdateImageFlag(realName, pageFlag);
        }

        /// <summary>
        /// 修改oper_type和modi_range
        /// </summary>
        /// <param name="realName">page唯一标识</param>
        /// <param name="opertype"></param>
        /// <param name="modirange"></param>
        /// <returns></returns>
        public bool UpdateImageOperTypeAndModiRange(string realName, string opertype, string modirange)
        {
            if (string.IsNullOrEmpty(realName))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.UpdateImageOperTypeAndModiRange(realName, opertype, modirange);
        }

        /// <summary>
        /// 添加图像批注文件
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="remark">批注信息</param>
        public bool AddImagePostil(string realname, string postilName, string remark)
        {
            if (string.IsNullOrEmpty(realname))
            {
                return false;
            }
            return this._ixmlio.AddImagePostil(realname, postilName, remark);
        }

        /// <summary>
        /// 修改图像批注信息
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="remark">批注信息</param>
        public bool UpdateImagePostil(string realname, string remark)
        {
            if (string.IsNullOrEmpty(realname))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.UpdateImagePostil(realname, remark);
        }

        /// <summary>
        /// 删除批注
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        public bool DeleteImagePostil(string realname)
        {
            if (string.IsNullOrEmpty(realname))
            {
                return false;
            }
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.DeleteImagePostil(realname);
        }
        #endregion

        #region 基本操作
        /// <summary>
        /// 切换节点对应的XML控件模板
        /// 描述：根据指定的模板名进行模板检索，并生成控件。根据指定的文档索引查找控件值，并填充控件。
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="templateName">模板名</param>
        public void ChangeXmlNode(EnumType.TableType tableType, string key, string templateName)
        {
            /**警告：防止多次绑定同一事件**/
            this._ixmldo.TextValidated -= this.TextValidated;
            this._ixmldo.TextValidated += this.TextValidated;

            this._ixmldo.BizMetadata1Changed -= this.BizMetadata1Changed;
            this._ixmldo.BizMetadata1Changed += this.BizMetadata1Changed;

            this._ixmldo.ChangeTreeNodeForeColor -= this.ChangeTreeNodeForeColor;
            this._ixmldo.ChangeTreeNodeForeColor += this.ChangeTreeNodeForeColor;

            this._ixmldo.XmlValidated -= this.XmlValidated;
            this._ixmldo.XmlValidated += this.XmlValidated;

            this._ixmldo.ChangeXmlNode(tableType, key, templateName);
        }

        ///// <summary>
        ///// 取得图像排序的索引键值对 
        ///// 描述：补扫、重扫，取得预加载的排序图像
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <returns>已排序好的(索引,文件名)键值对</returns>
        //public IDictionary<int, string> GetImageOrder(string pkuuid)
        //{
        //    return this._ixmlio.GetImageOrder(pkuuid);
        //}

        /// <summary>
        /// 获取所有改变的文件名
        /// </summary>
        /// <returns>改变的文件名列表</returns>
        public Collection<string> GetChangedFileNames()
        {
            return this._ixmlio.GetChangedFileNames();
        }

        ///// <summary>
        ///// 获取Control类型的子节点信息
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="nodeNames">子节点名称</param>
        ///// <returns>子节点名称跟子节点值的组合</returns>
        //public IDictionary<string, string> GetChildNodeValue(string pkuuid, string[] nodeNames)
        //{
        //    return this._ixmlio.GetChildNodeValue(pkuuid, nodeNames);
        //}

        ///// <summary>
        ///// 重扫补扫时获取整个业务图像信息<!--<doc index, <page index, file name>>-->
        ///// </summary>
        ///// <returns>返回结构(文档索引, (图像索引, 图像名)) </returns>
        //public IDictionary<string, Dictionary<int, string>> GetImageOrder()
        //{
        //    return this._ixmlio.GetImageOrder();
        //}

        /// <summary>
        /// 重扫补扫时获取整个业务图像信息和版本信息<!--<doc index, <page index, file name>>-->
        /// </summary>
        /// <param name="docFileInfo">图像信息</param>
        /// <param name="hizFileInfo">版本信息</param> 
        public void GetImageOrderAndHizVersion(ref IDictionary<string, List<string>> hizFileInfo)
        {
            this._ixmlio.GetImageOrderAndHizVersion(ref hizFileInfo);
        }

        ///// <summary>
        ///// 取得需要交互的整个业务XML文档
        ///// 描述：默认XML版本1.0、编码UTF-8
        ///// </summary>
        //public string GetAllBusinessXml(bool delIgnore)
        //{
        //    return this._ibxmle.CreateIndexXml(delIgnore);
        //}

        ///// <summary>
        ///// 获取业务交互XML文档
        ///// </summary>
        ///// <param name="version">版本</param>
        ///// <param name="encoding">编码格式</param>
        ///// <param name="description">描述与注释</param>
        ///// <returns></returns>
        //public string GetAllBusinessXml(string version, string encoding, string description, bool delIgnore)
        //{
        //    return this._ibxmle.CreateIndexXml(version, encoding, description, delIgnore);
        //}

        /// <summary>
        /// 按交易批量生成index xml
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="description">描述与注释</param>
        /// <returns>交易码，该交易对应的index xml</returns>
        public Dictionary<string, string> GetBatchBusinessXml(bool delIgnore)
        {
            return _ibxmle.BatchCreateIndexXml(delIgnore);
        }

        ///// <summary>
        ///// 按交易批量生成index xml
        ///// </summary>
        ///// <returns>交易码，该交易对应的index xml</returns>
        //public Dictionary<string, string> GetBatchBusinessXml(string version, string encoding, string description, bool delIgnore)
        //{
        //    return this._ibxmle.BatchCreateIndexXml(version, encoding, description, delIgnore);
        //}

        ///// <summary>
        ///// 根据指定的数据类型检索文档的PKUUID
        ///// </summary>
        ///// <param name="type">数据类型</param>
        ///// <returns></returns>
        //public string[] GetGuidByType(string type)
        //{
        //    return this._ixmldo.GetGuidByType(type);
        //}

        /// <summary>
        /// 根据指定的文档索引检索字段名为field的值
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="field">字段名</param>
        /// <returns></returns>
        public string GetValueByField(string pkuuid, string field)
        {
            return this._ixmldo.GetValueByField(pkuuid, field);
        }

        ///// <summary>
        ///// 根据指定的PKUID检索属性值
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="attribute">属性名</param>
        //public string GetDocAttribute(string pkuuid, string attribute)
        //{
        //    return this._ixmldo.GetDocAttribute(pkuuid, attribute);
        //}

        /// <summary>
        /// 清空内存表
        /// </summary>
        public void Clear()
        {
            this._ixmlio.IfGetAllImageInfo = true;
            this._dataServies.Clear();
        }

        ///// <summary>
        ///// 获取字段值为空的节点名称
        ///// </summary>
        ///// <returns></returns>
        //public IDictionary<int, string> GetErrorControlInfo()
        //{
        //    return this._ibxmle.GetErrorControlInfo();
        //}

        /// <summary>
        /// 获取page节点属性
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        public string GetPageAttribute(string realname, string attributeName)
        {
            return this._ixmlio.GetPageAttribute(realname, attributeName);
        }

        /// <summary>
        /// 获取pkuuid下所有page节点属性
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        /// <returns>返回pageIndex为key，属性值为value的字典</returns>
        public Dictionary<string, string> GetPageAttributes(string pkuuid, string attributeName)
        {

            return this._ixmlio.GetPageAttributes(pkuuid, attributeName);
        }

        /// <summary>
        /// 获取Version节点属性
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        public string GetVersionAttribute(string pkuuid, string attributeName)
        {
            return this._ixmlio.GetVersionAttribute(pkuuid, attributeName);
        }

        ////2019-06-19 ljf
        ///// <summary>
        ///// 获取Data_type节点
        ///// </summary>
        ///// <param name="pkuuid"></param>
        ///// <param name="attributeName"></param>
        ///// <returns></returns>
        //public string GetDataTypeAttribute(string pkuuid,string attributeName)
        //{
        //    return this._ixmlio.GetDataTypeAttribute(pkuuid, attributeName);
        //}


        /// <summary>
        /// 检索批次的字段名为field的值
        /// <param name="bussnessId">交易编号</param>
        /// <param name="field">字段名</param>
        public string GetValueByBusinessInfoField(string bussnessId, string field)
        {
            return this._ixmldo.GetValueByBusinessInfoField(bussnessId, field);
        }

        /// <summary>
        /// 设置businessinfo的节点值
        /// </summary>
        /// <param name="bussnessId">交易编号</param>
        /// <param name="busInfo">要设置的值<节点名字，节点值></param>
        /// <returns>是否修改成功</returns>
        public bool SetValueByBusinessInfoField(string bussnessId, Dictionary<string, string> busInfo)
        {
            bool isUpdateSucceed = this._ixmldo.SetValueByBusinessInfoField(bussnessId, busInfo);
            this._ixmlio.IfGetAllImageInfo = true;
            return isUpdateSucceed;
        }

        ///// <summary>
        ///// 重扫补扫时获取整个业务图像信息<!--<pkuuid, filename>-->
        ///// </summary>
        ///// <returns>返回结构(文档pkuid, 图像名)</returns>
        //public IDictionary<string, string[]> GetImageList()
        //{
        //    if (this._property.PageFlagMode == EnumType.EnumPageFlagMode.FrontBreak &&
        //        !this.OrderByPageFlagAll())
        //        return new Dictionary<string, string[]>();

        //    return this._ixmlio.GetImageList();
        //}

        /// <summary>
        /// 图像正反面排序
        /// 描述：将pkuuid所在的对象下面的所有图像执行正反排序
        ///      即1F1B2F2B...
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        public bool OrderByPageFlag(string pkuuid)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.OrderByPageFlag(pkuuid);
        }

        /// <summary>
        /// 在修改图像之后，更新modi_range属性
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        public bool UpdateModiRangeByEditImage(string realName)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.UpdateModiRangeByEditImage(realName);
        }



        /// <summary>
        /// 切换图像，更改图像的正反面
        /// </summary>
        /// <param name="realName">文件的唯一标识</param>
        public void ChangeImageNode(string realName, string laguage = "CN")
        {
            this._ixmlio.TextValidated -= this.TextValidated;
            this._ixmlio.TextValidated += this.TextValidated;
            this._ixmlio.IfGetAllImageInfo = true;
            //this._ixmlio.ChangeImageNode(realName, laguage);
        }

        /*/// <summary>
        /// 检索控件名为field的模板控件标签
        /// </summary>
        /// <param name="field">控件名</param>
        public string GetTemplateControlCaption(string field)
        {
            return this._ixmldo.GetTemplateControlCaption(field);
        }*/

        ///// <summary>
        ///// 判断在同一个对象内部是否已存在指定的文件名
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        //public bool IsExistsFileName(string fileName)
        //{
        //    return this._ixmlio.IsExistsFileName(fileName);
        //}

        /// <summary>
        /// 图像正反面排序
        /// 描述：处理正反面相同的doc_index，
        /// 将相同正反面doc_index的值设置一致，
        /// 第二个page的doc_index等于第一个page的doc_index
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        public void OrderByPageFlag1(string pkuuid)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            this._ixmlio.OrderByPageFlag1(pkuuid);
        }

        /// <summary>
        /// 全部图像正反面排序
        /// 描述：处理正反面相同的doc_index，
        /// 将相同正反面doc_index的值设置一致，
        /// 第二个page的doc_index等于第一个page的doc_index
        /// </summary>
        public bool OrderByPageFlagAll()
        {
            this._ixmlio.IfGetAllImageInfo = true;
            return this._ixmlio.OrderByPageFlagAll();
        }

        /// <summary>
        /// 判断元数据是否符合上传规则
        /// </summary>
        /// <param name="dic">pkuuid,模板名键值对</param>
        /// <returns>成功返回1,否则pkuuid或者返回错误消息</returns>
        public Dictionary<string, List<string>> CheckAllMetadata(Dictionary<int, Dictionary<string, string>> dic)
        {
            return this._ibxmle.CheckAllMetadata(dic);
        }

        ///// <summary>
        ///// 切换交易节点
        ///// </summary>
        ///// <param name="businessIndex">交易索引</param>
        //public void ChangeBusinessId(int businessIndex)
        //{


        //    this._commFunc.ChangeBusinessId(businessIndex);
        //    if (this._commFunc.BusinessIdChanged)
        //    {
        //        this._ixmlio.IfGetAllImageInfo = true;
        //    }
        //}

        ///// <summary>
        ///// 获取业务Business_info节点在数据结构中的编号
        ///// </summary>
        ///// <returns>业务Business_info节点在数据结构中的编号</returns>
        //public int GetBusinessId()
        //{
        //    return this._commFunc.GetBusinessId();
        //}

        /// <summary>
        /// 切换交易节点
        /// </summary>
        /// <param name="businessIndex">交易索引</param>
        public void ChangeWorkType(EnumType.EnumWorkType type)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            this._commFunc.ChangeWorkType(type);
        }

        ///// <summary>
        ///// 获取数据结构的XML
        ///// </summary>
        ///// <returns></returns>
        //public string GetDataXml()
        //{
        //    return this._ibxmle.GetDataXml();
        //}

        ///// <summary>
        ///// 修改节点的属性值(同时修改节点中的modi_time时间值)
        ///// </summary>
        ///// <param name="nodeId">节点编号</param>
        ///// <param name="attribute">属性名</param>
        ///// <param name="value">值</param>
        //public virtual void UpdateNodePropertyAndTime(int nodeId, string attribute, string value)
        //{
        //    this._ixmlio.IfGetAllImageInfo = true;
        //    this._xmlNodeOpeirate.UpdateNodePropertyAndTime(nodeId, attribute, value);
        //}

        /// <summary>
        /// 更新元数据 IndexMetadata 属性
        /// 描述：只是对Index_Metadata属性进行更新
        /// </summary>
        /// <param name="pkuuid">文档的唯一标识</param>
        /// <returns></returns>
        public virtual void UpdateDocIndexMetadata(string pkuuid, Dictionary<string, string> dicIndexMetadata)
        {
            this._ixmlio.IfGetAllImageInfo = true;
            this._ixmldo.UpdateDocIndexMetadata(pkuuid, dicIndexMetadata);
        }

        /// <summary>
        /// 根据数据类型获取对应的pkuuid
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <returns>pkuuid</returns>
        public string GetPkuuidByDatatype(string dataType)
        {
            return this._ixmldo.GetPkuuidByDatatype(dataType);
        }

        /// <summary>
        /// 根据pkuuid跟page_index获取对应的批注文件名
        /// </summary>
        /// <param name="pkuuid"></param>
        /// <param name="page_index"></param>
        /// <returns>批注文件名</returns>
        //public string GetPostName(string pkuuid, int page_index)
        //{
        //    return ixmlio.GetPostName(pkuuid, page_index);
        //}

        /// <summary>
        /// 根据pkuuid跟page_index获取对应的批注文件名
        /// </summary>
        /// <param name="realName"></param>
        /// <returns>批注文件名</returns>
        public string GetPostName(string realName)
        {
            return this._ixmlio.GetPostName(realName);
        }



        public Dictionary<string, List<Page>> ImageInfo
        {
            get
            {
                return this._ixmlio.ImageInfo;
            }
        }

        /// <summary>
        /// 根据realname取批注的oper_type
        /// </summary>
        /// <param name="realname"></param>
        /// <returns></returns>
        public string GetPostOperType(string realname)
        {
            return this._ixmlio.GetPostOperType(realname);
        }
        #endregion
    }
}
