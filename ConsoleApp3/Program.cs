
using Com.Boc.Icms.DoNetSDK;
using Com.Boc.Icms.DoNetSDK.Service;
using Com.Boc.Icms.DoNetSDK.Bean;
using WinFormTestDll;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApp3
{
    internal class Program
    {
        private static string _securityNo;
        static void Main(string[] args)
        {
            Test();
        }
        public static void Test()
        {

            DonetSdk donetSdk = new DonetSdk();
            string ip = "22.11.36.102";
            string port = "23456";
            string syscode = "COS1";
            string bankcode = "003";
            string branchcode = "00001";
            string operaterid = "1234567";           
            string version = "1.0";       
            string imagestoragemech = "00001";
            string originaltext = GetSecurityNo();
            string ciphertext = Encryption.Encrypt3Des("ABCDEFGHIJKLMNOPQRSTUVWX", originaltext);
            int transTimeOut = 12000;
            int fileTransOneSize = 0;
            string startersyscode = "";            
            string imagelibraryident = "TEST-COS";
            string iscurrentversion = "Y";
            string versionlabel = "";
            string isoriginal = "Y";
            string resolution = "";
            string includecontentfile = "Y";
            string savePath = "";
            string datatypes = "0145"; //
            string pageflag = "";
            string pageuuid = "";
            bool flag = true;
            int isconfig = 0;
            int pageindex = 0;
            int docindex = 0;
            string pkuuid = "";
            string biz = "";
            ReturnMsg resule = null;
            //Console.WriteLine("0001：新增交易信息及影像");
            Console.WriteLine("0002：通过pkuuid调阅影像");
            Console.WriteLine("0003：通过bizId调阅影像");
            Console.WriteLine("0005：根据交易编号删除该交易信息");
            Console.WriteLine("0006：根据pkuuid删除指定版本或最新版本影像");
            Console.WriteLine("0010：通过page维度进行调阅影像");
            Console.WriteLine("0011：通过page维度删除影像");
            Console.WriteLine("0013：通过交易编号查询影像列表");
            Console.WriteLine("0014：判断bizId是否存在");
            Console.WriteLine("0015：判断pkuuid是否存在");
            Console.WriteLine("0018：根据page列表删除元数据和影像文件");
            Console.WriteLine("0019：根据pkuuid列表删除元数据和影像文件");
            Console.WriteLine("0020：通过page列表调阅影像");
            Console.WriteLine("0021：通过bizMetadata列表查询影像列表");
            Console.WriteLine("0022：通过uniqMetadata下载");
            Console.WriteLine("0023：通过pkuuid列表调阅影像");
            while (flag)
            {               
                Console.Write("请输入请求类型编码：");
                string requesttpe = Console.ReadLine().ToString();
                switch (requesttpe)
                {
                    case "0001"://新增交易信息及影像
                        WriteLog("新增交易信息及影像");
                        //resule = donetSdk.AddBizAndImage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, "userdata");
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0002"://通过pkuuid调阅影像
                        WriteLog("通过pkuuid调阅影像");
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        resule = donetSdk.DownloadByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, startersyscode, pkuuid, imagelibraryident, iscurrentversion, versionlabel, isoriginal, resolution, includecontentfile, savePath);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);                      
                        break;
                    case "0003"://通过bizId调阅影像
                        WriteLog("通过bizId调阅影像");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：pk1|pk2|pkN");
                        pkuuid = Console.ReadLine().ToString();
                        resule = donetSdk.DownloadByBizId(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, startersyscode, biz, pkuuid, datatypes, imagelibraryident, isoriginal, resolution, savePath);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);                     
                        break;
                    case "0004"://新增文档信息及影像
                        WriteLog("新增文档信息及影像");
                        //resule = donetSdk.AddDocAndImage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, "userdata");
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0005"://根据交易编号删除该交易信息
                        WriteLog("根据交易编号删除该交易信息");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        resule = donetSdk.DeletedByBizId(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, imagelibraryident);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);                       
                        break;
                    case "0006"://根据pkuuid删除指定版本或最新版本影像
                        WriteLog("根据pkuuid删除指定版本或最新版本影像");
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        resule = donetSdk.DeletedByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, pkuuid, imagelibraryident, iscurrentversion, versionlabel);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0007"://文件传输
                        break;
                    case "0009"://修改文档信息及影像

                        break;
                    case "0010"://通过page维度进行调阅影像
                        WriteLog("通过page维度进行调阅影像");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        Console.Write("请输入docindex：");
                        docindex = Convert.ToInt32(Console.ReadLine());
                        Console.Write("请输入pageflag：");
                        pageflag = Console.ReadLine().ToString();
                        Console.Write("请输入pageuuid：");
                        pageuuid = Console.ReadLine().ToString();
                        Console.Write("请输入pageindex：");
                        pageindex =Convert.ToInt32( Console.ReadLine());
                        Console.Write("请输入iscurrentversion：");
                        iscurrentversion = Console.ReadLine().ToString();
                        Console.Write("请输入versionlabel：");
                        versionlabel = Console.ReadLine().ToString();
                        Console.Write("请输入isoriginal：");
                        isoriginal = Console.ReadLine().ToString();
                        Console.Write("请输入resolution：");
                        resolution = Console.ReadLine().ToString();
                        resule = donetSdk.DownLoadbyPage(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize,biz, pkuuid,imagelibraryident,iscurrentversion,versionlabel,docindex,pageflag, pageuuid, pageindex, isoriginal,resolution);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0011": //通过page维度删除影像  请求类型冲突 文档为0006
                        WriteLog("通过page维度删除影像");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        Console.Write("请输入pageflag：");
                        pageflag = Console.ReadLine().ToString();
                        Console.Write("请输入docindex：");
                        docindex = Convert.ToInt32(Console.ReadLine());
                        Console.Write("请输入pageuuid：");
                        pageuuid = Console.ReadLine().ToString();
                        Console.Write("请输入iscurrentversion：");
                        iscurrentversion = Console.ReadLine().ToString();
                        Console.Write("请输入versionlabel：");
                        versionlabel = Console.ReadLine().ToString();                        
                        resule = donetSdk.DeleteByPage( ip,  port,  syscode,  bankcode,  branchcode,  operaterid,  version,  imagestoragemech,  originaltext,  ciphertext,  transTimeOut,  fileTransOneSize, biz,  pkuuid,  imagelibraryident,  iscurrentversion,  versionlabel,  docindex,  pageflag,  pageuuid);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0012"://新增page信息及影像
                        break;
                    case "0013"://通过交易编号查询影像列表
                        WriteLog("通过交易编号查询影像列表");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuids：pk1|pk2|pkN");
                        pkuuid = Console.ReadLine().ToString();
                        resule = donetSdk.CheckListByBiz(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, datatypes);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);

                        break;
                    case "0014"://判断bizId是否存在
                        WriteLog("判断bizId是否存在");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        resule = donetSdk.CheckBizIsExists(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz,  imagelibraryident);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0015"://判断pkuuid是否存在
                        WriteLog("判断pkuuid是否存在");
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        resule = donetSdk.CheckPkuuidIsExists(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, pkuuid, imagelibraryident);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0016"://修改交易信息
                        break;
                    case "0018"://根据page列表删除元数据和影像文件
                        WriteLog("根据page列表删除元数据和影像文件");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        Console.Write("请输入iscurrentversion：");
                        iscurrentversion = Console.ReadLine().ToString();
                        Console.Write("请输入versionlabel：");
                        versionlabel = Console.ReadLine().ToString();
                        Console.Write("请输入docindex：");
                        docindex = Convert.ToInt32(Console.ReadLine());
                        Console.Write("请输入pageflag：");
                        pageflag = Console.ReadLine().ToString();
                        Console.Write("请输入pageuuid：");
                        pageuuid = Console.ReadLine().ToString();
                        Console.Write("请输入pageindex：");
                        pageindex = Convert.ToInt32(Console.ReadLine());
                       
                        resule = donetSdk.DeleteMateDataByPage( ip,  port,  syscode,  bankcode,  branchcode,  operaterid,  version,  imagestoragemech,  originaltext,  ciphertext,  transTimeOut,  fileTransOneSize, biz,  pkuuid,  imagelibraryident,  iscurrentversion,  versionlabel,  docindex,  pageflag,  pageuuid, pageindex);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0019"://根据pkuuid列表删除元数据和影像文件
                        WriteLog("根据pkuuid列表删除元数据和影像文件");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        Console.Write("请输入iscurrentversion：");
                        iscurrentversion = Console.ReadLine().ToString();
                        Console.Write("请输入versionlabel：");
                        versionlabel = Console.ReadLine().ToString();
                        resule = donetSdk.DeleteMateDataByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, biz, pkuuid, imagelibraryident, iscurrentversion, versionlabel);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);

                        break;
                    case "0020"://通过page列表调阅影像
                        WriteLog("通过page列表调阅影像");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        Console.Write("请输入docindex：");
                        docindex = Convert.ToInt32(Console.ReadLine());
                        Console.Write("请输入pageflag：");
                        pageflag = Console.ReadLine().ToString();
                        Console.Write("请输入pageuuid：");
                        pageuuid = Console.ReadLine().ToString();
                        Console.Write("请输入pageindex：");
                        pageindex = Convert.ToInt32(Console.ReadLine());
                        Console.Write("请输入iscurrentversion：");
                        iscurrentversion = Console.ReadLine().ToString();
                        Console.Write("请输入versionlabel：");
                        versionlabel = Console.ReadLine().ToString();
                        Console.Write("请输入isoriginal：");
                        isoriginal = Console.ReadLine().ToString();
                        Console.Write("请输入includecontentfile：");
                        includecontentfile = Console.ReadLine().ToString();
                        resule = donetSdk.DownLoadListByPage( ip,  port,  syscode,  bankcode,  branchcode,  operaterid,  version,  imagestoragemech,  originaltext,  ciphertext,  transTimeOut,  fileTransOneSize,biz, pkuuid,  imagelibraryident,  iscurrentversion,  versionlabel,  includecontentfile,  docindex,  pageflag, pageuuid, pageindex, isoriginal);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0021"://通过bizMetadata列表查询影像列表
                        WriteLog("通过bizMetadata列表查询影像列表");
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入includecontentfile：");
                        includecontentfile = Console.ReadLine().ToString();
                        resule = donetSdk.DownloadListByBizId( ip,  port,  syscode,  bankcode,  branchcode,  operaterid,  version, imagestoragemech,  originaltext,  ciphertext,  transTimeOut,  fileTransOneSize,biz,  datatypes,  imagelibraryident,  includecontentfile);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);
                        break;
                    case "0022"://通过uniqMetadata下载   //文档未添加请求类型
                        WriteLog("通过uniqMetadata下载");
                        Console.Write("请输入uniqmetadata：");
                        string uniqmetadata = Console.ReadLine().ToString();
                        Console.Write("请输入includecontentfile：");
                        includecontentfile = Console.ReadLine().ToString();
                        resule = donetSdk.DownloadListByUniq(ip, port, syscode, bankcode, branchcode, operaterid, version, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, uniqmetadata, datatypes, imagelibraryident, includecontentfile);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);

                        break;
                    case "0023"://通过pkuuid列表调阅影像
                        WriteLog("通过pkuuid列表调阅影像");
                        Console.Write("请输入includecontentfile：");
                        includecontentfile = Console.ReadLine().ToString();
                        Console.Write("请输入交易编号：");
                        biz = Console.ReadLine().ToString();
                        Console.Write("请输入pkuuid：");
                        pkuuid = Console.ReadLine().ToString();
                        Console.Write("请输入iscurrentversion：");
                        iscurrentversion = Console.ReadLine().ToString();
                        Console.Write("请输入versionlabel：");
                        versionlabel = Console.ReadLine().ToString();
                        Console.Write("请输入isoriginal：");
                        isoriginal = Console.ReadLine().ToString();

                        resule = donetSdk.DownloadListByPkuuid( ip,  port,  syscode,  bankcode,  branchcode,  operaterid,  version,  imagestoragemech,  originaltext,  ciphertext,  transTimeOut,  fileTransOneSize,datatypes,  imagelibraryident,  includecontentfile,  biz,  pkuuid,  iscurrentversion,  versionlabel,  isoriginal);
                        WriteLog("Return code: " + resule.ReturnCode + "; Return message:" + resule.Message + "; Return data:" + resule.ResultData);

                        break;
                    default:
                        Console.WriteLine("没有该请求类型");
                        break;
                }

                Console.WriteLine("End");
                Console.WriteLine("是否继续：是（1）否（0）？");
                isconfig = Convert.ToInt32(Console.ReadLine());
                if (isconfig == 1)
                    flag = true;
                else
                    flag = false;
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
    }
}