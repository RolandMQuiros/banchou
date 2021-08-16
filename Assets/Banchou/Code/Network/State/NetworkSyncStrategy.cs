using System;
using System.IO;
using System.Linq;

using ExitGames.Client.Photon;
using MessagePack;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;

using Banchou.Board;
using Banchou.Network.Message;

namespace Banchou.Network {
    public class NetworkSyncStrategy : MonoBehaviour {
        public void Construct(
            GameState state,
            MessagePackSerializerOptions messagePackOptions
        ) {
            // Check for changes in the network state...
            var observeInterval = state.ObserveNetwork()
                // ...but only if the current session is for a server
                .Where(network => network.Mode == NetworkMode.Host)
                // Calculate the interval from the tick rate
                .Select(network => TimeSpan.FromSeconds(1.0 / network.TickRate))
                .SelectMany(interval => Observable.Timer(TimeSpan.Zero, interval));

            var memoryStream = new MemoryStream(1024);
            var eventOptions = new RaiseEventOptions {
                Receivers = ReceiverGroup.Others
            };

            // Watch for changes from every pawn
            state.ObserveBoard()
                .SelectMany(_ => state.GetPawns().Values)
                .SelectMany(
                    pawn => pawn.Spatial.Observe()
                        .Sample(observeInterval)
                )
                .CatchIgnoreLog()
                .Subscribe(spatial => {
                    memoryStream.SetLength(0);
                    MessagePackSerializer.Serialize(memoryStream, spatial, messagePackOptions);
                    PhotonNetwork.RaiseEvent(
                        (byte)PayloadType.SyncSpatial,
                        memoryStream.GetBuffer(),
                        eventOptions,
                        SendOptions.SendUnreliable
                    );
                })
                .AddTo(this);
        }
    }
}