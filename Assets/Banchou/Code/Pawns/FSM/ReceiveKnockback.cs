using System.Collections;
using System.Collections.Generic;

using UniRx;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn.FSM {
    public class ReceiveKnockback : FSMBehaviour {
        [SerializeField] private bool _local;
        [Header("Output Parameters")]
        [SerializeField] private string _knockbackMagnitudeFloat;
        [SerializeField] private string _knockbackDirectionXFloat;
        [SerializeField] private string _knockbackDirectionYFloat;
        [SerializeField] private string _knockbackDirectionZFloat;
        
        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            var pawnId = getPawnId();
            var observeKnockback = ObserveStateUpdate
                .WithLatestFrom(
                    state.ObserveCombatant(pawnId),
                    (_, combatant) => combatant.KnockbackAt(state.GetTime())
                )
                .Where(knockback => knockback != Vector3.zero);

            if (!string.IsNullOrWhiteSpace(_knockbackMagnitudeFloat)) {
                var hash = Animator.StringToHash(_knockbackMagnitudeFloat);
                observeKnockback
                    .CatchIgnoreLog()
                    .Subscribe(knockback => {
                        animator.SetFloat(hash, knockback.magnitude);
                    })
                    .AddTo(this);
            }
            
            if (!string.IsNullOrWhiteSpace(_knockbackMagnitudeFloat)) {
                var hash = Animator.StringToHash(_knockbackDirectionXFloat);
                observeKnockback
                    .CatchIgnoreLog()
                    .Subscribe(knockback => {
                        animator.SetFloat(hash, knockback.x);
                    })
                    .AddTo(this);
            }
        }
    }
}