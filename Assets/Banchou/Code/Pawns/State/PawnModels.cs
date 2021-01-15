using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    public delegate int GetPawnId();

    [MessagePackObject]
    public class PawnState : Substate<PawnState> {
        [Key(0)] public readonly int PawnId;
        [Key(1)] public readonly string PrefabKey;
        [Key(2)] public int PlayerId { get; private set; }
        [Key(3)] public Vector3 Position { get; private set; }
        [Key(4)] public Vector3 Forward { get; private set; }
        [Key(5)] public Vector3 Up { get; private set; }
        [IgnoreMember] public Vector3 Right => Vector3.Cross(Forward, Up);
        [Key(6)] public Vector3 Velocity { get; private set; }
        [Key(7)] public bool IsContinuous { get; private set; }
        [Key(8)] public bool IsGrounded { get; private set; }
        [Key(9)] public float LastUpdated { get; private set; }

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
            float lastUpdated = 0f
        ) {
            PawnId = pawnId;
            PrefabKey = prefabKey;
            Position = position;
            PlayerId = playerId;
            Forward = forward ?? Vector3.forward;
            Up = up ?? Vector3.up;
            Velocity = velocity ?? Vector3.zero;
            IsContinuous = isContinuous;
            IsGrounded = isGrounded;
            LastUpdated = lastUpdated;
        }

        public void SyncGame(GameState sync, float when) {
            PawnState other;
            if (sync.Board.Pawns.TryGetValue(PawnId, out other)) {
                PlayerId = other.PlayerId;
                Position = other.Position;
                Forward = other.Forward;
                Up = other.Up;
                Velocity = other.Velocity;
                IsContinuous = other.IsContinuous;
                IsGrounded = other.IsGrounded;
                LastUpdated = other.LastUpdated;

                // CurrentFrame = other.CurrentFrame;
                Notify();
            }
        }

        public void Move(Vector3 velocity, float when) {
            Velocity += velocity;
            IsContinuous = true;
            LastUpdated = when;
            Notify();
        }

        public void Teleport(Vector3 position, float when, bool cancelMomentum = true) {
            Position = position;
            Velocity = cancelMomentum ? Vector3.zero : Velocity;
            IsContinuous = false;
            LastUpdated = when;
            Notify();
        }

        public void Moved(Vector3 position, bool isGrounded, float when, bool cancelMomentum = true) {
            Position = position;
            Velocity = cancelMomentum ? Vector3.zero : Velocity;
            IsGrounded = isGrounded;
            LastUpdated = when;
            Notify();
        }

        public void RemovePlayer(int playerId, float when) {
            if (playerId == PlayerId) {
                PlayerId = default;
                LastUpdated = when;
                Notify();
            }
        }

        public void Animated(Animator animator, float when) {
            // CurrentFrame.Animated(animator, when);
            Notify();
        }
    }
}