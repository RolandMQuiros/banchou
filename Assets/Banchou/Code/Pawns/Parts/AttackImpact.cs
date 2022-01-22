using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    [RequireComponent(typeof(ParticleSystem))]
    public class AttackImpact : MonoBehaviour {
        [SerializeField] private bool _moveToContact = true;
        [SerializeField] private bool _resetToDefaultPosition = true;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            TryGetComponent<ParticleSystem>(out var vfx);
            
            var originalPosition = vfx.transform.localPosition;
            state.ObserveAttackConfirms(getPawnId())
                .Where(_ => isActiveAndEnabled)
                .CatchIgnoreLog()
                .Subscribe(attackState => {
                    if (_moveToContact) {
                        vfx.transform.position = attackState.Contact;
                    } else if (_resetToDefaultPosition) {
                        vfx.transform.localPosition = originalPosition;
                    }
                    
                    vfx.Play();
                })
                .AddTo(this);
        }
    }
}