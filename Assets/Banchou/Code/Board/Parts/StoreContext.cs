using System;
using UniRx;
using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou {
    public class StoreContext : MonoBehaviour, IContext {
        [SerializeField] private GameStateStore _store = null;

        private GameActions _gameActions;

        public void Construct() {
            _gameActions = new GameActions((GetTime)GetLocalTime);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind<GetState>(_store.GetState)
                .Bind<IObservable<GameState>>(_store.ObserveState())
                .Bind<Dispatcher>(_store.Dispatch)
                .Bind<ProcessStateActions>(_store.ProcessStateActions)
                .Bind<GetTime>(GetLocalTime)
                .Bind<GetDeltaTime>(GetLocalDeltaTime);
        }

        private void LateUpdate() {
            _store.ProcessStateActions();
        }

        private float GetLocalTime() {
            return Time.fixedTime;
        }

        private float GetLocalDeltaTime() {
            return Time.fixedDeltaTime;
        }
    }
}