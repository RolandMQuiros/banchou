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
    private static readonly Color EditBgColor = new(0.95f, 0.95f, 0.95f);
    private static readonly float MaxContentHeight = EditorGUIUtility.singleLineHeight * 4f + 
                                                     EditorGUIUtility.standardVerticalSpacing * 4f;

    private bool _isExpanded = true;
    private bool _isEditing;
    private Vector2 _scroll;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        
        var hasComment = !string.IsNullOrEmpty(property.stringValue);
        var headerRect = new Rect(position) {
            width = position.width - 64f,
            height = EditorGUIUtility.singleLineHeight
        };

        if (hasComment && !_isEditing) {
            _isExpanded = EditorGUI.Foldout(headerRect, _isExpanded, label);
        } else {
            EditorGUI.LabelField(headerRect, label);
        }

        var editRect = new Rect(position) {
            x = position.x + position.width - 32f,
            width = 32f,
            height = EditorGUIUtility.singleLineHeight
        };

        _isExpanded |= _isEditing = EditorGUI.ToggleLeft(editRect, EditIcon, _isEditing);

        if (_isExpanded) {
            var labelContent = new GUIContent(property.stringValue);
            if (_isEditing) {
                EditorGUI.BeginChangeCheck();
                var layoutRect = GUILayoutUtility.GetRect(position.width, MaxContentHeight);
                
                EditorGUI.DrawRect(layoutRect, EditBgColor);
                var commentBody = EditorGUI.TextArea(layoutRect, property.stringValue, BodyStyle);
                if (EditorGUI.EndChangeCheck()) {
                    property.stringValue = commentBody;
                }
            } else {
                var bodyRect = new Rect(position) {
                    y = position.y + headerRect.height + EditorGUIUtility.standardVerticalSpacing,
                    height = position.height - headerRect.height - EditorGUIUtility.standardVerticalSpacing 
                };
                var innerRect = new Rect(bodyRect) { width = bodyRect.width - 64f };
                innerRect.height = BodyStyle.CalcHeight(labelContent, innerRect.width);
                
                // I don't understand how this is getting the initial position
                var layoutRect = GUILayoutUtility.GetRect(
                    innerRect.width,
                    Mathf.Min(MaxContentHeight, innerRect.height + EditorGUIUtility.standardVerticalSpacing)
                );
                
                _scroll = GUI.BeginScrollView(layoutRect, _scroll, innerRect, false, false);
                EditorGUI.SelectableLabel(innerRect, property.stringValue, BodyStyle);
                GUI.EndScrollView();
            }
        }
        
        EditorGUI.EndProperty();
    }
}