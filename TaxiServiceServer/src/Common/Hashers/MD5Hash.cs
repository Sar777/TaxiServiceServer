using System.Security.Cryptography;
using System.Text;

namespace TaxiServiceServer.Common.Hashers
{
    public class MD5Hash
    {
        public static string Get(string input)
        {
            var md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sBuilder = new StringBuilder();

            foreach (byte t in data)
                sBuilder.Append(t.ToString("x2"));

            return sBuilder.ToString();
        }
    }
}
