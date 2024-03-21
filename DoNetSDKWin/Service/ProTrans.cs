using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Com.Boc.Icms.DoNetSDK.Service
{
    /// <summary>
    /// 调用com方法，实现传输上传文件、下载文件、删除文件夹等主要操作
    /// </summary>
    class ProTrans
    {
        /// <summary>
        /// 打开ftp传输服务
        /// </summary>
        /// <param name="host">消息IP</param>
        /// <param name="port">消息端口</param>
        /// <returns>0表示成功</returns>
        [DllImport("ftpdllV07.3C.dll")]
        internal static extern int FTPopen(string host, int port);

        /// <summary>
        /// 关闭ftp传输服务
        /// </summary>
        /// <returns>0表示成功</returns>
        [DllImport("ftpdllV07.3C.dll")]
        internal static extern int FTPclose();

        /// <summary>
        /// 实现上传操作
        /// </summary>
        /// <param name="lfile">本地文件路径</param>
        /// <param name="rfile">远程路径</param>
        /// <returns>0表示成功</returns>
        [DllImport("ftpdllV07.3C.dll", CharSet = CharSet.Unicode)]
        internal static extern int FTPsend(string lfile, string rfile, string systemcode);

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="rfile">远程文件路径</param>
        /// <param name="lfile">本地保存路径</param>
        /// <returns>0表示成功</returns>
        [DllImport("ftpdllV07.3C.dll", CharSet = CharSet.Unicode)]
        internal static extern int FTPrecv(string rfile, string lfile, string systemcode);

        /// <summary>
        /// 检索远程文件列表
        /// </summary>
        /// <param name="rpath">远程路径</param>
        /// <param name="list">文件列表</param>
        /// <param name="listlen">列表容量</param>
        /// <returns>0表示成功</returns>
        [DllImport("ftpdllV07.3C.dll", CharSet = CharSet.Unicode)]
        internal static extern int FTPlist(string rpath, byte[] list, int listlen, string systemcode);

        /// <summary>
        /// 删除路径
        /// </summary>
        /// <param name="rpath">远程路径</param>
        /// <returns>0表示成功</returns>
        [DllImport("ftpdllV07.3C.dll", CharSet = CharSet.Unicode)]
        internal static extern int FTPrmdir(string rpath, string systemcode);

        /// <summary>
        /// 设置超时时间
        /// </summary>
        /// <param name="rpath"></param>
        /// <returns></returns>
        [DllImport("ftpdllV07.3C.dll")]
        internal static extern int FTPsetflag(int id, int flag);

        /// <summary>
        /// 获取超时时间
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DllImport("ftpdllV07.3C.dll")]
        internal static extern int FTPgetflag(int id);

        /// <summary>
        /// 定义变量
        /// </summary>
        internal string PFtpIp;  //IP地址
        internal int PFtpPort;   //端口

        private int CftpListLen = 320000;
        private int _hNetOpen = -1;


        //private readonly ProgressBar _processBar = null;  废弃进度条，以事件代替

        internal event TransferOnePage BeforeDownLoadOnePage = null;
        internal event TransferOnePage DownLoadSuccessOnePage = null;
        internal event BeforeChaneName BeforeChangeName = null;
        internal event TransferOnePage BeforeUploadLoadOnePage = null;
        internal event BeforeTransfer BeforeTransfer = null;

        //调用API用外部软件打开相应的文件
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hChild, IntPtr hParent);

        private readonly string _proIdAndThreadId = string.Empty;

        public ProTrans()
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }

        internal void SetProTrans(IntPtr parentControl, int pwidth, int pheight)
        {
            if (parentControl != null)
            {
                //ProcessBar = new ProgressBar();
                //ProcessBar.Width = pwidth;
                //ProcessBar.Height = pheight;
                //SetParent(ProcessBar.Handle, parentControl);
                ////ProcessBar.Parent = parentControl;
                //ProcessBar.Dock = DockStyle.Fill;
                //ProcessBar.Visible = false;
            }
        }

        #region old getfiles
        /*/// <summary>
        /// 从ftp指定目录下载文件到本机指定目录
        /// </summary>
        /// <param name="ftpIp">ftp服务器ip地址</param>
        /// <param name="ftpPort">ftp服务器ip地址</param>
        /// <param name="ftpRemotePath">ftp远程文件路径</param>
        /// <param name="ftpLocalPath">下载到本地的路径</param>
        /// <param name="ifDel"></param>
        /// <returns>true: 返回成功；false: 返回失败</returns>
        internal bool GetFiles(string ftpIp, int ftpPort, string ftpRemotePath, string ftpLocalPath, string systemid, int ifWriteTxtLog, WriteLog writeStrLog, ref int ftpResult, ref int successSign,ref List<string> dlfailedFiles, bool ifDel = true)
        {
            int hNetGet = -1;   //FTPrecv返回值
            int hNetDelDir = -1;  //删除目录返回值
            int hNetFind = -1;  //FTPList返回值
            int fcount = 0;  //下载文件数
            //string Os = "";  //合法路径名
            //byte[] list = new byte[2048]; //接收FTPList输出参数
            byte[] list = new byte[320000]; //文件多点且文件名长点的话，必要的时候需扩充
            string strFileName = ""; //存放下载文件名
            string[] strFileNameList = new string[] { }; //存放下载文件的文件名数组
            string logstr = "";
            
            string pFtpRemotePath;  //服务器路径
            string pFtpLocalPath;   //本地路径
            string pFtpRemoteFilePathName;
            string pFtpLocalFilePathName;

            //尝试下载次数
            int downloadCount = 0;
            successSign = -1;//用于标识文件下载成功

            this.PFtpIp = ftpIp;
            this.PFtpPort = ftpPort;
            pFtpRemotePath = ftpRemotePath;
            pFtpLocalPath = ftpLocalPath;

            //根据操作系统类型判断路径格式
            //Os = JudgeOsFormat(p_FtpRemotePath, p_FtpLocalPath);
            //p_FtpRemotePath = GetString("#5" + Os, "#5", "?",false);
            //p_FtpLocalPath = GetString(Os + "#5", "?", "#5");

            try
            {
                //建立连接
                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);

                //连接失败,返回false
                if (this._hNetOpen != 0)
                {
                    logstr = "Open the ftp port failed,the result:" + this._hNetOpen;
                    if (writeStrLog != null)
                        writeStrLog("7711 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    FTPclose();
                    ftpResult = this._hNetOpen;
                    return false;
                }
                else
                {
                    logstr = "Open the ftp port succees";
                    if (writeStrLog != null)
                        writeStrLog("7712 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                }

                if (int.Parse(CommonFunc.GetXmlNodeValue("//TotalFilesLength")) > 0)
                {
                    int totalFilesLength = int.Parse(CommonFunc.GetXmlNodeValue("//TotalFilesLength"));
                    list = new byte[totalFilesLength];
                    CftpListLen = totalFilesLength;

                    if (ifWriteTxtLog == 1)
                    {
                        logstr = "TotalFilesLength=" + totalFilesLength.ToString();
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    }
                }

                //检索远程文件路径
                hNetFind = FTPlist(pFtpRemotePath, list, CftpListLen, systemid);
                //检索文件失败，返回false
                if (hNetFind != 0)
                {
                    logstr = "Get the files from FTPlist failed, the return number is" + hNetFind;
                    if (writeStrLog != null)
                        writeStrLog("7713 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    ftpResult = hNetFind;
                    return false;
                }
                else
                {
                    logstr = "Get the files from FTPlist succees.";
                    if (writeStrLog != null)
                        writeStrLog("7714 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    FTPclose();
                }
                //从FTPlist得到要下载的文件名字符串
                strFileName = Encoding.Unicode.GetString(list); //strFileName = Encoding.Default.GetString(list); //edit by wangyan on 6.28
                logstr = "Retrievaled result:" + strFileName.TrimEnd('\0');
                if (writeStrLog != null)
                    writeStrLog("7715 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                //重新建立连接
                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                //连接失败,返回false
                if (this._hNetOpen != 0)
                {
                    logstr = "Open the ftp port failed,the return number is " + this._hNetOpen;
                    if (writeStrLog != null)
                        writeStrLog("7716 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    ftpResult = this._hNetOpen;
                    return false;
                }

                try
                {
                    //下载文件名数组
                    strFileNameList = strFileName.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries); //strFileNameList = strFileName.Split('|');  //edit by wangyan on 6.27
                    //下载文件数
                    fcount = strFileNameList.Length - 1;
                    //下载文件为0，返回false
                    if (fcount <= 0)
                    {
                        logstr = "Retrievaled files count is 0";
                        if (writeStrLog != null)
                            writeStrLog("7717 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                        return false;
                    }

                    //本地目录如不存在，创建本地目录
                    if (Directory.Exists(pFtpLocalPath) == false)
                    {
                        Directory.CreateDirectory(pFtpLocalPath);
                    }

                    logstr = "Start downloadind files";
                    if (writeStrLog != null)
                        writeStrLog("7718 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                    if (this._processBar != null)
                    {
                        this._processBar.Visible = true;
                        this._processBar.Maximum = fcount;
                        this._processBar.Minimum = 0;
                        this._processBar.Value = 0;
                    }
                    if (this.BeforeTransfer != null)
                    {
                        string[] fileNameList = new string[fcount];
                        Array.Copy(strFileNameList, 0, fileNameList, 0, fcount);
                        this.InvokeBeforeTransfer(fileNameList);
                    }

                    if (pFtpRemotePath.LastIndexOf('/') != pFtpRemotePath.Length - 1)
                        pFtpRemotePath = pFtpRemotePath + '/';

                    if (pFtpLocalPath.LastIndexOf('/') != pFtpLocalPath.Length - 1)
                        pFtpLocalPath = pFtpLocalPath + '/';

                    for (int i = 0; i < fcount; i++)
                    {
                        //避免下载多个文件假死现象
                        //Application.DoEvents();

                        //取文件名
                        strFileName = strFileNameList[i];
                        //设置远程文件路径及本地文件路径
                        pFtpRemoteFilePathName = pFtpRemotePath + strFileName;
                        pFtpLocalFilePathName = pFtpLocalPath + strFileName;

                        logstr = "Downloading the " + i + "file. The downloading file local path is " + pFtpLocalFilePathName + ",and remote path is " + pFtpRemoteFilePathName;
                        if (writeStrLog != null)
                            writeStrLog("7719 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
    
                        if (this.BeforeDownLoadOnePage != null)
                        {
                            this.BeforeDownLoadOnePage.Invoke(strFileName, pFtpLocalPath);
                        }
                         
                        if (this._hNetOpen != 0)
                        {
                            this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);                            
                        }
                        if (this._hNetOpen == 0)
                        {
                            //下载文件
                            hNetGet = FTPrecv(pFtpRemoteFilePathName, pFtpLocalFilePathName, systemid);
                            if (hNetGet != 0)
                            {
                                if (File.Exists(pFtpLocalFilePathName))
                                {
                                    File.Delete(pFtpLocalFilePathName);
                                }
                            }
                            else
                            {
                                if (this.DownLoadSuccessOnePage != null)
                                {
                                    this.DownLoadSuccessOnePage.Invoke(strFileName, pFtpLocalPath);
                                }
                            }
                        }
                        else
                        {
                            if (this._processBar != null)
                            {
                                if (this._processBar.Value < this._processBar.Maximum)
                                {
                                    this._processBar.Value += 1;
                                }
                            }

                            logstr = "Open the ftp port failed,the result:" + this._hNetOpen;
                            if (writeStrLog != null)
                                writeStrLog("7711 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                            //dlfailedFiles.Add(strFileName + ": " + this._hNetOpen);
                            if (this._hNetOpen == 9821 || this._hNetOpen == 9825 || this._hNetOpen == 9897)
                            {
                                //添加上传失败文件名字、大小及耗时
                                dlfailedFiles.Add("File name[" + strFileName + "],File size[" + ProTrans.FTPgetflag(20).ToString() + "],Cost time[" + ProTrans.FTPgetflag(19).ToString() + "]:" + this._hNetOpen.ToString());
                            }
                            else
                            {
                                dlfailedFiles.Add("FileName[" + strFileName + "]: " + this._hNetOpen.ToString());
                            }
                            continue;
                        }
                        // 2016/03/05 zhoucy 若文件下载失败，再下载一次，无论成功还是失败再继续下载下面的文件
                        downloadCount++;
                        if (hNetGet != 0)
                        {
                            if (downloadCount == 1)
                            {                                
                                //继续下载一次这个文件
                                i = i - 1;

                                //重新打开
                                FTPclose();
                                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);

                                //继续下载
                                continue;
                            }
                            downloadCount = 0;
                            if (this._processBar != null)
                            {
                                if (this._processBar.Value < this._processBar.Maximum)
                                {
                                    this._processBar.Value += 1;
                                }
                            }
                            logstr = "Download the file \"" + strFileName + "\" failed,the return number is " + hNetGet;
                            if (writeStrLog != null)
                                writeStrLog("7720 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                                                        
                            //dlfailedFiles.Add(strFileName + ": " + hNetGet);
                            if (hNetGet == 9821 || hNetGet == 9825 || hNetGet == 9897)
                            {
                                //添加下载失败文件名字、大小及耗时
                                dlfailedFiles.Add("File name[" + strFileName + "],File size[" + ProTrans.FTPgetflag(20).ToString() + "],Cost time[" + ProTrans.FTPgetflag(19).ToString() + "]:" + hNetGet.ToString());
                            }
                            else
                            {
                                dlfailedFiles.Add("FileName[" + strFileName + "]: " + hNetGet.ToString());
                            }

                            FTPclose();
                            this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                            //return false;
                            continue;
                        }
                        successSign = 0;
                        downloadCount = 0;
                        if (this._processBar != null)
                        {
                            if (this._processBar.Value < this._processBar.Maximum)
                            {
                                this._processBar.Value += 1;
                            }
                        }

                        logstr = "Download the file \"" + strFileName + "\" succees";
                        if (writeStrLog != null)
                            writeStrLog("7721 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                        //if (ifWriteTxtLog == 1)
                        //    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    }

                    if (ifDel)
                    {
                        //删除远程目录
                        hNetDelDir = this.DelDir(pFtpRemotePath, systemid);
                        //删除远程目录失败，返回false
                        if (hNetDelDir != 0)
                        {
                            logstr = "Deleting the directory failed,the result is " + hNetDelDir;
                            if (writeStrLog != null)
                                writeStrLog("7722 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                            return true;
                        }
                        else
                        {
                            logstr = "Deleting the directory succees";
                            if (writeStrLog != null)
                                writeStrLog("7723 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logstr = "Error occured when downloading files,the exception is" + ex.Message;
                    if (writeStrLog != null)
                        writeStrLog("7724 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    return false;
                }
            }
            catch (Exception ex)
            {
                logstr = "Error occured when downloading files,the exception is" + ex.Message;
                if (writeStrLog != null)
                    writeStrLog("7725 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                return false;
            }
            finally
            {
                //关闭连接
                FTPclose();
                if (this._processBar != null) this._processBar.Visible = false;
            }

            return true;
        }
*/
        #endregion

        /// <summary>
        /// 从ftp指定目录下载文件到本机指定目录
        /// </summary>
        /// <param name="ftpIp">ftp服务器ip地址</param>
        /// <param name="ftpPort">ftp服务器ip地址</param>
        /// <param name="ftpRemotePath">ftp远程文件路径</param>
        /// <param name="ftpLocalPath">下载到本地的路径</param>
        /// <param name="ifDel"></param>
        /// <returns>true: 返回成功；false: 返回失败</returns>
        internal bool GetFiles(string ftpIp, int ftpPort, string ftpRemotePath, string ftpLocalPath, string systemid, int ifWriteTxtLog, WriteLog writeStrLog, ref int ftpResult, ref int successSign, ref List<string> dlfailedFiles, bool ifDel = true)
        {
            int hNetGet = -1;   //FTPrecv返回值
            int hNetDelDir = -1;  //删除目录返回值
            int hNetFind = -1;  //FTPList返回值
            int fcount = 0;  //下载文件数
            //string Os = "";  //合法路径名
            //byte[] list = new byte[2048]; //接收FTPList输出参数
            byte[] list = new byte[320000]; //文件多点且文件名长点的话，必要的时候需扩充
            string strFileName = ""; //存放下载文件名
            string[] strFileNameList = new string[] { }; //存放下载文件的文件名数组
            string logstr = "";

            string pFtpRemotePath;  //服务器路径
            string pFtpLocalPath;   //本地路径
            string pFtpRemoteFilePathName;
            string pFtpLocalFilePathName;

            //尝试下载次数
            int downloadCount = 0;
            successSign = -1;//用于标识文件下载成功

            this.PFtpIp = ftpIp;
            this.PFtpPort = ftpPort;
            pFtpRemotePath = ftpRemotePath;
            pFtpLocalPath = ftpLocalPath;

            //根据操作系统类型判断路径格式
            //Os = JudgeOsFormat(p_FtpRemotePath, p_FtpLocalPath);
            //p_FtpRemotePath = GetString("#5" + Os, "#5", "?",false);
            //p_FtpLocalPath = GetString(Os + "#5", "?", "#5");

            try
            {
                //建立连接
                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);

                //连接失败,返回false
                if (this._hNetOpen != 0)
                {
                    logstr = "Open the ftp port failed,the result:" + this._hNetOpen;
                    if (writeStrLog != null)
                        writeStrLog("7711 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    FTPclose();
                    ftpResult = this._hNetOpen;
                    return false;
                }
                else
                {
                    logstr = "Open the ftp port succees";
                    if (writeStrLog != null)
                        writeStrLog("7712 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                }

                if (int.Parse(CommonFunc.GetXmlNodeValue("//TotalFilesLength")) > 0)
                {
                    int totalFilesLength = int.Parse(CommonFunc.GetXmlNodeValue("//TotalFilesLength"));
                    list = new byte[totalFilesLength];
                    CftpListLen = totalFilesLength;

                    if (ifWriteTxtLog == 1)
                    {
                        logstr = "TotalFilesLength=" + totalFilesLength.ToString();
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    }
                }

                //检索远程文件路径
                hNetFind = FTPlist(pFtpRemotePath, list, CftpListLen, systemid);
                //检索文件失败，返回false
                if (hNetFind != 0)
                {
                    logstr = "Get the files from FTPlist failed, the return number is" + hNetFind;
                    if (writeStrLog != null)
                        writeStrLog("7713 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    ftpResult = hNetFind;
                    return false;
                }
                else
                {
                    logstr = "Get the files from FTPlist succees.";
                    if (writeStrLog != null)
                        writeStrLog("7714 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    FTPclose();
                }
                //从FTPlist得到要下载的文件名字符串
                strFileName = Encoding.Unicode.GetString(list); //strFileName = Encoding.Default.GetString(list); //edit by wangyan on 6.28
                logstr = "Retrievaled result:" + strFileName.TrimEnd('\0');
                if (writeStrLog != null)
                    writeStrLog("7715 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                //重新建立连接
                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                //连接失败,返回false
                if (this._hNetOpen != 0)
                {
                    logstr = "Open the ftp port failed,the return number is " + this._hNetOpen;
                    if (writeStrLog != null)
                        writeStrLog("7716 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    ftpResult = this._hNetOpen;
                    return false;
                }

                try
                {
                    //下载文件名数组
                    strFileNameList = strFileName.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries); //strFileNameList = strFileName.Split('|');  //edit by wangyan on 6.27
                    //下载文件数
                    fcount = strFileNameList.Length - 1;
                    //下载文件为0，返回false
                    if (fcount <= 0)
                    {
                        logstr = "Retrievaled files count is 0";
                        if (writeStrLog != null)
                            writeStrLog("7717 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                        return false;
                    }

                    //本地目录如不存在，创建本地目录
                    if (Directory.Exists(pFtpLocalPath) == false)
                    {
                        Directory.CreateDirectory(pFtpLocalPath);
                    }

                    logstr = "Start downloadind files";
                    if (writeStrLog != null)
                        writeStrLog("7718 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                    //if (this._processBar != null)
                    //{
                    //    this._processBar.Visible = true;
                    //    this._processBar.Maximum = fcount;
                    //    this._processBar.Minimum = 0;
                    //    this._processBar.Value = 0;
                    //}
                    if (this.BeforeTransfer != null)
                    {
                        string[] fileNameList = new string[fcount];
                        Array.Copy(strFileNameList, 0, fileNameList, 0, fcount);
                        this.InvokeBeforeTransfer(fileNameList);
                    }


                    for (int i = 0; i < fcount; i++)
                    {
                        //避免下载多个文件假死现象
                        //Application.DoEvents();

                        //取文件名
                        strFileName = strFileNameList[i];


                        logstr = "Downloading the " + i + "file";
                        if (writeStrLog != null)
                            writeStrLog("7719 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);


                        if (this._hNetOpen != 0)
                        {
                            this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                        }
                        if (this._hNetOpen == 0)
                        {
                            //下载文件
                            hNetGet = this.GetFile(pFtpRemotePath, pFtpLocalPath, strFileName, systemid, ifWriteTxtLog, writeStrLog);

                        }
                        else
                        {
                            //if (this._processBar != null)
                            //{
                            //    if (this._processBar.Value < this._processBar.Maximum)
                            //    {
                            //        this._processBar.Value += 1;
                            //    }
                            //}

                            logstr = "Open the ftp port failed,the result:" + this._hNetOpen;
                            if (writeStrLog != null)
                                writeStrLog("7711 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                            //dlfailedFiles.Add(strFileName + ": " + this._hNetOpen);
                            if (this._hNetOpen == 9821 || this._hNetOpen == 9825 || this._hNetOpen == 9897)
                            {
                                //添加上传失败文件名字、大小及耗时
                                dlfailedFiles.Add("File name[" + strFileName + "],File size[" + ProTrans.FTPgetflag(20).ToString() + "],Cost time[" + ProTrans.FTPgetflag(19).ToString() + "]:" + this._hNetOpen.ToString());
                            }
                            else
                            {
                                dlfailedFiles.Add("FileName[" + strFileName + "]: " + this._hNetOpen.ToString());
                            }
                            continue;
                        }
                        // 2016/03/05 zhoucy 若文件下载失败，再下载一次，无论成功还是失败再继续下载下面的文件
                        downloadCount++;
                        if (hNetGet != 0)
                        {
                            if (downloadCount == 1)
                            {
                                //继续下载一次这个文件
                                i = i - 1;

                                //重新打开
                                FTPclose();
                                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);

                                //继续下载
                                continue;
                            }
                            downloadCount = 0;
                            //if (this._processBar != null)
                            //{
                            //    if (this._processBar.Value < this._processBar.Maximum)
                            //    {
                            //        this._processBar.Value += 1;
                            //    }
                            //}
                            logstr = "Download the file \"" + strFileName + "\" failed,the return number is " + hNetGet;
                            if (writeStrLog != null)
                                writeStrLog("7720 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                            //dlfailedFiles.Add(strFileName + ": " + hNetGet);
                            if (hNetGet == 9821 || hNetGet == 9825 || hNetGet == 9897)
                            {
                                //添加下载失败文件名字、大小及耗时
                                dlfailedFiles.Add("File name[" + strFileName + "],File size[" + ProTrans.FTPgetflag(20).ToString() + "],Cost time[" + ProTrans.FTPgetflag(19).ToString() + "]:" + hNetGet.ToString());
                            }
                            else
                            {
                                dlfailedFiles.Add("FileName[" + strFileName + "]: " + hNetGet.ToString());
                            }

                            FTPclose();
                            this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                            //return false;
                            continue;
                        }
                        successSign = 0;
                        downloadCount = 0;
                        //if (this._processBar != null)
                        //{
                        //    if (this._processBar.Value < this._processBar.Maximum)
                        //    {
                        //        this._processBar.Value += 1;
                        //    }
                        //}

                        logstr = "Download the file \"" + strFileName + "\" succees";
                        if (writeStrLog != null)
                            writeStrLog("7721 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                        //if (ifWriteTxtLog == 1)
                        //    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    }

                    if (ifDel)
                    {
                        //删除远程目录
                        hNetDelDir = this.DelDir(pFtpRemotePath, systemid);
                        //删除远程目录失败，返回false
                        if (hNetDelDir != 0)
                        {
                            logstr = "Deleting the directory failed,the result is " + hNetDelDir;
                            if (writeStrLog != null)
                                writeStrLog("7722 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                            return true;
                        }
                        else
                        {
                            logstr = "Deleting the directory succees";
                            if (writeStrLog != null)
                                writeStrLog("7723 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logstr = "Error occured when downloading files,the exception is" + ex.Message;
                    if (writeStrLog != null)
                        writeStrLog("7724 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    return false;
                }
            }
            catch (Exception ex)
            {
                logstr = "Error occured when downloading files,the exception is" + ex.Message;
                if (writeStrLog != null)
                    writeStrLog("7725 " + CommonFunc.GetLineNum() + " " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                return false;
            }
            finally
            {
                //关闭连接
                FTPclose();
                //if (this._processBar != null) this._processBar.Visible = false;
            }

            return true;
        }

        private readonly object Object = new object();
        /// <summary>
        /// 从本机指定目录上传到ftp指定目录
        /// </summary>
        /// <param name="ftpIp">ftp服务器ip地址</param>
        /// <param name="ftpPort">ftp服务器ip地址</param>
        /// <param name="ftpRemotePath">ftp远程文件路径</param>
        /// <param name="ftpLocalPath">上传的本地文件路径</param>
        /// <returns>true: 返回成功；false: 返回失败</returns>
        internal bool PutFilesMutiThread(string ftpIp, int ftpPort, string ftpRemotePath, string ftpLocalPath, string systemid, int ifWriteTxtLog, WriteLog writeStrLog, int upLoadFileThreadCount, ref string ftpResult, ref string failedFileName)
        {
            int fcount = 0;   //文件数
            //string Os = "";   //合法路径
            //string names = ""; //本地上传文件名字符串
            string logstr = "";

            string pFtpRemotePath;  //服务器路径
            string pFtpLocalPath;   //本地路径


            this.PFtpIp = ftpIp;
            this.PFtpPort = ftpPort;
            pFtpRemotePath = ftpRemotePath;
            pFtpLocalPath = ftpLocalPath;

            try
            {
                //需判断系统类型
                //Os = JudgeOsFormat(p_FtpRemotePath, p_FtpLocalPath);
                //p_FtpLocalPath = GetString(Os + "#5", "?", "#5");
                //获取本地文件数组
                DirectoryInfo directoryInfo = new DirectoryInfo(pFtpLocalPath);
                FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

                //统计文件数
                fcount = fileInfos.Length;

                //文件数为0，返回false
                if (fcount <= 0)
                {
                    logstr = "The uploading files count is 0";
                    if (writeStrLog != null)
                        writeStrLog("7727 " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    return false;
                }

                logstr = "Start uploadind files";
                if (writeStrLog != null)
                    writeStrLog("7728 " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                if (this.BeforeTransfer != null)
                {
                    int exFileCount = 0;
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if ((fileInfo.Name.ToLower() == "index.xml") || fileInfo.Name.ToLower().EndsWith(".atn") || (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                        {
                            exFileCount++;
                        }
                    }
                    this.BeforeTransfer.Invoke(fileInfos.Length - exFileCount);
                }

                if (pFtpRemotePath.IndexOf('/') != pFtpRemotePath.Length - 1)
                    pFtpRemotePath = pFtpRemotePath + '/';

                List<FileInfo> fileInfosList = new List<FileInfo>(fileInfos);


                string rusult = "";
                List<Thread> threadList = new List<Thread>();
                for (int i = 0; i < upLoadFileThreadCount; i++)
                {
                    Thread UpLoadFileThread = new Thread(
                        () =>
                        {
                            #region 上传单个文件
                            try
                            {
                                string logstrThread = "";
                                string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
                                int hNetOpen = -1;

                                while (fileInfosList.Count > 0)
                                {
                                    string fileName = "";

                                    string pFtpRemoteFilePathName;
                                    string pFtpLocalFilePathName;

                                    lock (Object)
                                    {
                                        if (fileInfosList.Count > 0)
                                        {
                                            if ((fileInfosList[0].Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                            {
                                                fileInfosList.RemoveAt(0);
                                                continue;
                                            }
                                            pFtpLocalFilePathName = fileInfosList[0].FullName;
                                            fileName = fileInfosList[0].Name;
                                            pFtpRemoteFilePathName = pFtpRemotePath + fileInfosList[0];
                                            fileInfosList.RemoveAt(0);
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }



                                    int iDoWork = 2; //最坏情况下跑两次（即失败重试一次）
                                    while (iDoWork-- > 0)
                                    {
                                        if (hNetOpen != 0)
                                        {

                                            //建立连接
                                            hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                                            //连接失败，返回false
                                            if (hNetOpen != 0)
                                            {
                                                logstrThread = "Open the ftp port failed,the result:" + hNetOpen;
                                                if (writeStrLog != null)
                                                    writeStrLog("7726 " + logstrThread, proIdAndThreadId);
                                                if (ifWriteTxtLog == 1)
                                                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstrThread, proIdAndThreadId);
                                            }

                                        }

                                        //ftpOpened已经打开，上传文件
                                        if (hNetOpen == 0)
                                        {
                                            if (this.BeforeUploadLoadOnePage != null)
                                            {
                                                this.BeforeUploadLoadOnePage.Invoke(fileName, pFtpLocalPath);
                                            }

                                            logstrThread = "Start Upload file. The uploading file local path is " + fileName + ",and remote path is " + pFtpRemoteFilePathName;
                                            if (writeStrLog != null)
                                                writeStrLog("7729 " + logstrThread, proIdAndThreadId);
                                            if (ifWriteTxtLog == 1)
                                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstrThread, proIdAndThreadId);

                                            int hNetput = FTPsend(pFtpLocalFilePathName, pFtpRemoteFilePathName, systemid);

                                            if (hNetput == 0) //成功后直接退出
                                            {
                                                break;
                                            }
                                            else //传输失败
                                            {
                                                logstrThread = "Upload the file \"" + pFtpLocalFilePathName + "\" failed,the return number is " + hNetput;
                                                if (writeStrLog != null)
                                                    writeStrLog("7720 " + CommonFunc.GetLineNum() + " " + logstrThread, proIdAndThreadId);
                                                if (ifWriteTxtLog == 1)
                                                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstrThread, proIdAndThreadId);


                                                if (iDoWork == 0) //重试失败后做的动作
                                                {
                                                    lock (Object)
                                                    {
                                                        rusult = hNetput.ToString();
                                                        fileInfosList.Clear();
                                                    }
                                                }
                                                else
                                                {
                                                    //重新开关连接
                                                    FTPclose();
                                                    //建立连接
                                                    hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                                                    //连接失败，返回false
                                                    if (hNetOpen != 0)
                                                    {
                                                        logstrThread = "Open the ftp port failed,the result:" + hNetOpen;
                                                        if (writeStrLog != null)
                                                            writeStrLog("7726 " + logstrThread, proIdAndThreadId);
                                                        if (ifWriteTxtLog == 1)
                                                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstrThread, proIdAndThreadId);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (iDoWork == 0) //重试失败后做的动作
                                            {
                                                lock (Object)
                                                {
                                                    rusult = hNetOpen.ToString();
                                                    fileInfosList.Clear();
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                logstr = "Error occured when uploading files,the exception is" + ex.Message;
                                if (writeStrLog != null)
                                    writeStrLog("7732 " + logstr, this._proIdAndThreadId);
                                if (ifWriteTxtLog == 1)
                                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                                lock (Object)
                                {
                                    rusult = ex.Message;
                                    fileInfosList.Clear();
                                }



                            }
                            finally
                            {
                                //关闭连接
                                FTPclose();
                            }
                            #endregion
                        });

                    UpLoadFileThread.Start();
                    threadList.Add(UpLoadFileThread);



                }

                foreach (Thread thread in threadList)
                {
                    thread.Join();
                }

                if (rusult != "")
                {
                    ftpResult = rusult;
                    return false;
                }
            }
            catch (Exception ex)
            {
                logstr = "Error occured when uploading files,the exception is" + ex.Message;
                if (writeStrLog != null)
                    writeStrLog("7732 " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                ftpResult = ex.Message;

                return false;
            }

            //正常返回true
            return true;
        }


        /// <summary>
        /// 从本机指定目录上传到ftp指定目录
        /// </summary>
        /// <param name="ftpIp">ftp服务器ip地址</param>
        /// <param name="ftpPort">ftp服务器ip地址</param>
        /// <param name="ftpRemotePath">ftp远程文件路径</param>
        /// <param name="ftpLocalPath">上传的本地文件路径</param>
        /// <returns>true: 返回成功；false: 返回失败</returns>
        internal bool PutFiles(string ftpIp, int ftpPort, string ftpRemotePath, string ftpLocalPath, string systemid, int ifWriteTxtLog, WriteLog writeStrLog, ref string ftpResult, ref string failedFileName)
        {
            string[] filenamelist = new string[] { };  //本地文件名数组
            int fcount = 0;   //文件数
            string[] files = new string[] { };   //本地文件数组
            int hNetput = -1;  //上传文件值
            //string Os = "";   //合法路径
            string names = ""; //本地上传文件名字符串
            string logstr = "";
            //Shell Frm = Transfer.banktrans;
            //上次上传成功的文件个数
            int lastUploadCount = 0;
            //每次打开socket传几个文件
            int uploadSuccessCount = -1;

            string pFtpRemotePath;  //服务器路径
            string pFtpLocalPath;   //本地路径
            string pFtpRemoteFilePathName;
            string pFtpLocalFilePathName;

            this.PFtpIp = ftpIp;
            this.PFtpPort = ftpPort;
            pFtpRemotePath = ftpRemotePath;
            pFtpLocalPath = ftpLocalPath;

            try
            {
                //需判断系统类型
                //Os = JudgeOsFormat(p_FtpRemotePath, p_FtpLocalPath);
                //p_FtpLocalPath = GetString(Os + "#5", "?", "#5");
                //获取本地文件数组
                files = Directory.GetFiles(pFtpLocalPath, "*", SearchOption.AllDirectories);
                //从文件数组中取文件名，组成文件名字符串
                foreach (string namestr in files)
                {
                    int i = namestr.LastIndexOf("\\");
                    string name = namestr.Substring(i + 1, namestr.Length - i - 1);
                    //names += "," + name; //modified by wang on 20120531
                    names += "|||" + name;
                }

                //取得文件名数组
                //filenamelist = names.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); //modified by wang on 20120531
                filenamelist = names.Split(new[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);
                //统计文件数
                fcount = filenamelist.Length;
                //建立连接
                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                //连接失败，返回false
                if (this._hNetOpen != 0)
                {
                    logstr = "Open the ftp port failed,the result:" + this._hNetOpen;
                    if (writeStrLog != null)
                        writeStrLog("7726 " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                    ftpResult = this._hNetOpen.ToString();
                    return false;
                }
                //文件数为0，返回false
                if (fcount <= 0)
                {
                    logstr = "The uploading files count is 0";
                    if (writeStrLog != null)
                        writeStrLog("7727 " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                    return false;
                }

                logstr = "Start uploadind files";
                if (writeStrLog != null)
                    writeStrLog("7728 " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                //if (this._processBar != null)
                //{
                //    this._processBar.Visible = true;
                //    this._processBar.Maximum = fcount;
                //    this._processBar.Minimum = 0;
                //    this._processBar.Value = 0;
                //}

                if (this.BeforeTransfer != null)
                {
                    this.InvokeBeforeTransfer(filenamelist);
                }

                if (pFtpRemotePath.IndexOf('/') != pFtpRemotePath.Length - 1)
                    pFtpRemotePath = pFtpRemotePath + '/';

                //开始上传文件
                for (int i = 0; i < fcount; i++)
                {
                    if ((new FileInfo(files[i]).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }

                    //取文件名
                    string strFileName = filenamelist[i];
                    pFtpLocalFilePathName = files[i];

                    pFtpRemoteFilePathName = pFtpRemotePath + strFileName;

                    logstr = "Uploading the " + i + "file. The uploading file local path is " + pFtpLocalFilePathName + ",and remote path is " + pFtpRemoteFilePathName;
                    if (writeStrLog != null)
                        writeStrLog("7729 " + logstr, this._proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                    if (lastUploadCount == uploadSuccessCount)
                    {
                        FTPclose();
                        this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);
                    }

                    if (this.BeforeUploadLoadOnePage != null)
                    {
                        this.BeforeUploadLoadOnePage.Invoke(strFileName, pFtpLocalPath);
                    }
                    //上传文件
                    //hNetput = FTPsend(﻿p_FtpLocalFilePathName, p_FtpRemoteFilePathName, systemid);
                    hNetput = FTPsend(pFtpLocalFilePathName, pFtpRemoteFilePathName, systemid);

                    //上传失败，返回false
                    if (hNetput != 0)
                    {
                        if (uploadSuccessCount == -1)
                        {
                            //定义每次传几个文件
                            if (lastUploadCount > 2)
                                uploadSuccessCount = lastUploadCount - 2;
                            else
                                uploadSuccessCount = 1;

                            logstr = "Everytime upload file count :" + uploadSuccessCount;
                            if (writeStrLog != null)
                                writeStrLog("7762 " + logstr, this._proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);

                            //初始上传成功文件个数
                            lastUploadCount = 0;

                            //从上传失败文件开始重传
                            i = i - 1;

                            //重新打开
                            FTPclose();
                            this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);

                            //继续上传
                            continue;
                        }

                        //if (this._processBar != null) this._processBar.Visible = false;

                        logstr = "Upload the file \"" + strFileName + "\" failed,the return number is " + hNetput;
                        if (writeStrLog != null)
                            writeStrLog("7730 " + logstr, this._proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);
                        ftpResult = hNetput.ToString();

                        failedFileName = strFileName;

                        return false;
                    }
                    else
                    {
                        lastUploadCount += 1;

                        //if (this._processBar != null)
                        //{
                        //    if (this._processBar.Value < this._processBar.Maximum)
                        //    {
                        //        this._processBar.Value += 1;
                        //    }
                        //}

                        logstr = "Upload the file \"" + strFileName + "\" succees";
                        if (writeStrLog != null)
                            writeStrLog("7731 " + logstr, this._proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, this._proIdAndThreadId);
                    }
                }
            }
            catch (Exception ex)
            {
                logstr = "Error occured when uploading files,the exception is" + ex.Message;
                if (writeStrLog != null)
                    writeStrLog("7732 " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                ftpResult = ex.Message;

                return false;
            }
            finally
            {
                //关闭连接
                FTPclose();
                //if (this._processBar != null) this._processBar.Visible = false;
            }

            //正常返回true
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftpRemotePath"></param>
        /// <param name="ftpLocalPath"></param>
        /// <param name="fileName"></param>
        /// <param name="systemid"></param>
        /// <param name="ifWriteTxtLog"></param>
        /// <param name="writeStrLog"></param>
        /// <returns></returns>
        internal int GetFile(string ftpRemotePath, string ftpLocalPath, string fileName, string systemid, int ifWriteTxtLog, WriteLog writeStrLog)
        {
            int hNetGet = -1;   //FTPrecv返回值

            string logstr = "";


            //根据操作系统类型判断路径格式
            //Os = JudgeOsFormat(p_FtpRemotePath, p_FtpLocalPath);
            //p_FtpRemotePath = GetString("#5" + Os, "#5", "?",false);
            //p_FtpLocalPath = GetString(Os + "#5", "?", "#5");
            string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            try
            {

                //本地目录如不存在，创建本地目录
                if (!Directory.Exists(ftpLocalPath))
                {
                    Directory.CreateDirectory(ftpLocalPath);
                }

                logstr = "Start downloadind files";
                if (writeStrLog != null)
                    writeStrLog("7718 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);


                if (ftpRemotePath.Contains("\\"))
                {
                    if (!ftpRemotePath.EndsWith("\\"))
                    {
                        ftpRemotePath = ftpRemotePath + '\\';
                    }
                }
                else if (!ftpRemotePath.EndsWith("/"))
                {
                    ftpRemotePath = ftpRemotePath + '/';
                }

                if (ftpLocalPath.Contains("\\"))
                {
                    if (!ftpLocalPath.EndsWith("\\"))
                    {
                        ftpLocalPath = ftpLocalPath + '\\';
                    }
                }
                else if (!ftpRemotePath.EndsWith("/"))
                {
                    ftpLocalPath = ftpLocalPath + '/';
                }

                //设置远程文件路径及本地文件路径
                string pFtpRemoteFilePathName = ftpRemotePath + fileName;
                string pFtpLocalFilePathName = ftpLocalPath + fileName;
                //添加tmp文件，下载完成后再改过来
                string pFtpLocalFileTmpPathName = ftpLocalPath + fileName + "~tmp";

                logstr = "Downloading the " + fileName + "file. The downloading file local path is " + pFtpLocalFilePathName + ",and remote path is " + pFtpRemoteFilePathName;
                if (writeStrLog != null)
                    writeStrLog("7719 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);

                if (this.BeforeDownLoadOnePage != null)
                {
                    this.BeforeDownLoadOnePage.Invoke(fileName, ftpLocalPath);
                }

                //下载文件
                hNetGet = FTPrecv(pFtpRemoteFilePathName, pFtpLocalFileTmpPathName, systemid);
                if (hNetGet != 0)
                {
                    if (File.Exists(pFtpLocalFileTmpPathName))
                    {
                        File.Delete(pFtpLocalFileTmpPathName);
                    }
                }
                else
                {

                    if (this.BeforeChangeName != null)
                    {
                        this.BeforeChangeName.Invoke(ref pFtpLocalFileTmpPathName, ref pFtpLocalFilePathName);
                    }
                    //下载成功后把temp修改成要下的文件名
                    File.Move(pFtpLocalFileTmpPathName, pFtpLocalFilePathName);
                    logstr = "Download the file \"" + fileName + "\" succees";
                    if (writeStrLog != null)
                        writeStrLog("7721 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                    if (ifWriteTxtLog == 1)
                        CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);

                    if (this.DownLoadSuccessOnePage != null)
                    {
                        this.DownLoadSuccessOnePage.Invoke(fileName, ftpLocalPath);
                    }
                }

            }
            catch (Exception ex)
            {
                logstr = "Error occured when downloading file,the exception is" + ex.Message;
                if (writeStrLog != null)
                    writeStrLog("7725 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, proIdAndThreadId);

                return hNetGet;
            }

            return hNetGet;
        }

        internal bool FtpOpen(string ftpIp, int ftpPort, int ifWriteTxtLog, WriteLog writeStrLog)
        {
            string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            //建立连接           
            int ftpResult = FTPopen(ftpIp, ftpPort);

            string logstr = "";

            if (ftpResult != 0)
            {
                logstr = "Open the ftp port failed,the result:" + ftpResult;
                if (writeStrLog != null)
                    writeStrLog("7711 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, proIdAndThreadId);

                FTPclose();
                return false;
            }
            else
            {
                logstr = "Open the ftp port succees";
                if (writeStrLog != null)
                    writeStrLog("7712 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);

                return true;
            }



        }


        internal void DelectFtpRemotePath(string ftpRemotePath, string systemid, int ifWriteTxtLog, WriteLog writeStrLog)
        {
            string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            //删除远程目录
            string logstr;
            int hNetDelDir = this.DelDir(ftpRemotePath, systemid);
            //删除远程目录失败，返回false
            if (hNetDelDir != 0)
            {
                logstr = "Deleting the directory failed,the result is " + hNetDelDir;
                if (writeStrLog != null)
                    writeStrLog("7722 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, proIdAndThreadId);
            }
            else
            {
                logstr = "Deleting the directory succees";
                if (writeStrLog != null)
                    writeStrLog("7723 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
            }
        }

        internal void DelectFtpRemotePath(string ip, int port, string ftpRemotePath, string systemid, int ifWriteTxtLog, WriteLog writeStrLog)
        {
            string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            //删除远程目录
            string logstr;
            int hNetDelDir = this.DelDir(ip, port, ftpRemotePath, systemid);
            //删除远程目录失败，返回false
            if (hNetDelDir != 0)
            {
                logstr = "Deleting the directory failed,the result is " + hNetDelDir;
                if (writeStrLog != null)
                    writeStrLog("7722 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "error", logstr, proIdAndThreadId);
            }
            else
            {
                logstr = "Deleting the directory succees";
                if (writeStrLog != null)
                    writeStrLog("7723 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
            }
        }

        private void InvokeBeforeTransfer(string[] fileNames)
        {
            int exFileCount = 0;
            foreach (string fileName in fileNames)
            {
                if ((fileName.ToLower() == "index.xml") || fileName.ToLower().EndsWith(".atn"))
                {
                    exFileCount++;
                }
            }
            this.BeforeTransfer.Invoke(fileNames.Length - exFileCount);
        }

        /// <summary>
        /// 根据路径判断操作系统类型
        /// </summary>
        /// <param name="rpath">目的地路径</param>
        /// <param name="lpath">本地路径</param>
        /// <returns>合法路径</returns>
        private string JudgeOsFormat(string rpath, string lpath)
        {
            string result = "";

            //    '/'unix;
            //    '\'windows
            //Rpath = Rpath + "/";

            if (rpath.IndexOf('/') > 0)
            {
                rpath = rpath + "/";
                //加'/'后可能出现'//',把'//'替换为'/'
                rpath = rpath.Replace("//", "/");
            }

            if (rpath.IndexOf("\\") > 0)
            {
                rpath = rpath + "\\";
                rpath = rpath.Replace("\\\\", "\\");
            }

            if (lpath.IndexOf("\\") > 0)
            {
                lpath = lpath + "\\";
                lpath = lpath.Replace("\\\\", "\\");
            }

            result = rpath + "?" + lpath;
            return result;
        }

        private string GetString(string sourStr, string patternFrom, string patternTo, bool isCase = false)
        {
            string result = "";
            int position = -1;

            if (!isCase)
                position = (sourStr.ToLower()).IndexOf(patternFrom.ToLower());
            else
                position = (sourStr.ToUpper()).IndexOf(patternFrom.ToUpper());

            if (position < 0)
            {
                result = "";
                return result;
            }

            sourStr = sourStr.Substring(position + 1, sourStr.Length - position - patternFrom.Length);

            if (!isCase)
                position = (sourStr.ToLower()).IndexOf(patternTo.ToLower());
            else
                position = (sourStr.ToUpper()).IndexOf(patternTo.ToUpper());

            result = sourStr.Substring(0, position);
            return result;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="ftpRemotePath">ftp服务器文件夹</param>
        /// <returns></returns>
        internal int DelDir(string ftpRemotePath, string systemid)
        {
            int result = -1;

            try
            {
                //强制关闭连接
                FTPclose();

                //打开连接
                this._hNetOpen = FTPopen(this.PFtpIp, this.PFtpPort);

                //打开连接失败，返回-1
                if (this._hNetOpen != 0)
                {
                    result = -1;
                    return result;
                }

                //如要删除目录不为空，删除目录
                if (ftpRemotePath != "")
                {
                    //删除目录
                    result = FTPrmdir(ftpRemotePath, systemid);
                }
            }
            catch
            {
                //MessageBox.Show(GlobalResource.AccessResource.GetInstance().GetValue("140"));
            }
            finally
            {
                //关闭连接
                FTPclose();
            }

            return result;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="ftpRemotePath">ftp服务器文件夹</param>
        /// <returns></returns>
        internal int DelDir(string ip, int port, string ftpRemotePath, string systemid)
        {
            int result = -1;

            try
            {
                //强制关闭连接
                FTPclose();

                //打开连接
                this._hNetOpen = FTPopen(ip, port);

                //打开连接失败，返回-1
                if (this._hNetOpen != 0)
                {
                    result = -1;
                    return result;
                }

                //如要删除目录不为空，删除目录
                if (ftpRemotePath != "")
                {
                    //删除目录
                    result = FTPrmdir(ftpRemotePath, systemid);
                }
            }
            catch
            {
                //MessageBox.Show(GlobalResource.AccessResource.GetInstance().GetValue("140"));
            }
            finally
            {
                //关闭连接
                FTPclose();
            }

            return result;
        }

        internal void SetFtpPara(int id, int flag, int ifWriteTxtLog, WriteLog writeStrToLog)
        {
            string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            int setValueResult = -1;
            string logstr = "";
            switch (id)
            {
                case 12:
                    {
                        if (flag > 12000)
                        {
                            setValueResult = FTPsetflag(id, flag);
                            logstr = "Set ftp parameter number: " + id + ",and value: " + flag + " and result: " + setValueResult;
                            if (writeStrToLog != null)
                                writeStrToLog("7733 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
                        }
                        //else
                        //    SetXmlValue("//UploadTimeOut", id, 0,IfWriteTxtLog,WriteStrToLog);
                        break;
                    }

                case 13:
                    {
                        if (flag > 12000)
                        {
                            setValueResult = FTPsetflag(id, flag);
                            logstr = "Set ftp parameter number: " + id + ",and value: " + flag + " and result: " + setValueResult;
                            if (writeStrToLog != null)
                                writeStrToLog("7734 " + CommonFunc.GetLineNum() + " " + logstr, proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
                        }
                        //else
                        //    SetXmlValue("//DownloadTimeOut", id, 0, IfWriteTxtLog, WriteStrToLog);

                        break;
                    }

                case 1:
                    {
                        if (flag >= 4 && flag <= 64)
                        {
                            setValueResult = FTPsetflag(id, flag * 1024);
                            logstr = "Set ftp parameter number: " + id + ",and value: " + (flag * 1024) + " and result: " + setValueResult;
                            if (writeStrToLog != null)
                                writeStrToLog("7735 " + logstr, proIdAndThreadId);
                            if (ifWriteTxtLog == 1)
                                CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
                        }
                        //else
                        //    SetXmlValue("//FileTransWriteOneSize", id, 1, IfWriteTxtLog, WriteStrToLog);
                        break;
                    }
            }
        }

        /// <summary>
        /// 取配置文件中的值
        /// </summary>
        /// <param name="paraname">配置文件中参数名称</param>
        /// <param name="id">ftp参数号</param>
        /// <param name="defaultvalue">是否是字节数,0:非字节数;1:字节数</param>
        /// <returns></returns>
        internal int SetXmlValue(string paraname, int id, int ifByteNum, int ifWriteTxtLog, WriteLog writeStrToLog)
        {
            string proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            int result = 0;
            int timeout = -1;
            string logstr = "";
            try
            {
                timeout = int.Parse(CommonFunc.GetXmlNodeValue(paraname));

                if (timeout != -1)
                {
                    if (ifByteNum == 0)
                    {
                        result = FTPsetflag(id, timeout);
                        logstr = "Set ftp parameter number: " + id + ",and value: " + timeout + " and result: " + result;
                        if (writeStrToLog != null)
                            writeStrToLog("7738 " + logstr, proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
                    }

                    if (ifByteNum == 1)
                    {
                        result = FTPsetflag(id, timeout * 1024);
                        logstr = "Set ftp parameter number: " + id + ",and value: " + (timeout * 1024) + " and result: " + result;
                        if (writeStrToLog != null)
                            writeStrToLog("7739 " + logstr, proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
                    }

                    if (ifByteNum == 2)
                    {
                        result = FTPsetflag(id, timeout * 1024 * 1024);
                        logstr = "Set ftp parameter number: " + id + ",and value: " + (timeout * 1024 * 1024) + " and result: " + result;
                        if (writeStrToLog != null)
                            writeStrToLog("7740 " + logstr, proIdAndThreadId);
                        if (ifWriteTxtLog == 1)
                            CommonFunc.WriteTxtLog("ProTrans", CommonFunc.GetLineNum().ToString(), "info", logstr, proIdAndThreadId);
                    }
                }
            }
            catch
            {
                return -1;
            }

            return result;
        }

        internal void SetXmlParaValue(int ifWriteTxtLog, WriteLog writeStrToLog)
        {
            this.SetXmlValue("//BufferSize", 1, 1, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//WriteBufToDiscTime", 2, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//ReadDiscToBufTime", 3, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//FileTransWriteOneSize", 4, 1, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//FileTransReadOneSize", 5, 1, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//SocketSendBuffer", 6, 2, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//SocketReceiveBuffer", 7, 2, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//WriteSocketSize", 8, 1, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//ReadSocketSize", 9, 1, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//WriteBufToSocketTime", 10, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//ReadSocketToBufTime", 11, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//UploadTimeOut", 12, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//DownloadTimeOut", 13, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//GetListFromServerTime", 14, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//IfWriteFtpLog", 15, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//IfCompressForTrans", 16, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//IfAsynForTrans", 17, 0, ifWriteTxtLog, writeStrToLog);
            this.SetXmlValue("//ClearDirTimeOut", 18, 0, ifWriteTxtLog, writeStrToLog);
        }
    }
}
