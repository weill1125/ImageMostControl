using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Com.Boc.Icms.LogDLL;
using Com.Boc.Icms.MetadataEdit.Business.BusinessData;
using Com.Boc.Icms.MetadataEdit.Support.Template;
using System.Xml;
using Com.Boc.Icms.MetadataEdit.Base.Xml;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit;
using Com.Boc.Icms.MetadataEdit.Models;
using Version = Com.Boc.Icms.MetadataEdit.Models.Version;
using System.Net;
using AntDesign;
using System.Xml.Linq;

namespace Com.Boc.Icms.MetadataEdit.Business.Operate
{
    /// <summary>
    /// 最终业务XML生成类
    /// 描述：根据内存中的数据结构生成交互的业务XML文档
    /// 开发者：李爱强
    /// </summary>
    public class XmlDataExport : IXmlDataExport
    {
        private StringBuilder _xmlStr = new StringBuilder();
        private readonly DataServies _dataServies;
        private readonly IXmlTemplateCache _ixmlct;
        private readonly string _proIdAndThreadId = string.Empty;

        public XmlDataExport(IXmlTemplateCache ixmlct, DataServies dataServies)
        {
            this._dataServies = dataServies;
            this._ixmlct = ixmlct;
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        ///// <summary>
        ///// 获取业务交互XML文档(默认XML头部信息)
        ///// </summary>
        ///// <returns></returns>
        //public string CreateIndexXml()
        //{
        //    return CreateIndexXml(CreateIndexXmlHead());
        //}

        /// <summary>
        /// 获取业务交互XML文档
        /// </summary>
        /// <param name="xmlHead">xml头部信息</param>
        /// <param name="delIgnore">是否忽略opera_type为I的节点</param>
        /// <returns></returns>
        public string CreateIndexXml(string xmlHead, bool delIgnore)
        {
            if (xmlHead == "")
            {
                return "";
            }

            StringBuilder xmlStr = new StringBuilder();

            try
            {
                //xml的文件信息
                this.GetIndexXml(ref xmlStr, delIgnore);

                //xml的头信息
                xmlStr.Insert(0, xmlHead);
               
            }
            catch (Exception ex)
            {
                SysLog.Write(7130, ex, _proIdAndThreadId);

                return "";
            }

            return xmlStr.ToString();
        }

        ///// <summary>
        ///// 获取业务交互XML文档
        ///// </summary>
        ///// <param name="version">版本</param>
        ///// <param name="encoding">编码格式</param>
        ///// <param name="description">描述与注释</param>
        ///// <returns></returns>
        //public string CreateIndexXml(string version, string encoding, string description, bool delIgnore)
        //{
        //    return CreateIndexXml(CreateIndexXmlHead(version, encoding, description), delIgnore);
        //}


        ///// <summary>
        ///// 按交易批量生成index xml
        ///// </summary>
        ///// <param name="version">版本</param>
        ///// <param name="encoding">编码格式</param>
        ///// <param name="description">描述与注释</param>
        ///// <returns>交易码，该交易对应的index xml</returns>
        //public Dictionary<string, string> BatchCreateIndexXml(string version, string encoding, string description, bool delIgnore)
        //{
        //    Dictionary<string, string> xmlDic = new Dictionary<string, string>();
        //    XmlDocument xmlDoc = new XmlDocument();

        //    try
        //    {
        //        string xmlInfo = CreateIndexXml(CreateIndexXmlHead(version, encoding, description), delIgnore);

        //        SysLog.Write(9146, _proIdAndThreadId, xmlInfo);
        //        xmlDoc.LoadXml(xmlInfo);

        //        string xmlHeadNode = xmlDoc.FirstChild.OuterXml;

        //        foreach (XmlNode xmlNode in xmlDoc.LastChild.ChildNodes)
        //        {
        //            StringBuilder tmpXmlInfo = new StringBuilder();
        //            tmpXmlInfo.Append(xmlHeadNode);
        //            tmpXmlInfo.Append("<batch>");
        //            tmpXmlInfo.Append(xmlNode.OuterXml);
        //            tmpXmlInfo.Append("</batch>");

        //            xmlDic.Add(xmlNode.FirstChild.InnerText, tmpXmlInfo.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(SysLog.GetMessage(9132, ex.Message));
        //        SysLog.Write(9132, ex, _proIdAndThreadId);
        //        xmlDic.Clear();
        //    }

        //    return xmlDic;
        //}

        /// <summary>
        /// 按交易批量生成index xml
        /// </summary>
        /// <returns>交易码，该交易对应的index xml</returns>
        public Dictionary<string, string> BatchCreateIndexXml(bool delIgnore)
        {
            Dictionary<string, string> xmlDic = new Dictionary<string, string>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                string xmlInfo = CreateIndexXml(CreateIndexXmlHead(), delIgnore);

                SysLog.Write(9146, _proIdAndThreadId, xmlInfo);
                xmlDoc.LoadXml(xmlInfo);


                xmlDic.Add(xmlDoc.SelectSingleNode("//biz_metadata").InnerText, xmlInfo.ToString());

                //string xmlHeadNode = xmlDoc.FirstChild.OuterXml;

                //foreach (XmlNode xmlNode in xmlDoc.LastChild.ChildNodes)
                //{
                //    StringBuilder tmpXmlInfo = new StringBuilder();
                //    tmpXmlInfo.Append(xmlHeadNode);
                //    tmpXmlInfo.Append("<request>");
                //    tmpXmlInfo.Append(xmlNode.OuterXml);
                //    tmpXmlInfo.Append("</request>");

                   
                //}
            }
            catch (Exception e)
            {
                //todo 通知
                //MessageBox.Show(SysLog.GetMessage(9132, e.Message));
                SysLog.Write(9132, e, _proIdAndThreadId);
                xmlDic.Clear();
                
            }

            return xmlDic;
        }

