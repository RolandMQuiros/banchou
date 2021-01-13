using System.Collections;
using UnityEngine;

namespace Banchou.Board.Part {
    public class BoardScratchPad : MonoBehaviour {
        public void Construct(
            Dispatcher dispatch,
            GetState getState,
            BoardActions boardActions
        ) {
            IEnumerator RunTest() {
                var pawnId = getState().GetNextPawnId();
                dispatch(
                    boardActions.AddPawn(
                        pawnId: pawnId,
                        playerId: 0,
                        prefabKey: "Erho",
                        position: new Vector3(0f, 2f, 0f)
                    )
                );
                yield return new WaitForSeconds(5f);
                dispatch(boardActions.RemovePawn(pawnId));
            }

            StartCoroutine(RunTest());
        }
    }
}