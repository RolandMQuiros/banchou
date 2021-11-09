using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        [SerializeField] private LayerMask _terrainMask = new LayerMask();

        private GameState _state;
        private PawnSpatial _spatial;
        private Rigidbody _rigidbody;
        private bool _isGrounded = false;
        private bool _moved = false;
        [SerializeField] private List<Vector3> _contacts = new List<Vector3>(32);

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Rigidbody body
        ) {
            _state = state;
            _rigidbody = body;
            _state.ObservePawnChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(pawn => _spatial = pawn.Spatial)
                .AddTo(this);
        }

        public void Step() {
            if (Snap(_rigidbody.position - _spatial.Position) != Vector3.zero || _spatial.IsGrounded != _isGrounded) {
                _spatial.Moved(Snap(_rigidbody.position), _rigidbody.velocity, _isGrounded, _state.GetTime(), _moved);
            }
            _moved = false;

            switch (_spatial.Style) {
                case PawnSpatial.MovementStyle.Offset: {
                    if (_spatial.Offset != Vector3.zero) {
                        var projected = _spatial.Offset.ProjectOnContacts(_spatial.Up, _contacts);
                        _rigidbody.MovePosition(_spatial.Position + projected);
                        _moved = true;
                    }
                } break;
                case PawnSpatial.MovementStyle.Instantaneous: {
                    _rigidbody.position = Snap(_spatial.TeleportTarget);
                    _rigidbody.velocity = _spatial.AmbientVelocity;
                } break;
                case PawnSpatial.MovementStyle.Interpolated: {
                    _rigidbody.position = Vector3.Slerp(_spatial.Position, _spatial.TeleportTarget, 0.5f);
                    _rigidbody.velocity = _spatial.AmbientVelocity;
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

        private void FixedUpdate() => Step();
        private void OnCollisionEnter(Collision collision) => OnCollisionStay(collision);
        private Vector3 Snap(Vector3 vec) => Snapping.Snap(vec, Vector3.one * 0.001f);
    }
}