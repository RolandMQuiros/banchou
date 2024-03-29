using System;
using System.IO;
using NUnit.Framework;

using MessagePack;
using MessagePack.Resolvers;

using UnityEngine;

using Banchou.Board;
using Banchou.Network.Message;
using Banchou.Pawn;
using Banchou.Player;
using Banchou.Serialization.Resolvers;

using Random = UnityEngine.Random;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Banchou.Test {
    public class GameStateSerializationTests {
        private MessagePackSerializerOptions _messagePackOptions = MessagePackSerializerOptions
            .Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(
                CompositeResolver.Create(
                    BanchouResolver.Instance,
                    MessagePack.Unity.UnityResolver.Instance,
                    StandardResolver.Instance
                )
            );

        // A Test behaves as an ordinary method
        [Test]
        public void GameStateSerialDeserialization() {
            var state = new GameState()
                .LoadScene("Sandbox")
                .AddPlayer(1, "Local Player")
                .AddPawn(
                    pawnId: 1,
                    prefabKey: "Erho",
                    playerId: 1,
                    position: new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f))
                )
                .UpdateLocalTime(0.01666666f);

            var perf = Stopwatch.StartNew();
            var bytes = MessagePackSerializer.Serialize(state, _messagePackOptions);
            perf.Stop();

            Debug.Log($"Serialized state to {bytes.Length} bytes in {perf.ElapsedTicks} nanoseconds");

            var json = MessagePackSerializer.SerializeToJson(state, _messagePackOptions);
            var serJson = MessagePackSerializer.ConvertToJson(bytes, _messagePackOptions);
            Debug.Log($"State JSON: {json}");
            Debug.Log($"Serialized Bytes to JSON: {serJson}");

            perf.Restart();
            var deserialized = MessagePackSerializer.Deserialize<GameState>(bytes, _messagePackOptions);
            perf.Stop();

            Debug.Log($"Deserialized state in {perf.ElapsedTicks} nanoseconds");

            Assert.AreEqual(state.GetLocalTime(), deserialized.GetLocalTime());
            Assert.AreEqual(state.GetPawns().Count, deserialized.GetPawns().Count);
            Assert.AreEqual(state.GetPawn(1)?.PrefabKey, deserialized.GetPawn(1)?.PrefabKey);
        }

        [Test]
        public void PlayerInputStatesSerialization() {
            var state = new PlayerInputState(987, InputCommand.LightAttack, Vector3.up, 12345f);
            var bytes = MessagePackSerializer.Serialize(state, _messagePackOptions);
            var deser = MessagePackSerializer.Deserialize<PlayerInputState>(bytes, _messagePackOptions);

            Assert.AreEqual(state.Commands, deser.Commands);
            Assert.AreEqual(state.Direction, deser.Direction);
            Assert.AreEqual(state.When, deser.When);
        }

        [Test]
        public void PlayerStateSerialization() {
            var state = new PlayerState(
                12345,
                "ASDF",
                4321,
                new PlayerInputState(12345, InputCommand.LightAttack, Vector3.up, 12345f)
            );
            var bytes = MessagePackSerializer.Serialize(state, _messagePackOptions);
            var deser = MessagePackSerializer.Deserialize<PlayerState>(bytes, _messagePackOptions);

            Assert.AreEqual(state.PlayerId, deser.PlayerId);
            Assert.AreEqual(state.PrefabKey, deser.PrefabKey);
            Assert.AreEqual(state.NetworkId, deser.NetworkId);

            Assert.AreEqual(state.Input.Commands, deser.Input.Commands);
            Assert.AreEqual(state.Input.Direction, deser.Input.Direction);
            Assert.AreEqual(state.Input.When, deser.Input.When);
        }

        [Test]
        public void MemoryStreamSerialization() {
            var connectMessage = new ConnectClient {
                ConnectionKey = "asdf",
                ClientConnectionTime = 1234f
            };
            var serializationBuffer = new MemoryStream(65536);

            serializationBuffer.WriteByte((byte)PayloadType.ConnectClient);
            MessagePackSerializer.Serialize<ConnectClient>(
                serializationBuffer,
                connectMessage,
                _messagePackOptions
            );

            var payload = new ReadOnlyMemory<byte>(serializationBuffer.GetBuffer(), 1, (int)serializationBuffer.Length - 1);
            var deserialized = MessagePackSerializer.Deserialize<ConnectClient>(payload, _messagePackOptions);

            Assert.AreEqual(PayloadType.ConnectClient, (PayloadType)serializationBuffer.GetBuffer()[0]);
            Assert.AreEqual(connectMessage, deserialized);
        }
    }
}