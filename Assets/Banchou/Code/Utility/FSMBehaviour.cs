using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Banchou {
    public class FSMBehaviour : StateMachineBehaviour, ICollection<IDisposable> {
        [SerializeField, DevComment] private string _comments;

        protected bool IsStateActive => _activeStates.Count > 0;

        private readonly List<IDisposable> _streams = new();
        protected ICollection<IDisposable> Streams => _streams;

        [Serializable, Flags] protected enum StateEvent { OnEnter = 1, OnUpdate = 2, OnExit = 4 }
        
        protected struct FSMUnit {
            public StateEvent StateEvent;
            public AnimatorStateInfo StateInfo;
            public int LayerIndex;
        }

        private readonly Subject<FSMUnit> _stateEventSubject = new();
        
        protected IObservable<FSMUnit> ObserveStateEvents => _stateEventSubject;
        protected IObservable<FSMUnit> ObserveStateEnter => _stateEventSubject
            .Where(unit => unit.StateEvent == StateEvent.OnEnter);
        protected IObservable<FSMUnit> ObserveStateUpdate => _stateEventSubject
            .Where(unit => unit.StateEvent == StateEvent.OnUpdate);
        protected IObservable<FSMUnit> ObserveStateExit => _stateEventSubject
            .Where(unit => unit.StateEvent == StateEvent.OnExit);
        
        private readonly HashSet<int> _activeStates = new();
        private FSMUnit _unit;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _activeStates.Add(stateInfo.fullPathHash);
            _unit.StateEvent = StateEvent.OnEnter;
            _unit.StateInfo = stateInfo;
            _unit.LayerIndex = layerIndex;
            _stateEventSubject.OnNext(_unit);
            OnStateEnter(animator, ref _unit);
            OnAllStateEvents(animator, ref _unit);
        }
        
        protected virtual void OnStateEnter(Animator animator, ref FSMUnit fsmUnit) { }
        
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _unit.StateEvent = StateEvent.OnUpdate;
            _unit.StateInfo = stateInfo;
            _unit.LayerIndex = layerIndex;
            _stateEventSubject.OnNext(_unit);
            OnStateUpdate(animator, ref _unit);
            OnAllStateEvents(animator, ref _unit);
        }
        
        protected virtual void OnStateUpdate(Animator animator, ref FSMUnit fsmUnit) { }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _unit.StateEvent = StateEvent.OnExit;
            _unit.StateInfo = stateInfo;
            _unit.LayerIndex = layerIndex;
            _stateEventSubject.OnNext(_unit);
            OnStateExit(animator, ref _unit);
            _activeStates.Remove(stateInfo.fullPathHash);
            OnAllStateEvents(animator, ref _unit);
        }

        protected virtual void OnStateExit(Animator animator, ref FSMUnit fsmUnit) { }

        protected virtual void OnAllStateEvents(Animator animator, ref FSMUnit fsmUnit) { }

        private void OnDisable() {
            _activeStates.Clear();
        }

        protected virtual void OnDestroy() {
            for (int i = 0; i < _streams.Count; i++) {
                _streams[i].Dispose();
            }
            _streams.Clear();
        }

        #region ICollection<IDisposable> facade
        public int Count => ((ICollection<IDisposable>)_streams).Count;
        public bool IsReadOnly => ((ICollection<IDisposable>)_streams).IsReadOnly;
        public void Add(IDisposable item) => ((ICollection<IDisposable>)_streams).Add(item);
        public void Clear() => ((ICollection<IDisposable>)_streams).Clear();
        public bool Contains(IDisposable item) => ((ICollection<IDisposable>)_streams).Contains(item);
        public void CopyTo(IDisposable[] array, int arrayIndex) => ((ICollection<IDisposable>)_streams).CopyTo(array, arrayIndex);
        public bool Remove(IDisposable item) => ((ICollection<IDisposable>)_streams).Remove(item);
        public IEnumerator<IDisposable> GetEnumerator() => ((IEnumerable<IDisposable>)_streams).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_streams).GetEnumerator();
        #endregion
    }
}