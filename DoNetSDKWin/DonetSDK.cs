using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Com.Boc.Icms.DoNetSDK.Bean;
using Com.Boc.Icms.DoNetSDK.Service;
using System.Threading;
using System.Linq;
namespace Com.Boc.Icms.DoNetSDK
{
    using Com.Boc.Icms.DoNetSDK.Log;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Xml;

    public delegate void WriteLog(string logstr, string proIdAndThreadId);
    public delegate void TransferOnePage(string downLoadFileName, string filePath);
    public delegate void BeforeChaneName(ref string fileName, ref string changeToName);
    public delegate void AsynchronousTransferOperate(PageInfo downLoadFileInfo);
    public delegate void BeforeTransfer(int fileCount);

    public delegate ReturnMsg DownLoadPage(string contentList, string savePath);

    [ClassInterface(ClassInterfaceType.AutoDual)]
    public partial class DonetSdk :IDonetSdk
    {
        public event WriteLog WriteStrToLog;

        public event DownLoadPage DownLoadPageList = null;

        public event TransferOnePage BeforeDownLoadOnePage
        {
            add
            {
                if (this._protrans != null) this._protrans.BeforeDownLoadOnePage += value;
            }
            remove
            {
                if (this._protrans != null) this._protrans.BeforeDownLoadOnePage -= value;
            }
        }

        public event TransferOnePage DownLoadSuccessOnePage
        {
            add
            {
                if (this._protrans != null) this._protrans.DownLoadSuccessOnePage += value;
            }
            remove
            {
                if (this._protrans != null) this._protrans.DownLoadSuccessOnePage -= value;
            }
        }

        public event TransferOnePage BeforeUploadLoadOnePage
        {
            add
            {
                if (this._protrans != null) this._protrans.BeforeUploadLoadOnePage += value;
            }
            remove
            {
                if (this._protrans != null) this._protrans.BeforeUploadLoadOnePage -= value;
            }
        }

        public event BeforeChaneName BeforeChangeName
        {
            add
            {
                if (this._protrans != null) this._protrans.BeforeChangeName += value;
            }
            remove
            {
                if (this._protrans != null) this._protrans.BeforeChangeName -= value;
            }
        }

        public event BeforeTransfer BeforeTransfer
        {
            add
            {
                if (this._protrans != null) this._protrans.BeforeTransfer += value;
            }
            remove
            {
                if (this._protrans != null) this._protrans.BeforeTransfer -= value;
            }
        }

        public event AsynchronousTransferOperate AsynchronousBeforeDownLoadOnePage;
        public event AsynchronousTransferOperate AsynchronousAfterDownLoadOnePage;
        public event AsynchronousTransferOperate AsynchronousDownLoadOnePageFail;

        public event AsynchronousTransferOperate AsynchronousTransferOnePage = null;
        readonly ProTrans _protrans = null;

        //是否删除文件传输临时目录
        private bool _ifDelDir = true;

        //安全验证原文
        private string _securityNo = "";

        //是否记录日志文件
        private int _ifWriteTxtLog;

        private readonly string _proIdAndThreadId;

        private string noThumbFlag = "2";

        public List<string> RPaths = new List<string>();

        public List<string> HasDelPaths = new List<string>();

        //存储需要弹出进度条的文件信息
        public List<PageInfo> NeedSendPages = new List<PageInfo>();

        //存储异步多线程下载时切换同步时正在下载的文件信息
        public List<PageInfo> NeedDelPages = new List<PageInfo>();

        //存储异步多线程下载时的远程临时路径信息
        public List<string> RemotePath = new List<string>();


        private bool isSynchronous = false;

        public string Version = "N045";

