using System;
using System.Collections.Generic;
using UnityEngine;

namespace Banchou.Pawn {
    [Serializable]
    public class FrameData : Notifiable<FrameData> {
        [field: SerializeField] public Vector3 Position { get; private set; }
        [field: SerializeField] public Vector3 Forward { get; private set; }

        public ReadOnlyMemory<int> StateHashes => _stateHashes;
        [SerializeField] private int[] _stateHashes;

        public ReadOnlyMemory<float> NormalizedTimes => _normalizedTimes;
        [SerializeField] private float[] _normalizedTimes;

        public IReadOnlyDictionary<int, float> Floats => _floats;
        [SerializeField] private Dictionary<int, float> _floats = new Dictionary<int, float>();

        public IReadOnlyDictionary<int, int> Ints => _ints;
        [SerializeField] private Dictionary<int, int> _ints = new Dictionary<int, int>();

        public IReadOnlyDictionary<int, bool> Bools => _bools;
        [SerializeField] private Dictionary<int, bool> _bools = new Dictionary<int, bool>();

        [field: SerializeField] public float When { get; private set; }

        public FrameData StartFrame(int layerCount, Vector3 position, Vector3 forward) {
            Position = position;
            Forward = forward;

            if (_stateHashes == null) {
                _stateHashes = new int[layerCount];
            }

            if (_stateHashes.Length != layerCount) {
                Array.Resize(ref _stateHashes, layerCount);
            }

            if (_normalizedTimes == null) {
                _normalizedTimes = new float[layerCount];
            }

            if (_normalizedTimes.Length < layerCount) {
                Array.Resize(ref _normalizedTimes, layerCount);
            }

            return this;
        }

        public FrameData SetLayerData(int layerIndex, int stateHash, float normalizedTime) {
            _stateHashes[layerIndex] = stateHash;
            _normalizedTimes[layerIndex] = normalizedTime;
            return this;
        }

        public FrameData SetFloat(int parameterFullPathHash, float value) {
            _floats[parameterFullPathHash] = value;
            return this;
        }

        public FrameData SetInt(int parameterFullPathHash, int value) {
            _floats[parameterFullPathHash] = value;
            return this;
        }

        public FrameData SetBool(int parameterFullPathHash, bool value) {
            _bools[parameterFullPathHash] = value;
            return this;
        }

        public FrameData FinishFrame(float when) {
            When = when;
            Notify();
            return this;
        }
    }
}