using System;
using System.Xml;
using Com.Boc.Icms.LogDLL;
using System.Data;
using Com.Boc.Icms.MetadataEdit.Base.Xml;
using Com.Boc.Icms.MetadataEdit.Services;
using Com.Boc.Icms.MetadataEdit.DataTables;
using Com.Boc.Icms.MetadataEdit.Models;
using Version = Com.Boc.Icms.MetadataEdit.Models.Version;


namespace Com.Boc.Icms.MetadataEdit.Business.Operate
{


    /// <summary>
    /// 交互XML文件的处理类
    /// </summary>
    class XmlDataImport : XmlOperate, IXmlDataImport
    {
        private readonly DataServies _dataServies;

        private readonly string _proIdAndThreadId = string.Empty;

        /// <summary>
        /// 文档XML根
        /// </summary>
        public new XmlElement Root
        {
            get
            {
                return base.Root;
            }
        }

        public XmlDataImport(DataServies dataServies)
            : base()
        {
            this._dataServies = dataServies;
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }




        /// <summary>
        /// 保存整个业务的xml文档(补扫、重扫)
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public bool SaveAllXml(string xml)
        {

            try
            {
                if (!this.SaveXml(xml)) throw new Exception();

                SortDocindex();

                return true;
            }
            catch (Exception ex)
            {
                SysLog.Write(7108, ex, this._proIdAndThreadId);
                return false;
            }
        }

        /// <summary>
        /// 保存业务的部分xml到数据缓存
        /// 描述：根据外部接口分开传递的xml文档(包括buessness_info、docs_info、Pages)
        /// </summary>
        /// <param name="xml">xml字符串</param>
        public bool SaveXml(string xml)
        {
            try
            {
                this.LoadXml(xml);
                //dataManage.Clear();
                SaveBussinessNode(this.Root.SelectSingleNode("//resp_body//business_info"));
                return true;
            }
            catch (Exception ex)
            {
                SysLog.Write(7132, ex.Message, this._proIdAndThreadId);
                return false;
            }
        }

        public void SaveBussinessNode(XmlNode bussinessNode)
        {
            Models.Business business = new Models.Business();

            XmlNode biz_metadata1Node = bussinessNode.SelectSingleNode("biz_metadata");
            business.Biz_metadata1 = biz_metadata1Node != null ? biz_metadata1Node.InnerText : "";

            XmlNode biz_metadata2Node = bussinessNode.SelectSingleNode("biz_metadata2");
            business.Biz_metadata2 = biz_metadata2Node != null ? biz_metadata2Node.InnerText : "";

            XmlNode biz_metadata3Node = bussinessNode.SelectSingleNode("biz_metadata3");
            business.Biz_metadata3 = biz_metadata3Node != null ? biz_metadata3Node.InnerText : "";

            XmlNode achr_secNode = bussinessNode.SelectSingleNode("achr_sec");
            business.Achr_sec = achr_secNode != null ? achr_secNode.InnerText : "";

            XmlAttribute create_time = bussinessNode.Attributes["create_time"];
            business.Create_time = create_time != null ? create_time.InnerText : "";

            XmlNode image_library_ident = bussinessNode.SelectSingleNode("//image_library_ident");
            business.Image_library_ident = image_library_ident != null ? image_library_ident.InnerText : "";

           
            DataRow[] dataRows= ((BusinessTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.BusinessTable)).Select("Biz_metadata1" + "='" + business.Biz_metadata1 + "'");
            if (dataRows.Length > 0)
            {
                foreach (DataRow row in dataRows)
                {
                    ((BusinessTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.BusinessTable)).Rows.Remove(row);
                }
            }

            ((BusinessTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.BusinessTable)).AddRow(business);

            XmlNode docsInfoNode = bussinessNode.SelectSingleNode("doc_list");
            if (docsInfoNode != null)
            {
                SaveDocsNode(docsInfoNode, business.Biz_metadata1);
            }


        }

        private void SaveDocsNode(XmlNode docsInfoNode, string bussinessId)
        {
            foreach (XmlNode docNode in docsInfoNode.SelectNodes("docs_info"))
            {
                SaveDocNode(docNode, bussinessId);
            }
        }

