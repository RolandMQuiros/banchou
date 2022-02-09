using System.Collections;
using System.Collections.Generic;
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
        private PawnSpatial _spatial;
        private HitState _hitState;

        private readonly HashSet<HurtVolume> _collidedVolumes = new();

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
                var alreadyHit = _collidedVolumes.Contains(hurtVolume);
                var canHurt = hurtVolume.PawnId != _pawnId && !alreadyHit &&
                              (hurtVolume.HurtHostile && _state.AreHostile(hurtVolume.PawnId, _pawnId) ||
                               hurtVolume.HurtFriendly && !_state.AreHostile(hurtVolume.PawnId, _pawnId));

                if (canHurt) {
                    var position = transform.position;
                    var knockback = hurtVolume.GetKnockbackOn(position);
                    var isFrontAttack = Vector3.Dot(_spatial.Forward, hurtVolume.transform.position - position) > 0f;
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
                        hurtVolume.HitStun * (isFrontAttack ? _frontHitStunScale : _rearHitStunScale),
                        Mathf.RoundToInt(hurtVolume.Damage * (isFrontAttack ? _frontDamageScale : _rearDamageScale)),
                        hurtVolume.IsGrab,
                        hurtVolume.LockOffOnConfirm
                    );
                    StartCoroutine(RunInterval(hurtVolume));
                }
            }
        }

        private IEnumerator RunInterval(HurtVolume hurtVolume) {
            var time = 0f;
            _collidedVolumes.Add(hurtVolume);
            while (hurtVolume.isActiveAndEnabled && time < hurtVolume.Interval + hurtVolume.HitPause) {
                time += _state.GetDeltaTime();
                yield return null;
            }
            _collidedVolumes.Remove(hurtVolume);
        }
    }
}