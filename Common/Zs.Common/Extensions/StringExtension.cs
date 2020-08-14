using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Zs.Common.Extensions
{
    public static class StringExtension
    {
        public static string GetMD5Hash(this string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            using var md5Hash = MD5.Create();
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }

        public static IEnumerable<string> SplitToParts(this string value, int partLength)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < value.Length; i += partLength)
                yield return value.Substring(i, Math.Min(partLength, value.Length - i));
        }
    }
}
