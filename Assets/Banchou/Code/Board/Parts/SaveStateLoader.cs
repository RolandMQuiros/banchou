using System.IO;
using MessagePack;
using UnityEngine;

namespace Banchou {
    public class SaveStateLoader : MonoBehaviour {
        [SerializeField] private string _saveNameFormat = "banchou.dev.{0}.sav";

        private GameState _state;
        private MessagePackSerializerOptions _messagePackOptions;
        
        public void Construct(GameState state, MessagePackSerializerOptions messagePackOptions) {
            _state = state;
            _messagePackOptions = messagePackOptions;
        }

        public void SaveState(int slot) {
            var savedBoard = MessagePackSerializer.Serialize(_state, _messagePackOptions);
            var filename = string.Format(_saveNameFormat, slot);
            File.WriteAllBytes(
                Path.Combine(Application.persistentDataPath, filename),
                savedBoard
            );
        }

        public void LoadState(int slot) {
            var filename = string.Format(_saveNameFormat, slot);
            var path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path)) {
                var saveFile = File.ReadAllBytes(path);
                var savedGame = MessagePackSerializer.Deserialize<GameState>(
                    saveFile,
                    _messagePackOptions
                );
                _state.SyncGame(savedGame);
            }
        }
    }
}