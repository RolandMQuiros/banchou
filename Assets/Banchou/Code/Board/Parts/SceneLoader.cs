using System.Collections.Generic;
using System.Linq;

using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Banchou.Board.Part {
    public class SceneLoader : MonoBehaviour {
        public void Construct(
            GameState state
        ) {
            var instances = new Dictionary<string, SceneInstance>();

            state.ObserveAddedScenes()
                .Do(add => Debug.Log($"Loading Board scene \"{add}\""))
                .SelectMany(scene => {
                    var load = Addressables.LoadSceneAsync(scene, LoadSceneMode.Additive);
                    return load.ToObservable()
                        .Select(_ => (scene, load.Result));
                })
                .CatchIgnoreLog()
                .Subscribe(args => {
                    var (scene, instance) = args;
                    state.Board.SceneLoaded(scene);
                    instances[scene] = instance;
                })
                .AddTo(this);

            state.ObserveRemovedScenes()
                .Do(removed => Debug.Log($"Unloaded Board scene \"{removed}\""))
                .CatchIgnoreLog()
                .Subscribe(scene => {
                    Addressables.UnloadSceneAsync(instances[scene]);
                })
                .AddTo(this);
        }
    }
}