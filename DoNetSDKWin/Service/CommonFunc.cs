using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml;
using Com.Boc.Icms.DoNetSDK.Bean;
using System.Runtime.InteropServices;
using Com.Boc.Icms.DoNetSDK.Log;
using Microsoft.SqlServer.Server;

namespace Com.Boc.Icms.DoNetSDK.Service
{
    public class CommonFunc
    {     
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userReq"></param>
        /// <returns></returns>
        internal static string SendMessage(MessageBean message, string data, int ifWriteTxtLog, WriteLog writeStrToLog, string pid)
        {
            System.Threading.ManualResetEvent TimeOutOnject = new System.Threading.ManualResetEvent(false);
            Exception socketexception = null;

            string result = "";
            byte[] buffer = new byte[20480]; //接收socket返回消息
            string spaceStr = "         ";
            string logstr = "";

            int length = Encoding.UTF8.GetBytes(data).Length;           
            string str = data.Length.ToString("D5");
            byte[] buf =Encoding.UTF8.GetBytes(str + data) ;

            //初始化socket,进行传输
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           
            #region 添加连接超时

            TimeOutOnject.Reset();

            string host;
            int port;
            //IPEndPoint ep;
            WriteLog("添加连接超时");
            if (string.IsNullOrEmpty(message.RecvSoketIp))
            {
                //ep = new IPEndPoint(IPAddress.Parse(message.SoketIp), int.Parse(message.SoketPort));
                host = message.SoketIp;
                port = int.Parse(message.SoketPort);
            }
            else
            {
                //ep = new IPEndPoint(IPAddress.Parse(message.RecvSoketIp), int.Parse(message.RecvSoketPort));
                host = message.RecvSoketIp;
                port = int.Parse(message.RecvSoketPort);
            }
            WriteLog("host："+ host+"\n"+"port:"+port);
            socket.BeginConnect(host,port,
                 asyresult =>
                 {
                     try
                     {
                         socket.EndConnect(asyresult);
                     }
                     catch (Exception ex)
                     {
                         socketexception = ex;
                     }
                     finally
                     {
                         TimeOutOnject.Set();
                     }
                 }, socket);

            int ConnectTimeout = 3000;

            if (int.Parse(GetXmlNodeValue("//ConnectTimeout")) > 0)
            {
                ConnectTimeout = int.Parse(GetXmlNodeValue("//ConnectTimeout"));
            }

            if (TimeOutOnject.WaitOne(ConnectTimeout, false))
            {

                if (socket.Connected == false)
                {
                    logstr = "Connect between Transfer control and servers failed！";
                    if (writeStrToLog != null)
                        writeStrToLog("7701 " + logstr, pid);
                    if (ifWriteTxtLog == 1)
                        WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                    WriteLog("CommonFunc："+ GetLineNum().ToString());
                    result = "7780 Connect socket failed,exception message:" + socketexception.Message + "host:" + host + "port:" + port.ToString();
                    WriteLog(result);
                    return result;
                }
            }
            else
            {
                try
                {
                    socket.Close();
                    WriteLog("socket关闭");
                }
                catch (Exception)
                {
                }

                logstr = "Connect between Transfer control and servers failed！";
                if (writeStrToLog != null)
                    writeStrToLog("7701 " + logstr, pid);
                if (ifWriteTxtLog == 1)
                    WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                result = "7780 Connect socket time out!" + " host:"+host+" port:"+port.ToString();
                WriteLog(result);
                return result;
            }

            #endregion
            try
            {
                #region 启用keepAlive定时探测
                
                uint dummy = 0;
                int keepAliveTime =30000;

                if (int.Parse(GetXmlNodeValue("//keepAliveInterval")) > 0)
                {
                    keepAliveTime = int.Parse(GetXmlNodeValue("//keepAliveInterval"));
                } 
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];

                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

                #endregion

                try
                {
                    socket.Send(buf, buf.Length, 0);
                    WriteLog("报文已发送");

                }
                catch (Exception ex)
                {
                    logstr = "Transfer sending message to servers failed！";
                    if (writeStrToLog != null)
                        writeStrToLog("7702 " + logstr, pid);
                    if (ifWriteTxtLog == 1)
                        WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                    result = "7781Send message failed,exception message:" + ex.Message;
                    return result;
                }

                logstr = "Send message：" + data;
                if (writeStrToLog != null)
                    writeStrToLog("7703 " + logstr, pid);
                if (ifWriteTxtLog == 1)
                    WriteTxtLog("CommonFunc", GetLineNum().ToString(), "info", logstr, pid);

                int iBytes = 0;

                if (int.Parse(GetXmlNodeValue("//RcvMsgTimeout")) > 0)
                {
                    socket.ReceiveTimeout = int.Parse(GetXmlNodeValue("//RcvMsgTimeout"));
                }
                else
                {
                    socket.ReceiveTimeout = 60000;
                }
                int receriveLength = 0;
                byte[] lenBytes = new byte[4];
                int bytesCount = 0;

                int reciveCount = 0;
                //先取长度，防止转换成string再截取四位时位数截取错误
                //取应接收字符串的长度
                reciveCount=socket.Receive(lenBytes, 4, 0);
                if (reciveCount==0)
                {
                    logstr = "Transfer receiving message from servers failed！";
                    if (writeStrToLog != null)
                        writeStrToLog("7704 " + logstr, pid);
                    if (ifWriteTxtLog == 1)
                        WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                    result = "7782Recerive message failed,the number of bytes received is 0!";
                    return result;
                }


                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lenBytes);
                //lenBytes中长度为Data Field长度，还得增加RETURN_CODE长度
                receriveLength = BitConverter.ToInt32(lenBytes, 0) + 4;


