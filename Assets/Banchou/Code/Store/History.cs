using System;
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

        private readonly int _bufferSize;
        private readonly Func<TRecordable> _createDefault;

        private Frame[] _frames;
        private int _front;
        private int _count;

        private Frame[] Frames => _frames ??= Enumerable
            .Range(0, _bufferSize)
            .Select(_ => new Frame { Data = _createDefault() })
            .ToArray();

        private Frame FrameAt(int index) => Frames[index % Frames.Length];

        public History(int bufferSize, Func<TRecordable> createDefault) {
            _bufferSize = bufferSize;
            _createDefault = createDefault ?? throw new ArgumentNullException(nameof(createDefault));
        }

        /// <summary>
        /// Adds a new snapshot to the history. Assumes all pushes happen in chronological order.
        /// </summary>
        /// <param name="frame">The frame to push</param>
        /// <param name="when">The timestamp of the push</param>
        /// <returns>This <see cref="History{T}"/> object</returns>
        public History<TRecordable> PushFrame(TRecordable frame, float when) {
            if (_bufferSize > 0 && _createDefault != null) {
                // Increment counters
                _front = (_front + 1) % Frames.Length;
                _count = Math.Min(_count + 1, Frames.Length);

                // Set the frame's data
                FrameAt(_front).Data.Set(frame);
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
            if (_bufferSize > 0 && _createDefault != null) {
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
                    _front -= (frameOffset - 1) % Frames.Length;
                }
            }
            return this;
        }
    }
}