        /// <summary>
        /// 获取index.xml的头(默认"1.0", "UTF-8")
        /// </summary>
        /// <returns></returns>
        public string CreateIndexXmlHead()
        {
            return this.CreateIndexXmlHead("1.0", "UTF-8", string.Empty);
        }

        /// <summary>
        /// 获取index.xml的头
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="description">描述与注释</param>
        /// <returns></returns>
        public string CreateIndexXmlHead(string version, string encoding, string description)
        {
            StringBuilder indexHead = new StringBuilder();
            try
            {
                indexHead.Append("<?xml version=\"" + version + "\" encoding=\"" + encoding + "\"?>");

                //注释不为空，要添加注释到index xml中
                if (description != string.Empty)
                {
                    indexHead.Append(description + "\n");
                }
            }
            catch (Exception ex)
            {
                SysLog.Write(7131, ex, this._proIdAndThreadId);

                return "";
            }

            return indexHead.ToString();
        }

        public string CreatReqheader()
        {
            StringBuilder indexHead = new StringBuilder();            
            try
            {
                indexHead.Append("<req_header>");
                indexHead.Append("<sys_code>" +  "</sys_code>");
                indexHead.Append("<bank_code>" + "</bank_code>");
                indexHead.Append("<branch_code>"  + "</branch_code>");
                indexHead.Append("<operater_id>"  + "</operater_id>");
                indexHead.Append("<trans_type>" + "</trans_type>");
                indexHead.Append("<version>" + "</version>");
                indexHead.Append("<trans_time>" + "</trans_time>");
                indexHead.Append("<trans_id>" + "</trans_id>");
                indexHead.Append("<client_type>" + "01" + "</client_type>");
                indexHead.Append("<client_ip>"  + "</client_ip>");
                indexHead.Append("<starter_sys_code>" + "</starter_sys_code>");
                indexHead.Append("</req_header>");

                //注释不为空，要添加注释到index xml中
            }
            catch (Exception ex)
            {
                SysLog.Write(7131, ex, this._proIdAndThreadId);

                return "";
            }

            return indexHead.ToString();
        }


