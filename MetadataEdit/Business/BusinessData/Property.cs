using System;

namespace Com.Boc.Icms.MetadataEdit.Business.BusinessData
{
    /// <summary>
    /// 业务属性类
    /// </summary>
    [Serializable]
    class Property
    {
        //private static Property instance;

        //public static Property Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new Property();

        //            return instance;
        //        }
        //        else
        //            return instance;
        //    }
        //}

        private int _businessId = 0;

        /// <summary>
        /// 业务Business_info节点在数据结构中的编号
        /// </summary>
        public int BusinessId
        {
            get { return this._businessId; }
            set
            {
                if (this._businessId != value)
                {
                    _businessIdChanged = true;
                }
                else
                {
                    _businessIdChanged = false;
                }
                this._businessId = value;
            }
        }


        private bool _businessIdChanged = false;
        public bool BusinessIdChanged
        {
            get { return this._businessIdChanged; }
        }


        private EnumType.EnumWorkType _workType = EnumType.EnumWorkType.InitScan;

        /// <summary>
        /// 业务当前的工作类型
        /// </summary>
        public EnumType.EnumWorkType WorkType
        {
            get { return this._workType; }
            set { this._workType = value; }
        }

        private EnumType.EnumPageFlagMode _pageFlagMode = EnumType.EnumPageFlagMode.FrontAll;

        /// <summary>
        /// 业务当前的正反模式
        /// </summary>
        public EnumType.EnumPageFlagMode PageFlagMode
        {
            get { return this._pageFlagMode; }
            set { this._pageFlagMode = value; }
        }
    }
}
