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

        public int NetworkId => _networkId;
        [SerializeField] private int _networkId;

        public NetworkMode Mode => _mode;
        [SerializeField] private NetworkMode _mode = NetworkMode.Local;

        public string ServerIP => _serverIP;
        [SerializeField] private string _serverIP;

        public int ServerPort => _serverPort;
        [SerializeField] private int _serverPort;

        public float ServerTimeOffset => _serverTimeOffset;
        [SerializeField] private float _serverTimeOffset;

        public int TickRate => _tickRate;
        [SerializeField] private int _tickRate = 20;

        public int SimulateMinLatency => _simulateMinLatency;
        [SerializeField] private int _simulateMinLatency = 0;

        public int SimulateMaxLatency => _simulateMaxLatency;
        [SerializeField] private int _simulateMaxLatency = 0;

        public NetworkStats Stats => _stats;
        [SerializeField] private NetworkStats _stats = new NetworkStats();

        public RollbackState Rollback => _rollback;
        [SerializeField] private RollbackState _rollback;

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
            _mode = NetworkMode.Client;
            _serverIP = ip;
            _serverPort = port;

            _simulateMinLatency = Math.Max(0, simulateMinLatency);
            _simulateMaxLatency = Math.Max(_simulateMinLatency, simulateMaxLatency);

            return this;
        }

        public NetworkState ConnectedToServer(int clientNetworkId, float serverTimeOffset) {
            _networkId = clientNetworkId;
            _serverTimeOffset = serverTimeOffset;
            _rollback = new RollbackState();
            Notify();
            return this;
        }

        public NetworkState StartServer(
            int port,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            _mode = NetworkMode.Server;
            _networkId = 0;
            _serverIP = Localhost;
            _serverPort = port;
            _tickRate = tickRate;
            _simulateMinLatency = simulateMinLatency;
            _simulateMaxLatency = Math.Max(_simulateMinLatency, simulateMaxLatency);

            Notify();
            return this;
        }

        public NetworkState UpdateServerTime(float serverTimeOffset) {
            _serverTimeOffset = serverTimeOffset;
            Notify();
            return this;
        }
    }
}