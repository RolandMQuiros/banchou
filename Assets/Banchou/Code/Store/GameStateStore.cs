using System;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        [field:SerializeField] public GameState State { get; } = new();
        [field:SerializeField] public string Version { get; } = "0.0.1";
    }
    public delegate void ProcessStateActions();
}