using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Boc.Icms.DoNetSDK.Log
{


    /// <summary>
    /// 日志内容
    /// </summary>
    public class FastLogMessage:IDisposable
    {
        public string Message { get; set; }
        public FastLogLevel Level { get; set; }
        public Exception Exception { get; set; }
        public FastLogType LogType { get; set; }
        public int EventId { get; set; }


        public void Dispose()
        {
            this.Exception = null;
            this.Message = null;
        }
    }
}
