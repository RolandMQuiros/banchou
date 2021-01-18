using System;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Network {
    [Serializable]
    public class NetworkState : Notifiable<NetworkState> {
        public int NetworkId => _networkId;
        [SerializeField] private int _networkId;

        public string ServerIP => _serverIP;
        [SerializeField] private string _serverIP;

        public int ServerPort => _serverPort;
        [SerializeField] private int _serverPort;

        public float ServerTimeOffset => _serverTimeOffset;
        [SerializeField] private float _serverTimeOffset;

        public int TickRate => _tickRate;
        [SerializeField] private int _tickRate = 20;

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

        public NetworkState ConnectedToServer(int clientNetworkId, string ip, int port, float serverTimeOffset) {
            _networkId = clientNetworkId;
            _serverIP = ip;
            _serverPort = port;
            _serverTimeOffset = serverTimeOffset;
            _rollback = new RollbackState();
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