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
        Vector3 Target = new(),
        PawnSpatial.MovementStyle Style = PawnSpatial.MovementStyle.Offset,
        Vector3 AmbientVelocity = new(),
        bool IsGrounded = false,
        float LastUpdated = 0f
    ) : NotifiableWithHistory<PawnSpatial>(32) {
        public enum MovementStyle : byte {
            Offset,
            Instantaneous,
            Interpolated
        }

        [Key(0)][field: SerializeField] public int PawnId { get; private set; } = PawnId;
        [Key(1)][field: SerializeField] public Vector3 Position { get; private set; } = Position;
        [Key(2)][field: SerializeField] public Vector3 Forward { get; private set; } = Forward;
        [Key(3)][field: SerializeField] public Vector3 Up { get; private set; } = Up;
        [IgnoreMember] public Vector3 Right => Vector3.Cross(Up, Forward);
        [Key(4)][field: SerializeField] public Vector3 Target { get; private set; } = Target;
        [Key(5)][field: SerializeField] public MovementStyle Style { get; private set; } = Style;
        [Key(6)][field: SerializeField] public Vector3 AmbientVelocity { get; private set; } = AmbientVelocity;
        [Key(7)][field: SerializeField] public bool IsGrounded { get; private set; } = IsGrounded;
        [Key(8)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;
        [IgnoreMember] public bool IsSync { get; private set; }
        
        public Vector3 DirectionTo(Vector3 target, bool onPlane = true) =>
            onPlane ? Vector3.ProjectOnPlane(target - Position, Up).normalized : (target - Position).normalized;

        public float DistanceTo(Vector3 target, bool onPlane = true) =>
            onPlane ? Vector3.ProjectOnPlane(target - Position, Up).magnitude : (target - Position).magnitude;

        public override void Set(PawnSpatial from) {
            PawnId = from.PawnId;
            Position = from.Position;
            Forward = from.Forward;
            Up = from.Up;
            Target = from.Target;
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
            IsSync = true;
            return Notify(other.LastUpdated);
        }

        public PawnSpatial Move(Vector3 offset, float when) {
            if (offset != Vector3.zero) {
                Target += offset;
                Style = MovementStyle.Offset;
                LastUpdated = when;
                return Notify(when);
            }
            return this;
        }

        public PawnSpatial Teleport(Vector3 position, float when, bool instant = false) {
            if (position != Target) {
                Target = position;
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
            Target = cancelMomentum ? Vector3.zero : Target;
            IsGrounded = isGrounded;
            LastUpdated = when;
            Style = MovementStyle.Offset;
            IsSync = false;
            return Notify(when);
        }

        public PawnSpatial SetStyle(MovementStyle style, float when) {
            if (style != Style) {
                Style = style;
                return Notify(when);
            }
            return this;
        }
    }
}