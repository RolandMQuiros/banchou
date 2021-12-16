using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Banchou.Combatant;
using Banchou.Pawn.Part;
using Banchou.Player;
using Cinemachine;
using UniRx;
using UnityEngine;

namespace Banchou.Board.Part {
    [RequireComponent(typeof(CinemachineTargetGroup))]
    public class LockOnTargetGroup : MonoBehaviour {
        [SerializeField] private CombatantTeam _targetingTeam;
        private CinemachineTargetGroup _targetGroup;
        
        public void Construct(
            GameState state,
            IReadOnlyReactiveDictionary<int, GameObject> pawnObjects
        ) {
            _targetGroup = GetComponent<CinemachineTargetGroup>();

            state.ObserveLockOns(_targetingTeam)
                .Select(targetId => {
                    pawnObjects.TryGetValue(targetId, out var target);
                    return target;
                })
                .Where(target => target != null)
                .CatchIgnoreLog()
                .Subscribe(AddTarget)
                .AddTo(this);
            
            state.ObserveLockOffs(_targetingTeam)
                .Select(targetId => {
                    pawnObjects.TryGetValue(targetId, out var target);
                    return target;
                })
                .Where(target => target != null)
                .CatchIgnoreLog()
                .Subscribe(target => _targetGroup.RemoveMember(target.transform))
                .AddTo(this);
        }
        
        private void AddTarget(GameObject pawnObject) {
            var targets = pawnObject.transform
                .BreadthFirstTraversal()
                .Select(child => child.GetComponent<CameraTarget>())
                .Where(target => target != null)
                .Select(target => (Transform: target.transform, target.Weight, target.Radius))
                .DefaultIfEmpty((Transform: pawnObject.transform, Weight: 1f, Radius: 1f))
                .ToList();
                    
            foreach (var target in targets) {
                if (_targetGroup.FindMember(target.Transform) == -1) {
                    _targetGroup.AddMember(target.Transform, target.Weight, target.Radius);
                }
            }
        } 
    }
}