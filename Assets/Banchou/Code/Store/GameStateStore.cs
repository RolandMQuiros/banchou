using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        private Queue<object> _buffer = new Queue<object>();
        private List<object> _actions = new List<object>();
        private GameState _state = new GameState();
        private long _processCount = 0L;

        public void Dispatch(in object action) {
            _buffer.Enqueue(action);
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

            // Load the buffer into the actions list
            _actions.Add(new StateAction.StartProcess {
                ProcessCount = _processCount
            });
            while (_buffer.Count > 0) {
                _actions.Add(_buffer.Dequeue());
            }
            _actions.Add(new StateAction.EndProcess {
                ProcessCount = _processCount
            });

            // Processing can cause more additions to the queue, so we need to juggle collections
            // so we don't skip anything
            _state.Process(_actions);
            _actions.Clear();
        }
    }

    namespace StateAction {
        public struct StartProcess {
            public long ProcessCount;
        }

        public struct EndProcess {
            public long ProcessCount;
        }
    }

    public delegate void Dispatcher(in object action);
    public delegate GameState GetState();
    public delegate void ProcessStateActions();
}