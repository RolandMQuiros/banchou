using System;
using Banchou.Utility;
using UnityEngine;


namespace Banchou.Pawn.FSM {
    public class RaiseAnimatorEvent : FSMBehaviour {
        [Flags]
        private enum ApplyEvent { OnEnter, OnStateTime, OnExit }
        [SerializeField] private ApplyEvent _onEvent;
        [SerializeField] private string _eventName;
        [SerializeField] private float _raiseAtStateTime;
        [SerializeField] private bool _clampStateTime;

        private AnimatorUnityEvents _events;
        private bool _raised;
        
        public void Construct(AnimatorUnityEvents events) {
            _events = events;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnEnter) && !string.IsNullOrEmpty(_eventName)) {
                _events.Raise(_eventName);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnStateTime) && !_raised && !string.IsNullOrEmpty(_eventName)) {
                var stateTime = stateInfo.normalizedTime;
                if (_clampStateTime) {
                    stateTime %= 1f;
                }

                if (stateTime >= _raiseAtStateTime) {
                    _events.Raise(_eventName);
                    _raised = true;
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnExit) && !string.IsNullOrEmpty(_eventName)) {
                _events.Raise(_eventName);
            }
        }
    }
}