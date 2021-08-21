using System;
using System.IO;

using MessagePack;
using Photon.Pun;
using UniRx;

using Banchou.Network.Message;
using Banchou.Pawn;
using Banchou.Player;

namespace Banchou.Network.Part {
    public class NetworkAgent : MonoBehaviourPunCallbacks {
        private GameState _state;
        private MessagePackSerializerOptions _messagePackOptions;
        private MemoryStream _buffer = new MemoryStream(1024);

        public void Construct(
            GameState state,
            MessagePackSerializerOptions messagePackOptions
        ) {
            _state = state;
            _messagePackOptions = messagePackOptions;

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
        }

        private void StartHost() {
            if (
                _state.IsConnected() &&
                _state.GetNetworkMode() == NetworkMode.Host &&
                !PhotonNetwork.IsMasterClient
            ) {
                PhotonNetwork.Disconnect();
            }
            
            PhotonNetwork.ConnectUsingSettings();
        }

        private void StartLocal() {
            PhotonNetwork.OfflineMode = true;
        }

        private void StartClient() {
            
        }

        public override void OnConnectedToMaster() {
            if (_state.GetNetworkMode() != NetworkMode.Host) {
                throw new Exception("OnConnectedToMaster called on a non-host actor");
            } else {
                PhotonNetwork.JoinOrCreateRoom(
                    _state.GetRoomName(),
                    new Photon.Realtime.RoomOptions(),
                    Photon.Realtime.TypedLobby.Default
                );
            }
        }

        public override void OnJoinedRoom() {
            _state.Network.ConnectedToHost(
                photonView.ControllerActorNr,
                PhotonNetwork.MasterClient.ActorNumber,
                PhotonNetwork.ServerTimestamp - Environment.TickCount
            );
        }

        [PunRPC]
        private void Rpc_OnEvent(byte[] eventData) {
            var payload = new MemoryStream(eventData);
            var payloadType = (PayloadType)payload.ReadByte();

            // Deserialize payload
            switch (payloadType) {
                case PayloadType.PlayerInput: {
                    var input = MessagePackSerializer.Deserialize<PlayerInputState>(payload, _messagePackOptions);
                    _state.SyncInput(input);
                } break;
                case PayloadType.SyncGame: {
                    var sync = MessagePackSerializer.Deserialize<GameState>(payload, _messagePackOptions);
                    _state.SyncGame(sync);
                } break;
                case PayloadType.SyncSpatial: {
                    var sync = MessagePackSerializer.Deserialize<PawnSpatial>(payload, _messagePackOptions);
                    _state.SyncSpatial(sync);
                } break;
            }
        }

        private int CalculateTimeOffset (int originTime, int receiptTime, int transmissionTime, int now) {
            return ((receiptTime - originTime) - (now - transmissionTime)) / 2;
        }
    }
}