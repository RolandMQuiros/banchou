using System;
using UniRx;
using UnityEngine;

using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class ApplyForce : FSMBehaviour {
        [Serializable, Flags] private enum ApplyEvent { OnEnter = 1, OnUpdate = 2, OnExit = 4 }

        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.OnUpdate;
        [SerializeField] private ForceMode _forceMode = ForceMode.Force;
        [SerializeField] private Vector3 _force = Vector3.zero;
        [SerializeField] private Vector3 _relativeForce = Vector3.zero;

        private Rigidbody _rigidbody;

        public void Construct(Rigidbody rigidbody) {
            _rigidbody = rigidbody;
        }

        private void Apply() {
            if (_force != Vector3.zero) {
                _rigidbody.AddForce(_force, _forceMode);
            }

            if (_relativeForce != Vector3.zero) {
                _rigidbody.AddRelativeForce(_relativeForce, _forceMode);
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) Apply();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnUpdate)) Apply();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) Apply();
        }
    }
}