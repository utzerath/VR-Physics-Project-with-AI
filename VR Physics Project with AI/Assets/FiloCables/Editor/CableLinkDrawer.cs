using UnityEditor;
using UnityEngine;

namespace Filo
{

    // IngredientDrawer
    [CustomPropertyDrawer(typeof(Cable.Link))]
    public class CableLinkDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            rect.y += 2;

            EditorGUI.BeginProperty(rect, label, property);

            SerializedProperty type = property.FindPropertyRelative("type");

            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight),
                type, GUIContent.none);

            EditorGUI.PropertyField(
                new Rect(rect.x + 82, rect.y, rect.width - 80, EditorGUIUtility.singleLineHeight),
                property.FindPropertyRelative("body"), GUIContent.none);

            if (type.enumValueIndex == (int)Cable.Link.LinkType.Attachment){

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2), rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("slack"), new GUIContent("Slack"));

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*2, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("inAnchor"), new GUIContent("In Anchor"));
    
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*3, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("outAnchor"), new GUIContent("Out Anchor"));

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*4, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("cableSpawnSpeed"), new GUIContent("Spawn Speed"));

            }else if (type.enumValueIndex == (int)Cable.Link.LinkType.Pinhole){

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2), rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("slack"), new GUIContent("Slack"));

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*2, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("inAnchor"), new GUIContent("In Anchor"));
    
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*3, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("outAnchor"), new GUIContent("Out Anchor"));
    
            }else{

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("orientation"), new GUIContent("Orientation"));

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2) * 2, rect.width, EditorGUIUtility.singleLineHeight),
                    property.FindPropertyRelative("slack"), new GUIContent("Slack"));

                if (type.enumValueIndex == (int)Cable.Link.LinkType.Hybrid){

                    var storedCable = property.FindPropertyRelative("storedCable");

                    /*storedCable.floatValue = Mathf.Max(0,EditorGUI.FloatField(
                        new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*3, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent("Stored Cable"), storedCable.floatValue));*/

                    EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*3, rect.width, EditorGUIUtility.singleLineHeight),
                                property.FindPropertyRelative("storedCable"), new GUIContent("Stored Cable"));

                    EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + 2)*4, rect.width, EditorGUIUtility.singleLineHeight),
                                property.FindPropertyRelative("spoolSeparation"), new GUIContent("Spool Separation"));

                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty type = property.FindPropertyRelative("type");
            if (type.enumValueIndex == (int)Cable.Link.LinkType.Attachment){
                return EditorGUIUtility.singleLineHeight*5+8;
            }else if (type.enumValueIndex == (int)Cable.Link.LinkType.Pinhole){
                return EditorGUIUtility.singleLineHeight*4+6;
            }else if (type.enumValueIndex == (int)Cable.Link.LinkType.Hybrid){
                return EditorGUIUtility.singleLineHeight*5+8;
            }else{
                return EditorGUIUtility.singleLineHeight*3+4;
            }
        }
    }
}