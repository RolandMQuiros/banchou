using System;
using UniRx;
using UnityEngine;
using Banchou.Board;
using Banchou.Player;

namespace Banchou.Pawn {
    public static class PawnEvents {
        public static IObservable<PawnState> ObservePawn(this GameState state, int pawnId) {
            return state.ObserveBoard()
                .Select(board => {
                    board.Pawns.TryGetValue(pawnId, out var pawn);
                    return pawn;
                })
                .StartWith(state.GetPawn(pawnId))
                .Where(pawn => pawn != null);
        }

        public static IObservable<PawnState> ObservePawnChanges(this GameState state, int pawnId) {
            return state.ObservePawn(pawnId)
                .SelectMany(pawn => pawn.Observe());
        }

        public static IObservable<PawnSpatial> ObservePawnSpatial(this GameState state, int pawnId) {
            return state
                .ObservePawn(pawnId)
                .Select(pawn => pawn.Spatial)
                .Where(spatial => spatial != null);
        }

        public static IObservable<PawnSpatial> ObservePawnSpatialChanges(this GameState state, int pawnId) {
            return state.ObservePawnSpatial(pawnId)
                .SelectMany(spatial => spatial.Observe());
        }

        public static IObservable<PawnSpatial> ObservePawnSpatialsChanges(this GameState state) {
            return state.ObserveAddedPawns()
                .Merge(state.ObserveRemovedPawns())
                .SelectMany(_ => state.GetPawns().Values)
                .SelectMany(pawn => pawn.Spatial.Observe());
        }

        public static IObservable<PlayerInputState> ObservePawnInput(this GameState state, int pawnId) {
            return state
                .ObservePawnChanges(pawnId)
                .Select(pawn => pawn.PlayerId)
                .DistinctUntilChanged()
                .CombineLatest(state.ObservePlayers(), (playerId, _) => playerId)
                .Where(playerId => playerId != default)
                .SelectMany(state.ObservePlayerInputChanges);
        }

        public static IObservable<(InputCommand Command, float When)> ObservePawnInputCommands(
            this GameState state, int pawnId
        ) {
            return state.ObservePawnInput(pawnId)
                .DistinctUntilChanged(input => input.Direction)
                .WithLatestFrom(
                    state.ObservePawnSpatialChanges(pawnId),
                    (input, spatial) => (
                        Command: new Vector2(
                            Vector3.Dot(input.Direction, spatial.Right),
                            Vector3.Dot(input.Direction, spatial.Forward)
                        ).DirectionToStick(),
                        When: input.When
                    )
                )
                .DistinctUntilChanged()
                .Merge(
                    state.ObservePawnInput(pawnId)
                        .Select(input => ( Command: input.Commands, input.When ))
                );
        }
    }
}