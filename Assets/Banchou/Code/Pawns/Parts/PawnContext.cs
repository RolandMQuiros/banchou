using UnityEngine;
using Banchou.DependencyInjection;

namespace Banchou.Pawn.Part {
    public class PawnContext : MonoBehaviour, IContext {
        [SerializeField] private PawnState _pawn = null;
        private Animator _animator;
        private CharacterController _controller;
        private Rigidbody _rigidbody;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _pawn = state.GetPawn(getPawnId());
            _animator = GetComponentInChildren<Animator>();
            _controller = GetComponentInChildren<CharacterController>();
            _rigidbody = GetComponentInChildren<Rigidbody>();
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(_pawn)
                .Bind(transform)
                .Bind(_animator)
                .Bind(_controller)
                .Bind(_rigidbody);
        }
    }
}