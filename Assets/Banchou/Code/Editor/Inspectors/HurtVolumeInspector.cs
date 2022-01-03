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
        private SerializedProperty _knockback;
        private SerializedProperty _recoil;
        private SerializedProperty _onHit;
        
        private void OnEnable() {
            _target = ((HurtVolume)target).transform;
            _interval = serializedObject.FindProperty("<Interval>k__BackingField");
            _damage = serializedObject.FindProperty("<Damage>k__BackingField");
            _hitStun = serializedObject.FindProperty("<HitStun>k__BackingField");
            _hitPause = serializedObject.FindProperty("<HitPause>k__BackingField");
            _knockback = serializedObject.FindProperty("_knockback");
            _recoil = serializedObject.FindProperty("_recoil");
            _onHit = serializedObject.FindProperty("_onHit");
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_damage);
            EditorGUILayout.PropertyField(_hitStun);
            EditorGUILayout.PropertyField(_hitPause);
            EditorGUILayout.PropertyField(_knockback);
            EditorGUILayout.PropertyField(_recoil);
            EditorGUILayout.PropertyField(_onHit, true);

            if (EditorGUI.EndChangeCheck()) {
                if (_hitPause.floatValue > _interval.floatValue) {
                    _hitPause.floatValue = _interval.floatValue;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI() {
            EditorGUI.BeginChangeCheck();
            _knockback.vector3Value = OffsetHandle(
                "Knockback", _knockback.vector3Value, Color.red, Color.blue
            );
            
            _recoil.vector3Value = OffsetHandle(
                "Recoil", _recoil.vector3Value, Color.yellow, Color.blue
            );
            
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private Vector3 OffsetHandle(string handleLabel, Vector3 offset, Color offsetColor, Color baseColor) {
            var position = _target.position;
            var handlePosition = offset;
            var handleRotation = Quaternion.identity;
            
            var parent = _target.parent;
            if (parent != null) {
                handlePosition = parent.TransformVector(offset);
                handleRotation = parent.rotation;
            }
            handlePosition += position;
            
            var groundProjection = new Vector3(handlePosition.x, 0f, handlePosition.z);
            
            Handles.color = offsetColor;
            Handles.DrawLine(position, handlePosition, 1f);

            Handles.DrawLine(groundProjection, handlePosition, 1f);
            Handles.DrawWireDisc(handlePosition - offset * 0.1f, offset, 0.1f);

            Handles.color = baseColor;
            Handles.DrawWireDisc(groundProjection, Vector3.up, 0.1f);
            
            Handles.Label(handlePosition, handleLabel);

            handlePosition = Handles.PositionHandle(handlePosition, handleRotation) - position;
            if (parent == null) {
                return handlePosition;
            }
            return parent.InverseTransformVector(handlePosition);
        }
    }
}