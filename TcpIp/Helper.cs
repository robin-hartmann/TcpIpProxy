using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpIp
{
    public static class Helper
    {
        public static Task<IPAddress> GetHostIpAsync(string hostName)
        {
            return Task.Run(() =>
            {
                IPAddress literalIp;

                if (IPAddress.TryParse(hostName, out literalIp))
                {
                    return literalIp;
                }

                IPAddress ipV6 = null;
                IPHostEntry host = Dns.GetHostEntry(hostName);

                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                    {
                        return ip;
                    }
                    else if (ipV6 == null && ip.AddressFamily.Equals(AddressFamily.InterNetworkV6))
                    {
                        ipV6 = ip;
                    }
                }
                return ipV6;
            });
        }

        public static bool IsAvailablePort(UInt16 port)
        {
            foreach (IPEndPoint endPoint in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
            {
                if (endPoint.Port == port)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
