using System;
using System.Diagnostics.CodeAnalysis;
using MessagePack;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn {
    public delegate int GetPawnId();

    [MessagePackObject, Serializable]
    public record PawnState(
        [property: Key(0)] int PawnId,
        [property: Key(1)] string PrefabKey,
        int PlayerId = 0,
        PawnSpatial Spatial = null,
        PawnAnimatorFrame AnimatorFrame = null,
        CombatantState Combatant = null,
        float TimeScale = 1f,
        float LastUpdated = 0f
    ) : Notifiable<PawnState> {
        [Key(2)][field: SerializeField]
        public int PlayerId { get; private set; } = PlayerId;

        [Key(3)][field: SerializeField, NotNull]
        public PawnSpatial Spatial { get; private set; } = Spatial ?? new PawnSpatial(PawnId);

        [Key(4)][field: SerializeField]
        public PawnAnimatorFrame AnimatorFrame { get; private set; } = AnimatorFrame;

        [Key(5)][field: SerializeField]
        public CombatantState Combatant { get; private set; } = Combatant;

        [Key(6)][field: SerializeField]
        public float TimeScale { get; private set; } = TimeScale;

        [Key(7)][field: SerializeField]
        public float LastUpdated { get; private set; } = LastUpdated;

        public PawnState(
            int pawnId,
            string prefabKey,
            int playerId = 0,
            Vector3 position = default,
            Vector3? forward = null,
            Vector3? up = null,
            float lastUpdated = 0f
        ) : this(
            pawnId,
            prefabKey,
            playerId,
            new PawnSpatial(pawnId, position, forward ?? Vector3.forward, up ?? Vector3.up, lastUpdated),
            LastUpdated: lastUpdated
        ) { }

        public override void Dispose() {
            base.Dispose();
            Spatial.Dispose();
            AnimatorFrame.Dispose();
            Combatant.Dispose();
        }

        public PawnState Sync(PawnState sync) {
            if (PrefabKey == sync.PrefabKey) {
                if (sync.Spatial != null) {
                    Spatial.Sync(sync.Spatial);
                }

                if (sync.AnimatorFrame != null) {
                    AnimatorFrame?.Sync(sync.AnimatorFrame);
                }

                if (sync.Combatant != null) {
                    Combatant = Combatant?.Sync(sync.Combatant) ?? sync.Combatant;
                }

                LastUpdated = sync.LastUpdated;
                if (sync.PlayerId != PlayerId) {
                    PlayerId = sync.PlayerId;
                    return Notify();
                }
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

        public PawnState SetCombatant(
            CombatantTeam team,
            int maxHealth,
            float when,
            out CombatantState combatant
        ) {
            Combatant = combatant = new CombatantState(
                PawnId, maxHealth, new CombatantStats(team, maxHealth), LastUpdated: when
            );
            LastUpdated = when;
            return Notify();
        }

        public PawnState AttachAnimator(float when, out PawnAnimatorFrame frame) {
            if (AnimatorFrame == null) {
                frame = AnimatorFrame = new PawnAnimatorFrame();
                LastUpdated = when;
                return Notify();
            }
            frame = AnimatorFrame;
            return this;
        }

        public PawnState SetTimeScale(float timeScale, float when) {
            if (!Mathf.Approximately(TimeScale, timeScale)) {
                TimeScale = timeScale;
                LastUpdated = when;
                return Notify();
            }
            return this;
        }
    }
}