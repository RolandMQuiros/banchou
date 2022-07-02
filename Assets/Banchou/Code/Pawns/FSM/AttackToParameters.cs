using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Banchou.Pawn.FSM {
    public class AttackToParameters : FSMBehaviour {
        [Serializable]
        private class AttackEvent : ISerializationCallbackReceiver {
            [SerializeField, HideInInspector] private string _name;
            [SerializeField] private HitStyle _hitStyle;
            [SerializeField] private OutputFSMParameter[] _output;
            [SerializeField] private bool _break;
            [SerializeField] private FloatFSMParameter[] _attackPause;
            
            private float _whenHit;
            private float _pauseTime;
            private bool _triggered;
            
            public void OnAttack(AttackState attack) {
                if (_hitStyle != HitStyle.None && attack.HitStyle != _hitStyle) return;
                _whenHit = attack.WhenHit;
                _pauseTime = attack.PauseTime;
                _triggered = true;
            }

            public void OnUpdate(Animator animator, float timeScale, float when) {
                if (_triggered) {
                    var timeElapsed = (when - _whenHit) * timeScale;
                    if (timeElapsed > _pauseTime) {
                        _triggered = false;
                        _output.ApplyAll(animator);
                    }
                    _attackPause.ApplyAll(animator, Mathf.Clamp01(timeElapsed / _pauseTime));
                }
            }
            
            public void OnBeforeSerialize() {
                _name = _hitStyle == HitStyle.None ? "On Connecting Attacks" : $"On {_hitStyle} Attacks";
            }
            public void OnAfterDeserialize() { }
        }

        [SerializeField] private AttackEvent[] _attackEvents;
        
        private GameState _state;
        private float _timeScale;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            var pawnId = getPawnId();

            _state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);

            _state.ObserveAttacksBy(pawnId)
                .Where(_ => IsStateActive)
                .Subscribe(attack => { foreach (var t in _attackEvents) t.OnAttack(attack); })
                .AddTo(this);
        }

        protected override void OnAllStateEvents(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnAllStateEvents(animator, stateInfo, layerIndex);
            foreach (var t in _attackEvents) t.OnUpdate(animator, _timeScale, _state.GetTime());
        }
    }
}