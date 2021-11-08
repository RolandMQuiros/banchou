using UnityEngine;
using Banchou.DependencyInjection;
using Banchou.Pawn.Part;
using UniRx;

namespace Banchou.Board.Part {
    public delegate void RegisterPawnObject(int pawnId, GameObject gameObject);
    
    public class BoardContext : MonoBehaviour, IContext {
        [SerializeField] private BoardState _board;
        private readonly ReactiveDictionary<int, GameObject> _pawnInstances = new ReactiveDictionary<int, GameObject>();
        
        public void Construct(GameState state) {
            _board = state.Board;
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container.Bind(_board)
                .Bind<IReadOnlyReactiveDictionary<int, GameObject>>(_pawnInstances)
                .Bind<RegisterPawnObject>(RegisterPawnObject, type => type == typeof(PawnContext) ||
                                                                      type == typeof(PawnFactory));
        }

        private void RegisterPawnObject(int pawnId, GameObject pawnObject) {
            _pawnInstances[pawnId] = pawnObject;
        }
    }
}