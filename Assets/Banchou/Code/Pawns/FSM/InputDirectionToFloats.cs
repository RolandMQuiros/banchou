using System.Collections;
using System.Collections.Generic;
using Banchou.Player;
using UniRx;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    public class InputDirectionToFloats : FSMBehaviour {
        [SerializeField] private string _magnitudeOut;
        [SerializeField] private string _forwardOut;
        [SerializeField] private string _rightOut;

        private PlayerInputState _input;
        private PawnSpatial _spatial;

        private int _magnitudeHash;
        private int _forwardHash;
        private int _rightHash;

        public void Construct(GameState state, GetPawnId getPawnId) {
            state.ObservePawnInput(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(player => _input = player)
                .AddTo(this);
            state.ObservePawnSpatialChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(spatial => _spatial = spatial)
                .AddTo(this);
            
            _magnitudeHash = Animator.StringToHash(_magnitudeOut);
            _forwardHash = Animator.StringToHash(_forwardOut);
            _rightHash = Animator.StringToHash(_rightOut);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if (_input == null) return;
            
            if (_magnitudeHash != 0) {
                animator.SetFloat(_magnitudeHash, _input.Direction.magnitude);
            }

            if (_forwardHash != 0) {
                animator.SetFloat(_forwardHash, Vector3.Dot(_input.Direction, _spatial.Forward));
            }

            if (_rightHash != 0) {
                animator.SetFloat(_rightHash, Vector3.Dot(_input.Direction, _spatial.Right));
            }
        }
    }   
}
