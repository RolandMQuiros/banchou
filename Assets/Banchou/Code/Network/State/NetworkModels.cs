using System.Collections;
using MessagePack;

namespace Banchou.Network {
    [MessagePackObject]
    public class NetworkState : Substate<NetworkState> {
        [IgnoreMember] public int NetworkId => _networkId;
        [Key(0)] private int _networkId;

        [IgnoreMember] public string ServerIP => _serverIP;
        [Key(1)] private string _serverIP;

        [IgnoreMember] public int ServerPort => _serverPort;
        [Key(2)] private int _serverPort;

        [IgnoreMember] public int TickRate => _tickRate;
        [Key(3)] private int _tickRate;

        [IgnoreMember] public Rollback Rollback => _rollback;
        [Key(4)] private Rollback _rollback;

        public void ConnectedToServer(int networkId, string ip, int port) {
            _networkId = networkId;
            _serverIP = ip;
            _serverPort = port;
            _rollback = new Rollback();
            Notify();
        }
    }

    public enum RollbackPhase : byte {
        Complete,
        Rewinding,
        Resimulating
    }

    public class Rollback : Substate<Rollback> {
        [Key(0)] public RollbackPhase Phase;
        [Key(1)] public float CorrectionTime;
        [Key(2)] public float DeltaTime;

        public Rollback StartRollback(float correctionTime, float deltaTime) {
            Phase = RollbackPhase.Rewinding;
            CorrectionTime = correctionTime;
            DeltaTime = deltaTime;

            Notify();
            return this;
        }

        public Rollback Simulate(float correctionTime) {
            Phase = RollbackPhase.Resimulating;
            CorrectionTime = correctionTime;

            Notify();
            return this;
        }

        public Rollback Finish(float correctionTime) {
            Phase = RollbackPhase.Complete;
            CorrectionTime = correctionTime;

            Notify();
            return this;
        }
    }
}