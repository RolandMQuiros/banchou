using System.Collections;
using System.Collections.Generic;
using MessagePack;

namespace Banchou.Player {
    [MessagePackObject]
    public class PlayerState : Substate {
        [Key(0)] public int PlayerId { get; private set; }
        [Key(1)] public string PrefabKey { get; private set; }
        [Key(2)] public int NetworkId { get; private set; }
        [Key(3)] public List<InputUnit> Inputs { get; private set; } = new List<InputUnit>();


        public PlayerState() { }

        public PlayerState(
            int id,
            string prefabKey,
            int networkId = 0
        ) {
            PlayerId = id;
            PrefabKey = prefabKey;
            NetworkId = networkId;
        }

        protected override bool Consume(IList actions) {
            var consumed = false;
            foreach (var action in actions) {
                if (action is Banchou.StateAction.StartProcess && Inputs.Count > 0) {
                    Inputs.Clear();
                    consumed = true;
                }

                if (action is StateAction.PushInput push && push.Unit.PlayerId == PlayerId) {
                    Inputs.Add(push.Unit);
                    consumed = true;
                }
            }
            return consumed;
        }
    }

    [MessagePackObject]
    public class PlayersState : Substate {
        [Key(0)] public Dictionary<int, PlayerState> Members { get; private set; } = new Dictionary<int, PlayerState>();

        protected override bool Consume(IList actions) {
            var consumed = false;
            foreach (var action in actions) {
                if (action is StateAction.AddPlayer add && !Members.ContainsKey(add.PlayerId)) {
                    Members[add.PlayerId] = new PlayerState(
                        id: add.PlayerId,
                        prefabKey: add.PrefabKey,
                        networkId: add.NetworkId
                    );
                    consumed = true;
                }

                if (action is StateAction.RemovePlayer remove) {
                    Members.Remove(remove.PlayerId);
                    consumed = true;
                }
            }
            foreach (var player in Members.Values) player.Process(actions);
            return consumed;
        }
    }

    public delegate int GetPlayerId();
}