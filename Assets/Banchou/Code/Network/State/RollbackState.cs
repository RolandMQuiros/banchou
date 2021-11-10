using System;
using UnityEngine;

namespace Banchou.Network {
    public enum RollbackPhase : byte {
        Complete,
        Rewinding,
        Resimulating
    }

    [Serializable]
    public record RollbackState : Notifiable<RollbackState> {
        public RollbackPhase Phase => _phase;
        [SerializeField] private RollbackPhase _phase = RollbackPhase.Complete;

        public float CorrectionTime => _correctionTime;
        [SerializeField] private float _correctionTime;

        public float DeltaTime => _deltaTime;
        [SerializeField] private float _deltaTime;

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