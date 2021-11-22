using Banchou.Combatant;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

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
         Tooltip("Force applied to the enemy Combatant on contact, in world space. Applied after Hit Pause.")]
        private Vector3 _knockback;

        public Vector3 Knockback => _transform.TransformVector(_knockback);

        [SerializeField,
         Tooltip("Backwards force applied to the attacker on contact, in world space. Applied after Hit Pause")]
        private Vector3 _recoil;

        public Vector3 Recoil => _transform.TransformVector(_recoil);

        [SerializeField] private UnityEvent _onHit;
        #endregion

        private HitState _hit;
        private Transform _transform;

        public void Construct(GameState state, GetPawnId getPawnId) {
            PawnId = getPawnId();
            _transform = transform;

            state.ObserveLastAttackChanges(getPawnId())
                .Where(_ => isActiveAndEnabled)
                .CatchIgnoreLog()
                .Subscribe(hit => {
                    Debug.Log($"Hit invoked at {hit.LastUpdated}");
                    _onHit.Invoke();
                })
                .AddTo(this);

            Debug.Log($"HurtVolume {gameObject.name}: {isActiveAndEnabled}");
        }

        public void Buh() {
            Debug.Log("Buh");
        }
    }
}