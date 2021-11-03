using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnSpatial : Notifiable<PawnSpatial>, IRecordable<PawnSpatial> {
        public enum MovementStyle : byte {
            Offset,
            Instantaneous,
            Interpolated
        }

        [Key(0)][field: SerializeField] public int PawnId { get; private set; }
        [Key(1)][field: SerializeField] public Vector3 Position { get; private set; }
        [Key(2)][field: SerializeField] public Vector3 Forward { get; private set; }
        [Key(3)][field: SerializeField] public Vector3 Up { get; private set; }
        [IgnoreMember] public Vector3 Right => Vector3.Cross(Up, Forward);
        [Key(4)][field: SerializeField] public Vector3 Offset { get; private set; }
        [Key(5)][field: SerializeField] public Vector3 TeleportTarget { get; private set; }
        [Key(6)][field: SerializeField] public MovementStyle Style { get; private set; }
        [Key(7)][field: SerializeField] public Vector3 Velocity { get; private set; }
        [Key(8)][field: SerializeField] public bool IsGrounded { get; private set; }
        [Key(9)][field: SerializeField] public float LastUpdated { get; private set; }
        [IgnoreMember] private History<PawnSpatial> _history;

        [SerializationConstructor]
        public PawnSpatial(
            int pawnId,
            Vector3 position,
            Vector3 forward,
            Vector3 up,
            Vector3 offset,
            Vector3 teleportTarget,
            MovementStyle style,
            Vector3 velocity,
            bool isGrounded,
            float lastUpdated
        ) {
            PawnId = pawnId;
            Position = position;
            Forward = forward;
            Up = up;
            Offset = offset;
            TeleportTarget = teleportTarget;
            Style = style;
            Velocity = velocity;
            IsGrounded = isGrounded;
            LastUpdated = lastUpdated;
            _history = new History<PawnSpatial>(32, () => new PawnSpatial(PawnId));
        }

        public PawnSpatial(PawnSpatial other) => Set(other);

        public void Set(PawnSpatial from) {
            PawnId = from.PawnId;
            Position = from.Position;
            Forward = from.Forward;
            Up = from.Up;
            Offset = from.Offset;
            TeleportTarget = from.TeleportTarget;
            Style = from.Style;
            Velocity = from.Velocity;
            IsGrounded = from.IsGrounded;
            LastUpdated = from.LastUpdated;
        }

        public PawnSpatial(int pawnId) {
            PawnId = pawnId;
            _history = new History<PawnSpatial>(32, () => new PawnSpatial(PawnId));
        }

        public PawnSpatial(
            int pawnId,
            Vector3 position,
            Vector3 forward,
            Vector3 up,
            float when
        ) {
            PawnId = pawnId;
            Position = position;
            Forward = forward;
            Up = up;
            _history = new History<PawnSpatial>(32, () => new PawnSpatial(PawnId));
        }

        public PawnSpatial Sync(PawnSpatial other) {
            TeleportTarget = other.Position;
            Style = MovementStyle.Interpolated;
            Forward = other.Forward;
            Up = other.Up;
            IsGrounded = other.IsGrounded;
            LastUpdated = other.LastUpdated;
            return Notify();
        }

        public PawnSpatial Move(Vector3 offset, float when) {
            if (offset != Vector3.zero) {
                Offset += offset;
                Style = MovementStyle.Offset;
                LastUpdated = when;
                return Notify();
            }
            return this;
        }

        public PawnSpatial Teleport(Vector3 position, float when, bool instant = false) {
            if (position != TeleportTarget) {
                TeleportTarget = position;
                Style = instant ? MovementStyle.Instantaneous : MovementStyle.Interpolated;
                LastUpdated = when;
                return Notify();
            }
            return this;
        }

        public PawnSpatial Rotate(Vector3 forward, float when) {
            if (Forward != forward) {
                Forward = forward;
                LastUpdated = when;
                Notify();
            }
            return this;
        }

        public PawnSpatial Moved(Vector3 position, bool isGrounded, float when, bool cancelMomentum = true) {
            Velocity = (position - Position) / (when - LastUpdated);
            Position = position;
            Offset = cancelMomentum ? Vector3.zero : Offset;
            IsGrounded = isGrounded;
            LastUpdated = when;

            return Notify();
        }
        
        #region History
        protected override PawnSpatial Notify() {
            _history.PushFrame(this, LastUpdated);
            return base.Notify();
        }
        #endregion
    }
}