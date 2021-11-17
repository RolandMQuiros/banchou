using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class SteppedStateTime : FSMBehaviour {
        [SerializeField] private int frameSample = 4;
        [SerializeField] private string _outputFloat;
        
        public void Construct(Animator animator) {
            var hash = Animator.StringToHash(_outputFloat);
            if (hash != 0) {
                ObserveStateUpdate
                    .SampleFrame(frameSample)
                    .Subscribe(args => {
                        animator.SetFloat(hash, args.StateInfo.normalizedTime);
                    })
                    .AddTo(this);
            }
        }
    }
}