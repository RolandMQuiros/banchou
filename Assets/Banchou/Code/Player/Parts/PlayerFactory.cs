using System.Linq;
using System.Collections.Generic;
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
            var spawned = new Dictionary<int, GameObject>();

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
                    spawned[player.PlayerId] = instantiate(
                        prefab,
                        parent: transform,
                        additionalBindings: (GetPlayerId)(() => player.PlayerId)
                    );
                })
                .AddTo(this);

            state.ObserveRemovedPlayers()
                .CatchIgnore()
                .Subscribe(player => {
                    Destroy(spawned[player.PlayerId]);
                })
                .AddTo(this);
        }
    }
}