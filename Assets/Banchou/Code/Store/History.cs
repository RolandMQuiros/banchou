using System;
using System.Collections.Generic;
using System.Linq;

namespace Banchou {
    /// <summary>
    /// An object whose member properties and fields can be overwritten by those of an object with the same type. Used
    /// to keep track of an object's state over multiple frames, for use in rollbacks.
    /// </summary>
    /// <typeparam name="TRecordable">The derived class</typeparam>
    public interface IRecordable<in TRecordable> where TRecordable : IRecordable<TRecordable> {
        /// <summary>
        /// Sets the current object's values to match another object's.
        /// </summary>
        /// <param name="other">The object to copy</param>
        void Set(TRecordable other);
    }

    /// <summary>
    /// Contains snapshots of <see cref="IRecordable{T}"/> property fields. Used for rollback.
    /// </summary>
    /// <typeparam name="TRecordable">The object type of the object being recorded</typeparam>
    public class History<TRecordable> where TRecordable : IRecordable<TRecordable> {
        [Serializable]
        private class Frame {
            public float When = -1;
            public TRecordable Data;
        }

        private List<Frame> _frames;
        private readonly int _bufferSize;
        private int _front;
        private int _count;

        private Frame FrameAt(int index) => _frames[index % _frames.Count];

        public History(int bufferSize) {
            _bufferSize = bufferSize;
            _frames = new List<Frame>(_bufferSize);
        }

        /// <summary>
        /// Adds a new snapshot to the history. Assumes all pushes happen in chronological order.
        /// </summary>
        /// <param name="frame">The frame to push</param>
        /// <param name="when">The timestamp of the push</param>
        /// <returns>This <see cref="History{T}"/> object</returns>
        public History<TRecordable> PushFrame(TRecordable frame, float when) {
            if (_bufferSize > 0) {
                // Increment counters
                _front = (_front + 1) % Math.Min(_frames.Count + 1, _bufferSize);
                
                // Set the frame's data
                if (_frames.Count < _bufferSize) {
                    _frames.Add(
                        new Frame {
                            When = when,
                            Data = frame
                        }
                    );
                } else {
                    var frameAt = FrameAt(_front);
                    frameAt.When = when;
                    frameAt.Data.Set(frame);
                }
            }
            return this;
        }

        /// <summary>
        /// Rewinds the history to the frame just before the given timestamp.
        /// </summary>
        /// <param name="when">The timestamp to rewind to</param>
        /// <param name="frame">The frame just before the requested timestamp</param>
        /// <returns>This <see cref="History{T}"/> object</returns>
        public History<TRecordable> Rewind(float when, out TRecordable frame) {
            // Find the first frame with the timestamp just before the provided one
            frame = default;
            if (_bufferSize > 0) {
                Frame foundFrame = null;
                int frameOffset;
                for (frameOffset = 1; frameOffset < _count; frameOffset++) {
                    var currentFrame = FrameAt(_front - frameOffset);
                    if (currentFrame.When < when) {
                        foundFrame = FrameAt(_front - frameOffset - 1);
                        break;
                    }
                }
                if (foundFrame != null) {
                    frame = foundFrame.Data;
                    _front -= (frameOffset - 1) % _frames.Count;
                }
            }
            return this;
        }
    }
}