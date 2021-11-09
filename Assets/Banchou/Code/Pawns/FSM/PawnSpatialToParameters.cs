using System;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class PawnSpatialToParameters : FSMBehaviour {
        [Header("Relative Offset")]
        [SerializeField] private string _offsetFloat;
        [SerializeField] private string _forwardOffsetFloat;
        [SerializeField] private string _rightOffsetFloat;
        [SerializeField] private string _upOffsetFloat;

        [Header("Position delta")]
        [SerializeField] private string _deltaSpeedFloat;
        [SerializeField] private string _forwardDeltaFloat;
        [SerializeField] private string _rightDeltaFloat;
        [SerializeField] private string _upDeltaFloat;
        
        [Space]
        [SerializeField] private string _isGroundedBool;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            PawnSpatial spatial = null;
            state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(s => spatial = s)
                .AddTo(this);

            void SubscribeToSpatial(string parameterName, Action<int> subscription) {
                if (!string.IsNullOrEmpty(parameterName)) {
                    var hash = Animator.StringToHash(parameterName);
                    ObserveStateUpdate
                        .Where(_ => spatial != null)
                        .Select(_ => hash)
                        .CatchIgnoreLog()
                        .Subscribe(subscription)
                        .AddTo(this);
                }
            }

            SubscribeToSpatial(_offsetFloat, hash => animator.SetFloat(hash, spatial.Offset.magnitude));
            SubscribeToSpatial(
                _forwardOffsetFloat, hash => animator.SetFloat(hash, Vector3.Dot(spatial.Offset, spatial.Forward))
            );
            SubscribeToSpatial(
                _rightOffsetFloat, hash => animator.SetFloat(hash, Vector3.Dot(spatial.Offset, spatial.Right))
            );
            SubscribeToSpatial(
                _upOffsetFloat, hash => animator.SetFloat(hash, Vector3.Dot(spatial.Offset, spatial.Up))
            );
            SubscribeToSpatial(_isGroundedBool, hash => animator.SetBool(hash, spatial.IsGrounded));
            
            SubscribeToSpatial(_deltaSpeedFloat, hash => animator.SetFloat(hash, spatial.AmbientVelocity.magnitude));
            SubscribeToSpatial(
                _forwardDeltaFloat,
                hash => animator.SetFloat(hash, Vector3.Dot(spatial.AmbientVelocity, spatial.Forward))
            );
            SubscribeToSpatial(
                _rightDeltaFloat,
                hash => animator.SetFloat(hash, Vector3.Dot(spatial.AmbientVelocity, spatial.Right))
            );
            SubscribeToSpatial(
                _upDeltaFloat,
                hash => animator.SetFloat(hash, Vector3.Dot(spatial.AmbientVelocity, spatial.Up))
            );
        }
    }
}