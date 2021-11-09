using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public record PawnSpatial(
        int PawnId,
        Vector3 Position = new(),
        Vector3 Forward = new(),
        Vector3 Up = new(),
        Vector3 Offset = new(),
        Vector3 TeleportTarget = new(),
        PawnSpatial.MovementStyle Style = PawnSpatial.MovementStyle.Offset,
        Vector3 AmbientVelocity = new(),
        bool IsGrounded = false,
        float LastUpdated = 0f
    ) : NotifiableRecordWithHistory<PawnSpatial>(32) {
        public enum MovementStyle : byte {
            Offset,
            Instantaneous,
            Interpolated
        }

        [field: SerializeField] public int PawnId { get; private set; } = PawnId;
        [field: SerializeField] public Vector3 Position { get; private set; } = Position;
        [field: SerializeField] public Vector3 Forward { get; private set; } = Forward;
        [field: SerializeField] public Vector3 Up { get; private set; } = Up;
        public Vector3 Right => Vector3.Cross(Up, Forward);
        [field: SerializeField] public Vector3 Offset { get; private set; } = Offset;
        [field: SerializeField] public Vector3 TeleportTarget { get; private set; } = TeleportTarget;
        [field: SerializeField] public MovementStyle Style { get; private set; } = Style;
        [field: SerializeField] public Vector3 AmbientVelocity { get; private set; } = AmbientVelocity;
        [field: SerializeField] public bool IsGrounded { get; private set; } = IsGrounded;
        [field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

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
        
        public PawnSpatial(
            int pawnId,
            Vector3 position,
            Vector3 forward,
            Vector3 up,
            float when
        ) : this(pawnId, position, forward, up, LastUpdated: when) { }

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