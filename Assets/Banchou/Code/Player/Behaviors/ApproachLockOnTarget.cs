using Banchou.Combatant;
using Banchou.Pawn;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace Banchou.Player.Behavior {
    public class ApproachLockOnTarget : Action {
        [SerializeField, Tooltip("The approaching and locked-on Pawn")]
        private SharedInt _pawnId;
        
        [SerializeField, Tooltip("How close to approach before the task succeeds")]
        private SharedFloat _minimumDistance;

        private GameState _state;
        private int _playerId;

        private PlayerInputState _input;
        private PawnSpatial _spatial;
        private PawnSpatial _lockOnSpatial;

        public void Construct(GameState state, GetPlayerId getPlayerId) {
            _state = state;
            _playerId = getPlayerId();
        }

        public override void OnStart() {
            if (_pawnId == null) return;
            _input = _state.GetPlayerInput(_playerId);
            _spatial = _state.GetPawnSpatial(_pawnId.Value);
            _lockOnSpatial = _state.GetLockOnSpatial(_pawnId.Value);
        }

        public override TaskStatus OnUpdate() {
            if (_spatial == null || _lockOnSpatial == null || _input == null) {
                return TaskStatus.Failure;
            }

            var distance = _spatial.DistanceTo(_lockOnSpatial.Position);
            if (distance <= _minimumDistance?.Value) {
                _input.PushMove(Vector3.zero, _state.GetTime());
                return TaskStatus.Success;
            }
            _input.PushMove(_spatial.DirectionTo(_lockOnSpatial.Position), _state.GetTime());
            return TaskStatus.Running;
        }
    }
}