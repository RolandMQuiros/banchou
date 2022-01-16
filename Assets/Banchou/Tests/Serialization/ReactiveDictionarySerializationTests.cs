using System.Collections.Generic;
using System.Linq;
using MessagePack;
using MessagePack.Resolvers;
using NUnit.Framework;
using UniRx;

using Banchou.Serialization.Resolvers;

namespace Banchou.Test {
    public class ReactiveDictionarySerializationTests {
        private MessagePackSerializerOptions _msgPackOptions =  MessagePackSerializerOptions
            .Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(
                CompositeResolver.Create(
                    BanchouResolver.Instance,
                    MessagePack.Unity.UnityResolver.Instance,
                    StandardResolver.Instance
                )
            );

        [Test]
        public void SerializeDeserialize() {
            var reactiveDictionary = new ReactiveDictionary<int, string> {
                [0] = "abc",
                [1] = "def"
            };

            var serialized = MessagePackSerializer.Serialize<IDictionary<int, string>>(reactiveDictionary, _msgPackOptions);
            var deserialized = MessagePackSerializer.Deserialize<ReactiveDictionary<int, string>>(serialized, _msgPackOptions);

            Assert.IsTrue(deserialized.Any());
            Assert.AreEqual(reactiveDictionary[0], deserialized[0]);
            Assert.AreEqual(reactiveDictionary[1], deserialized[1]);
        }
    }
}