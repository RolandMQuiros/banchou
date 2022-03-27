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
        [Key(0)] public int[] StateHashes => _stateHashes;
        [Key(1)] public float[] NormalizedTimes => _normalizedTimes;

        [Key(2)][field: SerializeField]
        public Dictionary<int, float> Floats { get; private set; } = Floats ?? new Dictionary<int, float>();
        
        [Key(3)][field: SerializeField]
        public Dictionary<int, int> Ints { get; private set; } = Ints ?? new Dictionary<int, int>();
        
        [Key(4)][field: SerializeField]
        public Dictionary<int, bool> Bools { get; private set; } = Bools ?? new Dictionary<int, bool>();
        
        [Key(5)][field: SerializeField]
        public float When { get; private set; } = When;
        
        [IgnoreMember] public bool IsSync { get; private set; }

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
            
            IsSync = false;
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
            return Notify();
        }

        public PawnAnimatorFrame Sync(PawnAnimatorFrame other) {
            _stateHashes = other._stateHashes;
            _normalizedTimes = other._normalizedTimes;
            Floats = other.Floats;
            Ints = other.Ints;
            Bools = other.Bools;
            When = other.When;
            IsSync = true;
            return Notify();
        }
    }
}