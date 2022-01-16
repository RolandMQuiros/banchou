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
            var state = new GameState();

            var emitCount = 0;
            for (int i = 1; i <= 10; i++) {
                state.AddPlayer(i, "Local Player")
                    .AddPawn(i, "Pawns/Isaac", i)
                    .ObservePawnInput(i)
                    .Where(input => input.Commands != InputCommand.None)
                    .Subscribe(input => {
                        Debug.Log(input);
                        emitCount++;
                    });
            }

            state.GetPlayer(3).Input.PushCommands(InputCommand.LockOn, state.GetTime());
            Assert.AreEqual(1, emitCount);
        }
    }
}