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
    public class GameState : Notifiable<GameState> {
        [IgnoreMember][field:SerializeField] public NetworkState Network { get; private set; } = new NetworkState();
        [Key(0)][field:SerializeField] public string Version { get; private set; }
        [Key(1)][field:SerializeField] public BoardState Board { get; private set; } = new BoardState();
        [Key(2)][field:SerializeField] public PlayersState Players { get; private set; } = new PlayersState();
        [Key(3)][field:SerializeField] public float LocalTime { get; private set; }
        [Key(4)][field:SerializeField] public float DeltaTime { get; private set; }

        public GameState() { }

        [SerializationConstructor]
        public GameState(
            string version,
            BoardState board,
            PlayersState players,
            float localTime,
            float deltaTime
        ) {
            Version = version;
            Board = board;
            Players = players;
            LocalTime = localTime;
            DeltaTime = deltaTime;
        }

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