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

        [SerializeField, Range(0f, 1f), Tooltip("When, in regular state time, the accepted command is output to a boolean")]
        private float _bufferUntilStateTime = 0f;

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
            var wasTriggered = false;

            state
                .ObservePawnInput(getPawnId())
                .Select(input => input.Commands)
                .Where(command => (command & _acceptedCommands) != InputCommand.None && IsStateActive && !wasTriggered)
                .WithLatestFrom(
                    ObserveStateUpdate,
                    (command, unit) => unit.StateInfo.normalizedTime % 1
                )
                .Where(stateTime => stateTime >= _acceptFromStateTime && stateTime <= _acceptUntilStateTime)
                .Subscribe(stateTime => {
                    wasTriggered = true;
                })
                .AddTo(this);

            if (outputHash != 0) {
                ObserveStateUpdate
                    .Where(unit => wasTriggered && unit.StateInfo.normalizedTime >= _bufferUntilStateTime)
                    .Subscribe(unit => {
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

            ObserveStateExit
                .Subscribe(_ => {
                    wasTriggered = false;
                })
                .AddTo(this);
        }
    }
}