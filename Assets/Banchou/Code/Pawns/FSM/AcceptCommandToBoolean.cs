using UnityEngine;
using UniRx;
using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class AcceptCommandToBoolean : FSMBehaviour {
        [SerializeField] private InputCommand _acceptedCommands = InputCommand.None;

        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is accepted")]
        private float _acceptFromStateTime = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is no longer accepted")]
        private float _acceptUntilStateTime = 1f;

        [SerializeField, Tooltip("Which animator parameter boolean to set when the command is accepted")]
        private string _outputParameter = null;

        private enum AcceptSetMode {
            Set, Unset, Toggle
        }

        [SerializeField, Tooltip("Whether to set, unset, or toggle the boolean when a command is accepted")]
        private AcceptSetMode _acceptMode;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            var outputHash = Animator.StringToHash(_outputParameter);

            if (outputHash != 0) {
                state.ObservePawnInput(getPawnId())
                    .Select(input => input.Commands)
                    .Where(command => IsStateActive && (command & _acceptedCommands) != InputCommand.None)
                    .WithLatestFrom(
                        ObserveStateUpdate,
                        (_, unit) => unit.StateInfo.normalizedTime % 1
                    )
                    .Where(stateTime => stateTime >= _acceptFromStateTime && stateTime <= _acceptUntilStateTime)
                    .Subscribe(_ => {
                        bool output = false;
                        switch (_acceptMode) {
                            case AcceptSetMode.Set:
                                output = true;
                                break;
                            case AcceptSetMode.Unset:
                                output = false;
                                break;
                            case AcceptSetMode.Toggle:
                                output = !animator.GetBool(outputHash);
                                break;
                        }
                        animator.SetBool(outputHash, output);
                    })
                    .AddTo(this);
            }
        }
    }
}