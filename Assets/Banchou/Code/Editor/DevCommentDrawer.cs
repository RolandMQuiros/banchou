using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DevCommentAttribute))]
public class DevCommentDrawer : PropertyDrawer {
    private static readonly GUIContent EditIcon = EditorGUIUtility.IconContent("d_editicon.sml");
    private static readonly GUIStyle BodyStyle = new(GUI.skin.label) {
        richText = true,
        alignment = TextAnchor.UpperLeft,
        stretchHeight = true,
        wordWrap = true
    };
    private static readonly Color BodyBgColor = new(0.95f, 0.95f, 0.95f);
    
    private bool _isExpanded;
    private bool _isEditing;
    private Vector2 _scroll;
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (_isExpanded) {
            return EditorGUIUtility.singleLineHeight * 5f + EditorGUIUtility.standardVerticalSpacing * 5f;
        }
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        var headerRect = new Rect(position) {
            width = position.width - 64f,
            height = EditorGUIUtility.singleLineHeight
        };
        _isExpanded = EditorGUI.Foldout(headerRect, _isExpanded, label);
        
        var editRect = new Rect(position) {
            x = position.x + position.width - 64f,
            width = 64f,
            height = EditorGUIUtility.singleLineHeight
        };
        if (GUI.Button(editRect, EditIcon)) {
            _isEditing = !_isEditing;
            _isExpanded |= _isEditing;
        }

        if (_isExpanded) {
            var bodyRect = new Rect(position) {
                y = position.y + headerRect.height + EditorGUIUtility.standardVerticalSpacing,
                height = position.height - headerRect.height - EditorGUIUtility.standardVerticalSpacing 
            };
            
            if (_isEditing) {
                EditorGUI.BeginChangeCheck();
                var commentBody = EditorGUI.TextArea(bodyRect, property.stringValue);
                if (EditorGUI.EndChangeCheck()) {
                    property.stringValue = commentBody;
                }
            } else {
                var labelContent = new GUIContent(property.stringValue);
                var innerRect = new Rect(bodyRect) { width = bodyRect.width - 64f };
                innerRect.height = BodyStyle.CalcHeight(labelContent, innerRect.width);

                EditorGUI.DrawRect(bodyRect, BodyBgColor);
                _scroll = GUI.BeginScrollView(bodyRect, _scroll, innerRect, false, false);
                EditorGUI.SelectableLabel(innerRect, property.stringValue, BodyStyle);
                GUI.EndScrollView();
            }
        }
        
        EditorGUI.EndProperty();
    }
}