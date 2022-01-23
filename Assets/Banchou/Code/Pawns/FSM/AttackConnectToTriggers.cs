using System.Collections;
using System.Linq;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class AttackConnectToTriggers : FSMBehaviour {
        [SerializeField, Tooltip("The trigger parameter to output to")]
        private string[] _outputParameters;

        [SerializeField] private bool _onConfirm;
        [SerializeField] private bool _onBlock;
        
        [SerializeField, Tooltip("Pause the editor on confirmation")]
        private bool _breakOnSet;

        private GameState _state;
        private int[] _outputHashes;
        private float _pauseTimer = -1f;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            _outputHashes = _outputParameters.Select(Animator.StringToHash)
                .Where(hash => hash != 0)
                .ToArray();
            if (_outputHashes.Length > 0) {
                _state.ObserveAttackConnects(getPawnId())
                    .Where(attack => IsStateActive && 
                                     (_onConfirm && attack.Confirmed || _onBlock && attack.Blocked))
                    .Subscribe(attack => {
                        _pauseTimer = attack.PauseTime;
                    })
                    .AddTo(this);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_pauseTimer > 0f) {
                _pauseTimer -= _state.GetDeltaTime();
                if (_pauseTimer <= 0f) {
                    foreach (var t in _outputHashes) animator.SetTrigger(t);
                    if (_breakOnSet) {
                        Debug.Break();
                    }
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach (var t in _outputHashes) animator.ResetTrigger(t);
        }
    }
}