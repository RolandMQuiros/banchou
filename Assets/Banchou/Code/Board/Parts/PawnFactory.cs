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
            BoardState board,
            Instantiator instantiate
        ) {
            var catalog = _catalog.ToDictionary(n => n.Key, n => n.Prefab);
            var spawned = new Dictionary<int, GameObject>();

            board.Pawns
                .ObserveAdd()
                .Select(add => add.Value)
                .CatchIgnore()
                .Subscribe(pawn => {
                    var prefabKey = pawn.PrefabKey;
                    GameObject prefab;
                    if (catalog.TryGetValue(prefabKey, out prefab)) {
                        spawned[pawn.PawnId] = instantiate(
                            prefab,
                            position: pawn.Spatial.Position,
                            rotation: Quaternion.LookRotation(pawn.Spatial.Forward),
                            parent: transform,
                            additionalBindings: (GetPawnId)(() => pawn.PawnId)
                        );
                    }
                })
                .AddTo(this);

            board.Pawns
                .ObserveRemove()
                .Select(add => add.Value)
                .CatchIgnore()
                .Subscribe(pawn => {
                    GameObject.Destroy(spawned[pawn.PawnId]);
                })
                .AddTo(this);
        }
    }
}