using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Com.Boc.Icms.MetadataEdit.Base.XML;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Business.Operate.DocInfo;
using Com.Boc.Icms.MetadataEdit.Support.ProviderEffect;
using Com.Boc.Icms.MetadataEdit.Business.Operate;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate.PageInfo
{
    /// <summary>
    /// 图像操作类
    /// </summary>
    partial class PageInfoOperate : XmlNodeOperate, IPageInfoOperate
    {
        private Property property;
        private IXmlDataImport ixmlfe;

        public PageInfoOperate(DataManage dataManage, Property property,
            IXmlDataImport ixmlfe)
            : base(dataManage)
        {
            this.property = property;
            this.ixmlfe = ixmlfe;
        }

        #region 图像
        /// <summary>
        /// 初始化pages_info的XML
        /// 
        /// 描述：图像添加到文档节点之后，生成图像XML，此处进行数据缓存。
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="xml">xml字符串</param>
        public void InitXml(int doc_index, string xml)
        {
            try
            {
                xml = "<delete_page/>" + xml + "<version_list/>";

                ixmlfe.ParentID = GetDocID(doc_index);
                ixmlfe.SaveXml(xml);
            }
            catch
            {
                throw new Exception("初始化加载图像XML出错！");
            }
        }

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="file_name">图像文件名</param>
        public void AddImageXml(int doc_index, string file_name)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //pages节点编号
                int pagesID = CheckNode(docID, "pages");

                //添加图像节点到数据缓存
                int pageIndex = dataManage.GetMaxIndex(pagesID,
                    EnumType.EnumNodeType.Node) + 1;

                string xml = "<page doc_index=\"" + doc_index +
                    "\" modi_time=\"" + Now +
                    "\" old_doc_index=\"" + doc_index +
                    "\" modi_range=\"N\"" +
                    " file_name=\"" + file_name +
                    "\" page_index=\"" + pageIndex +
                    "\" oper_type=\"A\"" +
                    " old_page_flag=\"F\"" +
                    " page_flag=\"F\"></page>";

                SeizeByIndex(pagesID, pageIndex);
                dataManage.AddRow(pagesID,
                    "page",
                    xml,
                    EnumType.EnumNodeType.Node,
                    pageIndex);

                //string oper_type = "A";
                //if (property.WorkType != EnumType.EnumWorkType.InitScan)
                //    oper_type = "E";

                DataRow row = this.dataManage.SelectNodeRow(docID);

                string oper_type = XmlNodeOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");

                if (oper_type != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");
                }

            }
            catch
            {
                throw new Exception("添加图像出错！");
            }
        }

        /// <summary>
        /// 增加图像节点集合
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="fileNames">图像文件名</param>
        public void AddImageXml(int doc_index, string[] fileNames)
        {
            try
            {
                fileNames.ToList().ForEach(a => AddImageXml(doc_index, a));
            }
            catch
            {
                throw new Exception("添加图像集合出错！");
            }
        }

        /// <summary>
        /// 删除图像节点
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        public void DeleteImageXml(int doc_index, int page_index)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得图像编号
                int pageID = GetPageID(docID, page_index);

                DataRow dr = dataManage.SelectNodeRow(pageID);

                //取得pages编号
                int pagesID = (int)dr["ParentID"];

                //执行删除与重新排序
                dataManage.DeleteByParentID(pageID);
                OrderBy(pagesID);

                //if (property.WorkType != EnumType.EnumWorkType.InitScan)
                //{
                //    //重扫、补扫的情况，增加pagedel节点
                //    AddImageDeleteNode(docID, page_index);

                //    //CheckHasPagesChild(docID);

                //    UpdateNodePropertyAndTime(docID, "oper_type", "E");
                //}

                #region 添加pagedel节点
                DataRow row = this.dataManage.SelectNodeRow(pageID);

                string oper_type = XmlNodeOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");

                if (oper_type != "A")
                {
                    AddImageDeleteNode(docID, page_index);
                }

                #endregion

                #region 更改docinfo的opert_ype属性
                row = this.dataManage.SelectNodeRow(docID);

                oper_type = XmlNodeOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");

                if (oper_type != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");
                }
                #endregion

            }
            catch
            {
                throw new Exception("删除图像出错！");
            }
        }

        /// <summary>
        /// 修改图像正反面
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="page_flag">当前正反面</param>
        public void UpdateImageFlag(int doc_index, int page_index, string page_flag)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得图像编号
                int pageID = GetPageID(docID, page_index);

                DataRow row = this.dataManage.SelectNodeRow(pageID);
                string oper_type = XmlNodeOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");

                if (oper_type != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                    UpdateNodeProperty(pageID, "page_flag", page_flag);
                    UpdateNodePropertyAndTime(pageID, "oper_type", "E");
                }
                else
                {
                    UpdateNodePropertyAndTime(pageID, "page_flag", page_flag);
                }
            }
            catch
            {
                throw new Exception("修改图像正反面出错！");
            }
        }

        /// <summary>
        /// 替换图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="file_name">图想文件名</param>
        public void ReplaceFileName(int doc_index, int page_index, string file_name)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得图像编号
                int pageID = GetPageID(docID, page_index);

                //取得图像批注
                DataRow[] postilRows = dataManage.SelectNodeRows(pageID,
                    "postil",
                    EnumType.EnumNodeType.Node);

                //if (property.WorkType != EnumType.EnumWorkType.InitScan)
                //{
                //    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                //    UpdateNodeProperty(pageID, "file_name", file_name);
                //    UpdateNodePropertyAndTime(pageID, "oper_type", "E");

                //    //修改图像批注的文件名
                //    if (postilRows.Length > 0)
                //    {
                //        int postilID = (int)postilRows[0][0];

                //        UpdateNodeProperty(postilID,
                //            "file_name",
                //            GetPostilFileName(file_name));
                //        UpdateNodePropertyAndTime(postilID, "oper_type", "E");
                //    }
                //}
                //else
                //{
                //    UpdateNodePropertyAndTime(pageID, "file_name", file_name);

                //    if (postilRows.Length > 0)
                //    {
                //        UpdateNodePropertyAndTime((int)postilRows[0][0],
                //            "file_name",
                //            GetPostilFileName(file_name));
                //    }
                //}

                DataRow row = this.dataManage.SelectNodeRow(pageID);
                string oper_type = XmlNodeOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");


                if (oper_type != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                    UpdateNodeProperty(pageID, "file_name", file_name);
                    UpdateNodePropertyAndTime(pageID, "oper_type", "E");

                    //修改图像批注的文件名
                    if (postilRows.Length > 0)
                    {
                        int postilID = (int)postilRows[0][0];

                        UpdateNodeProperty(postilID,
                            "file_name",
                            GetPostilFileName(file_name));

                        string postilOperType = XmlNodeOperate.GetNodePropertyValue(postilRows[0]["Value"].ToString(), "oper_type");

                        if (postilOperType != "A")
                        {
                            UpdateNodePropertyAndTime(postilID, "oper_type", "E");
                        }
                    }
                }
                else
                {
                    UpdateNodePropertyAndTime(pageID, "file_name", file_name);

                    if (postilRows.Length > 0)
                    {
                        UpdateNodePropertyAndTime((int)postilRows[0][0],
                            "file_name",
                            GetPostilFileName(file_name));
                    }
                }
            }
            catch
            {
                throw new Exception("替换图像文件名出错！");
            }
        }

        /// <summary>
        /// 移动图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        /// <param name="old_page_index">初始图像索引</param>
        public void MoveImage(int doc_index, int page_index, int old_doc_index, int old_page_index)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得pages节点编号
                int pagesID = CheckNode(docID, "pages");

                //取得初始文档编号
                int old_docID = GetDocID(old_doc_index);

                //取得初始图像编号
                int old_pageID = GetPageID(old_docID, old_page_index);

                DataRow dr = dataManage.SelectNodeRow(old_pageID);
                int old_pagesID = (int)dr["parentID"];

                //判断图像是否在当前父节点下面移动
                if (old_doc_index == doc_index)
                {
                    //目标图像编号
                    int pageID = dataManage.GetIDByIndex(pagesID, page_index);

                    base.MoveNode(old_pageID, pagesID, page_index);

                    //修改初始图像、目标图像的page_index属性
                    UpdateNodeProperty(old_pageID, "doc_index", page_index.ToString());

                    UpdateNodeProperty(pageID, "doc_index", old_page_index.ToString());

                    if (property.WorkType == EnumType.EnumWorkType.InitScan)
                    {
                        UpdateNodePropertyAndTime(old_pageID, "oper_type", "A");

                        UpdateNodePropertyAndTime(pageID, "oper_type", "A");
                    }
                }
                else
                {
                    //在新文档中占位
                    SeizeByIndex(pagesID, page_index);

                    //编辑图像节点
                    dr.BeginEdit();
                    dr["parentID"] = pagesID;
                    dr["Index"] = page_index;

                    //修改图像节点XML对应的文档索引、图像索引属性
                    string value = XMLOperate.UpdateNodeProperty(
                        dr["Value"].ToString(),
                        "doc_index",
                        (page_index+1).ToString());
                    //value = XMLOperate.UpdateNodeProperty(
                    //    value,
                    //    "page_index",
                    //    page_index.ToString());
                    dr["Value"] = value;

                    dr.EndEdit();
                    dataManage.GData.AcceptChanges();

                    UpdateNodePropertyAndTime(old_pageID, "oper_type", "A");

                    //重新排序初始父节点下面的子节点
                    OrderBy(old_pagesID);

                    DataRow row = this.dataManage.SelectNodeRow(old_docID);
                    string oper_type = XmlNodeOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");

                    if (oper_type != "A")
                    {
                        //判断delete_page节点是否存在,不存在则执行添加,并执行添加pagedel节点
                        AddImageDeleteNode(old_docID, old_page_index);

                        //CheckHasPagesChild(old_docID);

                        UpdateNodePropertyAndTime(old_docID, "oper_type", "E");
                    }
                }

                //if (property.WorkType != EnumType.EnumWorkType.InitScan)
                //    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                DataRow docRow = this.dataManage.SelectNodeRow(docID);
                string newDocOperType = XmlNodeOperate.GetNodePropertyValue(docRow["Value"].ToString(), "oper_type");

                if (newDocOperType != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");
                }
            }
            catch
            {
                throw new Exception("移动图像位置出错！");
            }
        }

        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="old_doc_index">初始文档索引</param>
        /// <param name="old_page_index">初始图像索引</param>
        public void CopyImage(int doc_index, int old_doc_index, int old_page_index)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得pages节点编号
                int pagesID = CheckNode(docID, "pages");

                //取得图像索引
                int pageIndex = dataManage.GetMaxIndex(pagesID,
                    EnumType.EnumNodeType.Node) + 1;

                //取得初始文档编号
                int old_docID = GetDocID(old_doc_index);

                //取得初始图像编号
                int old_pageID = GetPageID(old_docID, old_page_index);

                DataRow dr = dataManage.SelectNodeRow(old_pageID);

                //占位
                SeizeByIndex(pagesID, pageIndex);

                //添加初始图像副本到目标文档的最后一位
                int pageID = dataManage.AddRow(pagesID,
                    dr["Child"].ToString(),
                    dr["Value"].ToString(),
                    (EnumType.EnumNodeType)dr["Type"],
                    pageIndex);

                //挂接初始图像批注副本到目标图像下面
                DataRow[] rows = dataManage.SelectNodeRows(old_pageID,
                    "Postil",
                    EnumType.EnumNodeType.Node);
                if (rows.Length > 0)
                {
                    int postilID = dataManage.AddRow(pageID,
                        rows[0]["Child"].ToString(),
                        rows[0]["Value"].ToString(),
                        (EnumType.EnumNodeType)rows[0]["Type"],
                        0);

                    UpdateNodePropertyAndTime(postilID, "oper_type", "A");
                }

                //UpdateNodeProperty(pageID, "page_index", pageIndex.ToString());
                UpdateNodeProperty(pageID, "doc_index", pageIndex.ToString());
                UpdateNodePropertyAndTime(pageID, "oper_type", "A");

                //if (property.WorkType != EnumType.EnumWorkType.InitScan)
                //    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                DataRow docRow = this.dataManage.SelectNodeRow(docID);
                string oper_type = XmlNodeOperate.GetNodePropertyValue(docRow["Value"].ToString(), "oper_type");

                if (oper_type != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");
                }
            }
            catch
            {
                throw new Exception("复制图像出错！");
            }
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
            //索引,文件名键值对
            Dictionary<int, string> dic = new Dictionary<int, string>();
            //图像文件名
            string fileName = string.Empty;

            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                DataRow[] rows = dataManage.GetRowsByType(
                    GetSpecialNode(docID),
                    EnumType.EnumNodeType.Node);
                rows.OrderBy(a => a["Index"])
                    .ToList().ForEach(a =>
                    {
                        fileName = XMLOperate.GetNodePropertyValue(
                            a["Value"].ToString(),
                            "file_name");
                        dic.Add((int)a["Index"], fileName);
                    });
            }
            catch
            {
                throw new Exception("取得排序的图像键值对出错！");
            }

            return dic;
        }
        #endregion

        #region 图像批注
        /// <summary>
        /// 添加图像批注文件
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="remark">批注信息</param>
        public void AddImagePostil(int doc_index, int page_index, string remark)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得图像编号
                int pageID = GetPageID(docID, page_index);

                //添加图像批注节点
                DataRow dr = dataManage.SelectNodeRow(pageID);
                string fileName = XMLOperate.GetNodePropertyValue(
                    dr["Value"].ToString(),
                    "file_name");

                string operType = XMLOperate.GetNodePropertyValue(
                    dr["Value"].ToString(),
                    "oper_type");

                fileName = GetPostilFileName(fileName);

                string postilXml = "<postil modi_time=\"" + Now +
                    "\" remark=\"" + remark +
                    "\" file_name=\"" + fileName +
                    "\" oper_type=\"A\"/>";

                //判断批注是否存在，如果存在，则删除
                DataRow[] rows = dataManage.SelectNodeRows(pageID,
                    "postil",
                    EnumType.EnumNodeType.Node);
                if (rows.Length > 0)
                {
                    foreach (var row in rows)
                    {
                        row.Delete();
                    }

                    dataManage.GData.AcceptChanges();
                }

                //添加批注
                dataManage.AddRow(pageID,
                    "postil",
                    postilXml,
                    EnumType.EnumNodeType.Node, 0);

                //if (property.WorkType != EnumType.EnumWorkType.InitScan)
                //    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                DataRow docRow = this.dataManage.SelectNodeRow(docID);
                string docOperType = XmlNodeOperate.GetNodePropertyValue(docRow["Value"].ToString(), "oper_type");

                if (docOperType != "A")
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");
                }

                if(operType != "A")
                {
                    this.UpdateNodePropertyAndTime(pageID, "oper_type", "E");
                }
                //UpdateNodePropertyAndTime(pageID, "oper_type", "A");
            }
            catch
            {
                throw new Exception("添加图像批注出错！");
            }
        }

        /// <summary>
        /// 修改图像批注信息
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        /// <param name="remark">批注信息</param>
        public void UpdateImagePostil(int doc_index, int page_index, string remark)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得图像编号
                int pageID = GetPageID(docID, page_index);

                //取得图像批注编号
                int PostilID = dataManage.GetIDByIndex(pageID, 0);

                DataRow pageRow = this.dataManage.SelectNodeRow(pageID);
                string pageOperType = XmlNodeOperate.GetNodePropertyValue(pageRow["Value"].ToString(), "oper_type");

                if (pageOperType != "A")
                {
                    DataRow docRow = this.dataManage.SelectNodeRow(docID);
                    string docOperType = XmlNodeOperate.GetNodePropertyValue(pageRow["Value"].ToString(), "oper_type");

                    if (docOperType != "A")
                    {
                        UpdateNodePropertyAndTime(docID, "oper_type", "E");
                    }

                    UpdateNodePropertyAndTime(pageID, "oper_type", "E");

                    UpdateNodeProperty(PostilID, "remark", remark);

                    DataRow postilRow = this.dataManage.SelectNodeRow(PostilID);
                    string postilOperType = XmlNodeOperate.GetNodePropertyValue(postilRow["Value"].ToString(), "oper_type");

                    if (postilOperType != "A")
                    {
                        UpdateNodePropertyAndTime(PostilID, "oper_type", "E");
                    }
                }
                else
                {
                    UpdateNodePropertyAndTime(PostilID, "remark", remark);
                }
            }
            catch
            {
                throw new Exception("修改图像批注信息出错！");
            }
        }

        /// <summary>
        /// 删除批注
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="page_index">图像索引</param>
        public void DeleteImagePostil(int doc_index, int page_index)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //取得图像编号
                int pageID = GetPageID(docID, page_index);

                //取得图像批注编号
                int PostilID = dataManage.GetIDByIndex(pageID, 0);

                if (property.WorkType != EnumType.EnumWorkType.InitScan)
                {
                    UpdateNodePropertyAndTime(docID, "oper_type", "E");

                    UpdateNodePropertyAndTime(pageID, "oper_type", "D");

                    UpdateNodePropertyAndTime(PostilID, "oper_type", "D");
                }
                else
                {
                    //初始添加图像情况，直接删除批注节点
                    base.Delete(PostilID);
                }
            }
            catch
            {
                throw new Exception("删除图像批注出错！");
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 取得文档编号
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <returns></returns>
        private int GetDocID(int doc_index)
        {
            return dataManage.GetIDByIndex(
                GetSpecialNode(property.BusinessID),
                doc_index);
        }

        /// <summary>
        /// 取得图像编号
        /// </summary>
        /// <param name="docID">文档编号</param>
        /// <param name="page_index">图像索引</param>
        /// <returns></returns>
        private int GetPageID(int docID, int page_index)
        {
            int id = 0;

            try
            {
                DataRow[] rows = dataManage.SelectNodeRows(docID,
                    "pages",
                    EnumType.EnumNodeType.Node);

                id = dataManage.GetIDByIndex((int)rows[0][0], page_index);
            }
            catch
            {
                throw new Exception("图像不存在对于的pages节点！");
            }

            return id;
        }

        /// <summary>
        /// 取得图像批注的文件名
        /// </summary>
        /// <param name="fileName">图像文件名</param>
        /// <returns>批注文件名</returns>
        private string GetPostilFileName(string fileName)
        {
            return Regex.Replace(fileName, @".[^.]+$", ".ant");
        }

        /// <summary>
        /// 添加pagedel节点
        /// </summary>
        /// <param name="docID">文档节点编号</param>
        /// <param name="page_index">图像节点索引</param>
        private void AddImageDeleteNode(int docID, int page_index)
        {
            //判断是否需要添加pagedel节点
            if (property.WorkType == EnumType.EnumWorkType.InitScan) return;

            //添加图像的delete_page节点到数据缓存
            DataRow[] dr = dataManage.GData.Select("[ParentID] = " +
                docID +
                " and [Child] = 'delete_page'");
            //新增的pagedel节点
            string pagedelXml = "<pagedel page_index=" +
                "\"" + page_index.ToString() + "\"" + "/>";
            //deletePage节点
            string deletePage = string.Empty;
            //deletePage编号
            int id = 0;
            //索引
            int index = 0;

            if (dr.Length <= 0)
            {
                //构造delete_page空节点
                deletePage = " <delete_page></delete_page>";

                //在索引位置0处占位
                dataManage.SeizeByIndex(docID, EnumType.EnumNodeType.Node, 0);
                //index = dataManage.GetMaxIndex(docID,
                //   EnumType.EnumNodeType.Node) + 1;

                //添加delete_page节点到Doc_info的Node类型节点的第一位
                dataManage.AddRow(docID,
                    "delete_page",
                    deletePage,
                    EnumType.EnumNodeType.Node,
                    0);
            }

            id = int.Parse(dr[0]["ID"].ToString());
            //添加pagedel到delete_page子节点的最后一位
            index = dataManage.GetMaxIndex(id,
                EnumType.EnumNodeType.Node) + 1;

            dataManage.AddRow(id,
                "pagedel",
                pagedelXml,
                EnumType.EnumNodeType.Node,
                index);

            dataManage.GData.AcceptChanges();
        }

        /// <summary>
        /// 判断pages节点是否存在，如果存在，但是并没有子节点，则删除此节点
        /// </summary>
        /// <param name="docID">文档编号</param>
        private void CheckHasPagesChild(int docID)
        {
            DataRow[] rows = dataManage.SelectNodeRows(docID,
                "pages",
                EnumType.EnumNodeType.Node);
            if (rows.Length <= 0) return;

            int pagesID = (int)rows[0]["ID"];
            int pagesIndex = (int)rows[0]["Index"];
            rows = dataManage.GetRowsByType(pagesID, EnumType.EnumNodeType.Node);

            if (rows.Length <= 0)
            {
                dataManage.Delete(pagesID);

                dataManage.ReseizeByIndex(docID, EnumType.EnumNodeType.Node, pagesIndex);
            }
        }

        /// <summary>
        /// 将指定父节点编号的子节点DataRow数组中指定列名的列重新编号(排序并重新从0开始编号)
        /// </summary>
        /// <param name="rows">DataRow数组</param>
        public void OrderBy(int nodeID)
        {
            DataRow[] rows = this.dataManage.GData.Select("[parentID] = " + nodeID);
            rows = rows.OrderBy(a => a["Index"]).ToArray();

            //图像节点的XML
            string value = string.Empty;

            for (int i = 0; i < rows.Length; i++)
            {
                if (!rows[i]["Index"].Equals(i))
                {
                    rows[i].BeginEdit();
                    rows[i]["Index"] = i;

                    //同步page_index属性
                    rows[i]["Value"] = XMLOperate.UpdateNodeProperty(
                        rows[i]["Value"].ToString(),
                        "doc_index",
                        i.ToString());

                    rows[i].EndEdit();
                }
            }

            this.dataManage.GData.AcceptChanges();
        }

        /// <summary>
        /// 将指定父节点编号的子节点DataRow数组中指定列名的列用index占位(所有大于等于index的记录都后移一位)
        /// </summary>
        /// <param name="nodeID">父节点编号</param>
        /// <param name="index">占位索引</param>
        public void SeizeByIndex(int nodeID, int index)
        {
            DataRow[] rows = this.dataManage.GData.Select("[parentID] = " + nodeID);

            //图像节点的XML
            string value = string.Empty;
            int currentIndex = 0;

            //排序
            rows = rows.OrderBy(a => a["Index"]).ToArray();

            //占位
            foreach (var row in rows)
            {
                currentIndex = (int)row["Index"];

                row.BeginEdit();
                if (currentIndex >= index)
                {
                    row["Index"] = ++currentIndex;

                    //同步page_index属性
                    row["Value"] = XMLOperate.UpdateNodeProperty(
                        row["Value"].ToString(),
                        "doc_index",
                        currentIndex.ToString());
                }
                row.EndEdit();
            }

            this.dataManage.GData.AcceptChanges();
        }
        #endregion
    }

    /// <summary>
    /// XmlImageOperate分部类
    /// 
    /// 开发者：李爱强
    /// </summary>
    partial class PageInfoOperate
    {
        /// <summary>
        /// 添加文档
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="docXml">xml字符串</param>
        public void AddDoc(int doc_index, string docXml)
        {
            try
            {
                //取得docs_info节点编号
                int docs_infoID = GetSpecialNode(property.BusinessID);
                //获取docs_Info下面的doc
                DataRow[] docs = dataManage.GetRowsByType(GetSpecialNode(property.BusinessID), EnumType.EnumNodeType.Node);

                //遍历每一个doc
                foreach (DataRow row in docs)
                {
                    //如果doc的index大于等要添加的doc的index，需要后延一位
                    if (int.Parse(row["Index"].ToString()) >= doc_index)
                    {
                        row.BeginEdit();
                        row["Index"] = (int)row["Index"] + 1;
                        row.EndEdit();
                    }
                }

                //保存改变
                dataManage.GData.AcceptChanges();

                //添加新doc
                int docId = dataManage.AddRow(docs_infoID, "doc_info", docXml, EnumType.EnumNodeType.Node, doc_index);

                //新增文档默认添加一个pkuid节点
                string xml = System.Guid.NewGuid().ToString();
                base.Add(docId, "pkuid", xml, EnumType.EnumNodeType.Control);
            }
            catch
            {
                throw new Exception("添加文档出错！");
            }
        }

        /// <summary>
        /// 增加图像节点
        /// </summary>
        /// <param name="doc_index">文档索引</param>
        /// <param name="pageXml">xml字符串</param>
        public void AddImageXml(int doc_index, int page_index, string pageXml)
        {
            try
            {
                //取得文档编号
                int docID = GetDocID(doc_index);

                //pages节点编号
                int pagesID = CheckNode(docID, "pages");

                //获取pages下面的所有的page
                DataRow[] pages = dataManage.GetRowsByType(docID, EnumType.EnumNodeType.Node);

                //遍历每一个page
                foreach (DataRow row in pages)
                {
                    //每一个page的index如果大于等于新添加的page的index，需要后延一位
                    if (int.Parse(row["Index"].ToString()) >= page_index)
                    {
                        row.BeginEdit();
                        row["Index"] = (int)row["Index"] + 1;
                        row.EndEdit();
                    }
                }

                //保存改变
                dataManage.GData.AcceptChanges();
                //添加新page
                dataManage.AddRow(pagesID, "page", pageXml, EnumType.EnumNodeType.Node, page_index);

                //改变的doc的oper_type
                UpdateNodePropertyAndTime(docID, "oper_type", "A");
            }
            catch
            {
                throw new Exception("添加图像出错！");
            }
        }

        /// <summary>
        /// 重扫补扫时获取整个业务图像信息<!--<doc index, <page index, file name>>-->
        /// </summary>
        /// <returns>返回结构(文档索引, (图像索引, 图像名)) </returns>
        public Dictionary<int, Dictionary<int, string>> GetImageOrder()
        {
            //保存整个file的信息(文档索引, (图像索引, 图像名))
            Dictionary<int, Dictionary<int, string>> fileInfo = new Dictionary<int, Dictionary<int, string>>();
            //保存每个page的信息(图像索引, 图像名)
            Dictionary<int, string> pageInfo = null;

            try
            {
                //取得docs_info节点编号
                int docs_infoID = GetSpecialNode(property.BusinessID);
                //获取docs_info下面的所有的doc
                DataRow[] docs = dataManage.GetRowsByType(GetSpecialNode(property.BusinessID), EnumType.EnumNodeType.Node);

                //遍历每个doc
                foreach (DataRow row in docs)
                {
                    //pages节点编号
                    int pagesID = int.Parse(dataManage.SelectNodeRows(int.Parse(row["ID"].ToString()),
                        "pages", EnumType.EnumNodeType.Node)[0]["ID"].ToString());

                    //初始化对象
                    pageInfo = new Dictionary<int, string>();

                    //获取pages下面的page
                    DataRow[] rows = dataManage.GetRowsByType(pagesID, Support.GlobalCache.SupportEnumType.EnumNodeType.Node);

                    if (rows.Length > 0)
                    {
                        //对每个page按index排序，并添加到pageInfo对象里面
                        rows.OrderBy(a => a["Index"])
                            .ToList().ForEach(a =>
                            {
                                string fileName = XMLOperate.GetNodePropertyValue(a["Value"].ToString(), "file_name");
                                pageInfo.Add((int)a["Index"], fileName);
                            });
                    }

                    //将整个page信息添加到fileInfo对象里面
                    fileInfo.Add(int.Parse(row["Index"].ToString()), pageInfo);
                }
            }
            catch
            {
                throw new Exception("获取文件信息失败");
            }

            return fileInfo;
        }

        /// <summary>
        /// 获取改变的文件名
        /// </summary>
        /// <returns></returns>
        public List<string> GetChangedFileNames()
        {
            //用来保存文件名
            List<string> fileNames = new List<string>();

            //遍历整个数据表
            foreach (DataRow row in dataManage.GData.Rows)
            {
                //如果是page节点
                if (row["Child"].Equals("page"))
                {
                    //获取oper_type属性
                    string oper_type = XMLOperate.GetNodePropertyValue(row["Value"].ToString(), "oper_type");

                    //如果是空或者忽略，继续找下一个
                    if (oper_type == string.Empty || oper_type == "I")
                    {
                        continue;
                    }
                    else
                    {
                        //如果是改变的（增删改）文件名，提取出来
                        string fileName = XMLOperate.GetNodePropertyValue(row["Value"].ToString(), "file_name");

                        fileNames.Add(fileName);
                    }
                }
            }

            return fileNames;
        }

        /// <summary>
        /// 获取Control类型的子节点信息
        /// </summary>
        /// <param name="docIndex">文件类型编号</param>
        /// <param name="nodeNames">子节点名称</param>
        /// <returns>子节点名称跟子节点值的组合</returns>
        public Dictionary<string, string> GetChildNodeValue(int docIndex, List<string> nodeNames)
        {
            Dictionary<string, string> controlInfo = new Dictionary<string, string>();

            //取得docs_info节点编号
            int docs_infoID = GetSpecialNode(property.BusinessID);

            //取得doc_info节点编号

            int docID = dataManage.GetIDByIndex(docs_infoID, docIndex);

            DataRow[] controlRows = dataManage.GetRowsByType(docID, Support.GlobalCache.SupportEnumType.EnumNodeType.Control);

            foreach (DataRow row in controlRows)
            {
                if (nodeNames.Contains(row["Child"].ToString()))
                {
                    controlInfo.Add(row["Child"].ToString(), row["Value"].ToString());
                }
            }

            return controlInfo;
        }
    }
}
