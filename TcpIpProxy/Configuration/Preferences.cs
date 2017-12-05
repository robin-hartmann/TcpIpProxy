using System.Text;

namespace TcpIpProxy.Configuration
{
    internal static class Preferences
    {
        public static readonly int DEF_SEND_TIMEOUT = 5000;
        public static readonly int DEF_RECEIVE_TIMEOUT = 10000;
        public static readonly char DEF_NON_PRINTABLE_REPLACER = '.';
        public static readonly string DEF_HEX_OUTPUT_FORMAT = "<{0}>";
        public static readonly string DEF_HEX_INPUT_FORMAT = "0x{0}";
        public static readonly bool DEF_REPLACE_HEX = true;
        public static readonly Encoding DEF_STRING_ENCODING = Encoding.GetEncoding("Windows-1252");

        static Preferences() {
            SetDefaults();
        }

        public static int SendTimeout
        {
            get;
            set;
        }

        public static int ReceiveTimeout
        {
            get;
            set;
        }

        public static char NonPrintableReplacer
        {
            get;
            set;
        }

        public static string HexInputFormat
        {
            get;
            set;
        }

        public static string HexOutputFormat
        {
            get;
            set;
        }

        public static bool ReplaceHex
        {
            get;
            set;
        }

        public static Encoding ClientStringEncoding
        {
            get;
            set;
        }

        public static Encoding ServerStringEncoding
        {
            get;
            set;
        }
            
        public static void SetDefaults()
        {
            SendTimeout = DEF_SEND_TIMEOUT;
            ReceiveTimeout = DEF_RECEIVE_TIMEOUT;
            NonPrintableReplacer = DEF_NON_PRINTABLE_REPLACER;
            HexInputFormat = DEF_HEX_INPUT_FORMAT;
            HexOutputFormat = DEF_HEX_OUTPUT_FORMAT;
            ReplaceHex = DEF_REPLACE_HEX;
            ClientStringEncoding = DEF_STRING_ENCODING;
            ServerStringEncoding = DEF_STRING_ENCODING;
        }
    }
}
