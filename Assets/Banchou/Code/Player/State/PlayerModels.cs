using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using UnityEngine;

namespace Banchou.Player {
    /// <summary>
    /// Represents some entity controlling one or more pawns on the board. Can be a human on a gamepad, a human on
    /// the other side of a network connection, or an AI.
    /// </summary>
    [MessagePackObject, Serializable]
    public record PlayerState(
        int PlayerId,
        string PrefabKey,
        int NetworkId = 0,
        PlayerInputState Input = null
    ) : Notifiable<PlayerState> {
        /// <summary>Unique identifier for this player</summary>
        public int PlayerId { get; init; } = PlayerId;

        /// <summary>Name of the Prefab to spawn when this player is created.</summary>
        /// <remarks>
        ///     In the case of human players, the instantiated Game Object will process player input from Unity and
        ///     apply it to the <see cref="Input"/> property. With AI players, it might perform some evaluation of the
        ///     world state (opponents, their current attacking states, etc) before deciding on the next Input.
        /// </remarks>
        public string PrefabKey { get; init; } = PrefabKey;

        /// <summary>Identifier for the networked instance of the game this player belongs to.</summary>
        /// <remarks>
        ///     Every client connected to a server instance has a unique
        ///     <see cref="Banchou.Network.NetworkState.NetworkId">Network ID</c> assigned by the server, with the
        ///     server itself assigned <c>NetworkId = 0</c>. Multiple players can exist on a single networked instance
        ///     of the game.
        /// </remarks>
        [field: SerializeField] public int NetworkId { get; init; } = NetworkId;

        /// <summary>The player's latest input commands</summary>
        /// <returns></returns>
        [field: SerializeField] public PlayerInputState Input { get; init; } = Input ?? new PlayerInputState(PlayerId);

        /// <summary>Instantiates a new <c>PlayerState</c>.</summary>
        /// <param name="playerId">Unique identifier for this player. See <see cref="PlayerId"/>.</param>
        /// <param name="prefabKey">
        ///     Name of the prefab to spawn when player is created. See <see cref="PrefabKey"/>
        /// </param>
        /// <param name="networkId">
        ///     Identifier for the networked game instance this player belongs to. See
        ///     <see cref="NetworkId"/>
        /// </param>
        public PlayerState(
            int playerId,
            string prefabKey,
            int networkId = 0
        ) : this(playerId, prefabKey, networkId, null) { }

        /// <summary>Synchronizes this <c>PlayerState</c> with another, usually received over the network.</summary>
        /// <param name="other">The other <c>PlayerState</c> to synchronize the current one to.</param>
        /// <returns>This <c>PlayerState</c></returns>
        public PlayerState Sync(PlayerState other) {
            Input.Sync(other.Input);
            return this;
        }
    }

    [MessagePackObject, Serializable]
    public record PlayerInputState(
        int PlayerId = 0,
        InputCommand Commands = InputCommand.None,
        Vector3 Direction = new(),
        long Sequence = 0,
        float When = 0f
    ) : NotifiableWithHistory<PlayerInputState>(32) {
        [field: SerializeField] public int PlayerId { get; private set; } = PlayerId;
        [field: SerializeField] public InputCommand Commands { get; private set; } = Commands;
        [field: SerializeField] public Vector3 Direction { get; private set; } = Direction;

        // Look input is not shared across the network
        [field: SerializeField] public Vector2 Look { get; private set; }
        [field: SerializeField] public long Sequence { get; private set; } = Sequence;
        [field: SerializeField] public float When { get; private set; } = When;

        public override void Set(PlayerInputState other) {
            PlayerId = other.PlayerId;
            Commands = other.Commands;
            Direction = other.Direction;
            Look = other.Look;
            Sequence = other.Sequence;
            When = other.When;
        }

        public PlayerInputState Push(InputCommand commands, Vector3 direction, Vector2 look, long sequence, float when) {
            Commands = commands;
            Direction = direction;
            Look = look;
            Sequence = sequence;
            When = when;

            return Notify();
        }

        public PlayerInputState PushMove(Vector3 direction, long sequence, float when) {
            Direction = direction;
            Sequence = sequence;
            When = when;

            return Notify();
        }

        public PlayerInputState PushLook(Vector2 look, long sequence, float when) {
            Look = look;
            Sequence = sequence;
            When = when;

            return this;
        }

        public PlayerInputState PushCommands(InputCommand commands, long sequence, float when) {
            Commands = commands;
            Sequence = sequence;
            When = when;

            return Notify();
        }

        public PlayerInputState Sync(PlayerInputState sync) {
            Set(sync);
            return Notify();
        }
    }

    [MessagePackObject, Serializable]
    public record PlayersState(Dictionary<int, PlayerState> Members = null) : Notifiable<PlayersState> {
        public event Action<PlayerState> PlayerAdded;
        public event Action<PlayerState> PlayerRemoved;

        [field: SerializeField]
        public Dictionary<int, PlayerState> Members { get; init; } = Members ?? new Dictionary<int, PlayerState>();

        public PlayersState AddPlayer(int playerId, string prefabKey, int networkId = 0) {
            var player = new PlayerState(playerId, prefabKey, networkId);
            Members[playerId] = player;
            PlayerAdded?.Invoke(player);
            return Notify();
        }

        public PlayersState RemovePlayer(int playerId) {
            PlayerState player;
            if (Members.TryGetValue(playerId, out player) && Members.Remove(playerId)) {
                PlayerRemoved?.Invoke(player);
                Notify();
            }
            return this;
        }

        public PlayersState SyncGame(PlayersState sync) {
            var playerIds = Members.Select(p => p.Key).ToList();
            var syncPlayerIds = sync.Members.Select(p => p.Key).ToList();

            foreach (var added in syncPlayerIds.Except(playerIds)) {
                Members[added] = sync.Members[added];
            }

            foreach (var removed in playerIds.Except(syncPlayerIds)) {
                Members.Remove(removed);
            }

            foreach (var playerId in playerIds.Intersect(syncPlayerIds)) {
                Members[playerId].Sync(sync.Members[playerId]);
            }

            return Notify();
        }

        public PlayersState SyncBoard(IEnumerable<PlayerState> incoming) {
            foreach (var incomingPlayer in incoming) {
                PlayerState player;
                if (Members.TryGetValue(incomingPlayer.PlayerId, out player)) {
                    player.Sync(incomingPlayer);
                }
            }
            return this;
        }
    }

    public enum PlayerStickState : byte {
        Neutral = 5,
        Forward = 6,
        ForwardRight = 3,
        Right = 2,
        BackRight = 1,
        Back = 4,
        BackLeft = 7,
        Left = 8,
        ForwardLeft = 9
    }

    public delegate int GetPlayerId();
}