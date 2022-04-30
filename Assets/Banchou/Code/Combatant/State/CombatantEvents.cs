using System;
using System.Collections.Generic;
using System.Linq;
using Banchou.Board;
using Banchou.Pawn;
using UniRx;
using UnityEngine;

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
                .Where(combatant => combatant != null)
                .DistinctUntilChanged();
        }
        
        /// <summary>
        /// Emits every <see cref="CombatantState">Combatant</see> on the <see cref="BoardState">Board</see> whenever
        /// the Board changes.
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState">game state</see></param>
        public static IObservable<CombatantState> ObserveCombatants(this GameState state) =>
            state.ObservePawns()
                .SelectMany(pawn => pawn.Observe())
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

        public static IObservable<DefensiveState> ObserveDefense(this GameState state, int pawnId) =>
            state.ObserveCombatant(pawnId).SelectMany(combatant => combatant.Defense.Observe());

        public static IObservable<AttackState> ObserveAttack(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId).Select(combatant => combatant.Attack);
        }
        
        public static IObservable<AttackState> ObserveAttackChanges(this GameState state, int pawnId) {
            return state.ObserveCombatant(pawnId)
                .SelectMany(combatant => combatant.Attack.Observe());
        }

        public static IObservable<HitState> ObserveHits(this GameState state) =>
            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Hit.Observe());

        public static IObservable<HitState> ObserveHitsOn(this GameState state, int pawnId) =>
            state.ObserveCombatant(pawnId)
                .SelectMany(combatant => combatant.Hit.Observe());

        public static IObservable<AttackState> ObserveAttacksBy(this GameState state, int pawnId) =>
            state.ObserveAttackChanges(pawnId)
                .DistinctUntilChanged(attack => attack.TargetId)
                .Where(attack => attack.TargetId != default);

        public static IObservable<AttackState> ObserveAttacksOn(this GameState state, int pawnId) =>
            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Attack.Observe())
                .Where(attack => attack.TargetId == pawnId)
                .DistinctUntilChanged(attack => attack.AttackId);

        public static IObservable<GrabState> ObserveGrabsBy(this GameState state, int pawnId) =>
            state.ObserveCombatant(pawnId)
                .Select(combatant => combatant.Grab);

        public static IObservable<GrabState> ObserveGrabContactsOn(this GameState state, int pawnId) =>
            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Grab.Observe())
                .DistinctUntilChanged(grab => grab.Phase)
                .Where(grab => grab.Phase == GrabPhase.Contacted && grab.TargetId == pawnId);
        
        public static IObservable<GrabState> ObserveGrabHoldsOn(this GameState state, int pawnId) =>
            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Grab.Observe())
                .DistinctUntilChanged(grab => grab.Phase)
                .Where(grab => grab.Phase == GrabPhase.Held && grab.TargetId == pawnId);
        
        public static IObservable<GrabState> ObserveGrabReleasesOn(this GameState state, int pawnId) =>
            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Grab.Observe())
                .DistinctUntilChanged(grab => grab.Phase)
                .Where(grab => grab.Phase == GrabPhase.Released && grab.TargetId == pawnId);
        
        public static IObservable<GrabState> ObserveGrabInterruptionsOn(this GameState state, int pawnId) =>
            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Grab.Observe())
                .DistinctUntilChanged(grab => grab.Phase)
                .Where(grab => grab.Phase == GrabPhase.Interrupted && grab.TargetId == pawnId);
        
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
                .SelectMany(combatant => combatant.Observe())
                .Where(combatant => combatant.Stats.Team == team)
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
                .SelectMany(combatant => combatant.Observe())
                .Where(combatant => combatant.Stats.Team == team)
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

        public static IObservable<float> ObserveNormalizedHitPause<T>(
            this GameState state, int pawnId, IObservable<T> source
        ) =>
            source.WithLatestFrom(state.ObserveHitsOn(pawnId), (_, hit) => hit)
                .WithLatestFrom(
                    state.ObservePawnTimeScale(pawnId),
                    (hit, timeScale) => hit.NormalizedStunTime(timeScale, state.GetTime()) 
                );

        public static IObservable<float> ObserveNormalizedHitStun<T>(
            this GameState state, int pawnId, IObservable<T> source
        ) =>
            source.WithLatestFrom(state.ObserveHitsOn(pawnId), (_, hit) => hit)
                .WithLatestFrom(
                    state.ObservePawnTimeScale(pawnId),
                    (hit, timeScale) => hit.NormalizedStunTime(timeScale, state.GetTime()) 
                );

        public static IObservable<float> ObserveNormalizedAttackPause<T>(
            this GameState state, int pawnId, IObservable<T> source
        ) =>
            source.WithLatestFrom(state.ObserveAttacksBy(pawnId), (_, attack) => attack)
                .WithLatestFrom(
                    state.ObservePawnTimeScale(pawnId),
                    (attack, timeScale) => attack.NormalizedPauseTime(timeScale, state.GetTime()) 
                );
    }
}