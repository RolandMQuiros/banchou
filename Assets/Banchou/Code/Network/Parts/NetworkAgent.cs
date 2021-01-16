using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace Banchou.Network.Part {
    public class NetworkAgent : MonoBehaviour {
        private EventBasedNetListener _eventListener = new EventBasedNetListener();
        private NetManager _netManager;

        public void Construct(
            GameState state
        ) {
            _eventListener = new EventBasedNetListener();
            _netManager = new NetManager(_eventListener);
        }
    }
}