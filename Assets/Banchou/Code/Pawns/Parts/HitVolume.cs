using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HitVolume : MonoBehaviour {
        public int PawnId { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public float HitStun { get; private set; }
        [field: SerializeField] public float HitLag { get; private set; }
        
        [SerializeField] private Vector3 _knockback;
        public Vector3 Knockback => _transform.TransformVector(_knockback);

        [field: SerializeField] private Vector3 _recoil;
        public Vector3 Recoil => _transform.TransformVector(_recoil);

        private Transform _transform;

        public void Construct(GetPawnId getPawnId) {
            PawnId = getPawnId();
            _transform = transform;
        }
    }
}