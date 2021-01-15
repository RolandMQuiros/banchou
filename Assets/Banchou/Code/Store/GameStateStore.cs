using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Banchou {
    [CreateAssetMenu(fileName = "GameStateStore.asset", menuName = "Banchou/Game State Store")]
    public class GameStateStore : ScriptableObject {
        private GameState _state = new GameState();

        public GameState GetState() {
            return _state;
        }

        public IObservable<GameState> ObserveState() {
            return Observable.FromEvent<GameState>(
                h => _state.Changed += h,
                h => _state.Changed -= h
            ).StartWith(_state);
        }
    }
    public delegate void ProcessStateActions();
}