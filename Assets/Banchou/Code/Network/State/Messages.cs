using System.Diagnostics;
using MessagePack;
using UnityEngine;

using Banchou.Player;

namespace Banchou.Network.Message {
    public enum PayloadType {
        ConnectClient,
        Connected,
        TimeRequest,
        TimeResponse,
        InputUnit,
        Sync
    }

    [MessagePackObject]
    public struct Envelope {
        [Key(0)] public PayloadType PayloadType;
        [Key(1)] public byte[] Payload;

#if DEBUG
        private static Stopwatch _serializationPerformance = new Stopwatch();
#endif

        public static byte[] CreateMessage<T>(PayloadType payloadType, T payload, MessagePackSerializerOptions options) {
#if DEBUG
            _serializationPerformance.Restart();
#endif
            var message = MessagePackSerializer.Serialize(
                new Envelope {
                    PayloadType = payloadType,
                    Payload = MessagePackSerializer.Serialize(payload, options)
                },
                options
            );
#if DEBUG
            _serializationPerformance.Stop();
            UnityEngine.Debug.Log($"Serialized {typeof(T).FullName} to {message.Length} bytes in {_serializationPerformance.ElapsedTicks} nanoseconds");
#endif
            return message;
        }
    }

    [MessagePackObject]
    public struct ConnectClient {
        [Key(0)] public string ConnectionKey;
        [Key(1)] public float ClientConnectionTime;
        [IgnoreMember] public float ServerReceiptTime;
    }

    [MessagePackObject]
    public struct Connected {
        [Key(0)] public int ClientNetworkId;
        [Key(1)] public GameState State;
        [Key(2)] public float ClientTime;
        [Key(3)] public float ServerReceiptTime;
        [Key(4)] public float ServerTransmissionTime;
    }

    // https://gamedev.stackexchange.com/questions/93477/how-to-keep-server-client-clocks-in-sync-for-precision-networked-games-like-quak
    [MessagePackObject]
    public struct TimeRequest {
        [Key(0)] public float ClientTime;
    }

    [MessagePackObject]
    public struct TimeResponse {
        [Key(0)] public float ClientTime;
        [Key(1)] public float ServerTime;
    }
}