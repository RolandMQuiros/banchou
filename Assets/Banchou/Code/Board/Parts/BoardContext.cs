using Banchou.DependencyInjection;
using UnityEngine;

namespace Banchou.Board.Part {
    public class BoardContext : MonoBehaviour, IContext {
        private GameState _state;

        public void Construct(GameState state) {
            _state = state;
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_state.Board)
                .Bind(_state.Players);
        }
    }
}