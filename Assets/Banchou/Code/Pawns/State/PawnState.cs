using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    public delegate int GetPawnId();
    [MessagePackObject, Serializable]
    public class PawnState : Notifiable<PawnState> {
        [Key(0)] public readonly int PawnId;
        [Key(1)] public readonly string PrefabKey;
        [Key(2)][field: SerializeField] public int PlayerId { get; private set; }
        [Key(3)][field: SerializeField] public PawnSpatial Spatial { get; private set; }
        [Key(4)][field: SerializeField] public PawnAnimatorFrame AnimatorFrame { get; private set; }
        [Key(5)][field: SerializeField] public float LastUpdated { get; private set; }

        [SerializationConstructor]
        public PawnState(
            int pawnId,
            string prefabKey,
            int playerId,
            PawnSpatial spatial,
            PawnAnimatorFrame animatorFrame,
            float lastUpdated
        ) {
            PawnId = pawnId;
            PrefabKey = prefabKey;
            PlayerId = playerId;
            Spatial = spatial;
            AnimatorFrame = animatorFrame;
            LastUpdated = lastUpdated;
        }

        public PawnState(
            int pawnId,
            string prefabKey,
            int playerId = 0,
            Vector3 position = default,
            Vector3? forward = null,
            Vector3? up = null,
            float lastUpdated = 0f
        ) {
            PawnId = pawnId;
            PrefabKey = prefabKey;
            PlayerId = playerId;
            Spatial = new PawnSpatial(pawnId, position, forward ?? Vector3.forward, up ?? Vector3.up, lastUpdated);
            LastUpdated = lastUpdated;
        }

        public PawnState(in PawnState other) {
            PawnId = other.PawnId;
            PrefabKey = other.PrefabKey;
            PlayerId = other.PlayerId;
            Spatial = other.Spatial ?? new PawnSpatial(PawnId);
            LastUpdated = other.LastUpdated;
        }

        public PawnState Sync(PawnState sync) {
            Spatial.Sync(sync.Spatial);
            // History.Sync(sync.History);
            LastUpdated = sync.LastUpdated;

            if (sync.PlayerId != PlayerId) {
                PlayerId = sync.PlayerId;
                return Notify();
            }
            return this;
        }

        public PawnState AttachPlayer(int playerId, float when) {
            if (playerId != PlayerId) {
                PlayerId = playerId;
                LastUpdated = when;
                return Notify();
            }
            return this;
        }

        public PawnState RemovePlayer(int playerId, float when) {
            if (playerId == PlayerId) {
                PlayerId = default;
                LastUpdated = when;
                return Notify();
            }
            return this;
        }
    }
}