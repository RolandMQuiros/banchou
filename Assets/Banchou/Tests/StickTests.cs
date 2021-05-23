using UnityEngine;
using NUnit.Framework;

using Banchou.Player;

namespace Banchou.Test {
    public class StickTests {
        [Test]
        public void Vector2ToStick() {
            Assert.AreEqual(Vector2.up.DirectionToStick(),           PlayerStickState.Forward);
            Assert.AreEqual(Vector2.one.DirectionToStick(),          PlayerStickState.ForwardRight);
            Assert.AreEqual(Vector2.right.DirectionToStick(),        PlayerStickState.Right);
            Assert.AreEqual(new Vector2(1f, -1f).DirectionToStick(), PlayerStickState.BackRight);
            Assert.AreEqual(Vector2.down.DirectionToStick(),         PlayerStickState.Back);
            Assert.AreEqual((-Vector2.one).DirectionToStick(),       PlayerStickState.BackLeft);
            Assert.AreEqual(Vector2.left.DirectionToStick(),         PlayerStickState.Left);
            Assert.AreEqual(new Vector2(-1f, 1f).DirectionToStick(), PlayerStickState.ForwardLeft);
        }
    }
}