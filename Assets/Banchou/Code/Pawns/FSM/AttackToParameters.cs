using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class AttackToParameters : FSMBehaviour {
        [Serializable]
        private class AttackEvent : ISerializationCallbackReceiver {
            [SerializeField, HideInInspector] private string _name;
            [SerializeField] private HitStyle _hitStyle;
            [SerializeField] private OutputFSMParameter[] _output;
            [SerializeField] private bool _break;

            public void Apply(Animator animator, HitStyle incomingHitStyle) {
                if (_hitStyle == HitStyle.None || _hitStyle == incomingHitStyle) {
                    _output.ApplyAll(animator);
                }
            }

            public void OnBeforeSerialize() {
                _name = _hitStyle == HitStyle.None ? "On Connecting Attacks" : $"On {_hitStyle} Attacks";
            }

            public void OnAfterDeserialize() { }
        }

        [SerializeField] private AttackEvent[] _attackEvents;
        [SerializeField] private FloatFSMParameter[] _attackPauseOutput;
        
        private GameState _state;
        private float _timeScale;
        private float _whenHit;
        private float _pauseTime;
        private HitStyle _hitStyle;
        private bool _triggered;
        
        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            var pawnId = getPawnId();

            _state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);

            _state.ObserveAttacksBy(pawnId)
                .Where(_ => IsStateActive)
                .Subscribe(attack => {
                    _whenHit = attack.LastUpdated;
                    _pauseTime = attack.PauseTime;
                    _hitStyle = attack.HitStyle;
                    _triggered = true;
                })
                .AddTo(this);
        }

        protected override void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) {
            base.OnAllStateEvents(animator, ref fsmUnit);
            var now = _state.GetTime();
            
            if (_triggered) {
                var timeElapsed = (now - _whenHit) * _timeScale;
                if (timeElapsed > _pauseTime) {
                    _triggered = false;
                    foreach (var attackEvent in _attackEvents) attackEvent.Apply(animator, _hitStyle);
                }
                _attackPauseOutput.ApplyAll(animator, Mathf.Clamp01((now - _whenHit) * _timeScale / _pauseTime));
            }
        }
    }
}