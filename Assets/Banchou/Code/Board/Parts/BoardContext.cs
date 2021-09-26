using System.Collections.Generic;
using UnityEngine;

using Banchou.DependencyInjection;
using Banchou.Pawn.Part;

namespace Banchou.Board.Part {
    public class BoardContext : MonoBehaviour, IContext {
        [SerializeField] private BoardState _board;
        
        public void Construct(GameState state) {
            _board = state.Board;
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container.Bind(_board);
        }
    }
}