using System;
using UnityEngine;

namespace Banchou.Network {
    [Serializable]
    public class NetworkStats : Notifiable<NetworkStats> {
        public int Ping => _ping;
        [SerializeField] private int _ping;

        public float LastUpdated => _lastUpdated;
        [SerializeField] private float _lastUpdated;

        public NetworkStats Update(int ping, float when) {
            _ping = ping;
            _lastUpdated = when;
            Notify();
            return this;
        }
    }
}