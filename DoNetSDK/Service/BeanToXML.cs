using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Com.Boc.Icms.DoNetSDK.Bean;

namespace Com.Boc.Icms.DoNetSDK.Service
{
    class BeanToXml
    {
        private string _errString = string.Empty;
        private readonly string _proIdAndThreadId = string.Empty;

        public BeanToXml()
        {
            this._proIdAndThreadId = System.Diagnostics.Process.GetCurrentProcess().Id + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
        }
        //生成XML
        public int CombinXMl(PostInfo batchinfo, string dirPath, int ifWriteTxtLog, WriteLog writeStrToLog, ref string validXmlStr)
        {
            int result = -1;
            string xmlStr = "";
            string logstr = "";

            if (dirPath.EndsWith("\\") == false)
                dirPath = dirPath + "\\";

            logstr = "Begin CombinXml";
            if (writeStrToLog != null)
                writeStrToLog("7691 " + logstr, this._proIdAndThreadId);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            XmlWriter xmlwriter = XmlWriter.Create(dirPath + "index.xml");
            XmlSerializer xmlser = new XmlSerializer(typeof(PostInfo));
            xmlser.Serialize(xmlwriter, batchinfo);
            xmlwriter.Close();

            if (File.Exists(dirPath + "index.xml"))
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(dirPath + "index.xml");
                xmlStr = xmldoc.OuterXml;
            }
            else
            {
                return result;
            }

            logstr = "Create index.xml success";
            if (writeStrToLog != null)
                writeStrToLog("7692 " + logstr, this._proIdAndThreadId);

            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            bool xmlvalid = this.ValidXMl(xmlStr, ifWriteTxtLog, writeStrToLog,ref validXmlStr);
            if (xmlvalid == false)
                result = 1;
            else
                result = 0;

            logstr = "End CombinXml";
            if (writeStrToLog != null)
                writeStrToLog("7693 " + logstr, this._proIdAndThreadId);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            return result;
        }

        private bool ValidXMl(string xmlStr,int ifWriteTxtLog,WriteLog writeStrToLog,ref string validXmlStr)
        {
            bool result = true;
            string logstr = "";

            this._errString = string.Empty;
            StringReader sRead = null;
            XmlReader xmlRead = null;
            XmlSchemaSet schemaSet;

            logstr = "Begin check Xml";
            if (writeStrToLog != null)
                writeStrToLog("7694 " + logstr, this._proIdAndThreadId);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            try
            {
                 schemaSet = new XmlSchemaSet();
                 sRead = new StringReader(xmlStr); 
                 
　　             schemaSet.Add(null, "index.xsd"); 
　　             XmlReaderSettings settings = new XmlReaderSettings(); 
                 settings.ValidationEventHandler += this.ValidationEventCallBack; 
                 settings.ValidationType = ValidationType.Schema; 
                 settings.Schemas = schemaSet; 
                 xmlRead = XmlReader.Create(sRead, settings); 

                 while (xmlRead.Read()) 
                 {
                 }

                 if (this._errString != String.Empty)
                 {
                     logstr = "Check failed,the reason may be " + this._errString;
                     if (writeStrToLog != null)
                         writeStrToLog("7695 " + logstr, this._proIdAndThreadId);
                     if (ifWriteTxtLog == 1)
                         CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                     validXmlStr = this._errString;
                     result = false;
                 }
            }
            catch (XmlException exec)
            {
                logstr = "Exception occured when check xml,and the exception message is " + exec.Message;
                if (writeStrToLog != null)
                    writeStrToLog("7696 " + logstr, this._proIdAndThreadId);
                if (ifWriteTxtLog == 1)
                    CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "error", logstr, this._proIdAndThreadId);

                validXmlStr = exec.Message;
                result = false;
            }
            finally
            {
                if (xmlRead != null)
                    xmlRead.Close();
            }

            logstr = "End check Xml";
            if (writeStrToLog != null)
                writeStrToLog("7697 " + logstr, this._proIdAndThreadId);
            if (ifWriteTxtLog == 1)
                CommonFunc.WriteTxtLog("DonetSDK", CommonFunc.GetLineNum().ToString(), "Info", logstr, this._proIdAndThreadId);

            return result;
        }

        private void ValidationEventCallBack(Object sender, ValidationEventArgs e)
        {
            if (e.Severity != XmlSeverityType.Warning)//区分是警告还是错误 
                this._errString = "Err:" + e.Message;
        }
    }
}
