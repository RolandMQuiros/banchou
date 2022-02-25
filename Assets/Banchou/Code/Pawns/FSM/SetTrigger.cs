using System;
using System.Linq;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetTriggers : FSMBehaviour {
        [Flags] private enum ApplyEvent { OnEnter = 1, AtStateTime = 2, AtTime = 4, OnExit = 8 }

        [SerializeField, Tooltip("When to reset the triggers")]
        private ApplyEvent _onEvent;

        [SerializeField, Tooltip("If setting at state time, what time")]
        private float _atStateTime;

        [SerializeField, Tooltip("If setting at time after state entry, what time")]
        private float _atTime;
        
        [SerializeField, Tooltip("Names of the triggers to reset")]
        private string[] _triggers;
        
        private GetDeltaTime _getDeltaTime;
        private float _time;
        private int[] _hashes;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _getDeltaTime = state.PawnDeltaTime(getPawnId());
            _hashes = _triggers.Select(Animator.StringToHash).ToArray();
        }
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) {
                foreach (var hash in _hashes) animator.SetTrigger(hash);
            }
            _time = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            var resetAtStateTime = _onEvent.HasFlag(ApplyEvent.AtStateTime) && stateInfo.normalizedTime >= _atStateTime;
            var resetAtTime = _onEvent.HasFlag(ApplyEvent.AtTime) && _time >= _atTime;
            _time += _getDeltaTime() * animator.speed;
            if (resetAtStateTime || resetAtTime) {
                foreach (var hash in _hashes) animator.SetTrigger(hash);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) {
                foreach (var hash in _hashes) animator.SetTrigger(hash);
            }
        }
    }
}