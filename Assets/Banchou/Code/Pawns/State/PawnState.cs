using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    public delegate int GetPawnId();

    [MessagePackObject, Serializable]
    public class PawnState : Substate<PawnState> {
        [Key(0)] public readonly int PawnId;
        [Key(1)] public readonly string PrefabKey;

        [IgnoreMember] public int PlayerId => _playerId;
        [Key(2), SerializeField] public int _playerId { get; private set; }

        [IgnoreMember] public Vector3 Position => _position;
        [Key(3), SerializeField] public Vector3 _position;

        [IgnoreMember] public Vector3 Forward => _forward;
        [Key(4), SerializeField] public Vector3 _forward;

        [IgnoreMember] public Vector3 Up => _up;
        [Key(5), SerializeField] public Vector3 _up;

        [IgnoreMember] public Vector3 Right => Vector3.Cross(_forward, _up);

        [IgnoreMember] public Vector3 Velocity => _velocity;
        [Key(6), SerializeField] public Vector3 _velocity;

        [IgnoreMember] public bool IsContinuous => _isContinuous;
        [Key(7), SerializeField] public bool _isContinuous;

        [IgnoreMember] public bool IsGrounded => _isGrounded;
        [Key(8), SerializeField] public bool _isGrounded;

        [IgnoreMember] public PawnHistory History => _history;
        [Key(9), SerializeField] public PawnHistory _history;

        [IgnoreMember] public float LastUpdated => _lastUpdated;
        [Key(10), SerializeField] public float _lastUpdated;

        public PawnState(
            int pawnId,
            string prefabKey,
            int playerId = 0,
            Vector3 position = default,
            Vector3? forward = null,
            Vector3? up = null,
            Vector3? velocity = null,
            bool isContinuous = true,
            bool isGrounded = false,
            PawnHistory history = null,
            float lastUpdated = 0f
        ) {
            PawnId = pawnId;
            PrefabKey = prefabKey;
            _position = position;
            _playerId = playerId;
            _forward = forward ?? Vector3.forward;
            _up = up ?? Vector3.up;
            _velocity = velocity ?? Vector3.zero;
            _isContinuous = isContinuous;
            _isGrounded = isGrounded;
            _history = history ?? new PawnHistory();
            _lastUpdated = lastUpdated;
        }

        protected override void OnProcess() {
            History.Process();
        }

        public PawnState SyncGame(GameState sync, float when) {
            PawnState other;
            if (sync.Board.Pawns.TryGetValue(PawnId, out other)) {
                _playerId = other._playerId;
                _position = other._position;
                _forward = other._forward;
                _up = other._up;
                _velocity = other._velocity;
                _isContinuous = other._isContinuous;
                _isGrounded = other._isGrounded;
                _lastUpdated = other._lastUpdated;

                _history.Copy(other._history);
                Notify();
            }
            return this;
        }

        public PawnState Move(Vector3 velocity, float when) {
            _velocity += velocity;
            _isContinuous = true;
            _lastUpdated = when;

            Notify();
            return this;
        }

        public PawnState Teleport(Vector3 position, float when, bool cancelMomentum = true) {
            _position = position;
            _velocity = cancelMomentum ? Vector3.zero : _velocity;
            _isContinuous = false;
            _lastUpdated = when;

            Notify();
            return this;
        }

        public PawnState Moved(Vector3 position, bool isGrounded, float when, bool cancelMomentum = true) {
            _position = position;
            _velocity = cancelMomentum ? Vector3.zero : _velocity;
            _isGrounded = isGrounded;
            _lastUpdated = when;

            Notify();
            return this;
        }

        public PawnState RemovePlayer(int playerId, float when) {
            if (playerId == _playerId) {
                _playerId = default;
                _lastUpdated = when;
                Notify();
            }
            return this;
        }
    }
}