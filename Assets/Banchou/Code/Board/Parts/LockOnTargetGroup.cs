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
        private readonly Dictionary<int, CameraTarget> _targets = new();
        
        public void Construct(
            GameState state,
            GetPawnObjects getPawnObjects
        ) {
            _targetGroup = GetComponent<CinemachineTargetGroup>();

            var pawnObjects = getPawnObjects();
            state.ObserveLockOns(_targetingTeam)
                .Select(targetId => {
                    pawnObjects.TryGetValue(targetId, out var target);
                    return (targetId, target);
                })
                .Where(args => args.target != null)
                .CatchIgnoreLog()
                .Subscribe(args => AddTarget(args.targetId, args.target))
                .AddTo(this);
            
            state.ObserveLockOffs(_targetingTeam)
                .Select(targetId => {
                    _targets.TryGetValue(targetId, out var target);
                    return (targetId, target);
                })
                .Where(args => args.target != null)
                .CatchIgnoreLog()
                .Subscribe(args => {
                    _targetGroup.RemoveMember(args.target.transform);
                    _targets.Remove(args.targetId);
                })
                .AddTo(this);
        }
        
        private void AddTarget(int pawnId, GameObject pawnObject) {
            var targets = pawnObject.transform
                .BreadthFirstTraversal()
                .Select(child => child.GetComponent<CameraTarget>())
                .Where(target => target != null)
                .ToList();
                    
            foreach (var target in targets) {
                if (_targetGroup.FindMember(target.transform) == -1) {
                    _targets[pawnId] = target;
                    _targetGroup.AddMember(target.transform, target.Weight, target.Radius);
                }
            }
        } 
    }
}