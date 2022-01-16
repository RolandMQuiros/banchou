using System.Collections.Generic;
using System.Linq;
using Banchou.Combatant;
using Banchou.Pawn;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Banchou.Player.Behavior {
	public class LockOnToClosestEnemy : Action {
		public SharedInt PawnId;
		private GameState _state;
		private PawnState _pawn;
		private IEnumerable<PawnSpatial> _enemySpatials; 

		public void Construct(GameState state) {
			_state = state;
		}

		public override void OnStart() {
			_enemySpatials = _state.GetHostileSpatials(PawnId.Value);
			_pawn = _state.GetPawn(PawnId.Value);
		}

		public override TaskStatus OnUpdate() {
			if (_pawn?.Spatial != null && _pawn?.Combatant != null && _enemySpatials?.Any() == true) {
				var targetId = _enemySpatials
					.OrderBy(enemy => (_pawn.Spatial.Position - enemy.Position).sqrMagnitude)
					.FirstOrDefault()?.PawnId ?? 0;
				if (targetId != default) {
					_pawn.Combatant.LockOn(targetId, _state.GetTime());
					return TaskStatus.Success;
				}
			}
			return TaskStatus.Failure;
		}
	}
}