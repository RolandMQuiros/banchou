using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyKnockbackForce : FSMBehaviour {
        [Serializable, Flags] private enum ApplyEvent { OnEnter = 1, OnExit = 2 }
        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.OnExit;
        [SerializeField] private float _multiplier = 1f;

        private GameState _state;
        private Rigidbody _rigidbody;
        private AttackState _hit;
        
        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody) {
            _state = state;
            _rigidbody = rigidbody;
            _state.ObserveHitsOn(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(hit => _hit = hit)
                .AddTo(this);
        }

        private void Apply() {
            if (_hit != null) {
                _rigidbody.velocity = _multiplier * _hit.Knockback;
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) Apply();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) Apply();
        }
    }
}