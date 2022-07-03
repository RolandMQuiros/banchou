using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class ApplyForce : FSMBehaviour {
        [Serializable]
        private class AppliedForce {
            [SerializeField] public StateEvent _onEvent = StateEvent.OnEnter | StateEvent.OnUpdate | StateEvent.OnExit;
            [SerializeField] public List<FSMParameterCondition> _conditions;
            [SerializeField] public ForceMode _forceMode = ForceMode.Force;
            [SerializeField] public Vector3FSMParameterField _force;
            [SerializeField] public Vector3FSMParameterField _relativeForce;

            public void Apply(StateEvent onEvent, Animator animator, Rigidbody rigidbody, float timeScale) {
                if (!_onEvent.HasFlag(onEvent)) return;

                for (int i = 0; i < _conditions.Count; i++) {
                    if (!_conditions[i].Evaluate(animator)) {
                        return;
                    }
                }

                var force = _force.Get(animator);
                var relativeForce = _relativeForce.Get(animator);

                if (force != Vector3.zero) {
                    rigidbody.AddForce(force * timeScale, _forceMode);
                }

                if (relativeForce != Vector3.zero) {
                    rigidbody.AddRelativeForce(relativeForce * timeScale, _forceMode);
                }
            }
        }

        [SerializeField] private AppliedForce[] _forces;

        private Rigidbody _rigidbody;
        private float _timeScale;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody rigidbody, Animator animator) {
            _rigidbody = rigidbody;
            state.ObservePawnTimeScale(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);
        }

        protected override void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) {
            for (var i = 0; i < _forces.Length; i++)
                _forces[i].Apply(fsmUnit.StateEvent, animator, _rigidbody, _timeScale);
        }
    }
}