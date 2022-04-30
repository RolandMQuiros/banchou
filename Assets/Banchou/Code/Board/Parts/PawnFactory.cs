using System.Collections.Generic;

using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Banchou.Pawn;
using Banchou.DependencyInjection;

namespace Banchou.Board.Part {
    public class PawnFactory : MonoBehaviour {
        public void Construct(
            GameState state,
            Instantiator instantiate,
            GetMutablePawnObjects getPawnObjects
        ) {
            var pawnObjects = getPawnObjects();
            var createdInstances = new Dictionary<int, (GameObject Instance, string PrefabKey)>();
            
            state.ObserveRemovedPawns()
                .CatchIgnoreLog()
                .Subscribe(pawn => {
                    if (pawnObjects.TryGetValue(pawn.PawnId, out var instance)) {
                        pawnObjects.Remove(pawn.PawnId);
                        createdInstances.Remove(pawn.PawnId);
                        if (instance != null) {
                            Destroy(instance);
                        }
                    }
                })
                .AddTo(this);
            
            state.ObserveAddedPawns()
                .Where(pawn => !string.IsNullOrEmpty(pawn.PrefabKey))
                .Do(pawn => Debug.Log($"Creating Pawn {pawn.PawnId} from Prefab {pawn.PrefabKey}"))
                .SelectMany(pawn => {
                    var load = Addressables.LoadAssetAsync<GameObject>(pawn.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Pawn: pawn, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (pawn, prefab) = args;
                    
                    // If an instance exists with the same prefab key, we can skip
                    if (createdInstances.TryGetValue(pawn.PawnId, out var old) && old.PrefabKey == pawn.PrefabKey) {
                        Debug.Log($"Pawn instance for pawn ID {pawn.PawnId} with prefab key {pawn.PrefabKey} exists.");
                    } else {
                        // If an instance exists with a different prefab key, destroy it
                        if (old.Instance != null) {
                            Debug.Log($"Pawn {pawn.PawnId} instance with prefab key {old.PrefabKey} " +
                                      $"replaced with {pawn.PrefabKey}");
                            Destroy(old.Instance);
                        }
                        var instance = instantiate(
                            prefab,
                            pawn.Spatial.Position,
                            Quaternion.LookRotation(pawn.Spatial.Forward),
                            transform,
                            (GetPawnId)(() => pawn.PawnId)
                        );
                        createdInstances[pawn.PawnId] = (instance, pawn.PrefabKey);
                        pawnObjects[pawn.PawnId] = instance;
                    }
                })
                .AddTo(this);
        }
    }
}