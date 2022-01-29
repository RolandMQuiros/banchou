using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Banchou.Pawn.FSM {
    [CustomPropertyDrawer(typeof(FSMParameter))]
    public class FSMParameterDrawer : PropertyDrawer {
        private StateMachineBehaviour _behaviour;
        private AnimatorController _controller;

        private AnimatorControllerParameter[] _parameters;
        private string[] _parameterNames;
        private List<int> _parameterHashes;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!Application.isPlaying && TryGetController(property)) {
                DrawProperty(position, property, label);
            } else {
                DrawPropertyPlaymode(position, property, label);
            }
        }

        protected bool TryGetController(SerializedProperty property) {
            _behaviour ??= property.serializedObject.targetObject as StateMachineBehaviour;
            if (_behaviour != null) {
                _controller ??= AnimatorController
                    .FindStateMachineBehaviourContext(_behaviour)[0]
                    .animatorController;
                return true;
            }
            return false;
        }

        private void UpdateParameters(AnimatorControllerParameterType? type = null) {
            var parameters = _controller.parameters;
            if (type != null) {
                parameters = _controller.parameters
                    .Where(parameter => parameter.type == type)
                    .ToArray();
            }

            if (parameters != _parameters) {
                _parameters = parameters;
                _parameterNames = _parameters
                    .Select(parameter => parameter.name)
                    .Prepend("None")
                    .ToArray();
                _parameterHashes = _parameters
                    .Select(parameter => parameter.nameHash)
                    .Prepend(0)
                    .ToList();
            }
        }

        protected void DrawProperty(Rect position, SerializedProperty property, GUIContent label) {
            var name = property.FindPropertyRelative("_name");
            var hash = property.FindPropertyRelative("_hash");
            var type = property.FindPropertyRelative("_type");
            var filter = property.FindPropertyRelative("_filterByType");
            UpdateParameters(filter.boolValue ? (AnimatorControllerParameterType) type.intValue : null);

            EditorGUI.BeginProperty(position, label, property);
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginChangeCheck();

            var currentParameterIndex = Math.Max(_parameterHashes.IndexOf(hash.intValue), 0);
            currentParameterIndex = EditorGUI.Popup(position, currentParameterIndex, _parameterNames);

            if (EditorGUI.EndChangeCheck()) {
                var currentParameter = currentParameterIndex <= 0 ? null : _parameters[currentParameterIndex - 1];

                if (currentParameter != null) {
                    name.stringValue = _parameterNames[currentParameterIndex];
                    hash.intValue = _parameterHashes[currentParameterIndex];
                    type.intValue = (int) currentParameter.type;
                }
            }
            
            EditorGUI.EndProperty();
        }

        protected void DrawPropertyPlaymode(Rect position, SerializedProperty property, GUIContent label) {
            var name = property.FindPropertyRelative("_name");
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(position, name.stringValue);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(ApplyFSMParameter))]
    public class ApplyFSMParameterDrawer : PropertyDrawer {
        private SerializedProperty _parameter;
        private SerializedProperty _name;
        private SerializedProperty _hash;
        private SerializedProperty _type;
        private SerializedProperty _applyMode;
        private SerializedProperty _value;
        private SerializedProperty _sourceParameter;
        private SerializedProperty _sourceType;
        private SerializedProperty _sourceName;
        private SerializedProperty _sourceHash;
        private SerializedProperty _sourceFilter;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            _parameter = property.FindPropertyRelative("_parameter");
            _name = _parameter.FindPropertyRelative("_name");
            _hash = _parameter.FindPropertyRelative("_hash");
            _type = _parameter.FindPropertyRelative("_type");
            _applyMode = property.FindPropertyRelative("_applyMode");
            _value = property.FindPropertyRelative("_value");
            _sourceParameter = property.FindPropertyRelative("_sourceParameter");
            _sourceType = _sourceParameter.FindPropertyRelative("_type");
            _sourceName = _sourceParameter.FindPropertyRelative("_name");
            _sourceHash = _sourceParameter.FindPropertyRelative("_hash");
            _sourceFilter = _sourceParameter.FindPropertyRelative("_filterByType");

            EditorGUI.BeginProperty(position, label, property);
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel = 0;
            
            if (_hash.intValue != default) {
                var parameterType = (AnimatorControllerParameterType) _type.intValue;
                
                switch (parameterType) {
                    case AnimatorControllerParameterType.Bool:
                        DrawBoolean(position);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        DrawTrigger(position);
                        break;
                    case AnimatorControllerParameterType.Float:
                    case AnimatorControllerParameterType.Int:
                        DrawNumber(position);
                        break;
                }
            } else {
                var parameterRect = new Rect(position) { width = position.width * 0.5f };
                EditorGUI.PropertyField(parameterRect, _parameter, GUIContent.none);
            }
            EditorGUI.EndProperty();
        }

        private Rect DrawParameterAndApplyMode(
            Rect position, Func<Enum, bool> selectModes, ApplyFSMParameter.ApplyMode defaultMode
        ) {
            var parameterRect = new Rect(position) { width = position.width * 0.5f };
            EditorGUI.PropertyField(parameterRect, _parameter, GUIContent.none);

            var applyModeRect = new Rect(position) {
                x = position.x + parameterRect.width + 5f,
                width = position.width * 0.2f - 5f
            };
            
            var applyMode = (ApplyFSMParameter.ApplyMode) _applyMode.intValue;
            if (!selectModes(applyMode)) {
                applyMode = defaultMode;
            }

            if (Application.isPlaying) {
                EditorGUI.LabelField(applyModeRect, applyMode.ToString());
            } else {
                applyMode = (ApplyFSMParameter.ApplyMode) EditorGUI.EnumPopup(
                    applyModeRect, GUIContent.none, applyMode, selectModes
                );

                if (_applyMode.intValue != (int) applyMode) {
                    _applyMode.intValue = (int) applyMode;
                }
            }

            return new Rect(position) { width = parameterRect.width + applyModeRect.width + 5f };
        }

        private void DrawBoolean(Rect position) {
            var rect = DrawParameterAndApplyMode(
                position,
                mode => (ApplyFSMParameter.ApplyMode) mode != ApplyFSMParameter.ApplyMode.Toggle,
                ApplyFSMParameter.ApplyMode.Set
            );

            var setRect = new Rect(position) {
                x = position.x + rect.width + 5f,
                width = position.width - rect.width - 10f
            };

            if (_applyMode.intValue == (int) ApplyFSMParameter.ApplyMode.FromParameter) {
                EditorGUI.PropertyField(setRect, _sourceParameter, GUIContent.none);
                if (_sourceType.intValue != _type.intValue) {
                    _sourceHash.intValue = _hash.intValue;
                    _sourceType.intValue = _type.intValue;
                    _sourceName.stringValue = _name.stringValue;
                    _sourceFilter.boolValue = true;
                }
            }
        }

        private void DrawTrigger(Rect position) {
            DrawParameterAndApplyMode(
                position,
                mode => (ApplyFSMParameter.ApplyMode) mode == ApplyFSMParameter.ApplyMode.Set ||
                        (ApplyFSMParameter.ApplyMode) mode == ApplyFSMParameter.ApplyMode.Unset,
                ApplyFSMParameter.ApplyMode.Set
            );
        }
        
        private void DrawNumber(Rect position) {
            var rect = DrawParameterAndApplyMode(
                position,
                mode => (ApplyFSMParameter.ApplyMode) mode != ApplyFSMParameter.ApplyMode.Toggle,
                ApplyFSMParameter.ApplyMode.Set
            );

            var setRect = new Rect(position) {
                x = position.x + rect.width + 5f,
                width = position.width - rect.width - 10f
            };
            
            switch ((ApplyFSMParameter.ApplyMode) _applyMode.intValue) {
                case ApplyFSMParameter.ApplyMode.Set:
                    if (Application.isPlaying) {
                        EditorGUI.LabelField(setRect, _value.stringValue);
                    } else {
                        EditorGUI.PropertyField(setRect, _value, GUIContent.none);
                    }
                    break;
                case ApplyFSMParameter.ApplyMode.FromParameter:
                    EditorGUI.PropertyField(setRect, _sourceParameter, GUIContent.none);
                    if (_sourceType.intValue != _type.intValue) {
                        _sourceHash.intValue = _hash.intValue;
                        _sourceType.intValue = _type.intValue;
                        _sourceName.stringValue = _name.stringValue;
                        _sourceFilter.boolValue = true;
                    } 
                    break;
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(FSMParameterCondition))]
    public class FSMParameterConditionDrawer : PropertyDrawer {
        private string[] _parameterNames;
        private List<int> _parameterHashes;

        private SerializedProperty _parameter;
        private SerializedProperty _parameterType;
        private SerializedProperty _mode;
        private SerializedProperty _threshold;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            _parameter = property.FindPropertyRelative("_parameter");
            _parameterType = _parameter.FindPropertyRelative("_type");
            _mode = property.FindPropertyRelative("_mode");
            _threshold = property.FindPropertyRelative("_threshold");
            
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel = 0;
            
            var paramRect = new Rect(position) { width = position.width * 0.4f };
            EditorGUI.PropertyField(paramRect, _parameter, GUIContent.none);
            
            var conditionRect = new Rect(position) { x = position.x + paramRect.width + 5f };
            switch ((AnimatorControllerParameterType) _parameterType.intValue) {
                case AnimatorControllerParameterType.Bool:
                    DrawBoolCondition(conditionRect);
                    break;
                case AnimatorControllerParameterType.Float:
                    DrawFloatCondition(conditionRect);
                    break;
                case AnimatorControllerParameterType.Int:
                    DrawIntCondition(conditionRect);
                    break;
            }
            
            EditorGUI.EndProperty();
        }

        private void DrawBoolCondition(Rect position) {
            var mode = (FSMParameterCondition.ConditionMode) _mode.intValue;
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.Toggle(position, mode == FSMParameterCondition.ConditionMode.If)) {
                mode = FSMParameterCondition.ConditionMode.If;
            } else {
                mode = FSMParameterCondition.ConditionMode.IfNot;
            }
            if (EditorGUI.EndChangeCheck()) {
                _mode.intValue = (int) mode;
            }
        }
        
        private static readonly FSMParameterCondition.ConditionMode[] FloatModes = {
            FSMParameterCondition.ConditionMode.Greater,
            FSMParameterCondition.ConditionMode.Less
        };
        private static readonly string[] FloatModeLabels = FloatModes.Select(mode => mode.ToString()).ToArray();

        private void DrawFloatCondition(Rect position) {
            var mode = (FSMParameterCondition.ConditionMode) _mode.intValue;
            var modeRect = new Rect(position) { width = position.width * 0.5f - 5f };
            var modeIndex = Math.Max(Array.FindIndex(IntModes, intMode => intMode == mode), 0);
            var thresholdRect = new Rect(modeRect) { x = position.x + modeRect.width + 0.5f };

            EditorGUI.BeginChangeCheck();
            modeIndex = EditorGUI.Popup(modeRect, modeIndex, FloatModeLabels);
            var threshold = EditorGUI.FloatField(thresholdRect, _threshold.floatValue);
            if (EditorGUI.EndChangeCheck()) {
                _mode.intValue = (int) FloatModes[modeIndex];
                _threshold.floatValue = threshold;
            }
        }
        
        private static readonly FSMParameterCondition.ConditionMode[] IntModes = {
            FSMParameterCondition.ConditionMode.Greater,
            FSMParameterCondition.ConditionMode.Less,
            FSMParameterCondition.ConditionMode.Equals,
            FSMParameterCondition.ConditionMode.NotEqual
        };
        private static readonly string[] IntModeLabels = IntModes.Select(mode => mode.ToString()).ToArray();

        private void DrawIntCondition(Rect position) {
            var mode = (FSMParameterCondition.ConditionMode) _mode.intValue;
            var modeRect = new Rect(position) { width = position.width * 0.5f - 5f };
            var modeIndex = Math.Max(Array.FindIndex(IntModes, intMode => intMode == mode), 0);
            var thresholdRect = new Rect(modeRect) { x = position.x + modeRect.width + 0.5f };

            EditorGUI.BeginChangeCheck();
            modeIndex = EditorGUI.Popup(modeRect, modeIndex, IntModeLabels);
            var threshold = EditorGUI.IntField(thresholdRect, _threshold.intValue);
            if (EditorGUI.EndChangeCheck()) {
                _mode.intValue = (int) IntModes[modeIndex];
                _threshold.intValue = threshold;
            }
        }
    }
}