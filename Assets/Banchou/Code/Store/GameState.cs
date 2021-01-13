using System.Collections;
using MessagePack;

using Banchou.Board;
using Banchou.Player;

namespace Banchou {
    [MessagePackObject]
    public class GameState : Substate {
        [Key(0)] public BoardState Board { get; private set; } = new BoardState();
        [Key(1)] public PlayersState Players { get; private set; } = new PlayersState();

        protected override bool Consume(IList actions) {
            Board.Process(actions);
            return false;
        }
    }

    public delegate float GetTime();
    public delegate float GetDeltaTime();
}