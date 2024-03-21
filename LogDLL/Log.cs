using System;
using System.Text;
using System.IO;
using System.Globalization;

namespace Com.Boc.Icms.LogDLL
{
    /// <summary>
    /// 写入Txt日志文本
    /// </summary>
    public class ULogger
    {
        public const int LvlCriticalerror = 900;
        public const int LvlError = 800;
        public const int LvlWarnning = 700;
        public const int LvlCoreinfo = 500;
        public const int LvlCommon = 400;
        public const int LvlCommondetail = 300;
        public const int LvlMostdetail = 200;
        public const int LvlDebug = 100;

        //  
        public int NLogFileLvl;
        public FileStream FsLogFile;
        StreamWriter _sw;
        public string SLogDir = "c:\\oftpd";

        string ErrLvlToStr(int nErrlvl)
        {
            switch (nErrlvl)
            {
                case LvlCriticalerror:
                    return "致命错误";
                case LvlError:
                    return "错误信息";
                case LvlWarnning:
                    return "警告信息";
                case LvlCoreinfo:
                    return "核心信息";
                case LvlCommon:
                    return "提示信息";
                case LvlCommondetail:
                    return "详细信息";
                case LvlMostdetail:
                    return "详尽信息";
                case LvlDebug:
                    return "调试信息";
                default:
                    return "未知类型";
            }
        }

        void SetLogFileLevel(int nErrlvl)   //LVL_COMMON
        {
            this.NLogFileLvl = nErrlvl;
        }

        public void StartLogging(string sInLogDir, string sFilePre, int nErrlvl)  //=LVL_COMMON
        {
            this.NLogFileLvl = nErrlvl;
            this.SLogDir = sInLogDir + sFilePre;
            string sFilename = this.SLogDir + DateTime.Now.ToString("yyyyMMdd") + ".log";
            this.FsLogFile = new FileStream(sFilename, FileMode.Append);
            this._sw = new StreamWriter(this.FsLogFile);
        }

        public void EndLogFile()
        {
            this._sw.Close();
            this.FsLogFile.Close();
        }

        public void LogFile(string filePath, string fileName, int nErrlvl, string appendlogStr)
        {
            this.NLogFileLvl = nErrlvl;
            // sLogDir = sInLogDir + sFilePre;
            string sFilename = filePath + fileName + DateTime.Now.ToString("yyyyMMdd") + ".log";
            this.FsLogFile = new FileStream(sFilename, FileMode.Append);
            this._sw = new StreamWriter(this.FsLogFile);
            this._sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + this.ErrLvlToStr(nErrlvl));
            this._sw.WriteLine(appendlogStr);
            this._sw.Close();
            this.FsLogFile.Close();
        }

        public void LogFile(int nErrlvl, string s)
        {
            this._sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + this.ErrLvlToStr(nErrlvl));
            this._sw.WriteLine(s);
        }

        public string HexToStr(string mHex) // 返回十六进制代表的字符串 
        {
            mHex = mHex.Replace(" ", "");
            if (mHex.Length <= 0) return "";
            byte[] vBytes = new byte[mHex.Length / 2];
            for (int i = 0; i < mHex.Length; i += 2)
                if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2])) vBytes[i / 2] = 0;
            return Encoding.Default.GetString(vBytes);
        } /* HexToStr */

        public void DeleteLogFile(string filePath, string fileName)
        {
            string tmpFilename = filePath + fileName;
            if (File.Exists(tmpFilename))
            {
                File.Delete(tmpFilename);
            }
        }
    }
}
