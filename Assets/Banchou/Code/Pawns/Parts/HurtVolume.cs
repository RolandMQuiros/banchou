using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HurtVolume : MonoBehaviour {
        public int PawnId { get; private set; }
        
        #region Properties
        [field: SerializeField,
                Tooltip("How long after colliding with an enemy Combatant before applying damage again. " + 
                        "Should be several frames longer than Hit Pause to avoid multiple collisions.")]
        public float Interval { get; private set; }
        
        [field: SerializeField,
                Tooltip("How much health to subtract from the enemy Combatant")]
        public int Damage { get; private set; }
        
        [field: SerializeField,
                Tooltip("How long both the attacking and attacked Combatant freeze in place after contact")]
        public float HitPause { get; private set; }
        
        [field: SerializeField,
                Tooltip("How long after contact, in seconds, the enemy Combatant the enemy stays in a stunned state." +
                        " Runs after Hit Pause completes.")]
        public float HitStun { get; private set; }

        [SerializeField,
         Tooltip("Whether or not contact with enemy will apply a force to that enemy")]
        private bool _applyKnockback = true;
        
        [SerializeField,
         Tooltip("Direction of force applied to the enemy Combatant on contact, in world space." +
                 " Applied after Hit Pause.")]
        private Vector3 _knockbackDirection;

        [SerializeField,
         Tooltip("Magnitude of force applied to the enemy Combatant on contact, in world space." +
                 " Applied after Hit Pause.")]
        private float _knockbackScale = 1f;
        
        public Vector3 Knockback =>
                _applyKnockback ? _knockbackScale * _transform.TransformVector(_knockbackDirection) : Vector3.zero;
        
        [SerializeField,
         Tooltip("Whether or not contact with enemy will apply a force to the attacker")]
        private bool _applyRecoil = true;
        
        [SerializeField,
         Tooltip("Magnitude of backwards force applied to the attacker on contact, in world space." +
                 " Applied after Hit Pause")]
        private float _recoilScale = 1f;
        
        [SerializeField,
         Tooltip("Direction of backwards force applied to the attacker on contact, in world space." +
                 " Applied after Hit Pause")]
        private Vector3 _recoilDirection;

        public Vector3 Recoil =>
                _applyRecoil ? _recoilScale * _transform.TransformVector(_recoilDirection) : Vector3.zero;
        #endregion
        
        private Transform _transform;

        public void Construct(GetPawnId getPawnId) {
            PawnId = getPawnId();
            _transform = transform;
        }
    }
}