        /// <summary>
        /// 获取index xml（pageIndex不需要再重新排序）
        /// </summary>
        /// <param name="xmlStr">存储index xml</param>
        /// <param name="delIgnore">是否忽略opera_type为I的节点</param>
        private void GetIndexXml(ref StringBuilder str, bool delIgnore)
        {
            BusinessTable businessTable = (BusinessTable)this._dataServies.GetDataTable(EnumType.TableType.BusinessTable);
            DocTable docTable = (DocTable)this._dataServies.GetDataTable(EnumType.TableType.DocTable);
            PageTable pageTable = (PageTable)this._dataServies.GetDataTable(EnumType.TableType.PageTable);
            DeletedPageTable deletedPageTable = (DeletedPageTable)this._dataServies.GetDataTable(EnumType.TableType.DeletedPageTable);
            VersionTable versionTable = (VersionTable)this._dataServies.GetDataTable(EnumType.TableType.VersionTable);

            Models.Business[] businesses = businessTable.Find();

   
            str.Append("<request>");        
            str.Append(CreatReqheader());
            str.Append("<req_body>");

            str.Append("<file_path>" + "</file_path>");
            str.Append("<zpk>" + "</zpk>");
            foreach (Models.Business business in businesses)
            {
               
                //添加bussiness信息

                str.Append("<business_info>");
                str.Append("<biz_metadata>" + business.Biz_metadata1 + "</biz_metadata>");
                str.Append("<biz_metadata2>" + business.Biz_metadata2 + "</biz_metadata2>");
                str.Append("<biz_metadata3>" + business.Biz_metadata3 + "</biz_metadata3>");
                str.Append("<achr_sec>" + business.Achr_sec + "</achr_sec>");
                str.Append("<image_library_ident>" + business.Image_library_ident + "</image_library_ident>");
            
                string qust = "";
                if (delIgnore)
                {
                    qust = "Biz_metadata1='" + business.Biz_metadata1 + "' and oper_type <> 'I'";
                }
                else
                {
                    qust = "Biz_metadata1='" + business.Biz_metadata1 + "'";
                }
                 List < Doc > docs = null;
                if (delIgnore)
                {
                    docs = docTable.Find(qust)
                    .ToList().FindAll(d =>
                    {
                        if (d.Oper_type == "A") //排除无page的doc
                        {
                            return pageTable.Find("pkuuid = \'" + d.Pkuuid + "\'").Length > 0;
                        }
                        else
                        {
                            return true;
                        }
                    });
                }
                else
                {
                    docs = docTable.Find(qust).ToList();
                }
                           
                str.Append("<doc_list>");

                if (docs.Count > 0)
                {

                    foreach (Doc doc in docs)
                    {
                        str.Append("<docs_info>");
                        str.Append("<pkuuid>" + doc.Pkuuid + "</pkuuid>");
                        str.Append("<data_type>" + doc.Data_type + "</data_type>");
                        str.Append("<uniq_metadata>" + doc.Uniq_metadata + "</uniq_metadata>");
                        str.Append("<index_metadata1>" + doc.Index_metadata1 + "</index_metadata1>");
                        str.Append("<index_metadata2>" + doc.Index_metadata2 + "</index_metadata2>");
                        str.Append("<index_metadata3>" + doc.Index_metadata3 + "</index_metadata3>");
                        str.Append("<ext_metadata>" + doc.Ext_metadata + "</ext_metadata>");
                        str.Append("<is_cust_info>" + doc.Is_cust_info + "</is_cust_info>");
                        str.Append("<cust_no>" + doc.Cust_no + "</cust_no>");
                        str.Append("<cust_name>" + doc.Cust_name + "</cust_name>");
                        str.Append("<cert_type>" + doc.Cust_name + "</cert_type>");
                        str.Append("<cert_no>" + doc.Cert_no + "</cert_no>");
                        str.Append("<security>" + doc.Security + "</security>");
                        str.Append("<expire_date>" + doc.Expire_date + "</expire_date>");
                        str.Append("<image_storage_mech>" + doc.Image_storage_mech + "</image_storage_mech>");
                        str.Append("<is_compress>" + doc.Is_compress + "</is_compress>");


                        DeletedPage[] deletedPages = deletedPageTable.Find("pkuuid = \'" + doc.Pkuuid + "\'");

                        if (deletedPages.Length > 0)
                        {
                            str.Append("<delete_page>");
                            foreach (DeletedPage deletedPage in deletedPages)
                            {
                                str.Append("<pagedel page_index=" + "\"" + deletedPage.Page_index + "\"" + "/>");
                            }
                            str.Append("</delete_page>");
                        }

                        string pagequst = "";
                        if (delIgnore)
                        {
                            pagequst = "pkuuid='" + doc.Pkuuid + "' and oper_type <> 'I'";
                        }
                        else
                        {
                            pagequst = "pkuuid='" + doc.Pkuuid + "'";
                        }

                        Page[] pages = pageTable.Find(pagequst, "page_index asc");
                        SortPageIndex(pageTable, doc.Pkuuid);

                        str.Append("<page_list>");
                        if (pages.Length > 0)
                        {                           
                            foreach (Page page in pages)
                            {
                                str.Append("<page_info>");
                                str.Append("<page>");
                                str.Append("<doc_index>" + page.Doc_index + "</doc_index>");
                                str.Append("<page_flag>" + page.Page_flag + "</page_flag>");
                                str.Append("<page_uuid>" + page.Page_uuid + "</page_uuid>");
                                str.Append("<file_name>" + GetTransName(page.File_name) + "</file_name>");
                                str.Append("<is_have_postil>" + page.Is_have_postil + "</is_have_postil>");
                                str.Append("</page>");
                                str.Append("<postil>");
                                str.Append("<file_name>" + GetTransName(page.File_name) + "</file_name>");
                                str.Append("<remark>" + "" + "</remark>");

                                str.Append("</postil>");

                                if (!string.IsNullOrEmpty(page.PostilInfo))
                                {
                                    str.Append(page.PostilInfo);
                                }
                                str.Append("</page_info>");
                            }                          
                        }

                        str.Append("</page_list>");

                        Version[] vers = versionTable.Find("pkuuid = \'" + doc.Pkuuid + "\'");

                        if (vers.Length > 0)
                        {
                            str.Append("<version_list>");
                            foreach (Version ver in vers)
                            {
                                str.Append(ver.Ver_info);

                            }
                            str.Append("</version_list>");
                        }
                        str.Append("</docs_info>");

                    }
                }
                str.Append("</doc_list>");
                str.Append("</business_info>");
            }
            str.Append("</req_body>");
            str.Append("</request>");
        }

        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <returns>IP字符串</returns>
        public static string LocalIp()
        {
            try
            {
                string name = Dns.GetHostName();
                IPHostEntry entry = Dns.GetHostEntry(name);
                IPAddress[] ipArray = entry.AddressList;
                foreach (var ip in ipArray)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        return ip.ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
               // WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", ex.ToString(), "");
                return "";
            }
        }

