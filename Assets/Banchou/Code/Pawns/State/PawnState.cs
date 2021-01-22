using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    public delegate int GetPawnId();

    [MessagePackObject, Serializable]
    public class PawnState : Notifiable<PawnState> {
        [Key(0), SerializeField] public readonly int PawnId;
        [Key(1), SerializeField] public readonly string PrefabKey;

        [IgnoreMember] public int PlayerId => _playerId;
        [Key(2), SerializeField] private int _playerId;

        [IgnoreMember] public PawnSpatial Spatial => _spatial;
        [Key(3), SerializeField] private PawnSpatial _spatial;

        [IgnoreMember] public PawnHistory History => _history;
        [Key(9), SerializeField] private PawnHistory _history = new PawnHistory();

        [IgnoreMember] public float LastUpdated => _lastUpdated;
        [Key(10), SerializeField] private float _lastUpdated;

        public PawnState() { }
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
            _playerId = playerId;
            _spatial = new PawnSpatial(position, forward ?? Vector3.forward, up ?? Vector3.up, lastUpdated);
            _lastUpdated = lastUpdated;
        }

        public PawnState SyncGame(PawnState sync) {
            _playerId = sync._playerId;
            _spatial.Sync(sync._spatial);
            _history.Sync(sync._history);
            _lastUpdated = sync._lastUpdated;
            Notify();
            return this;
        }

        public PawnState Rollback(float correctionTime) {
            FrameData targetFrame;
            do { _history.Pop(out targetFrame); }
            while (targetFrame.When > correctionTime);

            _spatial.Teleport(targetFrame.Position, targetFrame.Forward, targetFrame.When, true);

            Notify();
            return this;
        }

        public PawnState AttachPlayer(int playerId, float when) {
            if (playerId != _playerId) {
                _playerId = playerId;
                _lastUpdated = when;
                Notify();
            }
            return this;
        }

        public PawnState RemovePlayer(int playerId, float when) {
            if (playerId == _playerId) {
                _playerId = default;
                _lastUpdated = when;
                Notify();
            }
            return this;
        }
    }
}