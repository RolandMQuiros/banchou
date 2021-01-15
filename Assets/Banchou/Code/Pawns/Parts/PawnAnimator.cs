using UnityEngine;

namespace Banchou.Pawn.Part {
    public class PawnAnimator : MonoBehaviour {
        private PawnState _pawn;
        private Animator _animator;
        private GetTime _getTime;

        public void Construct(
            PawnState pawn,
            Animator animator,
            GetTime getTime
        ) {
            _pawn = pawn;
            _animator = animator;
            _getTime = getTime;
        }

        private void LateUpdate() {
            _pawn.Animated(_animator, _getTime());
        }
    }
}