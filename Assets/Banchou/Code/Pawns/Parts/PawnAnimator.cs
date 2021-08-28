using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class PawnAnimator : MonoBehaviour {
        private GameState _state;
        private PawnState _pawn;
        private Animator _animator;
        private List<AnimatorControllerParameter> _cachedParameters;

        public void Construct(
            GameState state,
            PawnState pawn,
            Animator animator
        ) {
            _state = state;
            _pawn = pawn;
            _animator = animator;

            // Accessing Animator.parameters or Animator.GetParameter seems to generate garbage
            // so let's get this out of the way early
            _cachedParameters = animator.parameters.ToList();
        }

        private void OnAnimatorMove() {
            // Needed to enable script control of the animator
        }

        private void LateUpdate() {
            FrameData frame;
            _pawn.History.Push(out frame);

            frame.StartFrame(_animator.layerCount, _pawn.Spatial.Position, _pawn.Spatial.Forward);

            // Save layer values
            for (int layer = 0; layer < _animator.layerCount; layer++) {
                var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
                var nextStateInfo = _animator.GetNextAnimatorStateInfo(layer);
                var targetStateInfo = nextStateInfo.fullPathHash == 0 ? currentStateInfo : nextStateInfo;

                frame.SetLayerData(layer, targetStateInfo.fullPathHash, targetStateInfo.normalizedTime);
            }

            // Save parameter values
            for (int p = 0; p < _cachedParameters.Count; p++) {
                var parameter = _cachedParameters[p];
                switch (parameter.type) {
                    case AnimatorControllerParameterType.Float:
                        frame.SetFloat(parameter.nameHash, _animator.GetFloat(parameter.nameHash));
                        break;
                    case AnimatorControllerParameterType.Int:
                        frame.SetInt(parameter.nameHash, _animator.GetInteger(parameter.nameHash));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        frame.SetBool(parameter.nameHash, _animator.GetBool(parameter.nameHash));
                        break;
                }
            }

            frame.FinishFrame(_state.GetTime());
        }
    }
}