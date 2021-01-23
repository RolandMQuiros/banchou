using System;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        [field: NonSerialized] public GameState State { get; private set; } = new GameState();
    }
    public delegate void ProcessStateActions();
}