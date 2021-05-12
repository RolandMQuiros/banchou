using System.IO;
using LiteNetLib;
using MessagePack;

using Banchou.Network.Message;

namespace Banchou.Network {
    public static class NetworkExtensions {
        private static MemoryStream _outgoingBuffer = new MemoryStream(65536);
        public static void SendPayload<T>(
            this NetPeer peer,
            PayloadType payloadType,
            in T payload,
            DeliveryMethod deliveryMethod,
            MessagePackSerializerOptions serializerOptions
        ) {
            _outgoingBuffer.SetLength(0);
            _outgoingBuffer.WriteByte((byte)payloadType);
            MessagePackSerializer.Serialize<T>(_outgoingBuffer, payload, serializerOptions);
            peer.Send(_outgoingBuffer.GetBuffer(), 0, (int)_outgoingBuffer.Length, deliveryMethod);
        }

        public static void SendPayloadToAll<T>(
            this NetManager netManager,
            PayloadType payloadType,
            in T payload,
            DeliveryMethod deliveryMethod,
            MessagePackSerializerOptions serializerOptions
        ) {
            _outgoingBuffer.SetLength(0);
            _outgoingBuffer.WriteByte((byte)payloadType);
            MessagePackSerializer.Serialize<T>(_outgoingBuffer, payload, serializerOptions);
            netManager.SendToAll(_outgoingBuffer.GetBuffer(), 0, (int)_outgoingBuffer.Length, deliveryMethod);
        }
    }
}