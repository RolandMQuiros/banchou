using System;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    [Serializable]
    public class FSMParameter {
        [SerializeField] private string _name;
        [SerializeField] private int _hash;
        [SerializeField] private AnimatorControllerParameterType _type;
        [SerializeField] private bool _filterByType;

        public string Name => _name;
        public int Hash => _hash;
        public AnimatorControllerParameterType Type => _type;
        public bool IsSet => _hash != default;
        public FSMParameter(bool filterByType = false) {
            _filterByType = filterByType;
        }
    }

    [Serializable]
    public class FSMParameterCondition {
        public enum ConditionMode { If, IfNot, Greater, Less, Equals, NotEqual }
        [SerializeField] private FSMParameter _parameter;
        [SerializeField] public ConditionMode _mode;
        [SerializeField] public float _threshold;

        public bool Evaluate(Animator animator) {
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
            var value = animator.GetFloat(_parameter.Hash);
            switch (_mode) {
                case ConditionMode.Greater:
                    return value > _threshold;
                case ConditionMode.Less:
                    return value < _threshold;
            }
            return false;
        }

        private bool EvaluateInt(Animator animator) {
            var value = animator.GetInteger(_parameter.Hash);
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
            return animator.GetBool(_parameter.Hash);
        }
    }

    [Serializable]
    public class ApplyFSMParameter {
        public enum ApplyMode { Set, Unset, Toggle, FromParameter }

        [SerializeField] private FSMParameter _parameter;
        [SerializeField] private ApplyMode _applyMode;
        [SerializeField] private float _value;
        [SerializeField] private FSMParameter _sourceParameter = new(true);

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
                    animator.SetTrigger(_parameter.Hash);
                    break;
                case ApplyMode.Unset:
                    animator.ResetTrigger(_parameter.Hash);
                    break;
            }
        }
        
        private void ApplyBool(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.Set:
                    animator.SetBool(_parameter.Hash, true);
                    break;
                case ApplyMode.Unset:
                    animator.SetBool(_parameter.Hash, false);
                    break;
                case ApplyMode.Toggle:
                    animator.SetBool(_parameter.Hash, !animator.GetBool(_parameter.Hash));
                    break;
            }
        }

        private void ApplyFloat(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.FromParameter:
                    animator.SetFloat(_parameter.Hash, animator.GetFloat(_sourceParameter.Hash));
                    break;
                case ApplyMode.Set:
                    animator.SetFloat(_parameter.Hash, _value);
                    break;
                case ApplyMode.Unset:
                    animator.SetFloat(_parameter.Hash, 0f);
                    break;
            }
        }
        
        private void ApplyInt(Animator animator) {
            switch (_applyMode) {
                case ApplyMode.FromParameter:
                    animator.SetInteger(_parameter.Hash, animator.GetInteger(_sourceParameter.Hash));
                    break;
                case ApplyMode.Set:
                    animator.SetInteger(_parameter.Hash, (int)_value);
                    break;
                case ApplyMode.Unset:
                    animator.SetInteger(_parameter.Hash, 0);
                    break;
            }
        }
    }
}