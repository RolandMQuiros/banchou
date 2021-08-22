using System;
using System.Collections.Generic;

using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class FrameData : Notifiable<FrameData> {
        [Key(0)][field: SerializeField] public Vector3 Position { get; private set; }
        [Key(1)][field: SerializeField] public Vector3 Forward { get; private set; }
        [Key(2)] public int[] StateHashes => _stateHashes;
        [SerializeField] private int[] _stateHashes;
        [Key(3)] public float[] NormalizedTimes => _normalizedTimes;
        [SerializeField] private float[] _normalizedTimes;
        [Key(4)][field: SerializeField] public Dictionary<int, float> Floats { get; private set; }
        [Key(5)][field: SerializeField] public Dictionary<int, int> Ints { get; private set; }
        [Key(6)][field: SerializeField] public Dictionary<int, bool> Bools { get; private set; }
        [Key(7)][field: SerializeField] public float When { get; private set; }

        [SerializationConstructor]
        public FrameData(Vector3 position, Vector3 forward, int[] stateHashes, float[] normalizedTimes, Dictionary<int, float> floats, Dictionary<int, int> ints, Dictionary<int, bool> bools, float when) {
            Position = position;
            Forward = forward;
            _stateHashes = stateHashes;
            _normalizedTimes = normalizedTimes;
            Floats = floats;
            Ints = ints;
            Bools = bools;
        }

        public FrameData() {
            Floats = new Dictionary<int, float>();
            Ints = new Dictionary<int, int>();
            Bools = new Dictionary<int, bool>();
        }

        public FrameData StartFrame(int layerCount, Vector3 position, Vector3 forward) {
            Position = position;
            Forward = forward;

            if (StateHashes == null) {
                _stateHashes = new int[layerCount];
            }

            if (StateHashes.Length != layerCount) {
                Array.Resize(ref _stateHashes, layerCount);
            }

            if (NormalizedTimes == null) {
                _normalizedTimes = new float[layerCount];
            }

            if (NormalizedTimes.Length < layerCount) {
                Array.Resize(ref _normalizedTimes, layerCount);
            }

            return this;
        }

        public FrameData SetLayerData(int layerIndex, int stateHash, float normalizedTime) {
            StateHashes[layerIndex] = stateHash;
            NormalizedTimes[layerIndex] = normalizedTime;
            return this;
        }

        public FrameData SetFloat(int parameterFullPathHash, float value) {
            Floats[parameterFullPathHash] = value;
            return this;
        }

        public FrameData SetInt(int parameterFullPathHash, int value) {
            Ints[parameterFullPathHash] = value;
            return this;
        }

        public FrameData SetBool(int parameterFullPathHash, bool value) {
            Bools[parameterFullPathHash] = value;
            return this;
        }

        public FrameData FinishFrame(float when) {
            When = when;
            Notify();
            return this;
        }
    }
}