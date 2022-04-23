using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class PawnFSMBehaviour : FSMBehaviour {
        protected float StateStartTime { get; private set; }
        protected float StateTime { get; private set; }
        
        protected GameState State { get; private set; }
        protected int PawnId { get; private set; }
        protected float TimeScale { get; private set; }
        protected float DeltaTime { get; private set; }
        
        public void ConstructCommon(GameState state, GetPawnId getPawnId) {
            State = state;
            PawnId = getPawnId();

            State.ObservePawnTimeScale(PawnId)
                .CatchIgnoreLog()
                .Subscribe(timeScale => TimeScale = timeScale)
                .AddTo(this);
            
            State.ObservePawnDeltaTime(PawnId)
                .CatchIgnoreLog()
                .Subscribe(deltaTime => DeltaTime = deltaTime)
                .AddTo(this);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            StateStartTime = State.GetTime();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            StateTime = (State.GetTime() - StateStartTime) * TimeScale;
        }
    }
}