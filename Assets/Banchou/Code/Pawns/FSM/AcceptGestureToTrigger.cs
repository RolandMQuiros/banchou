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

        [SerializeField, Tooltip("Sequence of inputs needed to fire the trigger")]
        private InputCommand[] _inputSequence;

        [SerializeField, Tooltip("Lifetime of stick inputs in the buffer, in seconds")]
        private float _inputLifetime = 0.1666667f; // Approximately 10 frames

        [SerializeField, Tooltip("The name of the output trigger parameter to set if the gesture was input correctly")]
        private string _outputParameter;

        public void Construct(
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            if (_inputSequence.Length == 0) return;

            var outputHash = Animator.StringToHash(_outputParameter);
            var commandMask = _inputSequence.Aggregate((prev, next) => prev | next);

            state.ObservePawnInputCommands(getPawnId())
                .Where(unit => (unit.Command & commandMask) != InputCommand.None)
                .Pairwise()
                .Scan(0, (sequenceIndex, unitPair) => {
                    if (sequenceIndex >= _inputSequence.Length) {
                        sequenceIndex = 0;
                    }

                    var sequenceStarted = sequenceIndex > 0;
                    var previousCommandTooOld = unitPair.Current.When - unitPair.Previous.When >= _inputLifetime;
                    if (sequenceStarted && previousCommandTooOld) {
                        return 0;
                    } else if ((unitPair.Current.Command & _inputSequence[sequenceIndex]) != InputCommand.None) {
                        return sequenceIndex + 1;
                    }

                    return sequenceIndex;
                })
                .Where(sequenceIndex => sequenceIndex >= _inputSequence.Length)
                .Subscribe(_ => {
                    animator.SetTrigger(outputHash);
                })
                .AddTo(this);
        }
    }
}