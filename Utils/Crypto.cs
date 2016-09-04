using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace O2.Utils
{
    public static class Crypto
    {
        public enum TYPE
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        public static string MD5(string input)
        {
            // calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(input));

            // convert byte array on a hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }

        public static string SHA1(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA1Managed hashString = new SHA1Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);

            return hex;
        }

        public static string SHA256(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA256Managed hashString = new SHA256Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);

            return hex;
        }

        public static string SHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            SHA512Managed hashString = new SHA512Managed();
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                hex += String.Format("{0:x2}", x);

            return hex;
        }
    }
}
