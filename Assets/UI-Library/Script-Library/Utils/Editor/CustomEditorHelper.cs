using UnityEditor;

namespace DB.Library
{
    public class CustomEditorHelper : Editor
    {
        protected bool HideScriptField = false;

        public override void OnInspectorGUI()
        {
            OnBeforeDefaultEditor();
            if (HideScriptField)
            {
                DrawPropertiesExcluding(serializedObject, new string[] {"m_Script"});
            }
            else
            {
                DrawDefaultInspector();
            }

            OnAfterDefaultEditor();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnBeforeDefaultEditor()
        {
        }

        protected virtual void OnAfterDefaultEditor()
        {
        }
    }
}