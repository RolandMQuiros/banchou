using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou.Pawn.Part {
    public class PawnContext : MonoBehaviour, IContext {
        private Animator _animator;
        private CharacterController _controller;
        private Rigidbody _rigidbody;
        private PawnActions _pawnActions;

        public void Construct(GetPawnId getPawnId, GetTime getTime) {
            _animator = GetComponentInChildren<Animator>();
            _controller = GetComponentInChildren<CharacterController>();
            _rigidbody = GetComponentInChildren<Rigidbody>();
            _pawnActions = new PawnActions(getPawnId, getTime);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_animator)
                .Bind(_controller)
                .Bind(_rigidbody)
                .Bind(_pawnActions);
        }
    }
}