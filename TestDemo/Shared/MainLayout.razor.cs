using Com.Boc.Icms.DoNetSDK;
using Com.Boc.Icms.DoNetSDK.Bean;
using Com.Boc.Icms.DoNetSDK.Service;
using WinFormTestDll;

namespace TestDemo.Shared
{
    public partial class MainLayout
    {
        private string _securityNo;
        public void Request_Click()
        {
            DonetSdk donetSdk = new DonetSdk();
            string ip = "22.11.36.102";
            string port = "23456";
            string syscode = "TEST";
            string bankcode = "003";
            string branchcode = "00001";
            string operaterid = "123456";
            string transtype = "0002";
            string version = "1.0";
            string transtime = DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss zzz").Replace(" ", "");
            int index = transtime.LastIndexOf(':');
            transtime = transtime.Remove(index, 1);
            string transid = Guid.NewGuid().ToString().Replace("-", "").Trim();
            string clienttype = "01";
            string clientip = CommonFunc.LocalIp(); ;
            string imagestoragemech = "00001";
            string originaltext = GetSecurityNo();
            string ciphertext = Encryption.Encrypt3Des("ABCDEFGHIJKLMNOPQRSTUVWX", originaltext);
            int transTimeOut = 12000;
            int fileTransOneSize = 0;
            string startersyscode = "";
            string pkuuid = "";
            string imagelibraryident = "";
            bool iscurrentversion = true;
            string versionlabel = "";
            bool isoriginal = true;
            string resolution = "";
            bool includecontentfile = true;
            string savePath = "";
            ReturnMsg rermsg = donetSdk.DownloadByPkuuid(ip, port, syscode, bankcode, branchcode, operaterid, transtype, version, transtime, transid, clienttype, clientip, imagestoragemech, originaltext, ciphertext, transTimeOut, fileTransOneSize, startersyscode, pkuuid, imagelibraryident, iscurrentversion, versionlabel, isoriginal, resolution, includecontentfile, savePath);

        }
         public string GetSecurityNo()
        {
            try
            {
                this._securityNo = "";
                Random getRandom = new Random();
                for (int i = 0; i < 32; i++) this._securityNo += getRandom.Next(0, 10).ToString();
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
