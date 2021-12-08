using System;
using MessagePack;
using UnityEngine;

using Banchou.Combatant;

namespace Banchou.Pawn {
    public delegate int GetPawnId();
    [MessagePackObject, Serializable]
    public record PawnState(
        int PawnId, string PrefabKey, int PlayerId = 0, PawnSpatial Spatial = null,
        PawnAnimatorFrame AnimatorFrame = null, float LastUpdated = 0f, CombatantState Combatant = null
    ) : Notifiable<PawnState> {
        [field: SerializeField]
        public int PlayerId { get; private set; } = PlayerId;
        
        [field: SerializeField]
        public PawnSpatial Spatial { get; private set; } = Spatial ?? new PawnSpatial(PawnId);
        
        [field: SerializeField]
        public PawnAnimatorFrame AnimatorFrame { get; private set; } = AnimatorFrame ?? new PawnAnimatorFrame();
        
        [field: SerializeField]
        public float LastUpdated { get; private set; } = LastUpdated;
        
        [field: SerializeField]
        public CombatantState Combatant { get; private set; } = Combatant;

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

        public PawnState SetCombatant(
            CombatantTeam team,
            int maxHealth,
            float when,
            out CombatantState combatant
        ) {
            Combatant = combatant = new CombatantState(new CombatantStats(team, maxHealth), LastUpdated: when);
            LastUpdated = when;
            return Notify();
        }
    }
}