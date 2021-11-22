using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.Events;

namespace Banchou.Utility {
    [RequireComponent(typeof(Animator))]
    public class AnimatorUnityEvents : MonoBehaviour {
        [Serializable]
        private class ClipEvent {
            public string Name;
            public UnityEvent Event;
        }
        [SerializeField] private List<ClipEvent> _clipEvents;

        private Dictionary<string, UnityEvent> _runtimeClipEvents = new Dictionary<string, UnityEvent>();

        private void Awake() {
            _runtimeClipEvents = _clipEvents.ToDictionary(c => c.Name, c => c.Event);
        }

        public void Raise(string eventName) {
            if (_runtimeClipEvents.TryGetValue(eventName, out var clipEvent)) {
                clipEvent.Invoke();
            }
        }
    }
}