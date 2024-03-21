using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.Models;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Business.Operate.PageInfo;
using Com.Boc.Icms.MetadataEdit.Business.Operate;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Models;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.PageInfo
{


    /// <summary>
    /// 图像操作类
    /// 前提：doc_index修改原意之后的版本。
    ///       doc_index最初代表文档的索引，但中行目前的意义不同，故而要作此类的添加，
    ///       以保证目前项目的运行。之前的文档类、图像类代码保留不做调整。
    /// 描述：doc_index代表图像在文档中的位置，page_index代表图像在文档中的索引
    ///       此时，文档不支持位置的改变，只支持文档追加、文档下图像添加、文档删除等；
    ///       图像在文档间的拖放操作则先删除再添加(doc_index等于目标文档的图像位置，
    ///       page_index重新排序，old_index等于0)，图像在文档内部拖放，则同时需要更新
    ///       doc_index,old_index.
    ///       图像支持删除，重命名，添加批注等等操作。
    /// 注：1.由于page_info节点的属性page_index固定不会改变，故而图像移动位置之后，只有doc_index、
    ///     缓存index字段列改变。
    ///     2.目前对外接口的page_index代表缓存的index，不代表page_info节点的属性page_index。
    /// </summary>
    class NewPageInfoOperate : XmlNodeOperate, INewPageInfoOperate
    {
        private readonly IXmlDataImport _ixmlfe;

        private readonly Property _property = null;
        /// <summary>
        /// 在验证控件文本时发生
        /// </summary>
        public event ShowMessage TextValidated;

        private readonly string _proIdAndThreadId = string.Empty;

        private PageTable pageTable = null;
        public NewPageInfoOperate(DataServies dataServies,
            IXmlDataImport ixmlfe, Property property)
            : base(dataServies)
        {
            this._ixmlfe = ixmlfe;
            this._property = property;
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            pageTable = (PageTable)this.dataServies.GetDataTable(EnumType.TableType.PageTable);
        }

        #region 图像
        ///// <summary>
        ///// 初始化pages_info的XML
        ///// 描述：图像添加到文档节点之后，生成图像XML，此处进行数据缓存。
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="xml">xml字符串</param>
        //public bool InitXml(string pkuuid, string xml)
        //{
        //    try
        //    {
        //        this._ixmlfe.ParentID = this.GetDocId(pkuuid);
        //        xml = "<delete_page/>" + xml + "<version_list/>";
        //        if (!this._ixmlfe.SaveXml(xml)) throw new Exception();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7108, ex, this._proIdAndThreadId);
        //        return false;
        //    }
        //}

        /// <summary>
        /// 增加图像节点(默认全部正面)
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileName">图像文件名</param>
        public bool AddImage(string pkuuid, string fileName, string realName)
        {
            return this.AddImage(pkuuid, fileName, true, realName);
        }

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileName">图像文件名</param>
        /// <param name="pageFlag">图像正反面(True正-Flase反)</param>
        public bool AddImage(string pkuuid, string fileName, bool pageFlag, string realName)
        {
            try
            {
                this.AddImageRow(pkuuid, fileName, pageFlag, realName);
                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(7117, ex, this._proIdAndThreadId);
                this.dataServies.RollBack();
                return false;
            }
        }

        /// <summary>
        /// 增加图像节点集合
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileNames">图像文件名</param>
        /// <param name="pageFlagOrder">图像正反面排序(True全部正反配对-Flase全部正面)</param>
        public bool AddImage(string pkuuid, string[] fileNames, string[] realNames)
        {
            bool isSuccess = false;
            bool pageFlag = true;


            if (fileNames == null) return false;


            for (int i = 0; i < fileNames.Length; i++)
            {
                if (this._property.PageFlagMode == EnumType.EnumPageFlagMode.FrontBreak)
                    pageFlag = (i % 2 == 0) ? true : false;

                try
                {
                    this.AddImageRow(pkuuid, fileNames[i], pageFlag, realNames[i]);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    //SysLog.Write(7117, ex, this._proIdAndThreadId);
                    isSuccess = false;
                }
            }


            if (isSuccess) this.dataServies.Commit();
            else this.dataServies.RollBack();
            return isSuccess;
        }

        /// <summary>
        /// 删除图像节点
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        public bool DeleteImage(string realName)
        {
            try
            {
                //根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);

                if (pageRow != null) //
                {

                    //重新排序pages_info节点
                    //OrderBy(pagesID);
                    //在删除位置复位
                    this.ReseizeByIndex((string)pageRow["Pkuuid"], (int)pageRow["Doc_index"]);

                    //判断是否需要添加del_page节点
                    //this.CheckAddImageDeleteNode(pageRow,
                    //    (int)pageRow["Old_doc_index"] == 0 ? true : false);
                    this.CheckAddImageDeleteNode(pageRow,
                        (int)pageRow["Old_doc_index"] == 0 ? true : false);

                    //修改doc_info状态
                    this.UpdateDocOperTypeByDelPage((string)pageRow["Pkuuid"]);

                    pageRow.Delete();

                    this.dataServies.Commit();
                }



                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7119, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 修改图像正反面
        /// </summary>
        /// <param name="realName">page唯一标识</param>
        /// <param name="pageFlag">当前正反面</param>
        public bool UpdateImageFlag(string realName, string pageFlag)
        {
            try
            {
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);

                if (pageRow != null)
                {
                    pageRow.BeginEdit();
                    if (!"A".Equals(pageRow["oper_type"]))
                    {
                        pageRow["oper_type"] = "E";
                    }


                    pageRow["page_flag"] = pageFlag;

                    //修改Mode_Range
                    this.UpdateModiValue(pageRow, true);

                    pageRow.EndEdit();

                    //修改doc_info状态
                    this.UpdateDocOperType((string)pageRow["Pkuuid"], "E");

                    this.dataServies.Commit();
                }


                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7120, ex, this._proIdAndThreadId);
                return false;
            }
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
            try
            {
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);

                if (pageRow != null)
                {
                    pageRow.BeginEdit();
                    if (!"A".Equals(pageRow["oper_type"]) && !opertype.Equals(pageRow["oper_type"]))
                    {
                        pageRow["oper_type"] = opertype;
                        pageRow["modi_range"] = modirange;
                    }
                    pageRow.EndEdit();

                    this.dataServies.Commit();
                }


                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7120, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 替换图像
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        /// <param name="fileName">图想文件名</param>
        public bool ReplaceFileName(string realName, string fileName)
        {
            try
            {
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);

                if (pageRow != null)
                {

                    fileName = this.GetTransName(fileName);

                    pageRow.BeginEdit();



                    if (!"A".Equals(pageRow["oper_type"]))
                    {
                        pageRow["oper_type"] = "E";
                        //有批注修改批注
                        if (!string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
                        {
                            string postilInfo = (string)pageRow["PostilInfo"];

                            postilInfo = this.UpdateNodePropertyAndTime(postilInfo, "file_name", fileName + ".atn");


                            string postilOperType = GetNodePropertyValue(postilInfo, "oper_type");


                            if (postilOperType != "A" && postilOperType != "D")
                            {
                                postilInfo = UpdateNodeProperty(postilInfo, "oper_type", "E");
                            }

                            pageRow["PostilInfo"] = postilInfo;

                        }
                    }
                    else
                    {
                        //有批注修改批注
                        if (!string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
                        {
                            pageRow["PostilInfo"] = this.UpdateNodePropertyAndTime((string)pageRow["PostilInfo"], "file_name", fileName + ".atn");
                        }
                    }

                    pageRow["File_name"] = fileName;
                    //修改Mode_Range
                    this.UpdateModiValue(pageRow, true);
                    pageRow.EndEdit();

                    //修改doc_info状态
                    this.UpdateDocOperType((string)pageRow["Pkuuid"], "E");

                    this.dataServies.Commit();
                }




                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7121, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 只替换图像，不修改元数据
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        /// <param name="fileName">图想文件名</param>
        public bool ReplaceOnlyFileName(string realName, string fileName)
        {
            try
            {
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);

                if (pageRow != null)
                {
                    fileName = this.GetTransName(fileName);


                    pageRow["file_name"] = fileName;

                    pageRow["oper_type"] = "I";

                    //有批注修改批注
                    if (!string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
                    {
                        pageRow["PostilInfo"] = this.UpdateNodePropertyAndTime((string)pageRow["PostilInfo"], "file_name", fileName + ".atn");
                    }

                    this.dataServies.Commit();
                }


                fileName = this.GetTransName(fileName);
                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7121, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 移动图像
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="docIndex">目的节点的docIndex -1</param>
        /// <param name="dragRealName">拖拽文件的索引</param>
        public string MoveImage(string pkuuid, int docIndex, string dragRealName,bool ifChangeRealName)
        {
            try
            {
                docIndex++;
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(dragRealName);
                string realName = "";

                if (pageRow != null)
                {
                    int oldDocIndex = (int)pageRow["doc_index"];
                    int oldoldDocIndex = (int)pageRow["old_doc_index"];

                    string oldPkuuid = (string)pageRow["Pkuuid"];

                    pageRow.BeginEdit();

                    //先在移动点复位 再在目标点占位
                    this.ReseizeByIndex(oldPkuuid, oldDocIndex);
                    if (pkuuid == oldPkuuid)
                    {
                        this.SeizeByIndex(oldPkuuid, docIndex);
                        //修改目标节点Doc_index
                        pageRow["Doc_index"] = docIndex;

                        //修改拖动节点的modi_range属性
                        this.UpdateModiValue(pageRow, true);

                    }
                    else
                    {
                        //在新文档中占位
                        this.SeizeByIndex(pkuuid, docIndex);

                        //判断是否需要添加del_page节点
                        this.CheckAddImageDeleteNode(pageRow, oldoldDocIndex == 0 ? true : false);

                        //修改目标节点Doc_index
                        pageRow["Doc_index"] = docIndex;


                        //pageRow["page_index"] = System.DBNull.Value;

                        //修改图像的old_doc_index=0 
                        //原因：下载的图像，拖到新增的doc_info对象中，
                        //则它表现的动作是在原位置移除，在新位置增加
                        //(但：在数据结构中只更改索引即可，同时保证，子节点的存在)
                        //后续需要根据old_doc_index判断是否新增的图像()                       
                        pageRow["old_doc_index"] = 0;

                        //修改图像索引属性，值等于当前文档下面的最大图像索引属性值+1
                        pageRow["page_index"] = this.GetMaxPageIndex(pkuuid) + 1;

                        //修改pkuuid 先赋值page_index再改pkuuid，防止this.GetMaxPageIndex(pkuuid)取值不正确
                        pageRow["Pkuuid"] = pkuuid;

                        // 如果是下载模式，将一个图象拖到其他DOC下，意思是先删除再新增，则modi_range = A。
                        pageRow["modi_range"] = "A";


                        //重新排序初始父节点下面的子节点
                        this.OrderBy(oldPkuuid);



                        //修改源doc_info的oper_type属性
                        this.UpdateDocOperType(oldPkuuid, "E");

                        oldoldDocIndex = 0;

                    }


                    realName = Guid.NewGuid().ToString().Replace("-","");
                    if(ifChangeRealName)
                        pageRow["Realname"] = realName;
                    //新增模式或者下载模式的补扫图像的情况，
                    //page_info节点old_doc_index=0、oper_type=A
                    string pageOperType = oldoldDocIndex == 0 ? "A" : "E";

                    pageRow["oper_type"] = pageOperType;

                    try
                    {
                        pageRow.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        //SysLog.Write(7122, ex, this._proIdAndThreadId);
                        throw ex;
                    }


                    //有批注修改批注
                    if (!string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
                    {
                        if (pageOperType == "A")
                        {
                            pageRow["PostilInfo"] = this.UpdateNodePropertyAndTime((string)pageRow["PostilInfo"], "oper_type", "A");
                        }
                        else
                        {
                            string postilInfo = (string)pageRow["PostilInfo"];

                            string postilOperType = GetNodePropertyValue(postilInfo, "oper_type");


                            if (postilOperType != "A")
                            {
                                postilInfo = UpdateNodePropertyAndTime(postilInfo, "oper_type", "E");
                            }

                            pageRow["PostilInfo"] = postilInfo;

                        }
                    }
                    //修改目标节点doc_info的oper_type状态：
                    //新增模式oper_type=A，
                    //下载模式拖动下载的图像的情况，oper_type=E
                    //下载模式拖动补扫的图像的情况，oper_type=A
                    this.UpdateDocOperTypeByAddPage(pkuuid);

                    this.dataServies.Commit();
                }

                return realName;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7122, ex, this._proIdAndThreadId);
                return "";
            }
        }

        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="pkuuid">要拷贝到的文档唯一标识</param>
        /// <param name="copyRealName">初始文档唯一标识</param>
        /// <param name="realName">拷贝后文件的唯一标识</param>
        public bool CopyImage(string pkuuid, string copyRealName, ref string realName)
        {
            try
            {
                // 根据realName 查找行
                Page page = pageTable.FindbyKey(copyRealName);

                int oldDocIndex = page.Old_doc_index;

                if (page != null)
                {
                    //取得图像索引
                    int pageIndex = this.GetMaxPageIndex(pkuuid) + 1;

                    realName = page.Realname = Guid.NewGuid().ToString().Replace("-", "");
                    //修改pkuuid
                    page.Pkuuid = pkuuid;
                    //修改目标节点Doc_index
                    page.Page_index = pageIndex;

                    page.Doc_index = pageIndex + 1;

                    page.Oper_type = "A";

                    page.Old_doc_index = 0;

                    if (!string.IsNullOrEmpty(page.PostilInfo))
                    {
                        page.PostilInfo = this.UpdateNodePropertyAndTime(page.PostilInfo, "oper_type", "A");
                    }
                    //设置时间
                    page.Modi_time = this.Now;

                    DataRow pageRow = this.pageTable.AddRow(page);

                    this.UpdateModiValue(pageRow, true);

                }

                UpdateDocOperType(pkuuid, "E");

                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7123, ex, this._proIdAndThreadId);
                return false;
            }
        }

        ///// <summary>
        ///// 取得图像排序的索引键值对 
        ///// 描述：补扫、重扫，取得预加载的排序图像
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <returns>已排序好的(索引,文件名)键值对</returns>
        //public IDictionary<int, string> GetImageOrder(string pkuuid)
        //{
        //    //索引,文件名键值对
        //    IDictionary<int, string> dic = new Dictionary<int, string>();
        //    //图像文件名
        //    string fileName = string.Empty;

        //    try
        //    {
        //        //取得文档编号
        //        int docId = this.GetDocId(pkuuid);

        //        DataRow[] rows = this.DataManage.GetRowsByType(this.GetSpecialNode(docId),
        //            SupportEnumType.EnumNodeType.Node);
        //        rows.OrderBy(a => int.Parse(a["Index"].ToString()))
        //            .ToList().ForEach(a =>
        //            {
        //                fileName = GetNodePropertyValue(
        //                    a["Value"].ToString(),
        //                    "file_name");
        //                dic.Add((int)a["Index"], fileName);
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7146, ex, this._proIdAndThreadId);
        //    }

        //    return dic;
        //}
        #endregion

        #region 图像批注
        /// <summary>
        /// 添加图像批注文件
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="postilName">批注文件名</param>
        /// <param name="remark">批注信息</param>
        public bool AddImagePostil(string realname, string postilName, string remark)
        {
            try
            {
                postilName = this.GetTransName(postilName);
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realname);

                if (pageRow != null)
                {
                    string postilXml = "<postil modi_time=\"" + this.Now +
                   "\" remark=\"" + remark +
                   "\" file_name=\"" + postilName;

                    //"\" oper_type=\"A\"/>";

                    //判断批注是否存在，如果存在，则删除


                    if (!string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
                    {
                        string postilOperType = GetNodePropertyValue((string)pageRow["PostilInfo"], "oper_type");

                        if (postilOperType == "A")
                        {
                            postilXml += "\" oper_type=\"A\"/>";
                        }
                        else
                        {
                            postilXml += "\" oper_type=\"E\"/>";
                        }
                    }
                    else
                    {
                        postilXml += "\" oper_type=\"A\"/>";
                    }

                    pageRow.BeginEdit();
                    //添加批注
                    pageRow["PostilInfo"] = postilXml;

                    //if (Property.Instance.WorkType != EnumType.EnumWorkType.InitScan)
                    //    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                    //added by liaiqiang
                    if (!"A".Equals(pageRow["oper_type"]))
                    {
                        pageRow["oper_type"] = "E";

                    }
                    //修改Mode_Range
                    this.UpdateModiValue(pageRow, false);

                    pageRow.EndEdit();

                    UpdateDocOperType((string)pageRow["Pkuuid"], "E");


                    this.dataServies.Commit();
                }

                /*
                string fileName = XMLOperate.GetNodePropertyValue(
                    dr["Value"].ToString(),
                    "file_name");
                */

                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7124, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 修改图像批注信息
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="remark">批注信息</param>
        public bool UpdateImagePostil(string realname, string remark)
        {
            try
            {
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realname);

                if (pageRow != null)
                {
                    UpdateDocOperType((string)pageRow["Pkuuid"], "E");

                    pageRow.BeginEdit();
                    if (!"A".Equals(pageRow["oper_type"]))
                    {
                        pageRow["oper_type"] = "E";
                    }

                    string postilInfo = (string)pageRow["PostilInfo"];

                    string postilOperType = GetNodePropertyValue(postilInfo, "oper_type");

                    if (postilOperType != "A")
                    {
                        postilInfo = UpdateNodeProperty(postilInfo, "oper_type", "E");
                    }

                    postilInfo = UpdateNodePropertyAndTime(postilInfo, "remark", remark);

                    pageRow["PostilInfo"] = postilInfo;

                    pageRow.EndEdit();

                    this.dataServies.Commit();
                }

                //if (Property.Instance.WorkType != EnumType.EnumWorkType.InitScan)
                //{
                //    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                //    UpdateNodePropertyAndTime(pageID, "oper_type", "E");

                //    UpdateNodeProperty(PostilID, "remark", remark);
                //    UpdateNodePropertyAndTime(PostilID, "oper_type", "E");

                //    //修改Mode_Range
                //    UpdateModiRange(pageID, false);
                //}
                //else
                //{
                //    UpdateNodePropertyAndTime(PostilID, "remark", remark);
                //}

                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7125, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 删除批注
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        public bool DeleteImagePostil(string realname)
        {
            try
            {
                // 根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realname);

                if (pageRow != null)
                {
                    pageRow.BeginEdit();

                    if (this._property.WorkType != EnumType.EnumWorkType.InitScan)
                    {
                        UpdateDocOperType((string)pageRow["Pkuuid"], "E");


                        string postilInfo = (string)pageRow["PostilInfo"];

                        string postilOperType = GetNodePropertyValue(postilInfo, "oper_type");

                        if (postilOperType != "A" && postilOperType != "")
                        {
                            pageRow["oper_type"] = "E";
                            //this.UpdateNodePropertyAndTime(pageId, "oper_type", "E");
                            pageRow["PostilInfo"] = this.UpdateNodePropertyAndTime(postilInfo, "oper_type", "D");
                        }
                        else
                        {
                            pageRow["PostilInfo"] = "";
                        }
                    }
                    else
                    {
                        //初始添加图像情况，直接删除批注节点
                        pageRow["PostilInfo"] = "";
                    }

                    //修改Mode_Range
                    this.UpdateModiValue(pageRow, false);

                    pageRow.EndEdit();

                    this.dataServies.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                //SysLog.Write(7126, ex, this._proIdAndThreadId);
                return false;
            }
        }

        //获取批注oper_type
        public string GetPostOperType(string realname)
        {
            string oper_type = "";
            /*
            fileName = this.GetTransName(fileName);
            int pageId = -1;

            if (string.IsNullOrEmpty(pkuuid))
            {
                return oper_type;
            }

            DataRow[] pageRows = this.GetPageArray(pkuuid);

            foreach (DataRow row in pageRows)
            {
                if (GetNodePropertyValue(row["Value"].ToString(), "file_name") == fileName)
                {
                    pageId = int.Parse(row["ID"].ToString());
                }
            }

            if (pageId != -1)
            {
                DataRow[] rows = this.DataManage.SelectNodeRows(pageId,
                        "postil",
                        SupportEnumType.EnumNodeType.Node);

                if (rows.Length > 0)
                {
                    oper_type = GetNodePropertyValue(rows[0]["Value"].ToString(), "oper_type");
                }
            }
            */
            // 根据realName 查找行
            DataRow pageRow = pageTable.Rows.Find(realname);

            if (pageRow != null && !string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
            {
                oper_type = GetNodePropertyValue((string)pageRow["PostilInfo"], "oper_type");
            }

            return oper_type;
        }
        #endregion

        #region 私有方法


        /// <summary>
        /// 修改文件名中xml中的特殊字符
        /// </summary>
        /// <param name="oldFileName">原文件名</param>
        /// <returns>转移后的文件名</returns>
        private string GetTransName(string oldFileName)
        {
            //SysLog.Write(9902, this._proIdAndThreadId, "OldFileName: " + oldFileName);

            string result = oldFileName.Replace("&", "&amp;");
            //文件名中没有< > \
            //result = result.Replace("<", "&lt;");
            //result = result.Replace(">", "&gt;");
            //result = result.Replace("\"", "&quot;");

            //SysLog.Write(9903, this._proIdAndThreadId, "result: " + result);

            return result;
        }

        /// <summary>
        /// 把xml中的特殊字符转换正正常的字符
        /// </summary>
        /// <param name="oldFileName">原文件名</param>
        /// <returns>转移后的文件名</returns>
        private string TransNameToFileName(string oldFileName)
        {
            //SysLog.Write(9902, this._proIdAndThreadId, "OldFileName: " + oldFileName);

            string result = oldFileName.Replace("&amp;", "&");

            //文件名中没有< > \
            //result = result.Replace("&lt;", "<");
            //result = result.Replace("&gt;", ">");
            //result = result.Replace("&quot;", "\"");

            //SysLog.Write(9903, this._proIdAndThreadId, "result: " + result);

            return result;
        }

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="fileName">图像文件名</param>
        /// <param name="pageFlag">图像正反面(True正-Flase反)</param>
        private void AddImageRow(string pkuuid, string fileName, bool pageFlag, string realName)
        {
            try
            {
                string xmlfileName = this.GetTransName(fileName);

                Page page = new Page();

                page.Pkuuid = pkuuid;

                page.Realname = realName;

                page.File_name = xmlfileName;

                page.Old_doc_index = 0;

                page.Old_page_flag = page.Page_flag = pageFlag ? "F" : "B";

                page.Modi_time = this.Now;

                page.Oper_type = page.Modi_range = "A";

                //添加图像节点到数据缓存
                int pageIndex = this.GetMaxPageIndex(pkuuid) + 1;

                page.Page_index = pageIndex;

                page.Doc_index = GetMaxDocIndex(pkuuid) + 1;

                pageTable.AddRow(page);


                this.UpdateDocOperTypeByAddPage(pkuuid);
                //if (imageInfo.Count == 0)
                if (this.ImageInfo.Count == 0)
                {
                    imageInfo = GetAllImageInfo();
                    _ifGetAllImageInfo = false;
                }
                else
                {
                    //IcmsFileInfo icmsFileInfo = new IcmsFileInfo();
                    //icmsFileInfo.FileName = fileName;
                    //icmsFileInfo.DocIndex = pageIndex + 1;
                    //icmsFileInfo.PageIndex = pageIndex;
                    //icmsFileInfo.OperType = "A";
                    page.File_name = xmlfileName;
                    imageInfo[pkuuid].Add(page);
                }
            }
            catch (Exception ex)
            {
                //SysLog.Write(7117, ex, this._proIdAndThreadId);
                throw;
            }
        }

        ///// <summary>
        ///// 取得文档编号
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <returns></returns>
        //private int GetDocId(string pkuuid)
        //{
        //    return this.SelectDocInfoByGuid(this.GetSpecialNode(this._property.BusinessId),
        //        pkuuid);
        //}

        ///// <summary>
        ///// 取得图像编号
        ///// </summary>
        ///// <param name="docId">文档编号</param>
        ///// <param name="page_index">图像索引</param>
        ///// <returns></returns>
        //private int GetPageId(int docId, int pageIndex)
        //{
        //    int id = 0;

        //    try
        //    {
        //        DataRow[] rows = this.DataManage.SelectNodeRows(docId,
        //            "pages",
        //            SupportEnumType.EnumNodeType.Node);

        //        id = this.DataManage.GetIDByIndex((int)rows[0][0], pageIndex);
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7127, ex, this._proIdAndThreadId);
        //    }

        //    return id;
        //}
        /*
        /// <summary>
        /// 取得图像批注的文件名
        /// </summary>
        /// <param name="fileName">图像文件名</param>
        /// <returns>批注文件名</returns>
        private string GetPostilFileName(string fileName)
        {
            return Regex.Replace(fileName, @".[^.]+$", ".atn");
        }
        */
        /// <summary>
        /// 添加pagedel节点
        /// </summary>
        /// <param name="pkuuid">kpuuid</param>
        /// <param name="page_index">图像节点索引</param>
        private void AddImageDeleteNode(string pkuuid, int pageIndex)
        {
            try
            {
                DeletedPage deletedPage = new DeletedPage();
                deletedPage.Pkuuid = pkuuid;
                deletedPage.Page_index = pageIndex;

                ((DeletedPageTable)this.dataServies.GetDataTable(EnumType.TableType.DeletedPageTable)).AddRow(deletedPage);
            }
            catch (Exception ex)
            {
                //SysLog.Write(6143, ex, this._proIdAndThreadId);
            }
        }

        ///// <summary>
        ///// 判断pages节点是否存在，如果存在，但是并没有子节点，则删除此节点
        ///// </summary>
        ///// <param name="docId">文档编号</param>
        //private void CheckHasPagesChild(int docId)
        //{
        //    DataRow[] rows = this.DataManage.SelectNodeRows(docId,
        //        "pages",
        //        SupportEnumType.EnumNodeType.Node);
        //    if (rows.Length <= 0) return;

        //    int pagesId = (int)rows[0]["ID"];
        //    int pagesIndex = (int)rows[0]["Index"];
        //    rows = this.DataManage.GetRowsByType(pagesId, SupportEnumType.EnumNodeType.Node);

        //    if (rows.Length > 0) return;
        //    this.DataManage.Delete(pagesId);
        //    this.DataManage.ReseizeByIndex(docId, SupportEnumType.EnumNodeType.Node, pagesIndex);
        //}

        /// <summary>
        /// 将指定父节点编号的子节点DataRow数组中指定列名的列重新编号(排序并重新从0开始编号)
        /// </summary>
        /// <param name="pkuuid">节点所在的索引</param>
        public void OrderBy(string pkuuid)
        {
            //取得重新排序的page节点数组
            DataRow[] rows = GetOrderNodeArray(pkuuid);


            for (int i = 0; i < rows.Length; i++)
            {
                bool docChange = false;
                bool pageChange = false;
                if ((int)rows[i]["doc_index"] != i + 1)
                {
                    docChange = true;

                }
                if (this._property.WorkType == EnumType.EnumWorkType.InitScan && (int)rows[i]["Page_index"] != i + 1)
                {
                    pageChange = true;
                }
                if (docChange || pageChange)
                {
                    rows[i].BeginEdit();

                    if (docChange)
                    {
                        rows[i]["doc_index"] = i + 1;
                    }

                    if (pageChange)
                    {
                        rows[i]["Page_index"] = i;
                    }

                    this.UpdateModiValue(rows[i], true);

                    try
                    {
                        rows[i].EndEdit();
                    }
                    catch (Exception ex)
                    {
                        //SysLog.Write(7122, ex, this._proIdAndThreadId);
                        throw ex;
                    }
                }
            }
        }

        ///// <summary>
        ///// 检索节点的oper_type属性值
        ///// </summary>
        ///// <param name="nodeId">doc_info节点编号</param>
        //private string GetOperType(int nodeId)
        //{
        //    string value = this.DataManage.SelectNodeValue(nodeId);
        //    return GetNodePropertyValue(value, "oper_type");
        //}

        /// <summary>
        /// 获取按Index索引排序的节点数组
        /// </summary>
        /// <param name="pkuuid"></param>
        /// <returns></returns>
        private DataRow[] GetOrderNodeArray(string pkuuid)
        {
            return this.pageTable.Select("pkuuid = \'" + pkuuid + "\'", "doc_index asc");
            //DataRow[] rows = this.DataManage.GData.Select("[parentID] = " + nodeId);
            //rows = rows.OrderBy(a => int.Parse(a["Index"].ToString())).ToArray();
            //return rows;
        }

        /// <summary>
        /// 将指定父节点编号的子节点DataRow数组中指定列名的列用index占位(所有大于等于docindex的记录都后移一位)
        /// </summary>
        /// <param name="pkuuid">节点的pkuuid</param>
        /// <param name="docindex">占复位索引</param>
        private void SeizeByIndex(string pkuuid, int docindex)
        {
            this.SerialRowsByIndex(pkuuid, docindex, true);
        }

        /// <summary>
        /// 将指定父节点编号的子节点DataRow数组中指定列名的列用index复位(所有大于等于docindex的记录都前移一位)
        /// </summary>
        /// <param name="pkuuid">节点的pkuuid</param>
        /// <param name="docindex">占复位索引</param>
        private void ReseizeByIndex(string pkuuid, int docindex)
        {
            this.SerialRowsByIndex(pkuuid, docindex, false);
        }

        /// <summary>
        /// 将DataRow数组中Index列在index索引处执行占复位操作
        /// </summary>
        /// <param name="pkuuid">节点的pkuuid</param>
        /// <param name="docindex">占复位索引</param>
        /// <param name="isSeize">是否占位</param>
        private void SerialRowsByIndex(string pkuuid, int docindex, bool isSeize)
        {
            DataRow[] pageRows = pageTable.Select("Pkuuid = '" + pkuuid + "' and Doc_index >= " + docindex);
            //int currentIndex = 0;
            //string docOperType = string.Empty;

            try
            {
                //占复位
                foreach (var row in pageRows)
                {

                    row.BeginEdit();


                    //更新doc_index属性
                    row["Doc_index"] = isSeize ? (int)row["Doc_index"] + 1 : (int)row["Doc_index"] - 1;
                    //row["Index"] = isSeize ? ++currentIndex : --currentIndex;


                    if (this._property.WorkType != EnumType.EnumWorkType.InitScan)
                    {
                        ////下载模式，补扫的图像oper_type=A，否则E
                        //docOperType = GetNodePropertyValue(
                        //    row["Value"].ToString(),
                        //    "oper_type");
                        if (!"A".Equals(row["oper_type"]))
                        {
                            row["oper_type"] = "E";
                        }
                    }

                    this.UpdateModiValue(row, true);

                    try
                    {
                        row.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        //SysLog.Write(9135, ex, this._proIdAndThreadId);
                        throw ex;
                    }

                }
            }
            catch (Exception ex)
            {
                //SysLog.Write(7137, ex, this._proIdAndThreadId);
                throw ex;
            }
        }

        /// <summary>
        /// 修改modi_range属性
        /// <param name="drs_page">page行</param>
        /// <param name="isM">是否元数据编辑(这里代表page_info节点本身)</param>
        /// </summary>
        private void UpdateModiValue(DataRow drs_page, bool isM)
        {
            string value = (string)drs_page["modi_range"];
            if (value == "A" || (isM && value == "M") || (!isM && value == "C")) return;
            if ((isM && value == "C") || (!isM && value == "M")) value = "A";
            if (value == "N") value = isM ? "M" : "C";

            drs_page["modi_range"] = value;

            //drs_page["modi_time"] = this.Now; //时间在EndEdit时更改
        }


        /// <summary>
        /// 取得page节点中page_index属性数值最大的属性
        /// </summary>
        /// <param name="pagesId">pages节点编号</param>
        private int GetMaxPageIndex(string pkuuid)
        {
            object pageIndex = ((PageTable)this.dataServies.
                   GetDataTable(EnumType.TableType.PageTable)).Compute("Max(page_index)", "Pkuuid = '" + pkuuid + "'");

            if (pageIndex == System.DBNull.Value)
            {
                return -1;
            }
            else
            {
                return (int)pageIndex;
            }

        }

        /// <summary>
        /// 取得page节点中page_index属性数值最大的属性
        /// </summary>
        /// <param name="pagesId">pages节点编号</param>
        private int GetMaxDocIndex(string pkuuid)
        {
            object docIndex = ((PageTable)this.dataServies.
                   GetDataTable(EnumType.TableType.PageTable)).Compute("Max(doc_index)", "Pkuuid = '" + pkuuid + "'");

            if (docIndex == System.DBNull.Value)
            {
                return 0;
            }
            else
            {
                return (int)docIndex;
            }

        }

        /// <summary>
        /// 在page已被删除节点情形下，进行doc_info节点状态的判断与修改
        /// 描述：
        ///     一.判断新增模式(InitScan),docID是新增的doc_info:
        ///     1.如果此内部的图像没有清空,
        ///     则此doc_info的oper_type不变(A)
        ///     2.如果此内部的图像都已清空，
        ///     则将此doc_info的oper_type变成I(表示不上传)；
        ///     二.判断下载模式(SupplyScan、RenewScan),docID是否新增的doc_info:
        ///     1.如果是并且此内部的图像都已清空,
        ///         则将此doc_info的oper_type变成I(表示不上传)；
        ///     2.如果是并且此内部的图像没有清空，
        ///         则此doc_info的oper_type不变(A)；
        ///     3.如果否
        ///         则此doc_info的oper_type变成E(表示修改)；
        /// </summary>
        /// <param name="docId">文档节点编号</param>
        /// <param name="pagesId">pages_info节点编号</param>
        private void UpdateDocOperTypeByDelPage(string pkuuid)
        {

            DataRow docRow = this.dataServies.GetRowByKey(EnumType.TableType.DocTable, pkuuid);

            string operType = string.Empty;

            if (this._property.WorkType == EnumType.EnumWorkType.InitScan)
            {
                //oper_type = !hasPageNode ? "I" : "A";
                operType = "A";
            }
            else
            {
                if ("A".Equals(docRow["oper_type"]))
                {
                    //oper_type = !hasPageNode ? "I" : "A";
                    operType = "A";
                }
                else
                {
                    operType = "E";
                }
            }

            docRow["oper_type"] = operType;
            docRow["modi_time"] = this.Now;

        }


        private void UpdateDocOperType(string pkuuid, string oper_type)
        {

            DataRow docRow = this.dataServies.GetRowByKey(EnumType.TableType.DocTable, pkuuid);

            if (!"A".Equals(docRow["oper_type"]))
            {
                docRow["oper_type"] = oper_type;
            }

            docRow["modi_time"] = this.Now;

        }
        /// <summary>
        /// 判断是否需要添加pagedel节点,如果是，则进行添加
        /// </summary>
        /// <param name="pageRow">文档节点</param>
        /// <param name="isNewAddPage">是否新增page</param>
        private void CheckAddImageDeleteNode(DataRow pageRow, bool isNewAddPage)
        {
            if (this._property.WorkType == EnumType.EnumWorkType.InitScan) return;


            if (!isNewAddPage && !"A".Equals(pageRow["oper_type"])) //非新增节点，需要更新delpage
            {
                this.AddImageDeleteNode((string)pageRow["Pkuuid"], (int)pageRow["Page_index"]);
            }

        }

        ///// <summary>
        ///// 判断指定的docID是否是新增的Doc
        ///// </summary>
        ///// <param name="docId">文档编号</param>
        //private bool CheckDocIsAdd(int docId)
        //{
        //    if (this.GetNodeProperty(docId, "oper_type") == "A")
        //        return true;
        //    return false;
        //}

        /// <summary>
        /// 在新page添加节点情形下，进行doc_info节点状态的判断与修改
        /// </summary>
        /// <param name="pkuuid">文档编号</param>
        private void UpdateDocOperTypeByAddPage(string pkuuid)
        {
            UpdateDocOperType(pkuuid, "E");
        }

        /// <summary>
        /// 获取pages下面的所有的page
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        private DataRow[] GetPageArray(string pkuuid)
        {
            return this.pageTable.Select("pkuuid = \'" + pkuuid + "\'");
        }

        ///// <summary>
        ///// 重新编号Page节点正反面的doc_index
        ///// </summary>
        ///// <param name="id">父节点编号</param>
        //private void AnewNumberAllDocIndex(int id)
        //{
        //    DataRow[] rows = this.DataManage.GetRowsByType(id, SupportEnumType.EnumNodeType.Node);
        //    IEnumerable<DataRow> ienumRow = rows.Where(a => a["Child"].Equals("page"));
        //    if (ienumRow.Count() <= 0)
        //    {
        //        foreach (var row in rows)
        //        {
        //            this.AnewNumberAllDocIndex(int.Parse(row[0].ToString()));
        //        }
        //    }
        //    else
        //    {
        //        var sortRows = from row in ienumRow
        //                       orderby int.Parse(GetNodePropertyValue(
        //                       row["Value"].ToString(),
        //                       "doc_index")) ascending,
        //                       GetNodePropertyValue(
        //                       row["Value"].ToString(),
        //                       "page_flag") descending
        //                       select row;
        //        int i = 0;
        //        sortRows.ToList().ForEach(a =>
        //        {
        //            a.BeginEdit();
        //            a["Index"] = i.ToString();
        //            a["Value"] = XmlOperate.UpdateNodeProperty(
        //                a["Value"].ToString(),
        //                "doc_index",
        //                (++i).ToString());
        //            //a["Value"] = XMLOperate.UpdateNodeProperty(
        //            //   a["Value"].ToString(),
        //            //   "old_doc_index",
        //            //   i.ToString());
        //            try
        //            {
        //                a.EndEdit();
        //            }
        //            catch (Exception ex)
        //            {
        //                SysLog.Write(9136, ex, this._proIdAndThreadId);
        //                throw ex;
        //            }
        //        });
        //    }
        //}
        #endregion

        #region 新增方法
        /// <summary>
        /// 增加图像节点（注：确保xml中pageIndex/docIndex等的准确性）
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="pageXml">xml字符串</param>
        public bool AddImageByXml(string pkuuid, string pageXml, string realName)
        {
            try
            {
                string fileName = GetNodePropertyValue(pageXml, "file_name");
                string xmlfileName = this.GetTransName(fileName);

                pageXml = UpdateNodeProperty(pageXml, "file_name", xmlfileName);
                Page page = this._ixmlfe.AddOnePage(pageXml, pkuuid, realName);

                /* 所有的数据均从xml中取得，故保持xml数据准确性即可，暂不在内部调整
                //遍历每一个page
                foreach (DataRow row in pages)
                {
                    //每一个page的index如果大于等于新添加的page的index，需要后延一位
                    if (int.Parse(row["Index"].ToString()) >= pageIndex)
                    {
                        row.BeginEdit();
                        row["Index"] = (int)row["Index"] + 1;

                        int docIndex = (int)row["Index"] + 1;
                        this.UpdateNodePropertyAndTime(int.Parse(row["ID"].ToString()), "doc_index", docIndex.ToString());

                        try
                        {
                            row.EndEdit();
                        }
                        catch (Exception ex)
                        {
                            SysLog.Write(9137, ex, this._proIdAndThreadId);
                            throw ex;
                        }
                    }
                }

                //添加新page
                this.DataManage.AddRow(pagesId, "page", pageXml, SupportEnumType.EnumNodeType.Node, pageIndex);
                */
                //改变的doc的oper_type
                //this.UpdateNodePropertyAndTime(docId, "oper_type", "A");

                this.UpdateDocOperTypeByAddPage(pkuuid);

                //if (imageInfo.Count == 0)
                if (this.ImageInfo.Count == 0)
                {
                    imageInfo = GetAllImageInfo();
                    _ifGetAllImageInfo = false;
                }
                else
                {
                    ////对特殊字符进行处理
                    //IcmsFileInfo icmsFileInfo = new IcmsFileInfo();
                    //icmsFileInfo.FileName = fileName;
                    //icmsFileInfo.DocIndex = page.Doc_index;
                    //icmsFileInfo.PageIndex = page.Page_index;
                    //icmsFileInfo.OperType = "A";
                    page.File_name = xmlfileName;
                    imageInfo[pkuuid].Add(page);
                }
                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(7117, ex, this._proIdAndThreadId);
                this.dataServies.RollBack();
                return false;
            }
        }

        ///// <summary>
        ///// 重扫补扫时获取整个业务图像信息<!--<pkuuid, <page index, file name>>-->
        ///// </summary>
        ///// <returns>返回结构(文档索引, (图像索引, 图像名)) </returns>
        //public IDictionary<string, Dictionary<int, string>> GetImageOrder()
        //{
        //    //保存整个file的信息(文档索引, (图像索引, 图像名))
        //    IDictionary<string, Dictionary<int, string>> fileInfo = new Dictionary<string, Dictionary<int, string>>();
        //    //保存每个page的信息(图像索引, 图像名)
        //    Dictionary<int, string> pageInfo = null;

        //    try
        //    {
        //        //取得docs_info节点编号
        //        int docsInfoId = this.GetSpecialNode(this._property.BusinessId);
        //        //获取docs_info下面的所有的doc
        //        DataRow[] docs = this.DataManage.GetRowsByType(this.GetSpecialNode(this._property.BusinessId), SupportEnumType.EnumNodeType.Node);

        //        //遍历每个doc
        //        foreach (DataRow row in docs)
        //        {
        //            //pages节点编号
        //            DataRow[] pagedata = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
        //                "pages", SupportEnumType.EnumNodeType.Node);
        //            if (pagedata.Length > 0)
        //            {
        //                int pagesId = int.Parse(pagedata[0]["ID"].ToString());

        //                string pkuuid = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
        //                    "pkuuid", SupportEnumType.EnumNodeType.Control)[0]["Value"].ToString();
        //                //初始化对象
        //                pageInfo = new Dictionary<int, string>();

        //                //获取pages下面的page
        //                DataRow[] rows = this.DataManage.GetRowsByType(pagesId, SupportEnumType.EnumNodeType.Node);

        //                if (rows.Length > 0)
        //                {
        //                    //对每个page按index排序，并添加到pageInfo对象里面
        //                    rows.OrderBy(a =>
        //                        int.Parse(GetNodePropertyValue(
        //                        a["value"].ToString(),
        //                        "doc_index")))
        //                        .ToList().ForEach(a =>
        //                        {
        //                            //string fileName = XMLOperate.GetNodePropertyValue(a["Value"].ToString(), "file_name");
        //                            //pageInfo.Add((int)a["Index"], fileName);

        //                            string fileName = GetNodePropertyValue(a["Value"].ToString(), "file_name");
        //                            string pageIndex = GetNodePropertyValue(a["Value"].ToString(), "page_index");

        //                            //对特殊字符进行处理
        //                            string nFileName = this.TransNameToFileName(fileName);
        //                            pageInfo.Add(int.Parse(pageIndex), nFileName);
        //                            //pageInfo.Add(int.Parse(pageIndex), fileName);                                
        //                        });
        //                }

        //                //将整个page信息添加到fileInfo对象里面
        //                fileInfo.Add(pkuuid, pageInfo);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7128, ex, this._proIdAndThreadId);
        //    }

        //    return fileInfo;
        //}


        /// <summary>
        /// 重扫补扫时获取整个业务图像信息和版本信息<!--<doc index, <page index, file name>>-->
        /// </summary>
        /// <param name="docFileInfo">图像信息</param>
        /// <param name="hizFileInfo">版本信息</param> 
        public void GetImageOrderAndHizVersion(ref IDictionary<string, List<string>> hizFileInfo)
        {
            //保存整个file的信息(文档索引, (图像索引, 图像名))
            //docFileInfo = new Dictionary<string, Dictionary<int, string>>();
            hizFileInfo = new Dictionary<string, List<string>>();


            try
            {
                //获取docs_info下面的所有的pkuuids
                var pkuuids = this.dataServies.GetDataTable(EnumType.TableType.DocTable).
                    AsEnumerable().Select(c => c.Field<string>("Pkuuid")).ToList();

                //遍历每个doc
                foreach (string pkuuid in pkuuids)
                {
                    ////pages节点编号
                    //DataRow[] pagedata = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
                    //    "pages", SupportEnumType.EnumNodeType.Node);
                    //string pkuuid = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
                    //        "pkuuid", SupportEnumType.EnumNodeType.Control)[0]["Value"].ToString();
                    /*
                    if (pagedata.Length > 0)
                    {
                        int pagesID = int.Parse(pagedata[0]["ID"].ToString());

                        
                        //初始化对象
                        pageInfo = new Dictionary<int, string>();

                        //获取pages下面的page
                        DataRow[] rows = dataManage.GetRowsByType(pagesID, EnumType.EnumNodeType.Node);

                        if (rows.Length > 0)
                        {
                            //对每个page按index排序，并添加到pageInfo对象里面
                            rows.OrderBy(a =>
                                int.Parse(XMLOperate.GetNodePropertyValue(
                                a["value"].ToString(),
                                "doc_index")))
                                .ToList().ForEach(a =>
                                {
                                    //string fileName = XMLOperate.GetNodePropertyValue(a["Value"].ToString(), "file_name");
                                    //pageInfo.Add((int)a["Index"], fileName);

                                    string fileName = XMLOperate.GetNodePropertyValue(a["Value"].ToString(), "file_name");
                                    string pageIndex = XmlNodeOperate.GetNodePropertyValue(a["Value"].ToString(), "page_index");

                                    //对特殊字符进行处理
                                    string NFileName = fileName.Replace("&amp;", "&");
                                    NFileName = NFileName.Replace("&lt;", "<");
                                    NFileName = NFileName.Replace("&gt;", ">");
                                    NFileName = NFileName.Replace("&quot;", "\"");
                                    pageInfo.Add(int.Parse(pageIndex), NFileName);
                                    //pageInfo.Add(int.Parse(pageIndex), fileName);                                
                                });
                        }

                        //将整个page信息添加到fileInfo对象里面
                        docFileInfo.Add(pkuuid, pageInfo);
                    }
                    */

                    DataRow[] hizVersionDatas = this.dataServies.GetDataTable(EnumType.TableType.VersionTable).Select("pkuuid = \'" + pkuuid + "\'", "ver_no asc");
                    List<string> versionList = new List<string>();
                    if (hizVersionDatas.Length > 0)
                    {
                        foreach (DataRow hizVersionData in hizVersionDatas)
                        {
                            versionList.Add((string)hizVersionData["ver_no"]);
                        }

                        hizFileInfo.Add(pkuuid, versionList);
                    }

                }
            }
            catch (Exception ex)
            {
                //SysLog.Write(7128, ex, this._proIdAndThreadId);
            }
        }

        /// <summary>
        /// 获取改变的文件名
        /// </summary>
        /// <returns></returns>
        public Collection<string> GetChangedFileNames()
        {
            //用来保存文件名
            Collection<string> fileNames = new Collection<string>();

            try
            {

                //遍历整个page表
                foreach (DataRow row in this.pageTable.Rows)
                {

                    #region 因为后台不能在没有批注文件的情况下只更新批注文件面，所以oper_type为E或者A的批注文件都要上传
                    //如果是postil节点
                    string postil = (string)row["PostilInfo"];
                    if (!string.IsNullOrEmpty(postil))
                    {
                        //获取oper_type属性
                        string postiloperType = GetNodePropertyValue(postil, "oper_type");
                        //如果是空或者忽略，继续找下一个
                        if (postiloperType == string.Empty || postiloperType == "I")
                        {
                            continue;
                        }
                        //如果是改变的（增删改）文件名，提取出来
                        string postilfileName = GetNodePropertyValue(postil, "file_name");
                        //处理特殊字符
                        string postilnewFileName = this.TransNameToFileName(postilfileName);

                        if (!fileNames.Contains(postilnewFileName))
                        {
                            fileNames.Add(postilnewFileName);
                        }
                        //fileNames.Add(fileName);
                    }
                    #endregion

                    //获取oper_type属性
                    string operType = (string)row["oper_type"];
                    string modiRange = (string)row["modi_range"];
                    //如果是空或者忽略，继续找下一个
                    if (operType == string.Empty || operType == "I" || modiRange == "M")
                    {
                        continue;
                    }
                    //如果是改变的（增删改）文件名，提取出来
                    string fileName = (string)row["file_name"];
                    //处理特殊字符
                    string newFileName = this.TransNameToFileName(fileName);

                    if (!fileNames.Contains(newFileName))
                    {
                        fileNames.Add(newFileName);
                    }

                }
            }
            catch (Exception ex)
            {
                //SysLog.Write(6144, ex, this._proIdAndThreadId);
            }

            return fileNames;
        }

        ///// <summary>
        ///// 获取Control类型的子节点信息
        ///// </summary>
        ///// <param name="pkuuid">文档唯一标识</param>
        ///// <param name="nodeNames">子节点名称</param>
        ///// <returns>子节点名称跟子节点值的组合</returns>
        //public IDictionary<string, string> GetChildNodeValue(string pkuuid, string[] nodeNames)
        //{
        //    IDictionary<string, string> controlInfo = new Dictionary<string, string>();

        //    //取得docs_info节点编号
        //    int docsInfoId = this.GetSpecialNode(this._property.BusinessId);

        //    //取得doc_info节点编号
        //    int docId = this.GetDocId(pkuuid);

        //    DataRow[] controlRows = this.DataManage.GetRowsByType(docId, SupportEnumType.EnumNodeType.Control);

        //    foreach (DataRow row in controlRows)
        //    {
        //        if (nodeNames.Contains(row["Child"].ToString()))
        //        {
        //            controlInfo.Add(row["Child"].ToString(), row["Value"].ToString());
        //        }
        //    }

        //    return controlInfo;
        //}

        ///// <summary>
        ///// 根据pkuuid从docs_info里面查找doc_info
        ///// </summary>
        ///// <param name="docs_infoID">docs_info编号</param>
        ///// <param name="pkuuid">pkuuid</param>
        //private int SelectDocInfoByGuid(int docsInfoId, string pkuuid)
        //{
        //    //枚举docs_info下面的所有的doc_info
        //    DataRow[] rows = this.DataManage.GetRowsByType(docsInfoId,
        //        SupportEnumType.EnumNodeType.Node);

        //    int nodeId = 0;
        //    foreach (var row in rows)
        //    {
        //        //根据pkuuid查找doc_info
        //        nodeId = (int)row["ID"];
        //        DataRow[] itemRows = this.DataManage.SelectNodeRows(nodeId,
        //             "pkuuid",
        //             SupportEnumType.EnumNodeType.Control);

        //        if (itemRows.Length > 0 &&
        //            itemRows[0]["Value"].Equals(pkuuid))
        //            break;
        //    }

        //    return nodeId;
        //}

        /// <summary>
        /// 获取page节点属性
        /// </summary>
        /// <param name="realname">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        public string GetPageAttribute(string realname, string attributeName)
        {

            string attributeValue = "";

            //根据realName 查找行
            DataRow pageRow = pageTable.Rows.Find(realname);

            if (pageRow != null) //
            {
                attributeValue = pageRow[attributeName] == null ? "" : pageRow[attributeName] + "";
            }

            return attributeValue;
        }

        /// <summary>
        /// 获取pkuuid下所有page节点属性
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        /// <returns>返回realname为key，属性值为value的字典</returns>
        public Dictionary<string, string> GetPageAttributes(string pkuuid, string attributeName)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            DataRow[] pageArray = this.GetPageArray(pkuuid);

            //遍历每一个page
            foreach (DataRow row in pageArray)
            {
                values.Add((string)row["Realname"], (string)row[attributeName]);
            }

            return values;
        }

        /// <summary>
        /// page排序后修改pkuuid下文件的pageIndex
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="newPagesIndex">文件要设置的Index字典(文件名，pageIndex)</param>
        /// <returns></returns>
        public bool UpdatePagesIndex(string pkuuid, Dictionary<string, int> newPagesIndex)
        {
            bool isChanged = false;

            try
            {

                DataRow[] pageArray = this.pageTable.Select("pkuuid = \'" + pkuuid + "\'");

                //遍历每一个page
                foreach (DataRow row in pageArray)
                {
                    string fileName = (string)row["realname"];

                    //把xml带转译的&符号转换成正常的&符号
                    fileName = TransNameToFileName(fileName);

                    //如果文件名包含在修改的字典中
                    if (newPagesIndex.ContainsKey(fileName))
                    {
                        //检查节点index变化后才做修改
                        if ((int)row["doc_index"] != newPagesIndex[fileName] + 1)
                        {
                            row.BeginEdit();
                            //row["Index"] = newPagesIndex[fileName];

                            //同步doc_index属性
                            row["doc_index"] = newPagesIndex[fileName] + 1;


                            //图像oper_type=I，则改为E
                            if ("I".Equals(row["oper_type"]))
                            {
                                row["oper_type"] = "E";

                            }

                            ////修改更新时间 以及移到EndEdit进行
                            //row["Value"] = XmlOperate.UpdateNodeProperty(
                            //        row["Value"].ToString(),
                            //        "modi_time",
                            //        DateTime.Now.ToString("s") + "Z");

                            this.UpdateModiValue(row, true);
                            try
                            {
                                row.EndEdit();
                            }
                            catch (Exception ex)
                            {
                                //SysLog.Write(9135, ex, this._proIdAndThreadId);
                                throw;
                            }


                            isChanged = true;
                        }
                    }


                }


                //新增模式或者下载模式的补扫图像的情况，
                //page_info节点old_doc_index=0、oper_type=A
                //UpdateNodePropertyAndTime(old_pageID,
                //    "oper_type",
                //    old_doc_index == 0 ? "A" : "E");
                if (isChanged)
                {
                    //修改目标节点doc_info的oper_type状态：
                    //新增模式oper_type=A，
                    //下载模式拖动下载的图像的情况，oper_type=E
                    //下载模式拖动补扫的图像的情况，oper_type=A
                    this.UpdateDocOperTypeByAddPage(pkuuid);
                    DataRow docRow = this.dataServies.GetRowByKey(EnumType.TableType.DocTable, pkuuid);
                    docRow["modi_meta"] = "Y";
                    //this.DataManage.UpdateValue(docId,
                    //  XmlOperate.UpdateNodeProperty(this.DataManage.SelectNodeValue(docId), "modi_meta", "Y"));
                }

                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(9138, ex, this._proIdAndThreadId);
                this.dataServies.RollBack();
                return false;
            }
        }

        /// <summary>
        /// 获取Version节点属性
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="attributeName">属性名</param>
        public string GetVersionAttribute(string pkuuid, string attributeName)
        {
            string attributeValue = "";
            ////取得文档编号
            //int docId = this.GetDocId(pkuuid);

            ////version节点编号
            //int versionId = this.CheckNode(docId, "version_list");

            //if (versionId == -1)
            //{
            //    return "";
            //}

            DataRow[] verArr = this.dataServies.GetDataTable(EnumType.TableType.VersionTable).Select("pkuuid = \'" + pkuuid + "\'", "ver_no desc");

            if (verArr.Length > 0)
            {
                attributeValue = (string)verArr[0][attributeName];
            }

            return attributeValue;
        }

        ////2019-6-19 ljf
        ///// <summary>
        ///// 获取data_type节点属性
        ///// </summary>
        ///// <param name="pkuuid"></param>
        ///// <param name="attributeName"></param>
        ///// <returns></returns>
        //public string GetDataTypeAttribute(string pkuuid, string attributeName)
        //{
        //    string attributeValue = "";
        //    //取得文档编号

        //    int docId = this.GetSpecialNode(this._property.BusinessId);
        //    DataRow[] rows = this.DataManage.GetRowsByType(docId,
        //        SupportEnumType.EnumNodeType.Node);
        //    int nodeId = 0;
        //    foreach (var row in rows)
        //    {
        //        //根据pkuuid查找doc_info
        //        nodeId = (int)row["ID"];
        //        DataRow[] itemRows = this.DataManage.SelectNodeRows(nodeId,
        //             "pkuuid",
        //             SupportEnumType.EnumNodeType.Control);

        //        if (itemRows.Length > 0 &&
        //            itemRows[0]["Value"].Equals(pkuuid))
        //        {
        //            string mastr= "data_type=\"";
        //            string xml = row["Value"].ToString();
        //            attributeValue=xml.Substring(xml.IndexOf(mastr)+mastr.Length,4);


        //            break;
        //        }

        //    }

        //    return attributeValue;
        //}

        ///// <summary>
        ///// 重扫补扫时获取整个业务图像信息<!--<pkuuid, filename>-->
        ///// </summary>
        ///// <returns>返回结构(文档pkuid, 图像名)</returns>
        //public IDictionary<string, string[]> GetImageList()
        //{
        //    IDictionary<string, string[]> imageDic = new Dictionary<string, string[]>();
        //    List<string> fileList = new List<string>();

        //    try
        //    {
        //        //取得docs_info节点编号
        //        int docsInfoId = this.GetSpecialNode(this._property.BusinessId);
        //        //获取docs_info下面的所有的doc
        //        DataRow[] docs = this.DataManage.GetRowsByType(this.GetSpecialNode(this._property.BusinessId), SupportEnumType.EnumNodeType.Node);

        //        //遍历每个doc
        //        foreach (DataRow row in docs)
        //        {
        //            string pkuuid = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
        //                    "pkuuid", SupportEnumType.EnumNodeType.Control)[0]["Value"].ToString();

        //            fileList = new List<string>();

        //            //pages节点编号
        //            DataRow[] pagedate = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
        //                "pages", SupportEnumType.EnumNodeType.Node);
        //            if (pagedate.Length > 0)
        //            {
        //                int pagesId = int.Parse(this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
        //                "pages", SupportEnumType.EnumNodeType.Node)[0]["ID"].ToString());

        //                //获取pages下面的page
        //                DataRow[] rows = this.DataManage.GetRowsByType(pagesId, SupportEnumType.EnumNodeType.Node);

        //                if (rows.Length <= 0)
        //                {
        //                    imageDic.Add(pkuuid, fileList.ToArray());
        //                    continue;
        //                }

        //                //对每个page按index排序，并添加到pageInfo对象里面
        //                rows.OrderBy(a => int.Parse(GetNodePropertyValue(
        //                    a["Value"].ToString(),
        //                    "doc_index")))
        //                    .ToList().ForEach(a =>
        //                    {
        //                        string fileName = GetNodePropertyValue(
        //                            a["Value"].ToString(),
        //                            "file_name");

        //                        //对特殊字符进行处理
        //                        string nFileName = this.TransNameToFileName(fileName);
        //                        //fileList.Add(fileName);
        //                        fileList.Add(nFileName);
        //                    });

        //                //将整个page信息添加到fileInfo对象里面
        //                imageDic.Add(pkuuid, fileList.ToArray());
        //            }
        //            else
        //            {
        //                imageDic.Add(pkuuid, fileList.ToArray());
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(7128, ex, this._proIdAndThreadId);
        //    }

        //    return imageDic;
        //}

        /// <summary>
        /// 图像正反面排序
        /// 描述：将pkuuid所在的对象下面的所有图像执行正反排序
        ///       即1F1B2F2B...
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <returns></returns>
        public bool OrderByPageFlag(string pkuuid)
        {
            bool frontFlag = false;

            try
            {

                DataRow[] pageArray = GetOrderNodeArray(pkuuid);

                for (int i = 0; i < pageArray.Length; i++)
                {
                    frontFlag = !frontFlag;
                    pageArray[i].BeginEdit();
                    pageArray[i]["page_flag"] = frontFlag ? "F" : "B";

                    try
                    {
                        pageArray[i].EndEdit();
                    }
                    catch (Exception ex)
                    {
                        //SysLog.Write(7147, ex, this._proIdAndThreadId);
                        throw;
                    }
                }

                this.dataServies.Commit();
                return true;
            }
            catch
            {
                this.dataServies.RollBack();
                return false;
            }
        }

        /// <summary>
        /// 全部图像正反面排序
        /// 描述：处理正反面相同的doc_index，
        /// 将相同正反面doc_index的值设置一致，
        /// 第二个page的doc_index等于第一个page的doc_index
        /// </summary>
        public bool OrderByPageFlagAll()
        {
            //获取docs_info下面的所有的pkuuids
            var pkuuids = this.dataServies.GetDataTable(EnumType.TableType.DocTable).
                AsEnumerable().Select(c => c.Field<string>("Pkuuid")).ToList();
            if (pkuuids.Count() <= 0) return true;
            try
            {
                foreach (var pkuuid in pkuuids)
                {
                    this.OrderByPageFlag1(pkuuid);
                }

                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(9138, ex, this._proIdAndThreadId);
                this.dataServies.RollBack();
                return false;
            }
        }

        /// <summary>
        /// 图像正反面排序
        /// 描述：处理正反面相同的doc_index，
        /// 将相同正反面doc_index的值设置一致，
        /// 第二个page的doc_index等于第一个page的doc_index
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        public void OrderByPageFlag1(string pkuuid)
        {
            string pageFlag = string.Empty;
            string oldPageFlag = string.Empty;
            int oldDocIndex = 0;
            bool isCheck = false;

            try
            {
                DataRow[] pageArray = GetOrderNodeArray(pkuuid);
                for (int i = 0; i < pageArray.Length; i++)
                {
                    pageFlag = (string)pageArray[i]["page_flag"];
                    if (i > 0 &&
                        !isCheck &&
                        pageFlag != oldPageFlag &&
                        oldPageFlag == "F")
                    {
                        pageArray[i].BeginEdit();
                        pageArray[i]["doc_index"] = oldDocIndex;
                        try
                        {
                            pageArray[i].EndEdit();
                        }
                        catch (Exception ex)
                        {
                            //SysLog.Write(7147, ex, this._proIdAndThreadId);
                            throw;
                        }

                        isCheck = true;
                    }
                    else
                    {
                        pageArray[i].BeginEdit();
                        pageArray[i]["doc_index"] = ++oldDocIndex;
                        try
                        {
                            pageArray[i].EndEdit();
                        }
                        catch (Exception ex)
                        {
                            //SysLog.Write(7147, ex, this._proIdAndThreadId);
                            throw;
                        }

                        oldPageFlag = pageFlag;
                        if (i > 0) isCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {
                //SysLog.Write(9138, ex, this._proIdAndThreadId);
                throw ex;
            }
        }

        ///// <summary>
        ///// 处理下载模式正反面标志对应的DocIndex属性相同
        ///// 描述：目前下载模式，page的排序是按page_index索引顺序排列
        /////       处理后要按Doc_index与正反面的顺序排序
        ///// </summary>
        //public bool ProccessDocIndexByPageFlag()
        //{
        //    try
        //    {
        //        this.AnewNumberAllDocIndex(this._ixmlfe.ParentID);
        //        this.DataManage.Commit();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        SysLog.Write(9139, ex, this._proIdAndThreadId);
        //        this.DataManage.RollBack();
        //        return false;
        //    }
        //}

        /// <summary>
        /// 在修改图像之后，更新modi_range属性
        /// </summary>
        /// <param name="realName">文档唯一标识</param>
        public bool UpdateModiRangeByEditImage(string realName)
        {
            try
            {
                //根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);

                if (pageRow != null)
                {

                    pageRow.BeginEdit();
                    if (!"A".Equals(pageRow["oper_type"]))
                    {
                        pageRow["oper_type"] = "E";
                    }

                    //修改Mode_Range
                    this.UpdateModiValue(pageRow, false);

                    pageRow.EndEdit();

                    //修改doc_info状态
                    this.UpdateDocOperType((string)pageRow["Pkuuid"], "E");


                }

                //if (Property.Instance.WorkType != EnumType.EnumWorkType.InitScan)
                //{
                //    UpdateNodeProperty(docID, "oper_type", "E");
                //    if (GetNodeProperty(pageID, "oper_type") != "A")
                //        UpdateNodeProperty(pageID, "oper_type", "E");
                //}



                this.dataServies.Commit();
                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(9140, ex, this._proIdAndThreadId);
                this.dataServies.RollBack();
                return false;
            }
        }

        /*/// <summary>
        /// 切换图像，更改图像的正反面
        /// </summary>
        /// <param name="pkuuid">文档唯一标识</param>
        /// <param name="page_index">图像索引</param>
        public void ChangeImageNode(string realName, string laguage = "CN")
        {
            try
            {
                string lbText = "正反面：";
                string front = "正";
                string back = "反";
                string btnText = "确定";
                switch (laguage)
                {
                    case "CN":
                        lbText = "正反面：";
                        front = "正";
                        back = "反";
                        btnText = "确定";
                        break;
                    case "ZH":
                        lbText = "正反面：";
                        front = "正";
                        back = "反";
                        btnText = "確定";
                        break;
                    case "EN":
                        lbText = "Front Or Back:";
                        front = "Front";
                        back = "Back";
                        btnText = "Confirm";
                        break;
                }
                //根据realName 查找行
                DataRow pageRow = pageTable.Rows.Find(realName);
                if (pageRow != null)
                {
                    List<Control> controlArray = new List<Control>();
                    FlowLayoutPanel fPanel = new FlowLayoutPanel();
                    fPanel.FlowDirection = FlowDirection.TopDown;
                    fPanel.Top = 20;

                    //添加正反控件
                    Label label = new Label();
                    label.Text = lbText;
                    controlArray.Add(label);

                    ComboBox cbb = new ComboBox();
                    cbb.Name = "PageFlag_ComboBox";
                    cbb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cbb.Items.AddRange(new[] { front, back });
                    string pageFlag = (string)pageRow["page_flag"];
                    cbb.SelectedItem = pageFlag.Equals("F") ? front : back;
                    controlArray.Add(cbb);

                    Button btn = new Button();
                    btn.Text = btnText;
                    btn.Tag = cbb.Name;
                    btn.Click += (object sender, EventArgs e) =>
                    {
                        pageFlag = cbb.SelectedItem.Equals(front) ? "F" : "B";

                        pageRow.BeginEdit();

                        pageRow["page_flag"] = pageFlag;

                        //新增的图像同时更改page_flag、old_page_flag
                        if ("A".Equals(pageRow["oper_type"]))
                        {
                            pageRow["old_page_flag"] = pageFlag;
                        }

                        this.UpdateModiValue(pageRow, true);

                        pageRow.EndEdit();
                        //提示消息
                        string msg = SysLog.GetMessage(9100);
                        if (this.TextValidated != null)
                        {
                            this.TextValidated(msg);
                        }
                        else
                        {
                            MessageBox.Show(msg,
                              SysLog.GetMessage(9901),
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
                            SysLog.Write(9100, this._proIdAndThreadId, msg);
                        }
                    };

                    controlArray.Add(btn);
                    //this._control.Controls.Clear();
                    fPanel.Controls.AddRange(controlArray.ToArray());
                    //this._control.Controls.Add(fPanel);
                    this.dataServies.Commit();
                }


            }
            catch (Exception ex)
            {
                this.dataServies.RollBack();
                
                SysLog.Write(7150, ex, this._proIdAndThreadId);
                throw;
            }
        }*/

        ///// <summary>
        ///// 判断在同一个对象内部是否已存在指定的文件名
        ///// </summary>
        ///// <param name="fileName">文件名</param>
        //public bool IsExistsFileName(string fileName)
        //{
        //    fileName = this.GetTransName(fileName);
        //    return this.DataManage.GData.Rows
        //        .OfType<DataRow>()
        //        .Any(a =>
        //            GetNodePropertyValue(a["Value"].ToString(),
        //            "file_name") == fileName);
        //}

        ///// <summary>
        ///// 根据pkuuid跟page_index获取对应的批注文件名
        ///// </summary>
        ///// <param name="pkuuid"></param>
        ///// <param name="page_index"></param>
        ///// <returns>批注文件名</returns>
        //public string GetPostName(string pkuuid, int pageIndex)
        //{
        //    string postilName = "";

        //    if (string.IsNullOrEmpty(pkuuid))
        //    {
        //        return postilName;
        //    }

        //    int docId = this.GetDocId(pkuuid);
        //    int pageId = this.GetPageId(docId, pageIndex);

        //    DataRow[] rows = this.DataManage.SelectNodeRows(pageId,
        //            "postil",
        //            SupportEnumType.EnumNodeType.Node);

        //    if (rows.Length > 0)
        //    {
        //        postilName = GetNodePropertyValue(rows[0]["Value"].ToString(), "file_name");
        //    }

        //    return TransNameToFileName(postilName);
        //}

        /// <summary>
        /// 根据pkuuid跟page_index获取对应的批注文件名
        /// </summary>
        /// <param name="realName"></param>
        /// <returns>批注文件名</returns>
        public string GetPostName(string realName)
        {
            string postilName = "";
            //根据realName 查找行
            DataRow pageRow = pageTable.Rows.Find(realName);

            if (pageRow != null && !string.IsNullOrEmpty((string)pageRow["PostilInfo"]))
            {

                postilName = GetNodePropertyValue((string)pageRow["PostilInfo"], "file_name");

            }

            return TransNameToFileName(postilName);
        }

        public Dictionary<string, List<Page>> GetAllImageInfo()
        {
            imageInfo.Clear();

            try
            {
                //获取docs_info下面的所有的pkuuids
                var pkuuids = this.dataServies.GetDataTable(EnumType.TableType.DocTable).
                    AsEnumerable().Select(c => c.Field<string>("Pkuuid")).ToList();

                //遍历每个doc
                foreach (string pkuuid in pkuuids)
                {
                    List<Page> icmsFileInfoList = this.pageTable.Find("pkuuid = \'" + pkuuid + "\'", "doc_index asc").ToList();
                    ////pages节点编号
                    //DataRow[] pagedate = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
                    //    "pages", SupportEnumType.EnumNodeType.Node);

                    //string pkuuid = this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
                    //        "pkuuid", SupportEnumType.EnumNodeType.Control)[0]["Value"].ToString();

                    //if (pagedate.Length > 0)
                    //{
                    //    int pagesId = int.Parse(this.DataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
                    //    "pages", SupportEnumType.EnumNodeType.Node)[0]["ID"].ToString());

                    //    //获取pages下面的page
                    //    DataRow[] rows = this.DataManage.GetRowsByType(pagesId, SupportEnumType.EnumNodeType.Node);

                    //    if (rows.Length <= 0)
                    //    {
                    //        imageInfo.Add(pkuuid, icmsFileInfoList);
                    //        continue;
                    //    }

                    //    //对每个page按index排序，并添加到pageInfo对象里面
                    //    rows.OrderBy(a => int.Parse(GetNodePropertyValue(
                    //        a["Value"].ToString(),
                    //        "doc_index")))
                    //        .ToList().ForEach(a =>
                    //        {
                    //            string fileName = GetNodePropertyValue(
                    //                a["Value"].ToString(),
                    //                "file_name");

                    //            //对特殊字符进行处理
                    //            string nFileName = this.TransNameToFileName(fileName);

                    //            IcmsFileInfo icmsFileInfo = new IcmsFileInfo();
                    //            icmsFileInfo.FileName = nFileName;
                    //            icmsFileInfo.DocIndex = int.Parse(GetNodePropertyValue(a["Value"].ToString(), "doc_index"));
                    //            icmsFileInfo.PageIndex = int.Parse(GetNodePropertyValue(a["Value"].ToString(), "page_index"));
                    //            icmsFileInfo.OperType = GetNodePropertyValue(a["Value"].ToString(), "oper_type");

                    //            icmsFileInfoList.Add(icmsFileInfo);
                    //        });
                    //}

                    //将整个page信息添加到fileInfo对象里面
                    imageInfo.Add(pkuuid, icmsFileInfoList);
                }
            }
            catch (Exception ex)
            {
                //SysLog.Write(7128, ex, this._proIdAndThreadId);
            }

            return imageInfo;
        }


        private bool _ifGetAllImageInfo = true;
        public bool IfGetAllImageInfo
        {
            set
            {
                this._ifGetAllImageInfo = value;
            }
            get
            {
                return this._ifGetAllImageInfo;
            }
        }
        private Dictionary<string, List<Page>> imageInfo = new Dictionary<string, List<Page>>();
        public Dictionary<string, List<Page>> ImageInfo
        {
            get
            {
                if (_ifGetAllImageInfo)
                {
                    imageInfo = this.GetAllImageInfo();
                    _ifGetAllImageInfo = false;
                }

                return imageInfo;
            }
        }
        #endregion

    }
}
