using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;
using Com.Boc.Icms.GlobalResource;


namespace Com.Boc.Icms.LogDLL
{
    /// <summary>
    /// 写入系统日志
    /// 描述：调用此日志模块，需要在注册表Microsoft.Win32.Registry.LocalMachine
    /// 写入SOFTWARE\Founder\LogDLL\Name(系统日志源名称)、
    /// SOFTWARE\Founder\LogDLL\Path(日志配置XML文件路径)
    /// 更新：
    /// 支持注册表路径：公司名\系统名\版本号\程序集名称\属性
    /// 路径所需值，根据项目需要变动。
    /// 如需进一步扩展，可从配置文件中获取。
    /// </summary>
    public delegate void DisplayExMessage(string text);
    public class SysLog
    {
        /// <summary>
        /// 日志XML文档
        /// </summary>
        private readonly XmlDocument _xmlDoc = null;

        /// <summary>
        /// 本类唯一实例
        /// </summary>
        private static readonly SysLog _instanceSysLog = null;

        /// <summary>
        /// 维护本类唯一实例的锁
        /// </summary>
        private static readonly object Object = new object();

        /// <summary>
        /// 产品所属公司名称
        /// </summary>
        private static readonly string CompanyName = string.Empty;

        /// <summary>
        /// 产品名称
        /// </summary>
        private static readonly string SystemName = string.Empty;

        /// <summary>
        /// 项目版本号
        /// </summary>
        private static readonly string VersionNo = string.Empty;

        /// <summary>
        /// 日志记录的行号
        /// </summary>
        private int _lineNum = 0;

        /// <summary>
        /// 日志记录的类名称
        /// </summary>
        private string _className = string.Empty;

        /// <summary>
        /// 日志记录的方法名称
        /// </summary>
        private string _methodName = string.Empty;

        /// <summary>
        /// 当前程序集名称
        /// </summary>
        private static readonly string AssemblyName = string.Empty;
        public static DisplayExMessage disExMessage = null;

        static SysLog()
        {
            CompanyName = GlobalFinalProperties.CompanyName;
            SystemName = GlobalFinalProperties.SystemName;
            VersionNo = GlobalFinalProperties.VersionNo;
            AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            string path = Path.Combine(GlobalFinalProperties.GlobalInstallPath, AssemblyName);
            DeleteHisLog(path, "*.log", "^[0-9]{8}\\w*.log$");

            if (_instanceSysLog == null)
            {
                lock (Object)
                {
                    /*去掉写日志线程*/
                   // FastLogger.Instance().Register();
                  
                    _instanceSysLog = new SysLog();                
                }
            }
        }

