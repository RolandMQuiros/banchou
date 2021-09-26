using System.Linq;
using UnityEngine;

using Banchou.Board;
using Banchou.DependencyInjection;
using UniRx;

namespace Banchou.Pawn.Part {
    public class PawnContext : MonoBehaviour, IContext {
        [SerializeField] private PawnState _pawn;
        private int _pawnId;
        private Animator _animator;
        private CharacterController _controller;
        private Rigidbody _rigidbody;

        public void Construct(
            GameState state,
            GetPawnId getPawnId = null
        ) {
            if (getPawnId == null) {
                // If this pawn isn't in the state (i.e., baked into the scene), register it
                var xform = transform;
                state.AddPawn(out _pawn, position: xform.position, forward: xform.forward);
            } else {
                // If this pawn is in the state, grab it
                _pawn = state.GetPawn(getPawnId());
            }

            // If pawn is still null, destroy this object
            if (_pawn == null) {
                Debug.LogError($"Could not resolve a PawnState for this PawnContext. Destroying.");
                Destroy(gameObject);
            } else {
                _pawnId = _pawn.PawnId;
                _animator = GetComponentInChildren<Animator>();
                _controller = GetComponentInChildren<CharacterController>();
                _rigidbody = GetComponentInChildren<Rigidbody>();
            }
        }

        private int GetPawnId() => _pawnId;

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind((GetPawnId)GetPawnId)
                .Bind(_animator)
                .Bind(_controller)
                .Bind(_rigidbody);
        }
    }
}