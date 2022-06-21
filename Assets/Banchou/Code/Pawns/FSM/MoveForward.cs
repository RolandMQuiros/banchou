using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class MoveForward : PawnFSMBehaviour {
        [SerializeField] private InputFSMParameter _speed = new(AnimatorControllerParameterType.Float);
        [SerializeField, Range(0, 1f)] private float _startTime;
        [SerializeField, Range(0, 1f)] private float _endTime = 1f;
        [SerializeField] private List<FloatFSMParameter> _speedOutput;

        private PawnSpatial _spatial;

        public void Construct(GameState state, GetPawnId getPawnId) {
            ConstructCommon(state, getPawnId);
            state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
        }
        
        private void Apply(Animator animator) {
            var speed = _speed.GetFloat(animator);
            _spatial.Move(_spatial.Forward * speed * DeltaTime, StateTime);
            _speedOutput.ForEach(p => p.Apply(animator, speed));
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (_startTime <= 0f) {
                Apply(animator);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            Apply(animator);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (_endTime >= 1f) {
                Apply(animator);
            }
        }
    }
}