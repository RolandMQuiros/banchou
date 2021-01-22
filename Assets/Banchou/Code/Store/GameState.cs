using System;
using MessagePack;
using UnityEngine;

using Banchou.Board;
using Banchou.Network;
using Banchou.Player;

namespace Banchou {
    [MessagePackObject, Serializable]
    public class GameState : Notifiable<GameState> {
        [IgnoreMember] public NetworkState Network => _network;
        [IgnoreMember, SerializeField] private NetworkState _network = new NetworkState();

        [IgnoreMember] public BoardState Board => _board;
        [Key(0), SerializeField] private BoardState _board = new BoardState();

        [IgnoreMember] public PlayersState Players => _players;
        [Key(1), SerializeField] private PlayersState _players = new PlayersState();

        [IgnoreMember] public float LocalTime => _localTime;
        [Key(2), SerializeField] private float _localTime;

        [IgnoreMember] public float DeltaTime => _deltaTime;
        [Key(3), SerializeField] private float _deltaTime;

        public GameState SyncGame(GameState other) {
            _board.SyncGame(other.Board);
            _players.SyncGame(other.Players);
            return this;
        }

        public GameState SetLocalTime(float localTime, float deltaTime) {
            _localTime = localTime;
            _deltaTime = deltaTime;
            // Don't bother notifying for this
            return this;
        }
    }

    public delegate float GetTime();
    public delegate float GetDeltaTime();
}