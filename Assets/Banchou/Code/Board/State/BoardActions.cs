using UnityEngine;
using Random = System.Random;
namespace Banchou.Board {
    namespace StateAction {
        public struct AddPawn {
            public int PawnId;
            public string PrefabKey;
            public int PlayerId;
            public Vector3 Position;
            public Vector3 Forward;
            public float When;
        }

        public struct RemovePawn {
            public int PawnId;
            public float When;
        }

        public struct ClearPawns {
            public float When;
        }

        public struct RollbackBoard {
            public BoardState Board;
        }

        public struct ResimulateBoard {
            public float CorrectionTime;
        }
    }

    public class BoardActions {
        private GetTime _getTime;
        private int _idCount = 0;

        public BoardActions(GetTime getTime) {
            _getTime = getTime;
        }

        public StateAction.AddPawn AddPawn(
            string prefabKey,
            int? pawnId = null,
            int playerId = 0,
            Vector3? position = null,
            Vector3? forward = null,
            float? when = null
        ){
            return new StateAction.AddPawn {
                PawnId = pawnId ?? ++_idCount,
                PrefabKey = prefabKey,
                PlayerId = playerId,
                Position = position ?? Vector3.zero,
                Forward = forward ?? Vector3.forward,
                When = when ?? _getTime()
            };
        }

        public StateAction.RemovePawn RemovePawn(int pawnId, float? when = null) => new StateAction.RemovePawn {
            PawnId = pawnId,
            When = when ?? _getTime()
        };

        public StateAction.ClearPawns Clear(float? when = null) => new StateAction.ClearPawns {
            When = when ?? _getTime()
        };

        public StateAction.RollbackBoard Rollback(BoardState board) => new StateAction.RollbackBoard {
            Board = board
        };

        public StateAction.ResimulateBoard Resimulate(float correctionTime) => new StateAction.ResimulateBoard {
            CorrectionTime = correctionTime
        };
    }
}