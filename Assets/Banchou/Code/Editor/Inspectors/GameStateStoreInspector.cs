using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;
using UnityEditor;

using Banchou.Serialization.Resolvers;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Banchou.Tools {
    [CustomEditor(typeof(GameStateStore))]
    public class GameStateStoreInspector : Editor {
        private GameStateStore _store;
        private Stopwatch _stopwatch = new Stopwatch();

        private MessagePackSerializerOptions _messagePackOptions = MessagePackSerializerOptions
            .Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(CompositeResolver.Create(
                BanchouResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance,
                StandardResolver.Instance
            ));

        private void OnEnable() {
            _store = (GameStateStore)target;
        }

        public override void OnInspectorGUI() {
            if (Application.isPlaying) {
                var state = _store.State;

                _stopwatch.Restart();
                var bytes = MessagePackSerializer.Serialize(state, _messagePackOptions);
                _stopwatch.Stop();
                var serializationTime = _stopwatch.ElapsedTicks;

                _stopwatch.Restart();
                var json = MessagePackSerializer.ConvertToJson(bytes, _messagePackOptions);
                _stopwatch.Stop();
                var jsonTime = _stopwatch.ElapsedTicks;

                EditorGUILayout.TextArea(json);
                EditorGUILayout.LabelField($"Serialized to binary {bytes.Length} bytes in {serializationTime} ns");
                EditorGUILayout.LabelField($"Serialized to json {json.Length} bytes in {jsonTime} ns");
            }
        }
    }
}