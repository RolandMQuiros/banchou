using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    [RequireComponent(typeof(ParticleSystem))]
    public class HitImpact : MonoBehaviour {
        [SerializeField] private bool _moveToContact = true;
        [SerializeField] private bool _resetToDefaultPosition = true;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            TryGetComponent<ParticleSystem>(out var vfx);
            var originalPosition = vfx.transform.localPosition;
            state.ObserveLastHitChanges(getPawnId())
                .Where(_ => isActiveAndEnabled)
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    if (_moveToContact) {
                        vfx.transform.position = hit.Contact;
                    } else if (_resetToDefaultPosition) {
                        vfx.transform.localPosition = originalPosition;
                    }
                    vfx.Play();
                })
                .AddTo(this);
        }
    }
}