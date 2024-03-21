using System.Runtime.InteropServices;
using Com.Boc.Icms.DoNetSDK.Bean;

namespace Com.Boc.Icms.DoNetSDK
{
    [Guid("A5B47451-5791-4B45-B351-49299D0900C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    interface IDonetSdk
    {

        ReturnMsg AddBizAndImage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string userData);

        ReturnMsg DownloadByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string startersyscode,                                         string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel, string isoriginal, string resolution, string includecontentfile, string savePath, string zpk = "", string filenames = "");

        ReturnMsg DownloadByBizId(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string startersyscode,
                                          string biz, string pkuuids, string dataTypes, string imagelibraryident, string isoriginal, string resolution, string savePath, string zpk = "");
        ReturnMsg AddDocAndImage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize, string userData);

        ReturnMsg DeletedByBizId(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string imagelibraryident);
        ReturnMsg DeletedByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel);
        ReturnMsg DownLoadbyPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                        string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel, int docindex, string pageflag, string pageuuid, int pageindex, string isoriginal, string resolution);
        ReturnMsg DeleteByPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel, int docindex, string pageflag, string pageuuid);
        ReturnMsg CheckListByBiz(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuids, string imagelibraryident, string datatypes);
        ReturnMsg CheckBizIsExists(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string imagelibraryident);
        ReturnMsg DeleteMateDataByPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel, int docindex, string pageflag, string pageuuid, int pageindex);
        ReturnMsg DeleteMateDataByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                               string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel);
        ReturnMsg DownLoadListByPage(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string pkuuid, string imagelibraryident, string iscurrentversion, string versionlabel, string includecontentfile, int docindex, string pageflag, string pageuuid, int pageindex, string isoriginal);
        ReturnMsg DownloadListByBizId(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                          string biz, string datatypes, string imagelibraryident, string includecontentfile);
        ReturnMsg DownloadListByUniq(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                         string uniqmetadata, string datatypes, string imagelibraryident, string includecontentfile);
        ReturnMsg DownloadListByPkuuid(string ip, string port, string syscode, string bankcode, string branchcode, string operaterid, string version, string imagestoragemech, string originaltext, string ciphertext, int transTimeOut, int fileTransOneSize,
                                              string datatypes, string imagelibraryident, string includecontentfile, string biz, string pkuuid, string iscurrentversion, string versionlabel, string isoriginal);
    }
}
