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
    using System;
    using System.Buffers;
    using MessagePack;

    public sealed class PawnStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Banchou.Pawn.PawnState>
    {


        public void Serialize(ref MessagePackWriter writer, global::Banchou.Pawn.PawnState value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(11);
            writer.Write(value.PawnId);
            formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PrefabKey, options);
            writer.Write(value._playerId);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value._position, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value._forward, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value._up, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value._velocity, options);
            writer.Write(value._isContinuous);
            writer.Write(value._isGrounded);
            formatterResolver.GetFormatterWithVerify<global::Banchou.Pawn.PawnHistory>().Serialize(ref writer, value._history, options);
            writer.Write(value._lastUpdated);
        }

        public global::Banchou.Pawn.PawnState Deserialize(ref MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ___playerId__ = default(int);
            var __PawnId__ = default(int);
            var __PrefabKey__ = default(string);
            var ___position__ = default(global::UnityEngine.Vector3);
            var ___forward__ = default(global::UnityEngine.Vector3);
            var ___up__ = default(global::UnityEngine.Vector3);
            var ___velocity__ = default(global::UnityEngine.Vector3);
            var ___isContinuous__ = default(bool);
            var ___isGrounded__ = default(bool);
            var ___history__ = default(global::Banchou.Pawn.PawnHistory);
            var ___lastUpdated__ = default(float);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 2:
                        ___playerId__ = reader.ReadInt32();
                        break;
                    case 0:
                        __PawnId__ = reader.ReadInt32();
                        break;
                    case 1:
                        __PrefabKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        ___position__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        ___forward__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        ___up__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 6:
                        ___velocity__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 7:
                        ___isContinuous__ = reader.ReadBoolean();
                        break;
                    case 8:
                        ___isGrounded__ = reader.ReadBoolean();
                        break;
                    case 9:
                        ___history__ = formatterResolver.GetFormatterWithVerify<global::Banchou.Pawn.PawnHistory>().Deserialize(ref reader, options);
                        break;
                    case 10:
                        ___lastUpdated__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::Banchou.Pawn.PawnState(__PawnId__, __PrefabKey__, ___playerId__, ___position__, ___forward__, ___up__, ___velocity__, ___isContinuous__, ___isGrounded__, ___history__, ___lastUpdated__);
            ____result._position = ___position__;
            ____result._forward = ___forward__;
            ____result._up = ___up__;
            ____result._velocity = ___velocity__;
            ____result._isContinuous = ___isContinuous__;
            ____result._isGrounded = ___isGrounded__;
            ____result._history = ___history__;
            ____result._lastUpdated = ___lastUpdated__;
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