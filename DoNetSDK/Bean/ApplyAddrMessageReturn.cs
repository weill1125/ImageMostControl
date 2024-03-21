namespace Com.Boc.Icms.DoNetSDK.Bean
{
    class ApplyAddrMessageReturn
    {
        private string _ftpIp;
        private string _ftpPath;
        private int _ftpPort;
        private string _socketIp;
        private int _socketPort;
        private string _errorMessage;
        private string _bftpIp;
        private string _bftpPath;
        private int _bftpPort;
        private string _bsocketIp;
        private int _bsocketPort;

        public string FtpIp
        {
            get { return this._ftpIp; }
            set {
                this._ftpIp = value; }
        }

        public string FtpPath
        {
            get { return this._ftpPath; }
            set {
                this._ftpPath = value; }
        }

        public int FtpPort
        {
            get { return this._ftpPort; }
            set {
                this._ftpPort = value; }
        }

        public string SocketIp
        {
            get { return this._socketIp; }
            set {
                this._socketIp = value; }
        }

        public int SocketPort
        {
            get { return this._socketPort; }
            set {
                this._socketPort = value; }
        }

        public string ErrorMessage
        {
            get { return this._errorMessage; }
            set {
                this._errorMessage = value; }
        }

        public string BFtpIp
        {
            get { return this._bftpIp; }
            set
            {
                this._bftpIp = value;
            }
        }

        public string BFtpPath
        {
            get { return this._bftpPath; }
            set
            {
                this._bftpPath = value;
            }
        }

        public int BFtpPort
        {
            get { return this._bftpPort; }
            set
            {
                this._bftpPort = value;
            }
        }

        public string BSocketIp
        {
            get { return this._bsocketIp; }
            set
            {
                this._bsocketIp = value;
            }
        }

        public int BSocketPort
        {
            get { return this._bsocketPort; }
            set
            {
                this._bsocketPort = value;
            }
        }

        #region DMS
        private string _transtype;
        public string Transtype
        {
            get { return this._transtype; }
            set
            {
                this._transtype = value;
            }
        }

        private string _version;
        public string Version
        {
            get { return this._version; }
            set
            {
                this._version = value;
            }
        }

        private string _transid;
        public string Transid
        {
            get { return this._transid; }
            set
            {
                this._transid = value;
            }
        }

        private string _trantime;
        public string Trantime
        {
            get { return this._trantime; }
            set
            {
                this._trantime = value;
            }
        }

        private string _retcode;
        public string Retcode
        {
            get { return this._retcode; }
            set
            {
                this._retcode = value;
            }
        }

        private string _retmessage;
        public string Retmessage
        {
            get { return this._retmessage; }
            set
            {
                this._retmessage = value;
            }
        }

        private string _messageaddress;
        public string Messageaddress
        {
            get { return this._messageaddress; }
            set
            {
                this._messageaddress = value;
            }
        }

        private string _messageport;
        public string Messageport
        {
            get { return this._messageport; }
            set
            {
                this._messageport = value;
            }
        }

        private string _fileaddress;
        public string Fileaddress
        {
            get { return this._fileaddress; }
            set
            {
                this._fileaddress = value;
            }
        }

        private int _fileport;
        public int Fileport
        {
            get { return this._fileport; }
            set
            {
                this._fileport = value;
            }
        }

        private string _filepath;
        public string Filepath
        {
            get { return this._filepath; }
            set
            {
                this._filepath = value;
            }
        }


        #endregion
    }
}
