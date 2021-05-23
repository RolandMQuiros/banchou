using UnityEngine;
using UnityEngine.InputSystem;

namespace Banchou.Player.Part {
    public class PlayerInputDispatcher : MonoBehaviour {
        [SerializeField, Range(0f, 1f)] private float _lockTapTime = 0.2f;

        private PlayerInput _source;
        private PlayerInputState _input;
        private GetTime _getTime;
        private Transform _camera;
        private long _sequence = 0L;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private InputCommand _commandsInput;
        private float _timeAtLockPress = 0f;

        public void Construct(
            PlayerState player,
            GetTime getTime
        ) {
            _input = player.Input;
            _getTime = getTime;
            _camera = Camera.main.transform;

            _source = GetComponent<PlayerInput>();
            _source.onActionTriggered += HandleAction;
        }

        private void OnDestroy() {
            _source.onActionTriggered -= HandleAction;
        }

        private void HandleAction(InputAction.CallbackContext callbackContext) {
            switch (callbackContext.action.name) {
                case "Move": {
                    var direction = callbackContext.ReadValue<Vector2>();
                    _moveInput = direction;
                } break;
                case "Look": {
                    var direction = callbackContext.ReadValue<Vector2>();
                    _lookInput = direction;
                } break;
                case "Light Attack": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.LightAttack;
                    }
                } break;
                case "Lock On": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.LockOn;
                        _timeAtLockPress = _getTime();
                    } else if (callbackContext.canceled && _getTime() > _timeAtLockPress + _lockTapTime) {
                        _commandsInput |= InputCommand.LockOff;
                    }
                } break;
            }
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
                _commandsInput |= InputCommand.LightAttack;
            }
        }

        private void LateUpdate() {
            var move = _moveInput.CameraPlaneProject(_camera);
            var look = Snapping.Snap(_lookInput, Vector3.one * 0.25f);

            if (move != _input.Direction || look != _input.Look || _commandsInput != InputCommand.None) {
                _input.Push(_commandsInput, move, look, ++_sequence, _getTime());
            }

            _commandsInput = InputCommand.None;
        }
    }
}