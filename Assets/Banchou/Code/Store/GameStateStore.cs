using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        public GameState State { get; private set; } = new GameState();
    }
    public delegate void ProcessStateActions();
}