using System.Collections;
using UnityEngine;

using Banchou.Board;
using Banchou.Player;

namespace Banchou.Prototyping.Part {
    public class BoardScratchPad : MonoBehaviour {
        public void Construct(
            GameState state,
            GetTime getTime
        ) {
            IEnumerator RunTest() {
                state.AddPlayer(
                    playerId: 1,
                    prefabKey: "Local Player"
                );
                for (int i = 1; i <= 100; i++) {
                    state.AddPawn(
                        pawnId: i,
                        prefabKey: "Erho",
                        playerId: 1,
                        position: new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f))
                    );
                }
                yield break;
            }

            StartCoroutine(RunTest());
        }
    }
}