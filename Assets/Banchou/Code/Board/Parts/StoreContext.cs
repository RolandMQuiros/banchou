using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou {
    public class StoreContext : MonoBehaviour, IContext {
        [SerializeField] private GameStateStore _store = null;

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind<GetState>(_store.GetState)
                .Bind<Dispatcher>(_store.Dispatch)
                .Bind<ProcessStateActions>(_store.ProcessStateActions)
                .Bind<GetTime>(() => Time.fixedUnscaledTime)
                .Bind<GetDeltaTime>(() => Time.fixedUnscaledDeltaTime);
        }

        private void LateUpdate() {
            _store.ProcessStateActions();
        }
    }
}