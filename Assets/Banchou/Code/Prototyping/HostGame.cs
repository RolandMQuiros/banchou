using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.LoadScene("Banchou Board", LoadSceneMode.Single);

            _state
                .StartHost(_port, 1, _minPing, _maxPing)
                .LoadScene("Level/Sandbox")
                .AddPlayer(1, "Local Player");

            for (int i = 1; i <= 1; i++) {
                _state.AddPawn(
                    pawnId: i,
                    prefabKey: "Pawn/Isaac",
                    playerId: 1,
                    position: Vector3.up * 2f// new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f))
                );
            }
        }
    }
}