using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        private List<object> _actions = new List<object> { new StateAction.StartProcess() };
        private GameState _state = new GameState();
        private long _processCount = 0L;

        public void Dispatch(in object action) {
            _actions.Add(action);
        }

        public GameState GetState() {
            return _state;
        }

        public void ProcessStateActions() {
            _processCount++;
            _actions.Add(
                new StateAction.EndProcess {
                    ProcessCount = _processCount
                }
            );


            _state.Process(_actions);

            _actions.Clear();
            _actions.Add(
                new StateAction.StartProcess {
                    ProcessCount = _processCount
                }
            );
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