        private void SortPageIndex(PageTable pageTable, string pkuuid)
        {
            //this._dataServies.GetAllDocs();
            DataRow[] drs_pages = pageTable.
                Select("pkuuid = \'" + pkuuid + "\' and  oper_type = 'A'", "page_index asc");

            if (drs_pages.Length > 0)
            {
                int startIndex = (int)drs_pages[0]["page_index"];

                for (int i = 1; i < drs_pages.Length; i++)
                {
                    if ((int)drs_pages[i]["page_index"] != startIndex + i)
                    {
                        drs_pages[i]["page_index"] = startIndex + i;
                    }
                }
            }

        }

        private string GetTransName(string oldFileName)
        {
            string result = oldFileName.Replace("&", "&amp;");            

            return result;
        }

        ///// <summary>
        ///// 创建控件xml
        ///// </summary>
        ///// <param name="row">control行</param>
        ///// <returns>control xml</returns>
        //private string CreateControlXmlValue(DataRow row)
        //{
        //    StringBuilder controlXmlValue = new StringBuilder();

        //    //添加节点的头（<node name>）与尾（</node name>）
        //    controlXmlValue.Append("<" + row["Child"] + ">");
        //    controlXmlValue.Append(row["Value"]);
        //    controlXmlValue.Append("</" + row["Child"] + ">");

        //    return controlXmlValue.ToString();
        //}

