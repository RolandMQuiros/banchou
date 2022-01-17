using Banchou.Combatant;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HitVolume : MonoBehaviour {
        public int PawnId { get; private set; }
        [field: SerializeField] public float DamageScale { get; private set; } = 1f;
        [field: SerializeField] public float KnockbackScale { get; private set; } = 1f;
        [field: SerializeField] public float HitPauseScale { get; private set; } = 1f;
        [field: SerializeField] public float HitStunScale { get; private set; }  = 1f;

        private GameState _state;
        private HitState _hitState;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            PawnId = getPawnId();
            _hitState = _state.GetCombatantLastHit(PawnId);
        }
        
        private void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<HurtVolume>(out var hurtVolume)) {
                var attack = _state.GetCombatantAttack(hurtVolume.PawnId);
                var alreadyHit = _hitState.AttackerId == hurtVolume.PawnId &&
                                 attack.AttackId == _hitState.AttackId;
                var canHurt = hurtVolume.PawnId != PawnId && !alreadyHit &&
                              (hurtVolume.HurtHostile && _state.AreHostile(hurtVolume.PawnId, PawnId) ||
                               hurtVolume.HurtFriendly && !_state.AreHostile(hurtVolume.PawnId, PawnId));

                if (canHurt) {
                    Vector3 knockback;
                    switch (hurtVolume.KnockbackMethod) {
                        case HurtVolume.ForceMethod.Contact:
                            knockback = hurtVolume.KnockbackMagnitude *
                                        (transform.position - hurtVolume.transform.position).normalized;
                            break;
                        default:
                            knockback = hurtVolume.Knockback;
                            break;
                    }

                    _state.HitCombatant(
                        other.transform.TransformVector(other.ClosestPoint(transform.position)),
                        hurtVolume.PawnId,
                        PawnId,
                        knockback * KnockbackScale,
                        hurtVolume.HitPause * HitPauseScale,
                        hurtVolume.HitStun * HitStunScale,
                        Mathf.RoundToInt(hurtVolume.Damage * DamageScale)
                    );
                }
            }
        }
    }
}