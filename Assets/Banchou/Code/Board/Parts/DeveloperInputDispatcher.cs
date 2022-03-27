using UnityEngine;
using UnityEngine.InputSystem;

namespace Banchou.Board.Part {
    [RequireComponent(typeof(PlayerInput))]
    public class DeveloperInputDispatcher : MonoBehaviour {
        private PlayerInput _source;
        private SaveStateLoader _saveStateLoader;
        private int _slot;
        private bool _saving;
        
        private void Awake() {
            if (TryGetComponent(out _source)) {
                _source.onActionTriggered += HandleAction;
            }
            TryGetComponent(out _saveStateLoader);
        }

        private void HandleAction(InputAction.CallbackContext callbackContext) {
            var runSaveStateOperation = false;
            switch (callbackContext.action.name) {
                case "State Slot 1":
                    _slot = 1;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 2":
                    _slot = 2;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 3":
                    _slot = 3;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 4":
                    _slot = 4;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 5":
                    _slot = 5;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 6":
                    _slot = 6;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 7":
                    _slot = 7;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 8":
                    _slot = 8;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 9":
                    _slot = 9;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 10":
                    _slot = 10;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 11":
                    _slot = 11;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "State Slot 12":
                    _slot = 12;
                    runSaveStateOperation = callbackContext.canceled;
                    break;
                case "Save":
                    _saving = callbackContext.performed;
                    break;
            }

            if (runSaveStateOperation) {
                if (_saving) {
                    _saveStateLoader.SaveState(_slot);
                } else {
                    _saveStateLoader.LoadState(_slot);
                }
            }
        }
    }
}