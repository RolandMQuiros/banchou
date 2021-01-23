using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnHistory : Notifiable<PawnHistory> {
        [Key(0)] public FrameData Front => _frames[_frontIndex];
        [IgnoreMember] public FrameData Back => _frames[(_frontIndex - _frames.Length) % _frames.Length];
        [IgnoreMember] public IReadOnlyList<FrameData> Frames => _frames;
        [SerializeField] private FrameData[] _frames;
        [SerializeField] private int _frontIndex = 0;

        #region Serialization constructors
        public PawnHistory(FrameData front) {
            _frames = new FrameData[] { front };
        }
        #endregion

        public PawnHistory(int frames = 80) {
            _frames = new FrameData[frames];
            for (int i = 0; i < _frames.Length; i++) {
                _frames[i] = new FrameData();
            }
            _frontIndex = 0;
        }

        public PawnHistory Sync(PawnHistory other) {
            var back = (_frontIndex - _frames.Length) % _frames.Length;
            while (Front.When > other.Front.When && _frontIndex != back) {
                _frontIndex = (_frontIndex - 1) % _frames.Length;
            }
            _frames[_frontIndex] = other.Front;
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