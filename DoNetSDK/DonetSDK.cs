
using System.Reflection;
using System.Runtime.InteropServices;
using Com.Boc.Icms.DoNetSDK.Bean;
using Com.Boc.Icms.DoNetSDK.Service;

namespace Com.Boc.Icms.DoNetSDK
{
    using System.ComponentModel.DataAnnotations;
    //using Com.Boc.Icms.DoNetSDK.Log;
    using System.Diagnostics;
    using System.Xml;

    public delegate void WriteLog(string logstr, string proIdAndThreadId);
    public delegate void TransferOnePage(string downLoadFileName, string filePath);
    public delegate void BeforeChaneName(ref string fileName, ref string changeToName);
    public delegate void AsynchronousTransferOperate(PageInfo downLoadFileInfo);
    public delegate void BeforeTransfer(int fileCount);

    public delegate ReturnMsg DownLoadPage(string contentList, string savePath);

    [ClassInterface(ClassInterfaceType.AutoDual)]
    public partial class DonetSdk :  IDonetSdk
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
            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
            string sdkpath =Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location),"SdkLog");
            string ftppath =Path.Combine( Path.GetDirectoryName(Assembly.GetCallingAssembly().Location),"ftplog");
            CommonFunc.DeleteHisLog(sdkpath, "DonetSdk_*.log", "^DonetSdk_[0-9]{8}\\w*.log$");
            CommonFunc.DeleteHisLog(ftppath, "ftpc*.log", "^ftpc[0-9]{8}.log$");
          //  FastLogger.Instance().Register();
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
        /// 新增交易信息及影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>     
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg AddBizAndImage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,string userData)
        {
            string logstr = "AddBizAndImage input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext + ",transTimeOut:" + transTimeOut + ",userData:" + userData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";            
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);
            //ApplyAddrMessageReturn addr = new ApplyAddrMessageReturn();
            //addr.ErrorMessage = "";
            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            //string file_path = addr.Filepath;
            //addr.Filepath = "COS1/c5a6b4b9be8b4c5598dc6d7171b09106";
            int doWork = 2;
            message.Transtype = "0001";

            //处理报文信息
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(userData);

            XmlNode reqheadernode = xmlDoc.SelectSingleNode("//request//req_header");
            XmlNode sys_code = reqheadernode.SelectSingleNode("sys_code");
            if (sys_code != null && String.IsNullOrEmpty( sys_code.InnerText))
            {
                sys_code.InnerText = message.Syscode;
            }
            XmlNode bank_code = reqheadernode.SelectSingleNode("bank_code");
            if (bank_code != null && String.IsNullOrEmpty(bank_code.InnerText))
            {
                bank_code.InnerText = message.Bankcode;
            }
            XmlNode branch_code = reqheadernode.SelectSingleNode("branch_code");
            if (branch_code != null && String.IsNullOrEmpty(branch_code.InnerText))
            {
                branch_code.InnerText = message.Branchcode;
            }
            XmlNode operater_id = reqheadernode.SelectSingleNode("operater_id");
            if (operater_id != null && String.IsNullOrEmpty(operater_id.InnerText))
            {
                operater_id.InnerText = message.Operaterid;
            }
            XmlNode trans_type = reqheadernode.SelectSingleNode("trans_type");
            if (trans_type != null && String.IsNullOrEmpty(trans_type.InnerText))
            {
                trans_type.InnerText = message.Transtype;
            }
            XmlNode versionNode = reqheadernode.SelectSingleNode("version");
            if (versionNode != null && String.IsNullOrEmpty(versionNode.InnerText))
            {
                versionNode.InnerText = message.Version;
            }
            XmlNode trans_time = reqheadernode.SelectSingleNode("trans_time");
            if (trans_time != null && String.IsNullOrEmpty(trans_time.InnerText))
            {
                trans_time.InnerText = message.Transtime;
            }
            XmlNode trans_id = reqheadernode.SelectSingleNode("trans_id");
            if (trans_id != null && String.IsNullOrEmpty(trans_id.InnerText))
            {
                trans_id.InnerText = message.Transid;
            }
            XmlNode client_type = reqheadernode.SelectSingleNode("client_type");
            if (client_type != null && String.IsNullOrEmpty(client_type.InnerText))
            {
                client_type.InnerText = message.Clienttype;
            }
            XmlNode client_ip = reqheadernode.SelectSingleNode("client_ip");
            if (client_ip != null && String.IsNullOrEmpty(client_ip.InnerText))
            {
                client_ip.InnerText = message.ClientIp;
            }
            //XmlNode starter_sys_code = reqheadernode.SelectSingleNode("starter_sys_code");
            //if (starter_sys_code != null && String.IsNullOrEmpty(starter_sys_code.InnerText))
            //{
            //    starter_sys_code.InnerText = syscode;
            //}

            XmlNode reqbody = xmlDoc.SelectSingleNode("//request//req_body");
            XmlNode file_pathnode = reqbody.SelectSingleNode("file_path");
            if (file_pathnode != null && String.IsNullOrEmpty(file_pathnode.InnerText))
            {
                file_pathnode.InnerText = addr.Filepath;
            }
            //XmlNode zpk = reqbody.SelectSingleNode("zpk");
            //if (zpk != null && String.IsNullOrEmpty(zpk.InnerText))
            //{
            //    zpk.InnerText = syscode;
            //}
            userData = xmlDoc.InnerXml;
            //while (doWork > 0)
            //{
            //申请AddBizAndImage              


            BaseManager.WriteLog("开始新增交易信息及影像：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("新增交易信息及影像返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.Message + "; Return data:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            BaseManager.WriteLog("End download by pkuuid" + logstr);


            logstr = "End download by pkuuid!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);


            return returnMsg;

        }

        /// <summary>
        /// 通过pkuuid调阅影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="startersyscode">发起方业务系统</param>
        /// <param name="pkuuid">文档编号</param>
        /// <param name="zpk">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param>
        /// <param name="iscurrentversion">是否调阅最新版本</param>
        /// <param name="versionlabel">版本号</param>
        /// <param name="isoriginal">是否调阅原图</param>
        /// <param name="resolution">调阅影像分辨率</param>
        /// <param name="includecontentfile">是否包含影像文件</param>
        /// <param name="includecontentfile">是否包含影像文件</param>
        /// <param name="filenames">文件名</param>
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownloadByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version,  string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string startersyscode,
                                          string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel, string isoriginal, string resolution, string includecontentfile, string savePath, string zpk = "", string filenames = "")
        {
            string logstr = "DownloadByPkuuId input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext + ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();
           
            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;         
            int successSign = 0;
           
            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage); 
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            string file_path = addr.Filepath;

            int doWork = 2;
            message.Transtype = "0002";
            //while (doWork > 0)
            //{
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
                                    "<client_ip>" + message.ClientIp + "</client_ip>" +
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

                BaseManager.WriteLog("开始通过pkuuid调阅影像：" + userData);
               
                result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

                BaseManager.WriteLog("通过pkuuid调阅影像返回结果：" + result);
                if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);


                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    if (xmldoc.SelectSingleNode("//ret_code") != null)
                    {
                        string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                        if (!string.IsNullOrEmpty(retcode))
                        {
                            returnMsg.ReturnCode = retcode;
                        }
                    }
                    if (xmldoc.SelectSingleNode("//ret_message") != null)
                    {
                        string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                        if (!string.IsNullOrEmpty(retmessage))
                        {
                            returnMsg.Message = retmessage;
                        }
                    }
                    returnMsg.ResultData = result;

                    logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.Message + "; Return data:" + returnMsg.ResultData;
                    if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
                    if (this._ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
                    
               
               
           // }
           
            logstr = "End download by pkuuid!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
            
           
            return returnMsg;

        }

        
        /// <summary>
        ///通过bizId调阅影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="startersyscode">发起方业务系统</param>
        /// <param name="pkuuids">文档编号</param>
        /// <param name="zpk">工作密钥密文</param>
        /// <param name="biz">交易编号</param>
        /// <param name="dataTypes">文档类型集合</param>
        /// <param name="imagelibraryident">影像库标识</param>
        /// <param name="isoriginal">是否调阅原图</param>
        /// <param name="resolution">调阅影像分辨率</param>      
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownloadByBizId(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string startersyscode,
                                          string biz,string pkuuids,string dataTypes, string imagelibraryident,  string isoriginal, string resolution,string savePath, string zpk = "")
        {

            string logstr = "DownloadByBizId input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" +
                        ciphertext + ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuids + ",biz:" + biz + ",dataTypes:" + dataTypes + ",imagelibraryident:" + imagelibraryident +
                        ",isoriginal:" + isoriginal+ ",resolution:" + resolution + ",savePath:" + savePath;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0003";
           
            //while (doWork > 0)
            //{
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
                                    "<client_ip>" + message.ClientIp + "</client_ip>" +
                                    "<starter_sys_code>" + startersyscode + "</starter_sys_code>" +
                                 "</req_header>" +
                                 "<req_body>" +
                                    "<zpk>" + zpk + "</zpk>" +
                                    "<biz_metadata>" + biz + "</biz_metadata>" +
                                    "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                    "<pkuuids>" + pkuuids + "</pkuuids>" +
                                    "<dataTypes>" + dataTypes + "</dataTypes>" +
                                    "<file_path>" + addr.Filepath + "</file_path>" +
                                    "<is_original>" + isoriginal + "</is_original>" +
                                    "<resolution>" + resolution + "</resolution>" +
                                 "</req_body>" +
                              "</request>";

                BaseManager.WriteLog("开始通过bizId调阅影像：" + userData);

                result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

                BaseManager.WriteLog("通过bizId调阅影像返回结果：" + result);
                if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

                //判断返回结果是否为xml字符串

               
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    if (xmldoc.SelectSingleNode("//ret_code") != null)
                    {
                        string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                        if (!string.IsNullOrEmpty(retcode))
                        {
                            returnMsg.ReturnCode = retcode;
                        }
                    }
                    if (xmldoc.SelectSingleNode("//ret_message") != null)
                    {
                        string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                        if (!string.IsNullOrEmpty(retmessage))
                        {
                            returnMsg.Message = retmessage;
                        }
                    }
                    returnMsg.ResultData = result;

                    logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
                    if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
                    if (this._ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
                    
                
            //}
  
            logstr = "End download by biz!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);


            return returnMsg;

        }

        /// <summary>
        /// 新增文档信息及影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>     
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg AddDocAndImage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string userData)
        {
            string logstr = "AddDocAndImage input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext + ",transTimeOut:" + transTimeOut + ",userData:" + userData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0001";
            //while (doWork > 0)
            //{
            //申请AddBizAndImage              


            BaseManager.WriteLog("开始新增文档信息及影像：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("新增文档信息及影像返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.Message + "; Return data:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
           


            logstr = "End AddDocAndImage by pkuuid!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);


            return returnMsg;

        }


        /// <summary>
        ///根据交易编号删除该交易信息
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="biz">交易编号</param>
        /// <param name="imagelibraryident">影像库标识</param>
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DeletedByBizId(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string imagelibraryident)
        {

            string logstr = "DeletedByBizId input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext + ",transTimeOut:" + transTimeOut + ",biz:" + biz;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0005";

            //while (doWork > 0)
            //{
                //根据交易编号删除该交易信息               
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
                                    "<client_ip>" + message.ClientIp + "</client_ip>" +
                                 "</req_header>" +
                                 "<req_body>" +
                                    "<biz_metadata>" + biz + "</biz_metadata>" +
                                    "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                 "</req_body>" +
                              "</request>";

                BaseManager.WriteLog("开始根据交易编号删除该交易信息：" + userData);

                result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

                BaseManager.WriteLog("根据交易编号删除该交易信息返回结果：" + result);
                if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

                //判断返回结果是否为xml字符串

                
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    if (xmldoc.SelectSingleNode("//ret_code") != null)
                    {
                        string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                        if (!string.IsNullOrEmpty(retcode))
                        {
                            returnMsg.ReturnCode = retcode;
                        }
                    }
                    if (xmldoc.SelectSingleNode("//ret_message") != null)
                    {
                        string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                        if (!string.IsNullOrEmpty(retmessage))
                        {
                            returnMsg.Message = retmessage;
                        }
                    }
                    returnMsg.ResultData = result;

                    logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
                    if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
                    if (this._ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
                    
                
            //}

            logstr = "End deleted by biz!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);


            return returnMsg;

        }


        /// <summary>
        ///根据pkuuid删除指定版本或最新版本影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DeletedByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string pkuuid, string imagelibraryident,string iscurrentversion,string versionlabel)
        {

            string logstr = "DeletedByPkuuid input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext + 
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",iscurrentversion:" + iscurrentversion+ ",versionlabel:" + versionlabel;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0006";

            //while (doWork > 0)
            //{
                //根据交易编号删除该交易信息               
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
                                    "<client_ip>" + message.ClientIp + "</client_ip>" +
                                 "</req_header>" +
                                 "<req_body>" +
                                    "<pkuuid>" + pkuuid + "</pkuuid>" +
                                    "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                    "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                    "<version_label>" + versionlabel + "</version_label>" +
                                 "</req_body>" +
                              "</request>";

                BaseManager.WriteLog("开始根据pkuuid删除指定版本或最新版本影像：" + userData);

                result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

                BaseManager.WriteLog("根据pkuuid删除指定版本或最新版本影像返回结果：" + result);
                if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

                //判断返回结果是否为xml字符串

              
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    if (xmldoc.SelectSingleNode("//ret_code") != null)
                    {
                        string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                        if (!string.IsNullOrEmpty(retcode))
                        {
                            returnMsg.ReturnCode = retcode;
                        }
                    }
                    if (xmldoc.SelectSingleNode("//ret_message") != null)
                    {
                        string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                        if (!string.IsNullOrEmpty(retmessage))
                        {
                            returnMsg.Message = retmessage;
                        }
                    }
                    returnMsg.ResultData = result;

                    logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
                    if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
                    if (this._ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
                    
                
            //}

            logstr = "End deleted by pkuuid!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }


        /// <summary>
        /// 通过page维度进行调阅影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownLoadbyPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                         string biz,string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel,int docindex,string pageflag,string pageuuid,int pageindex,string isoriginal,string resolution)
        {
            string logstr = "DownLoadbyPage input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",iscurrentversion:" + iscurrentversion + ",versionlabel:" + versionlabel
                      + ",docindex:" + docindex + ",pageflag:" + pageflag + ",pageuuid:" + pageuuid + ",pageindex:" + pageindex + ",isoriginal:" + isoriginal + ",resolution:" + resolution;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0010";

            //while (doWork > 0)
            //{
                //通过page维度进行调阅影像      
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
                                    "<client_ip>" + message.ClientIp + "</client_ip>" +
                                 "</req_header>" +
                                 "<req_body>" +
                                    "<biz_metadata>" + biz + "</biz_metadata>" +
                                    "<pkuuid>" + pkuuid + "</pkuuid>" +
                                    "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                    "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                    "<version_label>" + versionlabel + "</version_label>" +
                                    "<doc_index>" + docindex + "</doc_index>" +
                                    "<page_flag>" + pageflag + "</page_flag>" +
                                    "<page_uuid>" + pageuuid + "</page_uuid>" +
                                    "<page_index>" + pageindex + "</page_index>" +
                                    "<is_original>" + isoriginal + "</is_original>" +
                                    "<resolution>" + resolution + "</resolution>" +
                                 "</req_body>" +
                              "</request>";

                BaseManager.WriteLog("开始通过page维度进行调阅影像：" + userData);

                result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

                BaseManager.WriteLog("通过page维度进行调阅影像返回结果：" + result);
                if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

                //判断返回结果是否为xml字符串

                
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(result);

                    if (xmldoc.SelectSingleNode("//ret_code") != null)
                    {
                        string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                        if (!string.IsNullOrEmpty(retcode))
                        {
                            returnMsg.ReturnCode = retcode;
                        }
                    }
                    if (xmldoc.SelectSingleNode("//ret_message") != null)
                    {
                        string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                        if (!string.IsNullOrEmpty(retmessage))
                        {
                            returnMsg.Message = retmessage;
                        }
                    }
                    returnMsg.ResultData = result;

                    logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.Message + "; Return data:" + returnMsg.ResultData;
                    if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
                    if (this._ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
                    
                
            //}

            logstr = "End download by page!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }


        /// <summary>
        ///通过page维度删除影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DeleteByPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz,string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel,int docindex,string pageflag,string pageuuid)
        {

            string logstr = "DeleteByPage input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",iscurrentversion:" + iscurrentversion + ",versionlabel:" + versionlabel;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0006";

            //while (doWork > 0)
            //{
            //通过page维度删除影像               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<biz_metadata>" + biz + "</biz_metadata>" +
                                "<pkuuid>" + pkuuid + "</pkuuid>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                "<version_label>" + versionlabel + "</version_label>" +
                                "<doc_index>" + docindex + "</doc_index>" +
                                "<page_flag>" + pageflag + "</page_flag>" +
                                "<page_uuid>" + pageuuid + "</page_uuid>" +
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始根据pkuuid删除指定版本或最新版本影像：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("根据pkuuid删除指定版本或最新版本影像返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串

            
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(result);

                if (xmldoc.SelectSingleNode("//ret_code") != null)
                {
                    string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                    if (!string.IsNullOrEmpty(retcode))
                    {
                        returnMsg.ReturnCode = retcode;
                    }
                }
                if (xmldoc.SelectSingleNode("//ret_message") != null)
                {
                    string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                    if (!string.IsNullOrEmpty(retmessage))
                    {
                        returnMsg.Message = retmessage;
                    }
                }
                returnMsg.ResultData = result;

                logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
               
            
            //}

            logstr = "End deleted by pkuuid!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///通过交易编号查询影像列表
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg CheckListByBiz(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuids, string imagelibraryident,  string datatypes)
        {

            string logstr = "CheckListByBiz input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuids:" + pkuuids   ;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0013";

            //while (doWork > 0)
            //{
            //通过page维度删除影像               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<biz_metadata>" + biz + "</biz_metadata>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                "<pkuuids>" + pkuuids + "</pkuuids>" +
                                "<data_types>" + datatypes + "</data_types>" +
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始通过交易编号查询影像列表：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("通过交易编号查询影像列表返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End CheckListByBiz!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///判断bizId是否存在
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg CheckBizIsExists(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string imagelibraryident)
        {

            string logstr = "CheckBizIsExists input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",biz:" + biz + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0014";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<biz_metadata>" + biz + "</biz_metadata>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                               
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始判断bizId是否存在：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("判断bizId是否存在返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End CheckBizIsExists!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///判断bizId是否存在
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg CheckPkuuidIsExists(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string pkuuid, string imagelibraryident)
        {

            string logstr = "CheckPkuuidIsExists input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0015";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<pkuuid>" + pkuuid + "</pkuuid>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +

                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始判断pkuuid是否存在：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("判断pkuuid是否存在返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End CheckPkuuidIsExists!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///根据page列表删除元数据和影像文件
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DeleteMateDataByPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz,string pkuuid, string imagelibraryident,string iscurrentversion,string versionlabel,int docindex,string pageflag,string pageuuid,int pageindex)
        {

            string logstr = "DeleteMateDataByPage input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0018";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                 "<page_list>" +
                                     "<page_info>" +
                                        "<biz_metadata>" + biz + "</biz_metadata>" +
                                        "<pkuuid>" + pkuuid + "</pkuuid>" +
                                        "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                        "<version_label>" + versionlabel + "</version_label>" +
                                        "<doc_index>" + docindex + "</doc_index>" +
                                        "<page_flag>" + pageflag + "</page_flag>" +
                                        "<page_uuid>" + pageuuid + "</page_uuid>" +
                                        "<page_index>" + pageindex + "</page_index>" +
                                     "</page_info>" +
                                 "</page_list>" +
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始根据page列表删除元数据和影像文件：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("根据page列表删除元数据和影像文件返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End DeleteMateDataByPage!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///根据page列表删除元数据和影像文件
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DeleteMateDataByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel)
        {

            string logstr = "DeleteMateDataByPkuuid input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0019";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                 "<pkuuid_list>" +
                                     "<pkuuid_info>" +
                                        "<biz_metadata>" + biz + "</biz_metadata>" +
                                        "<pkuuid>" + pkuuid + "</pkuuid>" +
                                        "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                        "<version_label>" + versionlabel + "</version_label>" +
                                      "</pkuuid_info>" +
                                 "</pkuuid_list>" +
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始根据pkuuid列表删除元数据和影像文件：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("根据pkuuid列表删除元数据和影像文件返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End DeleteMateDataByPkuuid!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }
        /// <summary>
        ///根据page列表删除元数据和影像文件
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownLoadListByPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel,string includecontentfile,int docindex,string pageflag,string pageuuid,int pageindex,string isoriginal)
        {
            string logstr = "DownLoadListByPage input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",pkuuid:" + pkuuid + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0020";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                "<include_content_file>" + includecontentfile + "</include_content_file>" +
                                 "<page_list>" +
                                     "<page_info>" +
                                        "<biz_metadata>" + biz + "</biz_metadata>" +
                                        "<pkuuid>" + pkuuid + "</pkuuid>" +
                                        "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                        "<version_label>" + versionlabel + "</version_label>" +
                                        "<doc_index>" + docindex + "</doc_index>" +
                                        "<page_flag>" + pageflag + "</page_flag>" +
                                        "<page_uuid>" + pageuuid + "</page_uuid>" +
                                        "<page_index>" + pageindex + "</page_index>" +
                                        "<is_original>" + isoriginal + "</is_original>" +
                                      "</page_info>" +
                                 "</page_list>" +
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始通过page列表调阅影像：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("通过page列表调阅影像返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End DownLoadListByPage!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///通过bizMetadata列表查询影像列表
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownloadListByBizId(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string datatypes, string imagelibraryident,string includecontentfile)
        {
            string logstr = "DownloadListByBizId input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",datatypes:" + datatypes + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0021";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                "<include_content_file>" + includecontentfile + "</include_content_file>" +
                                 "<business_info_list>" +
                                     "<business_info>" +
                                        "<biz_metadata>" + biz + "</biz_metadata>" +
                                        "<data_types>" + datatypes + "</data_types>" +
                                      "</business_info>" +
                                 "</business_info_list>" +
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始通过bizMetadata列表查询影像列表：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("通过bizMetadata列表查询影像列表返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串


            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
           

            //}

            logstr = "End DownloadListByBizId!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }
       
        
        /// <summary>
        ///通过uniqMetadata下载
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownloadListByUniq(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string uniqmetadata, string datatypes, string imagelibraryident,string includecontentfile)
        {
            string logstr = "DownloadListByUniq input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",datatypes:" + datatypes + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0022";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<data_type>" + datatypes + "</data_type>" +
                                "<uniq_metadata>" + uniqmetadata + "</uniq_metadata>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                "<include_content_file>" + includecontentfile + "</include_content_file>" +                                
                             "</req_body>" +
                          "</request>";

            BaseManager.WriteLog("开始通过uniqMetadata下载：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("通过uniqMetadata下载返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
           

            //}

            logstr = "End DownloadListByBizId!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }

        /// <summary>
        ///通过pkuuid列表调阅影像
        /// </summary>
        /// <param name="syscode">请求方系统标识</param>
        /// <param name="bankcode">银行号</param>
        /// <param name="branchcode">分支结构号</param>
        /// <param name="operaterid">操作柜员号或虚拟柜员号</param>
        /// <param name="version">版本号</param>
        /// <param name="imagestoragemech">影像存储机构</param>
        /// <param name="originaltext">随机数原文</param>
        /// <param name="ciphertext">随机数密文</param>
        /// <param name="transTimeOut">传输超时时间</param>
        /// <param name="fileTransOneSize">单个文件传输大小</param>
        /// <param name="pkuuid">发起方业务系统</param>
        /// <param name="iscurrentversion">文档编号</param>
        /// <param name="versionlabel">工作密钥密文</param>
        /// <param name="imagelibraryident">影像库标识</param> 
        /// <returns>返回信息:包括返回码及返回消息</returns>
        public ReturnMsg DownloadListByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                              string datatypes, string imagelibraryident,string includecontentfile, string biz,string pkuuid,string iscurrentversion,string versionlabel,string isoriginal)
        {
            string logstr = "DownloadListByPkuuid input parameter: syscode:" + syscode + ",bankcode:" + bankcode + ",branchcode:" + branchcode + ",operaterid:" + operaterid
                      + ",version:" + version + ",imagestoragemech:" + imagestoragemech + ",originaltext:" + originaltext + ",ciphertext:" + ciphertext +
                      ",transTimeOut:" + transTimeOut + ",datatypes:" + datatypes + ",imagelibraryident:" + imagelibraryident;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7600 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            ReturnMsg returnMsg = new ReturnMsg();
            List<string> dlfailedFiles = new List<string>();

            string datetime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            string transtime = datetime.Remove(datetime.LastIndexOf(':'), 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string desDir;
            string result = "";
            string userData;
            string failedFileName = "";
            bool ifGet = false;
            int ftpResult = 0;
            int successSign = 0;

            this._ifWriteTxtLog = int.Parse(CommonFunc.GetJsonNodeValue("//WriteTxtLog"));
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
            message.Version = version;
            message.Transtime = transtime;
            message.Transid = transid;
            message.Clienttype = "01";
            message.ClientIp = CommonFunc.LocalIp();
            message.Imagestoragemech = imagestoragemech;
            message.Originaltext = originaltext;
            message.Ciphertext = ciphertext;

            ApplyAddrMessageReturn addr = BaseManager.ApplyTransmittedInformation(message, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            if (addr.ErrorMessage != "")
            {//安全验证并申请地址未通过
                BaseManager.WriteLog(addr.ErrorMessage);
                returnMsg.ReturnCode = addr.Retcode;
                returnMsg.Message = "";
                returnMsg.ResultData = addr.Retmessage;

                logstr = "Appling for address and port failed.Return code:" + returnMsg.ReturnCode + ";Return message:" + returnMsg.ResultData;
                if (this.WriteStrToLog != null) this.WriteStrToLog("7601 " + logstr, this._proIdAndThreadId);
                if (this._ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                BaseManager.WriteLog(logstr);
                return returnMsg;
            }

            logstr = "Appling for address and port succees";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7602 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);
            BaseManager.WriteLog(logstr);

            int doWork = 2;
            message.Transtype = "0023";

            //while (doWork > 0)
            //{
            //判断bizId是否存在               
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
                                "<client_ip>" + message.ClientIp + "</client_ip>" +
                             "</req_header>" +
                             "<req_body>" +
                                "<image_library_ident>" + imagelibraryident + "</image_library_ident>" +
                                "<include_content_file>" + includecontentfile + "</include_content_file>" +
                                "<pkuuid_list>" +
                                    "<pkuuid_info>" +
                                        "<biz_metadata>" + biz + "</biz_metadata>" +
                                        "<pkuuid>" + pkuuid + "</pkuuid>" +
                                        "<is_current_version>" + iscurrentversion + "</is_current_version>" +
                                        "<version_label>" + versionlabel + "</version_label>" +
                                        "<is_original>" + isoriginal + "</is_original>" +
                                    "</pkuuid_info>" +
                                "</pkuuid_list>" +
                            "</req_body>" +
                         "</request>";

            BaseManager.WriteLog("开始通过pkuuid列表调阅影像：" + userData);

            result = BaseManager.CommServer(message, userData, this._ifWriteTxtLog, this.WriteStrToLog, this._proIdAndThreadId);

            BaseManager.WriteLog("通过pkuuid列表调阅影像返回结果：" + result);
            if (this.WriteStrToLog != null) this.WriteStrToLog("7759 " + result, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", result, this._proIdAndThreadId);

            //判断返回结果是否为xml字符串

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(result);

            if (xmldoc.SelectSingleNode("//ret_code") != null)
            {
                string retcode = xmldoc.SelectSingleNode("//ret_code").InnerText;
                if (!string.IsNullOrEmpty(retcode))
                {
                    returnMsg.ReturnCode = retcode;
                }
            }
            if (xmldoc.SelectSingleNode("//ret_message") != null)
            {
                string retmessage = xmldoc.SelectSingleNode("//ret_message").InnerText;
                if (!string.IsNullOrEmpty(retmessage))
                {
                    returnMsg.Message = retmessage;
                }
            }
            returnMsg.ResultData = result;

            logstr = "Return code: " + returnMsg.ReturnCode + "; Return message:" + returnMsg.ResultData;
            if (this.WriteStrToLog != null) this.WriteStrToLog("7628 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), returnMsg.ReturnCode, logstr, this._proIdAndThreadId);
            

            //}

            logstr = "End DownloadListByBizId!";
            if (this.WriteStrToLog != null) this.WriteStrToLog("7629 " + logstr, this._proIdAndThreadId);
            if (this._ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

            return returnMsg;

        }



        /// <summary>
        /// 获取原文
        /// </summary>
        /// <returns>返回原文字符串</returns>
        public string GetSecurityNo()
        {
            try
            {
                this._securityNo = "";
                Random getRandom = new Random();
                for (int i = 0; i < 38; i++) this._securityNo += getRandom.Next(0, 10).ToString();
            }
            catch
            {
                this._securityNo = "11111111111111111111111111111111111111";
                return this._securityNo;
            }

            return this._securityNo;
        }

    
    }
}
