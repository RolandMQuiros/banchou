using System;
using MessagePack;
using UnityEngine;

using Banchou.Board;
using Banchou.Network;
using Banchou.Player;

namespace Banchou {
    [MessagePackObject, Serializable]
    public class GameState : Substate<GameState> {
        [IgnoreMember] public NetworkState Network => _network;
        [Key(0), SerializeField] private NetworkState _network = new NetworkState();

        [IgnoreMember] public BoardState Board => _board;
        [Key(1), SerializeField] private BoardState _board = new BoardState();

        [IgnoreMember] public PlayersState Players => _players;
        [Key(2), SerializeField] private PlayersState _players = new PlayersState();

        protected override void OnProcess() {
            _board.Process();
            _players.Process();
        }

        public GameState SyncGame(GameState other) {
            _board.SyncGame(other);
            // _players.SyncGame(other);
            return this;
        }
    }

    public delegate float GetTime();
    public delegate float GetDeltaTime();
}