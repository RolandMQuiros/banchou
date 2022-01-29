using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DevCommentAttribute))]
public class DevCommentDrawer : PropertyDrawer {
    private static readonly GUIContent EditIcon = EditorGUIUtility.IconContent("d_editicon.sml");
    private static readonly GUIStyle BodyStyle = new(GUI.skin.label) {
        richText = true,
        alignment = TextAnchor.UpperLeft,
        stretchWidth = false,
        stretchHeight = true,
        wordWrap = true
    };
    private static readonly GUIStyle EmptyLabelStyle = new(GUI.skin.label) {
        alignment = TextAnchor.UpperRight
    };
    private static readonly Color EditBgColor = new(0.95f, 0.95f, 0.95f);
    private static readonly float MaxContentHeight = EditorGUIUtility.singleLineHeight * 5f + 
                                                     EditorGUIUtility.standardVerticalSpacing * 5f;
    private bool _isEditing;
    private Vector2 _scroll;
    private float _contentHeight;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (_isEditing) {
            return MaxContentHeight;
        }
        return Mathf.Min(MaxContentHeight, _contentHeight + EditorGUIUtility.singleLineHeight);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        
        var hasComment = !string.IsNullOrEmpty(property.stringValue);
        if (hasComment && Event.current.type == EventType.Repaint) {
            var commentContent = new GUIContent(property.stringValue);
            _contentHeight = BodyStyle.CalcHeight(commentContent, position.width - 32f);
        }
        
        var editRect = new Rect(position) {
            x = position.x + position.width - 32f,
            width = 32f,
            height = EditorGUIUtility.singleLineHeight
        };
        _isEditing = EditorGUI.ToggleLeft(editRect, EditIcon, _isEditing);

        var headerRect = new Rect(position) {
            width = position.width - editRect.width - 5f,
            height = EditorGUIUtility.singleLineHeight
        };
        
        if (!hasComment && !_isEditing) {
            EditorGUI.LabelField(headerRect, label, EmptyLabelStyle);
        }

        var outerRect = new Rect(headerRect) { height = position.height - headerRect.height };
        var innerRect = new Rect(outerRect) { width = outerRect.width - 32f };
        if (_isEditing) {
            EditorGUI.BeginChangeCheck();
            EditorGUI.DrawRect(outerRect, EditBgColor);
            var commentBody = EditorGUI.TextArea(innerRect, property.stringValue, BodyStyle);
            if (EditorGUI.EndChangeCheck()) {
                property.stringValue = commentBody.Trim();
            }
        } else {
            innerRect.height = _contentHeight;
            _scroll = GUI.BeginScrollView(outerRect, _scroll, innerRect, false, false);
            EditorGUI.SelectableLabel(innerRect, property.stringValue, BodyStyle);
            GUI.EndScrollView();
        }

        EditorGUI.EndProperty();
    }
}