using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnSpatial : NotifiableWithHistory<PawnSpatial> {
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
        [Key(7)][field: SerializeField] public Vector3 AmbientVelocity { get; private set; }
        [Key(8)][field: SerializeField] public bool IsGrounded { get; private set; }
        [Key(9)][field: SerializeField] public float LastUpdated { get; private set; }

        [SerializationConstructor]
        public PawnSpatial(
            int pawnId,
            Vector3 position,
            Vector3 forward,
            Vector3 up,
            Vector3 offset,
            Vector3 teleportTarget,
            MovementStyle style,
            Vector3 ambientVelocity,
            bool isGrounded,
            float lastUpdated
        ) : base(32) {
            PawnId = pawnId;
            Position = position;
            Forward = forward;
            Up = up;
            Offset = offset;
            TeleportTarget = teleportTarget;
            Style = style;
            AmbientVelocity = ambientVelocity;
            IsGrounded = isGrounded;
            LastUpdated = lastUpdated;
        }

        public PawnSpatial(PawnSpatial other) : base(32) => Set(other);

        public override void Set(PawnSpatial from) {
            PawnId = from.PawnId;
            Position = from.Position;
            Forward = from.Forward;
            Up = from.Up;
            Offset = from.Offset;
            TeleportTarget = from.TeleportTarget;
            Style = from.Style;
            AmbientVelocity = from.AmbientVelocity;
            IsGrounded = from.IsGrounded;
            LastUpdated = from.LastUpdated;
        }

        public PawnSpatial(int pawnId) : base(32) {
            PawnId = pawnId;
        }

        public PawnSpatial(
            int pawnId,
            Vector3 position,
            Vector3 forward,
            Vector3 up,
            float when
        ) : base(32) {
            PawnId = pawnId;
            Position = position;
            Forward = forward;
            Up = up;
            LastUpdated = when;
        }

        public PawnSpatial Sync(PawnSpatial other) {
            Set(other);
            return Notify(other.LastUpdated);
        }

        public PawnSpatial Move(Vector3 offset, float when) {
            if (offset != Vector3.zero) {
                Offset += offset;
                Style = MovementStyle.Offset;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public PawnSpatial Teleport(Vector3 position, float when, bool instant = false) {
            if (position != TeleportTarget) {
                TeleportTarget = position;
                Style = instant ? MovementStyle.Instantaneous : MovementStyle.Interpolated;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public PawnSpatial Rotate(Vector3 forward, float when) {
            if (Forward != forward) {
                Forward = forward;
                LastUpdated = when;
                Notify(when);
            }
            return this;
        }

        public PawnSpatial Moved(Vector3 position, Vector3 velocity, bool isGrounded, float when, bool cancelMomentum = true) {
            AmbientVelocity = velocity;
            Position = position;
            Offset = cancelMomentum ? Vector3.zero : Offset;
            IsGrounded = isGrounded;
            LastUpdated = when;
            return Notify(when);
        }
    }
}