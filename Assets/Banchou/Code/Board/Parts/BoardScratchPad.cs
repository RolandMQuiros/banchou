using System.Collections;
using UnityEngine;
using Banchou.Board;
using Banchou.Pawn;

namespace Banchou.Board.Part {
    public class BoardScratchPad : MonoBehaviour {
        public void Construct(
            Dispatcher dispatch,
            GetState getState
        ) {
            IEnumerator RunTest() {
                var pawnId = getState().GetNextPawnId();
                dispatch(
                    new Board.StateAction.AddPawn {
                        Pawn = new PawnState(
                            id: pawnId,
                            playerId: 0,
                            prefabKey: "Erho",
                            position: new Vector3(0f, 2f, 0f)
                        )
                    }
                );
                yield return new WaitForSeconds(5f);
                dispatch(
                    new Board.StateAction.RemovePawn {
                        Id = pawnId
                    }
                );
            }

            StartCoroutine(RunTest());
        }
    }
}