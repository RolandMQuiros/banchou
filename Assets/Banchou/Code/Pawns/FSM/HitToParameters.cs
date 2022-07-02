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
            [SerializeField] private FloatFSMParameter[] _hitPause;
            [SerializeField] private FloatFSMParameter[] _hitStun;

            private float _whenHit;
            private float _pauseTime;
            private float _stunTime;
            
            public void OnHit(Animator animator, HitState hit) {
                if (_hitStyle == HitStyle.None || hit.Style == _hitStyle) {
                    _whenHit = hit.LastUpdated;
                    _pauseTime = hit.PauseTime;
                    _stunTime = hit.StunTime;
                    _output.ApplyAll(animator);
                }
            }

            public void OnUpdate(Animator animator, float timeScale, float when) {
                var timeElapsed = (when - _whenHit) * timeScale;

                if (_hitPause.Length > 0) {
                    _hitPause.ApplyAll(animator, Mathf.Clamp01(timeElapsed / _pauseTime));
                }

                if (_hitStun.Length > 0) {
                    if (timeElapsed < _pauseTime + _stunTime) {
                        var stunTime = timeElapsed - _pauseTime;
                        if (stunTime < 0f) {
                            _hitStun.ApplyAll(animator, 0f);
                        }
                        else {
                            _hitStun.ApplyAll(animator, Mathf.Clamp01(stunTime / _stunTime));
                        }
                    }
                    else {
                        _hitStun.ApplyAll(animator, 1f);
                    }
                }
            }

            public void OnBeforeSerialize() {
                _name = _hitStyle == HitStyle.None ? "On Connected Hits" : $"On {_hitStyle} Hits";
            }

            public void OnAfterDeserialize() { }
        }

        [SerializeField] private HitEvent[] _hitEvents;
        
        private GameState _state;
        private float _timeScale;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            var pawnId = getPawnId();
            
            _state.ObserveHitsOn(pawnId)
                .Where(_ => IsStateActive)
                .Subscribe(hit => { foreach (var hitEvent in _hitEvents) hitEvent.OnHit(animator, hit); })
                .AddTo(this);
            
            _state.ObservePawnTimeScale(pawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => _timeScale = timeScale)
                .AddTo(this);
        }

        protected override void OnAllStateEvents(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnAllStateEvents(animator, stateInfo, layerIndex);
            var now = _state.GetTime();
            foreach (var hitEvent in _hitEvents) hitEvent.OnUpdate(animator, _timeScale, now);
        }
    }
}