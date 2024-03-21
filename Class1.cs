using Com.Boc.Icms.DoNetSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static DMSSocket.Doc;

namespace DMSSocket
{
    internal class Class1
    {

        


        //private void GetIndexXml(ref StringBuilder str, bool delIgnore)
        //{
        //    List<Business> businessTable = new  List<Business>();
        //    List<Doc> docTable = new List<Doc>();
        //    List<string> pageTable = new List<string>();
        //    List<string> deletedPageTable = new List<string>();
        //    List<string> versionTable = new List<string>();

        //    Business[] businesses = businessTable.ToArray();
        //    str.Append("<batch>");

        //    foreach (Business business in businesses)
        //    {

        //        //添加bussiness信息

        //        str.Append("<business_info>");
        //        str.Append("<biz_metadata1>" + business.Biz_metadata1 + "</biz_metadata1>");
        //        str.Append("<biz_metadata2>" + business.Biz_metadata2 + "</biz_metadata2>");
        //        str.Append("<biz_metadata3>" + business.Biz_metadata3 + "</biz_metadata3>");              
        //        str.Append("<achr_sec>" + business.Achr_sec + "</achr_sec>");
        //        str.Append("<image_library_ident>" + business.Image_library_ident + "</image_library_ident>");

        //        string qust = "";
        //        if (delIgnore)
        //        {
        //            qust = "Biz_metadata1='" + business.Biz_metadata1 + "' and oper_type <> 'I'";
        //        }
        //        else
        //        {
        //            qust = "Biz_metadata1='" + business.Biz_metadata1 + "'";
        //        }
        //        List<Doc> docs = null;
        //        /*if (delIgnore)
        //        {
        //            docs = docTable.Find(qust)
        //            .ToList().FindAll(d =>
        //            {
        //                if (d.Oper_type == "A") //排除无page的doc
        //                {
        //                    return pageTable.Find("pkuuid = \'" + d.Pkuuid + "\'").Length > 0;
        //                }
        //                else
        //                {
        //                    return true;
        //                }
        //            });
        //        }
        //        else
        //        {
        //            docs = docTable.Find(qust).ToList();
        //        }*/

        //        str.Append("<doc_list>");

        //        if (docs.Count > 0)
        //        {

        //            foreach (Doc doc in docs)
        //            {
        //                str.Append("<docs_info>");
        //                str.Append("<pkuuid>" + doc.Pkuuid + "</pkuuid>");
        //                str.Append("<data_type>" + doc.Data_type + "</data_type>");
        //                str.Append("<uniq_metadata>" + doc.Uniq_metadata + "</uniq_metadata>");
        //                str.Append("<index_metadata1>" + doc.Index_metadata1 + "</index_metadata1>");
        //                str.Append("<index_metadata2>" + doc.Index_metadata2 + "</index_metadata2>");
        //                str.Append("<index_metadata3>" + doc.Index_metadata3 + "</index_metadata3>");
        //                str.Append("<ext_metadata>" + doc.Ext_metadata + "</ext_metadata>");
        //                str.Append("<is_cust_info>" + doc.Is_cust_info + "</is_cust_info>");
        //                str.Append("<cust_no>" + doc.Cust_no + "</cust_no>");
        //                str.Append("<cust_name>" + doc.Cust_name + "</cust_name>");
        //                str.Append("<cert_type>" + doc.Cert_type + "</cert_type>");
        //                str.Append("<cert_no>" + doc.Cert_no + "</cert_no>");
        //                str.Append("<security>" + doc.Security + "</security>");
        //                str.Append("<expire_date>" + doc.Expire_date + "</expire_date>");
        //                str.Append("<image_storage_mech>" + doc.Image_storage_mech + "</image_storage_mech>");
        //                str.Append("<is_compress>" + doc.Is_compress + "</is_compress>");
                       


        //              /*  DeletedPage[] deletedPages = deletedPageTable.Find("pkuuid = \'" + doc.Pkuuid + "\'");

        //                if (deletedPages.Length > 0)
        //                {
        //                    str.Append("<delete_page>");
        //                    foreach (DeletedPage deletedPage in deletedPages)
        //                    {
        //                        str.Append("<pagedel page_index=" + "\"" + deletedPage.Page_index + "\"" + "/>");
        //                    }
        //                    str.Append("</delete_page>");
        //                }*/

        //                string pagequst = "";
        //                if (delIgnore)
        //                {
        //                    pagequst = "pkuuid='" + doc.Pkuuid + "' and oper_type <> 'I'";
        //                }
        //                else
        //                {
        //                    pagequst = "pkuuid='" + doc.Pkuuid + "'";
        //                }

        //                //Page[] pages = pageTable.Find("Pkuuid = \'" + doc.Pkuuid + "\' and  oper_type <> 'I'", "page_index asc");
        //                Page[] pages = pageTable.Find(pagequst, "page_index asc");
        //                //SortPageIndex(pageTable, doc.Pkuuid);

