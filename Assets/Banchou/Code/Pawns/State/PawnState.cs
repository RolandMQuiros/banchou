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
        [Key(4)][field: SerializeField] public PawnHistory History { get; private set; }
        [Key(5)][field: SerializeField] public float LastUpdated { get; private set; }

        #region Serialization constructors
        public PawnState(int pawnId, string prefabKey, int playerId, PawnSpatial spatial, PawnHistory history, float lastUpdated) {
            PawnId = pawnId;
            PrefabKey = prefabKey;
            Spatial = spatial;
            History = history;
            LastUpdated = lastUpdated;
        }
        #endregion

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
            Spatial = new PawnSpatial(position, forward ?? Vector3.forward, up ?? Vector3.up, lastUpdated);
            History = new PawnHistory();
            LastUpdated = lastUpdated;
        }

        public PawnState Sync(PawnState sync) {
            PlayerId = sync.PlayerId;
            Spatial.Sync(sync.Spatial);
            History.Sync(sync.History);
            LastUpdated = sync.LastUpdated;
            Notify();
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