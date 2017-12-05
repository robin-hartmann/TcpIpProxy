using System;

namespace TcpIpProxy.Threading
{
    public class StateReporter
    {
        private static IProgress<State> PROGRESS_REPORTER;

        public StateReporter(int senderId, Object sender)
        {
            this.SenderId = senderId;
            this.Sender = sender;
        }

        public StateReporter(Object sender)
            : this(State.DEF_SENDER_ID, sender)
        {
        }

        public int SenderId
        {
            get;
            private set;
        }

        public Object Sender
        {
            get;
            private set;
        }

        public static void setReporter(IProgress<State> reporter)
        {
            PROGRESS_REPORTER = reporter;
        }

        public void Report(string description, string message, bool translateToHex)
        {
            PROGRESS_REPORTER.Report(new State(description, message, translateToHex, SenderId, Sender));
        }

        public void Report(string description, string message)
        {
            Report(description, message, State.DEF_TRANSLATE_TO_HEX);
        }

        public void Report(string description)
        {
            Report(description, State.DEF_DATA);
        }
    }
}
