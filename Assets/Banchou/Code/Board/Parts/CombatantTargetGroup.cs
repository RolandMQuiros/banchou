using System.Linq;
using Cinemachine;
using UniRx;
using UnityEngine;

using Banchou.Combatant;
using Banchou.Pawn.Part;

namespace Banchou.Board.Part {
    [RequireComponent(typeof(CinemachineTargetGroup))]
    public class CombatantTargetGroup : MonoBehaviour {
        private CinemachineTargetGroup _targetGroup;
        
        public void Construct(
            GameState state,
            IReadOnlyReactiveDictionary<int, GameObject> pawnObjects
        ) {
            _targetGroup = GetComponent<CinemachineTargetGroup>();
            
            pawnObjects.ObserveAdd()
                .Select(added => (PawnId: added.Key, PawnObject: added.Value))
                .Merge(
                    pawnObjects.ObserveReplace()
                        .Select(replaced => (PawnId: replaced.Key, PawnObject: replaced.NewValue))
                )
                .Where(added => state.IsCombatant(added.PawnId))
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