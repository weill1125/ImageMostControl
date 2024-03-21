using Com.Boc.Icms.MetadataEdit.Support.GlobalCache;

namespace Com.Boc.Icms.MetadataEdit.Business.BusinessData
{
    public class EnumType : SupportEnumType
    {
        /// <summary>
        /// 业务工作类型
        /// </summary>
        public enum EnumWorkType : int
        {
            /// <summary>
            /// 初始添加图像
            /// </summary>
            InitScan = 0,

            /// <summary>
            /// 补扫图像
            /// </summary>
            SupplyScan = 1,

            /// <summary>
            /// 重扫图像
            /// </summary>
            RenewScan = 2
        }

        /// <summary>
        /// 业务正反模式
        /// </summary>
        public enum EnumPageFlagMode : int
        { 
            /// <summary>
            /// 正面模式
            /// </summary>
            FrontAll = 0,

            /// <summary>
            /// 正反模式
            /// </summary>
            FrontBreak = 1
        }

        public enum EnumIndex
        {
            DocIndex,

            PageIndex
        }

        //public enum NodeType
        //{
        //    Business,

        //    Pkuuid,

        //    Page
        //}

        public enum TableType
        {
            BusinessTable,

            DeletedPageTable,

            DocTable,

            PageTable,

            VersionTable
        }
    }
}
