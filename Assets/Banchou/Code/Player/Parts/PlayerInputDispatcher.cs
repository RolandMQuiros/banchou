using UnityEngine;
using UnityEngine.InputSystem;

namespace Banchou.Player.Part {
    public class PlayerInputDispatcher : MonoBehaviour {
        [SerializeField, Range(0f, 1f)] private float _lockTapTime = 0.2f;

        private PlayerInput _source;
        private PlayerInputState _input;
        private GameState _state;
        private Transform _camera;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private InputCommand _commandsInput;
        private float _timeAtLockPress = 0f;

        public void Construct(
            PlayerState player,
            GameState state
        ) {
            _input = player.Input;
            _state = state;
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
                case "Heavy Attack": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.HeavyAttack;
                    }
                } break;
                case "Jump": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.Jump;
                    }
                } break;
                case "Short Jump": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.ShortJump;
                    }
                } break;
                case "Lock On": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.LockOn;
                        _timeAtLockPress = _state.GetTime();
                    } else if (callbackContext.canceled && _state.GetTime() > _timeAtLockPress + _lockTapTime) {
                        _commandsInput |= InputCommand.LockOff;
                    }
                } break;
                case "Block": {
                    if (callbackContext.performed) {
                        _commandsInput |= InputCommand.Block;
                    } else if (callbackContext.canceled) {
                        _commandsInput |= InputCommand.Unblock;
                    }
                } break;
                case "DebugBreak": {
                    if (callbackContext.performed) {
                        Debug.Break();
                    }
                } break;
            }
        }

        private void LateUpdate() {
            var move = _moveInput.CameraPlaneProject(_camera);
            // var look = Snapping.Snap(_lookInput, Vector3.one * 0.25f);

            if (move != _input.Direction /*|| look != _input.Look*/ || _commandsInput != InputCommand.None) {
                _input.Push(_commandsInput, move, Vector3.zero, _state.GetTime());
            }

            _commandsInput = InputCommand.None;
        }
    }
}