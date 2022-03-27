using System.Collections.Generic;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class TriggerHitConnect : FSMBehaviour {
        [SerializeField, Tooltip("The trigger parameter to output to")]
        private List<ApplyFSMParameter> _output;

        [SerializeField] private bool _onConfirm;
        [SerializeField] private bool _onBlock;
        [SerializeField] private bool _onGrab;
        
        [SerializeField, Tooltip("Pause the editor on confirmation")]
        private bool _breakOnSet;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            if (_output.Count > 0) {
                state.ObserveHitsOn(getPawnId())
                    .Where(attack => IsStateActive && 
                                     (_onConfirm && attack.Style == HitStyle.Confirmed ||
                                      _onBlock && attack.Style == HitStyle.Blocked ||
                                      _onGrab && attack.Style == HitStyle.Grabbed))
                    .Subscribe(_ => {
                        if (_breakOnSet) {
                            Debug.Break();
                        }
                        _output.ApplyAll(animator);
                    })
                    .AddTo(this);
            }
        }
    }
}