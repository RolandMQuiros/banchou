using System;
using System.Linq;
using System.Collections.Generic;

using UniRx;
using UnityEngine;

using Banchou.Board;
using Banchou.Pawn;
using Banchou.DependencyInjection;

namespace Banchou.Part {
    public class PawnFactory : MonoBehaviour {
        [Serializable]
        private class NamedPrefab {
            public string Key;
            public GameObject Prefab;
        }

        [SerializeField] private NamedPrefab[] _catalog = null;

        public void Construct(
            GameState state,
            Instantiator instantiate
        ) {
            var catalog = _catalog.ToDictionary(n => n.Key, n => n.Prefab);
            var spawned = new Dictionary<int, GameObject>();

            state.ObserveBoard()
                .CatchIgnoreLog()
                .Subscribe(
                    _ => {
                        var added = state.GetPawnIds().Except(spawned.Keys);
                        var removed = spawned.Keys.Except(state.GetPawnIds());

                        foreach (var id in added) {
                            var prefabKey = state.GetPawnPrefabKey(id);
                            var vectors = state.GetPawnVectors(id);
                            GameObject prefab;
                            if (catalog.TryGetValue(prefabKey, out prefab)) {
                                spawned[id] = instantiate(
                                    prefab,
                                    position: vectors.Position,
                                    rotation: Quaternion.LookRotation(vectors.Forward),
                                    parent: transform,
                                    (GetPawnId)(() => id)
                                );
                            }
                        }

                        foreach (var id in removed) {
                            GameObject.Destroy(spawned[id]);
                        }
                    }
                ).AddTo(this);
        }
    }
}