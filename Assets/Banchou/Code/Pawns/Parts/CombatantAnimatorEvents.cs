using Banchou.Combatant;
using UniRx;
using UnityEngine;

#pragma warning disable CS0414

namespace Banchou.Pawn.Part {
    [Tooltip(Description)]
    public class CombatantAnimatorEvents : MonoBehaviour {
        public const string Description = "Contains Combatant methods called by animation events or the " +
                                          "AnimatorUnityEvents FSM Behaviour";
        // ReSharper disable once NotAccessedField.Local
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
        
        public void StartAttack() => _combatant.Attack.Start(_state.GetTime());
        public void ActivateAttack() => _combatant.Attack.Activate(_state.GetTime());
        public void RecoverAttack() => _combatant.Attack.Recover(_state.GetTime());
        public void FinishAttack() => _combatant.Attack.Finish(_state.GetTime());
    }
}