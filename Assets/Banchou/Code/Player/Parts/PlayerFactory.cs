using System.Collections.Generic;

using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayerFactory : MonoBehaviour {
        public void Construct(
            GameState state,
            Instantiator instantiate
        ) {
            var instances = new Dictionary<int, GameObject>();
            state.ObserveAddedPlayers()
                .DelayFrame(0, FrameCountType.EndOfFrame)
                .SelectMany(player => {
                    var load = Addressables.LoadAssetAsync<GameObject>(player.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Player: player, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (player, prefab) = args;
                    var instance = instantiate(
                        prefab,
                        parent: transform,
                        additionalBindings: (GetPlayerId)(() => player.PlayerId)
                    );
                    instances[player.PlayerId] = instance;
                    
                    var view = instance.GetComponent<PhotonView>();
                    if (view != null) {
                        view.ViewID = player.NetworkId;
                    }

                    instances[player.PlayerId] = instance;
                })
                .AddTo(this);

            state.ObserveRemovedPlayers()
                .CatchIgnore()
                .Subscribe(player => {
                    Destroy(instances[player.PlayerId]);
                    instances.Remove(player.PlayerId);
                })
                .AddTo(this);
        }
    }
}