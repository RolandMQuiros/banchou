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
                for (int i = 1; i <= 100; i++) {
                    dispatch(
                        boardActions.AddPawn(
                            pawnId: i,
                            playerId: 1,
                            prefabKey: "Erho",
                            position: new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f))
                        )
                    );
                }
                yield break;
            }

            StartCoroutine(RunTest());
        }
    }
}