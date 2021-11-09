using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HurtVolume : MonoBehaviour {
        public int PawnId { get; private set; }
        
        #region Properties
        [field: SerializeField,
                Tooltip("How long after colliding with an enemy Combatant before applying damage again")]
        public float Interval { get; private set; }
        
        [field: SerializeField,
                Tooltip("How much health to subtract from the enemy Combatant")]
        public int Damage { get; private set; }
        
        [field: SerializeField,
                Tooltip("How long after contact, in seconds, the enemy Combatant the enemy stays in a stunned state")]
        public float HitStun { get; private set; }
        
        [field: SerializeField,
                Tooltip("How long both the attacking and attacked Combatant freeze in place after contact")]
        public float HitLag { get; private set; }
        
        [SerializeField,
         Tooltip("How much force is applied to the enemy Combatant on contact, in world space")]
        private Vector3 _knockback;
        public Vector3 Knockback => _transform.TransformVector(_knockback);

        [SerializeField,
         Tooltip("How much backwards force is applied to the attacker on contact, in world space")]
        private Vector3 _recoil;
        public Vector3 Recoil => _transform.TransformVector(_recoil);
        #endregion
        
        private Transform _transform;

        public void Construct(GetPawnId getPawnId) {
            PawnId = getPawnId();
            _transform = transform;
        }
    }
}