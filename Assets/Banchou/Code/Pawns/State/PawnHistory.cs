using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnHistory : Notifiable<PawnHistory> {
        [Key(0)] public PawnAnimatorFrame Front => _frames[_frontIndex];
        [IgnoreMember] public PawnAnimatorFrame Back => _frames[(_frontIndex - _frames.Length) % _frames.Length];
        [IgnoreMember] public IReadOnlyList<PawnAnimatorFrame> Frames => _frames;
        [SerializeField] private PawnAnimatorFrame[] _frames;
        [SerializeField] private int _frontIndex = 0;

        [SerializationConstructor]
        public PawnHistory(PawnAnimatorFrame front) {
            _frames = new PawnAnimatorFrame[] { front };
        }

        public PawnHistory(int frames = 80) {
            _frames = new PawnAnimatorFrame[frames];
            for (int i = 0; i < _frames.Length; i++) {
                _frames[i] = new PawnAnimatorFrame();
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

        public PawnHistory Push(out PawnAnimatorFrame pushed) {
            _frontIndex = (_frontIndex + 1) % _frames.Length;
            pushed = _frames[_frontIndex];
            
            return this;
        }

        public PawnHistory Push() {
            PawnAnimatorFrame pushed;
            return Push(out pushed);
        }

        public PawnHistory Pop(out PawnAnimatorFrame popped) {
            popped = _frames[_frontIndex];
            _frontIndex = (_frontIndex - 1) % _frames.Length;
            
            return this;
        }

        public PawnHistory Pop() {
            PawnAnimatorFrame popped;
            return Pop(out popped);
        }
    }
}