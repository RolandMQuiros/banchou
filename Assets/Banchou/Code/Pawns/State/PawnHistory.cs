using System;
using System.Collections.Generic;

namespace Banchou.Pawn {
    [Serializable]
    public class PawnHistory : Notifiable<PawnHistory> {
        public FrameData Front => _frames[_frontIndex];
        public FrameData Back => _frames[(_frontIndex - _frames.Length) % _frames.Length];
        public IReadOnlyList<FrameData> Frames => _frames;
        private FrameData[] _frames;
        private int _frontIndex = 0;

        public PawnHistory() {
            _frames = new FrameData[7];
            for (int i = 0; i < _frames.Length; i++) {
                _frames[i] = new FrameData();
            }
            _frontIndex = 0;
        }

        public PawnHistory(int frames) {
            _frames = new FrameData[frames];
            for (int i = 0; i < _frames.Length; i++) {
                _frames[i] = new FrameData();
            }
            _frontIndex = 0;
        }

        public PawnHistory Sync(PawnHistory other) {
            _frames = other._frames;
            _frontIndex = other._frontIndex;

            Notify();
            return this;
        }

        public PawnHistory Push(out FrameData pushed) {
            _frontIndex = (_frontIndex + 1) % _frames.Length;
            pushed = _frames[_frontIndex];

            Notify();
            return this;
        }

        public PawnHistory Push() {
            FrameData pushed;
            return Push(out pushed);
        }

        public PawnHistory Pop(out FrameData popped) {
            popped = _frames[_frontIndex];
            _frontIndex = (_frontIndex - 1) % _frames.Length;

            Notify();
            return this;
        }

        public PawnHistory Pop() {
            FrameData popped;
            return Pop(out popped);
        }
    }
}