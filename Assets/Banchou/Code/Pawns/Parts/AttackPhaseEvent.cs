using System;
using Banchou.Combatant;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Banchou.Pawn.Part {
    public class AttackPhaseEvent : MonoBehaviour {
        [Serializable] private class AttackStateEvent : UnityEvent<AttackState> { } 
        
        [SerializeField] private AttackStateEvent _onNeutral;
        [SerializeField] private AttackStateEvent _onStarting;
        [SerializeField] private AttackStateEvent _onActive;
        [SerializeField] private AttackStateEvent _onRecovery;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            state.ObserveAttackChanges(getPawnId())
                .DistinctUntilChanged(attack => (attack.AttackId, attack.Phase))
                .CatchIgnoreLog()
                .Subscribe(attack => {
                    switch (attack.Phase) {
                        case AttackPhase.Neutral:
                            _onNeutral.Invoke(attack);
                            break;
                        case AttackPhase.Starting:
                            _onStarting.Invoke(attack);
                            break;
                        case AttackPhase.Active:
                            _onActive.Invoke(attack);
                            break;
                        case AttackPhase.Recover:
                            _onRecovery.Invoke(attack);
                            break;
                    }
                })
                .AddTo(this);
        }
    }
}