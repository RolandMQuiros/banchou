using System;
using System.Linq;
using MessagePack;
using UnityEngine;
using UniRx;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject, Serializable]
    public class BoardState : Notifiable<BoardState> {
        [IgnoreMember] public IReadOnlyReactiveCollection<string> ActiveScenes => _activeScenes;
        [Key(0), SerializeField] private ReactiveCollection<string> _activeScenes = new ReactiveCollection<string>();

        [IgnoreMember] public IReadOnlyReactiveCollection<string> LoadingScenes => _loadingScenes;
        [Key(1), SerializeField] private ReactiveCollection<string> _loadingScenes = new ReactiveCollection<string>();

        [IgnoreMember] public IReadOnlyReactiveDictionary<int, PawnState> Pawns => _pawns;
        [Key(2), SerializeField] private ReactiveDictionary<int, PawnState> _pawns = new ReactiveDictionary<int, PawnState>();

        [IgnoreMember] public IReadOnlyReactiveProperty<float> LastUpdated => _lastUpdated;
        [Key(3), SerializeField] private FloatReactiveProperty _lastUpdated = new FloatReactiveProperty();

        public BoardState SyncGame(BoardState sync) {
            // Add all of the incoming board's scenes to our loading queue, unless they've already been loaded
            var scenesToLoad = sync.ActiveScenes.Concat(sync.LoadingScenes).Except(_activeScenes);
            // Unload all scenes that aren't in play on the incoming board
            var scenesToRemove = _activeScenes.Except(scenesToLoad);

            foreach (var scene in scenesToLoad) {
                _loadingScenes.Add(scene);
            }

            foreach (var scene in scenesToRemove) {
                _activeScenes.Remove(scene);
            }


            var incomingPawnIds = sync.Pawns.Select(p => p.Key);

            // Add missing pawns
            foreach (var added in incomingPawnIds.Except(Pawns.Select(p => p.Key))) {
                _pawns[added] = sync.Pawns[added];
            }

            // Remove extraneous pawns
            foreach (var removed in Pawns.Select(p => p.Key).Except(incomingPawnIds)) {
                _pawns.Remove(removed);
            }

            // Propagate sync to remaining pawns
            foreach (var pawn in Pawns) {
                pawn.Value.SyncGame(sync.Pawns[pawn.Key]);
            }

            Notify();
            return this;
        }

        public BoardState LoadScene(string sceneName) {
            if (!_loadingScenes.Contains(sceneName) && !_activeScenes.Contains(sceneName)) {
                _loadingScenes.Add(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState SceneLoaded(string sceneName) {
            if (_loadingScenes.Remove(sceneName) && !_activeScenes.Contains(sceneName)) {
                _activeScenes.Add(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState UnloadScene(string sceneName) {
            if (_loadingScenes.Remove(sceneName) || _activeScenes.Remove(sceneName)) {
                Notify();
            }
            return this;
        }

        public BoardState AddPawn(
            int pawnId,
            string prefabKey,
            int playerId,
            Vector3 position,
            Vector3 forward,
            float when
        ) {
            var pawn = new PawnState(
                pawnId: pawnId,
                playerId: playerId,
                prefabKey: prefabKey,
                position: position,
                forward: forward,
                lastUpdated: when
            );

            _pawns.Add(pawnId, pawn);
            _lastUpdated.Value = when;

            Notify();
            return this;
        }

        public BoardState AddPawn(int pawnId, string prefabKey, int playerId, Vector3 position, float when) {
            return AddPawn(pawnId, prefabKey, playerId, position, Vector3.forward, when);
        }

        public BoardState RemovePawn(int pawnId, float when) {
            _pawns.Remove(pawnId);
            _lastUpdated.Value = when;

            Notify();
            return this;
        }

        public BoardState ClearPawns(float when) {
            _pawns.Clear();
            _lastUpdated.Value = when;

            Notify();
            return this;
        }
    }
}