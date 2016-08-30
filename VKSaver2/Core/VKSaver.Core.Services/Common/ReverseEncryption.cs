using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Common
{
    internal static class ReverseEncryption
    {
        public static void Reverse(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
                buffer[i] = ReverseByte(buffer[i]);
        }

        private static byte ReverseByte(byte originalByte)
        {
            int result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = result << 1;
                result += originalByte & 1;
                originalByte = (byte)(originalByte >> 1);
            }

            return (byte)result;
        }
    }
}
