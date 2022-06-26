using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Banchou.Player {
    [Flags]
    public enum InputCommand {
        None = 0,
        Neutral = 1,
        Forward = 1 << 1,
        ForwardRight = 1 << 2,
        Right = 1 << 3,
        BackRight = 1 << 4,
        Back = 1 << 5,
        BackLeft = 1 << 6,
        Left = 1 << 7,
        ForwardLeft = 1 << 8,
        LightAttack = 1 << 9,
        LightAttackHold = 1 << 10,
        LightAttackUp = 1 << 11,
        HeavyAttack = 1 << 12,
        HeavyAttackHold = 1 << 13,
        HeavyAttackUp = 1 << 14,
        SpecialAttack = 1 << 15,
        SpecialAttackHold = 1 << 16,
        SpecialAttackUp = 1 << 17,
        LockOn = 1 << 18,
        LockOff = 1 << 19,
        ShortJump = 1 << 20,
        Jump = 1 << 21,
        Block = 1 << 22,
        Unblock = 1 << 23,
        Burst = 1 << 24
    }

    [Serializable]
    public class InputGestureStep {
        public float Delay;
        public bool RequireConfirm;
        public InputCommand Command;
    };

    [Serializable]
    public class InputGesture {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public InputGestureStep[] Sequence { get; private set; }
    };

    public static class InputCommandMasks {
        public const InputCommand Stick = 
            InputCommand.Neutral |
            InputCommand.Forward |
            InputCommand.ForwardRight |
            InputCommand.Right |
            InputCommand.BackRight |
            InputCommand.Back |
            InputCommand.BackLeft |
            InputCommand.Left |
            InputCommand.ForwardLeft;

        public const InputCommand Attacks =
            InputCommand.LightAttack |
            InputCommand.LightAttackHold |
            InputCommand.HeavyAttack |
            InputCommand.HeavyAttackHold;

        public const InputCommand Gestures = Stick | Attacks;

        public static string EmojiString(this InputCommand command) {
            var builder = new StringBuilder();
            foreach (var value in Enum.GetValues(typeof(InputCommand)).Cast<InputCommand>()) {
                if (command.HasFlag(value)) {
                    switch (value) {
                        case InputCommand.None:
                            break;
                        case InputCommand.Neutral:
                            builder.Append("•");
                            break;
                        case InputCommand.Forward:
                            builder.Append("⬆");
                            break;
                        case InputCommand.ForwardRight:
                            builder.Append("↗");
                            break;
                        case InputCommand.Right:
                            builder.Append("➡");
                            break;
                        case InputCommand.BackRight:
                            builder.Append("↘");
                            break;
                        case InputCommand.Back:
                            builder.Append("⬇");
                            break;
                        case InputCommand.BackLeft:
                            builder.Append("↙");
                            break;
                        case InputCommand.Left:
                            builder.Append("⬅");
                            break;
                        case InputCommand.ForwardLeft:
                            builder.Append("↖");
                            break;
                        case InputCommand.LightAttack:
                            builder.Append("L");
                            break;
                        case InputCommand.LightAttackHold:
                            builder.Append("<b>L</b>");
                            break;
                        case InputCommand.LightAttackUp:
                            builder.Append("<b><i>L</i><b>");
                            break;
                        case InputCommand.HeavyAttack:
                            builder.Append("H");
                            break;
                        case InputCommand.HeavyAttackHold:
                            builder.Append("<b>H</b>");
                            break;
                        case InputCommand.HeavyAttackUp:
                            builder.Append("<b><i>H</i><b>");
                            break;
                        case InputCommand.SpecialAttack:
                            builder.Append("S");
                            break;
                        case InputCommand.SpecialAttackHold:
                            builder.Append("<b>S</b>");
                            break;
                        case InputCommand.SpecialAttackUp:
                            builder.Append("<b><i>S</i><b>");
                            break;
                        case InputCommand.Jump:
                            builder.Append("J");
                            break;
                        case InputCommand.ShortJump:
                            builder.Append("j");
                            break;
                        case InputCommand.LockOn:
                            builder.Append("L");
                            break;
                        case InputCommand.LockOff:
                            builder.Append("l");
                            break;
                        case InputCommand.Block:
                            builder.Append("🛡");
                            break;
                        case InputCommand.Unblock:
                            builder.Append("❌🛡");
                            break;
                        default:
                            builder.Append(value.ToString());
                            break;
                    }
                }
            }
            return builder.ToString();
        }
    }
}