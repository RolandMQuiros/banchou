using System;
using System.Linq;
using MessagePack;
using UnityEngine;
using UniRx;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject, Serializable]
    public class BoardState : Notifiable<BoardState> {
        [IgnoreMember] public IReadOnlyReactiveCollection<string> LoadedScenes => _loadedScenes;
        [Key(0), SerializeField] private ReactiveCollection<string> _loadedScenes = new ReactiveCollection<string>();

        [IgnoreMember] public IReadOnlyReactiveCollection<string> LoadingScenes => _loadingScenes;
        [Key(1), SerializeField] private ReactiveCollection<string> _loadingScenes = new ReactiveCollection<string>();

        [IgnoreMember] public IReadOnlyReactiveDictionary<int, PawnState> Pawns => _pawns;
        [Key(2), SerializeField] private ReactiveDictionary<int, PawnState> _pawns = new ReactiveDictionary<int, PawnState>();

        [IgnoreMember] public IReadOnlyReactiveProperty<float> LastUpdated => _lastUpdated;
        [Key(3), SerializeField] private FloatReactiveProperty _lastUpdated = new FloatReactiveProperty();

        public BoardState SyncGame(GameState sync) {
            PatchPawns(sync.Board);
            foreach (var pawn in Pawns) {
                pawn.Value.SyncGame(sync);
            }
            Notify();
            return this;
        }

        public BoardState LoadScene(string sceneName) {
            if (!_loadingScenes.Contains(sceneName) && !_loadedScenes.Contains(sceneName)) {
                _loadedScenes.Add(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState SceneLoaded(string sceneName) {
            if (_loadingScenes.Remove(sceneName) && !_loadedScenes.Contains(sceneName)) {
                _loadedScenes.Add(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState UnloadScene(string sceneName) {
            if (_loadingScenes.Remove(sceneName) || _loadedScenes.Remove(sceneName)) {
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

        private void PatchPawns(BoardState other) {
            var otherPawnIds = other.Pawns.Select(p => p.Key);

            // Add missing pawns
            foreach (var added in otherPawnIds.Except(Pawns.Select(p => p.Key))) {
                _pawns[added] = other.Pawns[added];
            }

            // Remove extraneous pawns
            foreach (var removed in Pawns.Select(p => p.Key).Except(otherPawnIds)) {
                _pawns.Remove(removed);
            }
        }
    }
}