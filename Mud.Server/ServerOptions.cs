using System;

namespace Mud.Server
{
    public class ServerOptions
    {
        #region Singleton

        private static readonly Lazy<ServerOptions> Lazy = new Lazy<ServerOptions>(() => new ServerOptions());

        public static ServerOptions Instance
        {
            get { return Lazy.Value; }
        }

        private ServerOptions()
        {
            PrefixForwardedMessages = true;
            ForwardSlaveMessages = true;
        }

        #endregion

        public bool PrefixForwardedMessages { get; set; } // Add <IMP> or <CTRL> before forwarding a message
        public bool ForwardSlaveMessages { get; set; } // Forward messages received by a slaved character
    }
}
