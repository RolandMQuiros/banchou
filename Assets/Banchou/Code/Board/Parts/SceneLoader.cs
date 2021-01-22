using System.Collections;
using System.Linq;

using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Banchou.Board.Part {
    public class SceneLoader : MonoBehaviour {
        public void Construct(
            BoardState board
        ) {
            IEnumerator LoadScene(BoardState board, string sceneName) {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                board.SceneLoaded(sceneName);
            }

            IEnumerator UnloadScene(BoardState board, string sceneName) {
                yield return SceneManager.UnloadSceneAsync(sceneName);
            }

            board.LoadingScenes
                .ObserveAdd()
                .Select(add => add.Value)
                .StartWith(board.LoadingScenes)
                .Do(add => Debug.Log($"Loading Board scene \"{add}\""))
                .CatchIgnoreLog()
                .Subscribe(scene => {
                    StartCoroutine(LoadScene(board, scene));
                })
                .AddTo(this);

            board.ActiveScenes
                .ObserveRemove()
                .Select(remove => remove.Value)
                .CatchIgnoreLog()
                .Subscribe(scene => {
                    StartCoroutine(UnloadScene(board, scene));
                })
                .AddTo(this);
        }
    }
}