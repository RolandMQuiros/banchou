using UnityEngine;
using UnityEngine.InputSystem;

namespace Banchou.Player.Part {
    public class PlayerInputDispatcher : MonoBehaviour {
        private PlayerInput _source;

        private PlayerInputStates _input;
        private GetTime _getTime;
        private Transform _camera;

        private long _sequence = 0L;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private InputCommand _commandsInput;

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
                case "Move":
                    DispatchMovement(callbackContext);
                    break;
                case "Look":
                    DispatchLook(callbackContext);
                    break;
                case "Light Attack":
                    DispatchLightAttack(callbackContext);
                    break;
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

        private void Update() {
            var move = _moveInput.CameraPlaneProject(_camera);
            var look = Snapping.Snap(_lookInput, Vector3.one * 0.25f);

            if (move != _input.Direction) {
                _input.PushMove(move, ++_sequence, _getTime());
            }

            if (look != _input.Look) {
                _input.PushLook(look, ++_sequence, _getTime());
            }

            if (_commandsInput != InputCommand.None) {
                _input.PushCommands(_commandsInput, ++_sequence, _getTime());
            }
            _commandsInput = InputCommand.None;
        }
    }
}