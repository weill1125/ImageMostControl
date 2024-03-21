using System;
using System.Collections.Generic;
using System.Xml;
using Com.Boc.Icms.DoNetSDK.Bean;

namespace Com.Boc.Icms.DoNetSDK.Service
{
    class BaseManager
    {
        public static ApplyAddrMessageReturn SecurityAndAddressHandler(MessageBean message, int ifWriteTxtLog, WriteLog writeStrToLog,string pid)
        {
            ApplyAddrMessageReturn appAddR = new ApplyAddrMessageReturn();
            message.ProtocolType = 2; //应用协议类型
            message.RequestType = "0011"; //请求类型编码-安全验证及申请地址

            string logstr = "Begin applying address";
            if (writeStrToLog != null)
                writeStrToLog("7681 " + logstr,pid);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "info", logstr,pid);

            string userData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><req><socketIp></socketIp>" +
                "<socketPort></socketPort><ftpIp></ftpIp><ftpPort></ftpPort><ftpPath>" +
                "</ftpPath><originaltext>" + message.OriginalNo + "</originaltext><ciphertext>" +
                message.VerifyNo + "</ciphertext></req>";      

            string result = CommonFunc.SendMessage(message, userData, ifWriteTxtLog, writeStrToLog,pid);
            if (result.Length >= 4 && result.Substring(0, 4) == "0000")
            {
                int pos = result.IndexOf('<');
                string strReturn = result.Substring(pos, result.Length - pos);
                strReturn = strReturn.Replace("\r\n", "");

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(strReturn);

                string socketIp = "";
                int socketPort = 0;
                string ftpIp = "";
                int ftpPort = 0;
                string ftpPath = "";
                string bsocketIp = "";
                int bsocketPort = 0;
                string bftpIp = "";
                int bftpPort = 0;
                string bftpPath = "";

                if (xmldoc.SelectSingleNode("//socketIp") != null)
                {
                    socketIp = xmldoc.SelectSingleNode("//socketIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//socketPort") != null)
                {
                    socketPort = int.Parse(xmldoc.SelectSingleNode("//socketPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//ftpIp") != null)
                {
                    ftpIp = xmldoc.SelectSingleNode("//ftpIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//ftpPort") != null)
                {
                    ftpPort = int.Parse(xmldoc.SelectSingleNode("//ftpPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//ftpPath") != null)
                {
                    ftpPath = xmldoc.SelectSingleNode("//ftpPath").InnerText;
                }

                if (xmldoc.SelectSingleNode("//bsocketIp") != null)
                {
                    bsocketIp = xmldoc.SelectSingleNode("//bsocketIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//bsocketPort") != null)
                {
                    bsocketPort = int.Parse(xmldoc.SelectSingleNode("//bsocketPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//bftpIp") != null)
                {
                    bftpIp = xmldoc.SelectSingleNode("//bftpIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//bftpPort") != null)
                {
                    bftpPort = int.Parse(xmldoc.SelectSingleNode("//bftpPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//bftpPath") != null)
                {
                    bftpPath = xmldoc.SelectSingleNode("//bftpPath").InnerText;
                }

                if (socketIp != "" && socketPort != 0 && ftpIp != "" && ftpPort != 0 || bsocketIp != "" && bsocketPort != 0 && bftpIp != "" && bftpPort != 0)
                {
                    appAddR.FtpIp = ftpIp;
                    appAddR.FtpPath = ftpPath;
                    appAddR.FtpPort = ftpPort;
                    appAddR.SocketIp = socketIp;
                    appAddR.SocketPort = socketPort;
                    appAddR.ErrorMessage = "";

                    appAddR.BFtpIp = bftpIp;
                    appAddR.BFtpPath = bftpPath;
                    appAddR.BFtpPort = bftpPort;
                    appAddR.BSocketIp = bsocketIp;
                    appAddR.BSocketPort = bsocketPort;

                    logstr = "Apply for address succees. ftpIp=" + appAddR.FtpIp + " ftpPath=" + appAddR.FtpPath + " ftpPort=" + appAddR.FtpPort +
                            " socketIp=" + appAddR.SocketIp + "soketPort=" + appAddR.SocketPort;
                    if (writeStrToLog != null)
                        writeStrToLog("7682 " + logstr,pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "0000", logstr,pid);
                }
                else
                {
                    logstr = "Apply for address failed. ftpIp=" + appAddR.FtpIp + " ftpPath=" + appAddR.FtpPath + " ftpPort=" + appAddR.FtpPort +
                            " socketIp=" + appAddR.SocketIp + "soketPort=" + appAddR.SocketPort;
                    if (writeStrToLog != null)
                        writeStrToLog("7683 " + logstr,pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
                    appAddR.ErrorMessage = "7786|Apply for address faile,the ip or port of socket or ftp is empty or zero"; //todo
                }
            }
            else
            {
                string errorno = "";
                if (result.Length >= 4)
                {
                    errorno = result.Substring(0, 4);
                    logstr = errorno + "|" + result.Substring(4,result.Length-4);
                    //logstr = "6007|Apply for address faile. And the error number is:" + errorno;
                }
                else
                {
                    logstr = "7787|Apply for address faile.";
                }

                if (writeStrToLog != null)
                {
                    writeStrToLog("7684 " + logstr,pid);
                }

                if (ifWriteTxtLog == 1)
                {
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
                }

                appAddR.ErrorMessage = logstr;
            }

            //message.SoketIp = appAddR.SocketIp;
            //message.SoketPort = Convert.ToString(appAddR.SocketPort);

            if (string.IsNullOrEmpty(appAddR.SocketIp)||string.IsNullOrEmpty(appAddR.FtpIp))
            {
                appAddR.FtpIp = appAddR.BFtpIp;
                appAddR.FtpPath = appAddR.BFtpPath;
                appAddR.FtpPort = appAddR.BFtpPort;
                appAddR.SocketIp = appAddR.BSocketIp;
                appAddR.SocketPort = appAddR.BSocketPort;                

                appAddR.BFtpIp = "";
                appAddR.BFtpPath = "";
                appAddR.BFtpPort = 0;
                appAddR.BSocketIp = "";
                appAddR.BSocketPort = 0;
            }
            message.RecvSoketIp = appAddR.SocketIp;
            message.RecvSoketPort = Convert.ToString(appAddR.SocketPort);

            return appAddR;
        }

        public static ApplyAddrMessageReturn SecurityAndAddressHandlerBroker(MessageBean message, int ifWriteTxtLog, WriteLog writeStrToLog,string pid)
        {
            ApplyAddrMessageReturn appAddR = new ApplyAddrMessageReturn();
            message.ProtocolType = 2; //应用协议类型
            message.RequestType = "0020"; //请求类型编码-安全验证及申请地址
            string result = "";
            string strReturn = "";
            string logstr = "";
            List<string> address = new List<string>();

            logstr = "Begin applying address";
            if (writeStrToLog != null)
                writeStrToLog("7681 " + logstr,pid);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "info", logstr,pid);
            string userData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><req><socketIp></socketIp>" +
                "<socketPort></socketPort><ftpIp></ftpIp><ftpPort></ftpPort><ftpPath>" +
                "</ftpPath><msProvince>" + message.MsProvince + "</msProvince><dataProvince>" + message.DataProvince +
                "</dataProvince><originaltext>" + message.OriginalNo + "</originaltext><ciphertext>" +
                message.VerifyNo + "</ciphertext></req>";            

            result = CommonFunc.SendMessage(message, userData, ifWriteTxtLog, writeStrToLog,pid);
            if (result.Length >= 4 && result.Substring(0, 4) == "0000")
            {
                int pos = result.IndexOf('<');
                strReturn = result.Substring(pos, result.Length - pos);
                strReturn = strReturn.Replace("\r\n", "");

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(strReturn);

                string socketIp = "";
                int socketPort = 0;
                string ftpIp = "";
                int ftpPort = 0;
                string ftpPath = "";
                string bsocketIp = "";
                int bsocketPort = 0;
                string bftpIp = "";
                int bftpPort = 0;
                string bftpPath = "";

                if (xmldoc.SelectSingleNode("//socketIp") != null)
                {
                    socketIp = xmldoc.SelectSingleNode("//socketIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//socketPort") != null)
                {
                    socketPort = int.Parse(xmldoc.SelectSingleNode("//socketPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//ftpIp") != null)
                {
                    ftpIp = xmldoc.SelectSingleNode("//ftpIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//ftpPort") != null)
                {
                    ftpPort = int.Parse(xmldoc.SelectSingleNode("//ftpPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//ftpPath") != null)
                {
                    ftpPath = xmldoc.SelectSingleNode("//ftpPath").InnerText;
                }

                if (xmldoc.SelectSingleNode("//bsocketIp") != null)
                {
                    bsocketIp = xmldoc.SelectSingleNode("//bsocketIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//bsocketPort") != null)
                {
                    bsocketPort = int.Parse(xmldoc.SelectSingleNode("//bsocketPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//bftpIp") != null)
                {
                    bftpIp = xmldoc.SelectSingleNode("//bftpIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//bftpPort") != null)
                {
                    bftpPort = int.Parse(xmldoc.SelectSingleNode("//bftpPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//bftpPath") != null)
                {
                    bftpPath = xmldoc.SelectSingleNode("//bftpPath").InnerText;
                }

                if ((socketIp != "" && socketPort != 0 && ftpIp != "" && ftpPort != 0 )||( bsocketIp != "" && bsocketPort != 0 && bftpIp != "" && bftpPort != 0))
                {
                    appAddR.FtpIp = ftpIp;
                    appAddR.FtpPath = ftpPath;
                    appAddR.FtpPort = ftpPort;
                    appAddR.SocketIp = socketIp;
                    appAddR.SocketPort = socketPort;
                    appAddR.ErrorMessage = "";

                    appAddR.BFtpIp = bftpIp;
                    appAddR.BFtpPath = bftpPath;
                    appAddR.BFtpPort = bftpPort;
                    appAddR.BSocketIp = bsocketIp;
                    appAddR.BSocketPort = bsocketPort;


                    logstr = "Apply for address succees. ftpIp=" + appAddR.FtpIp + " ftpPath=" + appAddR.FtpPath + " ftpPort=" + appAddR.FtpPort +
                            " socketIp=" + appAddR.SocketIp + "soketPort=" + appAddR.SocketPort;
                    if (writeStrToLog != null)
                        writeStrToLog("7682 " + logstr,pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "0000", logstr,pid);
                }
                else
                {
                    logstr = "Apply for address failed. ftpIp=" + appAddR.FtpIp + " ftpPath=" + appAddR.FtpPath + " ftpPort=" + appAddR.FtpPort +
                            " socketIp=" + appAddR.SocketIp + "soketPort=" + appAddR.SocketPort+ "bftpIp=" + appAddR.BFtpIp + " bftpPath=" + appAddR.BFtpPath + " bftpPort=" + appAddR.BFtpPort +
                            " bsocketIp=" + appAddR.BSocketIp + "bsoketPort=" + appAddR.BSocketPort;
                    if (writeStrToLog != null)
                        writeStrToLog("7683 " + logstr,pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
                    appAddR.ErrorMessage = "7786|Apply for address faile,the ip or port of socket or ftp is empty or zero"; //todo
                }
            }
            else
            {
                string errorno = "";
                if (result.Length >= 4)
                {
                    errorno = result.Substring(0, 4);
                    logstr = errorno + "|" + result.Substring(4, result.Length - 4);
                    //logstr = "6007|Apply for address faile. And the error number is:" + errorno;
                }
                else
                {
                    logstr = "7787|Apply for address faile.";
                }

                if (writeStrToLog != null)
                {
                    writeStrToLog("7684 " + logstr,pid);
                }

                if (ifWriteTxtLog == 1)
                {
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
                }

                appAddR.ErrorMessage = logstr;
            }

            //message.SoketIp = appAddR.SocketIp;
            //message.SoketPort =Convert.ToString(appAddR.SocketPort);

            if (string.IsNullOrEmpty(appAddR.SocketIp) || string.IsNullOrEmpty(appAddR.FtpIp))
            {
                appAddR.FtpIp = appAddR.BFtpIp;
                appAddR.FtpPath = appAddR.BFtpPath;
                appAddR.FtpPort = appAddR.BFtpPort;
                appAddR.SocketIp = appAddR.BSocketIp;
                appAddR.SocketPort = appAddR.BSocketPort;

                appAddR.BFtpIp = "";
                appAddR.BFtpPath = "";
                appAddR.BFtpPort = 0;
                appAddR.BSocketIp = "";
                appAddR.BSocketPort = 0;
            }

            message.RecvSoketIp = appAddR.SocketIp;
            message.RecvSoketPort = Convert.ToString(appAddR.SocketPort);         
            
            return appAddR;
        }

        public static ApplyAddrMessageReturn AddressHandlerBroker(MessageBean message, int ifWriteTxtLog, WriteLog writeStrToLog,string pid)
        {
            ApplyAddrMessageReturn appAddR = new ApplyAddrMessageReturn();
            message.ProtocolType = 2; //应用协议类型
            message.RequestType = "0021"; //请求类型编码-安全验证及申请地址
            string result = "";
            string strReturn = "";
            string logstr = "";
            List<string> address = new List<string>();

            logstr = "Begin applying address";
            if (writeStrToLog != null)
                writeStrToLog("7681 " + logstr,pid);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "info", logstr,pid);

            string userData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><req><socketIp></socketIp>" +
                "<socketPort></socketPort><ftpIp></ftpIp><ftpPort></ftpPort><ftpPath>" + "</ftpPath><msProvince>" +
                message.MsProvince + "</msProvince><dataProvince>" + message.DataProvince + "</dataProvince>";

            result = CommonFunc.SendMessage(message, userData, ifWriteTxtLog, writeStrToLog,pid);
            if (result.Length >= 4 && result.Substring(0, 4) == "0000")
            {
                int pos = result.IndexOf('<');
                strReturn = result.Substring(pos, result.Length - pos);
                strReturn = strReturn.Replace("\r\n", "");

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(strReturn);

                string socketIp = "";
                int socketPort = 0;
                string ftpIp = "";
                int ftpPort = 0;
                string ftpPath = "";

                if (xmldoc.SelectSingleNode("//socketIp") != null)
                {
                    socketIp = xmldoc.SelectSingleNode("//socketIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//socketPort") != null)
                {
                    socketPort = int.Parse(xmldoc.SelectSingleNode("//socketPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//ftpIp") != null)
                {
                    ftpIp = xmldoc.SelectSingleNode("//ftpIp").InnerText;
                }

                if (xmldoc.SelectSingleNode("//ftpPort") != null)
                {
                    ftpPort = int.Parse(xmldoc.SelectSingleNode("//ftpPort").InnerText);
                }

                if (xmldoc.SelectSingleNode("//ftpPath") != null)
                {
                    ftpPath = xmldoc.SelectSingleNode("//ftpPath").InnerText;
                }

                if (socketIp != "" && socketPort != 0 && ftpIp != "" && ftpPort != 0)
                {
                    appAddR.FtpIp = ftpIp;
                    appAddR.FtpPath = ftpPath;
                    appAddR.FtpPort = ftpPort;
                    appAddR.SocketIp = socketIp;
                    appAddR.SocketPort = socketPort;
                    appAddR.ErrorMessage = "";

                    logstr = "Apply for address succees. ftpIp=" + appAddR.FtpIp + " ftpPath=" + appAddR.FtpPath + " ftpPort=" + appAddR.FtpPort +
                            " socketIp=" + appAddR.SocketIp + "soketPort=" + appAddR.SocketPort;
                    if (writeStrToLog != null)
                        writeStrToLog("7682 " + logstr,pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "0000", logstr,pid);
                }
                else
                {
                    logstr = "Apply for address failed. ftpIp=" + appAddR.FtpIp + " ftpPath=" + appAddR.FtpPath + " ftpPort=" + appAddR.FtpPort +
                            " socketIp=" + appAddR.SocketIp + "soketPort=" + appAddR.SocketPort;
                    if (writeStrToLog != null)
                        writeStrToLog("7683 " + logstr,pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
                    appAddR.ErrorMessage = "7786|Apply for address faile,the ip or port of socket or ftp is empty or zero"; //todo
                }
            }
            else
            {
                string errorno = "";
                if (result.Length >= 4)
                {
                    errorno = result.Substring(0, 4);
                    logstr = errorno + "|" + result.Substring(4, result.Length - 4);
                    //logstr = "6007|Apply for address faile. And the error number is:" + errorno;
                }
                else
                {
                    logstr = "7787|Apply for address faile.";
                }

                if (writeStrToLog != null)
                {
                    writeStrToLog("7684 " + logstr,pid);
                }

                if (ifWriteTxtLog == 1)
                {
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
                }

                appAddR.ErrorMessage = logstr;
            }

            //message.SoketIp = appAddR.SocketIp;
            //message.SoketPort = Convert.ToString(appAddR.SocketPort);

            message.RecvSoketIp = appAddR.SocketIp;
            message.RecvSoketPort = Convert.ToString(appAddR.SocketPort);

            return appAddR;
        }


        public static ApplyAddrMessageReturn ApplyTransmittedInformation(MessageBean message, int ifWriteTxtLog, WriteLog writeStrToLog, string pid)
        {
            ApplyAddrMessageReturn appAddR = new ApplyAddrMessageReturn();

            string logstr = "Begin applying address";
            if (writeStrToLog != null)
                writeStrToLog("7681 " + logstr, pid);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "info", logstr, pid);
            WriteLog(logstr);

            message.Transtype = "0000";
            string userData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                              "<request>" +
                                "<req_header>" +
                                    "<sys_code>" + message.Syscode + "</sys_code>" +
                                    "<bank_code>" + message.Bankcode + "</bank_code>" +
                                    "<branch_code>" + message.Branchcode + "</branch_code>" +
                                    "<operater_id>" + message.Operaterid + "</operater_id>" +
                                    "<trans_type>" + message.Transtype + "</trans_type>" +
                                    "<version>" + message.Version + "</version>" +
                                    "<trans_time>" + message.Transtime + "</trans_time>" +
                                    "<trans_id>" + message.Transid + "</trans_id>" +
                                    "<client_type>" + message.Clienttype + "</client_type>" +
                                    "<client_ip>" + message.ClientIp + "</client_ip>" +
                                 "</req_header>" +
                                 "<req_body>" +
                                    "<image_storage_mech>" + message.Imagestoragemech + "</image_storage_mech>" +
                                    "<original_text>" + message.Originaltext + "</original_text>" +
                                    "<cipher_text>" + message.Ciphertext + "</cipher_text>" +
                                 "</req_body>" +
                              "</request>";

            WriteLog("报文信息：" + userData);
            string result = CommonFunc.SendMessage(message, userData, ifWriteTxtLog, writeStrToLog, pid);

            WriteLog("申请传输信息结果：" + result);

            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.LoadXml(result);

                string transtype = "";
                string version = "";
                string transid = "";
                string trantime = "";
                string retcode = "";
                string retmessage = "";
                string messageaddress = "";
                string messageport = "";
                string fileaddress = "";
                int fileport = 0;
                string filepath = "";


                if (xmldoc.SelectSingleNode("//trans_type") != null)
                {
                    transtype = xmldoc.SelectSingleNode("//trans_type").InnerText;
                }

                if (xmldoc.SelectSingleNode("//version") != null)
                {
                    version = xmldoc.SelectSingleNode("//version").InnerText;
                }

                if (xmldoc.SelectSingleNode("//trans_id") != null)
                {
                    transtype = xmldoc.SelectSingleNode("//trans_id").InnerText;
                }

                if (xmldoc.SelectSingleNode("//tran_time") != null)
                {
                    trantime = xmldoc.SelectSingleNode("//tran_time").InnerText;
                }

                if (xmldoc.SelectSingleNode("//ret_code") != null)
                {
                    retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                }

                if (xmldoc.SelectSingleNode("//ret_message") != null)
                {
                    retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                }

                if (xmldoc.SelectSingleNode("//message_address") != null)
                {
                    messageaddress = xmldoc.SelectSingleNode("//message_address").InnerText;
                }

                if (xmldoc.SelectSingleNode("//message_port") != null)
                {
                    messageport = xmldoc.SelectSingleNode("//message_port").InnerText;
                }

                if (xmldoc.SelectSingleNode("//file_address") != null)
                {
                    fileaddress = xmldoc.SelectSingleNode("//file_address").InnerText;
                }

                if (xmldoc.SelectSingleNode("//file_port") != null)
                {
                    fileport = Int32.Parse(xmldoc.SelectSingleNode("//file_port").InnerText);
                }
                if (xmldoc.SelectSingleNode("//file_path") != null)
                {
                    filepath = xmldoc.SelectSingleNode("//file_path").InnerText;
                }


                appAddR.Transtype = transtype;
                appAddR.Version = version;
                appAddR.Transid = transid;
                appAddR.Trantime = trantime;
                appAddR.Retcode = retcode;
                appAddR.Retmessage = retmessage;
                appAddR.Messageaddress = messageaddress;
                appAddR.Messageport = messageport;
                appAddR.Fileaddress = fileaddress;
                appAddR.Fileport = fileport;
                appAddR.Filepath = filepath;
                appAddR.ErrorMessage = "";

                if (appAddR.Retcode == "00000")
                {
                    logstr = "Apply for address succees. MessageAddress=" + appAddR.Messageaddress + " MessagePort=" + appAddR.Messageport + " FileAddress=" + appAddR.Fileaddress +
                           " FilePort=" + appAddR.Fileport + "FilePath=" + appAddR.Filepath;
                    if (writeStrToLog != null)
                        writeStrToLog("7682 " + logstr, pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "0000", logstr, pid);
                    WriteLog(logstr);
                }
                else
                {
                    appAddR.ErrorMessage = "7786|Apply for address faile,the ip or port of socket or ftp is empty or zero"; //todo
                    logstr = "Apply for address failed. ftpIp=" + appAddR.Messageaddress + " MessagePort=" + appAddR.Messageport + " FileAddress=" + appAddR.Fileaddress +
                                " FilePort=" + appAddR.Fileport + "FilePath=" + appAddR.Filepath;
                    if (writeStrToLog != null)
                        writeStrToLog("7683 " + logstr, pid);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("BaseManager", CommonFunc.GetLineNum().ToString(), "error", logstr, pid);
                    WriteLog(appAddR.ErrorMessage);
                }
            }
            catch (Exception ex)
            {

            }

            return appAddR;
        }
        public static string CommServer(MessageBean message, string userReq, int ifWriteTxtLog, WriteLog writeStrToLog,string pid)
        {
            string result = "";
            int tokPos = -1;
            string logstr = "";

            result = CommonFunc.SendMessage(message, userReq, ifWriteTxtLog, writeStrToLog,pid);

            //截取返回的信息
            //if (result.Length > 0)
            //{
            //    tokPos = result.IndexOf('|');

            //    if (tokPos >= 0)
            //    {
            //        if (result.IndexOf("<?xml") >= 0)
            //            result = "0000|" + result.Substring(tokPos + 1, result.Length - tokPos - 1);
            //        else
            //            result = result.Substring(0, 4) + '|' + result.Substring(4, result.Length - 4);
            //    }
            //    else
            //        result = result.Substring(0, 4) + '|' + result.Substring(4, result.Length - 4);
            //}
            //else
            //{
            //    logstr = "7788|Recerive message by socket failed!";
            //    if (writeStrToLog != null)
            //        writeStrToLog("7685 " + CommonFunc.GetLineNum() + " " + logstr,pid);
            //    if (ifWriteTxtLog == 1)
            //        CommonFunc.WriteTxtLog("BaseManager",  CommonFunc.GetLineNum().ToString(), "error", logstr,pid);
            //    result = logstr;
            //}

            return result;
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
