using System;

namespace Banchou.Network {
    namespace StateAction {
        public struct ConnectToServer {
            public string ServerIP;
            public int ServerHost;
        }

        public struct ConnectedToServer {
            public int NetworkId;
            public Exception Error;
        }

        public struct ClientConnected {
            public int ClientNetworkId;
            public string ClientIP;
            public string ClientPort;
        }
    }
}