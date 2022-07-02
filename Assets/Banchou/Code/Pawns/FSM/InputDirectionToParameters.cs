using Banchou.Player;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class InputDirectionToParameters : FSMBehaviour {
        [SerializeField] private FloatFSMParameter _magnitude;
        [SerializeField] private FloatFSMParameter _forward;
        [SerializeField] private FloatFSMParameter _right;

        private PlayerInputState _input;
        private PawnSpatial _spatial;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            state.ObservePawnInput(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(player => _input = player)
                .AddTo(this);
            state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
        }

        private void Apply(Animator animator) {
            if (_input == null) return;
            _magnitude.Apply(animator, _input.Direction.magnitude);
            _forward.Apply(animator, Vector3.Dot(_input.Direction, _spatial.Forward));
            _right.Apply(animator, Vector3.Dot(_input.Direction, _spatial.Right));
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Apply(animator);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            Apply(animator);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);
            Apply(animator);
        }
    }   
}
