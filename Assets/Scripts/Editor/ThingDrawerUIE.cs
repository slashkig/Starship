using Extensions.Enums;
using UnityEditor;
using UnityEngine;

// ThingDrawer
[CustomPropertyDrawer(typeof(Thing))]
public class ThingDrawerUIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var amountRect = new Rect(position.x - 90, position.y, 90, position.height);
        var typeRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("enumtype"), GUIContent.none);
        EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("number"), GUIContent.none);

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}