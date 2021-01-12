using System;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn {
    public static class PawnSelectors {
        public static PawnState GetPawn(this GameState state, int pawnId) {
            PawnState pawn;
            if (state.Board.Pawns.TryGetValue(pawnId, out pawn)) {
                return pawn;
            }
            return null;
        }

        public static IObservable<Unit> ObservePawnChanges(this GameState state, int pawnId) {
            var pawn = state.GetPawn(pawnId);
            if (pawn == null) {
                return Observable.Empty<Unit>();
            } else {
                return Observable.FromEvent(
                    h => pawn.Changed += h,
                    h => pawn.Changed -= h
                );
            }
        }

        public static int GetPawnPlayerId(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.PlayerId ?? 0;
        }

        public static string GetPawnPrefabKey(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.PrefabKey;
        }

        public static (Vector3 Position, Vector3 Forward, Vector3 Up, Vector3 Velocity) GetPawnVectors(this GameState state, int pawnId) {
            var pawn = state.GetPawn(pawnId);
            if (pawn == null) {
                return (
                    Position: Vector3.zero,
                    Forward: Vector3.zero,
                    Up: Vector3.zero,
                    Velocity: Vector3.zero
                );
            }
            return (
                Position: pawn.Position,
                Forward: pawn.Forward,
                Up: pawn.Up,
                Velocity: pawn.Velocity
            );
        }

        public static Vector3 GetPawnPosition(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.Position ?? Vector3.zero;
        }

        public static Quaternion GetPawnRotation(this GameState state, int pawnId) {
            var pawn = state.GetPawn(pawnId);
            if (pawn == null) {
                return Quaternion.identity;
            } else {
                return Quaternion.LookRotation(pawn.Forward, pawn.Up);
            }
        }
    }
}