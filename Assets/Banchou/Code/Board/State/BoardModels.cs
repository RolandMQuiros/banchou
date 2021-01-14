using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MessagePack;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject]
    public class BoardState : Substate<BoardState> {
        [Key(0)] public Dictionary<int, PawnState> Pawns { get; private set; } = new Dictionary<int, PawnState>();
        [Key(1)] public float LastUpdated { get; private set; }

        protected override bool Consume(IList actions) {
            var consumed = false;

            foreach (var action in actions) {
                if (action is Banchou.StateAction.SyncGame sync) {
                    PatchPawns(sync.Board);
                    LastUpdated = sync.When;
                    consumed = true;
                }

                if (action is StateAction.RollbackBoard rollback) {
                    PatchPawns(rollback.Board);
                    LastUpdated = rollback.Board.LastUpdated;
                    consumed = true;
                }

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

        private void PatchPawns(BoardState other) {
            var otherPawnIds = other.Pawns.Keys;

            // Add missing pawns
            foreach (var added in otherPawnIds.Except(Pawns.Keys)) {
                Pawns[added] = other.Pawns[added];
            }

            // Remove extraneous pawns
            foreach (var removed in Pawns.Keys.Except(otherPawnIds)) {
                Pawns.Remove(removed);
            }
        }
    }
}