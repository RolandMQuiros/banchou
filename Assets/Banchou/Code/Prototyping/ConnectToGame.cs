using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

using Banchou.Network;

namespace Banchou.Prototyping.Part {
    public class ConnectToGame : MonoBehaviour {
        private GameState _state;
        private IPAddress _ip;
        private int _port;
        private int _minPing;
        private int _maxPing;
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