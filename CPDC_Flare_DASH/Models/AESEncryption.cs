using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text; 

namespace CPDC_Flare_DASH.Models
{
    class AESEncryption
    {
        //AES雙向加密
        public static string AESEncryptBase64(string Message, string Key)
        {
            string Encrypt = "";

            try
            {
                byte[] MessageArray = Encoding.UTF8.GetBytes(Message);
                byte[] key = Encoding.UTF8.GetBytes(Key);
                byte[] iv = Encoding.UTF8.GetBytes(Key);

                SHA256CryptoServiceProvider SHA256 = new SHA256CryptoServiceProvider();
                MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
                key = SHA256.ComputeHash(key);
                iv = MD5.ComputeHash(iv);

                AesCryptoServiceProvider AESalg = new AesCryptoServiceProvider();
                ICryptoTransform AES = AESalg.CreateEncryptor(key, iv);
                Encrypt = Convert.ToBase64String(AES.TransformFinalBlock(MessageArray, 0, MessageArray.Length));
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 99);
            }

            return Encrypt;
        }

        public static string AESDecryptBase64(string Message, string Key)
        {
            string Decrypt = "";

            try
            {
                byte[] MessageArray = Convert.FromBase64String(Message);
                byte[] key = Encoding.UTF8.GetBytes(Key);
                byte[] iv = Encoding.UTF8.GetBytes(Key);

                SHA256CryptoServiceProvider SHA256 = new SHA256CryptoServiceProvider();
                MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
                key = SHA256.ComputeHash(key);
                iv = MD5.ComputeHash(iv);

                AesCryptoServiceProvider AESalg = new AesCryptoServiceProvider();
                ICryptoTransform AES = AESalg.CreateDecryptor(key, iv);
                Decrypt = Encoding.UTF8.GetString(AES.TransformFinalBlock(MessageArray, 0, MessageArray.Length));
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 99);
            }

            return Decrypt;
        }
    }
}
