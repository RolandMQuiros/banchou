using UnityEngine;
using UnityEngine.SceneManagement;

using Banchou.Board;
using Banchou.Network;
using Banchou.Player;

using Random = UnityEngine.Random;

namespace Banchou.Prototyping.Part {
    public class HostGame : MonoBehaviour {
        private GameState _state;
        private GetTime _getTime;

        private int _port;
        private int _minPing;
        private int _maxPing;
        public bool Rollback { get; set; }

        public void Construct(GameState state, GetTime getTime) {
            _state = state;
            _getTime = getTime;
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

            _state.StartServer(_port, 20, _minPing, _maxPing)
                .LoadScene("Sandbox")
                .AddPlayer(1, "Local Player")
                .AddPawn(
                    pawnId: 1,
                    prefabKey: "Erho",
                    playerId: 1,
                    position: new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f)),
                    when: _getTime()
                );
        }
    }
}