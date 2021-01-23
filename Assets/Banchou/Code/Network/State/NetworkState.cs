using System;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Network {
    public enum NetworkMode : byte {
        Local,
        Client,
        Server
    }

    [Serializable]
    public class NetworkState : Notifiable<NetworkState> {
        public const string Localhost = "127.0.0.1";
        public const int DefaultPort = 9050;
        [field: SerializeField] public int NetworkId { get; private set; }
        [field: SerializeField] public NetworkMode Mode { get; private set; } = NetworkMode.Local;
        [field: SerializeField] public string ServerIP { get; private set; }
        [field: SerializeField] public int ServerPort { get; private set;}
        [field: SerializeField] public float ServerTimeOffset { get; private set; }
        [field: SerializeField] public int TickRate { get; private set; }
        [field: SerializeField] public int SimulateMinLatency  { get; private set; }
        [field: SerializeField] public int SimulateMaxLatency  { get; private set; }
        [field: SerializeField] public NetworkStats Stats  { get; private set; } = new NetworkStats();
        [field: SerializeField] public RollbackState Rollback  { get; private set; } = new RollbackState();
        public IReadOnlyList<int> Clients => _clients;
        [SerializeField] private List<int> _clients = new List<int>();

        public NetworkState ClientConnected(int clientNetworkId) {
            _clients.Add(clientNetworkId);
            Notify();
            return this;
        }

        public NetworkState ConnectToServer(
            string ip,
            int port,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            Mode = NetworkMode.Client;
            ServerIP = ip;
            ServerPort = port;

            SimulateMinLatency = Math.Max(0, simulateMinLatency);
            SimulateMaxLatency = Math.Max(SimulateMinLatency, simulateMaxLatency);

            return this;
        }

        public NetworkState ConnectedToServer(int clientNetworkId, float serverTimeOffset) {
            NetworkId = clientNetworkId;
            ServerTimeOffset = serverTimeOffset;
            Notify();
            return this;
        }

        public NetworkState StartServer(
            int port,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            Mode = NetworkMode.Server;
            NetworkId = 0;
            ServerIP = Localhost;
            ServerPort = port;
            TickRate = tickRate;
            SimulateMinLatency = simulateMinLatency;
            SimulateMaxLatency = Math.Max(SimulateMinLatency, simulateMaxLatency);

            Notify();
            return this;
        }

        public NetworkState UpdateServerTime(float serverTimeOffset) {
            ServerTimeOffset = serverTimeOffset;
            Notify();
            return this;
        }
    }
}