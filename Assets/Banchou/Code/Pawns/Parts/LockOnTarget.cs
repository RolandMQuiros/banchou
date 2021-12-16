using UnityEngine;

namespace Banchou.Pawn.Part {
    public class LockOnTarget : MonoBehaviour {
        [SerializeField] private Vector3 _offset;
        public int PawnId { get; private set; }
        public Vector3 Origin => transform.TransformPoint(_offset);


        public void Construct(GetPawnId getPawnId) {
            PawnId = getPawnId();
        }
        
        private void OnDrawGizmos() {
            Gizmos.DrawIcon(Origin, "EditorIcon_Crosshair");
        }
    }
}