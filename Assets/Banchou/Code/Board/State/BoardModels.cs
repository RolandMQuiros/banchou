using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

using Banchou.Pawn;
using Banchou.Combatant;

namespace Banchou.Board {
    [MessagePackObject, Serializable]
    public class BoardState : Notifiable<BoardState> {
        public event Action<string> SceneAdded;
        public event Action<string> SceneRemoved;
        public event Action<PawnState> PawnAdded;
        public event Action<PawnState> PawnRemoved;

        [Key(0)][field: SerializeField] public List<string> ActiveScenes { get; private set; } = new List<string>();
        [Key(1)][field: SerializeField] public List<string> LoadingScenes { get; private set; } = new List<string>();
        [Key(2)][field: SerializeField] public Dictionary<int, PawnState> Pawns { get; private set; } = new Dictionary<int, PawnState>();
        [Key(3)][field: SerializeField] public CombatantStates Combatants { get; private set; } = new CombatantStates();
        [Key(4)][field: SerializeField] public float LastUpdated { get; private set; }

        public BoardState() { }

        [SerializationConstructor]
        public BoardState(
            List<string> activeScenes,
            List<string> loadingScenes,
            Dictionary<int, PawnState> pawns,
            CombatantStates combatants,
            float lastUpdated
        ) {
            ActiveScenes = activeScenes;
            LoadingScenes = loadingScenes;
            Pawns = pawns;
            Combatants = combatants;
            LastUpdated = lastUpdated;
        }

        public BoardState SyncGame(BoardState sync) {
            var localScenes = LoadingScenes.Concat(ActiveScenes).Distinct();
            var remoteScenes = sync.LoadingScenes.Concat(sync.ActiveScenes);

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
                Pawns[added] = new PawnState(pawn);
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

            Pawns.Add(pawnId, pawn);
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
            if (Pawns.TryGetValue(pawnId, out pawn) && Pawns.Remove(pawnId)) {
                LastUpdated = when;
                PawnRemoved?.Invoke(pawn);
                Notify();
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