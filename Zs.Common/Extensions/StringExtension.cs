using System.Security.Cryptography;
using System.Text;

namespace Zs.Common.Extensions
{
    public static class StringExtension
    {
        public static string GetMD5Hash(this string value)
        {
            using var md5Hash = MD5.Create();
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }
    }
}
