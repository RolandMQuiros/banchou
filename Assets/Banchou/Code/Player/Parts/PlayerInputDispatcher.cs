using UnityEngine;
using UnityEngine.InputSystem;

namespace Banchou.Player.Part {
    public class PlayerInputDispatcher : MonoBehaviour {
        [SerializeField] private Transform _camera;

        private int _playerId;
        private Dispatcher _dispatch;
        private PlayerActions _playerActions;
        private GetTime _getTime;

        private long _sequence = 0L;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private PlayerCommand _commandsInput;

        private InputUnit _lastUnit;

        public void Construct(
            GetPlayerId getPlayerId,
            Dispatcher dispatch,
            PlayerActions playerActions,
            GetTime getTime
        ) {
            _playerId = getPlayerId();
            _dispatch = dispatch;
            _playerActions = playerActions;
            _getTime = getTime;

            _camera = _camera == null ? Camera.main.transform : _camera;
        }

        public void DispatchMovement(InputAction.CallbackContext callbackContext) {
            var direction = callbackContext.ReadValue<Vector2>();
            _moveInput = direction;
        }

        public void DispatchLook(InputAction.CallbackContext callbackContext) {
            var direction = callbackContext.ReadValue<Vector2>();
            _lookInput = direction;
        }

        public void DispatchLightAttack(InputAction.CallbackContext callbackContext) {
            if (callbackContext.performed) {
                _commandsInput |= PlayerCommand.LightAttack;
            }
        }

        private void Update() {
            var move = _moveInput.CameraPlaneProject(_camera);
            var look = _lookInput;

            if (move != _lastUnit.Direction || look != _lastUnit.Look || _commandsInput != _lastUnit.Commands) {
                _lastUnit = new InputUnit(
                    playerId: _playerId,
                    when: _getTime(),
                    sequence: _sequence++,
                    commands: _commandsInput,
                    direction: move,
                    look: look
                );

                _dispatch(_playerActions.PushInput(_lastUnit));
                _commandsInput = PlayerCommand.None;
            }
        }
    }
}