using System.Linq;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Banchou.Player.Behavior {
	public class ChoosePawn : Action {
		[SerializeField] private SharedInt _outputPawnId;

		private GameState _state;
		private int _playerId;
		private int _chosenPawnId;

		public void Construct(GameState state, GetPlayerId getPlayerId) {
			_state = state;
			_playerId = getPlayerId();
		}

		public override void OnStart() {
			_chosenPawnId = _state.GetPlayerPawns(_playerId)
				.Select(pawn => pawn.PawnId)
				.FirstOrDefault();
		}

		public override TaskStatus OnUpdate() {
			if (_chosenPawnId != default) {
				_outputPawnId.SetValue(_chosenPawnId);
				return TaskStatus.Success;
			}
			return TaskStatus.Failure;
		}
	}
}