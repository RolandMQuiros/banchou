using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Banchou.Pawn.Part {
    public class AttackConnectEvent : MonoBehaviour {
        [Serializable] private class AttackStateEvent : UnityEvent<AttackState> { }

        [SerializeField] private bool _onConfirm = true;
        [SerializeField] private bool _onBlock = true;
        [SerializeField] private bool _onGrab = true;
        
        [SerializeField] private bool _moveToContact = true;
        [SerializeField] private bool _resetToDefaultPosition = true;

        [SerializeField] private bool _debugBreak;
        
        [SerializeField] private AttackStateEvent _onEvent;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var originalPosition = transform.localPosition;
            state.ObserveAttacksBy(getPawnId())
                .Where(attack => isActiveAndEnabled &&
                                (_onConfirm && attack.HitStyle == HitStyle.Confirmed ||
                                 _onBlock && attack.HitStyle == HitStyle.Blocked ||
                                 _onGrab && attack.HitStyle == HitStyle.Grabbed))
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