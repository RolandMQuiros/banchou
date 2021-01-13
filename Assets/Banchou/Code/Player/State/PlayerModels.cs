using System.Collections;
using System.Collections.Generic;
using MessagePack;

namespace Banchou.Player {
    public enum InputSource : byte {
        Local,
        AI,
        Network
    }

    [MessagePackObject]
    public class PlayerState : Substate {
        [Key(0)] public int PlayerId { get; private set; }
        [Key(1)] public int NetworkId { get; private set; }
        [Key(2)] public InputSource Source { get; private set; }
        [Key(3)] public List<InputUnit> Inputs { get; private set; } = new List<InputUnit>();

        public PlayerState(
            int id,
            InputSource source,
            int networkId = 0
        ) {
            PlayerId = id;
            Source = source;
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
        [Key(0)] public Dictionary<int, PlayerState> Players { get; private set; } = new Dictionary<int, PlayerState>();

        protected override bool Consume(IList actions) {
            var consumed = false;
            foreach (var action in actions) {
                if (action is StateAction.AddPlayer add && !Players.ContainsKey(add.PlayerId)) {
                    Players[add.PlayerId] = new PlayerState(
                        id: add.PlayerId,
                        source: add.Source,
                        networkId: add.NetworkId
                    );
                    consumed = true;
                }

                if (action is StateAction.RemovePlayer remove) {
                    Players.Remove(remove.PlayerId);
                    consumed = true;
                }
            }
            foreach (var player in Players.Values) player.Process(actions);
            return consumed;
        }
    }

    public delegate int GetPlayerId();
}