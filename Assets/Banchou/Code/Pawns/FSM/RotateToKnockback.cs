using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class RotateToKnockback : FSMBehaviour {
        [Serializable] private enum ApplyEvent { OnEnter, OnExit }
        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.OnEnter;
        [SerializeField] private bool _oppositeDirection = true;

        private GameState _state;
        private PawnSpatial _spatial;
        private Vector3 _knockback;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            state.ObserveHitsOn(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(hit => _knockback = hit.Knockback)
                .AddTo(this);
        }

        private void Apply() {
            if (_knockback == Vector3.zero) return;
            
            var knockback = _knockback;
            if (_oppositeDirection) knockback = -knockback;

            _spatial.Rotate(
                Vector3.ProjectOnPlane(knockback, _spatial.Up).normalized,
                _state.GetTime()
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent == ApplyEvent.OnEnter) Apply();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent == ApplyEvent.OnExit) Apply();
        }
    }
}