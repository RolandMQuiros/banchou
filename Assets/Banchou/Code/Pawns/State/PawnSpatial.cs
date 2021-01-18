using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnSpatial : Notifiable<PawnSpatial> {
        [IgnoreMember] public Vector3 Position => _position;
        [Key(0), SerializeField] private Vector3 _position;

        [IgnoreMember] public Vector3 Forward => _forward;
        [Key(1), SerializeField] private Vector3 _forward = Vector3.forward;

        [IgnoreMember] public Vector3 Up => _up;
        [Key(2), SerializeField] private Vector3 _up = Vector3.up;

        [IgnoreMember] public Vector3 Right => Vector3.Cross(_forward, _up);

        [IgnoreMember] public Vector3 Velocity => _velocity;
        [Key(3), SerializeField] private Vector3 _velocity = Vector3.zero;

        [IgnoreMember] public bool IsContinuous => _isContinuous;
        [Key(4), SerializeField] private bool _isContinuous = true;

        [IgnoreMember] public bool IsGrounded => _isGrounded;
        [Key(5), SerializeField] private bool _isGrounded = false;

        [IgnoreMember] public float LastUpdated => _lastUpdated;
        [Key(6), SerializeField] private float _lastUpdated;

        public PawnSpatial() { }

        public PawnSpatial(
            Vector3 position,
            Vector3 forward,
            Vector3 up,
            float when
        ) {
            _position = position;
            _forward = forward;
            _up = up;
        }

        public PawnSpatial Sync(PawnSpatial other) {
            _position = other._position;
            _forward = other._forward;
            _up = other._up;
            _isContinuous = other._isContinuous;
            _isGrounded = other._isGrounded;
            Notify();
            return this;
        }

        public PawnSpatial Move(Vector3 velocity, float when) {
            _velocity += velocity;
            _isContinuous = true;
            _lastUpdated = when;

            Notify();
            return this;
        }

        public PawnSpatial Teleport(Vector3 position, float when, bool cancelMomentum = true) {
            _position = position;
            _velocity = cancelMomentum ? Vector3.zero : _velocity;
            _isContinuous = false;
            _lastUpdated = when;

            Notify();
            return this;
        }

        public PawnSpatial Moved(Vector3 position, bool isGrounded, float when, bool cancelMomentum = true) {
            _position = position;
            _velocity = cancelMomentum ? Vector3.zero : _velocity;
            _isGrounded = isGrounded;
            _lastUpdated = when;

            Notify();
            return this;
        }
    }
}