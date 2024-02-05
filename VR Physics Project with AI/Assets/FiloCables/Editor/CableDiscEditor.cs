using UnityEditor;
using UnityEngine;

namespace Filo{
    
    [CustomEditor(typeof(CableDisc)), CanEditMultipleObjects] 
    public class CableDiscEditor : Editor
    {
        
        public override void OnInspectorGUI() {
            
            serializedObject.UpdateIfRequiredOrScript();
            
            Editor.DrawPropertiesExcluding(serializedObject,"m_Script");
            
            // Apply changes to the serializedProperty
            if (GUI.changed){
                serializedObject.ApplyModifiedProperties();                
            }
            
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void DrawGizmos(CableDisc disc, GizmoType gizmoType)
        {
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(disc.transform.position, disc.GetCablePlaneNormal(), disc.ScaledRadius);
        }
        
    }

}


