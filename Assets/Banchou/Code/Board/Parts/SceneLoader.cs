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
            IObservable<GameState> observeState,
            BoardState board
        ) {
            var currentlyLoading = new HashSet<string>();

            IEnumerator LoadScene(BoardState board, string sceneName) {
                currentlyLoading.Add(sceneName);
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                currentlyLoading.Remove(sceneName);
                board.SceneLoaded(sceneName);
            }

            board.Observe()
                .SelectMany(_ => board.LoadingScenes)
                .Where(scene => !currentlyLoading.Contains(scene))
                .SelectMany(scene => Observable.FromCoroutine(() => LoadScene(board, scene)))
                .CatchIgnoreLog()
                .Subscribe()
                .AddTo(this);
        }
    }
}