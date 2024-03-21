using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.Boc.Icms.LogDLL
{

    /// <summary>
    /// 日志等级
    /// </summary>
    public enum FastLogLevel
    {
        Debug,
        Info,
        Error,
        Warn,
        Fatal
    }

    public enum FastLogType
    { 
        SdkLog,
        MainLog,
        EventLog
    }
}
