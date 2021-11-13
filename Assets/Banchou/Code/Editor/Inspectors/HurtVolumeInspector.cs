using UnityEngine;
using UnityEditor;

namespace Banchou.Pawn.Part {
    [CustomEditor(typeof(HurtVolume))]
    public class HurtVolumeInspector : Editor {
        private Transform _target;
        private SerializedProperty _interval;
        private SerializedProperty _damage;
        private SerializedProperty _hitStun;
        private SerializedProperty _hitPause;
        private SerializedProperty _applyKnockback;
        private SerializedProperty _knockbackScale;
        private SerializedProperty _knockbackDirection;
        private SerializedProperty _applyRecoil;
        private SerializedProperty _recoilScale;
        private SerializedProperty _recoilDirection;

        private void OnEnable() {
            _target = ((HurtVolume)target).transform;
            _interval = serializedObject.FindProperty("<Interval>k__BackingField");
            _damage = serializedObject.FindProperty("<Damage>k__BackingField");
            _hitStun = serializedObject.FindProperty("<HitStun>k__BackingField");
            _hitPause = serializedObject.FindProperty("<HitPause>k__BackingField");
            _applyKnockback = serializedObject.FindProperty("_applyKnockback");
            _knockbackScale = serializedObject.FindProperty("_knockbackScale");
            _knockbackDirection = serializedObject.FindProperty("_knockbackDirection");
            _applyRecoil = serializedObject.FindProperty("_applyRecoil");
            _recoilScale = serializedObject.FindProperty("_recoilScale");
            _recoilDirection = serializedObject.FindProperty("_recoilDirection");
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_damage);
            EditorGUILayout.PropertyField(_hitStun);
            EditorGUILayout.PropertyField(_hitPause);
            EditorGUILayout.PropertyField(_applyKnockback);
            
            if (_applyKnockback.boolValue) {
                EditorGUILayout.PropertyField(_knockbackScale);
                EditorGUILayout.PropertyField(_knockbackDirection);
            }

            EditorGUILayout.PropertyField(_applyRecoil);
            if (_applyRecoil.boolValue) {
                EditorGUILayout.PropertyField(_recoilScale);
                EditorGUILayout.PropertyField(_recoilDirection);
            }

            if (EditorGUI.EndChangeCheck()) {
                if (_hitPause.floatValue > _interval.floatValue) {
                    _hitPause.floatValue = _interval.floatValue;
                }
                
                _knockbackDirection.vector3Value.Normalize();
                _recoilDirection.vector3Value.Normalize();

                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI() {
            EditorGUI.BeginChangeCheck();
            if (_applyKnockback.boolValue) {
                _knockbackDirection.vector3Value = Vector3.ClampMagnitude(
                    OffsetHandle(
                        "Knockback", _knockbackDirection.vector3Value, Color.red, Color.blue
                    ),
                    1f
                );
            }

            if (_applyRecoil.boolValue) {
                _recoilDirection.vector3Value = Vector3.ClampMagnitude(
                    OffsetHandle(
                        "Recoil", _recoilDirection.vector3Value, Color.yellow, Color.blue
                    ),
                    1f
                );
            }

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private Vector3 OffsetHandle(string handleLabel, Vector3 offset, Color offsetColor, Color baseColor) {
            var position = _target.position;
            var handlePosition = offset + position;
            var groundProjection = new Vector3(handlePosition.x, 0f, handlePosition.z);

            Handles.color = offsetColor;
            Handles.DrawLine(position, handlePosition, 1f);
            Handles.DrawLine(groundProjection, handlePosition, 1f);
            Handles.DrawWireDisc(handlePosition - offset * 0.1f, offset, 0.1f);

            Handles.color = baseColor;
            Handles.DrawWireDisc(groundProjection, Vector3.up, 0.1f);

            Handles.color = Color.white;
            Handles.Label(handlePosition, handleLabel);
            
            return (Handles.PositionHandle(offset + position, Quaternion.identity) - position);
        }
    }
}