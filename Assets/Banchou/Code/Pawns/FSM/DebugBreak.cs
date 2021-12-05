using System;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class DebugBreak : FSMBehaviour {
        [Serializable, Flags] private enum ApplyEvent { OnEnter = 1, OnExit = 2 }
        [SerializeField] private ApplyEvent _onEvent;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnEnter)) Debug.Break();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_onEvent.HasFlag(ApplyEvent.OnExit)) Debug.Break();
        }
    }
}