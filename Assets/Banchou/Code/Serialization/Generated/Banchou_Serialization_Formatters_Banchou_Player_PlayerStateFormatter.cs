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
    using global::System.Buffers;
    using global::MessagePack;

    public sealed class PlayerStateFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Banchou.Player.PlayerState>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Banchou.Player.PlayerState value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(4);
            writer.Write(value.PlayerId);
            formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PrefabKey, options);
            writer.Write(value.NetworkId);
            formatterResolver.GetFormatterWithVerify<global::Banchou.Player.PlayerInputState>().Serialize(ref writer, value.Input, options);
        }

        public global::Banchou.Player.PlayerState Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __PlayerId__ = default(int);
            var __PrefabKey__ = default(string);
            var __NetworkId__ = default(int);
            var __Input__ = default(global::Banchou.Player.PlayerInputState);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __PlayerId__ = reader.ReadInt32();
                        break;
                    case 1:
                        __PrefabKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        __NetworkId__ = reader.ReadInt32();
                        break;
                    case 3:
                        __Input__ = formatterResolver.GetFormatterWithVerify<global::Banchou.Player.PlayerInputState>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::Banchou.Player.PlayerState(__PlayerId__, __PrefabKey__, __NetworkId__, __Input__);
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
