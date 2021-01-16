using System;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnHistory : Substate<PawnHistory> {
        [IgnoreMember] public FrameData Front => _frames[_frontIndex];
        [IgnoreMember] public FrameData Back => _frames[(_frontIndex - _frames.Length) % _frames.Length];
        [Key(0), SerializeField] private FrameData[] _frames;
        [Key(1), SerializeField] private int _frontIndex;

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

        public PawnHistory Copy(PawnHistory other) {
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