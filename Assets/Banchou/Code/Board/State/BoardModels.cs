using System.Linq;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject]
    public class BoardState : Substate<BoardState> {
        [Key(0)] public Dictionary<int, PawnState> Pawns { get; private set; } = new Dictionary<int, PawnState>();
        [Key(1)] public float LastUpdated { get; private set; }

        protected override void OnProcess() {
            foreach (var pawn in Pawns.Values) {
                pawn.Process();
            }
        }

        public void SyncGame(GameState sync, float when) {
            PatchPawns(sync.Board);
            foreach (var pawn in Pawns.Values) {
                pawn.SyncGame(sync, when);
            }
            Notify();
        }

        public void AddPawn(int pawnId, string prefabKey, int playerId, Vector3 position, Vector3 forward, float when) {
            Pawns.Add(
                pawnId,
                new PawnState(
                    pawnId: pawnId,
                    playerId: playerId,
                    prefabKey: prefabKey,
                    position: position,
                    forward: forward,
                    lastUpdated: when
                )
            );
            LastUpdated = when;
            Notify();
        }

        public void AddPawn(int pawnId, string prefabKey, int playerId, Vector3 position, float when) {
            AddPawn(pawnId, prefabKey, playerId, position, Vector3.forward, when);
        }

        public void RemovePawn(int pawnId, float when) {
            Pawns.Remove(pawnId);
            LastUpdated = when;
            Notify();
        }

        public void ClearPawns(float when) {
            Pawns.Clear();
            LastUpdated = when;
            Notify();
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