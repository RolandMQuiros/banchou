using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

namespace Banchou.Player.Behavior {
	public class EnterCommandGesture : Action {
		[SerializeField] private InputGestureStep[] _gesture;

		private GameState _state;
		private PlayerInputState _input;
		private int _gestureIndex;
		private float _lastStepTime;

		public void Construct(GameState state, GetPlayerId getPlayerId) {
			_state = state;
			_input = _state.GetPlayerInput(getPlayerId());
		}

		public override void OnStart() {
			_lastStepTime = _state.GetTime();
			_gestureIndex = 0;
			_input.PushCommands(InputCommand.None, _state.GetTime());
		}

		public override TaskStatus OnUpdate() {
			if (_gesture == null || _gesture.Length == 0) {
				return TaskStatus.Failure;
			}

			var currentStep = _gesture[_gestureIndex];
			var now = _state.GetTime();

			if (_input.Commands != InputCommand.None) {
				_input.PushCommands(InputCommand.None, _state.GetTime());
			}

			if (now - _lastStepTime > currentStep.Delay) {
				_gestureIndex++;
				_lastStepTime = now;
				_input.PushCommands(currentStep.Command, _state.GetTime());
			}

			if (_gestureIndex >= _gesture.Length) {
				return TaskStatus.Success;
			}
			return TaskStatus.Running;
		}

		public override void OnEnd() {
			_input.PushCommands(InputCommand.None, _state.GetTime());
		}
	}
}