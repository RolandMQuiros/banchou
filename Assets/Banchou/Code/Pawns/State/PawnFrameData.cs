using System;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class FrameData : Notifiable<FrameData> {
        [IgnoreMember] public Vector3 Position => _position;
        [Key(0), SerializeField] private Vector3 _position;

        [IgnoreMember] public Vector3 Forward => _forward;
        [Key(1), SerializeField] private Vector3 _forward;

        [IgnoreMember] public ReadOnlyMemory<int> StateHashes => _stateHashes;
        [Key(2), SerializeField] private int[] _stateHashes;

        [IgnoreMember] public ReadOnlyMemory<float> NormalizedTimes => _normalizedTimes;
        [Key(3), SerializeField] private float[] _normalizedTimes;

        [IgnoreMember] public IReadOnlyDictionary<int, float> Floats => _floats;
        [Key(4)] private Dictionary<int, float> _floats = new Dictionary<int, float>();

        [IgnoreMember] public IReadOnlyDictionary<int, int> Ints => _ints;
        [Key(5)] private Dictionary<int, int> _ints = new Dictionary<int, int>();

        [IgnoreMember] public IReadOnlyDictionary<int, bool> Bools => _bools;
        [Key(6)] private Dictionary<int, bool> _bools = new Dictionary<int, bool>();

        [IgnoreMember] public float When => _when;
        [Key(7), SerializeField] private float _when;

        public FrameData StartFrame(int layerCount, Vector3 position, Vector3 forward) {
            _position = position;
            _forward = forward;

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
            _when = when;
            Notify();
            return this;
        }
    }
}