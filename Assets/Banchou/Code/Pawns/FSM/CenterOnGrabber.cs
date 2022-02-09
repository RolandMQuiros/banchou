using Banchou.Combatant;
using UniRx;

namespace Banchou.Pawn.FSM {
    public class CenterOnGrabber : FSMBehaviour {
        public void Construct(GameState state, GetPawnId getPawnId) {
            var pawnId = getPawnId();
            
            state.ObserveLastHitChanges(pawnId)
                .WithLatestFrom(state.ObservePawnSpatial(pawnId), (lastHit, spatial) => (lastHit, spatial))
                .Where(args => args.lastHit.IsGrabbed && args.lastHit.AttackerId != default)
                .Subscribe(args => {
                    var (lastHit, spatial) = args;
                    var grabberSpatial = state.GetPawnSpatial(lastHit.AttackerId);

                    if (grabberSpatial != null) {
                        spatial.Teleport(grabberSpatial.Position, state.GetTime(), true);
                    }
                })
                .AddTo(this);
        }
    }
}