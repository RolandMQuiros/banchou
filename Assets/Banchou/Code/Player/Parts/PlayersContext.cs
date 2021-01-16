using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayersContext : MonoBehaviour, IContext {
        [SerializeField] private PlayersState _players = null;

        public void Construct(GameState state) {
            _players = state.Players;
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container.Bind(_players);
        }
    }
}