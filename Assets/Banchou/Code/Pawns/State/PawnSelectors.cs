using System;
using System.Linq;
using UniRx;
using Banchou.Board;
using Banchou.Player;

namespace Banchou.Pawn {
    public static class PawnSelectors {
        public static IObservable<PawnState> ObservePawn(this GameState state, int pawnId) {
            return state
                .ObserveBoard()
                .SelectMany(board => {
                    PawnState pawn;
                    if (board.Pawns.TryGetValue(pawnId, out pawn)) {
                        return pawn.Observe();
                    }
                    return Observable.Empty<PawnState>();
                });
        }

        public static IObservable<PlayerInputState> ObservePawnInput(this GameState state, int pawnId) {
            return state
                .ObservePawn(pawnId)
                .Select(pawn => pawn.PlayerId)
                .DistinctUntilChanged()
                .CombineLatest(state.ObservePlayers(), (playerId, _) => playerId)
                .Where(playerId => playerId != default)
                .SelectMany(playerId => state.ObservePlayer(playerId))
                .Where(player => player?.Input != null)
                .SelectMany(player => player.Input.Observe());
        }

        public static PawnState GetPawn(this GameState state, int pawnId) {
            PawnState pawn;
            if (state.Board.Pawns.TryGetValue(pawnId, out pawn)) {
                return pawn;
            }
            return null;
        }

        public static PawnSpatial GetPawnSpatial(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.Spatial;
        }

        public static int GetPawnPlayerId(this GameState state, int pawnId) {
            return state.GetPawn(pawnId)?.PlayerId ?? 0;
        }

        public static PlayerState GetPawnPlayer(this GameState state, int pawnId) {
            return state.GetPlayer(state.GetPawnPlayerId(pawnId));
        }
    }
}