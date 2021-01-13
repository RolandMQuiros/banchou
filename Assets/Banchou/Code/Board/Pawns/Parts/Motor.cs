using UniRx;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public class Motor : MonoBehaviour {
        public void Construct(
            GetPawnId getPawnId,
            GetTime getTime,
            GameState state,
            Dispatcher dispatch,
            CharacterController controller
        ) {
            var pawnId = getPawnId();

            state.ObservePawnChanges(pawnId)
                .CatchIgnoreLog()
                .Subscribe(_ => {
                    var vectors = state.GetPawnVectors(pawnId);

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