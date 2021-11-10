using Banchou.Combatant;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class ShakeToKnockback : FSMBehaviour {
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private float _multiplier = 1f;
        [SerializeField] private float _maximumOffset = 2f;

        private GameState _state;
        private HitState _hit;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _state.ObserveLastHit(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(hit => { _hit = hit; })
                .AddTo(this);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            var xform = animator.transform;
            var stateTime = _hit.NormalizedPauseTimeAt(_state.GetTime());
            var magnitude = _multiplier * Mathf.Clamp01(_hit.Knockback.magnitude / _maximumOffset) *
                            _curve.Evaluate(stateTime);
            var offset = xform.InverseTransformVector(magnitude * _hit.Knockback.normalized);
            xform.localPosition = offset;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.transform.localPosition = Vector3.zero;
        }
    }
}