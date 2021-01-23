using NUnit.Framework;

using MessagePack;
using MessagePack.Resolvers;

using UnityEngine;
using UnityEngine.TestTools;

using Banchou.Board;
using Banchou.Network;
using Banchou.Player;
using Banchou.Serialization.Resolvers;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Banchou.Test {
    public class GameStateSerializationTests {
        // A Test behaves as an ordinary method
        [Test]
        public void GameStateSerializationTestsSimplePasses() {
            var state = new GameState()
                .LoadScene("Sandbox")
                .AddPlayer(1, "Local Player")
                .AddPawn(
                    pawnId: 1,
                    prefabKey: "Erho",
                    playerId: 1,
                    position: new Vector3(Random.Range(-10f, 10f), 2f, Random.Range(-10f, 10f))
                )
                .SetLocalTime(12345f, 0.01666666f);

            var options = MessagePackSerializerOptions
                .Standard
                .WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(
                    CompositeResolver.Create(
                        BanchouResolver.Instance,
                        MessagePack.Unity.UnityResolver.Instance,
                        StandardResolver.Instance
                    )
                );

            var perf = Stopwatch.StartNew();
            var bytes = MessagePackSerializer.Serialize(state, options);
            perf.Stop();

            Debug.Log($"Serialized state to {bytes.Length} bytes in {perf.ElapsedTicks} nanoseconds");

            var json = MessagePackSerializer.SerializeToJson(state, options);
            var serJson = MessagePackSerializer.ConvertToJson(bytes, options);
            Debug.Log($"State JSON: {json}");
            Debug.Log($"Serialized Bytes to JSON: {serJson}");

            perf.Restart();
            var deserialized = MessagePackSerializer.Deserialize<GameState>(bytes, options);
            perf.Stop();

            Debug.Log($"Deserialized state in {perf.ElapsedTicks} nanoseconds");

            Assert.AreEqual(state.GetLocalTime(), deserialized.GetLocalTime());
            Assert.AreEqual(state.GetPawns().Count, deserialized.GetPawns().Count);
            Assert.AreEqual(state.GetPawn(1)?.PrefabKey, deserialized.GetPawn(1)?.PrefabKey);
        }
    }
}