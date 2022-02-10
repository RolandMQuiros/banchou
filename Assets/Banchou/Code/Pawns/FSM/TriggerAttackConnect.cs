using System.Collections.Generic;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class TriggerAttackConnect : FSMBehaviour {
        [SerializeField, Tooltip("The trigger parameter to output to")]
        private List<ApplyFSMParameter> _output;

        [SerializeField] private bool _onConfirm;
        [SerializeField] private bool _onBlock;
        [SerializeField] private bool _onGrab;
        
        [SerializeField, Tooltip("Pause the editor on confirmation")]
        private bool _breakOnSet;

        private GameState _state;
        private float _pauseTimer = -1f;
        private bool _triggered = false;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            if (_output.Count > 0) {
                _state.ObserveAttackConnects(getPawnId())
                    .Where(attack => IsStateActive && 
                                     (_onConfirm && attack.Confirmed ||
                                      _onBlock && attack.Blocked ||
                                      _onGrab && attack.IsGrab))
                    .Subscribe(attack => {
                        _pauseTimer = attack.PauseTime;
                        _triggered = true;
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_triggered) {
                _pauseTimer -= _state.GetDeltaTime();
                if (_pauseTimer <= 0f) {
                    _triggered = false;
                    if (_breakOnSet) {
                        Debug.Break();
                    }
                    _output.ForEach(parameter => parameter.Apply(animator));
                }
            }
        }
    }
}