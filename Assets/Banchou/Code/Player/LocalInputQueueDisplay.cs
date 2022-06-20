using System.Collections.Generic;
using System.Linq;
using System.Text;
using Banchou.Board;
using Banchou.Player;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using TMPro;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.UI {
    public class LocalInputQueueDisplay : MonoBehaviour {
        private TextMeshProUGUI[] _labels;

        private readonly Dictionary<InputCommand, string> _commandLabelLookup = new() {
            [InputCommand.Neutral] = "5",
            [InputCommand.Forward] = "6",
            [InputCommand.ForwardRight] = "3",
            [InputCommand.Right] = "2",
            [InputCommand.BackRight] = "1",
            [InputCommand.Back] = "4",
            [InputCommand.BackLeft] = "7",
            [InputCommand.Left] = "8",
            [InputCommand.ForwardLeft] = "9",
            [InputCommand.Jump] = "J",
            [InputCommand.ShortJump] = "j",
            [InputCommand.Block] = "G",
            [InputCommand.LockOn] = "#",
            [InputCommand.LockOff] = ".",
            [InputCommand.LightAttack] = "L",
            [InputCommand.LightAttackHold] = "<b>L</b>",
            [InputCommand.LightAttackUp] = "<i>L</i>",
            [InputCommand.HeavyAttack] = "H",
            [InputCommand.HeavyAttackHold] = "<b>H</b>",
            [InputCommand.HeavyAttackUp] = "<i>H</i>",
            [InputCommand.SpecialAttack] = "S",
            [InputCommand.SpecialAttackHold] = "<b>S</b>",
            [InputCommand.SpecialAttackUp] = "<i>S</i>"
        };

        private void Awake() {
            _labels = GetComponentsInChildren<TextMeshProUGUI>();
        }

        public void Construct(GameState state) {
            var stringBuilder = new StringBuilder();
            
            state.ObserveAddedPawns()
                .Where(_ => isActiveAndEnabled)
                // Find first pawn with a local player
                .First(pawn => pawn.PlayerId != default && state.IsLocalPlayer(pawn.PlayerId))
                // Get input commands for that local player...
                .SelectMany(pawn => state.ObservePawnInputCommands(pawn.PawnId))
                // ...but only if we have labels to fill
                .Where(_ => _labels.Length > 0)
                .DistinctUntilChanged(step => step.Command | InputCommandMasks.Stick)
                .CatchIgnoreLog()
                .Subscribe(step => {
                    // Move bottom label to top
                    var topLabel = _labels.Single(label => label.transform.GetSiblingIndex() == _labels.Length - 1);
                    topLabel.transform.SetSiblingIndex(0);
                    
                    // Set the top label's text to the latest input
                    stringBuilder.Clear();
                    foreach (var commandLabel in _commandLabelLookup) {
                        if (step.Command.HasFlag(commandLabel.Key)) {
                            stringBuilder.Append(commandLabel.Value);
                        }
                    }

                    topLabel.text = stringBuilder.ToString();
                })
                .AddTo(this);
        }
    }
}