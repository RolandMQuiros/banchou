using System;
using System.Collections.Generic;
using System.Linq;
using Banchou.Board;
using Banchou.Pawn;
using UniRx;

namespace Banchou.Combatant {
    public static class CombatantEvents {
        /// <summary>
        /// Emits whenever a new <see cref="CombatantState"/> instance is associated with a pawn ID
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        /// <param name="pawnId">ID of the combatant pawn</param>
        /// <returns>An <see cref="IObservable{CombatantState}"/> that emits when a combatant is replaced</returns>
        public static IObservable<CombatantState> ObserveCombatant(this GameState state, int pawnId) {
            return state.ObservePawnChanges(pawnId)
                .Select(pawn => pawn.Combatant)
                .DefaultIfEmpty(state.GetCombatant(pawnId))
                .Where(combatant => combatant != null)
                .DistinctUntilChanged();
        }
        
        /// <summary>
        /// Emits every <see cref="CombatantState">Combatant</see> on the <see cref="BoardState">Board</see> whenever
        /// the Board changes.
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        public static IObservable<CombatantState> ObserveCombatants(this GameState state) =>
            state.ObserveBoardChanges()
                .SelectMany(board => board.Pawns.Values)
                .Where(pawn => pawn.Combatant != null)
                .Select(pawn => pawn.Combatant);

        /// <summary>
        /// Emits whenever the <see cref="CombatantState"/> associated with a pawn ID is modified
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        /// <param name="pawnId">ID of the combatant pawn</param>
        /// <returns>An <see cref="IObservable{CombatantState}"/> that emits when a combatant is modified</returns>
        public static IObservable<CombatantState> ObserveCombatantChanges(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId).SelectMany(combatant => combatant.Observe());
        }

        public static IObservable<HitState> ObserveLastHit(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId).Select(combatant => combatant.LastHit);
        }

        /// <summary>
        /// Emits whenever the <see cref="HitState"/> associated with a pawn ID is modified
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        /// <param name="pawnId">ID of the combatant pawn</param>
        /// <returns>An <see cref="IObservable{HitState}"/> that emits when the source <c>HitState</c> is modified
        /// </returns>
        public static IObservable<HitState> ObserveLastHitChanges(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId)
                .SelectMany(combatant => combatant.LastHit.Observe());
        }

        public static IObservable<AttackState> ObserveLastAttack(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId).Select(combatant => combatant.Attack);
        }

        public static IObservable<AttackState> ObserveLastAttackChanges(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId)
                .SelectMany(combatant => combatant.Attack.Observe());
        }

        public static IObservable<AttackState> ObserveAttackConnects(this GameState state, int pawnId) =>
            state.ObserveLastAttackChanges(pawnId)
                .DistinctUntilChanged(attack => attack.TargetId)
                .Where(attack => attack.TargetId != default);
        
        public static IObservable<AttackState> ObserveConfirmedAttack(this GameState state, int pawnId) =>
            state.ObserveAttackConnects(pawnId)
                .DistinctUntilChanged(attack => attack.Confirmed)
                .Where(attack => attack.Confirmed);

        public static IObservable<AttackState> ObserveBlockedAttack(this GameState state, int pawnId) =>
            state.ObserveAttackConnects(pawnId)
                .DistinctUntilChanged(attack => attack.Blocked)
                .Where(attack => attack.Blocked);

        public static IObservable<int> ObserveLockOn(this GameState state, int pawnId) =>
            state.ObserveCombatantChanges(pawnId)
                .Select(combatant => combatant.LockOnTarget)
                .DistinctUntilChanged();

        /// <summary>
        /// Emits the <see cref="PawnState">Pawn</see> IDs of all <see cref="CombatantState">Combatants</see> locked on
        /// by Combatants on the given team, whenever any of those team combatants lock on target changes.
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        /// <param name="team">The team whose lock-ons to observe</param>
        public static IObservable<int> ObserveLockOns(this GameState state, CombatantTeam team) =>
            state.ObserveCombatants()
                .Where(combatant => combatant.Stats.Team == team)
                .SelectMany(combatant => combatant.Observe())
                .Select(combatant => combatant.LockOnTarget)
                .DistinctUntilChanged()
                .Where(targetId => targetId != default);
        
        /// <summary>
        /// Emits the <see cref="PawnState">Pawn</see> IDs of all <see cref="CombatantState">Combatants</see> that were
        /// previously locked on by Combatants on the given team, whenever any of those team combatants' lock on target
        /// is cleared.
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        /// <param name="team">The team whose lock-offs to observe</param>
        public static IObservable<int> ObserveLockOffs(this GameState state, CombatantTeam team) =>
            state.ObserveCombatants()
                .Where(combatant => combatant.Stats.Team == team)
                .SelectMany(combatant => combatant.Observe())
                .Select(combatant => combatant.LockOnTarget)
                .DistinctUntilChanged()
                .Pairwise()
                .Where(pair => pair.Current == default)
                .Select(pair => pair.Previous);

        /// <summary>
        /// Emits a list of all enemy <see cref="PawnState">Pawns</see> on the <see cref="BoardState">Board</see>
        /// whenever the Board changes or a Pawn's team changes.
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        /// <param name="pawnId">ID of the friendly Pawn</param>
        public static IObservable<IEnumerable<PawnState>> ObserveHostiles(this GameState state, int pawnId) {
            var combatant = state.GetCombatant(pawnId);
            if (combatant != null) {
                return state.ObserveBoardChanges()
                    .SelectMany(
                        board => board.Pawns.Values
                            .Where(pawn => pawn.Combatant != null)
                            .Select(
                                pawn => pawn.Combatant.Stats.Observe()
                                    .DistinctUntilChanged(stats => stats.Team)
                            )
                            .Select(_ => board)
                    )
                    .Select(
                        board => board.Pawns.Values
                            .Where(pawn => pawn.Combatant != null &&
                                           pawn.Combatant.Stats.Team != combatant.Stats.Team)
                    );
            }
            return Observable.Empty<IEnumerable<PawnState>>();
        }

        public static IObservable<IEnumerable<PawnSpatial>> ObserveHostileSpatials(this GameState state, int pawnId) =>
            state.ObserveHostiles(pawnId)
                .Select(
                    pawns => pawns
                        .Where(pawn => pawn.Spatial != null)
                        .Select(pawn => pawn.Spatial)
                );
    }
}