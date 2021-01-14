using Banchou.Board;
// using Banchou.Network;
using Banchou.Player;

namespace Banchou {
    namespace StateAction {
        public class SyncGame {
            // public StageState Stage;
            public BoardState Board;
            public PlayersState Players;
            public float When;
        }
    }

    public class GameActions {
        private GetTime _getTime;
        public GameActions(GetTime getTime) {
            _getTime = getTime;
        }

        public StateAction.SyncGame Sync(BoardState board, PlayersState players, float? when = null) => new StateAction.SyncGame {
            Board = board,
            Players = players,
            When = when ?? _getTime()
        };
    }
}