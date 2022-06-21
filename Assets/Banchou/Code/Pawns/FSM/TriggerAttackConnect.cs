using System.Collections.Generic;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class TriggerAttackConnect : FSMBehaviour {
        [SerializeField, Tooltip("The trigger parameter to output to")]
        private List<OutputFSMParameter> _output;

        [SerializeField] private bool _onConfirm;
        [SerializeField] private bool _onBlock;
        [SerializeField] private bool _onGrab;
        
        [SerializeField, Tooltip("Pause the editor on confirmation")]
        private bool _breakOnSet;

        private GameState _state;
        private float _timeScale;
        private float _whenHit;
        private float _pauseTime;
        private bool _triggered;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            var pawnId = getPawnId();

            _state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);

            if (_output.Count > 0) {
                state.ObserveAttacksBy(pawnId)
                    .Where(attack => IsStateActive && 
                                     (_onConfirm && attack.HitStyle == HitStyle.Confirmed ||
                                      _onBlock && attack.HitStyle == HitStyle.Blocked ||
                                      _onGrab && attack.HitStyle == HitStyle.Grabbed))
                    .Subscribe(attack => {
                        _whenHit = attack.WhenHit;
                        _pauseTime = attack.PauseTime;
                        _triggered = true;
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (_triggered) {
                var timeElapsed = (_state.GetTime() - _whenHit) * _timeScale;
                if (timeElapsed > _pauseTime) {
                    _triggered = false;
                    if (_breakOnSet) {
                        Debug.Break();
                    }
                    _output.ApplyAll(animator);
                }
            }
        }
    }
}