        private string SaveDocNode(XmlNode docNode, string bussinessId)
        {
            Doc doc = new Doc();
            doc.Biz_metadata1 = bussinessId;
            XmlNode pkuuidNode = docNode.SelectSingleNode("pkuuid");
            doc.Pkuuid = pkuuidNode != null ? pkuuidNode.InnerText : "";

            if (string.IsNullOrEmpty(doc.Pkuuid))
            {
                doc.Pkuuid = Guid.NewGuid().ToString().Replace("-", "");
            }

            XmlNode uniq_metadataNode = docNode.SelectSingleNode("uniqMetadata");
            doc.Uniq_metadata = uniq_metadataNode != null ? uniq_metadataNode.InnerText : "";

            if (string.IsNullOrEmpty(doc.Uniq_metadata))
            {
                doc.Uniq_metadata = Guid.NewGuid().ToString().Replace("-", "");
            }
            XmlNode data_type = docNode.SelectSingleNode("data_type");
            doc.Data_type = data_type != null ? data_type.InnerText : "";


            XmlNode index_metadata1Node = docNode.SelectSingleNode("index_metadata1");
            doc.Index_metadata1 = index_metadata1Node != null ? index_metadata1Node.InnerText : "";

            XmlNode index_metadata2Node = docNode.SelectSingleNode("index_metadata2");
            doc.Index_metadata2 = index_metadata2Node != null ? index_metadata2Node.InnerText : "";


            XmlNode index_metadata3Node = docNode.SelectSingleNode("index_metadata3");
            doc.Index_metadata3 = index_metadata3Node != null ? index_metadata3Node.InnerText : "";

            XmlNode ext_metadataNode = docNode.SelectSingleNode("ext_metadata");
            doc.Ext_metadata = ext_metadataNode != null ? ext_metadataNode.InnerText : "";

            XmlNode cust_no = docNode.SelectSingleNode("cust_no");
            doc.Cust_no = cust_no != null ? cust_no.InnerText : "";

            XmlNode cust_name = docNode.SelectSingleNode("cust_name");
            doc.Cust_name = cust_name != null ? cust_name.InnerText : "";

            XmlNode cert_type = docNode.SelectSingleNode("cert_type");
            doc.Cert_type = cert_type != null ? cert_type.InnerText : "";

            XmlNode cert_no = docNode.SelectSingleNode("cert_no");
            doc.Cert_no = cert_no != null ? cert_no.InnerText : "";

            XmlNode security = docNode.SelectSingleNode("security");
            doc.Security = security != null ? security.InnerText : "";

            XmlNode expire_date = docNode.SelectSingleNode("expire_date");
            doc.Expire_date = expire_date != null ? expire_date.InnerText : "";

            XmlNode image_storage_mech = docNode.SelectSingleNode("image_storage_mech");
            doc.Image_storage_mech = image_storage_mech != null ? image_storage_mech.InnerText : "";

            XmlNode is_current_version = docNode.SelectSingleNode("is_current_version");
            doc.Is_current_version = is_current_version != null ? is_current_version.InnerText : "";

            XmlNode version_label = docNode.SelectSingleNode("version_label");
            doc.Version_label = version_label != null ? version_label.InnerText : "";

            XmlNode is_compress = docNode.SelectSingleNode("is_compress");
            doc.Is_compress = is_compress != null ? is_compress.InnerText : "";

            XmlNode modify_by = docNode.SelectSingleNode("modify_by");
            doc.Modify_by = modify_by != null ? modify_by.InnerText : "";

            XmlNode create_time = docNode.SelectSingleNode("create_time");
            doc.Create_time = create_time != null ? create_time.InnerText : "";
           
            XmlNode is_cust_info = docNode.SelectSingleNode("is_cust_info");
            doc.Is_cust_info = is_cust_info != null ? is_cust_info.InnerText : "";



            ((DocTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.DocTable)).AddRow(doc);

            XmlNode pagesNode = docNode.SelectSingleNode("page_list");
            if (pagesNode != null)
            {
                SavePagesNode(pagesNode, doc.Pkuuid);
            }

            XmlNode version_listNode = docNode.SelectSingleNode("version_list");
            if (version_listNode != null)
            {
                SaveVersion_listNode(version_listNode, doc.Pkuuid);
            }

            XmlNode delNodes=docNode.SelectSingleNode("delete_page");
            if (delNodes != null) {
                SaveDelNode_listNode(delNodes, doc.Pkuuid);
            }
            return doc.Pkuuid;
        }

        public void SavePagesNode(XmlNode pagesNode, string pkuuid)
        {
            foreach (XmlNode pageNode in pagesNode.SelectNodes("page_info"))
            {
                SavePageNode(pageNode, pkuuid, "");
            }
        }

        public Page SavePageNode(XmlNode pageinfoNode, string pkuuid, string realName)
        {
            Page page = new Page();
            page.Pkuuid = pkuuid;
            if (!string.IsNullOrEmpty(realName))
            {
                page.Realname = realName;
            }
            else
            {
                page.Realname = Guid.NewGuid().ToString().Replace("-", "");
            }

            XmlNode pageNode = pageinfoNode.SelectSingleNode("page");
            XmlNode file_name = pageNode.SelectSingleNode("file_name");
            string fileName= file_name != null ? file_name.InnerText : "";
            string xmlfileName = this.GetTransName(fileName);
            page.File_name = xmlfileName;

            XmlNode Page_index = pageNode.SelectSingleNode("page_index");
            page.Page_index = Page_index != null ? int.Parse(Page_index.InnerText) : -1;

            XmlNode doc_index = pageNode.SelectSingleNode("doc_index");
            page.Doc_index = doc_index != null ? int.Parse(doc_index.InnerText) : -1;

            XmlNode page_flag = pageNode.SelectSingleNode("page_flag");
            page.Page_flag = page_flag != null ? page_flag.InnerText : "";

            XmlNode page_uuid = pageNode.SelectSingleNode("page_flag");
            page.Page_uuid = page_uuid != null ? page_uuid.InnerText : "";

            XmlNode is_have_postil = pageNode.SelectSingleNode("page_flag");
            page.Is_have_postil = is_have_postil != null ? is_have_postil.InnerText : "";


           
            XmlNode postilNode = pageNode.SelectSingleNode("postil");
            if (postilNode != null)
            {
                page.PostilInfo = postilNode.OuterXml;
            }

            ((PageTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.PageTable)).AddRow(page);

            return page;
        }

      

