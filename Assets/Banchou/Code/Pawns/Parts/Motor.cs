using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        [SerializeField] private LayerMask _terrainMask = new LayerMask();

        private GameState _state;
        private PawnSpatial _spatial;
        private Rigidbody _rigidbody;
        private bool _isGrounded = false;
        private float _interpolationTime = 0f;
        private List<Vector3> _contacts = new List<Vector3>();

        public void Construct(
            GameState state,
            PawnState pawn,
            Rigidbody rigidbody
        ) {
            _state = state;
            _spatial = pawn.Spatial;
            _rigidbody = rigidbody;
        }

        private void FixedUpdate() {
            if (Snap(_rigidbody.position) != Snap(_spatial.Position)) {
                _spatial.Moved(Snap(_rigidbody.position), _isGrounded, _state.GetTime());
            }

            switch (_spatial.Style) {
                case PawnSpatial.MovementStyle.Offset: {
                    if (_spatial.Velocity != Vector3.zero) {
                        // _contacts.Sort(ContactSorter);
                        var projected = _spatial.Velocity.ProjectOnContacts(_spatial.Up, _contacts);
                        _rigidbody.MovePosition(_spatial.Position + projected);
                    }
                } break;
                case PawnSpatial.MovementStyle.Instantaneous: {
                    _rigidbody.position = _spatial.TeleportTarget;
                } break;
                case PawnSpatial.MovementStyle.Interpolated: {
                    _rigidbody.position = Vector3.Slerp(_spatial.Position, _spatial.TeleportTarget, 0.5f);
                } break;
            }

            _contacts.Clear();
            _isGrounded = false;
        }

        private void OnCollisionStay(Collision collision) {
            for (int c = 0; c < collision.contactCount; c++) {
                var contact = collision.GetContact(c);
                if ((_terrainMask.value & (1 << contact.otherCollider.gameObject.layer)) != 0) {
                    _isGrounded |= Vector3.Dot(contact.normal, _rigidbody.transform.up) >= 0.6f;
                    _contacts.Add(contact.normal);
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            OnCollisionStay(collision);
        }

        private int ContactSorter(Vector3 first, Vector3 second) {
            var diff = Vector3.Dot(first, _rigidbody.transform.up) - Vector3.Dot(second, _rigidbody.transform.up);
            return (int)Mathf.Sign(diff);
        }

        private Vector3 Snap(Vector3 vec) => Snapping.Snap(vec, Vector3.one * 0.001f);
    }
}