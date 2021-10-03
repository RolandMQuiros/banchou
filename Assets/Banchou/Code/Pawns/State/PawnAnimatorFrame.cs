using System;
using System.Collections.Generic;

using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public class PawnAnimatorFrame : Notifiable<PawnAnimatorFrame> {
        [SerializeField] private int[] _stateHashes;
        [Key(0)] public int[] StateHashes => _stateHashes;
        
        [SerializeField] private float[] _normalizedTimes;
        [Key(1)] public float[] NormalizedTimes => _normalizedTimes;
        
        [Key(2)][field: SerializeField] public Dictionary<int, float> Floats { get; private set; }
        [Key(3)][field: SerializeField] public Dictionary<int, int> Ints { get; private set; }
        [Key(4)][field: SerializeField] public Dictionary<int, bool> Bools { get; private set; }
        [Key(5)][field: SerializeField] public float When { get; private set; }

        [SerializationConstructor]
        public PawnAnimatorFrame(
            int[] stateHashes,
            float[] normalizedTimes,
            Dictionary<int, float> floats,
            Dictionary<int, int> ints,
            Dictionary<int, bool> bools,
            float when
        ) {
            _stateHashes = stateHashes;
            _normalizedTimes = normalizedTimes;
            Floats = floats;
            Ints = ints;
            Bools = bools;
            When = when;
        }

        public PawnAnimatorFrame() {
            Floats = new Dictionary<int, float>();
            Ints = new Dictionary<int, int>();
            Bools = new Dictionary<int, bool>();
        }

        public PawnAnimatorFrame StartFrame(int layerCount) {
            if (StateHashes == null) {
                _stateHashes = new int[layerCount];
            }

            if (StateHashes?.Length != layerCount) {
                Array.Resize(ref _stateHashes, layerCount);
            }

            if (NormalizedTimes == null) {
                _normalizedTimes = new float[layerCount];
            }

            if (NormalizedTimes?.Length < layerCount) {
                Array.Resize(ref _normalizedTimes, layerCount);
            }

            return this;
        }

        public PawnAnimatorFrame SetLayerData(int layerIndex, int stateHash, float normalizedTime) {
            StateHashes[layerIndex] = stateHash;
            NormalizedTimes[layerIndex] = normalizedTime;
            return this;
        }

        public PawnAnimatorFrame SetFloat(int parameterFullPathHash, float value) {
            Floats[parameterFullPathHash] = value;
            return this;
        }

        public PawnAnimatorFrame SetInt(int parameterFullPathHash, int value) {
            Ints[parameterFullPathHash] = value;
            return this;
        }

        public PawnAnimatorFrame SetBool(int parameterFullPathHash, bool value) {
            Bools[parameterFullPathHash] = value;
            return this;
        }

        public PawnAnimatorFrame FinishFrame(float when) {
            When = when;
            Notify();
            return this;
        }
    }
}