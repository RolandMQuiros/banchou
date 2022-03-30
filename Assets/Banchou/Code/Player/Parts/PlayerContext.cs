using Banchou.Board.Part;
using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayerContext : MonoBehaviour, IContext {
        [SerializeField] private int _reservedPlayerId;
        [SerializeField] private PlayerState _player;

        private GameState _state;
        private RegisterPlayerObject _registerPlayerObject;
        
        public void Construct(
            GameState state,
            RegisterPlayerObject registerPlayerObject,
            GetPlayerId getPlayerId = null
        ) {
            _state = state;
            
            // If the player isn't in the state (i.e., baked into the scene), register it
            if (getPlayerId == null) {
                _state.AddPlayer(out _player, _reservedPlayerId);
                _registerPlayerObject = registerPlayerObject;
            } else {
                // If this player is in the state, grab it
                _player = _state.GetPlayer(getPlayerId());
            }
            
            // If pawn is still null, destroy this object
            if (_player == null) {
                Destroy(gameObject);
            }
        }

        private void Start() {
            _registerPlayerObject?.Invoke(_player.PlayerId, gameObject);
        }

        public DiContainer InstallBindings(DiContainer container) {
            if (_player != null) {
                return container
                    .Bind(_player)
                    .Bind<GetPlayerId>(() => _player.PlayerId);
            }
            return container;
        }
    }
}
