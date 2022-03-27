using MessagePack;
using MessagePack.Resolvers;

using UnityEngine;
using Banchou.DependencyInjection;
using Banchou.Serialization.Resolvers;

namespace Banchou {
    public class StoreContext : MonoBehaviour, IContext {
        [SerializeField] private GameStateStore _store;
        [SerializeField] private GameState _state;
        private MessagePackSerializerOptions _messagePackOptions;

        public void Construct() {
            _state = _store.State;
            _state.SetVersion(_store.Version);
            _messagePackOptions = MessagePackSerializerOptions
                .Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(
                    CompositeResolver.Create(
                        BanchouResolver.Instance,
                        MessagePack.Unity.UnityResolver.Instance,
                        StandardResolver.Instance
                    )
                );
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_store.State)
                .Bind(_messagePackOptions);
        }

        private void FixedUpdate() {
            _store.State.UpdateLocalTime(Time.fixedDeltaTime);
        }
    }
}