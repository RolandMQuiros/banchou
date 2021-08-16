using Banchou.Network;

namespace Banchou {
    public static class GameStateSelectors {
        /// <summary>
        /// Retrieves the true local time. Should be the same value as <see cref="Time.fixedUnscaledTime"/>
        /// </summary>
        /// <returns>The true local time</returns>
        public static float GetLocalTime(this GameState state) {
            return state.LocalTime;
        }

        /// <summary>
        /// Calculates the current operational time, in seconds
        /// </summary>
        /// <remarks>
        /// If the current session is a local game with no networking, or the server, this should return the same value as <see cref="Time.fixedUnscaledTime"/>.
        /// If the current session is a client connected to a server, this should return the value this same method would give on the server.
        /// If the current session is either a client or server, and a network event triggers a rollback, this method returns the "correction time" of that rollback until
        /// it resimulates to the present.
        /// </remarks>
        /// <param name="state">Reference to the <see cref="GameState"/></param>
        /// <returns>The current operational time</returns>
        public static float GetTime(this GameState state) {
            if (state.GetRollbackPhase() == RollbackPhase.Resimulating) {
                return state.GetCorrectionTime();
            }
            return state.LocalTime + state.Network.HostTimeOffset;
        }

        /// <summary>
        /// Retrieves the current opreational delta time, in seconds
        /// </summary>
        /// <remarks>
        /// If the current session is a client or server, and a network event triggers a rollback, this method returns the "correction delta" in <see cref="RollbackState.DeltaTime"/>.
        /// In all other situations, it should return the same value as <see cref="Time.fixedUnscaledDeltaTime"/>.
        /// </remarks>
        /// <param name="state">Reference to the <see cref="GameState"/></param>
        /// <returns>The current operational time</returns>
        public static float GetDeltaTime(this GameState state) {
            if (state.GetRollbackPhase() == RollbackPhase.Resimulating) {
                return state.GetCorrectionDelta();
            }
            return state.DeltaTime;
        }
    }
}