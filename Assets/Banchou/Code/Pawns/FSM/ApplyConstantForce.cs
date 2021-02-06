using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ApplyConstantForce : FSMBehaviour {
        [SerializeField] private ForceMode _forceMode = ForceMode.Force;
        [SerializeField] private Vector3 _force = Vector3.zero;
        [SerializeField] private Vector3 _relativeForce = Vector3.zero;

        public void Construct(
            Rigidbody rigidbody
        ) {
            ObserveStateUpdate
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    if (_force != Vector3.zero) {
                        rigidbody.AddForce(_force, _forceMode);
                    }

                    if (_relativeForce != Vector3.zero) {
                        rigidbody.AddRelativeForce(_force, _forceMode);
                    }
                })
                .AddTo(this);
        }
    }
}