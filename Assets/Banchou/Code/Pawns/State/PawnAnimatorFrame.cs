using System;
using System.Collections.Generic;

using MessagePack;
using UnityEngine;

namespace Banchou.Pawn {
    [MessagePackObject, Serializable]
    public record PawnAnimatorFrame(
        int[] StateHashes = null,
        float[] NormalizedTimes = null,
        Dictionary<int, float> Floats = null,
        Dictionary<int, int> Ints = null,
        Dictionary<int, bool> Bools = null,
        float When = 0f
    ) : Notifiable<PawnAnimatorFrame> {
        private int[] _stateHashes = StateHashes;
        private float[] _normalizedTimes = NormalizedTimes;
        public int[] StateHashes => _stateHashes;
        public float[] NormalizedTimes => _normalizedTimes;

        [field: SerializeField]
        public Dictionary<int, float> Floats { get; init; } = Floats ?? new Dictionary<int, float>();
        
        [field: SerializeField]
        public Dictionary<int, int> Ints { get; init; } = Ints ?? new Dictionary<int, int>();
        
        [field: SerializeField]
        public Dictionary<int, bool> Bools { get; init; } = Bools ?? new Dictionary<int, bool>();
        
        [field: SerializeField]
        public float When { get; private set; } = When;

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