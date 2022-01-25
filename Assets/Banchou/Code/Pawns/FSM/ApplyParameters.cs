using System;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    [Serializable]
    public abstract class ApplyParameter {
        [SerializeField] private string _parameterName;
        protected Animator Animator;
        protected int Hash;

        public virtual void Construct(Animator animator) {
            Animator = animator;
            Hash = Animator.StringToHash(_parameterName);
        }

        public abstract void Apply();
    }
    
    [Serializable]
    public class ApplyTriggerParameter : ApplyParameter {
        public override void Apply() => Animator.SetTrigger(Hash);
        public void Reset() => Animator.ResetTrigger(Hash);
    }

    [Serializable]
    public class ApplyBooleanParameter : ApplyParameter {
        private enum ApplyMode { Set, Unset, Toggle }
        [SerializeField] private ApplyMode applyMode;

        public override void Apply() {
            bool output = false;
            switch (applyMode) {
                case ApplyMode.Set:
                    output = true;
                    break;
                case ApplyMode.Unset:
                    output = false;
                    break;
                case ApplyMode.Toggle:
                    output = !Animator.GetBool(Hash);
                    break;
            }
            Animator.SetBool(Hash, output);
        }

        public void Apply(bool value) => Animator.SetBool(Hash, value);
    }

    [Serializable]
    public class ParameterCondition {
        public enum ConditionMode { If, IfNot, Greater, Less, Equals, NotEqual }
        [SerializeField] public ConditionMode _mode;
        [SerializeField] public float _threshold;
        [SerializeField, HideInInspector] private int _hash;
        [SerializeField, HideInInspector] private AnimatorControllerParameterType _parameterType;

        public bool Evaluate(Animator animator) {
            switch (_parameterType) {
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
            var value = animator.GetBool(_hash);
            switch (_mode) {
                case ConditionMode.If:
                    return value;
                case ConditionMode.IfNot:
                    return !value;
            }
            return false;
        }

        private bool EvaluateFloat(Animator animator) {
            var value = animator.GetFloat(_hash);
            switch (_mode) {
                case ConditionMode.Greater:
                    return value > _threshold;
                case ConditionMode.Less:
                    return value < _threshold;
            }
            return false;
        }

        private bool EvaluateInt(Animator animator) {
            var value = animator.GetInteger(_hash);
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
            return animator.GetBool(_hash);
        }
    }
}