using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Network {
    [MessagePackObject, Serializable]
    public class NetworkState : Substate<NetworkState> {
        [IgnoreMember] public int NetworkId => _networkId;
        [Key(0)] private int _networkId;

        [IgnoreMember] public string ServerIP => _serverIP;
        [Key(1)] private string _serverIP;

        [IgnoreMember] public int ServerPort => _serverPort;
        [Key(2)] private int _serverPort;

        [IgnoreMember] public int TickRate => _tickRate;
        [Key(3)] private int _tickRate;

        [IgnoreMember] public RollbackState Rollback => _rollback;
        [Key(4)] private RollbackState _rollback;

        public void ConnectedToServer(int networkId, string ip, int port) {
            _networkId = networkId;
            _serverIP = ip;
            _serverPort = port;
            _rollback = new RollbackState();
            Notify();
        }
    }

    public enum RollbackPhase : byte {
        Complete,
        Rewinding,
        Resimulating
    }

    [Serializable]
    public class RollbackState : Substate<RollbackState> {
        [IgnoreMember] public RollbackPhase Phase => _phase;
        [Key(0), SerializeField] private RollbackPhase _phase;

        [IgnoreMember] public float CorrectionTime => _correctionTime;
        [Key(1), SerializeField] private float _correctionTime;

        [IgnoreMember] public float DeltaTime => _deltaTime;
        [Key(2), SerializeField] private float _deltaTime;

        public RollbackState StartRollback(float correctionTime, float deltaTime) {
            _phase = RollbackPhase.Rewinding;
            _correctionTime = correctionTime;
            _deltaTime = deltaTime;

            Notify();
            return this;
        }

        public RollbackState Simulate(float correctionTime) {
            _phase = RollbackPhase.Resimulating;
            _correctionTime = correctionTime;

            Notify();
            return this;
        }

        public RollbackState Finish(float correctionTime) {
            _phase = RollbackPhase.Complete;
            _correctionTime = correctionTime;

            Notify();
            return this;
        }
    }
}