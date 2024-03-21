using System.Runtime.InteropServices;

namespace Com.Boc.Icms.DoNetSDK.Bean
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ReturnMsg
    {
        private string _returnCode;
        private string _message;
        private string _resultData;

        public string ReturnCode
        {
            get { return this._returnCode; }
            set {
                this._returnCode = value; }
        }

        public string Message
        {
            get { return this._message; }
            set {
                this._message = value; }
        }

        public string ResultData
        {
            get { return this._resultData; }
            set {
                this._resultData = value; }
        }
    }
}
