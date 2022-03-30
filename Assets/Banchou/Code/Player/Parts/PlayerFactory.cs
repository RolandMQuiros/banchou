using System.Collections.Generic;
using Banchou.Board.Part;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayerFactory : MonoBehaviour {
        public void Construct(
            GameState state,
            Instantiator instantiate,
            GetMutablePlayerInstances getPlayerInstances
        ) {
            var playerInstances = getPlayerInstances();
            var createdByFactory = new Dictionary<int, (GameObject Instance, string PrefabKey)>();
            
            state.ObserveRemovedPlayers()
                .CatchIgnoreLog()
                .Subscribe(player => {
                    if (playerInstances.TryGetValue(player.PlayerId, out var instance)) {
                        playerInstances.Remove(player.PlayerId);
                        createdByFactory.Remove(player.PlayerId);
                        Destroy(instance);
                    }
                })
                .AddTo(this);
            
            state.ObserveAddedPlayers()
                .Where(player => !string.IsNullOrEmpty(player.PrefabKey))
                .DelayFrame(0, FrameCountType.EndOfFrame)
                .SelectMany(player => {
                    var load = Addressables.LoadAssetAsync<GameObject>(player.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Player: player, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (player, prefab) = args;
                    
                    // If an instance exists with the same prefab key, we can skip
                    if (createdByFactory.TryGetValue(player.PlayerId, out var old) && old.PrefabKey == player.PrefabKey) {
                        Debug.Log($"Player instance for player ID {player.PlayerId} " +
                                  $"with prefab key {player.PrefabKey} exists.");
                    } else {
                        // If an instance exists with a different prefab key, destroy it
                        if (old.Instance != null) {
                            Debug.Log($"Player {player.PlayerId} instance with prefab key {old.PrefabKey} " +
                                      $"replaced with {player.PrefabKey}");
                            Destroy(old.Instance);
                        }

                        var instance = instantiate(
                            prefab,
                            parent: transform,
                            additionalBindings: (GetPlayerId) (() => player.PlayerId)
                        );
                        createdByFactory[player.PlayerId] = (instance, player.PrefabKey);

                        var view = instance.GetComponent<PhotonView>();
                        if (view != null) {
                            view.ViewID = player.NetworkId;
                        }

                        playerInstances[player.PlayerId] = instance;
                    }
                })
                .AddTo(this);
        }
    }
}