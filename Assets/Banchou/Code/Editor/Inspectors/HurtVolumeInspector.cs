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
        private SerializedProperty _attackPause;

        private SerializedProperty _knockbackMethod;
        private SerializedProperty _knockback;
        private SerializedProperty _knockbackMagnitude;
        private SerializedProperty _additionalKnockback;
        
        private SerializedProperty _recoilMethod;
        private SerializedProperty _recoil;
        private SerializedProperty _recoilMagnitude;
        private SerializedProperty _additionalRecoil;

        private SerializedProperty _lockOffOnConfirm;
        private SerializedProperty _onHit;

        private string BackingField(string fieldName) => $"<{fieldName}>k__BackingField";

        private void OnEnable() {
            _target = ((HurtVolume)target).transform;
            
            _hurtFriendly = serializedObject.FindProperty(BackingField(nameof(HurtVolume.HurtFriendly)));
            _hurtHostile = serializedObject.FindProperty(BackingField(nameof(HurtVolume.HurtHostile)));
            _isGrab = serializedObject.FindProperty(BackingField(nameof(HurtVolume.IsGrab)));
            
            _interval = serializedObject.FindProperty(BackingField(nameof(HurtVolume.Interval)));
            _damage = serializedObject.FindProperty(BackingField(nameof(HurtVolume.Damage)));
            _hitStun = serializedObject.FindProperty(BackingField(nameof(HurtVolume.HitStun)));
            _hitPause = serializedObject.FindProperty(BackingField(nameof(HurtVolume.HitPause)));
            _attackPause = serializedObject.FindProperty(BackingField(nameof(HurtVolume.AttackPause)));
            
            _attackPause = serializedObject.FindProperty(BackingField(nameof(HurtVolume.AttackPause)));
            _knockbackMethod = serializedObject.FindProperty(BackingField(nameof(HurtVolume.KnockbackMethod)));
            _knockback = serializedObject.FindProperty("_knockback");
            _knockbackMagnitude = serializedObject.FindProperty(BackingField(nameof(HurtVolume.KnockbackMagnitude)));
            _additionalKnockback = serializedObject.FindProperty(BackingField(nameof(HurtVolume.AdditionalKnockback)));
            
            _recoilMethod = serializedObject.FindProperty(BackingField(nameof(HurtVolume.RecoilMethod)));
            _recoil = serializedObject.FindProperty("_recoil");
            _recoilMagnitude = serializedObject.FindProperty(BackingField(nameof(HurtVolume.RecoilMagnitude)));
            _additionalRecoil = serializedObject.FindProperty(BackingField(nameof(HurtVolume.AdditionalRecoil)));
            
            _lockOffOnConfirm = serializedObject.FindProperty("<LockOffOnConfirm>k__BackingField");
            _onHit = serializedObject.FindProperty("_onHit");
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Volume Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hurtFriendly);
            EditorGUILayout.PropertyField(_hurtHostile);
            EditorGUILayout.PropertyField(_isGrab);
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_damage);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Defender Reactions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hitStun);
            EditorGUILayout.PropertyField(_hitPause);
            EditorGUILayout.PropertyField(_knockbackMethod);
            switch ((HurtVolume.ForceMethod) _knockbackMethod.enumValueIndex) {
                case HurtVolume.ForceMethod.ForwardRelative:
                    EditorGUILayout.PropertyField(_knockback);
                    break;
                case HurtVolume.ForceMethod.Contact:
                case HurtVolume.ForceMethod.ContactUpProjected:
                    EditorGUILayout.PropertyField(_knockbackMagnitude);
                    EditorGUILayout.PropertyField(_additionalKnockback);
                    break;
            }
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Attacker Reactions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_attackPause);
            EditorGUILayout.PropertyField(_lockOffOnConfirm);
            EditorGUILayout.PropertyField(_recoilMethod);
            switch ((HurtVolume.ForceMethod) _recoilMethod.enumValueIndex) {
                case HurtVolume.ForceMethod.ForwardRelative:
                    EditorGUILayout.PropertyField(_recoil);
                    break;
                case HurtVolume.ForceMethod.Contact:
                    EditorGUILayout.PropertyField(_recoilMagnitude);
                    EditorGUILayout.PropertyField(_additionalRecoil);
                    break;
            }
            
            EditorGUILayout.PropertyField(_onHit, true);

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI() {
            EditorGUI.BeginChangeCheck();

            if (_knockbackMethod.enumValueIndex == (int) HurtVolume.ForceMethod.ForwardRelative) {
                _knockback.vector3Value = OffsetHandle(
                    "Knockback", _knockback.vector3Value, Color.red, Color.blue
                );
            }

            if (_recoilMethod.enumValueIndex == (int) HurtVolume.ForceMethod.ForwardRelative) {
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