        //                str.Append("<page_list>");
        //                if (pages.Length > 0)
        //                {
        //                    foreach (Page page in pages)
        //                    {
        //                        str.Append("<page>");
        //                        str.Append("<doc_index>" + page.Doc_index + "</doc_index>");
        //                        str.Append("<page_flag>" + page.Page_flag + "</page_flag>");
        //                        str.Append("<page_uuid>" + page.Page_uuid + "</page_uuid>");
        //                        str.Append("<file_name>" + page.File_name + "</file_name>");
        //                        str.Append("<is_have_postil>" + page.Is_have_postil + "</is_have_postil>");




        //                        if (!string.IsNullOrEmpty(page.PostilInfo))
        //                        {
        //                            str.Append(page.PostilInfo);
        //                        }
        //                        str.Append("</page>");
        //                    }
        //                }

        //                str.Append("</page_list>");

        //                Version[] vers = versionTable.Find("pkuuid = \'" + doc.Pkuuid + "\'");

        //                if (vers.Length > 0)
        //                {
        //                    str.Append("<version_list>");
        //                    foreach (Version ver in vers)
        //                    {
        //                        str.Append(ver.Ver_info);

        //                    }
        //                    str.Append("</version_list>");
        //                }
        //                str.Append("</docs_info>");

        //            }
        //        }
        //        str.Append("</doc_list></business_info>");
        //    }

        //    str.Append("</batch>");
        //}
    
    }



    public class Business
    {
        public string Biz_metadata1
        {
            get;
            set;
        }

        public string Biz_metadata2
        {
            get;
            set;
        }

        public string Biz_metadata3
        {
            get;
            set;
        }

        public string Source_system
        {
            get;
            set;
        }

        public string Create_province
        {
            get;
            set;
        }

        public string Check_telno
        {
            get;
            set;
        }

        public string Achr_sec
        {
            get;
            set;
        }

        public string Cust_no
        {
            get;
            set;
        }

        public string Modi_time
        {
            get;
            set;
        }

        public string Modi_meta
        {
            get;
            set;
        }
        public string Image_library_ident
        {
            get;
            set;
        }

    }

    public class Doc
    {
        public string Pkuuid
        {
            get;
            set;
        }

        public string Biz_metadata1
        {
            get;
            set;
        }


        public string Uniq_metadata
        {
            get;
            set;
        }

        public string Index_metadata1
        {
            get;
            set;
        }

        public string Index_metadata2
        {
            get;
            set;
        }

        public string Index_metadata3
        {
            get;
            set;
        }

        public string Ext_metadata
        {
            get;
            set;
        }

        public string Oper_type
        {
            get;
            set;
        }

        public string Modi_time
        {
            get;
            set;
        }

        public string Modi_meta
        {
            get;
            set;
        }

        public string Data_type
        {
            get;
            set;
        }


        public string Full_index
        {
            get;
            set;
        }

        public string New_version
        {
            get;
            set;
        }

        public string Expire_date
        {
            get;
            set;
        }

        public string Security
        {
            get;
            set;
        }


        public string Archieved
        {
            get;
            set;
        }     
        
        public string Is_cust_info
        {
            get;
            set;
        }  
        
        public string Cust_no
        {
            get;
            set;
        }
        
        public string Cust_name
        {
            get;
            set;
        }
        
        public string Cert_type
        {
            get;
            set;
        }
        
        public string Cert_no
        {
            get;
            set;
        } 
        
        public string Image_storage_mech
        {
            get;
            set;
        }
        
        public string Is_compress
        {
            get;
            set;
        }

        public class Page
        {
            private string realname = "";

            private string pkuuid = "";


            private string file_name = "";

            private int page_index = -1;

            private int doc_index = -1;

            private int old_doc_index = -1;

            private string page_flag = "";

            private string old_page_flag = "";

            private string modi_time = "";

            private string oper_type = "";

            private string modi_range = "";

            private string postilInfo = "";
            private string page_uuid = "";
            private string is_have_postil = "";




            public string Realname { get => realname; set => realname = value; }
            public string Pkuuid { get => pkuuid; set => pkuuid = value; }
            public string File_name { get => TransNameToFileName(file_name); set => file_name = value; }
            public int Page_index { get => page_index; set => page_index = value; }
            public int Doc_index { get => doc_index; set => doc_index = value; }
            public int Old_doc_index { get => old_doc_index; set => old_doc_index = value; }
            public string Page_flag { get => page_flag; set => page_flag = value; }
            public string Old_page_flag { get => old_page_flag; set => old_page_flag = value; }
            public string Modi_time { get => modi_time; set => modi_time = value; }
            public string Oper_type { get => oper_type; set => oper_type = value; }
            public string Modi_range { get => modi_range; set => modi_range = value; }
            public string PostilInfo { get => postilInfo; set => postilInfo = value; }
            public string Page_uuid { get => page_uuid; set => page_uuid = value; }
            public string Is_have_postil { get => is_have_postil; set => is_have_postil = value; }



            private string TransNameToFileName(string oldFileName)
            {
                string result = oldFileName.Replace("&amp;", "&");
                return result;
            }
        }

    }
}
