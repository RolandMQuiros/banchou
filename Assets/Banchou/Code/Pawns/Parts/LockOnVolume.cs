using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

using Banchou.Combatant;
using Banchou.Player;

namespace Banchou.Pawn.Part {
    [RequireComponent(typeof(Collider))]
    public class LockOnVolume : MonoBehaviour {
        [SerializeField] private LayerMask _obstructionMask;
        [SerializeField] private Vector3 _linecastOrigin;

        public Vector3 Origin => transform.TransformPoint(_linecastOrigin);

        private GameState _state;
        private int _pawnId;
        private readonly HashSet<LockOnTarget> _targets = new(32);
        private CombatantState _combatant;
        private LockOnTarget _target;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _state = state;
            _pawnId = getPawnId();
            
            state.ObserveCombatant(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(combatant => _combatant = combatant)
                .AddTo(this);
            
            state.ObservePawnInput(_pawnId)
                .CatchIgnoreLog()
                .Subscribe(input => {
                    if (input.Commands.HasFlag(InputCommand.LockOn)) {
                        if (_combatant.LockOnTarget == default) {
                            // If combatant has no lock on target, choose one
                            var forward = input.Direction != Vector3.zero ? input.Direction.normalized :
                                transform.forward;
                            var origin = Origin;
                            
                            var selected = _targets
                                .Where(target => !Physics.Linecast(Origin, target.Origin, _obstructionMask.value))
                                .Select(
                                    target => (
                                        Target: target,
                                        Distance: (target.Origin - origin).magnitude, 
                                        Dot: Vector3.Dot((target.Origin - Origin).normalized, forward) 
                                    )
                                )
                                .OrderByDescending(targetArgs => Math.Sign(targetArgs.Dot))
                                .ThenBy(targetArgs => (2f - Mathf.Abs(targetArgs.Dot)) * targetArgs.Distance)
                                .Select(targetArgs => targetArgs.Target)
                                .FirstOrDefault(target => target.PawnId != _combatant.LockOnTarget);
                            if (selected != default) {
                                _target = selected;
                                _combatant.LockOn(selected.PawnId, state.GetTime());
                            }
                        } else {
                            // If combatant has a lock on target, unlock
                            _target = null;
                            _combatant.LockOff(state.GetTime());
                        }
                    }
                    
                    if (input.Commands.HasFlag(InputCommand.LockOff)) {
                        _target = null;
                        _combatant.LockOff(state.GetTime());
                    }
                })
                .AddTo(this);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<LockOnTarget>(out var target) && target.PawnId != _pawnId) {
                _targets.Add(target);
            } 
        }

        private void OnTriggerExit(Collider other) {
            if (other.TryGetComponent<LockOnTarget>(out var target)) {
                _targets.Remove(target);
            }
        }

        private void OnDrawGizmos() {
            var origin = Origin;
            var xform = transform;
            var forward = xform.forward;
            var up = xform.up;
            
            foreach (var target in _targets) {
                var dot = Vector3.Dot((target.Origin - origin).normalized, forward);
                var targetOrigin = target.Origin;
                var distance = (target.Origin - origin).magnitude;

                if (target != _target && dot > 0f) {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(origin, targetOrigin);

                    var sortFactor = origin + (2f - dot) * distance * up;
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(targetOrigin, sortFactor);
                    Gizmos.DrawSphere(sortFactor, 0.05f);
                }
            }

            if (_target != null) {
                var targetOrigin = _target.Origin;
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(origin, targetOrigin);
            }
        }
    }
}