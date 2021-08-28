using System;
using UnityEngine;

namespace Banchou.Player {
    [Serializable]
    [CreateAssetMenu(menuName = "Banchou/Player Command Gesture", fileName = "Player Command Gesture.asset")]
    public class PlayerCommandGesture : ScriptableObject {
        [field:SerializeField, Tooltip("Name of the gesture")]
        public string Name { get; private set; }
        
        [field:SerializeField, Tooltip("Sequence of inputs needed to fire the trigger")]
        public InputCommand[] Sequence { get; private set; }

        [field:SerializeField, Tooltip("Lifetime of stick inputs in the buffer, in seconds")]
        public float Lifetime { get; private set; } = 0.1666667f; // Approximately 10 frames
    }
}