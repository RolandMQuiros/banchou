using NUnit.Framework;
using UniRx;
using UnityEngine;

using Banchou.Board;
using Banchou.Pawn;
using Banchou.Player;

namespace Banchou.Test {
    public class PlayerInputEventTests {
        [Test]
        public void SingleChangeSingleEmit() {
            var state = new GameState()
                .AddPlayer(1, "Local Player")
                .AddPlayer(2, "Local Player")
                .AddPlayer(3, "Local Player")
                .AddPlayer(4, "Local Player")
                .AddPlayer(5, "Local Player")
                .AddPawn(1, "Pawns/Isaac", 1);

            var emitCount = 0;
            state.ObservePawnInputCommands(1)
                .Where(args => args.Command != InputCommand.None)
                .Subscribe(command => {
                    Debug.Log(command);
                    Assert.AreEqual(1, ++emitCount);
                });

            state.GetPlayer(1).Input.PushCommands(
                InputCommand.Jump,
                1,
                state.GetTime()
            );
        }
    }
}