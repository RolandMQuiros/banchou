using System;
using System.IO;

using ExitGames.Client.Photon;
using MessagePack;
using Photon.Pun;
using Photon.Realtime;
using UniRx;

using Banchou.Network.Message;
using Banchou.Pawn;
using Banchou.Player;

namespace Banchou.Network.Part {
    public class NetworkAgent : MonoBehaviourPunCallbacks, IOnEventCallback {
        private GameState _state;
        private MessagePackSerializerOptions _messagePackOptions;
        private MemoryStream _buffer = new MemoryStream(1024);

        public void Construct(
            GameState state,
            MessagePackSerializerOptions messagePackOptions
        ) {
            _state = state;
            _messagePackOptions = messagePackOptions;

            PhotonPeer.RegisterType(
                typeof(MemoryStream),
                1,
                PayloadExtensions.SerializeMemoryStream,
                PayloadExtensions.DeserializeMemoryStream
            );

            _state.ObserveNetwork()
                .Select(network => network.Mode)
                .DistinctUntilChanged()
                .CatchIgnoreLog()
                .Subscribe(networkMode => {
                    switch (networkMode) {
                        case NetworkMode.Host:
                            StartHost();
                            break;
                        case NetworkMode.Local:
                            StartLocal();
                            break;
                        case NetworkMode.Client:
                            StartClient();
                            break;
                    }
                })
                .AddTo(this);

            _state.ObserveLocalPlayerInputChanges()
                .Where(_ => _state.IsConnected())
                .CatchIgnoreLog()
                .Subscribe(input => {
                    input.SendOverNetwork(PayloadType.PlayerInput, true, _messagePackOptions);
                })
                .AddTo(this);

            var tickInterval = _state.ObserveNetwork()
                .Where(_ => _state.IsHosting())
                .Select(network => TimeSpan.FromSeconds(1.0 / network.TickRate))
                .SelectMany(interval => Observable.Timer(TimeSpan.Zero, interval));
            
            _state.ObservePawnSpatialsChanges()
                .Sample(tickInterval)
                .CatchIgnoreLog()
                .Subscribe(spatial => {
                    spatial.SendOverNetwork(PayloadType.SyncSpatial, false, _messagePackOptions);
                })
                .AddTo(this);
        }

        private void StartHost() {
            if (
                _state.IsConnected() &&
                _state.GetNetworkMode() == NetworkMode.Host &&
                !PhotonNetwork.IsMasterClient
            ) {
                PhotonNetwork.Disconnect();
            }

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }

        private void StartLocal() {
            PhotonNetwork.OfflineMode = true;
        }

        private void StartClient() {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster() {
            switch (_state.GetNetworkMode()) {
                case NetworkMode.Host:
                    PhotonNetwork.JoinOrCreateRoom(
                        _state.GetRoomName(),
                        new RoomOptions(),
                        TypedLobby.Default
                    );
                    break;
                case NetworkMode.Client:
                    PhotonNetwork.JoinLobby();
                    break;
            }
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
            if (_state.IsHosting()) {
                _state.SendOverNetwork(
                    PayloadType.SyncGame,
                    true,
                    _messagePackOptions,
                    newPlayer.ActorNumber
                );
            }
        }

        public override void OnJoinedLobby() {
            PhotonNetwork.JoinRoom(_state.GetRoomName());
        }

        public override void OnJoinedRoom() {
            _state.Network.ConnectedToHost(
                PhotonNetwork.LocalPlayer.ActorNumber,
                PhotonNetwork.MasterClient.ActorNumber,
                PhotonNetwork.ServerTimestamp - Environment.TickCount
            );

            var settings = PhotonNetwork
                .NetworkingClient
                .LoadBalancingPeer
                .NetworkSimulationSettings;
            var minLag = _state.Network.SimulateMinLatency;
            var maxLag = _state.Network.SimulateMaxLatency;
            
            var jitter = (maxLag - minLag) / 2;
            var lag = minLag + jitter;

            settings.IncomingLag = settings.OutgoingLag = lag;
            settings.IncomingJitter = settings.OutgoingJitter = jitter;
        }
        
        public void OnEvent(EventData photonEvent) {
            var payloadType = (PayloadType)photonEvent.Code;

            // Deserialize payload
            switch (payloadType) {
                case PayloadType.PlayerInput: {
                    var input = MessagePackSerializer.Deserialize<PlayerInputState>(
                        (MemoryStream)photonEvent.CustomData,
                        _messagePackOptions
                    );
                    _state.SyncInput(input);
                } break;
                case PayloadType.SyncGame: {
                    var sync = MessagePackSerializer.Deserialize<GameState>(
                        (MemoryStream)photonEvent.CustomData,
                        _messagePackOptions
                    );
                    _state.SyncGame(sync);
                } break;
                case PayloadType.SyncSpatial: {
                    var sync = MessagePackSerializer.Deserialize<PawnSpatial>(
                        (MemoryStream)photonEvent.CustomData,
                        _messagePackOptions
                    );
                    _state.SyncSpatial(sync);
                } break;
            }
        }

        private int CalculateTimeOffset (int originTime, int receiptTime, int transmissionTime, int now) {
            return ((receiptTime - originTime) - (now - transmissionTime)) / 2;
        }
    }
}