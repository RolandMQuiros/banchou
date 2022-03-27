// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace Banchou.Serialization.Formatters.Banchou.Combatant
{
    using global::System.Buffers;
    using global::MessagePack;

    public sealed class GrabStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Banchou.Combatant.GrabState>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Banchou.Combatant.GrabState value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(9);
            writer.Write(value.AttackerId);
            writer.Write(value.AttackId);
            writer.Write(value.TargetId);
            formatterResolver.GetFormatterWithVerify<global::Banchou.Combatant.GrabPhase>().Serialize(ref writer, value.Phase, options);
            formatterResolver.GetFormatterWithVerify<global::Banchou.Combatant.GrabbedPose>().Serialize(ref writer, value.Pose, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.AnchorPosition, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Serialize(ref writer, value.AnchorRotation, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.LaunchForce, options);
            writer.Write(value.LastUpdated);
        }

        public global::Banchou.Combatant.GrabState Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __AttackerId__ = default(int);
            var __AttackId__ = default(int);
            var __TargetId__ = default(int);
            var __Phase__ = default(global::Banchou.Combatant.GrabPhase);
            var __Pose__ = default(global::Banchou.Combatant.GrabbedPose);
            var __AnchorPosition__ = default(global::UnityEngine.Vector3);
            var __AnchorRotation__ = default(global::UnityEngine.Quaternion);
            var __LaunchForce__ = default(global::UnityEngine.Vector3);
            var __LastUpdated__ = default(float);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __AttackerId__ = reader.ReadInt32();
                        break;
                    case 1:
                        __AttackId__ = reader.ReadInt32();
                        break;
                    case 2:
                        __TargetId__ = reader.ReadInt32();
                        break;
                    case 3:
                        __Phase__ = formatterResolver.GetFormatterWithVerify<global::Banchou.Combatant.GrabPhase>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        __Pose__ = formatterResolver.GetFormatterWithVerify<global::Banchou.Combatant.GrabbedPose>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        __AnchorPosition__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        __AnchorRotation__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        __LaunchForce__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 8:
                        __LastUpdated__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::Banchou.Combatant.GrabState(__AttackerId__, __AttackId__, __TargetId__, __Phase__, __Pose__, __AnchorPosition__, __AnchorRotation__, __LaunchForce__, __LastUpdated__);
            reader.Depth--;
            return ____result;
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
