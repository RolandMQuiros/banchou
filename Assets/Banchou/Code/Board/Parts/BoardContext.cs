using System.Collections.Generic;
using Banchou.DependencyInjection;
using UnityEngine;

namespace Banchou.Board.Part {
    public class BoardContext : MonoBehaviour, IContext {
        [SerializeField] private BoardState _board;
        private Dictionary<int, GameObject> _pawnObjects = new Dictionary<int, GameObject>();

        public void Construct(GameState state) {
            _board = state.Board;
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_board)
                .Bind<IDictionary<int, GameObject>>(_pawnObjects, type => type == typeof(PawnFactory))
                .Bind<IReadOnlyDictionary<int, GameObject>>(_pawnObjects);
        }
    }
}