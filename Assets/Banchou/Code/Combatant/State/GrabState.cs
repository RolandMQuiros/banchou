using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Combatant {
    [MessagePackObject, Serializable]
    public record GrabState(
        int AttackerId,
        int AttackId = default,
        int TargetId = default,
        GrabPhase Phase = GrabPhase.Released,
        GrabbedPose Pose = default,
        Vector3 AnchorPosition = default,
        Quaternion AnchorRotation = default,
        Vector3 LaunchForce = default,
        float LastUpdated = default
    ) : NotifiableWithHistory<GrabState>(32) {
        [Key(0)][field: SerializeField] public int AttackerId { get; private set; } = AttackerId;
        [Key(1)][field: SerializeField] public int AttackId { get; private set; } = AttackId;
        [Key(2)][field: SerializeField] public int TargetId { get; private set; } = TargetId;
        [Key(3)][field: SerializeField] public GrabPhase Phase { get; private set; } = Phase;
        [Key(4)][field: SerializeField] public GrabbedPose Pose { get; private set; } = Pose;
        [Key(5)][field: SerializeField] public Vector3 AnchorPosition { get; private set; } = AnchorPosition;
        [Key(6)][field: SerializeField] public Quaternion AnchorRotation { get; private set; } = AnchorRotation;
        [Key(7)][field: SerializeField] public Vector3 LaunchForce { get; private set; } = LaunchForce;
        [Key(8)][field: SerializeField] public float LastUpdated { get; private set; } = LastUpdated;

        public override void Set(GrabState other) {
            AttackerId = other.AttackerId;
            AttackId = other.AttackId;
            TargetId = other.TargetId;
            Pose = other.Pose;
            AnchorPosition = other.AnchorPosition;
            AnchorRotation = other.AnchorRotation;
            LastUpdated = other.LastUpdated;
        }

        public GrabState Sync(GrabState other) {
            Set(other);
            return Notify();
        }

        public GrabState Contact(int attackId, int targetId, float when) {
            if (Phase is GrabPhase.Released or GrabPhase.Interrupted) {
                Phase = GrabPhase.Contacted;
                AttackId = attackId;
                TargetId = targetId;
                LaunchForce = Vector3.zero;
                return Notify(when);
            }
            return this;
        }

        public GrabState Hold(Vector3 position, Quaternion rotation, GrabbedPose pose, float when) {
            if (Phase is GrabPhase.Contacted or GrabPhase.Held && TargetId != default) {
                Phase = GrabPhase.Held;
                AnchorPosition = position;
                AnchorRotation = rotation;
                Pose = pose;
                return Notify(when);
            }
            return Interrupt(position, rotation, when);
        }

        public GrabState Release(Vector3 position, Quaternion rotation, Vector3 launchForce, float when) {
            if (Phase == GrabPhase.Held) {
                LaunchForce = launchForce;
                AnchorPosition = position;
                AnchorRotation = rotation;
                Phase = GrabPhase.Released;
                Pose = GrabbedPose.None;
                return Notify(when);
            }
            return this;
        }

        public GrabState Interrupt(float when) {
            return Interrupt(AnchorPosition, AnchorRotation, when);
        }
        
        public GrabState Interrupt(Vector3 position, Quaternion rotation, float when) {
            if (Phase is GrabPhase.Contacted or GrabPhase.Held) {
                LaunchForce = Vector3.zero;
                AnchorPosition = position;
                AnchorRotation = rotation;
                Phase = GrabPhase.Interrupted;
                Pose = GrabbedPose.None;
                return Notify(when);
            }
            return this;
        }
    }
    
    public enum GrabbedPose : byte {
        None,
        Lifted,
        Crumpled,
        Flung,
        Splatted
    }

    public enum GrabPhase : byte {
        Contacted,
        Held,
        Released,
        Interrupted
    }
}