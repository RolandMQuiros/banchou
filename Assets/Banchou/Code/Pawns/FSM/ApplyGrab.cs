using Banchou.Combatant;
using UnityEngine;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class ApplyGrab : FSMBehaviour {
        private GameState _state;
        private int _pawnId;
        [SerializeField] private ApplyFSMParameter[] _onGrabStart;
        [SerializeField] private ApplyFSMParameter[] _onGrabRelease;

        public void Construct(GameState state, GetPawnId getPawnId, Animator animator) {
            _state = state;
            _pawnId = getPawnId();
            _state.ObserveGrabContactsOn(_pawnId)
                .Where(_ => IsStateActive)
                .CatchIgnoreLog()
                .Subscribe(_ => { _onGrabStart.ApplyAll(animator); })
                .AddTo(this);
            _state.ObserveGrabReleasesOn(_pawnId)
                .Where(_ => IsStateActive)
                .CatchIgnoreLog()
                .Subscribe(_ => { _onGrabRelease.ApplyAll(animator); })
                .AddTo(this);
        }
    }
}