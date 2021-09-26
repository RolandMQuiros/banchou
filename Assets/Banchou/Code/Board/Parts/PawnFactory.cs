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
            Instantiator instantiate
        ) {
            var instances = new Dictionary<int, (GameObject Instance, string PrefabKey)>();
            
            state.ObserveAddedPawns()
                .Where(pawn => !string.IsNullOrEmpty(pawn.PrefabKey))
                .Do(pawn => Debug.Log($"Creating Pawn (Id: {pawn.PawnId})"))
                .SelectMany(pawn => {
                    var load = Addressables.LoadAssetAsync<GameObject>(pawn.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Pawn: pawn, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (pawn, prefab) = args;
                    
                    // If an instance exists with the same prefab key, we can skip
                    if (instances.TryGetValue(pawn.PawnId, out var old) && old.PrefabKey == pawn.PrefabKey) {
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
                            position: pawn.Spatial.Position,
                            rotation: Quaternion.LookRotation(pawn.Spatial.Forward),
                            parent: transform,
                            additionalBindings: (GetPawnId)(() => pawn.PawnId)
                        );
                    
                        instances[pawn.PawnId] = (instance, pawn.PrefabKey);
                    }
                })
                .AddTo(this);
        }
    }
}