        ///// <summary>
        ///// 获取字段值为空的节点名称
        ///// </summary>
        ///// <returns></returns>
        //public Dictionary<int, string> GetErrorControlInfo()
        //{
        //    //用来保存文件名
        //    Dictionary<int, string> errorInfo = new Dictionary<int, string>();

        //    //遍历整个数据表
        //    foreach (DataRow row in this._dManager.GData.Rows)
        //    {
        //        //如果是page节点
        //        if (row["Child"].Equals("index_metadata1"))
        //        {
        //            if (row["Value"].ToString() == "")
        //            {
        //                DataRow docRow = this._dManager.SelectNodeRow(int.Parse(row["ParentId"].ToString()));
        //                errorInfo.Add(int.Parse(docRow["Index"].ToString()), row["Child"].ToString());
        //            }
        //        }
        //    }

        //    return errorInfo;
        //}

        #region 新增方法
        ///// <summary>
        ///// 排除子节点为空的doc_info
        ///// </summary>
        ///// <param name="row">行对象</param>
        //private bool RemoveNoChildNodeDocInfo(DataRow row)
        //{
        //    if (!row["Child"].Equals("doc_info")) return true;

        //    string operType = XmlOperate.GetNodePropertyValue(
        //        row["Value"].ToString(),
        //        "oper_type");
        //    if (operType != "A") return true;

        //    //判断子子节点是否存在pages，如果不存在，则代表空doc_info
        //    //如果存在，并且没有任何的page节点，则代表空doc_info
        //    Func<DataRow, string, DataRow[]> checkDocInfo = (a, b) =>
        //        (this._dManager.SelectNodeRows(
        //        int.Parse(a[0].ToString()),
        //        b,
        //        SupportEnumType.EnumNodeType.Node));
        //    DataRow[] rows = checkDocInfo(row, "pages");
        //    return rows.Length > 0 ?
        //        (checkDocInfo(rows[0], "page").Length > 0 ?
        //        true :
        //        false) :
        //        false;
        //}

        ///// <summary>
        ///// 预处理Page_Info节点的page_index为连续序号
        ///// </summary>
        ///// <param name="xmlNodeDic">节点集合</param>
        //private void OrderPageIndex(ref Dictionary<int, DataRow> xmlNodeDic)
        //{
        //    int startIndex = 0;
        //    List<double> docIndexList = new List<double>();
        //    List<int> pageIndexList = new List<int>();
        //    int currentIndex = 0;
        //    List<int> intKeys = xmlNodeDic.Keys.ToList();

        //    for (int i = 0; i < xmlNodeDic.Keys.Count; i++)
        //    {
        //        if (xmlNodeDic[intKeys[i]]["Child"].Equals("page"))
        //            pageIndexList.Add(int.Parse(XmlOperate.GetNodePropertyValue(
        //             xmlNodeDic[intKeys[i]]["Value"].ToString(),
        //             "page_index")));
        //        else
        //            pageIndexList.Add(-1);
        //    }

        //    if (!pageIndexList.Any()) return;
        //    if (!pageIndexList.Where(a => a != -1).Any()) return;

        //    startIndex = this.GetEditPagesCount(xmlNodeDic.Values.ToList<DataRow>());
        //    docIndexList = this.GetSortedDocIndex(xmlNodeDic.Values.ToList<DataRow>());
        //    List<int> clonePageIndexList = pageIndexList.ToList().OrderBy(a => a).ToList();
        //    for (int i = 0; i < docIndexList.Count; i++)
        //    {
        //        foreach (DataRow tmpRow in xmlNodeDic.Values.ToList<DataRow>())
        //        {
        //            double docIndex = double.Parse(XmlOperate.GetNodePropertyValue(tmpRow["Value"].ToString(), "doc_index"));
        //            double flag = XmlOperate.GetNodePropertyValue(tmpRow["Value"].ToString(), "page_flag") == "F" ? 0 : 0.5;

