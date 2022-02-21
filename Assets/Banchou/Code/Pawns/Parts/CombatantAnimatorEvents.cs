using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    [Tooltip(Description)]
    public class CombatantAnimatorEvents : MonoBehaviour {
        public const string Description = "Contains Combatant methods called by animation events or the " +
                                          "AnimatorUnityEvents FSM Behaviour";
        [SerializeField, DevComment] private string _comment = Description;
        private GameState _state;
        private CombatantState _combatant;
        
        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            state.ObserveCombatant(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
        }

        public void Invincible() => _combatant.Defense.SetInvincibility(true, _state.GetTime());
        public void Vulnerable() => _combatant.Defense.SetInvincibility(false, _state.GetTime());
        public void StartAttack() => _combatant.Attack.Start(_state.GetTime());
        public void ActivateAttack() => _combatant.Attack.Activate(_state.GetTime());
        public void RecoverAttack() => _combatant.Attack.Recover(_state.GetTime());
        public void FinishAttack() => _combatant.Attack.Finish(_state.GetTime());
    }
}