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

namespace Banchou.Serialization.Formatters.Banchou.Pawn
{
    using global::System.Buffers;
    using global::MessagePack;

    public sealed class PawnSpatialFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Banchou.Pawn.PawnSpatial>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Banchou.Pawn.PawnSpatial value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(9);
            writer.Write(value.PawnId);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.Position, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.Forward, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.Up, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.Target, options);
            formatterResolver.GetFormatterWithVerify<global::Banchou.Pawn.PawnSpatial.MovementStyle>().Serialize(ref writer, value.Style, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.AmbientVelocity, options);
            writer.Write(value.IsGrounded);
            writer.Write(value.LastUpdated);
        }

        public global::Banchou.Pawn.PawnSpatial Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __PawnId__ = default(int);
            var __Position__ = default(global::UnityEngine.Vector3);
            var __Forward__ = default(global::UnityEngine.Vector3);
            var __Up__ = default(global::UnityEngine.Vector3);
            var __Target__ = default(global::UnityEngine.Vector3);
            var __Style__ = default(global::Banchou.Pawn.PawnSpatial.MovementStyle);
            var __AmbientVelocity__ = default(global::UnityEngine.Vector3);
            var __IsGrounded__ = default(bool);
            var __LastUpdated__ = default(float);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __PawnId__ = reader.ReadInt32();
                        break;
                    case 1:
                        __Position__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        __Forward__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        __Up__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        __Target__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        __Style__ = formatterResolver.GetFormatterWithVerify<global::Banchou.Pawn.PawnSpatial.MovementStyle>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        __AmbientVelocity__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        __IsGrounded__ = reader.ReadBoolean();
                        break;
                    case 8:
                        __LastUpdated__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::Banchou.Pawn.PawnSpatial(__PawnId__, __Position__, __Forward__, __Up__, __Target__, __Style__, __AmbientVelocity__, __IsGrounded__, __LastUpdated__);
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
