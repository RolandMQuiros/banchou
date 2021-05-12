using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using LiteNetLib;
using MessagePack;
using UniRx;
using UnityEngine;

using Banchou.Board;
using Banchou.Network.Message;
using Banchou.Pawn;
using Banchou.Player;

namespace Banchou.Network {
    public class NetworkSyncStrategy : MonoBehaviour {
        private GameState _state;
        private NetManager _netManager;
        private MessagePackSerializerOptions _messagePackOptions;

        public void Construct(
            GameState state,
            NetManager netManager,
            MessagePackSerializerOptions messagePackOptions
        ) {
            // Check for changes in the network state...
            var observeInterval = state.ObserveNetwork()
                // ...but only if the current session is for a server
                .Where(network => network.Mode == NetworkMode.Server)
                // Calculate the interval from the tick rate
                .Select(network => TimeSpan.FromSeconds(1.0 / network.TickRate))
                .SelectMany(interval => Observable.Timer(TimeSpan.Zero, interval))
                .Where(_ => netManager.ConnectedPeersCount > 0);

            // Watch for changes from every pawn
            var pawnBuffer = new List<PawnState>(state.GetPawns().Count);
            state.ObserveBoard()
                .SelectMany(_ => state.GetPawns().Values)
                .SelectMany(
                    pawn => pawn.Spatial.Observe()
                        .Sample(observeInterval)
                )
                .CatchIgnoreLog()
                .Where(_ => netManager.ConnectedPeersCount > 0)
                .Subscribe(spatial => {
                    netManager.SendPayloadToAll(
                        PayloadType.SyncSpatial,
                        spatial,
                        DeliveryMethod.Unreliable,
                        messagePackOptions
                    );
                })
                .AddTo(this);
        }
    }
}