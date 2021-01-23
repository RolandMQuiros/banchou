using System;
using UniRx;
using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou {
    public class StoreContext : MonoBehaviour, IContext {
        [SerializeField] private GameStateStore _store = null;
        [SerializeField] private GameState _state = null;

        public void Construct() {
            _state = _store.State;
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind<GameState>(_store.State)
                .Bind<GetTime>(GetLocalTime)
                .Bind<GetDeltaTime>(GetLocalDeltaTime);
        }

        private void FixedUpdate() {
            _store.State.SetLocalTime(Time.fixedUnscaledTime, Time.fixedUnscaledDeltaTime);
        }

        private float GetLocalTime() {
            return Time.fixedTime;
        }

        private float GetLocalDeltaTime() {
            return Time.fixedDeltaTime;
        }
    }
}