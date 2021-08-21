using System.Collections.Generic;

using Photon.Pun;

using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Banchou.Pawn;
using Banchou.DependencyInjection;

namespace Banchou.Board.Part {
    public class PawnFactory : MonoBehaviourPunCallbacks {
        private GameState _state;
        private Instantiator _instantiate;
        private IDictionary<int, GameObject> _pawnObjects;

        public void Construct(
            GameState state,
            Instantiator instantiate,
            IDictionary<int, GameObject> pawnObjects
        ) {
            _state = state;
            _instantiate = instantiate;
            _pawnObjects = pawnObjects;
        }

        public override void OnJoinedRoom() {
            _state.ObserveAddedPawns()
                .DelayFrame(0, FrameCountType.EndOfFrame)
                .Do(pawn => Debug.Log($"Creating Pawn (Id: {pawn.PawnId})"))
                .SelectMany(pawn => {
                    var load = Addressables.LoadAssetAsync<GameObject>(pawn.PrefabKey);
                    return load.ToObservable()
                        .Select(_ => (Pawn: pawn, Prefab: load.Result));
                })
                .CatchIgnore()
                .Subscribe(args => {
                    var (pawn, prefab) = args;
                    var instance = _instantiate(
                        prefab,
                        position: pawn.Spatial.Position,
                        rotation: Quaternion.LookRotation(pawn.Spatial.Forward),
                        parent: transform,
                        additionalBindings: (GetPawnId)(() => pawn.PawnId)
                    );

                    var photonView = instance.GetComponent<PhotonView>();
                    if (photonView != null) {
                        photonView.ViewID = pawn.NetworkId;
                    }

                    _pawnObjects[pawn.PawnId] = instance;
                })
                .AddTo(this);

            _state.ObserveRemovedPawns()
                .CatchIgnore()
                .Subscribe(pawn => {
                    Destroy(_pawnObjects[pawn.PawnId]);
                    _pawnObjects.Remove(pawn.PawnId);
                })
                .AddTo(this);
        }
    }
}