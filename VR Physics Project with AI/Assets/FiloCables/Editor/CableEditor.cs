using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Filo{
    
    [CustomEditor(typeof(Cable)), CanEditMultipleObjects] 
    public class CableEditor : Editor
    {

        [MenuItem("GameObject/Filo Cables/Cable", false, 10)]
        static void CreateCable(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Cable");
            go.AddComponent<Cable>();
            go.AddComponent<CableRenderer>();

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/Filo Cables/Cable Solver", false, 10)]
        static void CreateSolver(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Solver");
            go.AddComponent<CableSolver>();

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        private ReorderableList list;
        
        public void OnEnable(){

            list = new ReorderableList(serializedObject, 
                                       serializedObject.FindProperty("links"), 
                                       true, true, true, true);

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
					EditorGUI.PropertyField(rect, list.serializedProperty.GetArrayElementAtIndex(index));
            };

            list.elementHeightCallback = (index) => 
            {
				float propertyHeight = EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true);
				float spacing = EditorGUIUtility.singleLineHeight / 2;
				return propertyHeight + spacing;
            };

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Links");
            };

        }
        
        public override void OnInspectorGUI() {
            
            serializedObject.UpdateIfRequiredOrScript();
            
            Editor.DrawPropertiesExcluding(serializedObject,"m_Script", "links");

            list.DoLayoutList();
            
            // Apply changes to the serializedProperty
            if (GUI.changed){
                serializedObject.ApplyModifiedProperties();                
            }
            
        }
        
    }

}


