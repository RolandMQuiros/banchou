using Banchou.Combatant;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Banchou.Pawn.Part {
    public class HurtVolume : MonoBehaviour {
        public enum ForceMethod { Static, Contact }
        
        public int PawnId { get; private set; }

        public int AttackId { get; private set; }

        #region Properties
        [field: SerializeField, Tooltip("Whether or not to hurt friendly Pawns")]
        public bool HurtFriendly { get; private set; } = false;

        [field: SerializeField, Tooltip("Whether or not to hurt hostile Pawns")]
        public bool HurtHostile { get; private set; } = true;

        [field: SerializeField, Tooltip("Whether or not this attack is a grab")]
        public bool IsGrab { get; private set; } = false;
        
        [field: SerializeField,
                Tooltip("How long after colliding with an enemy Combatant before applying damage again. " +
                        "Usually keep this at a high value unless you want one volume to apply damage " +
                        "multiple times.")]
        public float Interval { get; private set; } = 1f;

        [field: SerializeField,
                Tooltip("How much health to subtract from the enemy Combatant")]
        public int Damage { get; private set; }

        [field: SerializeField,
                Tooltip("How long both the attacking and attacked Combatant freeze in place after contact")]
        public float HitPause { get; private set; }

        [field: SerializeField,
                Tooltip("How long after contact, in seconds, the enemy Combatant the enemy stays in a stunned state." +
                        " Runs after Hit Pause completes.")]
        public float HitStun { get; private set; } = 0.5f;

        [field: SerializeField,
                Tooltip("How to apply knockback to Pawns hurt by this volume\n" +
                        "Static: Use a defined force vector\n" +
                        "Contact: Apply a force in the direction of the contact normal")]
        public ForceMethod KnockbackMethod { get; private set; } = ForceMethod.Static;

        [SerializeField,
         Tooltip("Force applied to the enemy Combatant on contact, in world space. Applied after Hit Pause.")]
        private Vector3 _knockback;

        public Vector3 Knockback => _transform.TransformVector(_knockback);

        [field: SerializeField,
                Tooltip("How much force to apply to hurt Pawns")]
        public float KnockbackMagnitude { get; private set; }

        [field: SerializeField,
                Tooltip("How to apply recoil to the Pawn using this Hurt Volume\n" +
                        "Static: Use a defined force vector\n" +
                        "Contact: Apply a force in the direction of the contact normal")]
        public ForceMethod RecoilMethod { get; private set; } = ForceMethod.Static;
        
        [SerializeField,
         Tooltip("Backwards force applied to the attacker on contact, in world space. Applied after Hit Pause")]
        private Vector3 _recoil;

        public Vector3 Recoil => _transform.TransformVector(_recoil);

        [field: SerializeField,
         Tooltip("Where, in local Pawn space, to hold the grab target")]
        public Vector3 GrabTargetPosition { get; private set; }

        [field: SerializeField,
                Tooltip("Remove lock-on if hit confirms. Use for attacks with huge knockback.")]
        public bool LockOffOnConfirm { get; private set; }

        [SerializeField] private UnityEvent _onHit;
        #endregion

        private GameState _state;
        private AttackState _attack;
        private Transform _transform;

        public Vector3 GetKnockbackOn(Vector3 to) {
            switch (KnockbackMethod) {
                case ForceMethod.Contact:
                    return KnockbackMagnitude * (to - transform.position).normalized;
                default:
                    return Knockback;
            }
        }

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            PawnId = getPawnId();
            _attack = _state.GetCombatantAttack(PawnId);
            _transform = transform;

            _state.ObserveLastAttackChanges(getPawnId())
                .Where(_ => isActiveAndEnabled)
                .CatchIgnoreLog()
                .Subscribe(_ => { _onHit.Invoke(); })
                .AddTo(this);

            this.OnEnableAsObservable()
                .Subscribe(_ => {
                    AttackId = _attack.Activate(_state.GetTime()).AttackId;
                })
                .AddTo(this);

            this.OnDisableAsObservable()
                .Subscribe(_ => {
                    if (AttackId == _attack.AttackId) {
                        _attack.Recover(_state.GetTime());
                    }
                })
                .AddTo(this);
        }

        private void FixedUpdate() {
            var now = _state.GetTime();
            if (
                AttackId == _attack.AttackId &&
                _attack.Phase == AttackPhase.Active &&
                now - _attack.LastUpdated > Interval
            ) {
                _attack.Reactivate(now);
            }
        }
    }
}