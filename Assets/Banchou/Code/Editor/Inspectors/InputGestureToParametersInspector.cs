using UnityEngine;
using UnityEditor;

namespace Banchou.Pawn.FSM {
    [CustomEditor(typeof(InputGestureToParameters))]
    public class InputGestureToParametersInspector : Editor {
        private SerializedProperty _comments;
        private SerializedProperty _inputSequence;
        private SerializedProperty _inputLifetime;
        private SerializedProperty _overrideGesture;
        private SerializedProperty _conditions;
        private SerializedProperty _inNormalizedTime;
        private SerializedProperty _acceptFromTime;
        private SerializedProperty _acceptUntilTime;
        private SerializedProperty _bufferUntilTime;
        private SerializedProperty _output;
        private SerializedProperty _breakOnGesture;
        private SerializedProperty _breakOnAccept;
        private SerializedProperty _breakOnCommand;
        
        private void OnEnable() {
            _comments = serializedObject.FindProperty("_comments");
            _inputSequence = serializedObject.FindProperty("_inputSequence");
            _inputLifetime = serializedObject.FindProperty("_inputLifetime");
            _overrideGesture = serializedObject.FindProperty("_overrideGesture");
            _conditions = serializedObject.FindProperty("_conditions");
            _inNormalizedTime = serializedObject.FindProperty("_inNormalizedTime");
            _acceptFromTime = serializedObject.FindProperty("_acceptFromTime");
            _acceptUntilTime = serializedObject.FindProperty("_acceptUntilTime");
            _bufferUntilTime = serializedObject.FindProperty("_bufferUntilTime");
            _output = serializedObject.FindProperty("_output");
            _breakOnGesture = serializedObject.FindProperty("_breakOnGesture");
            _breakOnAccept = serializedObject.FindProperty("_breakOnAccept");
            _breakOnCommand = serializedObject.FindProperty("_breakOnCommand");
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_comments);
            EditorGUILayout.PropertyField(_inputSequence);
            EditorGUILayout.PropertyField(_inputLifetime);
            EditorGUILayout.PropertyField(_overrideGesture);
            EditorGUILayout.PropertyField(_conditions);
            EditorGUILayout.PropertyField(_inNormalizedTime);
            
            var fromTime = _acceptFromTime.floatValue;
            var untilTime = _acceptUntilTime.floatValue;
            if (_inNormalizedTime.boolValue) {
                EditorGUILayout.MinMaxSlider(ref fromTime, ref untilTime, 0f, 1f);
                fromTime = Mathf.Clamp(EditorGUILayout.FloatField("Accept From Time", fromTime), 0f, untilTime);
                untilTime = Mathf.Clamp(EditorGUILayout.FloatField("Accept Until Time", untilTime), fromTime, 1f);
                EditorGUILayout.Slider(_bufferUntilTime, 0f, 1f);
            } else {
                fromTime = Mathf.Max(0f, EditorGUILayout.FloatField("Accept From Time", fromTime));
                untilTime = Mathf.Max(0f, EditorGUILayout.FloatField("Accept Until Time", untilTime));
                EditorGUILayout.PropertyField(_bufferUntilTime);
            }
            
            EditorGUILayout.PropertyField(_output);
            
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_breakOnGesture);
            EditorGUILayout.PropertyField(_breakOnAccept);
            EditorGUILayout.PropertyField(_breakOnCommand);
            
            if (EditorGUI.EndChangeCheck()) {
                _acceptFromTime.floatValue = fromTime;
                _acceptUntilTime.floatValue = untilTime;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }
    }
}