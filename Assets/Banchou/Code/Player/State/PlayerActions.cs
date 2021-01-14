namespace Banchou.Player {
    namespace StateAction {
        public class AddPlayer {
            public int PlayerId;
            public string PrefabKey;
            public int NetworkId;
            public float When;
        }

        public class RemovePlayer {
            public int PlayerId;
            public float When;
        }

        public class PushInput {
            public InputUnit Unit;
            public float When;
        }
    }

    public class PlayerActions {
        private GetTime _getTime;
        public PlayerActions(GetTime getTime) {
            _getTime = getTime;
        }

        public StateAction.AddPlayer Add(int playerId, string prefabKey, int networkId, float when) => new StateAction.AddPlayer {
            PlayerId = playerId,
            PrefabKey = prefabKey,
            NetworkId = networkId,
            When = when
        };
        public StateAction.AddPlayer Add(int playerId, string prefabKey, int networkId) => Add(playerId, prefabKey, networkId, _getTime());
        public StateAction.AddPlayer Add(int playerId, string prefabKey) => Add(playerId, prefabKey, 0, _getTime());
        public StateAction.RemovePlayer Remove(int playerId, float when) => new StateAction.RemovePlayer { PlayerId = playerId, When = when };
        public StateAction.RemovePlayer Remove(int playerId) => Remove(playerId, _getTime());
        public StateAction.PushInput PushInput(InputUnit unit) => new StateAction.PushInput{ Unit = unit };
    }
}