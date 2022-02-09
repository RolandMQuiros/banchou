using UnityEngine;
using UnityEditor;

namespace Banchou.Pawn.Part {
    [CustomEditor(typeof(HurtVolume))]
    public class HurtVolumeInspector : Editor {
        private Transform _target;

        private SerializedProperty _hurtFriendly;
        private SerializedProperty _hurtHostile;
        private SerializedProperty _isGrab;
        
        private SerializedProperty _interval;
        private SerializedProperty _damage;
        private SerializedProperty _hitStun;
        private SerializedProperty _hitPause;

        private SerializedProperty _knockbackMethod;
        private SerializedProperty _knockback;
        private SerializedProperty _knockbackMagnitude;
        
        private SerializedProperty _recoilMethod;
        private SerializedProperty _recoil;
        private SerializedProperty _recoilMagnitude;
        
        private SerializedProperty _lockOffOnConfirm;
        private SerializedProperty _onHit;

        private void OnEnable() {
            _target = ((HurtVolume)target).transform;
            
            _hurtFriendly = serializedObject.FindProperty("<HurtFriendly>k__BackingField");
            _hurtHostile = serializedObject.FindProperty("<HurtHostile>k__BackingField");
            _isGrab = serializedObject.FindProperty("<IsGrab>k__BackingField");
            
            _interval = serializedObject.FindProperty("<Interval>k__BackingField");
            _damage = serializedObject.FindProperty("<Damage>k__BackingField");
            _hitStun = serializedObject.FindProperty("<HitStun>k__BackingField");
            _hitPause = serializedObject.FindProperty("<HitPause>k__BackingField");
            
            _knockbackMethod = serializedObject.FindProperty("<KnockbackMethod>k__BackingField");
            _knockback = serializedObject.FindProperty("_knockback");
            _knockbackMagnitude = serializedObject.FindProperty("<KnockbackMagnitude>k__BackingField");
            
            _recoilMethod = serializedObject.FindProperty("<RecoilMethod>k__BackingField");
            _recoil = serializedObject.FindProperty("_recoil");
            _recoilMagnitude = serializedObject.FindProperty("<RecoilMagnitude>k__BackingField");
            
            _lockOffOnConfirm = serializedObject.FindProperty("<LockOffOnConfirm>k__BackingField");
            _onHit = serializedObject.FindProperty("_onHit");
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_hurtFriendly);
            EditorGUILayout.PropertyField(_hurtHostile);
            EditorGUILayout.PropertyField(_isGrab);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_damage);
            EditorGUILayout.PropertyField(_hitStun);
            EditorGUILayout.PropertyField(_hitPause);
            EditorGUILayout.PropertyField(_lockOffOnConfirm);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_knockbackMethod);

            switch ((HurtVolume.ForceMethod) _knockbackMethod.enumValueIndex) {
                case HurtVolume.ForceMethod.Static:
                    EditorGUILayout.PropertyField(_knockback);
                    break;
                case HurtVolume.ForceMethod.Contact:
                    EditorGUILayout.PropertyField(_knockbackMagnitude);
                    break;
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_recoilMethod);
            switch ((HurtVolume.ForceMethod) _recoilMethod.enumValueIndex) {
                case HurtVolume.ForceMethod.Static:
                    EditorGUILayout.PropertyField(_recoil);
                    break;
                case HurtVolume.ForceMethod.Contact:
                    EditorGUILayout.PropertyField(_recoilMagnitude);
                    break;
            }
            
            EditorGUILayout.PropertyField(_onHit, true);

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI() {
            EditorGUI.BeginChangeCheck();

            if (_knockbackMethod.enumValueIndex == (int) HurtVolume.ForceMethod.Static) {
                _knockback.vector3Value = OffsetHandle(
                    "Knockback", _knockback.vector3Value, Color.red, Color.blue
                );
            }

            if (_recoilMethod.enumValueIndex == (int) HurtVolume.ForceMethod.Static) {
                _recoil.vector3Value = OffsetHandle(
                    "Recoil", _recoil.vector3Value, Color.yellow, Color.blue
                );
            }

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