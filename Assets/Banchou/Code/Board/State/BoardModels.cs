using System.Collections;
using System.Collections.Generic;
using MessagePack;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject]
    public class BoardState : Substate {
        [Key(0)] public Dictionary<int, PawnState> Pawns { get; private set; } = new Dictionary<int, PawnState>();

        public BoardState() { }
        public BoardState(BoardState prev) {
            Pawns = prev.Pawns;
        }
        protected override bool Consume(IEnumerable actions) {
            var consumed = false;

            foreach (var action in actions) {
                if (action is StateAction.AddPawn add && !Pawns.ContainsKey(add.Pawn.Id)) {
                    Pawns.Add(add.Pawn.Id, add.Pawn);
                    consumed = true;
                }

                if (action is StateAction.RemovePawn remove) {
                    Pawns.Remove(remove.Id);
                    consumed = true;
                }

                if (action is StateAction.ClearPawns clear) {
                    Pawns.Clear();
                    consumed = true;
                }
            }

            foreach (var pawn in Pawns.Values) pawn.Process(actions);
            return consumed;
        }
    }
}