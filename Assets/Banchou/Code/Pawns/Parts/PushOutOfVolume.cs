using System;
using UnityEngine;
using UnityEngine.Events;

namespace Banchou.Pawn.Part {
    [RequireComponent(typeof(Collider))]
    public class PushOutOfVolume : MonoBehaviour {
        [Flags] private enum ApplyMode { OnEnter = 1, OnStay = 2, OnExit = 4 }
        private enum BounceMode { Direction, ContactNormal }

        [SerializeField] private ApplyMode _applyMode;
        [SerializeField] private BounceMode _bounceMode; 
        [SerializeField] private ForceMode _forceMode;
        [SerializeField] private float _magnitude;
        [SerializeField] private Vector3 _direction;

        private void Apply(Collider other) {
            if (other.attachedRigidbody != null) {
                var position = transform.position;
                
                switch (_bounceMode) {
                    case BounceMode.Direction:
                        other.attachedRigidbody.AddForce(_magnitude * _direction, _forceMode);
                        break;
                    case BounceMode.ContactNormal:
                        var normal = Vector3.Normalize(other.ClosestPoint(position) - position);
                        other.attachedRigidbody.AddForce(_magnitude * normal, _forceMode);
                        break;
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (_applyMode.HasFlag(ApplyMode.OnEnter)) Apply(other);
        }

        private void OnTriggerStay(Collider other) {
            if (_applyMode.HasFlag(ApplyMode.OnStay)) Apply(other);
        }

        private void OnTriggerExit(Collider other) {
            if (_applyMode.HasFlag(ApplyMode.OnExit)) Apply(other);
        }
    }
}