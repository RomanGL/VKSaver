using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace VKSaver.Core
{
    internal static class Md5Helper
    {
        public static string GetMd5Hash(string data)
        {
            return CryptographicBuffer.EncodeToHexString(HashAlgorithmProvider.OpenAlgorithm("MD5")
                .HashData(CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8)));
        }
    }
}
