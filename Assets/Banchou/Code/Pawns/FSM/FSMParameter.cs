using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414

namespace Banchou.Pawn.FSM {
    [Serializable]
    public class FSMParameter {
        [SerializeField] private string _name;
        [SerializeField] private int _hash;
        [SerializeField] protected AnimatorControllerParameterType _type;
        [SerializeField] protected bool _filterByType;

        public string Name => _name;
        public int Hash => _hash;
        public AnimatorControllerParameterType Type => _type;
        public bool IsSet => _hash != default;
        
        public FSMParameter() { }
        
        public FSMParameter(AnimatorControllerParameterType type) {
            _type = type;
            _filterByType = true;
        }

        public bool GetBool(Animator animator, bool defaultValue = default) =>
            IsSet ? animator.GetBool(_hash) : defaultValue;
        public float GetFloat(Animator animator, float defaultValue = default) =>
            IsSet ? animator.GetFloat(_hash) : defaultValue;
        public int GetInt(Animator animator, int defaultValue = default) =>
            IsSet ? animator.GetInteger(_hash) : defaultValue;

        public void Apply(Animator animator, bool value) {
            if (IsSet) {
                animator.SetBool(_hash, value);
            }
        }
        
        public void Apply(Animator animator, float value) {
            if (IsSet) {
                animator.SetFloat(_hash, value);
            }
        }
        
        public void Apply(Animator animator, int value) {
            if (IsSet) {
                animator.SetInteger(_hash, value);
            }
        }
        
        public void SetTrigger(Animator animator) {
            if (IsSet) {
                animator.SetTrigger(_hash);
            }
        }

        public void ResetTrigger(Animator animator) {
            if (IsSet) {
                animator.ResetTrigger(_hash);
            }
        }
    }

    #region FSMParameter child classes for serialization
    [Serializable]
    public class BoolFSMParameter : FSMParameter, ISerializationCallbackReceiver {
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() {
            _type = AnimatorControllerParameterType.Bool;
            _filterByType = true;
        }
    }

    [Serializable]
    public class FloatFSMParameter : FSMParameter, ISerializationCallbackReceiver {
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() {
            _type = AnimatorControllerParameterType.Float;
            _filterByType = true;
        }
    }

    [Serializable]
    public class IntFSMParameter : FSMParameter {
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() {
            _type = AnimatorControllerParameterType.Int;
            _filterByType = true;
        }
    }

    [Serializable]
    public class TriggerFSMParameter : FSMParameter {
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() {
            _type = AnimatorControllerParameterType.Trigger;
            _filterByType = true;
        }
    }
    #endregion

    [Serializable]
    public class FSMReadParameter {
        [SerializeField] private AnimatorControllerParameterType _parameterType;
        [SerializeField] private FSMParameter _source;
        [SerializeField] private float _floatValue;
        [SerializeField] private int _intValue;
        [SerializeField] private bool _boolValue;
        
        public FSMReadParameter(AnimatorControllerParameterType type) {
            _parameterType = type;
            _source = new(type);
        }
        
        public bool GetBool(Animator animator) => _source.GetBool(animator, _boolValue);
        public float GetFloat(Animator animator) => _source.GetFloat(animator, _floatValue);
        public int GetInt(Animator animator) => _source.GetInt(animator, _intValue);
    }

    [Serializable]
    public class FSMParameterCondition {
        public enum ConditionMode { If, IfNot, Greater, Less, Equals, NotEqual }
        [SerializeField] private FSMParameter _parameter;
        [SerializeField] public ConditionMode _mode;
        [SerializeField] public float _threshold;

        public bool Evaluate(Animator animator) {
            if (!_parameter.IsSet) return true;
            switch (_parameter.Type) {
                case AnimatorControllerParameterType.Bool:
                    return EvaluateBool(animator);
                case AnimatorControllerParameterType.Float:
                    return EvaluateFloat(animator);
                case AnimatorControllerParameterType.Int:
                    return EvaluateInt(animator);
                case AnimatorControllerParameterType.Trigger:
                    return EvaluateTrigger(animator);
            }
            return false;
        }

