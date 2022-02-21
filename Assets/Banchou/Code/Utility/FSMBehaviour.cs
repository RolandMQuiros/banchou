using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UniRx;
using System.Collections;

namespace Banchou {
    public class FSMBehaviour : StateMachineBehaviour, ICollection<IDisposable> {
        [SerializeField, DevComment] private string _comments;
        
        public bool IsStateActive => _activeStates.Count > 0;

        private readonly List<IDisposable> _streams = new();
        protected ICollection<IDisposable> Streams => _streams;

        protected struct FSMUnit {
            public AnimatorStateInfo StateInfo;
            public int LayerIndex;
        }

        protected readonly Subject<FSMUnit> ObserveStateEnter = new();
        protected readonly Subject<FSMUnit> ObserveStateUpdate = new();
        protected readonly Subject<FSMUnit> ObserveStateExit = new();
        private readonly HashSet<int> _activeStates = new();
        private FSMUnit _unit;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _activeStates.Add(stateInfo.fullPathHash);
            _unit.StateInfo = stateInfo;
            _unit.LayerIndex = layerIndex;
            ObserveStateEnter.OnNext(_unit);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _unit.StateInfo = stateInfo;
            _unit.LayerIndex = layerIndex;
            ObserveStateExit.OnNext(_unit);
            _activeStates.Remove(stateInfo.fullPathHash);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            _unit.StateInfo = stateInfo;
            _unit.LayerIndex = layerIndex;
            ObserveStateUpdate.OnNext(_unit);
        }

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