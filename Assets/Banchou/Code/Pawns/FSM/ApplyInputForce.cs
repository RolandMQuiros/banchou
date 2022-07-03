using System;
using Banchou.Player;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyInputForce : FSMBehaviour {
        [Serializable]
        private class AppliedForce {
            [SerializeField] public StateEvent _onEvent = StateEvent.OnUpdate;
            [SerializeField] public ForceMode _forceMode = ForceMode.Force;
            [SerializeField] public FSMParameterField<FloatFSMParameter> _inputForce;
            
            public void Apply(
                StateEvent onEvent, Animator animator, Rigidbody rigidbody, PlayerInputState input, float timeScale
            ) {
                if (!_onEvent.HasFlag(onEvent)) return;
                
                var inputForce = _inputForce.GetFloat(animator);
                if (!Mathf.Approximately(inputForce, 0f) && input.Direction != Vector3.zero) {
                    rigidbody.AddForce(timeScale * input.Direction * inputForce, _forceMode);
                }
            }
        }

        [SerializeField] private AppliedForce[] _forces;

        private Rigidbody _rigidbody;
        private PlayerInputState _input;
        private float _timeScale;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Rigidbody rigidbody
        ) {
            var pawnId = getPawnId();
            _rigidbody = rigidbody;
            state.ObservePawnInput(pawnId)
                .CatchIgnoreLog()
                .Subscribe(input => _input = input)
                .AddTo(this);
            state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);
        }

        protected override void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) {
            for (var i = 0; i < _forces.Length; i++) {
                _forces[i].Apply(fsmUnit.StateEvent, animator, _rigidbody, _input, _timeScale);
            }
        }
    }
}