using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitConfirmToTrigger : FSMBehaviour {
        [SerializeField, Tooltip("The trigger parameter to output to")]
        private string _outputParameter;
        [SerializeField, Tooltip("Pause the editor on confirmation")]
        private bool _breakOnConfirm;

        private int _outputHash;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _outputHash = Animator.StringToHash(_outputParameter);
            if (_outputHash != default) {
                state.ObserveLastAttackChanges(getPawnId())
                    .Where(_ => IsStateActive)
                    .Subscribe(_ => {
                        animator.SetTrigger(_outputHash);
                        if (_breakOnConfirm) {
                            Debug.Break();
                        }
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.ResetTrigger(_outputHash);
        }
    }
}