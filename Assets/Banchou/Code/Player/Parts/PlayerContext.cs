using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayerContext : MonoBehaviour, IContext {
        [SerializeField] private PlayerState _player;

        public void Construct(GameState state, GetPlayerId getPlayerId) {
            _player = state.GetPlayer(getPlayerId());
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container.Bind(_player);
        }
    }
}