                while (socket.Connected)
                {
                    try
                    {
                        //从socket接收指定字节数的数据，并将数据存入接收缓冲区
                        iBytes = socket.Receive(buffer, buffer.Length, 0);
                        bytesCount += iBytes;

                        //将字节数组中的一个字节序列解码为字符串
                        result += Encoding.UTF8.GetString(buffer, 0, iBytes);
                        WriteLog("返回结果：" + result) ;
                        //接收到指定长度时结束接收
                        if (bytesCount >= receriveLength)
                        {
                            break;
                        }

                        if (iBytes==0)
                        {
                            logstr = "Transfer receiving message from servers failed！";
                            if (writeStrToLog != null)
                                writeStrToLog("7704 " + logstr, pid);
                            if (ifWriteTxtLog == 1)
                                WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                            result = "7782Recerive message failed,the number of bytes received is 0!";
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        logstr = "Transfer receiving message from servers failed！";
                        if (writeStrToLog != null)
                            writeStrToLog("7704 " + logstr, pid);
                        if (ifWriteTxtLog == 1)
                            WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                        result = "7782Recerive message failed,exception message:" + ex.Message;
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                logstr = "Exception occured when procedure running：：" + ex.Message;
                if (writeStrToLog != null)
                    writeStrToLog("7705 " + logstr, pid);
                if (ifWriteTxtLog == 1)
                    WriteTxtLog("CommonFunc", GetLineNum().ToString(), "error", logstr, pid);

                return result;
            }
            finally
            {
                //关闭socket
                socket.Disconnect(false);

                //连接负载均衡，不调用Close ，socket会不停的发送探测报文
                socket.Close();
            }

            logstr = "Receive message: " + result;
            if (writeStrToLog != null)
                writeStrToLog("7706 " + logstr, pid);
            if (ifWriteTxtLog == 1)
                WriteTxtLog("CommonFunc", GetLineNum().ToString(), "info", logstr, pid);

            return result;
        }
        
        /// <summary>
        /// 获取本地IP
        /// </summary>
        /// <returns>IP字符串</returns>
        public static string LocalIp()
        {
            string name = Dns.GetHostName();
            IPHostEntry entry = Dns.GetHostEntry(name);
            IPAddress[] ipArray = entry.AddressList;
            foreach (var ip in ipArray)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return ip.ToString();
                }
            }

            return string.Empty;
        }

        internal static void WriteTxtLog(string className,string lineNo,string errorNo,string message,string pid)
        {
            string logName = "DonetSdk_" + DateTime.Now.ToString("yyyyMMdd")+"_"+pid + ".log";
            string strSpace = "   ";
            string newLine = "\r\n";
            string strLog = pid + strSpace + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + strSpace + className + ":" + lineNo + strSpace + errorNo + strSpace + message + newLine;
            //string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "\\SdkLog\\";
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            //File.AppendAllText(path + logName, strLog, Encoding.UTF8);

            FastLogger.Info(strLog, FastLogType.SdkLog);
        }

