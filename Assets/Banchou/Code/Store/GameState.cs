using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

using Banchou.Board;
using Banchou.Network;
using Banchou.Pawn;
using Banchou.Player;

namespace Banchou {
    [MessagePackObject, Serializable]
    public record GameState(
        string Version = null, BoardState Board = null, PlayersState Players = null, float LocalTime = 0f,
        float DeltaTime = 0f
    ) : Notifiable<GameState> {
        [field: SerializeField] public NetworkState Network { get; private set; } = new();
        [field: SerializeField] public string Version { get; private set; } = Version;
        [field: SerializeField] public BoardState Board { get; private set; } = Board ?? new BoardState();
        [field: SerializeField] public PlayersState Players { get; private set; } = Players ?? new PlayersState();
        [field: SerializeField] public float LocalTime { get; private set; } = LocalTime;
        [field: SerializeField] public float DeltaTime { get; private set; } = DeltaTime;

        public GameState SetVersion(string version) {
            Version = version;
            return Notify();
        }

        public GameState SyncGame(GameState other) {
            Board.SyncGame(other.Board);
            Players.SyncGame(other.Players);
            return this;
        }

        public GameState SyncBoard(IEnumerable<PawnState> pawns, IEnumerable<PlayerState> players) {
            Players.SyncBoard(players);
            Board.SyncBoard(pawns);
            return this;
        }

        public GameState SetLocalTime(float localTime, float deltaTime) {
            LocalTime = localTime;
            DeltaTime = deltaTime;
            // Don't bother notifying for this
            return this;
        }
    }
}