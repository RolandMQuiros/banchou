using System;
using System.Collections;
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
            IDictionary<int, GameObject> pawnObjects
        ) {
            state.ObserveAddedPawns()
                .DelayFrame(0, FrameCountType.EndOfFrame)
                .Do(pawn => Debug.Log($"Creating Pawn (Id: {pawn.PawnId})"))
                .SelectMany(pawn => {
                    var load = Addressables.LoadAssetAsync<GameObject>(pawn.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Pawn: pawn, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (pawn, prefab) = args;
                    pawnObjects[pawn.PawnId] = instantiate(
                        prefab,
                        position: pawn.Spatial.Position,
                        rotation: Quaternion.LookRotation(pawn.Spatial.Forward),
                        parent: transform,
                        additionalBindings: (GetPawnId)(() => pawn.PawnId)
                    );
                })
                .AddTo(this);

            state.ObserveRemovedPawns()
                .CatchIgnore()
                .Subscribe(pawn => {
                    Destroy(pawnObjects[pawn.PawnId]);
                    pawnObjects.Remove(pawn.PawnId);
                })
                .AddTo(this);
        }
    }
}