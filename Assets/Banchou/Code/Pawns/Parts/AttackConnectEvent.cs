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
        
        [SerializeField] private bool _moveToContact = true;
        [SerializeField] private bool _resetToDefaultPosition = true;

        [SerializeField] private bool _debugBreak;
        
        [SerializeField] private AttackStateEvent _onEvent;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            var originalPosition = transform.localPosition;
            state.ObserveAttackConnects(getPawnId())
                .Where(attack => isActiveAndEnabled &&
                                (_onConfirm && attack.Confirmed || _onBlock && attack.Blocked))
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