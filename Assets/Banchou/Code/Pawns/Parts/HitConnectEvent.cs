using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Banchou.Pawn.Part {
    public class HitConnectEvent : MonoBehaviour {
        [Serializable] private class HitStateEvent : UnityEvent<HitState> { }

        [SerializeField] private bool _onConfirm = true;
        [SerializeField] private bool _onBlock = true;
        
        [SerializeField] private bool _moveToContact = true;
        [SerializeField] private bool _resetToDefaultPosition = true;

        [SerializeField] private bool _debugBreak;
        
        [SerializeField] private HitStateEvent _onEvent;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var originalPosition = transform.localPosition;
            state.ObserveLastHitChanges(getPawnId())
                .Where(hit => isActiveAndEnabled && 
                              (_onBlock && hit.Blocked || _onConfirm && !hit.Blocked))
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    if (_debugBreak) {
                        Debug.Break();
                    }
                    
                    if (_moveToContact) {
                        transform.position = hit.Contact;
                    } else if (_resetToDefaultPosition) {
                        transform.localPosition = originalPosition;
                    }
                    _onEvent.Invoke(hit);
                })
                .AddTo(this);
        }
    }
}