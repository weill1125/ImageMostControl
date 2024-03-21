using System.Runtime.InteropServices;
using Com.Boc.Icms.DoNetSDK.Bean;

namespace Com.Boc.Icms.DoNetSDK
{
    [Guid("A5B47451-5791-4B45-B351-49299D0900C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    interface IDonetSdk
    {
        //综合上传，实现内容文件的新增、修改、删除等操作，包括版本管理等
        // ReSharper disable once InconsistentNaming
        ReturnMsg DownloadByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string transtype, string version, string transtime, string transid, string clienttype, string clientip, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string startersyscode,
                                          string pkuuid, string imagelibraryident, bool iscurrentversion, string versionlabel, bool isoriginal, string resolution, bool includecontentfile, string savePath, string zpk = "", string filenames = "");

    }
}
