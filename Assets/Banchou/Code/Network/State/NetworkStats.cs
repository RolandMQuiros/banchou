using System;
using UnityEngine;

namespace Banchou.Network {
    [Serializable]
    public class NetworkStats : Notifiable<NetworkStats> {
        public int Ping => _ping;
        [SerializeField] private int _ping;
    }
}