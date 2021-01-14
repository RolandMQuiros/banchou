using UnityEngine;

namespace Banchou.Pawn {
    namespace StateAction {
        public class MovePawn {
            public int PawnId;
            public Vector3 Direction;
            public float When;
        }

        public class TeleportPawn {
            public int PawnId;
            public Vector3 Position;
            public bool CancelMomentum;
            public float When;
        }

        public class PawnMoved {
            public int PawnId;
            public Vector3 Position;
            public bool CancelMomentum;
            public bool IsGrounded;
            public float When;
        }

        public class PawnAnimated {
            public AnimatorFrameData FrameData;
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
            int pawnId,
            float when
        ) => new StateAction.MovePawn {
            PawnId = pawnId,
            Direction = direction,
            When = when
        };
        public StateAction.MovePawn Move(Vector3 direction) => Move(direction, _pawnId, _getTime());

        public StateAction.PawnMoved Moved(
            Vector3 position,
            int pawnId,
            float when,
            bool isGrounded,
            bool cancelMomentum = true
        ) => new StateAction.PawnMoved {
            PawnId = pawnId,
            Position = position,
            IsGrounded = isGrounded,
            CancelMomentum = cancelMomentum,
            When = when
        };
        public StateAction.PawnMoved Moved(Vector3 position, bool isGrounded, bool cancelMomentum = true) => Moved(position, _pawnId, _getTime(), isGrounded, cancelMomentum);

        public StateAction.TeleportPawn Teleport(
            int pawnId,
            Vector3 position,
            bool cancelMomentum,
            float when
        ) => new StateAction.TeleportPawn {
            PawnId = pawnId,
            Position = position,
            CancelMomentum = cancelMomentum,
            When = when
        };
        public StateAction.TeleportPawn Teleport(Vector3 position) => Teleport(_pawnId, position, true, _getTime());

        public StateAction.PawnAnimated Animated(Animator animator, float when) => new StateAction.PawnAnimated {
            FrameData = new AnimatorFrameData(animator, when)
        };
        public StateAction.PawnAnimated Animated(Animator animator) => Animated(animator, _getTime());
    }
}