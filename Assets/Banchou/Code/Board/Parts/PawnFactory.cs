using System;
using System.Linq;
using System.Collections.Generic;

using UniRx;
using UnityEngine;

using Banchou.Pawn;
using Banchou.Board;
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
            IObservable<GameState> observeState,
            Instantiator instantiate
        ) {
            var catalog = _catalog.ToDictionary(n => n.Key, n => n.Prefab);
            var spawned = new Dictionary<int, GameObject>();

            observeState
                .OnBoardChanged()
                .CatchIgnoreLog()
                .Subscribe(
                    state => {
                        var pawnIds = state.GetPawnIds();
                        var added = pawnIds.Except(spawned.Keys);
                        var removed = spawned.Keys.Except(pawnIds);

                        foreach (var id in added) {
                            var pawn = state.GetPawn(id);
                            var prefabKey = pawn.PrefabKey;
                            GameObject prefab;
                            if (catalog.TryGetValue(prefabKey, out prefab)) {
                                spawned[id] = instantiate(
                                    prefab,
                                    position: pawn.Position,
                                    rotation: Quaternion.LookRotation(pawn.Forward),
                                    parent: transform,
                                    additionalBindings: new object[] {
                                        (GetPawnId)(() => id),
                                        pawn
                                    }
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