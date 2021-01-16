using System;
using System.Linq;
using System.Collections.Generic;

using UniRx;
using UnityEngine;

using Banchou.Pawn;
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
            IObservable<GameState> observeState,
            Instantiator instantiate
        ) {
            var catalog = _catalog.ToDictionary(n => n.Key, n => n.Prefab);
            var spawned = new Dictionary<int, GameObject>();

            observeState
                .OnPlayersChanged()
                .CatchIgnoreLog()
                .Subscribe(
                    state => {
                        var playerIds = state.GetPlayerIds();

                        var added = playerIds.Except(spawned.Keys);
                        var removed = spawned.Keys.Except(playerIds);

                        foreach (var id in added) {
                            var player = state.GetPlayer(id);
                            var prefabKey = player.PrefabKey;
                            GameObject prefab;
                            if (catalog.TryGetValue(prefabKey, out prefab)) {
                                spawned[id] = instantiate(
                                    prefab,
                                    parent: transform,
                                    additionalBindings: (GetPlayerId)(() => id)
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