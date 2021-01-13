using System.Collections;
using System.Collections.Generic;
using MessagePack;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject]
    public class BoardState : Substate<BoardState> {
        [Key(0)] public Dictionary<int, PawnState> Pawns { get; private set; } = new Dictionary<int, PawnState>();
        [Key(1)] public float LastUpdated;

        protected override bool Consume(IList actions) {
            var consumed = false;

            foreach (var action in actions) {
                if (action is StateAction.AddPawn add && !Pawns.ContainsKey(add.PawnId)) {
                    Pawns.Add(
                        add.PawnId,
                        new PawnState(
                            pawnId: add.PawnId,
                            playerId: add.PlayerId,
                            prefabKey: add.PrefabKey,
                            position: add.Position,
                            forward: add.Forward,
                            lastUpdated: add.When
                        )
                    );
                    LastUpdated = add.When;
                    consumed = true;
                }

                if (action is StateAction.RemovePawn remove) {
                    Pawns.Remove(remove.PawnId);
                    LastUpdated = remove.When;
                    consumed = true;
                }

                if (action is StateAction.ClearPawns clear) {
                    Pawns.Clear();
                    LastUpdated = clear.When;
                    consumed = true;
                }
            }

            foreach (var pawn in Pawns.Values) pawn.Process(actions);
            return consumed;
        }
    }
}