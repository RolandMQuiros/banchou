using System;
using System.Collections.Generic;
using System.Linq;
using Banchou.Player;
using UnityEngine;

namespace Banchou.Combatant {
    [CreateAssetMenu(menuName = "Banchou/Input Command Gesture Set", fileName = "Input Command Gesture Set.asset")]
    public class InputCommandGestureSet : ScriptableObject {
        [Serializable]
        private class InputCommandGesture {
            public string Name;
            public InputCommandGestureStep[] Steps;
        }
        [SerializeField] private InputCommandGesture[] _gestures;
        private Dictionary<string, InputCommandGestureStep[]> _runtimeGestures;

        public IReadOnlyDictionary<string, InputCommandGestureStep[]> Gestures {
            get {
                _runtimeGestures ??= _gestures.ToDictionary(g => g.Name, g => g.Steps);
                return _runtimeGestures;
            }
        }
    }

    [Serializable]
    public class InputCommandGestureStep {
        public InputCommand Command;
        public float Lifetime = 0.166667f;
    };
}