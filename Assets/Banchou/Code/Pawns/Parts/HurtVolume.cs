using Banchou.Combatant;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Banchou.Pawn.Part {
    public class HurtVolume : MonoBehaviour {
        public enum ForceMethod {
            /// <summary>Force is applied relative to Pawn's forward vector </summary>
            ForwardRelative,
            /// <summary>Force is applied in the direction the target is from the Pawn</summary>
            Contact,
            /// <summary>
            ///     Force is applied in the direction the target is from the Pawn, projected on the plane created by the
            ///     Pawn's up vector
            /// </summary>
            ContactUpProjected
        }
        
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
                Tooltip("How long the attacking Combatant freezes in place after contact")]
        public float AttackPause { get; private set; }

        [field: SerializeField,
                Tooltip("How to apply knockback to Pawns hurt by this volume\n" +
                        "Static: Use a defined force vector\n" +
                        "Contact: Apply a force in the direction of the contact normal")]
        public ForceMethod KnockbackMethod { get; private set; } = ForceMethod.ForwardRelative;

        [SerializeField,
         Tooltip("Force applied to the enemy Combatant on contact, in world space. Applied after Hit Pause.")]
        private Vector3 _knockback;

        public Vector3 Knockback => Quaternion.LookRotation(_spatial.Forward) * _knockback;

        [field: SerializeField,
                Tooltip("How much force to apply to hurt Pawns")]
        public float KnockbackMagnitude { get; private set; }

        [field: SerializeField,
                Tooltip("World-space force added to calculated contact knockback")]
        public Vector3 AdditionalKnockback { get; private set; }

        [field: SerializeField,
                Tooltip("How to apply recoil to the Pawn using this Hurt Volume\n" +
                        "Static: Use a defined force vector\n" +
                        "Contact: Apply a force in the direction of the contact normal")]
        public ForceMethod RecoilMethod { get; private set; } = ForceMethod.ForwardRelative;
        
        [SerializeField,
         Tooltip("Backwards force applied to the attacker on contact, in world space. Applied after Hit Pause")]
        private Vector3 _recoil;

        public Vector3 Recoil => Quaternion.LookRotation(_spatial.Forward) * _recoil;
        
        [field: SerializeField,
                Tooltip("How much force to apply to attacking Pawn")]
        public float RecoilMagnitude { get; private set; }
        
        [field: SerializeField,
                Tooltip("World-space force added to calculated contact recoil")]
        public Vector3 AdditionalRecoil { get; private set; }


        [field: SerializeField,
                Tooltip("Remove lock-on if hit confirms. Use for attacks with huge knockback.")]
        public bool LockOffOnConfirm { get; private set; }

        [SerializeField] private UnityEvent _onHit;
        #endregion

        private GameState _state;
        private PawnSpatial _spatial;
        private AttackState _attack;

        public Vector3 GetKnockbackOn(Vector3 to) {
            switch (KnockbackMethod) {
                case ForceMethod.Contact:
                    return KnockbackMagnitude * (to - transform.position).normalized + AdditionalKnockback;
                case ForceMethod.ContactUpProjected:
                    return Vector3.ProjectOnPlane(
                        KnockbackMagnitude * (to - transform.position).normalized, _spatial.Up
                    ) + AdditionalKnockback;
                default:
                    return Knockback;
            }
        }
        
        public Vector3 GetRecoilOn(Vector3 to) {
            switch (RecoilMethod) {
                case ForceMethod.Contact:
                    return RecoilMagnitude * (to - transform.position).normalized + AdditionalRecoil;
                case ForceMethod.ContactUpProjected:
                    return Vector3.ProjectOnPlane(
                        RecoilMagnitude * (to - transform.position).normalized, _spatial.Up
                    ) + AdditionalRecoil;
                default:
                    return Knockback;
            }
        }

        public void Construct(GameState state, GetPawnId getPawnId) {
            PawnId = getPawnId();
            _state = state;

            _state.ObserveAttack(PawnId)
                .CatchIgnoreLog()
                .Subscribe(attack => _attack = attack)
                .AddTo(this);
            _state.ObservePawnSpatial(PawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            _state.ObserveAttackChanges(getPawnId())
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
                    AttackId = default;
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
                AttackId = _attack.Activate(now).AttackId;
            }
        }
    }
}