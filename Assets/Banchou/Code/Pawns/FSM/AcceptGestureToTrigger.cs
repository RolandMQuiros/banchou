using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

using Banchou.Player;

namespace Banchou.Pawn.FSM {
    /*
        Maybe this should be moved/replicated on MonoBehaviour level, so we at least have them all in one
        place to generate command lists from. Then AIs will just need to force trigger names directly instead
        of input commands?

        but also AIs inputting commands kinda rules.
    */
    public class AcceptGestureToTrigger : FSMBehaviour {
        [SerializeField, Tooltip("Sequence of stick inputs preceding the command")]
        private PlayerStickState[] _sequence;

        [SerializeField, Tooltip("Commands input after the stick sequence to set the trigger")]
        private InputCommand _acceptedCommand;

        [SerializeField, Tooltip("The name of the output trigger parameter to set if the gesture was input correctly")]
        private string _outputParameter;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            var outputHash = Animator.StringToHash(_outputParameter);
            var sequenceBuffered = state
                .ObservePawnInput(getPawnId())
                .Select(input => input.Direction)
                // Project stick direction to 2D, with Pawn's forward direction mapped to up, right to right
                .WithLatestFrom(
                    state.ObservePawnSpatial(getPawnId()),
                    (direction, spatial) => new Vector2(
                        Vector3.Dot(direction, spatial.Right),
                        Vector3.Dot(direction, spatial.Forward)
                    ).DirectionToStick()
                )
                .DistinctUntilChanged()
                .Buffer(_sequence.Length, 1)
                .Select(buffer => buffer.SequenceEqual(_sequence));

            state.ObservePawnInput(getPawnId())
                .Where(_ => IsStateActive)
                .Select(input => input.Commands)
                .Where(command => (command & _acceptedCommand) != InputCommand.None)
                .WithLatestFrom(sequenceBuffered, (_, buffered) => buffered)
                .Where(buffered => buffered)
                .Subscribe(_ => {
                    animator.SetTrigger(outputHash);
                })
                .AddTo(this);
        }
    }
}