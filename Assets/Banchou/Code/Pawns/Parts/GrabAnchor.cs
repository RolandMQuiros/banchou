using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.Part {
    public class GrabAnchor : MonoBehaviour {
        [SerializeField] private GrabbedPose _targetPose;
        [SerializeField] private Vector3 _launchForce;

        private Transform _transform;
        private GameState _state;
        private int _pawnId;
        private PawnSpatial _spatial;
        private GrabState _grab;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _transform = transform;
            _state = state;
            _pawnId = getPawnId();
            state.ObservePawnSpatial(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            state.ObserveGrab(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(grab => _grab = grab)
                .AddTo(this);
        }

        private void OnEnable() {
            _grab.Hold(_transform.position, _transform.rotation, _targetPose, _state.GetTime());
        }

        private void OnDisable() {
            _grab.Release(
                _transform.position,
                _transform.rotation,
                Quaternion.LookRotation(_spatial.Forward) * _launchForce,
                _state.GetTime()
            );
        }

        private void FixedUpdate() {
            var now = _state.GetTime();
            _grab.Hold(_transform.position, _transform.rotation, _targetPose, now);
        }

        private void OnDrawGizmos() {
            var origin = transform.position;
            Gizmos.DrawIcon(origin, "EditorIcon_GrabbyHand", true);
            Gizmos.DrawLine(origin, origin + _launchForce);
        }
    }
}