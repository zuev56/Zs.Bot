using System.Security.Cryptography;
using System.Text;

namespace Zs.Common.Extensions
{
    public static class MD5Extension
    {
        public static string GetMd5Hash(this MD5 md5Hash, string input)
        {
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }
    }
}