        /// <summary>
        /// 系统日志构造函数
        /// </summary>
        SysLog()
        {
            try
            {
                //事件源
                string source = string.Empty;
                //日志XML配置文件路径
                string logXmlPath = string.Empty;


                 logXmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "LogMsg.xml");

                this._xmlDoc = new XmlDocument();
                this._xmlDoc.Load(logXmlPath);
            }
            catch (Exception ex)
            {
                //初始化日志失败，失败原因：
                //MessageBox.Show(GetMessage(9907, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 记录系统日志
        /// </summary>
        /// <param name="id">日志编号</param>
        /// <param name="values">替换参数值数组</param>
        public static void Write(int id, string strId, params string[] values)
        {
            try
            {
                //todo
               
                 _instanceSysLog.GetRecordInformation(2);
                 _instanceSysLog.Write(id, LogEnum.EventType.None, false, strId, values);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// 记录系统日志
        /// </summary>
        /// <param name="id">日志编号</param>
        /// <param name="e">异常信息</param>
        /// <param name="values">替换参数值数组</param>
        public static void Write(int id, Exception e, string strId)
        {
            try
            {
                //todo
                _instanceSysLog.GetRecordInformation(e);
                _instanceSysLog.Write(id, LogEnum.EventType.None, true, strId, e.Message + "\r\n" + e.StackTrace);
                if (disExMessage != null)
                    disExMessage(e.Message + "\r\n" + e.StackTrace);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 记录系统日志
        /// </summary>
        /// <param name="id">日志编号</param>
        /// <param name="eventType">事件类型(无、安装、其他)</param>
        /// <param name="ifException">是否为异常</param>
        /// <param name="strId">行号</param>
        /// <param name="values">替换参数值数组</param>
        private void Write(int id, LogEnum.EventType eventType, bool ifException, string strId, params string[] values)
        {
            try
            {
                string msg;
                if (ifException)
                {
                    string xmlMsg = AccessResource.GetInstance().GetValue(id.ToString());

                    if (!xmlMsg.EndsWith("%s"))
                    {
                        xmlMsg += "Exception message:%s";
                    }

                    msg = ReplaceParam(xmlMsg, values);
                }
                else
                {
                    msg = ReplaceParam(AccessResource.GetInstance().GetValue(id.ToString()), values);
                }

                //显示的消息信息
                msg = "PID:" + strId +
                    "\rDateTime:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") +
                    "\rclassName:" + this._className +
                    "\rmethodName:" + this._methodName +
                    "\rLineNo:" + this._lineNum +
                    "\rMessage:" + msg;

                //string logID = AddStartZero(id);
                //string className = logID.Substring(0, 4);
                //string errorID = logID.Substring(4, 4);

                XmlNode node = this.GetMsgNode(id.ToString());

                //判断日志级别
                string logLv = node.Attributes["LogLv"].Value;
                string logSwitch = node.ParentNode.Attributes["LogSwitch"].Value;

                if (!string.IsNullOrEmpty(logLv) && !string.IsNullOrEmpty(logSwitch))
                {
                    if (int.Parse(logLv) > int.Parse(logSwitch))
                    {
                        return;
                    }
                }

                //判断写入日志的事件类型
                string logType = node.Attributes["EventLogEntryType"].Value;
                string display = node.ParentNode.Attributes["Display"].Value;

                if (!display.Split(',').Any(a => a == logType.ToString()))
                {
                    return;
                }

                //写入事件查看器
                if (AccessResource.IfWriteEvtLog)
                {
                    //进行日志写入操作
                    //EventLogEntryType type = (EventLogEntryType)int.Parse(logType);
                    //string msg = ReplaceParam(node.InnerText);
                    //string msg = GetMessage(int.Parse(errorID), values);

                    if (msg.Length > 32766)
                    {
                        msg = msg.Substring(0, 31885);
                    }
                    //this._eventLog.WriteEntry(msg, type, id, (short)eventType);

                    FastLogLevel logLevel = FastLogLevel.Info;
                    switch (logType)
                    {
                        case "1":
                            logLevel = FastLogLevel.Error;
                            break;
                        case "2":
                            logLevel = FastLogLevel.Warn;
                            break;
                        case "4":
                            logLevel = FastLogLevel.Info;
                            break;
                    }

                    WriteEventLog(msg, logLevel, id);
                }

                //写入系统日志
                if (AccessResource.IfWriteTxtLog)
                {
                    string logLevel = "";
                    switch (logType)
                    {
                        case "1":
                            logLevel = "Error ";
                            break;
                        case "2":
                            logLevel = "Warn  ";
                            break;
                        case "4":
                            logLevel = "Info  ";
                            break;
                        default:
                            break;
                    }
                    //WriteTextFile(logLevel + msg.Replace("\r", "\t"), strId);

                    WriteLogFile(logLevel + msg.Replace("\r", "\t"), FastLogType.MainLog);
                }
            }
            catch (Exception ex)
            {
                //记录系统日志失败，失败原因：
                //MessageBox.Show(GetMessage(9908, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
                AccessResource.IfWriteEvtLog = false;
                AccessResource.IfWriteTxtLog = true;
            }
        }

        /// <summary>
        /// 根据错误编码查询错误信息节点
        /// </summary>
        /// <param name="id">消息编号</param>
        private XmlNode GetMsgNode(string id)
        {
            return this._xmlDoc.DocumentElement.SelectSingleNode("Message/Entry[@ID=\"" + id + "\"]");
        }

        /// <summary>
        /// 日志编号补零
        /// </summary>
        /// <param name="id">日志编号</param>
        /// <returns></returns>
        private string AddStartZero(int id)
        {
            string zerostr = "0000";
            string errorId = id.ToString();
            return (errorId.Length < 4) ? (zerostr.Substring(0, 4 - errorId.Length) + errorId) : errorId;
        }

        /// <summary>
        /// 检索注册表值
        /// </summary>
        /// <param name="rPath">注册表目录路径</param>
        /// <param name="keyName">注册表键</param>
        /// <returns>注册表值</returns>
        private object GetRegistryValue(string rPath, string keyName)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(rPath);

            if (key != null)
            {
                return key.GetValue(keyName);
            }
            return null;
        }

        /// <summary>
        /// 获取日志消息
        /// </summary>
        /// <param name="id">消息编号</param>
        /// <returns></returns>
        public static string GetMessage(int id)
        {
            try
            {
                //XmlNode xn = GetMsgNode(id.ToString());
                //return xn.InnerText;
                return AccessResource.GetInstance().GetValue(id.ToString());
            }
            catch (Exception ex)
            {
                //获取日志消息失败，失败原因：
                //MessageBox.Show(GetMessage(9909, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取日志消息
        /// </summary>
        /// <param name="id">消息编号</param>
        /// <param name="values">替换参数值数组</param>
        /// <returns></returns>
        public static string GetMessage(int id, params string[] values)
        {
            return ReplaceParam(GetMessage(id), values);
        }

        /// <summary>
        /// 替换日志消息参数
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="values">替换参数值数组</param>
        /// <returns></returns>
        public static string ReplaceParam(string msg, params string[] values)
        {
            if (values != null && values.Length > 0)
            {
                int i = 0;

                msg = Regex.Replace(msg,
                    "%s",
                    delegate
                    {
                        try
                        {
                            return values[i++];
                        }
                        catch (Exception ex)
                        {
                            //替换日志消息参数异常，异常原因：
                            //MessageBox.Show(GetMessage(9910, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return string.Empty;
                        }
                    });
            }

            return msg;
        }

        #region 注释
        ///// <summary>
        ///// 取得正常代码行编号
        ///// </summary>
        ///// <param name="sf">当前线程的调用堆栈中的一个函数调用</param>
        ///// <returns>代码行编号</returns>
        //public static int GetFileLineNumber(StackFrame frame)
        //{
        //    StackTrace st = new StackTrace(frame);
        //    frame = st.GetFrame(0);
        //    return frame.GetFileLineNumber();
        //}

        ///// <summary>
        ///// 取得异常代码行编号
        ///// </summary>
        ///// <param name="e">异常类对象</param>
        ///// <returns>代码行编号</returns>
        //public static int GetFileLineNumber(Exception ex)
        //{
        //    StackTrace st = new StackTrace(ex, true);
        //    StackFrame frame = st.GetFrame(0);
        //    return frame.GetFileLineNumber();
        //}
        #endregion

        ///// <summary>
        ///// 将消息写入文本文件
        ///// </summary>
        ///// <param name="id">消息编号</param>
        ///// <param name="values">替换参数值数组</param>
        //public static void WriteTextFile(int id, params string[] values)
        //{
        //    WriteTextFile(GetMessage(id, values));
        //}

        /// <summary>
        /// 将消息写入文本文件
        /// </summary>
        /// <param name="msg">消息</param>
        /// <remarks>
        /// 路径：当前目录\日志程序集名称\源写日志程序集名称\年\月\日.txt
        /// 文件不存在，则创建文件
        /// </remarks>
        public static void WriteTextFile(string msg, string pid)
        {
            try
            {
                string str = string.Empty;

                #region 注释
                //try
                //{
                //    //目前还没有找到有效的方式能取到网页里调用的DLL名称
                //    str = Assembly.GetEntryAssembly().GetName().Name;
                //    if (str == assemblyName) str = "ImageMostControl";
                //}
                //catch
                //{
                //    str = "ImageMostControl";
                //}

                ////Environment.CurrentDirectory 在IE的情况下当前目录属于IE的临时缓存目录
                //string path = GlobalFinalProperties.GlobalInstallPath + "\\"
                //    + assemblyName + "\\"
                //    + str + "\\"
                //    + DateTime.Now.Year + "\\"
                //    + DateTime.Now.Month;
                #endregion

                string path = Path.Combine(GlobalFinalProperties.GlobalInstallPath, AssemblyName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path += DateTime.Now.ToString("yyyyMMdd") + "_" + pid + ".log";

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(msg);
                    sw.Close();
                }

            }
            catch (Exception ex)
            {
                //将消息写入文本文件失败，失败原因：
                //MessageBox.Show(GetMessage(9911, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void WriteLogFile(string msg, FastLogType logType)
        {
            //todo
            FastLogger.Info(msg, logType);
        }

        public static void WriteEventLog(string msg, FastLogLevel fastLogLevel, int eventId)
        {
            switch (fastLogLevel)
            {
                case FastLogLevel.Debug:
                    FastLogger.Debug(msg, FastLogType.EventLog, eventId: eventId);
                    break;
                case FastLogLevel.Info:
                    FastLogger.Info(msg, FastLogType.EventLog, eventId: eventId);
                    break;
                case FastLogLevel.Error:
                    FastLogger.Error(msg, FastLogType.EventLog, eventId: eventId);
                    break;
                case FastLogLevel.Warn:
                    FastLogger.Warn(msg, FastLogType.EventLog, eventId: eventId);
                    break;
                case FastLogLevel.Fatal:
                    FastLogger.Fatal(msg, FastLogType.EventLog, eventId: eventId);
                    break;
            }
        }

        /// <summary>
        /// 取得正常代码行需要记录行号、文件名、方法名信息
        /// </summary>
        /// <param name="skipFrames">堆栈中的帧数，将从其上开始跟踪。</param>
        private void GetRecordInformation(int skipFrames)
        {
            try
            {
                StackTrace st = new StackTrace(skipFrames, true);
                this._lineNum = st.GetFrame(0).GetFileLineNumber();
                string fullPath = st.GetFrame(0).GetFileName();
                if (fullPath != null)
                {
                    this._className = fullPath.Substring(fullPath.LastIndexOf("\\") + 1, fullPath.Length - fullPath.LastIndexOf("\\") - 1);
                }

                this._methodName = st.GetFrame(0).GetMethod().Name;
            }
            catch (Exception ex)
            {
                //取得正常代码行失败，失败原因：
                //MessageBox.Show(GetMessage(9912, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 取得异常代码行需要记录行号、文件名、方法名信息
        /// </summary>
        /// <param name="skipFrames">堆栈中的帧数，将从其上开始跟踪。</param>
        private void GetRecordInformation(Exception e)
        {
            try
            {
                StackTrace st = new StackTrace(e, true);
                int framesLength = st.GetFrames().Length;
                this._lineNum = st.GetFrame(framesLength - 1).GetFileLineNumber();
                string fullPath = st.GetFrame(framesLength - 1).GetFileName();
                if (fullPath != null)
                {
                    this._className = fullPath.Substring(fullPath.LastIndexOf("\\") + 1, fullPath.Length - fullPath.LastIndexOf("\\") - 1);
                }

                this._methodName = st.GetFrame(framesLength - 1).GetMethod().Name;
            }
            catch (Exception ex)
            {
                //取得异常代码行失败，失败原因：
                //MessageBox.Show(GetMessage(9913, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void WriteTxtLog(string logstr, string pid)
        {
            try
            {
                //todo
                _instanceSysLog.GetRecordInformation(2);
                string errorId = logstr.Substring(0, 4);
                string substr = logstr.Substring(5);
                _instanceSysLog.WriteSdkStr(int.Parse(errorId), LogEnum.EventType.None, substr,pid);
            }
            catch (Exception ex)
            {
                //写入文本日志异常，异常原因：
                //MessageBox.Show(GetMessage(9914, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //public static void WriteTxtLog(string logstr, Exception e)
        //{
        //    try
        //    {
        //        instanceSysLog.GetRecordInformation(e);
        //        string errorID = logstr.Substring(0, 4);
        //        string substr = logstr.Substring(5);
        //        instanceSysLog.WriteSdkStr(int.Parse(errorID), LogEnum.EventType.None, substr);
        //    }
        //    catch (Exception ex)
        //    {
        //        //写入文本日志异常，异常原因：
        //        MessageBox.Show(GetMessage(9914, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void WriteSdkStr(int id, LogEnum.EventType eventType, string logstr, string pid)
        {
            try
            {
                XmlNode node = this.GetMsgNode(id.ToString());
                //判断日志级别
                string logLv = node.Attributes["LogLv"].Value;
                string logSwitch = node.ParentNode.Attributes["LogSwitch"].Value;

                //判断写入日志的事件类型
                string logType = node.Attributes["EventLogEntryType"].Value;

                if (!string.IsNullOrEmpty(logLv) && !string.IsNullOrEmpty(logSwitch))
                {
                    if (int.Parse(logLv) > int.Parse(logSwitch))
                    {
                        return;
                    }
                }

                //显示的消息信息
                logstr = "PID:" + pid +
                    "\rDateTime:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") +
                    "\rclassName:" + this._className +
                    "\rmethodName:" + this._methodName +
                    "\rLineNo:" + this._lineNum +
                    "\rMessage:" + logstr;

                if (AccessResource.IfWriteEvtLog)
                {
                    //进行日志写入操作
                    //EventLogEntryType type = (EventLogEntryType)int.Parse(logType);

                    //写入系统日志
                    if (logstr.Length > 32760)
                    {
                        logstr = logstr.Substring(0, 32760) + "......";
                    }

                    //this._eventLog.WriteEntry(logstr, type, id, (short)eventType);

                    FastLogLevel logLevel = FastLogLevel.Info;
                    switch (logType)
                    {
                        case "1":
                            logLevel = FastLogLevel.Error;
                            break;
                        case "2":
                            logLevel = FastLogLevel.Warn;
                            break;
                        case "4":
                            logLevel = FastLogLevel.Info;
                            break;
                    }

                    WriteEventLog(logstr, logLevel, id);
                }

                if (AccessResource.IfWriteTxtLog)
                {
                    string logLevel = "";
                    switch (logType)
                    {
                        case "1":
                            logLevel = "Error ";
                            break;
                        case "2":
                            logLevel = "Warn  ";
                            break;
                        case "4":
                            logLevel = "Info  ";
                            break;
                        default:
                            break;
                    }

                    //WriteTextFile(logstr.Replace("\r", "\t"),pid);

                    WriteLogFile(logLevel + logstr.Replace("\r", "\t"), FastLogType.MainLog);
                }
            }
            catch (Exception ex)
            {
                //SDK写入日志失败，失败原因：
                //MessageBox.Show(GetMessage(9915, ex.Message), GetMessage(28), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal static void DeleteHisLog(string dirPath, string whoselog, string regexStr)
        {
            try
            {
                if (Directory.Exists(dirPath) == false)
                {
                    return;
                }

                int pos = whoselog.IndexOf('*');
                DateTime startDateTime = DateTime.Now.AddDays(-10);
                int deathDate = int.Parse(startDateTime.ToString("yyyyMMdd"));
                string[] filenames = Directory.GetFiles(dirPath, whoselog);
                foreach (string filename in filenames)
                {
                    FileInfo file = new FileInfo(filename);
                    if (Regex.IsMatch(file.Name, regexStr))
                    {
                        string date = file.Name.Substring(pos, 8);
                        if (int.Parse(date) < deathDate)
                        {
                            file.Attributes = FileAttributes.Archive;
                            file.Delete();
                        }
                    }
                }
            }
            catch (Exception e)
            { }
        }

    }
}
