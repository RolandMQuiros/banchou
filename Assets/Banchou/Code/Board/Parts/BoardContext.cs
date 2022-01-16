using UnityEngine;
using Banchou.DependencyInjection;
using Banchou.Pawn.Part;
using Banchou.Player;
using Banchou.Player.Part;
using UniRx;

namespace Banchou.Board.Part {
    public delegate void RegisterPlayerObject(int playerId, GameObject gameObject);
    public delegate IReadOnlyReactiveDictionary<int, GameObject> GetPlayerInstances();
    
    public delegate void RegisterPawnObject(int pawnId, GameObject gameObject);
    public delegate IReadOnlyReactiveDictionary<int, GameObject> GetPawnObjects();

    public class BoardContext : MonoBehaviour, IContext {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private BoardState _board;
        private readonly ReactiveDictionary<int, GameObject> _playerInstances = new();
        private readonly ReactiveDictionary<int, GameObject> _pawnInstances = new();

        public void Construct(GameState state) {
            _board = state.Board;
            if (_mainCamera == null) {
                _mainCamera = Camera.main;
            }

            state.ObserveRemovedPawns()
                .CatchIgnoreLog()
                .Subscribe(pawn => {
                    if (_pawnInstances.TryGetValue(pawn.PawnId, out var instance)) {
                        _pawnInstances.Remove(pawn.PawnId);
                        Destroy(instance);
                    }
                })
                .AddTo(this);

            state.ObserveRemovedPlayers()
                .CatchIgnoreLog()
                .Subscribe(player => {
                    if (_playerInstances.TryGetValue(player.PlayerId, out var instance)) {
                        _playerInstances.Remove(player.PlayerId);
                        Destroy(instance);
                    }
                })
                .AddTo(this);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container.Bind(_board)
                .Bind(_mainCamera)
                .Bind<GetPlayerInstances>(() => _playerInstances)
                .Bind<RegisterPlayerObject>(
                    (playerId, playerObject) => _playerInstances[playerId] = playerObject,
                    type => type == typeof(PlayerContext) || type == typeof(PlayerFactory)
                )
                .Bind<GetPawnObjects>(() => _pawnInstances)
                .Bind<RegisterPawnObject>(
                    (pawnId, pawnObject) => _pawnInstances[pawnId] = pawnObject,
                    type => type == typeof(PawnContext) || type == typeof(PawnFactory)
                );
        }
    }
}