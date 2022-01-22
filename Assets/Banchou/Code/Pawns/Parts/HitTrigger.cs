using Banchou.Combatant;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Banchou.Pawn.Part {
    public class HitTrigger : MonoBehaviour {
        [field: SerializeField] public float DamageScale { get; private set; } = 1f;
        [field: SerializeField] public float KnockbackScale { get; private set; } = 1f;
        [field: SerializeField] public float RecoilScale { get; private set; } = 0f;
        [field: SerializeField] public float HitPauseScale { get; private set; } = 1f;
        [field: SerializeField] public float HitStunScale { get; private set; }  = 1f;
        
        private int _pawnId;
        private GameState _state;
        private HitState _hitState;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody body) {
            _state = state;
            _pawnId = getPawnId();
            _state.ObserveLastHit(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => _hitState = hit)
                .AddTo(this);

            body.OnTriggerEnterAsObservable()
                .Where(_ => isActiveAndEnabled)
                .Subscribe(OnVolumeEnter)
                .AddTo(this);
        }
        
        private void OnVolumeEnter(Collider other) {
            if (other.TryGetComponent<HurtVolume>(out var hurtVolume)) {
                var attack = _state.GetCombatantAttack(hurtVolume.PawnId);
                var alreadyHit = _hitState.AttackerId == hurtVolume.PawnId &&
                                 attack.AttackId == _hitState.AttackId;
                var canHurt = hurtVolume.PawnId != _pawnId && !alreadyHit &&
                              (hurtVolume.HurtHostile && _state.AreHostile(hurtVolume.PawnId, _pawnId) ||
                               hurtVolume.HurtFriendly && !_state.AreHostile(hurtVolume.PawnId, _pawnId));

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
                        other.ClosestPoint(transform.position),
                        hurtVolume.PawnId,
                        _pawnId,
                        knockback * KnockbackScale,
                        hurtVolume.Recoil * RecoilScale,
                        hurtVolume.HitPause * HitPauseScale,
                        hurtVolume.HitStun * HitStunScale,
                        Mathf.RoundToInt(hurtVolume.Damage * DamageScale),
                        hurtVolume.LockOffOnConfirm
                    );
                }
            }
        }
    }
}