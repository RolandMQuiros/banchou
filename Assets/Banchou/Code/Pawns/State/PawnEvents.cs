using System;
using UniRx;
using UnityEngine;
using Banchou.Board;
using Banchou.Player;

namespace Banchou.Pawn {
    public static class PawnEvents {
        public static IObservable<PawnState> ObservePawns(this GameState state) =>
            state.GetPawns().Values.ToObservable()
                .Merge(state.ObserveAddedPawns());

        public static IObservable<PawnState> ObservePawn(this GameState state, int pawnId) =>
            Observable.Never<PawnState>()
                .StartWith(state.GetPawn(pawnId))
                .Where(pawn => pawn != null);

        public static IObservable<PawnState> ObservePawnChanges(this GameState state, int pawnId) =>
            state.GetPawn(pawnId).Observe();

        public static IObservable<PawnSpatial> ObservePawnSpatial(this GameState state, int pawnId) =>
            state.ObservePawn(pawnId)
                .Select(pawn => pawn.Spatial)
                .Where(spatial => spatial != null);

        public static IObservable<PawnSpatial> ObservePawnSpatialChanges(this GameState state, int pawnId) =>
            state.GetPawn(pawnId)?.Spatial?.Observe() ?? Observable.Empty<PawnSpatial>();
            
        public static IObservable<PawnSpatial> ObservePawnSpatialsChanges(this GameState state) =>
            state.ObserveAddedPawns()
                .Merge(state.ObserveRemovedPawns())
                .SelectMany(_ => state.GetPawns().Values)
                .SelectMany(pawn => pawn.Spatial.Observe());

        public static IObservable<PawnAnimatorFrame> ObservePawnAnimatorFrame(this GameState state, int pawnId) =>
            state.ObservePawn(pawnId)
                .Where(pawn => pawn.AnimatorFrame != null)
                .Select(pawn => pawn.AnimatorFrame);

        public static IObservable<PawnAnimatorFrame> ObservePawnAnimatorFrameChanges(this GameState state, int pawnId) {
            return state.ObservePawnAnimatorFrame(pawnId)
                .SelectMany(frame => frame.Observe());
        }

        public static IObservable<int> ObservePawnPlayerId(this GameState state, int pawnId) =>
            state.ObservePawnChanges(pawnId)
                .Select(pawn => pawn.PlayerId)
                .DistinctUntilChanged()
                .Where(playerId => playerId != default);

        public static IObservable<PlayerState> ObservePawnPlayer(this GameState state, int pawnId) =>
            state.ObservePawnPlayerId(pawnId)
                .Select(state.GetPlayer)
                .DistinctUntilChanged();

        public static IObservable<PlayerInputState> ObservePawnInput(this GameState state, int pawnId) =>
            state.ObservePawnPlayerId(pawnId)
                .SelectMany(state.ObservePlayerInputChanges);

        public static IObservable<(InputCommand Command, float When)> ObservePawnInputCommands(
            this GameState state, int pawnId
        ) => state.ObservePawnInput(pawnId)
            .WithLatestFrom(
                state.ObservePawnSpatialChanges(pawnId),
                (input, spatial) => (
                    Command: new Vector2(
                        Vector3.Dot(input.Direction, spatial.Right),
                        Vector3.Dot(input.Direction, spatial.Forward)
                    ).DirectionToStick() | input.Commands,
                    input.When
                )
            );

        public static IObservable<float> ObservePawnTimeScale(this GameState state, int pawnId) {
            var pawn = state.GetPawn(pawnId);

            if (pawn != null) {
                // Optimized to avoid SelectMany
                return state.ObserveBoardChanges().Select(_ => 0)
                    .Merge(state.ObservePawnChanges(pawnId).Select(_ => 0))
                    .Select(_ => state.Board.TimeScale * (pawn ??= state.GetPawn(pawnId)).TimeScale)
                    .DistinctUntilChanged();
            }
            return Observable.Empty<float>();
        }

        public static IObservable<float> ObservePawnDeltaTime(this GameState state, int pawnId) {
            return Observable.EveryFixedUpdate()
                .WithLatestFrom(state.ObservePawnTimeScale(pawnId), (_, timeScale) => timeScale)
                .Select(timeScale => state.GetDeltaTime() * timeScale);
        }
    }
}