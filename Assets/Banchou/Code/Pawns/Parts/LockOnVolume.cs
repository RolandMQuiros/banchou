using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

using Banchou.Combatant;
using Banchou.Player;
using UnityEngine.UIElements;

namespace Banchou.Pawn.Part {
    [RequireComponent(typeof(Collider))]
    public class LockOnVolume : MonoBehaviour {
        [SerializeField] private LayerMask _obstructionMask;
        [SerializeField] private Vector3 _linecastOrigin;

        public Vector3 Origin => transform.TransformPoint(_linecastOrigin);

        private int _pawnId;
        private readonly HashSet<LockOnTarget> _targets = new(32);
        private LockOnTarget _target;

        public void Construct(GameState state, GetPawnId getPawnId) {
            _pawnId = getPawnId();
            
            state.ObservePawnInput(_pawnId)
                .WithLatestFrom(
                    state.ObserveCombatant(_pawnId),
                    (input, combatant) => (input, Combatant: combatant)
                )
                .CatchIgnoreLog()
                .Subscribe(args => {
                    var (input, combatant) = args;

                    if (input.Commands.HasFlag(InputCommand.LockOn)) {
                        if (args.Combatant.LockOnTarget == default) {
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
                                .FirstOrDefault(target => target.PawnId != combatant.LockOnTarget);
                            if (selected != default) {
                                _target = selected;
                                combatant.LockOn(selected.PawnId, state.GetTime());
                            }
                        } else {
                            // If combatant has a lock on target, unlock
                            args.Combatant.LockOff(state.GetTime());
                        }
                    }
                    
                    if (input.Commands.HasFlag(InputCommand.LockOff)) {
                        args.Combatant.LockOff(state.GetTime());
                    }
                })
                .AddTo(this);
        }

        private void OnTriggerEnter(Collider other) {
            var target = other.GetComponent<LockOnTarget>();
            if (target != null && target.PawnId != _pawnId) {
                _targets.Add(target);
            } 
        }

        private void OnTriggerExit(Collider other) {
            var target = other.GetComponent<LockOnTarget>();
            if (target != null) {
                _targets.Remove(target);
            }
        }

        private void OnDrawGizmos() {
            var origin = Origin;
            var forward = transform.forward;
            var up = transform.up;
            
            foreach (var target in _targets) {
                var dot = Vector3.Dot((target.Origin - origin).normalized, forward);
                var targetOrigin = target.Origin;
                var distance = (target.Origin - origin).magnitude;

                if (target != _target && dot > 0f) {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(origin, targetOrigin);
                    // Gizmos.DrawLine(targetOrigin, origin + dot * forward);

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