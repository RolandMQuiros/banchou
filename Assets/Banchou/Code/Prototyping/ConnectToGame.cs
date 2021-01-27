using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

using Banchou.Network;

namespace Banchou.Prototyping.Part {
    public class ConnectToGame : MonoBehaviour {
        private GameState _state;
        private IPAddress _ip = IPAddress.Parse("127.0.0.1");
        private int _port = 9050;
        private int _minPing = 0;
        private int _maxPing = 0;
        public bool Rollback { get; set; }

        public void Construct(GameState state) {
            _state = state;
        }

        public void ParseIP(string input) {
            IPAddress.TryParse(input, out _ip);
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

        public void Connect() {
            SceneManager.LoadScene("Banchou Board", LoadSceneMode.Single);
            _state.ConnectToServer(_ip.ToString(), _port, _minPing, _maxPing);
        }
    }
}