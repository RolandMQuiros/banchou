using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Network {
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