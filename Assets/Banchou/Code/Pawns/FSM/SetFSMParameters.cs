using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetFSMParameters : FSMBehaviour {
        [Flags] private enum ApplyEvent { OnEnter = 1, AtStateTime = 2, AtTime = 4, OnExit = 8 }
        [SerializeField] private ApplyEvent _onEvent;
        [SerializeField] private float _stateTime;
        [SerializeField] private float _time;
        [SerializeField] private List<ApplyFSMParameter> _output;

        private float _deltaTime;
        private float _timer;

        private bool _appliedAtStateTime;
        private bool _appliedAtTime;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            state.ObservePawnDeltaTime(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(deltaTime => _deltaTime = deltaTime)
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) {
                _output.ApplyAll(animator);
            }
            _timer = 0f;
            _appliedAtTime = false;
            _appliedAtStateTime = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.AtStateTime) && !_appliedAtStateTime &&
                stateInfo.normalizedTime >= _stateTime) {
                _output.ApplyAll(animator);
                _appliedAtStateTime = true;
            }
            
            _timer += _deltaTime;
            if (_onEvent.HasFlag(ApplyEvent.AtTime) && !_appliedAtTime && _timer >= _time) {
                _output.ApplyAll(animator);
                _appliedAtTime = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) {
                _output.ApplyAll(animator);
            }
            _timer = 0f;
            _appliedAtTime = false;
            _appliedAtStateTime = false;
        }
    }
}