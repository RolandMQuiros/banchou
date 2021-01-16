using MessagePack;
using Banchou.Board;
using Banchou.Network;
using Banchou.Player;

namespace Banchou {
    [MessagePackObject]
    public class GameState : Substate<GameState> {
        [Key(0)] public NetworkState Network { get; private set; } = new NetworkState();
        [Key(1)] public BoardState Board { get; private set; } = new BoardState();
        [Key(2)] public PlayersState Players { get; private set; } = new PlayersState();

        protected override void OnProcess() {
            Board.Process();
            Players.Process();
        }
    }

    public delegate float GetTime();
    public delegate float GetDeltaTime();
}