using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou.Pawn.Part {
    public class PawnContext : MonoBehaviour, IContext {
        private Animator _animator;
        private CharacterController _controller;
        private Rigidbody _rigidbody;
        private PawnActions _pawnActions;
        private PawnState _pawn;

        public void Construct(
            GetPawnId getPawnId,
            GetTime getTime,
            GetState getState
        ) {
            _animator = GetComponentInChildren<Animator>();
            _controller = GetComponentInChildren<CharacterController>();
            _rigidbody = GetComponentInChildren<Rigidbody>();
            _pawnActions = new PawnActions(getPawnId, getTime);
            _pawn = getState().GetPawn(getPawnId());
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(transform)
                .Bind(_pawn)
                .Bind(_animator)
                .Bind(_controller)
                .Bind(_rigidbody)
                .Bind(_pawnActions);
        }
    }
}