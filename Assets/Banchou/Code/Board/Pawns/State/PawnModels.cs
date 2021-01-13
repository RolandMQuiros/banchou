using System.Collections;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    public delegate int GetPawnId();

    [MessagePackObject]
    public class PawnState : Substate {
        [Key(0)] public int PawnId { get; private set; }
        [Key(1)] public int PlayerId { get; private set; }
        [Key(2)] public string PrefabKey { get; private set; }
        [Key(3)] public Vector3 Position { get; private set; }
        [Key(4)] public Vector3 Forward { get; private set; }
        [Key(5)] public Vector3 Up { get; private set; }
        [Key(6)] public Vector3 Velocity { get; private set; }
        [Key(7)] public bool IsContinuous { get; private set; }
        [Key(8)] public float LastUpdated { get; private set; }

        public PawnState(
            int pawnId,
            int playerId,
            string prefabKey,
            Vector3 position,
            Vector3? forward = null,
            Vector3? up = null,
            Vector3? velocity = null,
            bool isContinuous = false,
            float lastUpdated = 0f
        ) {
            PawnId = pawnId;
            PlayerId = playerId;
            PrefabKey = prefabKey;
            Position = position;
            Forward = forward ?? Vector3.forward;
            Up = up ?? Vector3.up;
            Velocity = velocity ?? Vector3.zero;
            IsContinuous = isContinuous;
            LastUpdated = lastUpdated;
        }

        protected override bool Consume(IList actions) {
            var consumed = false;

            foreach (var action in actions) {
                if (action is StateAction.MovePawn move && move.PawnId == PawnId) {
                    Velocity = move.Direction;
                    IsContinuous = true;
                    LastUpdated = move.When;
                    consumed = true;
                }

                if (action is StateAction.PawnMoved moved && moved.PawnId == PawnId) {
                    Position = moved.Position;
                    LastUpdated = moved.When;
                    consumed = true;
                }

                if (action is StateAction.TeleportPawn teleport && teleport.PawnId == PawnId) {
                    Position = teleport.Position;
                    Velocity = teleport.CancelMomentum ? Vector3.zero : Velocity;
                    IsContinuous = false;
                    LastUpdated = teleport.When;
                    consumed = true;
                }
            }

            return consumed;
        }
    }
}