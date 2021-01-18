using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayerFactory : MonoBehaviour {
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

            state.ObserveAddedPlayers()
                .CatchIgnore()
                .Subscribe(player => {
                    var prefabKey = player.PrefabKey;
                    GameObject prefab;
                    if (catalog.TryGetValue(prefabKey, out prefab)) {
                        spawned[player.PlayerId] = instantiate(
                            prefab,
                            parent: transform,
                            additionalBindings: (GetPlayerId)(() => player.PlayerId)
                        );
                    }
                })
                .AddTo(this);

            state.ObserveRemovedPlayers()
                .CatchIgnore()
                .Subscribe(player => {
                    GameObject.Destroy(spawned[player.PlayerId]);
                })
                .AddTo(this);
        }
    }
}