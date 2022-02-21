using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class MoveToGrabAnchor : FSMBehaviour {
        private GameState _state;
        private int _pawnId;
        private PawnSpatial _spatial;
        private GrabState _grab;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _pawnId = getPawnId();
            _state.ObservePawnSpatial(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _state.ObserveGrabHoldsOn(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(grab => _grab = grab)
                .AddTo(this);
        }
        
        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_spatial != null && _grab?.Phase == GrabPhase.Held) {
                var now = _state.GetTime();
                _spatial.Move(_grab.AnchorPosition - _spatial.Position, now);
                animator.bodyRotation = _grab.AnchorRotation;
            }
        }
    }
}