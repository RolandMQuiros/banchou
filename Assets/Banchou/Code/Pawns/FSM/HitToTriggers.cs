using System.Linq;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitToTriggers : FSMBehaviour {
        [SerializeField, Tooltip("The trigger parameter to output to")]
        private string[] _outputParameters;

        [SerializeField] private bool _onConfirm;
        [SerializeField] private bool _onBlock;
        
        [SerializeField, Tooltip("Pause the editor on confirmation")]
        private bool _breakOnSet;
        private int[] _outputHashes;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _outputHashes = _outputParameters.Select(Animator.StringToHash)
                .Where(hash => hash != 0)
                .ToArray();
            if (_outputHashes.Length > 0) {
                state.ObserveLastHitChanges(getPawnId())
                    .Where(attack => IsStateActive && 
                                     (_onConfirm && !attack.Blocked || _onBlock && attack.Blocked))
                    .Subscribe(_ => {
                        foreach (var t in _outputHashes) animator.SetTrigger(t);
                        if (_breakOnSet) {
                            Debug.Break();
                        }
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach (var t in _outputHashes) animator.ResetTrigger(t);
        }
    }
}