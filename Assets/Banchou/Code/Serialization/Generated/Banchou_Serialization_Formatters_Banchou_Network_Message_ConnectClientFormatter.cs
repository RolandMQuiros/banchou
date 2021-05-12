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

namespace Banchou.Serialization.Formatters.Banchou.Network.Message
{
    using System;
    using System.Buffers;
    using MessagePack;

    public sealed class ConnectClientFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Banchou.Network.Message.ConnectClient>
    {

        public void Serialize(ref MessagePackWriter writer, global::Banchou.Network.Message.ConnectClient value, global::MessagePack.MessagePackSerializerOptions options)
        {
            IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(2);
            formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConnectionKey, options);
            writer.Write(value.ClientConnectionTime);
        }

        public global::Banchou.Network.Message.ConnectClient Deserialize(ref MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __ConnectionKey__ = default(string);
            var __ClientConnectionTime__ = default(float);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __ConnectionKey__ = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        __ClientConnectionTime__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::Banchou.Network.Message.ConnectClient();
            ____result.ConnectionKey = __ConnectionKey__;
            ____result.ClientConnectionTime = __ClientConnectionTime__;
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
