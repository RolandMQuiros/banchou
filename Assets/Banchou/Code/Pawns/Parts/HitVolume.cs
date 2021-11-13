using System;
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
        private int _lastAttackId;

        private readonly HashSet<int> _collidedPawns = new HashSet<int>();

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            PawnId = getPawnId();
        }

        private void OnTriggerEnter(Collider other) {
            var hurtVolume = other.GetComponent<HurtVolume>();
            if (hurtVolume != null && hurtVolume.PawnId != PawnId && !_collidedPawns.Contains(hurtVolume.PawnId)) {
                _collidedPawns.Add(hurtVolume.PawnId);
                StartCoroutine(HurtVolumeInterval(hurtVolume.Interval, hurtVolume.PawnId));
                Debug.Log($"Hit detected from Pawn {hurtVolume.PawnId} on Pawn {PawnId} at {_state.GetTime()}. Interval: {hurtVolume.Interval}, Collided volumes: {_collidedPawns.Count}");
                _state.HitCombatant(
                    hurtVolume.PawnId,
                    PawnId,
                    hurtVolume.Knockback * KnockbackScale,
                    Mathf.Min(hurtVolume.HitPause * HitPauseScale, hurtVolume.Interval),
                    hurtVolume.HitStun * HitStunScale,
                    Mathf.RoundToInt(hurtVolume.Damage * DamageScale)
                );
            }
        }

        private void OnTriggerStay(Collider other) => OnTriggerEnter(other);

        private IEnumerator HurtVolumeInterval(float interval, int pawnId) {
            yield return new WaitForSeconds(interval);
            _collidedPawns.Remove(pawnId);
        }
    }
}