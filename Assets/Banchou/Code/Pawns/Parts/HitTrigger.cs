using Banchou.Combatant;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Banchou.Pawn.Part {
    public class HitTrigger : MonoBehaviour {
        [SerializeField] private bool _isBlocking;
        
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
        private PawnSpatial _spatial;
        private HitState _hitState;

        public void Construct(GameState state, GetPawnId getPawnId, Rigidbody body) {
            _state = state;
            _pawnId = getPawnId();
            _state.ObservePawnSpatial(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _state.ObserveLastHit(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(hit => _hitState = hit)
                .AddTo(this);

            body.OnTriggerEnterAsObservable()
                .Where(_ => isActiveAndEnabled)
                .Subscribe(OnVolumeEnter)
                .AddTo(this);
        }

        private void Start() {
            // Need this for Enabled checkbox to show up
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
                    var position = transform.position;
                    var knockback = hurtVolume.GetKnockbackOn(position);
                    var isFrontAttack = Vector3.Dot(_spatial.Forward, hurtVolume.transform.position - position) > 0f;

                    _state.HitCombatant(
                        other.ClosestPoint(position),
                        hurtVolume.PawnId,
                        _pawnId,
                        _isBlocking && isFrontAttack,
                        knockback * (isFrontAttack ? _frontKnockbackScale : _rearKnockbackScale),
                        hurtVolume.Recoil * (isFrontAttack ? _frontRecoilScale : _rearRecoilScale),
                        hurtVolume.HitPause * (isFrontAttack ? _frontHitPauseScale : _rearHitPauseScale),
                        hurtVolume.HitStun * (isFrontAttack ? _frontHitStunScale : _rearHitStunScale),
                        Mathf.RoundToInt(hurtVolume.Damage * (isFrontAttack ? _frontDamageScale : _rearDamageScale)),
                        hurtVolume.LockOffOnConfirm
                    );
                }
            }
        }
    }
}