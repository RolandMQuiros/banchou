using System.Linq;
using System.Net;
using System.Collections.Generic;

using LiteNetLib;
using MessagePack;
using MessagePack.Resolvers;
using UniRx;
using UnityEngine;

using Banchou.Network.Message;
using Banchou.Player;
using Banchou.Serialization.Resolvers;

namespace Banchou.Network.Part {
    public class NetworkAgent : MonoBehaviour {
        private GameState _state;
        private NetworkState _network;
        private GetTime _getLocalTime;

        private EventBasedNetListener _eventListener = new EventBasedNetListener();
        private NetManager _netManager;
        private MessagePackSerializerOptions _messagePackOptions;
        private Dictionary<IPEndPoint, ConnectClient> _connectingClients = new Dictionary<IPEndPoint, ConnectClient>();

        private NetPeer _server;
        private Dictionary<int, NetPeer> _clients = new Dictionary<int, NetPeer>();

        public void Construct(GameState state, GetTime getLocalTime) {
            _state = state;
            _network = state.Network;
            _getLocalTime = getLocalTime;
            _eventListener = new EventBasedNetListener();
            _netManager = new NetManager(_eventListener);
            _messagePackOptions = MessagePackSerializerOptions
                .Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(CompositeResolver.Create(
                    BanchouResolver.Instance,
                    MessagePack.Unity.UnityResolver.Instance,
                    StandardResolver.Instance
                ));

            _eventListener.ConnectionRequestEvent += OnConnectionRequest;
            _eventListener.PeerConnectedEvent += OnPeerConnected;
            _eventListener.NetworkReceiveEvent += OnReceive;

            Observable
                .IntervalFrame(Application.targetFrameRate / _state.Network.TickRate)
                .CatchIgnoreLog()
                .Subscribe(SendSync)
                .AddTo(this);

            _network.Observe()
                .Where(
                    network => network.NetworkId == default &&
                        !string.IsNullOrWhiteSpace(network.ServerIP) &&
                        network.ServerPort != default
                )
                .CatchIgnoreLog()
                .Subscribe(network => ConnectToServer(network.ServerIP, network.ServerPort))
                .AddTo(this);
        }

        private void OnDestroy() {
            _eventListener.ConnectionRequestEvent -= OnConnectionRequest;
            _eventListener.PeerConnectedEvent -= OnPeerConnected;
            _eventListener.NetworkReceiveEvent -= OnReceive;
        }

        private void ConnectToServer(string ip, int port) {
            _server = _netManager.Connect(ip, port, "BanchouConnectionKey");
        }

        private void OnConnectionRequest(ConnectionRequest request) {
            Debug.Log($"Connection request from {request.RemoteEndPoint}");

            var connectData = MessagePackSerializer.Deserialize<ConnectClient>(request.Data.GetRemainingBytes(), _messagePackOptions);

            if (connectData.ConnectionKey == "BanchouConnectionKey") {
                request.Accept();
                connectData.ServerReceiptTime = _getLocalTime();
                _connectingClients[request.RemoteEndPoint] = connectData;
                Debug.Log($"Accepted connection from {request.RemoteEndPoint}");
            } else {
                request.Reject();
            }
        }

        private void OnPeerConnected(NetPeer peer) {
            Debug.Log($"Setting up client connection from {peer.EndPoint}");

            // Generate a new network ID
            var newNetworkId = peer.Id;
            _network.ClientConnected(newNetworkId);
            _clients[newNetworkId] = peer;

            var connectData = _connectingClients[peer.EndPoint];
            _connectingClients.Remove(peer.EndPoint);

            var gameStateBytes = MessagePackSerializer.Serialize(_state.Board, _messagePackOptions);

            var syncClientMessage = Envelope.CreateMessage(
                PayloadType.Sync,
                new Connected {
                    ClientNetworkId = newNetworkId,
                    State = _state,
                    ClientTime = connectData.ClientConnectionTime,
                    ServerReceiptTime = connectData.ServerReceiptTime,
                    ServerTransmissionTime = _getLocalTime()
                },
                _messagePackOptions
            );

            peer.Send(syncClientMessage, DeliveryMethod.ReliableOrdered);
        }

        private void OnReceive(NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod) {
            var envelope = MessagePackSerializer.Deserialize<Envelope>(dataReader.GetRemainingBytes(), _messagePackOptions);

            // Deserialize payload
            switch (envelope.PayloadType) {
                case PayloadType.Connected: {
                    var syncClient = MessagePackSerializer.Deserialize<Connected>(envelope.Payload, _messagePackOptions);
                    _network.ConnectedToServer(
                        clientNetworkId: syncClient.ClientNetworkId,
                        serverTimeOffset: CalculateTimeOffset(
                            syncClient.ClientTime,
                            syncClient.ServerReceiptTime,
                            syncClient.ServerTransmissionTime,
                            _getLocalTime()
                        )
                    );
                } break;
                case PayloadType.InputUnit: {
                    var inputUnit = MessagePackSerializer.Deserialize<InputUnit>(envelope.Payload, _messagePackOptions);
                    // handle rollbacks
                } break;
                case PayloadType.TimeRequest: {
                    var request = MessagePackSerializer.Deserialize<TimeRequest>(envelope.Payload, _messagePackOptions);
                    var response = Envelope.CreateMessage(
                        PayloadType.TimeResponse,
                        new TimeResponse {
                            ClientTime = request.ClientTime,
                            ServerTime = _getLocalTime()
                        },
                        _messagePackOptions
                    );
                    fromPeer.Send(response, DeliveryMethod.Unreliable);
                } break;
                case PayloadType.TimeResponse: {
                    var response = MessagePackSerializer.Deserialize<TimeResponse>(envelope.Payload, _messagePackOptions);
                    _network.UpdateServerTime(
                        serverTimeOffset: CalculateTimeOffset(
                            response.ClientTime,
                            response.ServerTime,
                            response.ServerTime,
                            _getLocalTime()
                        )
                    );
                } break;
                case PayloadType.Sync: {
                    var sync = MessagePackSerializer.Deserialize<GameState>(envelope.Payload, _messagePackOptions);
                    _state.SyncGame(sync);
                } break;
            }
        }

        private void SendSync(long tick) {
            if (_clients.Count > 0) {
                var sync = Envelope.CreateMessage<GameState>(PayloadType.Sync, _state, _messagePackOptions);
                foreach (var client in _clients.Values) {
                    client.Send(sync, DeliveryMethod.Unreliable);
                }
            }
        }

        private float CalculateTimeOffset (float originTime, float receiptTime, float transmissionTime, float now) {
            return ((receiptTime - originTime) - (now - transmissionTime)) / 2f;
        }
    }
}