using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using UniRx;
using UnityEngine;

using Banchou.Network.Message;
using Banchou.Player;

namespace Banchou.Network.Part {
    public class NetworkClient : MonoBehaviour {
        private GameState _state;
        private EventBasedNetListener _eventListener;
        private NetManager _netManager;
        private MessagePackSerializerOptions _messagePackOptions;
        private NetPeer _server;

        public void Construct(
            GameState state,
            EventBasedNetListener eventListener,
            NetManager netManager,
            MessagePackSerializerOptions messagePackOptions
        ) {
            _state = state;
            _eventListener = eventListener;
            _netManager = netManager;
            _messagePackOptions = messagePackOptions;

            _eventListener.NetworkReceiveEvent += OnReceive;

            _state.Network.Observe()
                .Where(network => network.Mode == NetworkMode.Client)
                .Select(network => ( network.ServerIP, network.ServerPort ))
                .DistinctUntilChanged()
                .CatchIgnoreLog()
                .Subscribe(args => ConnectToServer(args.ServerIP, args.ServerPort))
                .AddTo(this);
        }

        private void OnDestroy() {
            _eventListener.NetworkReceiveEvent -= OnReceive;
        }

        private void ConnectToServer(string ip, int port) {
            if (_netManager.IsRunning) {
                _netManager.Stop();
            }
            _netManager.Start();

            var connectArgs = new NetDataWriter();
            connectArgs.Put(
                MessagePackSerializer.Serialize<ConnectClient>(
                    new ConnectClient {
                        ConnectionKey = "BanchouConnectionKey",
                        ClientConnectionTime = _state.LocalTime
                    },
                    _messagePackOptions
                )
            );

            _server = _netManager.Connect(ip, port, connectArgs);
            Debug.Log($"Connected to server at {_server.EndPoint}");
        }

        private void OnReceive(NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod) {
            var envelope = MessagePackSerializer.Deserialize<Envelope>(dataReader.GetRemainingBytes(), _messagePackOptions);

            // Deserialize payload
            switch (envelope.PayloadType) {
                case PayloadType.Connected: {
                    var connected = MessagePackSerializer.Deserialize<Connected>(envelope.Payload, _messagePackOptions);
                    _state.Network.ConnectedToServer(
                        clientNetworkId: connected.ClientNetworkId,
                        serverTimeOffset: CalculateTimeOffset(
                            connected.ClientTime,
                            connected.ServerReceiptTime,
                            connected.ServerTransmissionTime,
                            _state.LocalTime
                        )
                    );
                    _state.SyncGame(connected.State);
                } break;
                case PayloadType.InputUnit: {
                    var inputUnit = MessagePackSerializer.Deserialize<InputUnit>(envelope.Payload, _messagePackOptions);
                    // handle rollbacks
                } break;
                case PayloadType.TimeResponse: {
                    var response = MessagePackSerializer.Deserialize<TimeResponse>(envelope.Payload, _messagePackOptions);
                    _state.Network.UpdateServerTime(
                        serverTimeOffset: CalculateTimeOffset(
                            response.ClientTime,
                            response.ServerTime,
                            response.ServerTime,
                            _state.LocalTime
                        )
                    );
                } break;
                case PayloadType.Sync: {
                    var sync = MessagePackSerializer.Deserialize<GameState>(envelope.Payload, _messagePackOptions);
                    _state.SyncGame(sync);
                } break;
            }
        }

        private float CalculateTimeOffset (float originTime, float receiptTime, float transmissionTime, float now) {
            return ((receiptTime - originTime) - (now - transmissionTime)) / 2f;
        }
    }
}