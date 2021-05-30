using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Gravity : FSMBehaviour {
        [SerializeField] private Vector3 _acceleration = Vector3.down * 10f;
        [SerializeField] private bool _clearSpeedOnEnter = false;
        [SerializeField] private bool _clearSpeedOnExit = false;
        public void Construct(
            GameState state,
            PawnState pawn
        ) {
            var accumulated = Vector3.zero;

            if (_clearSpeedOnEnter) {
                ObserveStateExit
                    .CatchIgnoreLog()
                    .Subscribe(_ => { accumulated = Vector3.zero; })
                    .AddTo(this);
            }

            if (_clearSpeedOnExit) {
                ObserveStateExit
                    .CatchIgnoreLog()
                    .Subscribe(_ => { accumulated = Vector3.zero; })
                    .AddTo(this);
            }

            ObserveStateUpdate
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    if (pawn.Spatial.IsGrounded) {
                        accumulated = Vector3.zero;
                    } else {
                        accumulated += _acceleration * state.GetDeltaTime() * state.GetDeltaTime();
                        pawn.Spatial.Move(accumulated, state.GetTime());
                    }
                })
                .AddTo(this);
        }
    }
}