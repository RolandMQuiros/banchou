using System.Collections;
using UnityEngine;

using Banchou.Player;

namespace Banchou.Board.Part {
    public class BoardScratchPad : MonoBehaviour {
        public void Construct(
            Dispatcher dispatch,
            GetState getState,
            BoardActions boardActions,
            PlayerActions playerActions
        ) {
            IEnumerator RunTest() {
                dispatch(
                    playerActions.Add(
                        playerId: 1,
                        prefabKey: "Local Player",
                        networkId: 0
                    )
                );
                dispatch(
                    boardActions.AddPawn(
                        pawnId: 1,
                        playerId: 1,
                        prefabKey: "Erho",
                        position: new Vector3(0f, 2f, 0f)
                    )
                );
                yield break;
            }

            StartCoroutine(RunTest());
        }
    }
}