        public void SaveDelNode_listNode(XmlNode del_listNode, string pkuuid)
        {
            foreach (XmlNode delNode in del_listNode.SelectNodes("pagedel"))
            {
                SaveDelNode(pkuuid, int.Parse(delNode.Attributes["page_index"].Value));
            }
        }

        public void SaveDelNode(string pkuuid,int pageIndex)
        {
            DeletedPage deletedPage = new DeletedPage();
            deletedPage.Pkuuid = pkuuid;
            deletedPage.Page_index = pageIndex;

            ((DeletedPageTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.DeletedPageTable)).AddRow(deletedPage);
        }

        public void SaveVersion_listNode(XmlNode version_listNode, string pkuuid)
        {
            foreach (XmlNode versionNode in version_listNode.SelectNodes("ver"))
            {
                SaveVersionNode(versionNode, pkuuid);
            }
        }

        public void SaveVersionNode(XmlNode versionNode, string pkuuid)
        {
            Version version = new Version();
            version.Pkuuid = pkuuid;
            XmlAttribute ver_no = versionNode.Attributes["ver_no"];
            version.Ver_no = ver_no != null ? ver_no.InnerText : "";
            version.Ver_info = versionNode.OuterXml;
            ((VersionTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.VersionTable)).AddRow(version);
        }

        public void AddDocs(string xml, string businessId)
        {
            this.LoadXml(xml);
            XmlNode docsInfoNode = this.Root.SelectSingleNode("//docs_info");
            if (docsInfoNode != null)
            {
                SaveDocsNode(docsInfoNode, businessId);
            }
            else
            {
                foreach (XmlNode docNode in this.Root.SelectNodes("//doc_info"))
                {
                    SaveDocNode(docNode, businessId);
                }
            }
        }

        public Page AddOnePage(string xml, string pkuuid, string realName)
        {
            this.LoadXml(xml);
            XmlNode pageInfoNode = this.Root.SelectSingleNode("//page");
            return SavePageNode(pageInfoNode, pkuuid, realName);
        }

        public string AddDoc(string xml, string businessId)
        {
            this.LoadXml(xml);
            return SaveDocNode(this.Root.SelectSingleNode("//doc_info"), businessId);


        }

        /// <summary>
        /// 对docindex重新排序
        /// </summary>
        public void SortDocindex()
        {
            try
            {
                DataRow[] docs = ((DocTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.DocTable)).Select();

                foreach (DataRow doc in docs)
                {
                    //this._dataServies.GetAllDocs();
                    DataRow[] drs_pages = ((PageTable)this._dataServies.GetDataTable(BusinessData.EnumType.TableType.PageTable)).
                        Select("pkuuid = \'" + doc["pkuuid"] + "\'", "doc_index asc");

                    bool isSort = false;
                    for (int i = 0; i < drs_pages.Length; i++)
                    {
                        if ((int)drs_pages[i]["doc_index"] != i + 1)
                        {
                            isSort = true;
                            drs_pages[i].BeginEdit();

                            drs_pages[i]["doc_index"] = i + 1;
                            drs_pages[i]["oper_type"] = "E";
                            UpdateModiValue(drs_pages[i]);

                            try
                            {
                                drs_pages[i].EndEdit();
                            }
                            catch (Exception ex)
                            {
                                SysLog.Write(7122, ex, this._proIdAndThreadId);
                                throw ex;
                            }

                        }
                    }

                    if (isSort)
                    {
                        doc.BeginEdit();

                        doc["oper_type"] = "E";
                        //doc["modi_time"] = this.Now;

                        try
                        {
                            doc.EndEdit();
                        }
                        catch (Exception ex)
                        {
                            //SysLog.Write(7122, ex, this._proIdAndThreadId);
                            throw ex;
                        }
                    }
                }


                this._dataServies.Commit();

            }
            catch (Exception ex)
            {
                this._dataServies.RollBack();
                //SysLog.Write(7122, ex, this._proIdAndThreadId);
            }
        }

        /// <summary>
        /// 修改Modi_range和Modi_time属性
        /// <param name="pageId">page_info节点编号</param>
        /// <param name="isM">是否元数据编辑(这里代表page_info节点本身)</param>
        /// </summary>
        private void UpdateModiValue(DataRow drs_page)
        {
            bool isM = true;
            string value = (string)drs_page["modi_range"];
            if (value == "A" || (isM && value == "M") || (!isM && value == "C")) return;
            if ((isM && value == "C") || (!isM && value == "M")) value = "A";
            if (value == "N") value = isM ? "M" : "C";

            drs_page["modi_range"] = value;

            //drs_page["modi_time"] = this.Now;

        }
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
    }
}
