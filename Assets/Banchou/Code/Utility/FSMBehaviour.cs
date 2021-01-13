using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Animations;
using UniRx;
using System.Collections;

namespace Banchou {
    public class FSMBehaviour : StateMachineBehaviour, ICollection<IDisposable> {
        public bool IsStateActive { get; private set; }

        private List<IDisposable> _streams = new List<IDisposable>();
        protected ICollection<IDisposable> Streams { get => _streams; }

        protected struct FSMUnit {
            public AnimatorStateInfo StateInfo;
            public float DeltaTime;
            public int LayerIndex;
            public AnimatorControllerPlayable Playable;
        }

        protected Subject<FSMUnit> ObserveStateEnter = new Subject<FSMUnit>();
        protected Subject<FSMUnit> ObserveStateUpdate = new Subject<FSMUnit>();
        protected Subject<FSMUnit> ObserveStateExit = new Subject<FSMUnit>();

        private float _previousTime = 0f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            IsStateActive = true;
            ObserveStateEnter.OnNext(new FSMUnit {
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
                Playable = playable
            });
            _previousTime = 0f;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            IsStateActive = false;
            ObserveStateExit.OnNext(new FSMUnit {
                StateInfo = stateInfo,
                LayerIndex = layerIndex,
                Playable = playable
            });
            _previousTime = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable playable) {
            var deltaTime = (stateInfo.normalizedTime - _previousTime) * stateInfo.length; // doesn't work
            ObserveStateUpdate.OnNext(new FSMUnit {
                StateInfo = stateInfo,
                DeltaTime = deltaTime,
                LayerIndex = layerIndex,
                Playable = playable
            });
            _previousTime = stateInfo.normalizedTime;
        }

        private void OnDisable() {
            IsStateActive = false;
        }

        private void OnDestroy() {
            _streams.ForEach(s => s.Dispose());
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