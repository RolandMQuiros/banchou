using System;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SetFSMParameters : FSMBehaviour {
        [Serializable]
        private class SetEvent {
            [SerializeField] public StateEvent _onEvent;
            [SerializeField, Range(0f, 1f)] public float _stateTime;
            [SerializeField] public FSMParameterCondition[] _conditions;
            [SerializeField] public List<OutputFSMParameter> _output;
            
            private bool _appliedAtTime;

            public void Apply(Animator animator, ref FSMUnit fsmUnit) {
                if (!_onEvent.HasFlag(fsmUnit.StateEvent)) return;
                switch (fsmUnit.StateEvent) {
                    case StateEvent.OnEnter:
                    case StateEvent.OnExit:
                        if (_conditions.Evaluate(animator)) {
                            _output.ApplyAll(animator);
                        }
                        _appliedAtTime = false;
                        break;
                    case StateEvent.OnUpdate:
                        if (!_appliedAtTime &&
                            fsmUnit.StateInfo.normalizedTime >= _stateTime &&
                            _conditions.Evaluate(animator))
                        {
                            _output.ApplyAll(animator);
                            _appliedAtTime = true;
                        }    
                        break;
                }
            }
        }

        [SerializeField] private SetEvent[] _setEvents;
        
        protected override void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) {
            for (int i = 0; i < _setEvents.Length; i++) _setEvents[i].Apply(animator, ref fsmUnit);
        }
    }
}