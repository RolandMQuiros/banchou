using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        private List<object> _buffer = new List<object>();
        private List<object> _actions = new List<object>();
        private GameState _state = new GameState();
        private long _processCount = 0L;

        public void Dispatch(in object action) {
            _buffer.Add(action);
        }

        public GameState GetState() {
            return _state;
        }

        public IObservable<GameState> ObserveState() {
            return Observable.FromEvent<GameState>(
                h => _state.Changed += h,
                h => _state.Changed -= h
            ).StartWith(_state);
        }

        public void ProcessStateActions() {
            _processCount++;

            // Swap the actions and buffer lists
            var swap = _actions;
            _actions = _buffer;
            _buffer = swap;

            // Processing can cause more additions to the queue, so we need to juggle collections
            // so we don't skip anything
            _state.Process(_actions);
            _actions.Clear();
        }
    }

    public delegate void Dispatcher(in object action);
    public delegate GameState GetState();
    public delegate void ProcessStateActions();
}