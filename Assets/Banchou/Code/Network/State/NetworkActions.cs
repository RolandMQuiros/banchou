using System;

namespace Banchou.Network {
    namespace StateAction {
        public class ConnectToServer {
            public string ServerIP;
            public int ServerHost;
        }

        public class ConnectedToServer {
            public int NetworkId;
            public Exception Error;
        }

        public class ClientConnected {
            public int ClientNetworkId;
            public string ClientIP;
            public string ClientPort;
        }
    }
}