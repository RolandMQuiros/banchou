using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Banchou.Pawn.Part {
    public class HitConnectEvent : MonoBehaviour {
        [Serializable] private class HitStateEvent : UnityEvent<AttackState> { }

        [SerializeField] private bool _onConfirm = true;
        [SerializeField] private bool _onBlock = true;
        
        [SerializeField] private bool _moveToContact = true;
        [SerializeField] private bool _resetToDefaultPosition = true;

        [SerializeField] private bool _debugBreak;
        
        [SerializeField] private HitStateEvent _onEvent;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var originalPosition = transform.localPosition;
            state.ObserveHitsOn(getPawnId())
                .Where(attack => isActiveAndEnabled && 
                              (_onBlock && attack.Blocked || _onConfirm && !attack.Blocked))
                .CatchIgnoreLog()
                .Subscribe(attack => {
                    if (_debugBreak) {
                        Debug.Break();
                    }
                    
                    if (_moveToContact) {
                        transform.position = attack.Contact;
                    } else if (_resetToDefaultPosition) {
                        transform.localPosition = originalPosition;
                    }
                    _onEvent.Invoke(attack);
                })
                .AddTo(this);
        }
    }
}