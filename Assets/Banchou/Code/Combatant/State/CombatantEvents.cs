using System;
using Banchou.Pawn;
using UniRx;
using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantEvents {
        /// <summary>
        /// Emits whenever a new <see cref="CombatantState"/> instance is associated with a pawn ID
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState"/></param>
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
        /// Emits whenever the <see cref="CombatantState"/> associated with a pawn ID is modified
        /// </summary>
        /// <param name="state">Reference to the <see cref="GameState"/></param>
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
        /// <param name="state">Reference to the <see cref="GameState"/></param>
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
    }
}