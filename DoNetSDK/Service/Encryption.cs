using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Com.Boc.Icms.DoNetSDK.Service
{
    class Encryption
    {
        /// <summary>  
        /// 对文件内容进行aes加密(支持中文)  
        /// </summary>  
        /// <param name="sourceFile">待加密的文件绝对路径</param>  
        /// <param name="destFile">加密后的文件保存的绝对路径</param>  
        /// <param name="sKey">加密密钥16位</param>  
        public static void EncryptFileByUnicode(string sourceFile, string destFile, string sKey)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            string destFileDirectory = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(destFileDirectory))
                Directory.CreateDirectory(destFileDirectory);
            //一次读取大小
            int maxLength = 20480;
            byte[] buffer = new byte[maxLength];
            //读取位置
            int length = 0;

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = Encoding.UTF8.GetBytes(sKey);
            rDel.Mode = CipherMode.ECB;
            try
            {
                using (FileStream fin = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                using (FileStream fout = new FileStream(destFile, FileMode.Create, FileAccess.Write))
                using (CryptoStream cs = new CryptoStream(fout, rDel.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    while ((length = fin.Read(buffer, 0, maxLength)) > 0)
                        cs.Write(buffer, 0, (int)length);
                 }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (NotSupportedException e)
            {
                throw new NotSupportedException(e.Message);
            }
            catch (CryptographicException e)
            {
                throw new CryptographicException(e.Message);
            }
            catch (IOException e)
            {
                throw new IOException(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>  
        /// 对文件内容进行aes解密(支持中文)  
        /// </summary>  
        /// <param name="sourceFile">待解密的文件绝对路径</param>  
        /// <param name="destFile">解密后的文件保存的绝对路径</param>  
        /// <param name="sKey">解密密钥24位</param>  
        public static void DecryptFileByUnicode(string sourceFile, string destFile, string sKey)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);

            string destFileDirectory = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(destFileDirectory))
                Directory.CreateDirectory(destFileDirectory);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = Encoding.UTF8.GetBytes(sKey);
            rDel.Mode = CipherMode.ECB;

            //一次读取1M
            int maxLength = 20480;
            byte[] buffer = new byte[maxLength];
            //读取位置
            int length = 0;
            try
            {
                using (FileStream fin = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                using (FileStream fout = new FileStream(destFile, FileMode.Create, FileAccess.Write))
                using (CryptoStream cs = new CryptoStream(fout, rDel.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    while ((length = fin.Read(buffer, 0, maxLength)) > 0)
                        cs.Write(buffer, 0, (int)length);
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException(e.Message);
            }
            catch (NotSupportedException e)
            {
                throw new NotSupportedException(e.Message);
            }
            catch (CryptographicException e)
            {
                throw new CryptographicException(e.Message);
            }
            catch (IOException e)
            {
                throw new IOException(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static string GetAesKey()
        {
            string key;
            try
            {
                string[] aryChar = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
                key = "";
                Random getRandom = new Random();
                for (int i = 0; i < 16; i++)
                    key += aryChar[getRandom.Next(aryChar.Length)];
            }
            catch
            {
                key = "1111111111111111";
                return key;
            }

            return key;
        }

        public static string RsaEncrypt(string modulus, string content)
        {
            try
            {
                string publickey = @"<RSAKeyValue><Modulus>" + modulus + "</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                byte[] cipherbytes;
                rsa.FromXmlString(publickey);
                //rsa.ImportParameters(param);
                cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);

                return Convert.ToBase64String(cipherbytes);
            }
            catch (CryptographicException e)
            {
                throw new CryptographicException(e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}