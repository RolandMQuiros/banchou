using UnityEngine;

namespace Banchou.Pawn.Part {
    public class HurtVolume : MonoBehaviour {
        public int PawnId { get; private set; }

        public void Construct(GetPawnId getPawnId) {
            PawnId = getPawnId();
        }
    }
}