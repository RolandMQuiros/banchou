using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;

using LiteNetLib;
using MessagePack;
using UniRx;
using UnityEngine;

using Banchou.Network.Message;
using Banchou.Board;
using Banchou.Player;

namespace Banchou.Network.Part {
    public class NetworkServer : MonoBehaviour {
        private GameState _state;
        private EventBasedNetListener _eventListener;
        private NetManager _netManager;
        private MessagePackSerializerOptions _messagePackOptions;
        private NetPeer _server;
        private Dictionary<IPEndPoint, ConnectClient> _connectingClients = new Dictionary<IPEndPoint, ConnectClient>();
        private Dictionary<int, NetPeer> _clients = new Dictionary<int, NetPeer>();

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

            _eventListener.ConnectionRequestEvent += OnConnectionRequest;
            _eventListener.PeerConnectedEvent += OnPeerConnected;
            _eventListener.NetworkReceiveEvent += OnReceive;

            _state.Network
                .Observe()
                .Where(network => network.Mode == NetworkMode.Server)
                .Select(network => network.TickRate)
                .DistinctUntilChanged()
                .SelectMany(tickRate => Observable.Interval(TimeSpan.FromSeconds(1.0 / tickRate)))
                .Where(_ => _clients.Count > 0)
                .CatchIgnoreLog()
                .Subscribe(_ => SendSync())
                .AddTo(this);

            _state.Network.Observe()
                .Where(network => network.Mode == NetworkMode.Server)
                .Select(network => network.ServerPort)
                .DistinctUntilChanged()
                .CatchIgnoreLog()
                .Subscribe(port => StartServer(port))
                .AddTo(this);
        }

        private void OnDestroy() {
            _eventListener.ConnectionRequestEvent -= OnConnectionRequest;
            _eventListener.PeerConnectedEvent -= OnPeerConnected;
            _eventListener.NetworkReceiveEvent -= OnReceive;
        }

        private void StartServer(int port) {
            if (_netManager.IsRunning) {
                _netManager.Stop();
            }
            _netManager.Start(port);
            Debug.Log($"Server started on port {port}");
        }

        private void OnConnectionRequest(ConnectionRequest request) {
            if (_state.GetNetworkMode() != NetworkMode.Server) {
                return;
            }

            Debug.Log($"Connection request from {request.RemoteEndPoint}");

            var connectData = MessagePackSerializer.Deserialize<ConnectClient>(request.Data.GetRemainingBytes(), _messagePackOptions);

            if (connectData.ConnectionKey == "BanchouConnectionKey") {
                request.Accept();
                connectData.ServerReceiptTime = _state.GetLocalTime();
                _connectingClients[request.RemoteEndPoint] = connectData;
                Debug.Log($"Accepted connection from {request.RemoteEndPoint}");
            } else {
                request.Reject();
            }
        }

        private void OnPeerConnected(NetPeer peer) {
            if (_state.GetNetworkMode() != NetworkMode.Server) {
                return;
            }

            Debug.Log($"Setting up client connection from {peer.EndPoint}");

            // Generate a new network ID
            var newNetworkId = peer.Id;
            _state.Network.ClientConnected(newNetworkId);
            _clients[newNetworkId] = peer;

            var connectData = _connectingClients[peer.EndPoint];
            _connectingClients.Remove(peer.EndPoint);

            var gameStateBytes = MessagePackSerializer.Serialize(_state.Board, _messagePackOptions);

            var syncClientMessage = Envelope.CreateMessage(
                PayloadType.Connected,
                new Connected {
                    ClientNetworkId = newNetworkId,
                    State = _state,
                    ClientTime = connectData.ClientConnectionTime,
                    ServerReceiptTime = connectData.ServerReceiptTime,
                    ServerTransmissionTime = _state.LocalTime
                },
                _messagePackOptions
            );

            peer.Send(syncClientMessage, DeliveryMethod.ReliableOrdered);
        }

        private void OnReceive(NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod) {
            if (_state.GetNetworkMode() != NetworkMode.Server) {
                return;
            }

            var envelope = MessagePackSerializer.Deserialize<Envelope>(dataReader.GetRemainingBytes(), _messagePackOptions);

            // Deserialize payload
            switch (envelope.PayloadType) {
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
                            ServerTime = _state.GetLocalTime()
                        },
                        _messagePackOptions
                    );
                    fromPeer.Send(response, DeliveryMethod.Unreliable);
                } break;
                case PayloadType.SyncGame: {
                    var sync = MessagePackSerializer.Deserialize<GameState>(envelope.Payload, _messagePackOptions);
                    _state.SyncGame(sync);
                } break;
            }
        }

        private void SendSync() {
            if (_clients.Count > 0) {
                var sync = Envelope.CreateMessage<SyncBoard>(
                    PayloadType.SyncBoard,
                    new SyncBoard {
                        Pawns = _state.GetPawns().Values.ToList(),
                        Players = _state.GetPlayers().Values.ToList()
                    },
                    _messagePackOptions
                );
                foreach (var client in _clients.Values) {
                    client.Send(sync, DeliveryMethod.Unreliable);
                }
            }
        }
    }
}