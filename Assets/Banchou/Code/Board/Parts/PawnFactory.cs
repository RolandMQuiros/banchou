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
            GetState getState,
            Instantiator instantiate
        ) {
            var catalog = _catalog.ToDictionary(n => n.Key, n => n.Prefab);
            var spawned = new Dictionary<int, GameObject>();

            getState().ObserveBoard()
                .CatchIgnoreLog()
                .Subscribe(
                    _ => {
                        Debug.Log("Board update detected");
                        var state = getState();
                        var added = state.GetPawnIds().Except(spawned.Keys);
                        var removed = spawned.Keys.Except(state.GetPawnIds());

                        foreach (var id in added) {
                            var prefabKey = state.GetPawnPrefabKey(id);
                            var vectors = state.GetPawnSpatial(id);
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