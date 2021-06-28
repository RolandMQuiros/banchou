using UnityEngine;

namespace Banchou.Combatant {
    public static class CombatantSelectors {
        public static CombatantBlockDirection DirectionToBlock(this Vector2 vec) {
            var snapped = Snapping.Snap(vec, Vector2.one);
            if      (snapped == Vector2.up          )  return CombatantBlockDirection.Forward;
            else if (snapped == Vector2.one         )  return CombatantBlockDirection.ForwardRight;
            else if (snapped == Vector2.right       )  return CombatantBlockDirection.Right;
            else if (snapped == new Vector2(1f, -1f))  return CombatantBlockDirection.BackRight;
            else if (snapped == Vector2.down        )  return CombatantBlockDirection.Back;
            else if (snapped == -Vector2.one        )  return CombatantBlockDirection.BackLeft;
            else if (snapped == Vector2.left        )  return CombatantBlockDirection.Left;
            else if (snapped == new Vector2(-1f, 1f))  return CombatantBlockDirection.ForwardLeft;
            return CombatantBlockDirection.Neutral;
        }
    }
}