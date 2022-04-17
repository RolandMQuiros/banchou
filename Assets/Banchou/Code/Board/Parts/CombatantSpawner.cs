using Banchou.Combatant;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Banchou.Board.Part {
    public class CombatantSpawner : PawnSpawner {
        [field: SerializeField, AssetReferenceUILabelRestriction("Players")]
        public override AssetReferenceGameObject PlayerAsset { get; protected set; }
        
        [field: SerializeField, AssetReferenceUILabelRestriction("Combatants")]
        public override AssetReferenceGameObject PawnAsset { get; protected set; }

        [SerializeField] private CombatantTeam _team;
        [SerializeField] private int _maxHealth;
        
        protected override void Start() {
            base.Start();
            if (Application.isPlaying) {
                State.SetCombatant(_team, PawnId, _maxHealth);
            }
        }
    }
}