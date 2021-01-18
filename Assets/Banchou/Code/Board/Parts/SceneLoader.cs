using System;
using System.Collections;
using System.Collections.Generic;
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
                .SelectMany(scene => Observable.FromCoroutine(() => LoadScene(board, scene)))
                .CatchIgnoreLog()
                .Subscribe()
                .AddTo(this);

            board.LoadedScenes
                .ObserveRemove()
                .Select(remove => remove.Value)
                .SelectMany(scene => Observable.FromCoroutine(() => UnloadScene(board, scene)))
                .CatchIgnoreLog()
                .Subscribe()
                .AddTo(this);
        }
    }
}