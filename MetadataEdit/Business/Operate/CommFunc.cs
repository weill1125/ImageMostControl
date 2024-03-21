using System;
using Com.Boc.Icms .MetadataEdit.Business.BusinessData;
using Com.Boc.Icms .MetadataEdit.Business.BusinessData;

namespace Com.Boc.Icms .MetadataEdit.Business.Operate
{
    /// <summary>
    /// 业务公共操作类
    /// </summary>
    class CommFunc
    {
        private readonly Property _property = null;
        private readonly string _proIdAndThreadId = string.Empty;

        public CommFunc(Property property)
        {
            this._property = property;
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        /// <summary>
        /// 切换业务工作类型
        /// 描述：设置当前的WorkType，并且取得当前的Business_info编号
        /// </summary>
        /// <param name="type">业务工作类型</param>
        /// <param name="index">交易节点索引</param>
        public bool ChangeWorkType(EnumType.EnumWorkType type)
        {
            try
            {
                this._property.WorkType = type;

                return true;
            }
            catch (Exception ex)
            {
                //SysLog.Write(7129, ex, this._proIdAndThreadId);
                return false;
            }
        }

    }
}
