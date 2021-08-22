using System.IO;

using ExitGames.Client.Photon;
using MessagePack;
using Photon.Pun;
using Photon.Realtime;

using Banchou.Network.Message;

namespace Banchou.Network {
    public static class PayloadExtensions {
        private static readonly MemoryStream _outgoingBuffer = new MemoryStream(short.MaxValue);
        private static readonly MemoryStream _incomingBuffer = new MemoryStream(short.MaxValue);
        private static readonly RaiseEventOptions _eventOptions = new RaiseEventOptions {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.Others
        };
        private static SendOptions _sendOptions = new SendOptions();
        
        public static void SendOverNetwork<T>(
            this T payload,
            PayloadType payloadType,
            bool reliable,
            MessagePackSerializerOptions serializerOptions,
            params int[] targetPlayers
        ) {
            _outgoingBuffer.SetLength(0);
            MessagePackSerializer.Serialize(_outgoingBuffer, payload, serializerOptions);
            _eventOptions.TargetActors = targetPlayers;
            _sendOptions.Reliability = reliable;

            PhotonNetwork.RaiseEvent(
                (byte)payloadType,
                _outgoingBuffer,
                _eventOptions,
                _sendOptions
            );
        }

        public static short SerializeMemoryStream(StreamBuffer outStream, object customObject) {
            var memoryStream = (MemoryStream)customObject;
            memoryStream.Position = 0;
            int read;
            short count = 0;
            
            while ((read = memoryStream.ReadByte()) != -1) {
                outStream.WriteByte((byte)read);
                count++;
            }

            return count;
        }

        public static object DeserializeMemoryStream(StreamBuffer inStream, short length) {
            _incomingBuffer.SetLength(0);
            while (inStream.Available > 0) {
                _incomingBuffer.WriteByte(inStream.ReadByte());
            }
            _incomingBuffer.Position = 0;
            return _incomingBuffer;
        }
    }
}