        /// <summary>
        /// 构造函数
        /// </summary>
        public DonetSdk()
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            if (this._protrans == null) this._protrans = new ProTrans();
            this._ifWriteTxtLog = int.Parse(CommonFunc.GetXmlNodeValue("//WriteTxtLog"));
            string sdkpath = Path.Combine( Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) , "SdkLog");
            string ftppath = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) , "ftplog");
            CommonFunc.DeleteHisLog(sdkpath, "DonetSdk_*.log", "^DonetSdk_[0-9]{8}\\w*.log$");
            CommonFunc.DeleteHisLog(ftppath, "ftpc*.log", "^ftpc[0-9]{8}.log$");
            FastLogger.Instance().Register();
        }

        /// <summary>
        /// 设置文件传输进度条显示位置
        /// </summary>
        /// <param name="barParent">父窗体句柄</param>
        /// <param name="pwidth">显示宽度</param>
        /// <param name="pheight">显示高度</param>
        public void SetDisplayControl(IntPtr barParent, int pwidth, int pheight)
        {
            if (this._protrans != null)
            {
                this._protrans.SetProTrans(barParent, pwidth, pheight);
            }
        }

        /// <summary>
        /// 设置是否删除文件传输服务器端的临时目录
        /// </summary>
        /// <param name="delDir">true表示删除,false表示不删除</param>
        public void SetIfDelDir(bool delDir)
        {
            this._ifDelDir = delDir;
        }

        /// <summary>
        /// 通过pkuuid调阅影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="transtype">交易类型</param>
        /// <param name="version">版本号</param>
        /// <param name="transtime">交易日期+时间</param>
        /// <param name="transid">交易流水号，幂等域</param>
        /// <param name="clienttype">客户端类型</param>
        /// <param name="clientip">客户端IP</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownloadByPkuuid(string ip,string port,string syscode, string bankcode, string branchcode, string operaterid, string transtype, string version, string transtime, string transid, string clienttype, string clientip, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string startersyscode,
                                          string pkuuid,string imagelibraryident,bool iscurrentversion,string versionlabel,bool isoriginal,string resolution,bool includecontentfile, string savePath,string zpk="", string filenames="")
        {                                            

            string logstr = "DownloadByPkuuId input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid + ",transtype:" + transtype
                      + ",version:" + version + ",transtime:" + transtime + ",transid:" + transid + ",clienttype:" + clienttype
                      + ",clientip:" + clientip + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext + ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            bool ifGet = false;
            string desDir;
            string result = "";
            string userData;
            int rpPos = -1;
            string rpath = "";
            string serialNo = DateTime.Now.ToString("HHmmss");
            int ftpResult = 0;
            string failedFileName = "";
            int successSign = 0;
            this._ifWriteTxtLog = int.Parse(CommonFunc.GetXmlNodeValue("//WriteTxtLog"));
            List<string> dlfailedFiles = new List<string>();



            logstr = "Begin Apply!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            MessageBean message = new MessageBean();
            message.SoketIp = ip;
            message.SoketPort = port;
            message.Syscode = syscode;
            message.Bankcode = bankcode;
            message.Branchcode = branchcode;
            message.Operaterid = operaterid;
            message.Transtype = transtype;
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = clienttype;
            message.Clientip = clientip;
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;
           

            logstr = "Upload serial number: " + message.ClientIp + "-" + message.MsSystemId + "-" + message.SerialNo;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7746 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);
           
            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                returnMsg.ReturnCode = addr.ErrorMessage.Substring(0, 4);
                returnMsg.Message = "";
                returnMsg.ResultData = addr.ErrorMessage.Substring(5, addr.ErrorMessage.Length - 5);

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            int doWork = 2;
            while (doWork > 0)
            {
                //申请PKUUID下载               
                userData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
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
                                    "<client_ip>" + message.Clientip + "</client_ip>" +
                                    "<starter_sys_code>" + startersyscode + "</starter_sys_code>" +
                                 "</req_header>" +
                                 "<req_body>" +
                                    "<zpk>" + zpk + "</zpk>" +
                                    "<pkuuid>" + pkuuid + "</pkuuid>" +
                                    "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                    "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                    "<version_label>" + versionlabel + "</version_label>" +
                                    "<is_original>" + isoriginal + "</is_original>" +
                                    "<resolution>" + resolution + "</resolution>" +
                                    "<include_content_file>" + includecontentfile + "</include_content_file>" +
                                    "<file_names>" + filenames + "</file_names>" +
                                 "</req_body>" +
                              "</request>";
                result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

                if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

                //if (result.Length >= 4 && result.Substring(0, 4) == "0000")
                if (!string.IsNullOrEmpty(result))
                {
                    logstr = "Get files from content server succees.";
                    if (this.WriteStrToLog != null) this.WriteStrToLog("7625 " + logstr, this._proIdAndThreadId);
                    if (this._ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "0000", logstr, this._proIdAndThreadId);


                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    //取服务器端下载的目录
                   
                    rpath = xmldoc.SelectSingleNode("//file_path").InnerText; ;

                    this._protrans.SetFtpPara(13, transTimeOut, this._ifWriteTxtLog, this.WriteStrToLog);
                    this._protrans.SetFtpPara(1, fileTransOneSize, this._ifWriteTxtLog, this.WriteStrToLog);
                    this._protrans.SetFtpPara(15, this._ifWriteTxtLog, this._ifWriteTxtLog, this.WriteStrToLog);
                    this._protrans.SetXmlParaValue(this._ifWriteTxtLog, this.WriteStrToLog);

                    ifGet = this._protrans.GetFiles(addr.Fileaddress, addr.Fileport, rpath, savePath, syscode, this._ifWriteTxtLog, this.WriteStrToLog, ref ftpResult, ref successSign, ref dlfailedFiles, this._ifDelDir);
                    if (ifGet)
                    {
                        ftpResult = 0;
                        logstr = "Download by pkuuid from ftpserver succees!";
                        if (this.WriteStrToLog != null) this.WriteStrToLog("7626 " + logstr, this._proIdAndThreadId);
                        if (this._ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                        #region  2016/03/05 zhoucy 文件若部分下载失败则返回8888，若全部下载失败则返回9999
                        if (dlfailedFiles.Count != 0)
                        {
                            if (successSign != 0)
                            {
                                returnMsg.ReturnCode = "8888";
                                returnMsg.Message = "";
                                foreach (var dlfailedFile in dlfailedFiles)
                                {
                                    returnMsg.ResultData = returnMsg.ResultData + dlfailedFile + "/";
                                }
                                returnMsg.ResultData = returnMsg.ResultData.Substring(0, returnMsg.ResultData.Length - 1);
                                //return returnMsg;
                            }
                            else
                            {
                                returnMsg.ReturnCode = "9999";
                                returnMsg.Message = "";
                                foreach (var dlfailedFile in dlfailedFiles)
                                {
                                    returnMsg.ResultData = returnMsg.ResultData + dlfailedFile + "/";
                                }
                                returnMsg.ResultData = returnMsg.ResultData.Substring(0, returnMsg.ResultData.Length - 1);
                            }

                            if (string.IsNullOrEmpty(addr.BSocketIp) || string.IsNullOrEmpty(addr.BFtpIp))
                            {
                                doWork = 0;
                            }
                            else
                            {
                                message.RecvSoketIp = addr.Messageaddress;
                                message.RecvSoketPort = Convert.ToString(addr.Messageport);
                                addr.FtpIp = addr.Fileaddress;
                                addr.FtpPort = addr.Fileport;
                                addr.FtpPath = addr.Filepath;
                                doWork--;
                            }
                        }
                        else
                            doWork = 0;
                       
                        #endregion
                    }
                    else
                    {
                        logstr = "Download by pkuuid  from ftpserver failed!";
                        if (this.WriteStrToLog != null) this.WriteStrToLog("7627 " + logstr, this._proIdAndThreadId);
                        if (this._ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                        if (ftpResult.ToString().Length >= 4)
                        {
                            returnMsg.ReturnCode = ftpResult.ToString();
                            returnMsg.Message = "";
                            returnMsg.ResultData = CommonFunc.GetErrorMsg(ftpResult.ToString());
                        }
                        else
                        {
                            returnMsg.ReturnCode = "7785";
                            returnMsg.Message = "";
                            returnMsg.ResultData = "Download files failed,return code:" + ftpResult;

                        }

                        if (string.IsNullOrEmpty(addr.BSocketIp) || string.IsNullOrEmpty(addr.BFtpIp))
                        {
                            doWork = 0;
                        }
                        else
                        {
                            message.RecvSoketIp = addr.Messageaddress;
                            message.RecvSoketPort = Convert.ToString(addr.Messageport);
                            addr.FtpIp = addr.Fileaddress;
                            addr.FtpPort = addr.Fileport;
                            addr.FtpPath = addr.Filepath;
                            doWork--;
                        }
                    }
                }
                else
                {
                    if (result.Length >= 4 && result.Substring(0, 4) == "7780")
                    {
                        if (string.IsNullOrEmpty(addr.BSocketIp) || string.IsNullOrEmpty(addr.BFtpIp))
                        {
                            doWork = 0;
                        }
                        else
                        {
                            message.RecvSoketIp = addr.Messageaddress;
                            message.RecvSoketPort = Convert.ToString(addr.Messageport);
                            addr.FtpIp = addr.Fileaddress;
                            addr.FtpPort = addr.Fileport;
                            addr.FtpPath = addr.Filepath;
                            doWork--;
                        }
                    }
                    else
                    {
                        doWork = 0;
                    }
                }

            }




            logstr = "End upload!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7606 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            
            


            
            return returnMsg;

        }

       


       
    }
}
