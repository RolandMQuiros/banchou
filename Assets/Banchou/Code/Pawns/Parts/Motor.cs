using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        [SerializeField] private LayerMask _terrainMask = new LayerMask();

        private PawnState _pawn;
        private Rigidbody _rigidbody;
        private GetTime _getTime;
        private bool _isGrounded = false;
        private bool _moved = false;
        private List<Vector3> _contacts = new List<Vector3>();

        public void Construct(
            PawnState pawn,
            Rigidbody rigidbody,
            GetTime getTime
        ) {
            _pawn = pawn;
            _rigidbody = rigidbody;
            _getTime = getTime;
        }

        private void FixedUpdate() {
            Vector3 Snap(Vector3 vec) => Snapping.Snap(vec, Vector3.one * 0.001f);

            if (Snap(_rigidbody.position) != Snap(_pawn.Position)) {
                _pawn.Moved(Snap(_rigidbody.position), _isGrounded, _getTime(), cancelMomentum: _moved);
            }
            _moved = false;

            if (_pawn.Velocity != Vector3.zero) {
                // _contacts.Sort(ContactSorter);
                var projected = _pawn.Velocity.ProjectOnContacts(_pawn.Up, _contacts);
                _rigidbody.MovePosition(_pawn.Position + projected);
                _moved = true;
            } else {
                _rigidbody.position = _pawn.Position;
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
    }
}