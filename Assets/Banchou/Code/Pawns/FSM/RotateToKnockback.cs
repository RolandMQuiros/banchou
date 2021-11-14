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
        private HitState _lastHit;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            state.ObserveLastHit(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(hit => _lastHit = hit)
                .AddTo(this);
        }

        private void Apply() {
            if (_lastHit.Knockback == Vector3.zero) return;
            
            var knockback = _lastHit.Knockback;
            if (_oppositeDirection) knockback = -knockback;
            
            _spatial.Rotate(
                Vector3.ProjectOnPlane(knockback, _spatial.Up).normalized,
                _state.GetTime()
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent == ApplyEvent.OnEnter) Apply();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent == ApplyEvent.OnExit) Apply();
        }
    }
}