        //            if (docIndexList[i] == docIndex + flag)
        //            {
        //                tmpRow["Value"] = XmlOperate.UpdateNodeProperty(tmpRow["Value"].ToString(), "page_index", (i + startIndex).ToString());
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 获取非新增page数
        ///// </summary>
        ///// <param name="dataRowList"></param>
        ///// <returns></returns>
        //private int GetEditPagesCount(List<DataRow> dataRowList)
        //{
        //    int count = 0;
        //    for (int i = 0; i < dataRowList.Count; i++)
        //    {
        //        string operType = XmlOperate.GetNodePropertyValue(dataRowList[i]["Value"].ToString(), "oper_type");
        //        if (operType == "E" || operType == "I")
        //        {
        //            count++;
        //        }
        //    }

        //    return count;
        //}

        //private List<double> GetSortedDocIndex(List<DataRow> dataRowList)
        //{
        //    List<double> docIndexList = new List<double>();
        //    for (int i = 0; i < dataRowList.Count; i++)
        //    {
        //        string docIndex = XmlOperate.GetNodePropertyValue(dataRowList[i]["Value"].ToString(), "doc_index");
        //        string pageFlag = XmlOperate.GetNodePropertyValue(dataRowList[i]["Value"].ToString(), "page_flag");
        //        string operType = XmlOperate.GetNodePropertyValue(dataRowList[i]["Value"].ToString(), "oper_type");

        //        if (operType == "A")
        //        {
        //            if (pageFlag == "B")
        //            {
        //                docIndexList.Add(int.Parse(docIndex) + 0.5);
        //            }
        //            else
        //            {
        //                docIndexList.Add(int.Parse(docIndex));
        //            }
        //        }                
        //    }

        //    docIndexList.Sort();

        //    return docIndexList;
        //}
        /*
        /// <summary>
        /// 判断元数据是否符合上传规则
        /// </summary>
        /// <param name="dic">pkuuid,模板名键值对</param>
        /// <returns>成功返回1,否则pkuuid或者返回错误消息</returns>
        public string CheckAllMetadata(Dictionary<string, string> dic)
        {
            DataRow[] drArray = null;
            string tempControlXml = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();

            if (dic == null) return string.Empty;
            try
            {
                foreach (var item in dic)
                {
                    //批次传入0
                    if (item.Key == "0")
                    {
                        drArray = dManager.Select("ParentID = 2 and Type=1");
                    }
                    else
                    {
                        drArray = dManager.Select("Child = 'pkuuid' and Value = '" + item.Key + "'");
                        //取得同级控件
                        drArray = dManager.Select("ParentID = " + drArray[0]["ParentID"].ToString() + " and Type=1");
                    }
                    //取得模板XML
                    xmlDoc.LoadXml(ixmlct.FindTemplate(item.Value));
                    //根据模板进行判断
                    XmlNodeList xmlNodes = xmlDoc.DocumentElement.ChildNodes;
                    foreach (XmlNode node in xmlNodes)
                    {
                        if (!bool.Parse(node.Attributes["Visible"].Value) ||
                            !bool.Parse(node.Attributes["Enable"].Value))
                        {
                            continue;
                        }
                        else
                        {
                            //元数据的值与控件模板的正则进行验证
                            foreach (var row in drArray)
                            {
                                //查找匹配的控件
                                if (row["Child"].Equals(node.Attributes["Name"]))
                                {
                                    if (!Regex.IsMatch(row["Value"].ToString(),
                                         node.Attributes["Validate"].ToString()))
                                        return item.Key;
                                }
                            }
                        }
                    }
                }
                return "1";
            }
            catch
            {
                return "2";
            }
        }
        */

