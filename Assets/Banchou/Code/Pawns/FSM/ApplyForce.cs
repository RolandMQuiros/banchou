using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.Serialization;

namespace Banchou.Pawn.FSM {
    public class ApplyForce : FSMBehaviour {
        [Serializable, Flags] private enum ApplyEvent { OnEnter = 1, OnUpdate = 2, OnExit = 4 }

        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.OnEnter | ApplyEvent.OnUpdate | ApplyEvent.OnExit;
        [SerializeField] private List<FSMParameterCondition> _conditions;
        [SerializeField] private ForceMode _forceMode = ForceMode.Force;
        [SerializeField] private Vector3InputFSMParameter _force;
        [SerializeField] private Vector3InputFSMParameter _relativeForce;

        private Rigidbody _rigidbody;
        private float _timeScale;
        private float _currentTime;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody, Animator animator) {
            _rigidbody = rigidbody;
            state.ObservePawnTimeScale(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);
        }

        private void Apply(Animator animator) {
            for (int i = 0; i < _conditions.Count; i++) {
                if (!_conditions[i].Evaluate(animator)) {
                    return;
                }
            }

            var force = _force.Get(animator);
            var relativeForce = _relativeForce.Get(animator);

            if (force != Vector3.zero) {
                _rigidbody.AddForce(force * _timeScale, _forceMode);
            }

            if (relativeForce != Vector3.zero) {
                _rigidbody.AddRelativeForce(relativeForce * _timeScale, _forceMode);
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) Apply(animator);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (!Mathf.Approximately(_currentTime, Time.fixedTime) && _onEvent.HasFlag(ApplyEvent.OnUpdate)) {
                _currentTime = Time.fixedTime;
                Apply(animator);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) Apply(animator);
        }
    }
}