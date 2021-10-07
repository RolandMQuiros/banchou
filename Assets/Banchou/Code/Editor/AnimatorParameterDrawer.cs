using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AnimatorParameterAttribute))]
public class AnimatorParameterDrawer : PropertyDrawer {
    private static Type _animatorWindowType;
    private static FieldInfo _animatorField;
    private static FieldInfo _controllerField;
    

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String) return;

        _animatorWindowType ??= Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
        if (_animatorWindowType == null) return;
        
        _animatorField ??=
            _animatorWindowType.GetField("m_PreviewAnimator", BindingFlags.Instance | BindingFlags.NonPublic);
        _controllerField ??=
            _animatorWindowType.GetField("m_AnimatorController", BindingFlags.Instance | BindingFlags.NonPublic);

        var window = EditorWindow.GetWindow(_animatorWindowType);
        var animator = _animatorField.GetValue(window) as Animator; // May be null
        var controller = _controllerField.GetValue(window) as UnityEditor.Animations.AnimatorController;
        

        // // First get the attribute since it contains the range for the slider
        // RangeAttribute range = attribute as RangeAttribute;
        //
        // // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
        // if (property.propertyType == SerializedPropertyType.Float)
        //     EditorGUI.Slider(position, property, range.min, range.max, label);
        // else if (property.propertyType == SerializedPropertyType.Integer)
        //     EditorGUI.IntSlider(position, property, Convert.ToInt32(range.min), Convert.ToInt32(range.max), label);
        // else
        //     EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
    }
}
