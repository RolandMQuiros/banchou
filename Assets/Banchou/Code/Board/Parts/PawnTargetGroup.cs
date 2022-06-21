using System;
using System.Linq;
using Cinemachine;
using UniRx;
using UnityEngine;

using Banchou.Combatant;
using Banchou.Pawn.Part;

namespace Banchou.Board.Part {
    [RequireComponent(typeof(CinemachineTargetGroup))]
    public class PawnTargetGroup : MonoBehaviour {
        [Flags] private enum Criteria {
            IsFriendly,
            IsEnemy
        }

        [SerializeField] private Criteria _criteria;
        
        private CinemachineTargetGroup _targetGroup;
        
        public void Construct(
            GameState state,
            GetPawnObjects getPawnObjects
        ) {
            _targetGroup = GetComponent<CinemachineTargetGroup>();
            var pawnObjects = getPawnObjects();

            state.ObserveCombatants()
                .SelectMany(combatant => combatant.Stats.Observe()
                    .Select(_ => combatant))
                .Where(
                    added =>
                        _criteria == 0 ||
                        (_criteria.HasFlag(Criteria.IsFriendly) && state.GetCombatantTeam(added.PawnId) == CombatantTeam.Friendly) ||
                        (_criteria.HasFlag(Criteria.IsEnemy) && state.GetCombatantTeam(added.PawnId) == CombatantTeam.Enemy)
                )
                .CatchIgnoreLog()
                .Subscribe(added => {
                    if (pawnObjects.TryGetValue(added.PawnId, out var pawnObject)) {
                        AddTarget(added.PawnId, pawnObject);
                    }
                })
                .AddTo(this);

            pawnObjects.ObserveRemove()
                .Select(removed => removed.Value.transform)
                .Merge(
                    pawnObjects.ObserveReplace()
                        .Select(replaced => replaced.OldValue.transform)
                )
                .Where(removed => _targetGroup.FindMember(removed) != -1)
                .CatchIgnoreLog()
                .Subscribe(removed => _targetGroup.RemoveMember(removed))
                .AddTo(this);

            foreach (var pair in pawnObjects) {
                AddTarget(pair.Key, pair.Value);
            }
        }

        private void AddTarget(int pawnId, GameObject pawnObject) {
            var targets = pawnObject.transform
                .BreadthFirstTraversal()
                .Select(child => child.GetComponent<CameraTarget>())
                .Where(target => target != null)
                .Select(target => (Transform: target.transform, target.Weight, target.Radius))
                .DefaultIfEmpty((Transform: pawnObject.transform, Weight: 1f, Radius: 1f))
                .ToList();
                    
            foreach (var target in targets) {
                _targetGroup.AddMember(target.Transform, target.Weight, target.Radius);
            }
        }
    }
}