using AntDesign;
using Com.Boc.Icms.DoNetSDK;
using Com.Boc.Icms.DoNetSDK.Bean;
using Com.Boc.Icms.MetadataEdit;
using System.Net.Sockets;
using WinFormTestDll;

namespace DMSSocketApp.Shared
{
    public partial class MainLayout
    {
        public string CheckSocket { get; set; } = "接口列表";
        public string RequestType { get; set; }
        public string ip { get; set; } = "22.11.36.102";
        public string port { get; set; } = "23456";
        public string syscode { get; set; } = "COS1";
        public string bankcode { get; set; } = "003";
        public string branchcode { get; set; } = "00001";
        public string operaterid { get; set; } = "1234567";
        public string version { get; set; } = "1.0";
        public string imagestoragemech { get; set; } = "00001";
        public string imagelibraryident { get; set; } = "TEST-COS";
        public string iscurrentversion { get; set; } = "Y";
        public string versionlabel { get; set; } = "1.0";
        public string biz { get; set; } = "";
        public string pkuuid { get; set; } = "";
        public string uniqmetadata { get; set; } = "";
        public string isoriginal { get; set; } = "Y";
        public string includecontentfile { get; set; } = "Y";
        public string datatypes { get; set; } = "";
        public string pageflag { get; set; } = "F";
        public string pageuuid { get; set; } = "";
        public int pageindex { get; set; }
        public int docindex { get; set; }

        public string IsShow { get; set; } = "none";
        public string iscurrentversionVisibility { get; set; } = "none";
        public string versionlabelVisibility { get; set; } = "none";
        public string bizVisibility { get; set; } = "none";
        public string pkuuidVisibility { get; set; } = "none";
        public string uniqmetadataVisibility { get; set; } = "none";
        public string isoriginalVisibility { get; set; } = "none";
        public string includecontentfileVisibility { get; set; } = "none";
        public string datatypesVisibility { get; set; } = "none";
        public string pageflagVisibility { get; set; } = "none";
        public string pageuuidVisibility { get; set; } = "none";
        public string pageindexVisibility { get; set; } = "none";
        public string docindexVisibility { get; set; } = "none";

        private static string _securityNo;

        private MetadataEdit commonControlTemplateWidget1 = new MetadataEdit();


        protected override async Task OnInitializedAsync()
        {
            string oriIndex = File.ReadAllText("./Index.xml");
            this.commonControlTemplateWidget1.InitAllXml(oriIndex);
        }

        public async void TestBtn()
        {           
            DonetSdk donetSdk = new DonetSdk(); 
            ReturnMsg resule = null;
            string originaltext = GetSecurityNo();
            string ciphertext = Encryption.Encrypt3Des("ABCDEFGHIJKLMNOPQRSTUVWX", originaltext);
            int transTimeOut = 12000;
            int fileTransOneSize = 0;
            string startersyscode = "";
            string resolution = "";
            string savePath = "";

            switch (RequestType)
            {
                case "0001"://新增交易信息及影像
                    WriteLog("新增交易信息及影像");
                    var userdata = this.commonControlTemplateWidget1.GetBatchBusinessXml(false);
                    resule = donetSdk.AddBizAndImage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, userdata["20240103"]);
                    break;
                case "0002"://通过pkuuid调阅影像
                    WriteLog("通过pkuuid调阅影像");
                    resule = donetSdk.DownloadByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, startersyscode, pkuuid, imagelibraryident, iscurrentversion, versionlabel, isoriginal, resolution, includecontentfile, savePath);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0003"://通过bizId调阅影像
                    WriteLog("通过bizId调阅影像");
                    resule = donetSdk.DownloadByBizId(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, startersyscode, biz, pkuuid, datatypes, imagelibraryident, isoriginal, resolution, savePath);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0004"://新增文档信息及影像
                    WriteLog("新增文档信息及影像");
                    //resule = donetSdk.AddDocAndImage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, "userdata");
                    break;
                case "0005"://根据交易编号删除该交易信息
                    WriteLog("根据交易编号删除该交易信息");
                    resule = donetSdk.DeletedByBizId(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, imagelibraryident);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0006"://根据pkuuid删除指定版本或最新版本影像
                    WriteLog("根据pkuuid删除指定版本或最新版本影像");
                    resule = donetSdk.DeletedByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, pkuuid, imagelibraryident, iscurrentversion, versionlabel);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0007"://文件传输
                    break;
                case "0009"://修改文档信息及影像

