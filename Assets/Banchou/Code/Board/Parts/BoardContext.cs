using UnityEngine;
using Banchou.DependencyInjection;
using Banchou.Pawn.Part;
using Banchou.Player.Part;
using UniRx;

namespace Banchou.Board.Part {
    public delegate void RegisterPlayerObject(int playerId, GameObject gameObject);
    public delegate IReadOnlyReactiveDictionary<int, GameObject> GetPlayerInstances();

    public delegate ReactiveDictionary<int, GameObject> GetMutablePlayerInstances();

    public delegate void RegisterPawnObject(int pawnId, GameObject gameObject);
    public delegate IReadOnlyReactiveDictionary<int, GameObject> GetPawnObjects();
    public delegate ReactiveDictionary<int, GameObject> GetMutablePawnObjects();

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
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container.Bind(_board)
                .Bind(_mainCamera)
                .Bind<GetPlayerInstances>(() => _playerInstances)
                .Bind<GetMutablePlayerInstances>(() => _playerInstances)
                .Bind<RegisterPlayerObject>(
                    (playerId, playerObject) => _playerInstances[playerId] = playerObject,
                    type => type == typeof(PlayerContext) || type == typeof(PlayerFactory)
                )
                .Bind<GetPawnObjects>(() => _pawnInstances)
                .Bind<GetMutablePawnObjects>(() => _pawnInstances)
                .Bind<RegisterPawnObject>(
                    (pawnId, pawnObject) => _pawnInstances[pawnId] = pawnObject,
                    type => type == typeof(PawnContext) || type == typeof(PawnFactory)
                );
        }
    }
}