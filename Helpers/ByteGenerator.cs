using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;


namespace L2.Helpers
{
    public class ByteGenerator
    {
        public static byte[] ConvertToBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        public static string ConvertToBytes(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] ConvertToBytes(char[] text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