        internal static void DeleteHisLog(string dirPath,string whoselog,string regexStr)
        {
            if (Directory.Exists(dirPath) == false)
                return;
            int pos = whoselog.IndexOf('*');
            DateTime startDateTime = DateTime.Now.AddDays(-10);
            int deathDate = int.Parse(startDateTime.ToString("yyyyMMdd"));            
            string[] filenames = Directory.GetFiles(dirPath, whoselog);
            foreach (string filename in filenames)
            {
                FileInfo file = new FileInfo(filename);
                if (System.Text.RegularExpressions.Regex.IsMatch(file.Name, regexStr))
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

        internal static string GetXmlNodeValue(string nodename)
        {
            string result = "";
            XmlDocument xmldoc = new XmlDocument();
            string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            xmldoc.Load(Path.Combine( path , "SDK_Paras.xml"));
            XmlNodeList xmlnodelist = xmldoc.SelectNodes(nodename);
            if (xmlnodelist != null && xmlnodelist.Count > 0)
            {
                if (xmlnodelist[0].InnerText != null)
                    result = xmlnodelist[0].InnerText;
            }

            if (result == "") 
                result = "-1";
            return result;
        }

        /// <summary>
        /// 系统号补空格，4位后补
        /// </summary>
        /// <param name="systemId">系统号</param>
        /// <returns></returns>
        private static string FillEmpty(string systemId)
        {
            if (systemId.Length < 4)
            {
                string spacestr = "    ";
                systemId = systemId + spacestr.Substring(0, 4 - systemId.Length);
            }

            return systemId;
        }

        /// <summary>
        /// 获取行号
        /// </summary>
        /// <returns></returns>
        internal static int GetLineNum()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
            return st.GetFrame(0).GetFileLineNumber();
        }

        internal static string GetErrorMsg(string errorNo)
        {
            string result = "Get ftp error message fail";
            result = Properties.Resource.ResourceManager.GetString("s"+errorNo);
            return result;
        }

        /// <summary>
        /// 清除文件夹下的文件
        /// </summary>
        /// <param name="dirpath">要清除的文件夹路径</param>
        internal static void DeleteDirectoryInfo(string dirpath)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(dirpath);

            //删除子目录下的子目录及文件
            DirectoryInfo[] dirs = dirinfo.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                dir.Attributes = FileAttributes.Archive;
                FileInfo[] curfiles = dir.GetFiles();
                foreach (FileInfo fileinfo in curfiles)
                {
                    fileinfo.Attributes = FileAttributes.Archive;
                    fileinfo.Delete();
                }

                DirectoryInfo[] subdirs = dir.GetDirectories();
                if (subdirs.Length > 0)
                    DeleteDirectoryInfo(dir.FullName);

                dir.Delete(true);
            }

            //删除目录下的文件
            FileInfo[] files = dirinfo.GetFiles();
            foreach (FileInfo fileinfo in files)
            {
                fileinfo.Attributes = FileAttributes.Archive;
                fileinfo.Delete();
            }
        }

        /// <summary>
        /// 准备加解密临时存放的文件夹，并清除此目录中之前存放的文件
        /// </summary>
        /// <returns>加解密临时存放的文件路径</returns>
        internal static string PrepareTempFile()
        {
            //加密后文件临时存放路径
            string tempFilePath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "\\temp\\";
            if (!Directory.Exists(tempFilePath))
                Directory.CreateDirectory(tempFilePath);
            else
            {
                DeleteDirectoryInfo(tempFilePath);
            }
            //在upload中创建子目录，防止删除失败导致无法上传
            tempFilePath += Guid.NewGuid().ToString().Replace("-", "") + "\\";
            Directory.CreateDirectory(tempFilePath);
            return tempFilePath;
        }

        /// <summary>
        /// 加密文件夹中的所有文件，并放入指定文件夹中
        /// </summary>
        /// <param name="filePath">要加密的文件夹</param>
        /// <param name="tempFilePath">加密后放入的文件夹</param>
        internal static void EncryptDirectoryFile(string filePath, string tempFilePath, string aesKey)
        {
            //检索目录中的所有文件
            string[] files = Directory.GetFiles(filePath, "*", SearchOption.AllDirectories);
            //文件名的位置
            int i = filePath.Length;
            //文件名
            string name = null;
            foreach (string file in files)
            {
                name = file.Substring(i, file.Length - i);
                Encryption.EncryptFileByUnicode(file, tempFilePath + name, aesKey);
            }
        }

        /// <summary>
        /// 解密文件夹中的所有文件，并放入指定文件夹中
        /// </summary>
        /// <param name="filePath">要解密的文件夹</param>
        /// <param name="tempFilePath">解密后放入的文件夹</param>tempFilePath
        internal static void DecryptDirectoryFile(string tempFilePath, string filePath, string aesKey)
        {
            //检索目录中的所有文件
            string[] files = Directory.GetFiles(tempFilePath, "*", SearchOption.AllDirectories);

            //文件解密到filePath文件夹中
            //文件名的位置
            int i = tempFilePath.Length;
            //相对路径文件名
            string name = null;
            foreach (string file in files)
            {
                name = file.Substring(i, file.Length - i);
                Encryption.DecryptFileByUnicode(file, filePath + name, aesKey);
            }
        }

        public static void WriteLog(string message)
        {
            StreamWriter streamWriter = null;
            try
            {
                string LogPathDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "File");
                if (!Directory.Exists(LogPathDir))
                {
                    Directory.CreateDirectory(LogPathDir);
                }
                streamWriter = new StreamWriter(Path.Combine(LogPathDir, "SocketLog.txt"), true);
                streamWriter.WriteLine(DateTime.Now.ToString() + ":" + message);

                streamWriter.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