                    break;
                case "0010"://通过page维度进行调阅影像
                    WriteLog("通过page维度进行调阅影像");
                    resule = donetSdk.DownLoadbyPage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, iscurrentversion, versionlabel, docindex, pageflag, pageuuid, pageindex, isoriginal, resolution);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0011": //通过page维度删除影像  请求类型冲突 文档为0006
                    WriteLog("通过page维度删除影像");
                    resule = donetSdk.DeleteByPage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, iscurrentversion, versionlabel, docindex, pageflag, pageuuid);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0012"://新增page信息及影像
                    break;
                case "0013"://通过交易编号查询影像列表
                    WriteLog("通过交易编号查询影像列表");
                    resule = donetSdk.CheckListByBiz(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, datatypes);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0014"://判断bizId是否存在
                    WriteLog("判断bizId是否存在");
                    resule = donetSdk.CheckBizIsExists(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, imagelibraryident);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0015"://判断pkuuid是否存在
                    WriteLog("判断pkuuid是否存在");
                    resule = donetSdk.CheckPkuuidIsExists(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, pkuuid, imagelibraryident);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0016"://修改交易信息
                    break;
                case "0018"://根据page列表删除元数据和影像文件
                    WriteLog("根据page列表删除元数据和影像文件");
                    resule = donetSdk.DeleteMateDataByPage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, iscurrentversion, versionlabel, docindex, pageflag, pageuuid, pageindex);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0019"://根据pkuuid列表删除元数据和影像文件
                    WriteLog("根据pkuuid列表删除元数据和影像文件");
                    resule = donetSdk.DeleteMateDataByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, iscurrentversion, versionlabel);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0020"://通过page列表调阅影像
                    WriteLog("通过page列表调阅影像");
                    resule = donetSdk.DownLoadListByPage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, iscurrentversion, versionlabel, includecontentfile, docindex, pageflag, pageuuid, pageindex, isoriginal);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0021"://通过bizMetadata列表查询影像列表
                    WriteLog("通过bizMetadata列表查询影像列表");
                    resule = donetSdk.DownloadListByBizId(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, datatypes, imagelibraryident, includecontentfile);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0022"://通过uniqMetadata下载   //文档未添加请求类型
                    WriteLog("通过uniqMetadata下载");
                    resule = donetSdk.DownloadListByUniq(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, uniqmetadata, datatypes, imagelibraryident, includecontentfile);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message);
                    break;
                case "0023"://通过pkuuid列表调阅影像
                    WriteLog("通过pkuuid列表调阅影像");
                    resule = donetSdk.DownloadListByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, datatypes, imagelibraryident, includecontentfile, biz, pkuuid, iscurrentversion, versionlabel, isoriginal);
                    await ShowMessage(resule.ReturnCode + "\n" + resule.Message); 
                    break;
                default:
                    await ShowMessage("没有该请求类型");                   
                    break;
            }


        }


        public void ListVisible()
        {
            ListVisible(CheckSocket);
            IsShow = "unset";
        }


        public void ListVisible(string socket)
        {
            switch(socket)
            {
                case "通过pkuuid调阅影像":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "none";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "block";
                    includecontentfileVisibility = "block";
                    datatypesVisibility = "none";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none"; 
                    uniqmetadataVisibility = "none";
                    break;
                case "通过bizId调阅影像":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "block";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "block";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "根据交易编号删除该交易信息":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "block";
                    pkuuidVisibility = "none";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "根据pkuuid删除指定版本或最新版本影像":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "none";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "通过page维度进行调阅影像":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "block";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "block";
                    pageflagVisibility = "block";
                    pageuuidVisibility = "block";
                    pageindexVisibility = "block";
                    docindexVisibility = "block";
                    uniqmetadataVisibility = "none";
                    break;
                case "通过page维度删除影像":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "block";
                    pageuuidVisibility = "block";
                    pageindexVisibility = "none";
                    docindexVisibility = "block";
                    uniqmetadataVisibility = "none";
                    break;
                case "通过交易编号查询影像列表":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "block";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "判断bizId是否存在":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "block";
                    pkuuidVisibility = "none";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "判断pkuuid是否存在":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "none";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "根据page列表删除元数据和影像文件":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "block";
                    pageuuidVisibility = "block";
                    pageindexVisibility = "block";
                    docindexVisibility = "block";
                    uniqmetadataVisibility = "none";
                    break;
                case "根据pkuuid列表删除元数据和影像文件":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "none";
                    datatypesVisibility = "none";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "通过page列表调阅影像":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "block";
                    includecontentfileVisibility = "block";
                    datatypesVisibility = "none";
                    pageflagVisibility = "block";
                    pageuuidVisibility = "block";
                    pageindexVisibility = "block";
                    docindexVisibility = "block";
                    uniqmetadataVisibility = "none";
                    break;
                case "通过bizMetadata列表查询影像列表":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "block";
                    pkuuidVisibility = "none";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "block";
                    datatypesVisibility = "block";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;
                case "通过uniqMetadata下载":
                    iscurrentversionVisibility = "none";
                    versionlabelVisibility = "none";
                    bizVisibility = "none";
                    pkuuidVisibility = "none";
                    uniqmetadataVisibility = "block";
                    isoriginalVisibility = "none";
                    includecontentfileVisibility = "block";
                    datatypesVisibility = "block";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    break;
                case "通过pkuuid列表调阅影像":
                    iscurrentversionVisibility = "block";
                    versionlabelVisibility = "block";
                    bizVisibility = "block";
                    pkuuidVisibility = "block";
                    isoriginalVisibility = "block";
                    includecontentfileVisibility = "block";
                    datatypesVisibility = "block";
                    pageflagVisibility = "none";
                    pageuuidVisibility = "none";
                    pageindexVisibility = "none";
                    docindexVisibility = "none";
                    uniqmetadataVisibility = "none";
                    break;

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

        public static string GetSecurityNo()
        {
            try
            {
                _securityNo = "";
                Random getRandom = new Random();
                for (int i = 0; i < 32; i++) _securityNo += getRandom.Next(0, 10).ToString();
            }
            catch
            {
                _securityNo = "11111111111111111111111111111111111111";
                return _securityNo;
            }

            return _securityNo;
        }


        public async Task<ConfirmResult> ShowMessage(string content)
        {
            ConfirmResult isTrue = await _confirmService.Show(content, "返回信息", ConfirmButtons.OK, ConfirmIcon.Info);
            return isTrue;
        }
    }
}
