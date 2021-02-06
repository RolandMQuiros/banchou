using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnSpatial : Notifiable<PawnSpatial> {
        public enum MovementStyle : byte {
            Offset,
            Instantaneous,
            Interpolated
        }

        [Key(0)] public readonly int PawnId;
        [Key(1)][field: SerializeField] public Vector3 Position { get; private set; }
        [Key(2)][field: SerializeField] public Vector3 Forward { get; private set; }
        [Key(3)][field: SerializeField] public Vector3 Up { get; private set; }
        [IgnoreMember] public Vector3 Right => Vector3.Cross(Up, Forward);
        [Key(4)][field: SerializeField] public Vector3 Velocity { get; private set; }
        [Key(5)][field: SerializeField] public Vector3 TeleportTarget { get; private set; }
        [Key(6)][field: SerializeField] public MovementStyle Style { get; private set; }
        [Key(7)][field: SerializeField] public bool IsGrounded { get; private set; }
        [Key(8)][field: SerializeField] public float LastUpdated { get; private set; }

        [SerializationConstructor]
        public PawnSpatial(int pawnId, Vector3 position, Vector3 forward, Vector3 up, Vector3 velocity, Vector3 teleportTarget, MovementStyle style, bool isGrounded, float lastUpdated) {
            PawnId = pawnId;
            Position = position;
            Forward = forward;
            Up = up;
            Velocity = velocity;
            TeleportTarget = teleportTarget;
            Style = style;
            IsGrounded = isGrounded;
            LastUpdated = lastUpdated;
        }

        public PawnSpatial(int pawnId) {
            PawnId = pawnId;
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
        }

        public PawnSpatial Sync(PawnSpatial other) {
            TeleportTarget = other.Position;
            Style = MovementStyle.Interpolated;
            Forward = other.Forward;
            Up = other.Up;
            IsGrounded = other.IsGrounded;
            LastUpdated = other.LastUpdated;
            Notify();
            return this;
        }

        public PawnSpatial Move(Vector3 velocity, float when) {
            if (velocity != Vector3.zero) {
                Velocity += velocity;
                Style = MovementStyle.Offset;
                LastUpdated = when;

                Notify();
            }
            return this;
        }

        public PawnSpatial Teleport(Vector3 position, float when, bool instant = false) {
            if (position != TeleportTarget) {
                TeleportTarget = position;
                Style = instant ? MovementStyle.Instantaneous : MovementStyle.Interpolated;
                LastUpdated = when;

                Notify();
            }
            return this;
        }

        public PawnSpatial Rotate(Vector3 forward, float when) {
            if (Forward != forward) {
                Forward = forward;
                Notify();
            }
            return this;
        }

        public PawnSpatial Moved(Vector3 position, bool isGrounded, float when, bool cancelMomentum = true) {
            Position = position;
            Velocity = cancelMomentum ? Vector3.zero : Velocity;
            IsGrounded = isGrounded;
            LastUpdated = when;

            Notify();
            return this;
        }
    }
}