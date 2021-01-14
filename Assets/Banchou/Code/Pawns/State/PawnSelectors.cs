using System;
using UniRx;
using UnityEngine;

using Banchou.Board;
using Banchou.Player;

namespace Banchou.Pawn {
    public static class PawnSelectors {
        public static IObservable<PawnState> ObservePawn(this IObservable<GameState> observeState, int pawnId) {
            return observeState
                .ObserveBoard()
                .SelectMany(board => {
                    PawnState pawn;
                    if (board.Pawns.TryGetValue(pawnId, out pawn)) {
                        return pawn.Observe();
                    }
                    return Observable.Empty<PawnState>();
                });
        }

        public static IObservable<GameState> OnPawnChanged(this IObservable<GameState> observeState, int pawnId) {
            return observeState
                .OnBoardChanged()
                .SelectMany(
                    state => {
                        var pawn = state.GetPawn(pawnId);
                        if (pawn != null) {
                            return pawn.Observe().Select(_ => state);
                        }
                        return Observable.Empty<GameState>();
                    }
                );
        }

        public static IObservable<InputUnit> ObservePawnInput(this IObservable<GameState> observeState, int pawnId) {
            return observeState
                .ObservePawn(pawnId)
                .Select(pawn => pawn.PlayerId)
                .SelectMany(playerId => observeState.ObservePlayer(playerId))
                .Select(player => player.LastInput)
                .DistinctUntilChanged();
        }

        public static PawnState GetPawn(this GameState state, int pawnId) {
            PawnState pawn;
            if (state.Board.Pawns.TryGetValue(pawnId, out pawn)) {
                return pawn;
            }
            return null;
        }

        public static int GetPawnPlayerId(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.PlayerId ?? 0;
        }

        public static PlayerState GetPawnPlayer(this GameState state, int pawnId) {
            return state.GetPlayer(state.GetPawnPlayerId(pawnId));
        }

        public static string GetPawnPrefabKey(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.PrefabKey;
        }

        public static (Vector3 Position, Vector3 Forward, Vector3 Up, Vector3 Velocity) GetPawnSpatial(this GameState state, int pawnId) {
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