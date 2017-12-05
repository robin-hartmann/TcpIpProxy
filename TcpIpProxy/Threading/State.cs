using Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using TcpIpProxy.Configuration;
using TcpIpProxy.Helper;
using TcpIpProxy.Networking;

namespace TcpIpProxy.Threading
{
    public class State
    {
        public static readonly int DEF_SENDER_ID = -1;
        public static readonly string DEF_DATA = string.Empty;
        public static readonly bool DEF_TRANSLATE_TO_HEX = false;

        public State(string description, string data, bool translateToHex, int senderId, object sender)
        {
            string replacer = Preferences.NonPrintableReplacer.ToString();
            this.Timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            this.DescriptionLong = GetLongDescription(description, senderId, sender);
            this.Data = data.ReplaceControlChars(replacer, false);
            this.DataWithoutWhitespace = Data.ReplaceControlChars(replacer, true);
            this.Sender = sender;

            if (translateToHex)
            {
                Encoding sourceEncoding;

                if (sender is Client)
                {
                    sourceEncoding = Preferences.ClientStringEncoding;
                }
                else
                {
                    sourceEncoding = Preferences.ServerStringEncoding;
                }

                this.HexData = Utilities.EncodedStringToHexStrings(data, sourceEncoding);
            }
        }

        public State(string description, int senderId, object sender)
            : this(description, DEF_DATA, DEF_TRANSLATE_TO_HEX, senderId, sender)
        {
        }

        public string Timestamp
        {
            get;
            private set;
        }

        public string DescriptionLong
        {
            get;
            private set;
        }

        public string Data
        {
            get;
            private set;
        }

        public string DataWithoutWhitespace
        {
            get;
            private set;
        }

        public List<string> HexData
        {
            get;
            private set;
        }

        public object Sender
        {
            get;
            private set;
        }

        private string GetLongDescription(string description, int senderId, object sender)
        {
            string longText = string.Empty;
            string id = string.Empty;

            if (senderId != State.DEF_SENDER_ID)
            {
                id = senderId.ToString("D3");
            }

            longText = "[" + sender.GetType().Name.ToUpper() + id + "]" + description;

            return longText;
        }
    }
}
