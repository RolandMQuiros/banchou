using Banchou.Combatant;
using UnityEngine;
using UniRx;
using UniRx.Diagnostics;

namespace Banchou.Pawn.FSM {
    public class AttackConnectPause : FSMBehaviour {
        [SerializeField, Tooltip("Whether or not to stop the rigid body's movement during the pause")]
        private bool _freezeOnHit = true;

        [SerializeField, Tooltip("Whether or not to restore momentum after freezing")]
        private bool _preserveMomentum = true;
        
        private GetDeltaTime _getDeltaTime;
        private float _originalSpeed;
        private float _pauseTime;
        private float _timeElapsed;

        private Rigidbody _rigidbody;
        private RigidbodyConstraints _originalConstraints;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator,
            Rigidbody rigidbody
        ) {
            var pawnId = getPawnId();
            _getDeltaTime = state.PawnDeltaTime(pawnId);
            _originalSpeed = animator.speed;
            _rigidbody = rigidbody;
            _originalConstraints = _rigidbody.constraints;

            state.ObserveConfirmedAttack(pawnId)
                .Where(_ => IsStateActive)
                .DistinctUntilChanged(attack => attack.AttackId) // Only pause once per attack
                .CatchIgnoreLog()
                .Subscribe(attack => {
                    _pauseTime = attack.PauseTime;
                    _timeElapsed = 0f;
                })
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _pauseTime = 0f;
            _timeElapsed = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            if (_pauseTime > 0f && _timeElapsed <= _pauseTime) {
                _timeElapsed += _getDeltaTime();
                if (_timeElapsed < _pauseTime) {
                    animator.speed = 0f;
                    if (_freezeOnHit) {
                        if (!_preserveMomentum) {
                            _rigidbody.velocity = Vector3.zero;
                        }

                        // This is messing up on weird transitions
                        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                    }
                } else {
                    animator.speed = _originalSpeed;
                    _rigidbody.constraints = _originalConstraints;
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            animator.speed = _originalSpeed;
            _rigidbody.constraints = _originalConstraints;
        }
    }
}