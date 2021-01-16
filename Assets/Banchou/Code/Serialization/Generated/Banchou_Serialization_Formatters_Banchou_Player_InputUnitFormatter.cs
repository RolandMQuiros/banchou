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

namespace Banchou.Serialization.Formatters.Banchou.Player
{
    using System;
    using System.Buffers;
    using MessagePack;

    public sealed class InputUnitFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Banchou.Player.InputUnit>
    {


        public void Serialize(ref MessagePackWriter writer, global::Banchou.Player.InputUnit value, global::MessagePack.MessagePackSerializerOptions options)
        {
            IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(6);
            writer.Write(value.PlayerId);
            writer.WriteNil();
            formatterResolver.GetFormatterWithVerify<global::Banchou.Player.InputCommand>().Serialize(ref writer, value.Commands, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.Direction, options);
            writer.Write(value.Sequence);
            writer.Write(value.When);
        }

        public global::Banchou.Player.InputUnit Deserialize(ref MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __PlayerId__ = default(int);
            var __Commands__ = default(global::Banchou.Player.InputCommand);
            var __Direction__ = default(global::UnityEngine.Vector3);
            var __Sequence__ = default(long);
            var __When__ = default(float);

            for (int i = 0; i < length; i++)
            {
                var key = i;

                switch (key)
                {
                    case 0:
                        __PlayerId__ = reader.ReadInt32();
                        break;
                    case 2:
                        __Commands__ = formatterResolver.GetFormatterWithVerify<global::Banchou.Player.InputCommand>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        __Direction__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        __Sequence__ = reader.ReadInt64();
                        break;
                    case 5:
                        __When__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::Banchou.Player.InputUnit();
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