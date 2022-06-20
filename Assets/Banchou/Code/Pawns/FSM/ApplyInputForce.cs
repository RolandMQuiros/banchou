using System;
using Banchou.Player;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Banchou.Pawn.FSM {
    public class ApplyInputForce : FSMBehaviour {
        [Serializable, Flags] private enum ApplyEvent { OnEnter = 1, OnUpdate = 2, OnExit = 4 }

        [SerializeField] private ApplyEvent _onEvent = ApplyEvent.OnUpdate;
        [SerializeField] private ForceMode _forceMode = ForceMode.Force;
        [SerializeField] private FSMReadParameter _inputForce = new(AnimatorControllerParameterType.Float);

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

        private void Apply(Animator animator) {
            var inputForce = _inputForce.GetFloat(animator);

            if (!Mathf.Approximately(inputForce, 0f) && _input.Direction != Vector3.zero) {
                _rigidbody.AddForce(_timeScale * _input.Direction * inputForce, _forceMode);
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) Apply(animator);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnUpdate)) Apply(animator);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) Apply(animator);
        }
    }
}