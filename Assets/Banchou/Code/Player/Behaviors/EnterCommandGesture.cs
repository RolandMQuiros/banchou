using Banchou.Combatant;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UniRx;
using UnityEngine;

namespace Banchou.Player.Behavior {
	public class EnterCommandGesture : Action {
		[SerializeField] private SharedInt _pawnId;
		[SerializeField] private InputGestureStep[] _gesture;

		private GameState _state;
		private AttackState _attack;
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
			_state.ObserveLastAttack(_pawnId.Value)
				.CatchIgnoreLog()
				.Subscribe(attack => _attack = attack)
				.AddTo(this);
		}

		public override TaskStatus OnUpdate() {
			if (_gesture == null || _gesture.Length == 0 || _attack == null) {
				return TaskStatus.Failure;
			}

			var currentStep = _gesture[_gestureIndex];
			var now = _state.GetTime();
			
			// Release previous command
			if (_input.Commands != InputCommand.None) {
				_input.PushCommands(InputCommand.None, _state.GetTime());
			}

			// If the current step requires a hit confirm, wait until one happens 
			if (_gestureIndex > 0 && currentStep.RequireConfirm && !_attack.Confirmed) {
				if (now - _lastStepTime <= currentStep.Delay) {
					return TaskStatus.Running;
				}
				// If timed out, failure
				return TaskStatus.Failure;
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