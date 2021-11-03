using UnityEngine;
using UnityEditor;

namespace Banchou.Pawn.FSM {
    [CustomEditor(typeof(AcceptGestureToTrigger))]
    public class AcceptGestureToTriggerInspector : Editor {
        private SerializedProperty _inputSequence;
        private SerializedProperty _inputLifetime;
        private SerializedProperty _overrideGesture;
        private SerializedProperty _inNormalizedTime;
        private SerializedProperty _acceptFromTime;
        private SerializedProperty _acceptUntilTime;
        private SerializedProperty _resetOnEnter;
        private SerializedProperty _outputParameters;
        
        private void OnEnable() {
            _inputSequence = serializedObject.FindProperty("_inputSequence");
            _inputLifetime = serializedObject.FindProperty("_inputLifetime");
            _overrideGesture = serializedObject.FindProperty("_overrideGesture");
            _inNormalizedTime = serializedObject.FindProperty("_inNormalizedTime");
            _acceptFromTime = serializedObject.FindProperty("_acceptFromTime");
            _acceptUntilTime = serializedObject.FindProperty("_acceptUntilTime");
            _resetOnEnter = serializedObject.FindProperty("_resetOnEnter");
            _outputParameters = serializedObject.FindProperty("_outputParameters");
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_inputSequence);
            EditorGUILayout.PropertyField(_inputLifetime);
            EditorGUILayout.PropertyField(_overrideGesture);
            EditorGUILayout.PropertyField(_inNormalizedTime);
            
            var fromTime = _acceptFromTime.floatValue;
            var untilTime = _acceptUntilTime.floatValue;
            if (_inNormalizedTime.boolValue) {
                EditorGUILayout.MinMaxSlider(ref fromTime, ref untilTime, 0f, 1f);
                fromTime = Mathf.Clamp(EditorGUILayout.FloatField("Accept From Time", fromTime), 0f, untilTime);
                untilTime = Mathf.Clamp(EditorGUILayout.FloatField("Accept Until Time", untilTime), fromTime, 1f);
            } else {
                fromTime = Mathf.Max(0f, EditorGUILayout.FloatField("Accept From Time", fromTime));
                untilTime = Mathf.Max(0f, EditorGUILayout.FloatField("Accept Until Time", untilTime));
            }
            
            EditorGUILayout.PropertyField(_resetOnEnter);
            EditorGUILayout.PropertyField(_outputParameters);
            
            if (EditorGUI.EndChangeCheck()) {
                _acceptFromTime.floatValue = fromTime;
                _acceptUntilTime.floatValue = untilTime;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }
    }
}