namespace Com.Boc.Icms.DoNetSDK.Bean
{
    class MessageBean
    {

        public string SoketIp { get; set; }

        public string SoketPort { get; set; }

        public string MsSystemId { get; set; }

        public string DataSystemId { get; set; }

        public string Pkuuid { get; set; }

        public bool IncludeContentFile { get; set; }

        public string SavePath { get; set; }

        public string ExtData { get; set; }

        public string Encode { get; set; }

        public string Bankcode { get; set; }

        public string Version { get; set; }

        private string _clientIp;
        public string ClientIp
        {
            get 
            { 
                return this._clientIp; 
            }

            set
            {
                this._clientIp = value;
                string spaceStr = "         ";
                //IP地址不够15位的补齐15位
                if (this._clientIp.Length < 15) this._clientIp = this._clientIp + spaceStr.Substring(0, 15 - this._clientIp.Length);
            }
        }

        public byte ProtocolType { get; set; }

        public string RequestType { get; set; }

        public string OriginalNo { get; set; }

        public string VerifyNo { get; set; }

        public string SerialNo { get; set; }

        public string MsProvince { get; set; }

        public string DataProvince { get; set; }

        public string RecvSoketIp { get; set; }

        public string RecvSoketPort { get; set; }


        #region DMS新增结构

        public string Syscode { get; set; }
        public string Branchcode { get; set; }
        public string Operaterid { get; set; }
        public string Transtype { get; set; }
        public string Transtime { get; set; }
        public string Transid { get; set; }
        public string Clienttype { get; set; }
        public string Imagestoragemech { get; set; }
        public string Originaltext { get; set; }
        public string Ciphertext { get; set; }
       


        #endregion

    }
}
