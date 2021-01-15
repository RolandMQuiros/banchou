using System;
using UniRx;
using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou {
    public class StoreContext : MonoBehaviour, IContext {
        [SerializeField] private GameStateStore _store = null;

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind<GameState>(_store.GetState())
                .Bind<IObservable<GameState>>(_store.ObserveState())
                .Bind<GetTime>(GetLocalTime)
                .Bind<GetDeltaTime>(GetLocalDeltaTime);
        }

        private void LateUpdate() {
            _store.GetState().Process();
        }

        private float GetLocalTime() {
            return Time.fixedTime;
        }

        private float GetLocalDeltaTime() {
            return Time.fixedDeltaTime;
        }
    }
}