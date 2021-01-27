using LiteNetLib;
using MessagePack;
using MessagePack.Resolvers;
using UniRx;
using UnityEngine;

using Banchou.DependencyInjection;
using Banchou.Serialization.Resolvers;

namespace Banchou.Network.Part {
    public class NetworkAgentContext : MonoBehaviour, IContext {
        [SerializeField] private NetworkState _network;
        private EventBasedNetListener _eventListener;
        private NetManager _netManager;
        private MessagePackSerializerOptions _messagePackOptions;

        public void Construct(GameState state) {
            _network = state.Network;
            _eventListener = new EventBasedNetListener();
            _netManager = new NetManager(_eventListener);
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

            state.ObserveNetwork()
                .CatchIgnoreLog()
                .Subscribe(network => {
                    _netManager.SimulateLatency = network.SimulateMinLatency > 0 || network.SimulateMaxLatency > 0;
                    _netManager.SimulationMinLatency = network.SimulateMinLatency;
                    _netManager.SimulationMaxLatency = network.SimulateMaxLatency;
                })
                .AddTo(this);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_eventListener)
                .Bind(_netManager)
                .Bind(_messagePackOptions);
        }

        private void Update() {
            if (_netManager.IsRunning) {
                _netManager.PollEvents();
            }
        }
    }
}