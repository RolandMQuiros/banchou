using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Banchou.Pawn.FSM {
    [CustomPropertyDrawer(typeof(ParameterCondition))]
    public class ParameterConditionDrawer : PropertyDrawer {
        private StateMachineBehaviour _behaviour;
        private AnimatorController _controller;

        private string[] _parameterNames;
        private List<int> _parameterHashes;
        
        private static readonly HashSet<ParameterCondition.ConditionMode> BoolModes = new() {
            ParameterCondition.ConditionMode.If,
            ParameterCondition.ConditionMode.IfNot
        };
        private static readonly HashSet<ParameterCondition.ConditionMode> FloatModes = new() {
            ParameterCondition.ConditionMode.Greater,
            ParameterCondition.ConditionMode.Less
        };
        private static readonly HashSet<ParameterCondition.ConditionMode> IntModes = new() {
            ParameterCondition.ConditionMode.Greater,
            ParameterCondition.ConditionMode.Less,
            ParameterCondition.ConditionMode.Equals,
            ParameterCondition.ConditionMode.NotEqual
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            _behaviour ??= property.serializedObject.targetObject as StateMachineBehaviour;
            if (_behaviour == null || Application.isPlaying) {
                return;
            }

            _controller ??= AnimatorController
                .FindStateMachineBehaviourContext(_behaviour)[0]
                .animatorController;
            _parameterNames = _controller.parameters
                .Select(parameter => parameter.name)
                .Prepend("None")
                .ToArray();
            _parameterHashes = _controller.parameters
                .Select(parameter => parameter.nameHash)
                .Prepend(0)
                .ToList();

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel = 0;
            
            var hash = property.FindPropertyRelative("_hash");
            var currentParameterIndex = _parameterHashes.IndexOf(hash.intValue);
            var currentParameter = _controller.parameters[currentParameterIndex - 1];

            var type = property.FindPropertyRelative("_parameterType");
            
            var mode = property.FindPropertyRelative("_mode");
            var currentMode = (ParameterCondition.ConditionMode)mode.intValue;
            
            var threshold = property.FindPropertyRelative("_threshold");
            
            var nameRect = new Rect(
                position.x,
                position.y,
                position.width * 0.4f,
                position.height
            );

            var modeRect = Rect.zero;
            var thresholdRect = Rect.zero;
            
            // Omit the threshold for triggers
            if (currentParameter.type != AnimatorControllerParameterType.Trigger) {
                modeRect = new Rect(
                    position.x + nameRect.width + 5f,
                    position.y,
                    100f,
                    position.height
                );
                thresholdRect = new Rect(
                    position.x + nameRect.width + modeRect.width + 10f,
                    position.y, position.width - nameRect.width - modeRect.width - 10f,
                    position.height
                );
            }

            EditorGUI.BeginChangeCheck();
            
            // Get parameter
            currentParameterIndex = EditorGUI.Popup(nameRect, currentParameterIndex, _parameterNames);
            currentParameter = _controller.parameters[currentParameterIndex - 1];

            // Only display mode and threshold if the parameter type allows
            if (currentParameter.type != AnimatorControllerParameterType.Trigger) {
                // Display modes based on parameter type
                HashSet<ParameterCondition.ConditionMode> modeSet = null;
                switch (currentParameter.type) {
                    case AnimatorControllerParameterType.Bool:
                        modeSet = BoolModes;
                        break;
                    case AnimatorControllerParameterType.Float:
                        modeSet = FloatModes;
                        break;
                    case AnimatorControllerParameterType.Int:
                        modeSet = IntModes;
                        break;
                }
                // If the current mode isn't in the mode set, pick the first available one
                if (modeSet?.Contains(currentMode) == false) {
                    currentMode = modeSet.First();
                }
                // Show the mode dropdown
                currentMode = (ParameterCondition.ConditionMode)
                    EditorGUI.EnumPopup(
                        modeRect,
                        GUIContent.none,
                        currentMode,
                        m => modeSet?.Contains((ParameterCondition.ConditionMode)m) == true
                    );
                
                EditorGUI.PropertyField(thresholdRect, threshold, GUIContent.none);
            }

            if (EditorGUI.EndChangeCheck()) {
                hash.intValue = _parameterHashes[currentParameterIndex];
                type.intValue = (int) currentParameter.type;
                mode.intValue = (int) currentMode;
            }
            
            EditorGUI.EndProperty();
        }
    }
}