using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class PawnRootMotion : FSMBehaviour {
        [SerializeField] private bool _rootPosition = true;
        [SerializeField] private bool _rootRotation = false;
        private GameState _state;
        private PawnSpatial _spatial;
        private Animator _animator;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => {
                    _spatial = spatial;
                })
                .AddTo(this);
        }
        
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_rootPosition) {
                _spatial.Move(animator.deltaPosition, _state.GetTime());
            }
        
            if (_rootRotation) {
                _spatial.Rotate(
                    animator.deltaRotation * _spatial.Forward,
                    _state.GetTime()
                );
            }
        }
    }
}