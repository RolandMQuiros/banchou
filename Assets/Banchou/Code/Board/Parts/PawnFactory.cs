using System;
using System.Linq;
using System.Collections.Generic;

using UniRx;
using UnityEngine;

using Banchou.Pawn;
using Banchou.DependencyInjection;

namespace Banchou.Board.Part {
    public class PawnFactory : MonoBehaviour {
        [Serializable]
        private class NamedPrefab {
            public string Key;
            public GameObject Prefab;
        }
        [SerializeField] private NamedPrefab[] _catalog = null;

        public void Construct(
            GameState state,
            Instantiator instantiate,
            IDictionary<int, GameObject> pawnObjects
        ) {
            var catalog = _catalog.ToDictionary(n => n.Key, n => n.Prefab);

            state.ObserveAddedPawns()
                .DelayFrame(0, FrameCountType.EndOfFrame)
                .Do(pawn => Debug.Log($"Creating Pawn (Id: {pawn.PawnId})"))
                .CatchIgnore()
                .Subscribe(pawn => {
                    var prefabKey = pawn.PrefabKey;
                    GameObject prefab;
                    if (catalog.TryGetValue(prefabKey, out prefab)) {
                        pawnObjects[pawn.PawnId] = instantiate(
                            prefab,
                            position: pawn.Spatial.Position,
                            rotation: Quaternion.LookRotation(pawn.Spatial.Forward),
                            parent: transform,
                            additionalBindings: (GetPawnId)(() => pawn.PawnId)
                        );
                    }
                })
                .AddTo(this);

            state.ObserveRemovedPawns()
                .CatchIgnore()
                .Subscribe(pawn => {
                    GameObject.Destroy(pawnObjects[pawn.PawnId]);
                    pawnObjects.Remove(pawn.PawnId);
                })
                .AddTo(this);
        }
    }
}