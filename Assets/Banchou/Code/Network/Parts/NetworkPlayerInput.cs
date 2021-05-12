using LiteNetLib;
using MessagePack;
using UniRx;
using UnityEngine;

using Banchou.Network.Message;
using Banchou.Player;

namespace Banchou.Network.Part {
    public class NetworkPlayerInput : MonoBehaviour
    {
        public void Construct(
            GameState state,
            NetManager netManager,
            MessagePackSerializerOptions messagePackOptions
        ) {
            state.ObserveLocalPlayerInput()
                .CatchIgnoreLog()
                .Where(_ => netManager.ConnectedPeersCount > 0)
                .Subscribe(input => {
                    netManager.SendPayloadToAll(
                        PayloadType.PlayerInput,
                        input,
                        DeliveryMethod.ReliableOrdered,
                        messagePackOptions
                    );
                })
                .AddTo(this);
        }
    }
}