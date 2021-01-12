using UnityEngine;

namespace Banchou.Pawn.StateAction {
    public struct Move {
        public int PawnId;
        public Vector3 Direction;
        public float When;
    }

    public struct Teleport {
        public int PawnId;
        public Vector3 Position;
        public bool CancelMomentum;
        public float When;
    }

    public struct Moved {
        public int PawnId;
        public Vector3 Position;
        public float When;
    }
}