using System;
using System.Security.Cryptography;
using System.Text;

namespace WinFormTestDll
{
    class Encryption
    {
        /// <summary>
        /// DESede算法生成密文
        /// </summary>
        /// <param name="key">DES密钥</param>
        /// <param name="securityNo">传入明文</param>
        /// <returns>加密后密文</returns>
        internal static string Encrypt3Des(String key, string securityNo)
        {
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.Key = Encoding.ASCII.GetBytes(key);
            des.Mode = CipherMode.ECB;
            ICryptoTransform desEncrypt = des.CreateEncryptor();
            byte[] buffer = Encoding.UTF8.GetBytes(securityNo);
            return Hex2Byte(desEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        private static string Hex2Byte(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty).ToLower();
        }
    }
}
