using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        public void Construct(
            GetPawnId getPawnId,
            GetState getState,
            Dispatcher dispatch,
            PawnActions pawnActions,
            CharacterController controller
        ) {
            var pawnId = getPawnId();

            getState().ObservePawn(pawnId)
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    var spatial = getState().GetPawnSpatial(pawnId);

                    if (controller.transform.position != spatial.Position) {
                        controller.enabled = false;
                        controller.transform.position = spatial.Position;
                        controller.enabled = true;
                    }

                    if (spatial.Velocity != Vector3.zero) {
                        controller.Move(spatial.Velocity);
                        dispatch(
                            pawnActions.Moved(
                                position: controller.transform.position,
                                isGrounded: controller.isGrounded
                            )
                        );
                    }
                })
                .AddTo(this);
        }
    }
}