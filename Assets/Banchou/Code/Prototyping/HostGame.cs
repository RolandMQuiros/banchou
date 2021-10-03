using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

using Banchou.Board;
using Banchou.Network;
using Banchou.Player;

namespace Banchou.Prototyping.Part {
    public class HostGame : MonoBehaviour {
        private GameState _state;

        private int _port = 9050;
        private int _minPing;
        private int _maxPing;
        public bool Rollback { get; set; }

        public void Construct(GameState state) {
            _state = state;
        }

        public void ParsePort(string input) {
            int.TryParse(input, out _port);
        }

        public void ParseMinPing(string input) {
            int.TryParse(input, out _minPing);
        }

        public void ParseMaxPing(string input) {
            int.TryParse(input, out _maxPing);
        }

        public void Host() {
            IEnumerator Run() {
                yield return SceneManager.LoadSceneAsync("Banchou Board", LoadSceneMode.Single);
                yield return new WaitForEndOfFrame(); // Let the Awakes run

                _state
                    // .StartHost("TestRoom", 1, _minPing, _maxPing)
                    .LoadScene("Level/Sandbox")
                    .AddPlayer(1, "Local Player");
                // yield return new WaitUntil(() => _state.IsConnected());
                // _state.AddPlayer(1, "Local Player");

                for (int i = 0; i < 10; i++) {
                    _state.AddPawn(
                        out var pawn,
                        pawnId: i,
                        prefabKey: "Pawn/Isaac",
                        playerId: 1,
                        position: Vector3.up * 2f
                    );
                    Debug.Log($"Added pawn {pawn.PawnId}");
                }
            }

            Observable.FromCoroutine(Run).Subscribe();
        }
    }
}