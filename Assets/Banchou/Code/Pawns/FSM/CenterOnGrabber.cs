using Banchou.Combatant;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class CenterOnGrabber : FSMBehaviour {
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            var spatial = state.GetPawnSpatial(pawnId);
            state.ObserveHitsOn(pawnId)
                .Where(lastHit => lastHit.Style == HitStyle.Grabbed && lastHit.AttackerId != default)
                .Subscribe(lastHit => {
                    var grabberSpatial = state.GetPawnSpatial(lastHit.AttackerId);

                    if (grabberSpatial != null) {
                        spatial.Teleport(grabberSpatial.Position, state.GetTime(), true);
                    }
                })
                .AddTo(this);
        }
    }
}