using System;
using System.Collections.Generic;

using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        [SerializeField] private LayerMask _terrainMask = new LayerMask();
        private List<Vector3> _contacts = new List<Vector3>();
        private Rigidbody _rigidbody;
        private ContactSorter _sorter;
        private bool _isGrounded = false;

        public void Construct(
            PawnState pawn,
            Rigidbody rigidbody,
            GetTime getTime
        ) {
            _rigidbody = rigidbody;
            _sorter = new ContactSorter(rigidbody.transform);

            var moved = false;
            this.FixedUpdateAsObservable()
                .Subscribe(_ => {
                    pawn.Moved(rigidbody.position, _isGrounded, getTime(), cancelMomentum: moved);
                    moved = false;

                    if (pawn.IsContinuous && pawn.Velocity != Vector3.zero) {
                        _contacts.Sort(_sorter);
                        var projected = pawn.Velocity.ProjectOnContacts(pawn.Up, _contacts);
                        rigidbody.MovePosition(rigidbody.position + projected);
                        moved = true;
                    } else if (!pawn.IsContinuous) {
                        rigidbody.position = pawn.Position;
                    }

                    _contacts.Clear();
                    _isGrounded = false;
                })
                .AddTo(this);
        }

        private void OnCollisionStay(Collision collision) {
            for (int c = 0; c < collision.contacts.Length; c++) {
                var contact = collision.contacts[c];
                if ((_terrainMask.value & (1 << contact.otherCollider.gameObject.layer)) != 0) {
                    _isGrounded |= Vector3.Dot(contact.normal, _rigidbody.transform.up) >= 0.6f;
                    _contacts.Add(contact.normal);
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            OnCollisionStay(collision);
        }

        private class ContactSorter : IComparer<Vector3> {
            private Transform _xform;
            public ContactSorter(Transform xform) {
                _xform = xform;
            }
            public int Compare(Vector3 first, Vector3 second) {
                var diff = Vector3.Dot(first, _xform.up) - Vector3.Dot(first, _xform.up);
                return (int)Mathf.Sign(diff);
            }
        }
    }
}