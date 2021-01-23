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
                .SelectMany(interval => Observable.Interval(interval))
                .Where(_ => netManager.ConnectedPeersCount > 0);

            // Watch for changes from every player
            var playerBuffer = new List<PlayerState>(state.GetPlayers().Count);
            state.ObservePlayers()
                .SelectMany(_ => state.GetPlayers().Values)
                .SelectMany(
                    player => player.Observe()
                        .Merge(player.Input.OnChange(player))
                        .Sample(observeInterval)
                )
                .CatchIgnoreLog()
                .Subscribe(player => {
                    if (!playerBuffer.Contains(player)) {
                        playerBuffer.Add(player);
                    }
                })
                .AddTo(this);

            // Watch for changes from every pawn
            var pawnBuffer = new List<PawnState>(state.GetPawns().Count);
            state.ObserveBoard()
                .SelectMany(_ => state.GetPawns().Values)
                .SelectMany(
                    pawn => pawn.Observe()
                        .Merge(pawn.Spatial.OnChange(pawn))
                        .Sample(observeInterval)
                )
                .CatchIgnoreLog()
                .Subscribe(pawn => {
                    if (!pawnBuffer.Contains(pawn)) {
                        pawnBuffer.Add(pawn);
                    }
                })
                .AddTo(this);

            // On the tick interval, if any pawn or player changes were detected, create a SyncBoard message
            observeInterval
                .Where(_ => netManager.ConnectedPeersCount > 0)
                .Where(_ => playerBuffer.Any() || pawnBuffer.Any())
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    var sync = new SyncBoard {
                        Pawns = pawnBuffer,
                        Players = playerBuffer
                    };

                    var message = Envelope.CreateMessage(PayloadType.SyncBoard, sync, messagePackOptions);
                    for (int p = 0; p < netManager.ConnectedPeersCount; p++) {
                        var peer = netManager.GetPeerById(p);
                        peer.Send(message, DeliveryMethod.Unreliable);
                    }

                    pawnBuffer.Clear();
                    playerBuffer.Clear();
                })
                .AddTo(this);
        }
    }
}