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
        
        private DiContainer _diContainer;
        private GameState _state;
        private PawnAnimatorFrame _frame;
        private Animator _animator;
        private List<AnimatorControllerParameter> _cachedParameters;

        public void Construct(
            DiContainer diContainer,
            GameState state,
            GetPawnId getPawnId,
            Animator animator
        ) {
            _diContainer = diContainer;
            _state = state;
            _animator = animator;
            _state.ObservePawnChanges(getPawnId())
                .CatchIgnoreLog()
                .Subscribe(pawn => _frame = pawn.AnimatorFrame)
                .AddTo(this);

            // Accessing Animator.parameters or Animator.GetParameter seems to generate garbage
            // so let's get this out of the way early
            _cachedParameters = animator.parameters.ToList();

            InstallBindings(_diContainer);
            
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
                        _frame.SetBool(parameter.nameHash, _animator.GetBool(parameter.nameHash));
                        break;
                }
            }
            _frame.FinishFrame(_state.GetTime());
        }

        public DiContainer InstallBindings(DiContainer container) {
            return container
                .Bind(GetComponent<AnimatorUnityEvents>())
                .Bind<GetPawnCollider>(() => _pawnCollider);
        }
    }
}