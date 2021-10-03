using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Banchou.Pawn.Part {
    [CustomEditor(typeof(HitVolume))]
    public class HitVolumeInspector : Editor {
        private Transform _target;
        private SerializedProperty _damage;
        private SerializedProperty _hitStun;
        private SerializedProperty _hitLag;
        private SerializedProperty _knockback;
        private SerializedProperty _recoil;

        private void OnEnable() {
            _target = ((HitVolume)target).transform;
            _damage = serializedObject.FindProperty("<Damage>k__BackingField");
            _hitStun = serializedObject.FindProperty("<HitStun>k__BackingField");
            _hitLag = serializedObject.FindProperty("<HitLag>k__BackingField");
            _knockback = serializedObject.FindProperty("_knockback");
            _recoil = serializedObject.FindProperty("_recoil");
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_damage);
            EditorGUILayout.PropertyField(_hitStun);
            EditorGUILayout.PropertyField(_hitLag);
            EditorGUILayout.PropertyField(_knockback);
            EditorGUILayout.PropertyField(_recoil);
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void OnSceneGUI() {
            EditorGUI.BeginChangeCheck();
            var newKnockback = OffsetHandle("Knockback", _knockback.vector3Value, Color.red, Color.blue); 
            if (EditorGUI.EndChangeCheck()) {
                _knockback.vector3Value = newKnockback;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            var newRecoil = OffsetHandle("Recoil", _recoil.vector3Value, Color.yellow, Color.blue);
            if (EditorGUI.EndChangeCheck()) {
                _recoil.vector3Value = newRecoil;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private Vector3 OffsetHandle(string name, Vector3 offset, Color offsetColor, Color baseColor) {
            var position = _target.position;
            var rotation = offset == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(offset);
            
            var handlePosition = offset + position;
            var groundProjection = new Vector3(handlePosition.x, 0f, handlePosition.z);

            Handles.color = offsetColor;
            Handles.DrawLine(position, handlePosition, 1f);
            Handles.DrawLine(groundProjection, handlePosition, 1f);

            Handles.color = baseColor;
            Handles.DrawWireDisc(groundProjection, Vector3.up, 0.1f);

            Handles.color = Color.white;
            Handles.Label(handlePosition, name);

            return Handles.PositionHandle(offset + position, rotation) - position;
        }
    }
}