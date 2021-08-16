using System;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        [field:NonSerialized] public GameState State { get; private set; } = new GameState();
        [field:SerializeField] public string Version { get; private set; } = "0.0.1";
    }
    public delegate void ProcessStateActions();
}