using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

using Banchou.Pawn;

namespace Banchou.Board {
    [MessagePackObject, Serializable]
    public record BoardState(
        List<string> ActiveScenes = null,
        List<string> LoadingScenes = null,
        Dictionary<int, PawnState> Pawns = null,
        float LastUpdated = 0f
    ) : Notifiable<BoardState> {
        public event Action<string> SceneAdded;
        public event Action<string> SceneRemoved;
        public event Action<PawnState> PawnAdded;
        public event Action<PawnState> PawnRemoved;

        [field: SerializeField]
        public List<string> ActiveScenes { get; private set; } = ActiveScenes ?? new List<string>();
        
        [field: SerializeField]
        public List<string> LoadingScenes { get; private set; } = LoadingScenes ?? new List<string>();
        
        [field: SerializeField]
        public Dictionary<int, PawnState> Pawns { get; private set; } = Pawns ?? new Dictionary<int, PawnState>();
        
        [field: SerializeField]
        public float LastUpdated { get; private set; } = LastUpdated;

        public BoardState SyncGame(BoardState sync) {
            var localScenes = LoadingScenes.Concat(ActiveScenes).Distinct().ToList();
            var remoteScenes = sync.LoadingScenes.Concat(sync.ActiveScenes).ToList();

            // Add all of the incoming board's scenes to our loading queue, unless they've already been loaded
            var scenesToLoad = remoteScenes.Except(localScenes);

            // Unload all scenes that aren't in play on the incoming board
            var scenesToRemove = localScenes.Except(remoteScenes).ToList();

            foreach (var scene in scenesToLoad) {
                LoadingScenes.Add(scene);
                SceneAdded?.Invoke(scene);
            }

            foreach (var scene in scenesToRemove) {
                if (ActiveScenes.Remove(scene)) {
                    SceneRemoved?.Invoke(scene);
                }
            }

            var incomingPawnIds = sync.Pawns.Keys;

            // Add missing pawns
            foreach (var added in incomingPawnIds.Except(Pawns.Keys)) {
                var pawn = sync.Pawns[added];
                Pawns[added] = pawn with { };
                PawnAdded?.Invoke(pawn);
            }

            // Remove extraneous pawns
            foreach (var removed in Pawns.Keys.Except(incomingPawnIds)) {
                var pawn = Pawns[removed];
                Pawns.Remove(removed);
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
            if (!LoadingScenes.Contains(sceneName) && !ActiveScenes.Contains(sceneName)) {
                LoadingScenes.Add(sceneName);
                SceneAdded?.Invoke(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState SceneLoaded(string sceneName) {
            if (LoadingScenes.Remove(sceneName) && !ActiveScenes.Contains(sceneName)) {
                ActiveScenes.Add(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState UnloadScene(string sceneName) {
            if (LoadingScenes.Remove(sceneName) || ActiveScenes.Remove(sceneName)) {
                SceneRemoved?.Invoke(sceneName);
                Notify();
            }
            return this;
        }

        public BoardState AddPawn(
            float when,
            out PawnState pawn,
            int pawnId = default,
            string prefabKey = default,
            int playerId = default,
            Vector3 position = default,
            Vector3 forward = default
        ) {
            if (pawnId == default) {
                // Find the first available pawn ID
                var usedIds = Pawns.Keys.OrderBy(id => id).ToList();
                for (pawnId = 1; pawnId <= usedIds.Count && pawnId == usedIds[pawnId - 1]; pawnId++) { }
            }
            
            pawn = new PawnState(
                pawnId,
                playerId: playerId,
                prefabKey: prefabKey,
                position: position,
                forward: forward == Vector3.zero ? Vector3.forward : forward,
                lastUpdated: when
            );

            // Add new pawn
            Pawns[pawnId] = pawn;
            LastUpdated = when;
            PawnAdded?.Invoke(pawn);
            
            return Notify();
        }

        public BoardState RemovePawn(int pawnId, float when) {
            PawnState pawn;
            if (Pawns.TryGetValue(pawnId, out pawn) && Pawns.Remove(pawnId)) {
                LastUpdated = when;
                PawnRemoved?.Invoke(pawn);
                return Notify();
            }
            return this;
        }

        public BoardState ClearPawns(float when) {
            if (Pawns.Any()) {
                if (PawnRemoved != null) {
                    foreach (var pawn in Pawns.Values) {
                        PawnRemoved(pawn);
                    }
                }
                Pawns.Clear();
                LastUpdated = when;
                Notify();
            }
            return this;
        }
    }
}