using System.Linq;
using System.Collections.Generic;
using Banchou.DependencyInjection;
using Banchou.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Banchou.Pawn.Part {
    public delegate Collider GetPawnCollider();
    
    public class PawnAnimator : MonoBehaviour, IContext {
        [SerializeField] private Collider _pawnCollider;

        private int _pawnId;
        private DiContainer _diContainer;
        private GameState _state;
        private PawnAnimatorFrame _frame;
        private Animator _animator;
        private List<AnimatorControllerParameter> _cachedParameters;
        private float _lastFrameTime;

        public void Construct(
            DiContainer diContainer,
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            _diContainer = diContainer;
            _state = state;
            _pawnId = getPawnId();
            _animator = animator;
            // Accessing Animator.parameters or Animator.GetParameter seems to generate garbage
            // so let's get this out of the way early
            _cachedParameters = _animator.parameters.ToList();
            
            // Query latest animator frame
            _state.ObservePawnChanges(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(pawn => {
                    _frame = pawn.AnimatorFrame;
                    animator.speed = _state.Board.TimeScale * pawn.TimeScale;
                })
                .AddTo(this);

            // Inject references needed by StateMachineBehaviours
            InstallBindings(_diContainer);
            
            // Handle frame syncs
            _state.ObservePawn(_pawnId)
                .SelectMany(pawn => pawn.AnimatorFrame.Observe())
                .DistinctUntilChanged(frame => (frame.IsSync, frame.When))
                .Where(frame => frame.IsSync)
                .CatchIgnoreLog()
                .Subscribe(ApplyFrame)
                .AddTo(this);

            // Need to re-inject when enabled, because Animators delete their StateMachineBehaviours on disable
            this.OnEnableAsObservable()
                .Subscribe(_ => { Inject(); })
                .AddTo(this);
            Inject();
        }

        /// <summary>
        /// Need to explicitly inject into animator on enable, since StateMachineBehaviours are completely
        /// re-instantiated
        /// </summary>
        private void Inject() {
            foreach (var behaviour in _animator.GetBehaviours<StateMachineBehaviour>()) {
                _diContainer.Inject(behaviour);
            }
        }

        private void OnAnimatorMove() {
            // Needed to enable script control of the animator
        }

        /// <summary>
        /// Apply a <see cref="PawnAnimatorFrame"/>'s data to the Animator
        /// </summary>
        private void ApplyFrame(PawnAnimatorFrame frame) {
            _cachedParameters = _animator.parameters.ToList();

            foreach (var param in frame.Bools) {
                _animator.SetBool(param.Key, param.Value);
            }

            foreach (var param in frame.Floats) {
                _animator.SetFloat(param.Key, param.Value);
            }

            foreach (var param in frame.Ints) {
                _animator.SetInteger(param.Key, param.Value);
            }

            for (int layer = 0; layer < frame.StateHashes.Length; layer++) {
                var stateHash = frame.StateHashes[layer];
                var normalizedTime = frame.NormalizedTimes[layer];
                _animator.Play(stateHash, layer, normalizedTime);
            }
        }
        
        /// <summary>
        /// Save the current state the animator to the game state
        /// </summary>
        private void LateUpdate() {
            _frame.StartFrame(_animator.layerCount);

            // Save layer values
            for (int layer = 0; layer < _animator.layerCount; layer++) {
                var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(layer);
                var nextStateInfo = _animator.GetNextAnimatorStateInfo(layer);
                var targetStateInfo = nextStateInfo.fullPathHash == 0 ? currentStateInfo : nextStateInfo;

                _frame.SetLayerData(layer, targetStateInfo.fullPathHash, targetStateInfo.normalizedTime);
            }

            // Save parameter values
            for (int p = 0; p < _cachedParameters.Count; p++) {
                var parameter = _cachedParameters[p];
                switch (parameter.type) {
                    case AnimatorControllerParameterType.Float:
                        _frame.SetFloat(parameter.nameHash, _animator.GetFloat(parameter.nameHash));
                        break;
                    case AnimatorControllerParameterType.Int:
                        _frame.SetInt(parameter.nameHash, _animator.GetInteger(parameter.nameHash));
                        break;
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Trigger:
                        _frame.SetBool(parameter.nameHash, _animator.GetBool(parameter.nameHash));
                        break;
                }
            }

            _lastFrameTime = _state.GetTime();
            _frame.FinishFrame(_lastFrameTime);
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(GetComponent<AnimatorUnityEvents>())
                .Bind<GetPawnCollider>(() => _pawnCollider);
        }
    }
}