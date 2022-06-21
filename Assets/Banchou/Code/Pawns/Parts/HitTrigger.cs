using Banchou.Combatant;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Banchou.Pawn.Part {
    public class HitTrigger : MonoBehaviour {
        [SerializeField] private bool _isBlocking;
        [SerializeField] private bool _isCounterable;
        
        [Header("Front Damage Scales")]
        [SerializeField] private float _frontDamageScale = 1f;
        [SerializeField] private float _frontKnockbackScale = 1f;
        [SerializeField] private float _frontRecoilScale = 0f;
        [SerializeField] private float _frontHitPauseScale = 1f;
        [SerializeField] private float _frontHitStunScale = 1f;
        
        [Header("Rear Damage Scales")]
        [SerializeField] private float _rearDamageScale = 1f;
        [SerializeField] private float _rearKnockbackScale = 1f;
        [SerializeField] private float _rearRecoilScale = 0f;
        [SerializeField] private float _rearHitPauseScale = 1f;
        [SerializeField] private float _rearHitStunScale = 1f;
        
        private int _pawnId;
        private GameState _state;
        private CombatantState _combatant;
        private PawnSpatial _spatial;
        private HitState _hit;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody body) {
            _state = state;
            _pawnId = getPawnId();
            _spatial = state.GetPawnSpatial(_pawnId);
            _combatant = _state.GetCombatant(_pawnId);

            _state.ObserveCombatant(_pawnId)
                .Select(combatant => combatant.Defense.IsInvincible)
                .DistinctUntilChanged()
                .CatchIgnoreLog()
                .Subscribe(isInvincible => {
                    if (enabled == isInvincible) {
                        enabled = !isInvincible;
                    }
                })
                .AddTo(this);

            _state.ObserveHitsOn(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => _hit = hit)
                .AddTo(this);

            body.OnTriggerEnterAsObservable()
                .Merge(body.OnTriggerStayAsObservable())
                .Where(_ => isActiveAndEnabled)
                .Subscribe(OnVolumeEnter)
                .AddTo(this);
        }

        private void OnEnable() {
            var defense = _combatant?.Defense;
            if (defense?.IsInvincible == true) {
                defense.SetInvincibility(false, _state.GetTime());
            }
        }

        private void OnDisable() {
            var defense = _combatant?.Defense;
            if (defense?.IsInvincible == false) {
                defense.SetInvincibility(true, _state.GetTime());
            }
        }

        private void OnVolumeEnter(Collider other) {
            if (other.TryGetComponent<HurtVolume>(out var hurtVolume)) {
                var alreadyHit = _hit?.AttackerId == hurtVolume.PawnId &&
                                 _hit?.AttackId == hurtVolume.AttackId;
                var canHurt = hurtVolume.PawnId != _pawnId && !alreadyHit &&
                              (hurtVolume.HurtHostile && _state.AreHostile(hurtVolume.PawnId, _pawnId) ||
                               hurtVolume.HurtFriendly && !_state.AreHostile(hurtVolume.PawnId, _pawnId));

                if (canHurt) {
                    var position = transform.position;
                    var knockback = hurtVolume.GetKnockbackOn(position);
                    var isFrontAttack = Vector3.Dot(_spatial.Forward, hurtVolume.transform.position - position) > -0.25f;
                    var blocked = _isBlocking && isFrontAttack;

                    _state.HitCombatant(
                        other.ClosestPoint(position),
                        hurtVolume.AttackId,
                        hurtVolume.PawnId,
                        _pawnId,
                        blocked,
                        !blocked && _isCounterable,
                        knockback * (isFrontAttack ? _frontKnockbackScale : _rearKnockbackScale),
                        hurtVolume.Recoil * (isFrontAttack ? _frontRecoilScale : _rearRecoilScale),
                        hurtVolume.HitPause * (isFrontAttack ? _frontHitPauseScale : _rearHitPauseScale),
                        hurtVolume.AttackPause,
                        hurtVolume.HitStun * (isFrontAttack ? _frontHitStunScale : _rearHitStunScale),
                        Mathf.RoundToInt(hurtVolume.Damage * (isFrontAttack ? _frontDamageScale : _rearDamageScale)),
                        hurtVolume.IsGrab,
                        hurtVolume.LockOffOnConfirm
                    );
                }
            }
        }
    }
}