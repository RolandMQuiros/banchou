using UnityEngine;
using Banchou.Board;
using Banchou.Board.Part;
using Banchou.DependencyInjection;

namespace Banchou.Pawn.Part {
    public class PawnContext : MonoBehaviour, IContext {
        [SerializeField] private int _reservedPlayerId;
        [SerializeField] private PawnState _pawn;
        [SerializeField] private Collider _worldCollider;

        private GameState _state;
        private int _pawnId;
        private Animator _animator;
        private CharacterController _controller;
        private Rigidbody _rigidbody;
        private RegisterPawnObject _registerPawnObject;

        public void Construct(
            GameState state,
            RegisterPawnObject registerPawnObject,
            GetPawnId getPawnId = null
        ) {
            _state = state;
            _animator = GetComponentInChildren<Animator>();
            _controller = GetComponentInChildren<CharacterController>();
            _rigidbody = GetComponentInChildren<Rigidbody>();
            
            if (getPawnId == null) {
                // If this pawn isn't in the state (i.e., baked into the scene), register it
                var xform = transform;
                _state.AddPawn(
                    out _pawn,
                    playerId: _reservedPlayerId,
                    position: xform.position,
                    forward: xform.forward
                );
                _registerPawnObject = registerPawnObject;
            } else {
                // If this pawn is in the state, grab it
                _pawn = _state.GetPawn(getPawnId());
            }

            // If pawn is still null, destroy this object
            if (_pawn == null) {
                Destroy(gameObject);
            } else {
                _pawnId = _pawn.PawnId;
            }
        }

        private void Start() {
            _registerPawnObject?.Invoke(_pawnId, gameObject);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(this)
                .Bind<GetPawnId>(() => _pawnId)
                .Bind(_animator)
                .Bind(_controller)
                .Bind(_rigidbody)
                .Bind(_worldCollider)
                .Bind<GetDeltaTime>(() => _state.Board.TimeScale * _pawn.TimeScale * _state.GetDeltaTime());
        }
    }
}