using System;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Network {
    public enum NetworkMode : byte {
        Local,
        Client,
        Host
    }

    [Serializable]
    public record NetworkState : Notifiable<NetworkState> {
        public const string Localhost = null;//"127.0.0.1";
        public const int DefaultPort = 0;
        public const int MaxPlayers = 32;
        [field: SerializeField] public int NetworkId { get; private set; }
        [field: SerializeField] public int HostNetworkId { get; private set; }
        [field: SerializeField] public NetworkMode Mode { get; private set; } = NetworkMode.Local;
        [field: SerializeField] public string HostName { get; private set; }
        [field: SerializeField] public int HostPort { get; private set;}
        [field: SerializeField] public int HostTimeOffset { get; private set; }
        [field: SerializeField] public string RoomName { get; private set; }
        [field: SerializeField] public int TickRate { get; private set; }
        [field: SerializeField] public int SimulateMinLatency  { get; private set; }
        [field: SerializeField] public int SimulateMaxLatency  { get; private set; }
        [field: SerializeField] public RollbackState Rollback  { get; private set; } = new RollbackState();
        [field: SerializeField] public NetworkStats Stats  { get; private set; } = new NetworkStats();
        public IReadOnlyList<int> Clients => _clients;
        [SerializeField] private List<int> _clients = new List<int>();

        public override void Dispose() {
            base.Dispose();
            Rollback.Dispose();
            Stats.Dispose();
        }

        public NetworkState ClientConnected(int clientNetworkId) {
            _clients.Add(clientNetworkId);
            Notify();
            return this;
        }

        public NetworkState ConnectToHost(
            string ip,
            int port,
            string roomName = null,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            Mode = NetworkMode.Client;
            HostName = ip;
            HostPort = port;
            RoomName = roomName;

            SimulateMinLatency = Math.Max(0, simulateMinLatency);
            SimulateMaxLatency = Math.Max(SimulateMinLatency, simulateMaxLatency);

            return this;
        }

        public NetworkState ConnectedToHost(int clientNetworkId, int hostNetworkId, int serverTimeOffset) {
            NetworkId = clientNetworkId;
            HostNetworkId = hostNetworkId;
            HostTimeOffset = serverTimeOffset;
            return Notify();
        }

        public NetworkState ConnectedToRoom(string roomName) {
            RoomName = roomName;
            return this;
        }

        public NetworkState StartHost(
            int port,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            Mode = NetworkMode.Host;
            NetworkId = 0;
            HostName = Localhost;
            HostPort = port;
            TickRate = tickRate;
            SimulateMinLatency = simulateMinLatency;
            SimulateMaxLatency = Math.Max(SimulateMinLatency, simulateMaxLatency);
            return Notify();
        }

        public NetworkState StartHost(
            string roomName,
            int tickRate,
            int simulateMinLatency = 0,
            int simulateMaxLatency = 0
        ) {
            Mode = NetworkMode.Host;
            NetworkId = 0;
            RoomName = roomName;
            TickRate = tickRate;
            SimulateMinLatency = simulateMinLatency;
            SimulateMaxLatency = Math.Max(SimulateMinLatency, simulateMaxLatency);
            return Notify();
        }
    }
}