using System.Collections.Generic;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        private Queue<object> _actions = new Queue<object>();
        private GameState _state = new GameState();

        public void Dispatch(in object action) {
            _actions.Enqueue(action);
        }

        public GameState GetState() {
            return _state;
        }

        public void ProcessStateActions() {
            _state.Process(_actions);
            _actions.Clear();
        }
    }

    public delegate void Dispatcher(in object action);
    public delegate GameState GetState();
    public delegate void ProcessStateActions();
}