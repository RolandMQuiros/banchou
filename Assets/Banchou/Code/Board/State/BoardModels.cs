using System;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject]
    public class BoardState : Substate<BoardState> {
        [IgnoreMember] public IReadOnlyCollection<string> LoadedScenes => _loadedScenes;
        [Key(0)] private HashSet<string> _loadedScenes = new HashSet<string>();

        [IgnoreMember] public IReadOnlyCollection<string> LoadingScenes => _loadingScenes;
        [Key(1)] private HashSet<string> _loadingScenes = new HashSet<string>();

        [IgnoreMember] public IReadOnlyDictionary<int, PawnState> Pawns => _pawns;
        [Key(2), SerializeField] private Dictionary<int, PawnState> _pawns = new Dictionary<int, PawnState>();
        [IgnoreMember] public float LastUpdated => _lastUpdated;
        [Key(3), SerializeField] private float _lastUpdated;

        protected override void OnProcess() {
            // foreach (var pawn in Pawns.Values) {
            //     pawn.Process();
            // }
        }

        public BoardState SyncGame(GameState sync) {
            PatchPawns(sync.Board);
            foreach (var pawn in Pawns.Values) {
                pawn.SyncGame(sync);
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
            if (_loadingScenes.Remove(sceneName) && _loadedScenes.Add(sceneName)) {
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
            _lastUpdated = when;

            Notify();
            return this;
        }

        public BoardState AddPawn(int pawnId, string prefabKey, int playerId, Vector3 position, float when) {
            return AddPawn(pawnId, prefabKey, playerId, position, Vector3.forward, when);
        }

        public BoardState RemovePawn(int pawnId, float when) {
            _pawns.Remove(pawnId);
            _lastUpdated = when;

            Notify();
            return this;
        }

        public BoardState ClearPawns(float when) {
            _pawns.Clear();
            _lastUpdated = when;

            Notify();
            return this;
        }

        private void PatchPawns(BoardState other) {
            var otherPawnIds = other.Pawns.Keys;

            // Add missing pawns
            foreach (var added in otherPawnIds.Except(Pawns.Keys)) {
                _pawns[added] = other.Pawns[added];
            }

            // Remove extraneous pawns
            foreach (var removed in Pawns.Keys.Except(otherPawnIds)) {
                _pawns.Remove(removed);
            }
        }
    }
}