using Cinemachine;
using UnityEngine;

namespace Banchou.Utility {
    [RequireComponent(typeof(CinemachineDollyCart))]
    public class MoveCartAlongTrack : MonoBehaviour {
        [SerializeField] private Transform _target;
        private CinemachineDollyCart _dollyCart;

        [SerializeField, Min(0f)] private float _maxSpeed;
        [SerializeField, Min(0f)] private float _pathCalculationInterval = 0f;
        
        /// <summary>This enum defines the options available for the update method.</summary>
        private enum UpdateMethod
        {
            /// <summary>Updated in normal MonoBehaviour Update.</summary>
            Update,
            /// <summary>Updated in sync with the Physics module, in FixedUpdate</summary>
            FixedUpdate,
            /// <summary>Updated in normal MonoBehaviour LateUpdate</summary>
            LateUpdate
        };

        /// <summary>When to move the cart, if Velocity is non-zero</summary>
        [SerializeField, Tooltip("When to move the cart")]
        private UpdateMethod _updateMethod = UpdateMethod.Update;

        private float _targetPathPosition;
        private float _pathCalculationTimer = 0f;

        private void Awake() {
            _dollyCart = GetComponent<CinemachineDollyCart>();
            _dollyCart.m_PositionUnits = CinemachinePathBase.PositionUnits.PathUnits;
        }

        private void Apply() {
            if (_dollyCart.m_Path != null) {
                if (_pathCalculationTimer >= _pathCalculationInterval) {
                    _targetPathPosition = _dollyCart.m_Path.FindClosestPoint(_target.position, 0, -1, 10);
                    _pathCalculationTimer = 0f;
                }
                
                _dollyCart.m_Position = Mathf.MoveTowards(_dollyCart.m_Position, _targetPathPosition, _maxSpeed);
            }
        }

        private void Update() {
            if (_updateMethod == UpdateMethod.Update) {
                _pathCalculationTimer += Time.deltaTime;
                Apply();
            }
        }

        private void FixedUpdate() {
            if (_updateMethod == UpdateMethod.FixedUpdate) {
                _pathCalculationTimer += Time.fixedDeltaTime;
                Apply();
            }
        }

        private void LateUpdate() {
            if (_updateMethod == UpdateMethod.LateUpdate) {
                _pathCalculationTimer += Time.deltaTime;
                Apply();
            }
        }
    }
}