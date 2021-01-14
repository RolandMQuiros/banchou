using UnityEngine;

namespace Banchou.Pawn.Part {
    public class PawnAnimator : MonoBehaviour {
        private Animator _animator;
        private Dispatcher _dispatch;
        private PawnActions _pawnActions;

        public void Construct(
            Animator animator,
            Dispatcher dispatch,
            PawnActions pawnActions
        ) {
            _animator = animator;
            _dispatch = dispatch;
            _pawnActions = pawnActions;
        }

        private void LateUpdate() {
            _dispatch(_pawnActions.Animated(_animator));
        }
    }
}