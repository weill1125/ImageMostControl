namespace Com.Boc.Icms.LogDLL
{
    /// <summary>
    /// 日志枚举类
    /// </summary>
    public class LogEnum
    {
        /// <summary>
        /// 事件类型
        /// EventType: 1无 2安装 4其他
        /// </summary>
        public enum EventType : int
        {
            /// <summary>
            /// 无
            /// </summary>
            None = 1,

            /// <summary>
            /// 安装
            /// </summary>
            Build = 2,

            /// <summary>
            /// 其他
            /// </summary>
            Other = 4
        }
    }
}
