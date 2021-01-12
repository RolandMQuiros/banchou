using Banchou.Pawn;

namespace Banchou.Board.StateAction {
    public struct AddPawn {
        public PawnState Pawn;
    }

    public struct RemovePawn {
        public int Id;
    }

    public struct ClearPawns { }
}