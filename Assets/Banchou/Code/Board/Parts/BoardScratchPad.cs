using System.Collections;
using UnityEngine;

namespace Banchou.Board.Part {
    public class BoardScratchPad : MonoBehaviour {
        public void Construct(
            GameState state,
            GetTime getTime
        ) {
            IEnumerator RunTest() {
                state.Players.AddPlayer(
                    playerId: 1,
                    prefabKey: "Local Player",
                    networkId: 0
                );
                for (int i = 1; i <= 1; i++) {
                    state.Board.AddPawn(
                        pawnId: i,
                        prefabKey: "Erho",
                        playerId: 1,
                        position: new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f)),
                        getTime()
                    );
                }
                yield break;
            }

            StartCoroutine(RunTest());
        }
    }
}