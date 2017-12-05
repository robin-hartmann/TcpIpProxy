using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TcpIpProxy.Configuration;

namespace TcpIpProxy.Helper
{
    public static class Utilities
    {
        public static string ConvertAllHex(string value, Encoding targetEncoding)
        {
            string hexValuePattern = "(([0-9a-fA-F]{2}){1,4})";
            string hexFormatPattern = String.Format(Preferences.HexInputFormat, hexValuePattern);

            return Regex.Replace(value, hexFormatPattern, match =>
            {
                return HexStringToEncodedString(match.Groups[1].Value, targetEncoding);
            });
        }

        public static string HexStringToEncodedString(string hexString, Encoding targetEncoding)
        {
            byte[] hexBytes = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                hexBytes[i / 2] = byte.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber);
            }

            return targetEncoding.GetString(hexBytes);
        }

        public static List<string> EncodedStringToHexStrings(string dataString, Encoding sourceEncoding)
        {
            List<string> hexStrings = new List<string>();
            StringBuilder hexStringBuilder = new StringBuilder();

            foreach (char c in dataString.ToCharArray())
            {
                byte[] bytes = sourceEncoding.GetBytes(c.ToString());

                foreach (byte b in bytes)
                {
                    hexStringBuilder.AppendFormat("{0:X2}", b);
                }

                hexStrings.Add(String.Format(Preferences.HexOutputFormat, hexStringBuilder.ToString()));
                hexStringBuilder.Clear();
            }

            return hexStrings;
        }
    }
}
