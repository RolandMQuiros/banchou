using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        public void Construct(
            GetPawnId getPawnId,
            GetTime getTime,
            GetState getState,
            Dispatcher dispatch,
            CharacterController controller
        ) {
            var pawnId = getPawnId();

            getState().ObservePawn(pawnId)
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    var vectors = getState().GetPawnVectors(pawnId);

                    if (controller.transform.position != vectors.Position) {
                        controller.enabled = false;
                        controller.transform.position = vectors.Position;
                        controller.enabled = true;
                    }

                    if (vectors.Velocity != Vector3.zero) {
                        controller.Move(vectors.Velocity);
                        dispatch(
                            new StateAction.PawnMoved {
                                PawnId = pawnId,
                                Position = controller.transform.position,
                                When = getTime()
                            }
                        );
                    }
                })
                .AddTo(this);
        }
    }
}