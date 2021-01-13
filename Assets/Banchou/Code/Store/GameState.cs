using System.Collections;
using MessagePack;
using Banchou.Board;

namespace Banchou {
    [MessagePackObject]
    public class GameState : Substate {
        [Key(0)] public BoardState Board { get; private set; } = new BoardState();

        public GameState() { }

        public GameState(in GameState prev) {
            Board = prev.Board;
        }

        protected override bool Consume(IList actions) {
            Board.Process(actions);
            return false;
        }
    }

    public delegate float GetTime();
    public delegate float GetDeltaTime();
}