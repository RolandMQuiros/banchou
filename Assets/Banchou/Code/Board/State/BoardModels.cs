using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject, Serializable]
    public class BoardState : Notifiable<BoardState> {
        public event Action<string> SceneAdded;
        public event Action<string> SceneRemoved;
        public event Action<PawnState> PawnAdded;
        public event Action<PawnState> PawnRemoved;

        [Key(0)] public IReadOnlyList<string> ActiveScenes => _activeScenes;
        [SerializeField] private List<string> _activeScenes = new List<string>();

        [Key(1)] public IReadOnlyList<string> LoadingScenes => _loadingScenes;
        [SerializeField] private List<string> _loadingScenes = new List<string>();

        [Key(2)] public IReadOnlyDictionary<int, PawnState> Pawns => _pawns;
        [SerializeField] private Dictionary<int, PawnState> _pawns = new Dictionary<int, PawnState>();

        [Key(3)][field: SerializeField] public float LastUpdated { get; private set; }

        #region Serialization Constructors
        public BoardState() { }
        public BoardState(
            IReadOnlyList<string> activeScenes,
            IReadOnlyList<string> loadingScenes,
            IReadOnlyDictionary<int, PawnState> pawns,
            float lastUpdated
        ) {
            _activeScenes = new List<string>(activeScenes);
            _loadingScenes = new List<string>(loadingScenes);
            _pawns = pawns.ToDictionary(p => p.Key,  p => p.Value);
            LastUpdated = lastUpdated;
        }
        #endregion

        public BoardState SyncGame(BoardState sync) {
            var localScenes = _loadingScenes.Concat(_activeScenes).Distinct();
            var remoteScenes = sync.LoadingScenes.Concat(sync.ActiveScenes);

            // Add all of the incoming board's scenes to our loading queue, unless they've already been loaded
            var scenesToLoad = remoteScenes.Except(localScenes);

            // Unload all scenes that aren't in play on the incoming board
            var scenesToRemove = localScenes.Except(remoteScenes).ToList();

            foreach (var scene in scenesToLoad) {
                _loadingScenes.Add(scene);
                SceneAdded?.Invoke(scene);
            }

            foreach (var scene in scenesToRemove) {
                if (_activeScenes.Remove(scene)) {
                    SceneRemoved?.Invoke(scene);
                }
            }

            var incomingPawnIds = sync.Pawns.Keys;

            // Add missing pawns
            foreach (var added in incomingPawnIds.Except(_pawns.Keys)) {
                var pawn = sync.Pawns[added];
                _pawns[added] = pawn;
                PawnAdded?.Invoke(pawn);
            }

            // Remove extraneous pawns
            foreach (var removed in _pawns.Keys.Except(incomingPawnIds)) {
                var pawn = _pawns[removed];
                _pawns.Remove(removed);
                PawnRemoved?.Invoke(pawn);
            }

            // Propagate sync to remaining pawns
            foreach (var pawn in Pawns) {
                pawn.Value.Sync(sync.Pawns[pawn.Key]);
            }

            Notify();
            return this;
        }

        public BoardState SyncBoard(IEnumerable<PawnState> incoming) {
            foreach (var incomingPawn in incoming) {
                PawnState pawn;
                if (Pawns.TryGetValue(incomingPawn.PawnId, out pawn)) {
                    pawn.Sync(incomingPawn);
                }
            }
            return this;
        }

        public BoardState LoadScene(string sceneName) {
            if (!_loadingScenes.Contains(sceneName) && !_activeScenes.Contains(sceneName)) {
                _loadingScenes.Add(sceneName);
                SceneAdded?.Invoke(sceneName);
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
                SceneRemoved?.Invoke(sceneName);
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
            LastUpdated = when;

            PawnAdded?.Invoke(pawn);
            Notify();
            return this;
        }

        public BoardState AddPawn(int pawnId, string prefabKey, int playerId, Vector3 position, float when) {
            return AddPawn(pawnId, prefabKey, playerId, position, Vector3.forward, when);
        }

        public BoardState RemovePawn(int pawnId, float when) {
            PawnState pawn;
            if (_pawns.TryGetValue(pawnId, out pawn) && _pawns.Remove(pawnId)) {
                LastUpdated = when;
                PawnRemoved?.Invoke(pawn);
                Notify();
            }
            return this;
        }

        public BoardState ClearPawns(float when) {
            if (_pawns.Any()) {
                if (PawnRemoved != null) {
                    foreach (var pawn in _pawns.Values) {
                        PawnRemoved(pawn);
                    }
                }
                _pawns.Clear();
                LastUpdated = when;
                Notify();
            }
            return this;
        }
    }
}