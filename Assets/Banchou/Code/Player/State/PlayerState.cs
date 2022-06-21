using System;
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
        [Key(0)] public int PlayerId { get; private set; } = PlayerId;

        /// <summary>Name of the Prefab to spawn when this player is created.</summary>
        /// <remarks>
        ///     In the case of human players, the instantiated Game Object will process player input from Unity and
        ///     apply it to the <see cref="Input"/> property. With AI players, it might perform some evaluation of the
        ///     world state (opponents, their current attacking states, etc) before deciding on the next Input.
        /// </remarks>
        [Key(1)] public string PrefabKey { get; private set; } = PrefabKey;

        /// <summary>Identifier for the networked instance of the game this player belongs to.</summary>
        /// <remarks>
        ///     Every client connected to a server instance has a unique
        ///     <see cref="Banchou.Network.NetworkState.NetworkId">Network ID</c> assigned by the server, with the
        ///     server itself assigned <c>NetworkId = 0</c>. Multiple players can exist on a single networked instance
        ///     of the game.
        /// </remarks>
        [Key(2)][field: SerializeField] public int NetworkId { get; private set; } = NetworkId;

        /// <summary>The player's latest input commands</summary>
        /// <returns></returns>
        [Key(3)][field: SerializeField] public PlayerInputState Input { get; private set; } = Input ?? new PlayerInputState(PlayerId);

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

        public override void Dispose() {
            base.Dispose();
            Input.Dispose();
        }

        /// <summary>Synchronizes this <c>PlayerState</c> with another, usually received over the network.</summary>
        /// <param name="other">The other <c>PlayerState</c> to synchronize the current one to.</param>
        /// <returns>This <c>PlayerState</c></returns>
        public PlayerState Sync(PlayerState other) {
            Input.Sync(other.Input);
            return this;
        }
    }

    public delegate int GetPlayerId();
}