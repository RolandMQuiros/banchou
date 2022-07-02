using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Banchou.Pawn.FSM {
    [CustomPropertyDrawer(typeof(FSMParameter), useForChildren: true)]
    public class FSMParameterDrawer : PropertyDrawer {
        private StateMachineBehaviour _behaviour;
        private AnimatorController _controller;

        private AnimatorControllerParameter[] _parameters;
        private string[] _parameterNames;
        private List<int> _parameterHashes;
        
        /// <summary>
        /// Draws the <see cref="FSMParameter"/> in the inspector
        /// </summary>
        /// <param name="position">The encapsulating <see cref="Rect"/> in which to draw the property</param>
        /// <param name="property">The property to draw</param>
        /// <param name="label">An accompanying label drawn next to the property</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!Application.isPlaying && TryGetController(property)) {
                DrawPropertyEditMode(position, property, label);
            } else {
                DrawPropertyPlaymode(position, property, label);
            }
        }
        
        /// <summary>
        /// Attempt to extract a reference to the encompassing <see cref="AnimatorController"/> from the owning
        /// <see cref="SerializedObject"/>.
        /// </summary>
        /// <remarks>
        /// TODO: See if we can grab it from an owning MonoBehaviour too
        /// </remarks>
        /// <param name="property">The property being drawn</param>
        /// <returns><c>true</c> if a <see cref="AnimatorController"/> was found. <c>false</c> otherwise.</returns>
        protected bool TryGetController(SerializedProperty property) {
            _behaviour ??= property.serializedObject.targetObject as StateMachineBehaviour;
            if (_behaviour != null) {
                _controller ??= AnimatorController
                    .FindStateMachineBehaviourContext(_behaviour)?
                    .ElementAt(0)?
                    .animatorController;
                return _controller != null;
            }
            return false;
        }
        
        /// <summary>
        /// Update the list of <see cref="AnimatorControllerParameter"/>s available in the dropdowns, filtered by
        /// <see cref="AnimatorControllerParameterType"/> 
        /// </summary>
        /// <param name="type">Filters the parameter list by this type. <c>null</c> for no filter.</param>
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

        /// <summary>
        /// Draws this property in edit mode, with fully-interactable widgets
        /// </summary>
        /// <param name="position">The encapsulating <see cref="Rect"/> in which to draw the property</param>
        /// <param name="property">The property to draw</param>
        /// <param name="label">An accompanying label drawn next to the property</param>
        protected void DrawPropertyEditMode(Rect position, SerializedProperty property, GUIContent label) {
            // Extract every bit from the serialized property
            var name = property.FindPropertyRelative("_name");
            var hash = property.FindPropertyRelative("_hash");
            var type = property.FindPropertyRelative("_type");
            var filter = property.FindPropertyRelative("_filterByType");
            
            // Grab the latest state of the parameters from the controller
            UpdateParameters(filter.boolValue ? (AnimatorControllerParameterType) type.intValue : null);

            EditorGUI.BeginProperty(position, label, property);
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginChangeCheck();
            
            // Display a dropdown with the available animator parameters
            var currentParameterIndex = Math.Max(_parameterHashes.IndexOf(hash.intValue), 0);
            currentParameterIndex = EditorGUI.Popup(position, currentParameterIndex, _parameterNames);

            // Apply changes, if any
            if (EditorGUI.EndChangeCheck()) {
                var currentParameter = currentParameterIndex <= 0 ? null : _parameters[currentParameterIndex - 1];

                if (currentParameterIndex > 0 && currentParameter == null) {
                    Debug.LogWarning($"Could not find animator parameter \"{name.stringValue}\" " +
                                     $"for object at {property.propertyPath}");
                } else {
                    name.stringValue = _parameterNames[currentParameterIndex];
                    hash.intValue = _parameterHashes[currentParameterIndex];
                    if (currentParameter != null) {
                        type.intValue = (int) currentParameter.type;
                    }
                }
            }
            
            EditorGUI.EndProperty();
        }
        
        /// <summary>
        /// Draw the parameter and its values only as labels, since <see cref="AnimatorController"/>s cannot be modified
        /// during PlayMode.
        /// </summary>
        /// <param name="position">The encapsulating <see cref="Rect"/> in which to draw the property</param>
        /// <param name="property">The property to draw</param>
        /// <param name="label">An accompanying label drawn next to the property</param>
        protected void DrawPropertyPlaymode(Rect position, SerializedProperty property, GUIContent label) {
            var name = property.FindPropertyRelative("_name");
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(position, name.stringValue);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(FSMParameterField<FSMParameter>))]
    [CustomPropertyDrawer(typeof(FSMParameterField<FloatFSMParameter>))]
    [CustomPropertyDrawer(typeof(FSMParameterField<IntFSMParameter>))]
    [CustomPropertyDrawer(typeof(FSMParameterField<BoolFSMParameter>))]
    [CustomPropertyDrawer(typeof(FSMParameterField<TriggerFSMParameter>))]
    public class FSMParameterFieldDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var source = property.FindPropertyRelative("_source");
            var parameterType = source.FindPropertyRelative("_type");
            var sourceHash = source.FindPropertyRelative("_hash");
            var floatValue = property.FindPropertyRelative("_floatValue");
            var intValue = property.FindPropertyRelative("_intValue");
            var boolValue = property.FindPropertyRelative("_boolValue");
            
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var parameterRect = position;
            if (sourceHash.intValue == default) {
                var fieldRect = new Rect(position) { width = position.width - 20f };
                parameterRect = new Rect(position) { xMin = position.xMax - 20f };
                
                switch ((AnimatorControllerParameterType) parameterType.intValue) {
                    case AnimatorControllerParameterType.Bool:
                    case AnimatorControllerParameterType.Trigger:
                        EditorGUI.PropertyField(fieldRect, boolValue, GUIContent.none);
                        break;
                    case AnimatorControllerParameterType.Float:
                        EditorGUI.PropertyField(fieldRect, floatValue, GUIContent.none);
                        break;
                    case AnimatorControllerParameterType.Int:
                        EditorGUI.PropertyField(fieldRect, intValue, GUIContent.none);
                        break;
                }
            }
            EditorGUI.PropertyField(parameterRect, source, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Vector2FSMParameterField)), CustomPropertyDrawer(typeof(Vector3FSMParameterField))]
    public class VectorInputFSMParameterDrawer : PropertyDrawer {
        private static readonly GUIContent _xLabel = new("X");
        private static readonly GUIContent _yLabel = new("Y");
        private static readonly GUIContent _zLabel = new("Z");
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var x = property.FindPropertyRelative("_x");
            var y = property.FindPropertyRelative("_y");
            var z = property.FindPropertyRelative("_z");
            var columns = (x != null ? 1 : 0) + (y != null ? 1 : 0) + (z != null ? 1 : 0);

            if (columns > 0) {
                EditorGUI.BeginProperty(position, label, property);
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                var columnWidth = position.width / columns;
                var columnRect = new Rect(position) { width = columnWidth };
                var paramRect = new Rect(columnRect) { x = position.x + 16f, width = columnWidth - 20f };
                var labelRect = new Rect(columnRect) { width = 16f };

                if (x != null) {
                    EditorGUI.LabelField(labelRect, _xLabel);
                    EditorGUI.PropertyField(paramRect, x, GUIContent.none);
                    labelRect.x += columnWidth;
                    paramRect.x += columnWidth;
                }
                
                if (y != null) {
                    EditorGUI.LabelField(labelRect, _yLabel);
                    EditorGUI.PropertyField(paramRect, y, GUIContent.none);
                    labelRect.x += columnWidth;
                    paramRect.x += columnWidth;
                }
                
                if (z != null) {
                    EditorGUI.LabelField(labelRect, _zLabel);
                    EditorGUI.PropertyField(paramRect, z, GUIContent.none);
                }

                EditorGUI.EndProperty();
            }
        }
    }

    [CustomPropertyDrawer(typeof(OutputFSMParameter))]
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
            Rect position, Func<Enum, bool> selectModes, OutputFSMParameter.ApplyMode defaultMode
        ) {
            var parameterRect = new Rect(position) { width = position.width * 0.5f };
            EditorGUI.PropertyField(parameterRect, _parameter, GUIContent.none);

            var applyModeRect = new Rect(position) {
                x = position.x + parameterRect.width + 5f,
                width = position.width * 0.2f - 5f
            };
            
            var applyMode = (OutputFSMParameter.ApplyMode) _applyMode.intValue;
            if (!selectModes(applyMode)) {
                applyMode = defaultMode;
            }

            if (Application.isPlaying) {
                EditorGUI.LabelField(applyModeRect, applyMode.ToString());
            } else {
                applyMode = (OutputFSMParameter.ApplyMode) EditorGUI.EnumPopup(
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
                mode => (OutputFSMParameter.ApplyMode) mode != OutputFSMParameter.ApplyMode.Toggle,
                OutputFSMParameter.ApplyMode.Set
            );

            var setRect = new Rect(position) {
                x = position.x + rect.width + 5f,
                width = position.width - rect.width - 10f
            };

            if (_applyMode.intValue == (int) OutputFSMParameter.ApplyMode.FromParameter) {
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
                mode => (OutputFSMParameter.ApplyMode) mode == OutputFSMParameter.ApplyMode.Set ||
                        (OutputFSMParameter.ApplyMode) mode == OutputFSMParameter.ApplyMode.Unset,
                OutputFSMParameter.ApplyMode.Set
            );
        }
        
        private void DrawNumber(Rect position) {
            var rect = DrawParameterAndApplyMode(
                position,
                mode => (OutputFSMParameter.ApplyMode) mode != OutputFSMParameter.ApplyMode.Toggle,
                OutputFSMParameter.ApplyMode.Set
            );

            var setRect = new Rect(position) {
                x = position.x + rect.width + 5f,
                width = position.width - rect.width - 10f
            };
            
            switch ((OutputFSMParameter.ApplyMode) _applyMode.intValue) {
                case OutputFSMParameter.ApplyMode.Set:
                case OutputFSMParameter.ApplyMode.Add:
                case OutputFSMParameter.ApplyMode.Multiply:
                    if (Application.isPlaying) {
                        switch ((AnimatorControllerParameterType) _type.intValue) {
                            case AnimatorControllerParameterType.Float:
                                EditorGUI.LabelField(setRect, _value.floatValue.ToString());
                                break;
                            case AnimatorControllerParameterType.Int:
                                EditorGUI.LabelField(setRect, _value.intValue.ToString());
                                break;
                        }
                    } else {
                        EditorGUI.PropertyField(setRect, _value, GUIContent.none);
                    }
                    break;
                case OutputFSMParameter.ApplyMode.FromParameter:
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
        
        private static readonly FSMParameterCondition.ConditionMode[] _floatModes = {
            FSMParameterCondition.ConditionMode.Greater,
            FSMParameterCondition.ConditionMode.Less
        };
        private static readonly string[] _floatModeLabels = _floatModes.Select(mode => mode.ToString()).ToArray();

        private void DrawFloatCondition(Rect position) {
            var mode = (FSMParameterCondition.ConditionMode) _mode.intValue;
            var modeRect = new Rect(position) { width = position.width * 0.5f - 5f };
            var modeIndex = Math.Max(Array.FindIndex(IntModes, intMode => intMode == mode), 0);
            var thresholdRect = new Rect(modeRect) { x = position.x + modeRect.width + 0.5f };

            EditorGUI.BeginChangeCheck();
            modeIndex = EditorGUI.Popup(modeRect, modeIndex, _floatModeLabels);
            var threshold = EditorGUI.FloatField(thresholdRect, _threshold.floatValue);
            if (EditorGUI.EndChangeCheck()) {
                _mode.intValue = (int) _floatModes[modeIndex];
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