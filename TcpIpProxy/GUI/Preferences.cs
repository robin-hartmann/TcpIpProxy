using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpIpProxy.GUI
{
    public static class Preferences
    {
        static Preferences()
        {
            ResetDefaults();
        }

        public static int ReceiveTimeout
        {
            set;
            get;
        }

        public static int SendTimeout
        {
            get;
            set;
        }

        public static void ResetDefaults()
        {
            ReceiveTimeout = 15000;
            SendTimeout = 30000;
        }
    }
}