        /// <summary>
        /// 判断元数据是否符合上传规则
        /// </summary>
        /// <param name="dic">pkuuid,模板名键值对</param>
        /// <returns>成功返回1,否则pkuuid或者返回错误消息</returns>
        public Dictionary<string, List<string>> CheckAllMetadata(Dictionary<int, Dictionary<string, string>> dic)
        {
            DataRow dataRow = null;
            // string tempControlXml = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            if (dic == null) return null;
            try
            {
                foreach (int key in dic.Keys)
                {

                    foreach (string valueKey in dic[key].Keys)
                    {
                        //drArray = this._dManager.Select("Value = '" + valueKey + "'");
                        ////取得同级控件
                        //drArray = this._dManager.Select("ParentID = " + drArray[0]["ParentID"] + " and Type=1");
                        //取得模板XML


                        if (key == 0) //交易节点
                        {
                            dataRow = this._dataServies.GetDataTable(EnumType.TableType.BusinessTable).Rows.Find(valueKey);
                        }
                        else //doc 节点
                        {
                            dataRow = this._dataServies.GetDataTable(EnumType.TableType.DocTable).Rows.Find(valueKey);
                        }

                        xmlDoc.LoadXml(this._ixmlct.FindTemplate(dic[key][valueKey]));
                        //根据模板进行判断
                        XmlNodeList xmlNodes = xmlDoc.DocumentElement.ChildNodes;
                        foreach (XmlNode node in xmlNodes)
                        {
                            bool visibleValue = false;
                            bool enableValue = false;
                            if (node.Attributes["Visible"] == null)
                            {
                                visibleValue = true;
                            }
                            else
                            {
                                visibleValue = bool.Parse(node.Attributes["Visible"].Value);
                            }

                            if (node.Attributes["Enable"] == null)
                            {
                                enableValue = true;
                            }
                            else
                            {
                                enableValue = bool.Parse(node.Attributes["Enable"].Value);
                            }

                            if (!visibleValue || !enableValue)
                            {
                                continue;
                            }

                            if (dataRow.Table.Columns.Contains(node.Attributes["Name"].Value))
                            {
                                string value = (string)dataRow[node.Attributes["Name"].Value];

                                if (!Regex.IsMatch(value, node.Attributes["Validate"].Value))
                                {
                                    //return item.Key;
                                    //交易节点
                                    if (key == 0)
                                    {
                                        if (result.Keys.Contains("0"))
                                        {
                                            result["0"].Add(node.Attributes["Caption"].Value);
                                        }
                                        else
                                        {
                                            List<string> values = new List<string>();
                                            values.Add(node.Attributes["Caption"].Value);

                                            result.Add("0", values);
                                        }
                                    }
                                    else
                                    {
                                        if (result.Keys.Contains(valueKey))
                                        {
                                            result[valueKey].Add(node.Attributes["Caption"].Value);
                                        }
                                        else
                                        {
                                            List<string> values = new List<string>();
                                            values.Add(node.Attributes["Caption"].Value);

                                            result.Add(valueKey, values);
                                        }
                                    }

                                }

                            }


                            ////元数据的值与控件模板的正则进行验证
                            //foreach (var row in drArray)
                            //{
                            //    //查找匹配的控件
                            //    if (row["Child"].Equals(node.Attributes["Name"].Value))
                            //    {
                            //        if (!Regex.IsMatch(row["Value"].ToString(),
                            //            node.Attributes["Validate"].Value))
                            //        {
                            //            //return item.Key;
                            //            //交易节点
                            //            if (key == 0)
                            //            {
                            //                if (result.Keys.Contains("0"))
                            //                {
                            //                    result["0"].Add(node.Attributes["Caption"].Value);
                            //                }
                            //                else
                            //                {
                            //                    List<string> values = new List<string>();
                            //                    values.Add(node.Attributes["Caption"].Value);

                            //                    result.Add("0", values);
                            //                }

                            //                break;
                            //            }
                            //            if (result.Keys.Contains(valueKey))
                            //            {
                            //                result[valueKey].Add(node.Attributes["Caption"].Value);
                            //            }
                            //            else
                            //            {
                            //                List<string> values = new List<string>();
                            //                values.Add(node.Attributes["Caption"].Value);

                            //                result.Add(valueKey, values);
                            //            }

                            //            break;
                            //        }
                            //    }
                            //}
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                SysLog.Write(9905, ex.ToString(), this._proIdAndThreadId);
                return null;
            }
        }

        ///// <summary>
        ///// 获取数据结构的XML
        ///// </summary>
        ///// <returns></returns>
        //public string GetDataXml()
        //{
        //    return this._dManager.GetDataXml();
        //}
        #endregion
    }
}
