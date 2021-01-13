using System;
using UnityEngine;
using UniRx;
using Banchou.Player;

namespace Banchou.Pawn.FSM {
    public class AcceptCommandToTrigger : FSMBehaviour {
        [SerializeField] private InputCommand _acceptedCommands = InputCommand.None;

        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is accepted")]
        private float _acceptFromStateTime = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("The normalized state time after which the command is no longer accepted")]
        private float _acceptUntilStateTime = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("When, in regular state time, the accepted command is output to a trigger")]
        private float _bufferUntilStateTime = 0f;

        [SerializeField, Tooltip("Which animator parameter trigger to set when the command is accepted")]
        private string _outputParameter = null;

        public void Construct(
            GetPawnId getPawnId,
            GetState getState,
            Animator animator
        ) {
            var commandHash = Animator.StringToHash(_outputParameter);
            var wasTriggered = false;

            getState()
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

            if (commandHash != 0) {
                ObserveStateUpdate
                    .Where(unit => wasTriggered && unit.StateInfo.normalizedTime >= _bufferUntilStateTime)
                    .Subscribe(unit => {
                        animator.SetTrigger(commandHash);
                    })
                    .AddTo(this);
            }

            ObserveStateExit
                .Subscribe(_ => {
                    if (wasTriggered) {
                        animator.ResetTrigger(commandHash);
                        wasTriggered = false;
                    }
                })
                .AddTo(this);
        }
    }
}