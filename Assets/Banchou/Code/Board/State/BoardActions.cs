
using UnityEngine;

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
    }

    public class BoardActions {
        private GetTime _getTime;
        public BoardActions(GetTime getTime) {
            _getTime = getTime;
        }

        public StateAction.AddPawn AddPawn(
            int pawnId,
            string prefabKey,
            int playerId = 0,
            Vector3? position = null,
            Vector3? forward = null,
            float? when = null
        ) => new StateAction.AddPawn {
            PawnId = pawnId,
            PrefabKey = prefabKey,
            PlayerId = playerId,
            Position = position ?? Vector3.zero,
            Forward = forward ?? Vector3.forward,
            When = when ?? _getTime()
        };

        public StateAction.RemovePawn RemovePawn(int pawnId, float? when = null) => new StateAction.RemovePawn {
            PawnId = pawnId,
            When = when ?? _getTime()
        };

        public StateAction.ClearPawns Clear(float? when = null) => new StateAction.ClearPawns {
            When = when ?? _getTime()
        };
    }
}