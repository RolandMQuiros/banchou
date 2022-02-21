using UnityEngine;

namespace Banchou.Pawn.Part {
    [RequireComponent(typeof(Collider))]
    public class LockOnTarget : MonoBehaviour {
        public int PawnId { get; private set; }
        
        public void Construct(GetPawnId getPawnId) {
            PawnId = getPawnId();
        }
        
        private void OnDrawGizmos() {
            Gizmos.DrawIcon(transform.position, "EditorIcon_Crosshair");
        }
    }
}