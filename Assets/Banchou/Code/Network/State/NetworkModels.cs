using System.Collections;
using MessagePack;

namespace Banchou.Network {
    [MessagePackObject]
    public class NetworkState : Substate<NetworkState> {
        [Key(0)] public int NetworkId { get; private set; }
        [Key(1)] public string ServerIP { get; private set; }
        [Key(2)] public int ServerPort { get; private set; }

        public void ConnectedToServer(int networkId, string ip, int port) {
            NetworkId = networkId;
            ServerIP = ip;
            ServerPort = port;
            Notify();
        }
    }
}