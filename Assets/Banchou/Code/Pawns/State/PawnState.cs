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
        [IgnoreMember][field: SerializeField] public PawnHistory History { get; private set; }
        [Key(4)][field: SerializeField] public float LastUpdated { get; private set; }

        [SerializationConstructor]
        public PawnState(int pawnId, string prefabKey, int playerId, PawnSpatial spatial, float lastUpdated) {
            PawnId = pawnId;
            PrefabKey = prefabKey;
            PlayerId = playerId;
            Spatial = spatial;
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
            History = new PawnHistory();
            LastUpdated = lastUpdated;
        }

        public PawnState(in PawnState other) {
            PawnId = other.PawnId;
            PrefabKey = other.PrefabKey;
            PlayerId = other.PlayerId;
            Spatial = other.Spatial ?? new PawnSpatial(PawnId);
            History = other.History ?? new PawnHistory();
            LastUpdated = other.LastUpdated;
        }

        public PawnState Sync(PawnState sync) {
            Spatial.Sync(sync.Spatial);
            // History.Sync(sync.History);
            LastUpdated = sync.LastUpdated;

            if (sync.PlayerId != PlayerId) {
                PlayerId = sync.PlayerId;
                Notify();
            }
            return this;
        }

        public PawnState Rollback(float correctionTime) {
            FrameData targetFrame;
            do { History.Pop(out targetFrame); }
            while (targetFrame.When > correctionTime);

            Spatial.Teleport(targetFrame.Position, targetFrame.When, instant: true);
            Spatial.Rotate(targetFrame.Forward, targetFrame.When);

            Notify();
            return this;
        }

        public PawnState AttachPlayer(int playerId, float when) {
            if (playerId != PlayerId) {
                PlayerId = playerId;
                LastUpdated = when;
                Notify();
            }
            return this;
        }

        public PawnState RemovePlayer(int playerId, float when) {
            if (playerId == PlayerId) {
                PlayerId = default;
                LastUpdated = when;
                Notify();
            }
            return this;
        }
    }
}