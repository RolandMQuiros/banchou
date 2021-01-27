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
                    var message = Envelope.CreateMessage(PayloadType.PlayerInput, input, messagePackOptions);
                    netManager.SendToAll(message, DeliveryMethod.ReliableOrdered);
                })
                .AddTo(this);
        }
    }
}