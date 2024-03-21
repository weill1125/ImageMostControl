using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace Com.Boc.Icms.GlobalResource
{
    public class AccessResource
    {
        /// <summary>
        /// 本类唯一实例
        /// </summary>
        public static AccessResource InstanceResource = null;

        /// <summary>
        /// 维护本类唯一实例的锁
        /// </summary>
        private static readonly object Object = new object();

        public static string _language = "EN";
        public static string LanguageResource
        {
            get { return _language; }
            set
            {
                _language = value;
                if (Xmldoc != null)
                {
                    string rPath = GlobalFinalProperties.GlobalRegeditPath;
                    object path = GetRegistryValue(rPath, "Path");
                    if (path == null || path.Equals(""))
                    {
                        throw new Exception("Can not find resource file path!");
                    }
                    string logXmlPath = Path.Combine(path.ToString(), LanguageResource + ".xml");
                    Xmldoc.Load(logXmlPath);
                }
            }
        }

        //public static string LanguageResource = "EN";
        public static XmlDocument Xmldoc = null;

        public static bool IfWriteTxtLog = true;

        public static bool IfWriteEvtLog = true;

        //加入先导语言判断，解决限制全局只能配置一次多语言的瓶颈
        private readonly Dictionary<string, XmlDocument> _initResourceDic
            = new Dictionary<string, XmlDocument>();

        static AccessResource()
        {
            try
            {

                if (InstanceResource == null)
                {
                    lock (Object)
                        InstanceResource = new AccessResource();
                }

                //事件源
                string source = string.Empty;
                //日志XML配置文件路径
                string logXmlPath = string.Empty;

                //从注册表中读取LOG日志配置文件路径与系统日志的源名称
                string rPath = GlobalFinalProperties.GlobalRegeditPath;
                //   + Assembly.GetExecutingAssembly().GetName().Name;

                object path = GetRegistryValue(rPath, "Path");

                //加载日志XML配置文件
                if (path == null || path.Equals(""))
                {
                    throw new Exception("Can not find resource file path!");
                }
                logXmlPath = Path.Combine(path.ToString(), LanguageResource + ".xml");


                //switch (LanguageResource)
                //{
                //    case "CN": filename = "CN_Resource.xml"; break;
                //    case "EN": filename = "EN_Resource.xml"; break;
                //    default: filename = "CN_Resource.xml"; break;
                //}

                Xmldoc = new XmlDocument();
                Xmldoc.Load(logXmlPath);
            }
            catch (Exception)
            {
                throw;
                //MessageBox.Show("Load Language Package exception:" + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 取得本类的唯一实例
        /// </summary>
        public static AccessResource GetInstance()
        {
            if (InstanceResource == null)
            {
                lock (Object)
                    InstanceResource = new AccessResource();
            }

            return InstanceResource;
        }

        /// <summary>
        /// 检索注册表值
        /// </summary>
        /// <param name="rPath">注册表目录路径</param>
        /// <param name="keyName">注册表键</param>
        /// <returns>注册表值</returns>
        public static object GetRegistryValue(string rPath, string keyName)
        {
            //todo 检索配置文件
            /* Microsoft.Win32.RegistryKey key =
                         Microsoft.Win32.Registry.LocalMachine.OpenSubKey(rPath);

             if (key != null)
                 return key.GetValue(keyName);
             return null;*/

            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 初始化先导语言集
        /// </summary>
        /// <param name="key"></param>
        public void InitResourceDic(string key, string language)
        {
            if (!this._initResourceDic.ContainsKey(key))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(Path.Combine(GetRegistryValue(GlobalFinalProperties.GlobalRegeditPath, "Path").ToString(), language + ".xml"));
                    this._initResourceDic.Add(key, xmldoc);
                }
                catch (Exception)
                {
                    throw;
                    //MessageBox.Show("Load resourse file exception:" + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public string GetValue(string keyName)
        {
            string resultValue = "";
            //判断先导语言
            XmlDocument xmlDocument = null;
            //GetEntryAssembly()在网页中嵌入ocx无法取到入口可执行程序集
            string key = Assembly.GetCallingAssembly().GetName().Name;

            if (this._initResourceDic != null && this._initResourceDic.ContainsKey(key))
            {
                xmlDocument = this._initResourceDic[key];
            }
            else
            {
                xmlDocument = Xmldoc;
            }

            if (xmlDocument != null && xmlDocument.DocumentElement != null)
            {
                XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("//Resource//Common//data[@name=\"" + keyName + "\"]");
                if (xmlNode != null)
                    resultValue = xmlNode.InnerText;
            }

            return resultValue;
        }
        public string GetScanValue(string keyName)
        {
            string resultValue = "";
            //判断先导语言
            XmlDocument xmlDocument = null;
            //GetEntryAssembly()在网页中嵌入ocx无法取到入口可执行程序集
            string key = Assembly.GetCallingAssembly().GetName().Name;

            if (this._initResourceDic != null && this._initResourceDic.ContainsKey(key))
            {
                xmlDocument = this._initResourceDic[key];
            }
            else
            {
                xmlDocument = Xmldoc;
            }

            if (xmlDocument != null && xmlDocument.DocumentElement != null)
            {               
                if (xmlDocument.DocumentElement.SelectSingleNode("//Resource//ScanTemplate//" + keyName).Attributes["text"] != null)
                    resultValue = xmlDocument.DocumentElement.SelectSingleNode("//Resource//ScanTemplate//" + keyName).Attributes["text"].Value;
            }

            return resultValue;
        }

        public void StarProcess(string programName, params string[] arguments)
        {
            string argumentsS = "";
            if (arguments != null)
            {
                foreach(string arg in arguments)
                {
                    argumentsS = argumentsS + " "+ arg;
                }
                if(argumentsS.Length > 1)
                    argumentsS = argumentsS.Substring(1);
            }
            // 创建进程启动信息
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = programName,
                // 可以设置其他启动参数，例如命令行参数等
                // ...不能重定向信息，如果重定向，会造成主web面调用此方法的线程卡死
                //RedirectStandardOutput = true
                CreateNoWindow = false
            };

            if(!string.IsNullOrEmpty(argumentsS))
            {
                startInfo.Arguments = argumentsS;
            }
            try
            {
                // 创建并启动新进程
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    Console.WriteLine($"已启动 {programName}");

                    /*StreamReader Outputresults = process.StandardOutput;
                    while (!Outputresults.EndOfStream)
                    {
                        Outputresults.ReadLine();
                        Console.WriteLine(Outputresults.ReadLine());
                    }*/
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"启动进程时出现错误：{ex.Message}");
                Console.WriteLine($"启动进程时出现错误：{ex.Message}");
            }
        }

        public string GetNodeXmlStr(string nodepath)
        {
            string result = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
            XmlNode xmlnode = Xmldoc.SelectSingleNode(nodepath);
            result += xmlnode.OuterXml;
            return result;
        }
        #region 注释
        //public static string GetValue(string KeyName)
        //{
        //    string ResultValue = "";
        //    ResourceManager RM;
        //    try
        //    {
        //        RM = new ResourceManager("GlobalResource.Greeting", Assembly.GetExecutingAssembly());
        //        ResultValue = RM.GetString(KeyName);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (KeyName == "Test")
        //            return "TestError";
        //    }
        //    return ResultValue;
        //}
        #endregion
    }
}