using System.Collections.Generic;

using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Banchou.DependencyInjection;

namespace Banchou.Player.Part {
    public class PlayerFactory : MonoBehaviourPunCallbacks {
        private GameState _state;
        private Instantiator _instantiate;
        private readonly Dictionary<int, GameObject> _instances = new Dictionary<int, GameObject>();

        public void Construct(
            GameState state,
            Instantiator instantiate
        ) {
            _state = state;
            _instantiate = instantiate;
        }
        
        public override void OnJoinedRoom() {
            _state.ObserveAddedPlayers()
                .DelayFrame(0, FrameCountType.EndOfFrame)
                .SelectMany(player => {
                    var load = Addressables.LoadAssetAsync<GameObject>(player.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Player: player, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (player, prefab) = args;
                    var instance = _instantiate(
                        prefab,
                        parent: transform,
                        additionalBindings: (GetPlayerId)(() => player.PlayerId)
                    );
                    _instances[player.PlayerId] = instance;
                    
                    var view = instance.GetComponent<PhotonView>();
                    if (view != null) {
                        view.ViewID = player.NetworkId;
                    }

                    _instances[player.PlayerId] = instance;
                })
                .AddTo(this);

            _state.ObserveRemovedPlayers()
                .CatchIgnore()
                .Subscribe(player => {
                    Destroy(_instances[player.PlayerId]);
                    _instances.Remove(player.PlayerId);
                })
                .AddTo(this);
        }
    }
}