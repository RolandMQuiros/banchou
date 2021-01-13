namespace Banchou.Player.StateAction {
    public struct AddPlayer {
        public int PlayerId;
        public InputSource Source;
        public int NetworkId;
    }

    public struct RemovePlayer {
        public int PlayerId;
    }

    public struct PushInput {
        public InputUnit Unit;
    }
}