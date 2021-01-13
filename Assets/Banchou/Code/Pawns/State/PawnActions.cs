using UnityEngine;

namespace Banchou.Pawn {
    namespace StateAction {
        public struct MovePawn {
            public int PawnId;
            public Vector3 Direction;
            public float When;
        }

        public struct TeleportPawn {
            public int PawnId;
            public Vector3 Position;
            public bool CancelMomentum;
            public float When;
        }

        public struct PawnMoved {
            public int PawnId;
            public Vector3 Position;
            public bool CancelMomentum;
            public bool IsGrounded;
            public float When;
        }
    }

    public class PawnActions {
        private int _pawnId;
        private GetTime _getTime;

        public PawnActions(GetPawnId getPawnId, GetTime getTime) {
            _pawnId = getPawnId();
            _getTime = getTime;
        }

        public StateAction.MovePawn Move(
            Vector3 direction,
            int? pawnId = null,
            float? when = null
        ) => new StateAction.MovePawn {
            PawnId = pawnId ?? _pawnId,
            Direction = direction,
            When = when ?? _getTime()
        };

        public StateAction.PawnMoved Moved(
            Vector3 position,
            bool isGrounded,
            bool cancelMomentum = true,
            int? pawnId = null,
            float? when = null
        ) => new StateAction.PawnMoved {
            PawnId = pawnId ?? _pawnId,
            Position = position,
            IsGrounded = isGrounded,
            CancelMomentum = cancelMomentum,
            When = when ?? _getTime()
        };

        public StateAction.TeleportPawn Teleport(
            Vector3 position,
            bool cancelMomentum = true,
            int? pawnId = null,
            float? when = null
        ) => new StateAction.TeleportPawn {
            PawnId = pawnId ?? _pawnId,
            Position = position,
            CancelMomentum = cancelMomentum,
            When = when ?? _getTime()
        };
    }
}