using System.Collections.Generic;
using MessagePack;
using UnityEngine;

using Banchou.Pawn;
using Banchou.Player;

namespace Banchou.Network.Message {
    public enum PayloadType {
        ConnectClient,
        Connected,
        TimeRequest,
        TimeResponse,
        InputUnit,
        SyncGame,
        SyncBoard
    }

    [MessagePackObject]
    public struct Envelope {
        [Key(0)] public PayloadType PayloadType;
        [Key(1)] public byte[] Payload;

        public static byte[] CreateMessage<T>(PayloadType payloadType, T payload, MessagePackSerializerOptions options) {
            var message = MessagePackSerializer.Serialize(
                new Envelope {
                    PayloadType = payloadType,
                    Payload = MessagePackSerializer.Serialize(payload, options)
                },
                options
            );
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

    [MessagePackObject]
    public struct InputUnit {
        [Key(0)] public int PlayerId;
        [Key(2)] public InputCommand Commands;
        [Key(3)] public Vector3 Move;
        [Key(4)] public long Sequence;
        [Key(5)] public float When;
    }

    [MessagePackObject]
    public struct SyncBoard {
        [Key(0)] public List<PawnState> Pawns;
        [Key(1)] public List<PlayerState> Players;
    }
}