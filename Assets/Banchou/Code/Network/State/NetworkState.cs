using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Network {

    [MessagePackObject, Serializable]
    public class NetworkState : Substate<NetworkState> {
        [IgnoreMember] public int NetworkId => _networkId;
        [Key(0), SerializeField] private int _networkId;

        [IgnoreMember] public string ServerIP => _serverIP;
        [Key(1), SerializeField] private string _serverIP;

        [IgnoreMember] public int ServerPort => _serverPort;
        [Key(2), SerializeField] private int _serverPort;

        [IgnoreMember] public float ServerTimeOffset => _serverTimeOffset;
        [Key(3), SerializeField] private float _serverTimeOffset;

        [IgnoreMember] public int TickRate => _tickRate;
        [Key(4), SerializeField] private int _tickRate = 20;

        [IgnoreMember] public RollbackState Rollback => _rollback;
        [Key(4), SerializeField] private RollbackState _rollback;

        [IgnoreMember] public IReadOnlyList<int> Clients => _clients;
        [Key(5), SerializeField] private List<int> _clients = new List<int>();

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