using System.Collections;
using System.Linq;

using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Banchou.Board.Part {
    public class SceneLoader : MonoBehaviour {
        public void Construct(
            GameState state
        ) {
            IEnumerator LoadScene(BoardState board, string sceneName) {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                board.SceneLoaded(sceneName);
            }

            IEnumerator UnloadScene(string sceneName) {
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }

            state.ObserveAddedScenes()
                .Do(add => Debug.Log($"Loading Board scene \"{add}\""))
                .CatchIgnoreLog()
                .Subscribe(scene => {
                    StartCoroutine(LoadScene(state.Board, scene));
                })
                .AddTo(this);

            state.ObserveRemovedScenes()
                .Do(removed => Debug.Log($"Unloaded Board scene \"{removed}\""))
                .CatchIgnoreLog()
                .Subscribe(scene => {
                    StartCoroutine(UnloadScene(scene));
                })
                .AddTo(this);
        }
    }
}