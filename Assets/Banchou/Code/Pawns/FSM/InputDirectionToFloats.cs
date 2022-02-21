using Banchou.Player;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class InputDirectionToFloats : FSMBehaviour {
        [SerializeField] private FSMParameter _magnitude = new(AnimatorControllerParameterType.Float);
        [SerializeField] private FSMParameter _forward = new(AnimatorControllerParameterType.Float);
        [SerializeField] private FSMParameter _right = new(AnimatorControllerParameterType.Float);

        private PlayerInputState _input;
        private PawnSpatial _spatial;
        public void Construct(GameState state, GetPawnId getPawnId) {
            state.ObservePawnInput(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(player => _input = player)
                .AddTo(this);
            state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            if (_input == null) return;
            
            if (_magnitude.IsSet) {
                animator.SetFloat(_magnitude.Hash, _input.Direction.magnitude);
            }

            if (_forward.IsSet) {
                animator.SetFloat(_forward.Hash, Vector3.Dot(_input.Direction, _spatial.Forward));
            }

            if (_right.IsSet) {
                animator.SetFloat(_right.Hash, Vector3.Dot(_input.Direction, _spatial.Right));
            }
        }
    }   
}
