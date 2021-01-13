using Banchou.DependencyInjection;
using UnityEngine;

using Banchou.Player;

namespace Banchou.Board.Part {
    public class BoardContext : MonoBehaviour, IContext {
        private BoardActions _boardActions;
        private PlayerActions _playerActions;

        public void Construct(GetTime getTime) {
            _boardActions = new BoardActions(getTime);
            _playerActions = new PlayerActions(getTime);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_boardActions)
                .Bind(_playerActions);
        }
    }
}