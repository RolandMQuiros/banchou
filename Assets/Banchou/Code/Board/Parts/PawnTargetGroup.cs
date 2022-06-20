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
            IsCombatant = 1,
            IsFriendly = 1 << 1,
            IsEnemy = 1 << 2,
        }

        [SerializeField] private Criteria _criteria;
        
        private CinemachineTargetGroup _targetGroup;
        
        public void Construct(
            GameState state,
            GetPawnObjects getPawnObjects
        ) {
            _targetGroup = GetComponent<CinemachineTargetGroup>();
            var pawnObjects = getPawnObjects();
            
            pawnObjects.ObserveAdd()
                .Select(added => (PawnId: added.Key, PawnObject: added.Value))
                .Merge(
                    pawnObjects.ObserveReplace()
                        .Select(replaced => (PawnId: replaced.Key, PawnObject: replaced.NewValue))
                )
                .Where(
                    added =>
                        _criteria == 0 ||
                        (_criteria.HasFlag(Criteria.IsCombatant) && state.IsCombatant(added.PawnId)) ||
                        (_criteria.HasFlag(Criteria.IsFriendly) && state.GetCombatantTeam(added.PawnId) == CombatantTeam.Friendly) ||
                        (_criteria.HasFlag(Criteria.IsEnemy) && state.GetCombatantTeam(added.PawnId) == CombatantTeam.Enemy)
                )
                .CatchIgnoreLog()
                .Subscribe(added => { AddTarget(added.PawnId, added.PawnObject); })
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