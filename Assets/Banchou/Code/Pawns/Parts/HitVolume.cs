using System.Collections;
using System.Collections.Generic;
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
        private HurtVolume _hurtVolume;
        private readonly HashSet<HurtVolume> _collidedVolumes = new();

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            PawnId = getPawnId();
        }

        private void OnDisable() {
            StopAllCoroutines();
            _collidedVolumes.Clear();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<HurtVolume>(out var hurtVolume)) {
                var canHurt = hurtVolume.PawnId != PawnId &&
                              !_collidedVolumes.Contains(hurtVolume) &&
                              (hurtVolume.HurtHostile && _state.AreHostile(hurtVolume.PawnId, PawnId) ||
                               hurtVolume.HurtFriendly && !_state.AreHostile(hurtVolume.PawnId, PawnId));

                if (canHurt) {
                    _collidedVolumes.Add(hurtVolume);
                    StartCoroutine(HurtVolumeInterval(hurtVolume));

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

        private IEnumerator HurtVolumeInterval(HurtVolume volume) {
            var duration = 0f;
            var interval = volume.Interval + volume.HitPause;
            
            // Remove the attacking pawn from collided list when the interval duration passes or the hurt volume
            // is disabled
            while (duration < interval && volume.isActiveAndEnabled) {
                duration += _state.GetDeltaTime();
                yield return null;
            }
            
            _collidedVolumes.Remove(volume);
        }
    }
}