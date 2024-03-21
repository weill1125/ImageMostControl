using log4net;
using log4net.Config;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Com.Boc.Icms.DoNetSDK.Log
{
    public sealed class FastLogger
    {
        private readonly ConcurrentQueue<FastLogMessage> messageQueue;

        private Thread thread;

        /// <summary>
        /// 控件日志
        /// </summary>
        private readonly ILog _mainlog;

        /// <summary>
        /// sdk日志
        /// </summary>
        private readonly ILog _sdklog;

        /// <summary>
        /// 事件查看器日志
        /// </summary>
        private readonly ILog _eventlog;

        /// <summary>
        /// 日志
        /// </summary>
        private static FastLogger _fastLog;

        private volatile static bool IsProcessing = false;

        /// <summary>
        /// 信号
        /// </summary>
        private readonly ManualResetEvent _mre;


        private FastLogger()
        {            

            var configFile = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "sdk_log4net.config"));
            
            if (configFile.Exists)
            {
                // 设置日志配置文件路径
                XmlConfigurator.ConfigureAndWatch(configFile);
                //  throw new Exception("未配置log4net配置文件！");
            }
            messageQueue = new ConcurrentQueue<FastLogMessage>();
            _mre = new ManualResetEvent(false);            
            _sdklog = log4net.LogManager.GetLogger("SdkLog");  //记录Sdk日志                        
        }

        /// <summary>
        /// 实现单例
        /// </summary>
        /// <returns></returns>
        public static FastLogger Instance()
        {
            if (_fastLog == null)
                _fastLog = new FastLogger();
            return _fastLog;
        }

        public void Register()
        {

            thread = new Thread(Trigger);
            thread.IsBackground = true;

            thread.Start();
        }
       
        private void EnqueueMessage(string message, FastLogLevel fastLogLevel = FastLogLevel.Info, FastLogType logType = FastLogType.MainLog, Exception ex = null, int eventId = 0)
        {
            messageQueue.Enqueue(new FastLogMessage()
            {
                Message = message,
                Level = fastLogLevel,
                LogType = logType,
                Exception = ex,
                EventId = eventId
            });
            _mre.Set();
            Trigger();
        }

        public void Trigger()
        {
            _mre.WaitOne();
            if (IsProcessing)
            {
                return;
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    WriteLog();
                });
            }
        }

        private void WriteLog()
        {
            while (messageQueue.TryDequeue(out FastLogMessage msg))
            {
                IsProcessing = true;

                switch (msg.Level)
                {
                    case FastLogLevel.Debug:
                        if (msg.LogType == FastLogType.MainLog && _mainlog.IsDebugEnabled)
                            _mainlog.Debug(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.SdkLog && _sdklog.IsDebugEnabled)
                            _sdklog.Debug(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.EventLog && _eventlog.IsDebugEnabled)
                        {
                            ThreadContext.Properties["EventID"] = msg.EventId;
                            _eventlog.Debug(msg.Message, msg.Exception);
                            log4net.ThreadContext.Properties["EventID"] = 0;
                        }
                        break;
                    case FastLogLevel.Info:
                        if (msg.LogType == FastLogType.MainLog && _mainlog.IsInfoEnabled)
                            _mainlog.Info(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.SdkLog && _sdklog.IsInfoEnabled)
                            _sdklog.Info(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.EventLog && _eventlog.IsInfoEnabled)
                        {
                            ThreadContext.Properties["EventID"] = msg.EventId;
                            _eventlog.Info(msg.Message, msg.Exception);
                            ThreadContext.Properties["EventID"] = 0;
                        }
                        break;
                    case FastLogLevel.Error:
                        if (msg.LogType == FastLogType.MainLog && _mainlog.IsErrorEnabled)
                            _mainlog.Error(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.SdkLog && _sdklog.IsErrorEnabled)
                            _sdklog.Error(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.EventLog && _eventlog.IsErrorEnabled)
                        {
                            ThreadContext.Properties["EventID"] = msg.EventId;
                            _eventlog.Error(msg.Message, msg.Exception);
                            ThreadContext.Properties["EventID"] = 0;
                        }
                        break;
                    case FastLogLevel.Warn:
                        if (msg.LogType == FastLogType.MainLog && _mainlog.IsWarnEnabled)
                            _mainlog.Warn(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.SdkLog && _sdklog.IsWarnEnabled)
                            _sdklog.Warn(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.EventLog && _eventlog.IsWarnEnabled)
                        {
                            ThreadContext.Properties["EventID"] = msg.EventId;
                            _eventlog.Warn(msg.Message, msg.Exception);
                            ThreadContext.Properties["EventID"] = 0;
                        }
                        break;
                    case FastLogLevel.Fatal:
                        if (msg.LogType == FastLogType.MainLog && _mainlog.IsFatalEnabled)
                            _mainlog.Fatal(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.SdkLog && _sdklog.IsFatalEnabled)
                            _sdklog.Fatal(msg.Message, msg.Exception);
                        else if (msg.LogType == FastLogType.EventLog && _eventlog.IsFatalEnabled)
                        {
                            ThreadContext.Properties["EventID"] = msg.EventId;
                            _eventlog.Fatal(msg.Message, msg.Exception);
                            ThreadContext.Properties["EventID"] = 0;
                        }
                        break;
                }

                msg.Dispose();


            }
            _mre.Reset();
            Thread.Sleep(1);
            IsProcessing = false;
        }

        public static void Debug(string msg, FastLogType logType = FastLogType.MainLog, Exception ex = null, int eventId = 0)
        {
            Instance().EnqueueMessage(msg, FastLogLevel.Debug, logType, ex, eventId);
        }

        public static void Error(string msg, FastLogType logType = FastLogType.MainLog, Exception ex = null, int eventId = 0)
        {
            Instance().EnqueueMessage(msg, FastLogLevel.Error, logType, ex, eventId);
        }

        public static void Fatal(string msg, FastLogType logType = FastLogType.MainLog, Exception ex = null, int eventId = 0)
        {
            Instance().EnqueueMessage(msg, FastLogLevel.Fatal, logType, ex, eventId);
        }

        public static void Info(string msg, FastLogType logType = FastLogType.MainLog, Exception ex = null, int eventId = 0)
        {
            Instance().EnqueueMessage(msg, FastLogLevel.Info, logType, ex, eventId);
        }

        public static void Warn(string msg, FastLogType logType = FastLogType.MainLog, Exception ex = null, int eventId = 0)
        {
            Instance().EnqueueMessage(msg, FastLogLevel.Warn, logType, ex, eventId);
        }
    }
}