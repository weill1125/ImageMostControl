using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Boc.Icms.MetadataEdit.Models
{
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

        private string pageuuid = "";

        private string ishavepostil = "";




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
        public string Page_uuid { get => pageuuid; set => pageuuid = value; }
        public string Is_have_postil { get => ishavepostil; set => ishavepostil = value; }



        private string TransNameToFileName(string oldFileName)
        {            
            string result = oldFileName.Replace("&amp;", "&");
            return result;
        }
    }
}
