using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class HitToParameters : FSMBehaviour {
        [Serializable]
        private class HitEvent : ISerializationCallbackReceiver {
            [SerializeField, HideInInspector] private string _name;
            [SerializeField] private HitStyle _hitStyle;
            [SerializeField] private OutputFSMParameter[] _output;

            public void OnHit(Animator animator, HitState hit) {
                if (_hitStyle == HitStyle.None || hit.Style == _hitStyle) {
                    _output.ApplyAll(animator);
                }
            }

            public void OnBeforeSerialize() {
                _name = _hitStyle == HitStyle.None ? "On Connected Hits" : $"On {_hitStyle} Hits";
            }

            public void OnAfterDeserialize() { }
        }

        [SerializeField] private HitEvent[] _hitEvents;
        [SerializeField] private FloatFSMParameter[] _hitPauseOutput;
        [SerializeField] private FloatFSMParameter[] _hitStunOutput;
        
        private GameState _state;
        private float _timeScale;
        private float _whenHit;
        private float _pauseTime;
        private float _stunTime;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            var pawnId = getPawnId();
            
            _state.ObserveHitsOn(pawnId)
                .Where(_ => IsStateActive)
                .Subscribe(hit => {
                    foreach (var hitEvent in _hitEvents) hitEvent.OnHit(animator, hit);
                    _whenHit = hit.LastUpdated;
                    _pauseTime = hit.PauseTime;
                    _stunTime = hit.StunTime;
                })
                .AddTo(this);
            
            _state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);
        }

        protected override void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) {
            var now = _state.GetTime();
            var timeElapsed = (now - _whenHit) * _timeScale;

            if (_hitPauseOutput.Length > 0) {
                _hitPauseOutput.ApplyAll(animator, Mathf.Clamp01(timeElapsed / _pauseTime));
            }

            if (_hitStunOutput.Length > 0) {
                if (timeElapsed < _pauseTime + _stunTime) {
                    var stunTime = timeElapsed - _pauseTime;
                    if (stunTime < 0f) {
                        _hitStunOutput.ApplyAll(animator, 0f);
                    }
                    else {
                        _hitStunOutput.ApplyAll(animator, Mathf.Clamp01(stunTime / _stunTime));
                    }
                }
                else {
                    _hitStunOutput.ApplyAll(animator, 1f);
                }
            }
        }
    }
}