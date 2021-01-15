using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject]
    public class AnimatorFrame : Substate<AnimatorFrame> {
        public static readonly AnimatorFrame Empty = default;
        [Key(0)] public int[] StateHashes { get; private set; }
        [Key(1)] public float[] NormalizedTimes { get; private set; }
        [Key(2)] public Dictionary<int, float> Floats { get; private set; } = new Dictionary<int, float>();
        [Key(3)] public Dictionary<int, int> Ints { get; private set; } = new Dictionary<int, int>();
        [Key(4)] public Dictionary<int, bool> Bools { get; private set; } = new Dictionary<int, bool>();
        [Key(5)] public float When;

        // public AnimatorFrame(Animator animator, float when) {
        //     StateHashes = new int[animator.layerCount];
        //     NormalizedTimes = new float[animator.layerCount];
        // }

        public void Animated(Animator animator, float when) {
            When = when;

            for (int layer = 0; layer < animator.layerCount; layer++) {
                var currentStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                var nextStateInfo = animator.GetNextAnimatorStateInfo(layer);
                var targetStateInfo = nextStateInfo.fullPathHash == 0 ? currentStateInfo : nextStateInfo;

                StateHashes[layer] = targetStateInfo.fullPathHash;
                NormalizedTimes[layer] = targetStateInfo.normalizedTime;
            }

            foreach (var p in animator.parameters) {
                switch (p.type) {
                    case AnimatorControllerParameterType.Float:
                        Floats[p.nameHash] = animator.GetFloat(p.nameHash);
                        break;
                    case AnimatorControllerParameterType.Int:
                        Ints[p.nameHash] = animator.GetInteger(p.nameHash);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        Bools[p.nameHash] = animator.GetBool(p.nameHash);
                        break;
                }
            }

            Notify();
        }
    }
}