        private bool EvaluateBool(Animator animator) {
            var value = animator.GetBool(_parameter.Hash);
            switch (_mode) {
                case ConditionMode.If:
                    return value;
                case ConditionMode.IfNot:
                    return !value;
            }
            return false;
        }

        private bool EvaluateFloat(Animator animator) {
            var value = _parameter.GetFloat(animator);
            switch (_mode) {
                case ConditionMode.Greater:
                    return value > _threshold;
                case ConditionMode.Less:
                    return value < _threshold;
            }
            return false;
        }

        private bool EvaluateInt(Animator animator) {
            var value = _parameter.GetInt(animator);
            var rounded = Mathf.RoundToInt(_threshold);
            switch (_mode) {
                case ConditionMode.Equals:
                    return value == rounded;
                case ConditionMode.NotEqual:
                    return value != rounded;
                case ConditionMode.Greater:
                    return value > _threshold;
                case ConditionMode.Less:
                    return value < _threshold;
            }
            return false;
        }

        private bool EvaluateTrigger(Animator animator) {
            return _parameter.GetBool(animator);
        }
    }

    [Serializable]
    public class ApplyFSMParameter {
        public enum ApplyMode { Set, Unset, Toggle, Add, Multiply, FromParameter }

        [SerializeField] private FSMParameter _parameter;
        [SerializeField] private ApplyMode _applyMode;
        [SerializeField] private float _value;
        [SerializeField] private FSMParameter _sourceParameter;

        public ApplyFSMParameter() { }
        public ApplyFSMParameter(AnimatorControllerParameterType type) {
            _parameter = new(type);
        }
        
        public void Apply(Animator animator) {
            if (_parameter.IsSet) {
                switch (_parameter.Type) {
                    case AnimatorControllerParameterType.Bool:
                        ApplyBool(animator);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        ApplyTrigger(animator);
                        break;
                    case AnimatorControllerParameterType.Float:
                        ApplyFloat(animator);
                        break;
                    case AnimatorControllerParameterType.Int:
                        ApplyInt(animator);
                        break;
                }
            }
        }

        private void ApplyTrigger(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.Set:
                    _parameter.SetTrigger(animator);
                    break;
                case ApplyMode.Unset:
                    _parameter.ResetTrigger(animator);
                    break;
            }
        }
        
        private void ApplyBool(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.Set:
                    _parameter.Apply(animator, true);
                    break;
                case ApplyMode.Unset:
                    _parameter.Apply(animator, false);
                    break;
                case ApplyMode.Toggle:
                    _parameter.Apply(animator, _parameter.GetBool(animator));
                    break;
            }
        }

        private void ApplyFloat(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.FromParameter:
                    _parameter.Apply(animator, _sourceParameter.GetFloat(animator));
                    break;
                case ApplyMode.Set:
                    _parameter.Apply(animator, _value);
                    break;
                case ApplyMode.Unset:
                    _parameter.Apply(animator, 0f);
                    break;
                case ApplyMode.Add:
                    _parameter.Apply(animator, _parameter.GetFloat(animator) + _value);
                    break;
                case ApplyMode.Multiply:
                    _parameter.Apply(animator, _parameter.GetFloat(animator) * _value);
                    break;
            }
        }
        
        private void ApplyInt(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.FromParameter:
                    _parameter.Apply(animator, _parameter.GetInt(animator));
                    break;
                case ApplyMode.Set:
                    _parameter.Apply(animator, (int)_value);
                    break;
                case ApplyMode.Unset:
                    _parameter.Apply(animator, 0);
                    break;
                case ApplyMode.Add:
                    _parameter.Apply(animator, _parameter.GetInt(animator) + (int)_value);
                    break;
                case ApplyMode.Multiply:
                    _parameter.Apply(animator, _parameter.GetInt(animator) * (int)_value);
                    break;
            }
        }
    }

    public static class FSMParameterExtensions {
        public static void ApplyAll(this List<ApplyFSMParameter> parameters, Animator animator) {
            for (int i = 0; i < parameters.Count; i++) {
                parameters[i].Apply(animator);
            }
        }
        
        public static void ApplyAll(this ApplyFSMParameter[] parameters, Animator animator) {
            for (int i = 0; i < parameters.Length; i++) {
                parameters[i].Apply(animator);
            }
        }
    }
}