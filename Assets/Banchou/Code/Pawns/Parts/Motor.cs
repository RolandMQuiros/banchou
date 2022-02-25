using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        [SerializeField] private LayerMask _projectionMask;
        
        [Header("Grounded Test")]
        [SerializeField] private Vector3 _groundedCastOrigin;
        [SerializeField] private float _groundedCastLength = 0.1f;
        [SerializeField] private float _groundedCastRadius = 0.48f;

        private GameState _state;
        private PawnSpatial _spatial;
        private Rigidbody _rigidbody;
        private bool _moved;
        private bool _isGrounded;

        private readonly List<Vector3> _contacts = new(32);

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Rigidbody body
        ) {
            _state = state;
            _rigidbody = body;

            var pawnId = getPawnId();
            _state.ObservePawnChanges(pawnId)
                .CatchIgnoreLog()
                .Subscribe(pawn => _spatial = pawn.Spatial)
                .AddTo(this);
            
            // When timescale changes, scale current velocity
            _state.ObservePawnTimeScale(pawnId)
                .Pairwise()
                .CatchIgnoreLog()
                .Subscribe(pair => _rigidbody.velocity *= pair.Current / pair.Previous)
                .AddTo(this);
        }

        public void Step() {
            _isGrounded = Physics.CheckCapsule(
                _spatial.Position + _groundedCastOrigin,
                _spatial.Position + _groundedCastOrigin - _spatial.Up * _groundedCastLength,
                _groundedCastRadius,
                _projectionMask.value
            );
            
            switch (_spatial.Style) {
                case PawnSpatial.MovementStyle.Offset: {
                    if (_spatial.Target != Vector3.zero) {
                        var projected = _spatial.Target.ProjectOnContacts(_spatial.Up, _contacts);
                        _rigidbody.MovePosition(_rigidbody.position + projected);
                        _moved = true;
                    }
                } break;
                case PawnSpatial.MovementStyle.Instantaneous: {
                    _rigidbody.position = Snap(_spatial.Target);
                    _rigidbody.velocity = _spatial.AmbientVelocity;
                } break;
                case PawnSpatial.MovementStyle.Interpolated: {
                    _rigidbody.position = Vector3.Slerp(_spatial.Position, _spatial.Target, 0.5f);
                    _rigidbody.velocity = _spatial.AmbientVelocity;
                } break;
            }
            
            if (Snap(_rigidbody.position - _spatial.Position) != Vector3.zero || _moved || 
                _spatial.IsGrounded != _isGrounded) {
                _spatial.Moved(Snap(_rigidbody.position), _rigidbody.velocity, _isGrounded, _state.GetTime(), _moved);
            } else {
                _spatial.SetStyle(PawnSpatial.MovementStyle.Offset, _state.GetTime());
            }
            
            _moved = false;
            _contacts.Clear();
        }

        private void OnCollisionStay(Collision collision) {
            for (int c = 0; c < collision.contactCount; c++) {
                var contact = collision.GetContact(c);
                if ((_projectionMask.value & (1 << contact.otherCollider.gameObject.layer)) != 0) {
                    _contacts.Add(contact.normal);
                }
            }
        }

        private void FixedUpdate() => Step();

        private void OnCollisionEnter(Collision collision) {
            OnCollisionStay(collision);
        }

        private Vector3 Snap(Vector3 vec) => Snapping.Snap(vec, Vector3.one * 0.001f);

        private void OnDrawGizmos() {
            var xform = transform;
            var origin = xform.position;
            var up = xform.up;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin + _groundedCastOrigin, _groundedCastRadius);
            Gizmos.DrawWireSphere(origin + _groundedCastOrigin - up * _groundedCastLength, _groundedCastRadius);
        }
    }
}