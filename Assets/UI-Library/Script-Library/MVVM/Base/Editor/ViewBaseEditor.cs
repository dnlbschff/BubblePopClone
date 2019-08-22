using System.Collections;
using System.Collections.Generic;
using DB.Library.MVVM;
using UnityEditor;
using UnityEngine;

namespace DB.Library
{
    /// <summary>
    /// Custom editor for <see cref="ViewBase{VM}"/> providing switching between
    /// auto-updating and manual updating in the editor.
    /// </summary>
    [CustomEditor(typeof(ViewBase<>), true)]
    public class ViewBaseEditor : CustomEditorHelper
    {
        private Object _obj;
        private IView _view;
        private bool isAutoUpdateEnabled = true;
        //private Component linkedView = null;

        //----------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            _obj = serializedObject.targetObject;
            _view = _obj as IView;

            HideScriptField = true;
            base.OnInspectorGUI();

            if (isAutoUpdateEnabled && GUI.changed)
            {
                UpdateView(_obj, _view);
            }
        }

        //----------------------------------------------------------------------
        protected override void OnBeforeDefaultEditor()
        {
            // HEADER
//            EditorGUILayout.LabelField("View Model", EditorStyles.boldLabel);
//
//            // VIEW MODEL LINKING
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Link a View with a ViewModel:");
//
//            // OBJECT FIELD
//            Component linkedView = null;
//            linkedView = (Component)EditorGUILayout.ObjectField(linkedView, typeof(Component), allowSceneObjects: true);
//            if (linkedView != null)
//            {
//                var view = linkedView as IView;
////                _view.IViewModel = view.IViewModel;
//            }
//
//            // BREAK LINK BUTTON
//            if (GUILayout.Button("Break Link"))
//            {
////                _view.IViewModel = null;
//            }
//            EditorGUILayout.EndHorizontal();
//
//            // AUTO-UPDATING
//            EditorGUILayout.BeginHorizontal();
//            isAutoUpdateEnabled = 
//                GUILayout.Toggle(isAutoUpdateEnabled, "Auto-Update View");
//
//            if (!isAutoUpdateEnabled)
//            {
//                if (GUILayout.Button("Update View"))
//                {
//                    UpdateView(_obj, _view);
//                }
//            }
//            EditorGUILayout.EndHorizontal();
        }

        //----------------------------------------------------------------------
        private static void UpdateView(Object obj, IView view)
        {
//            if (view.IViewModel == null) return;
//            view.IViewModel.NotifyViews();
            EditorUtility.SetDirty(obj);
        }

        //----------------------------------------------------------------------
    }
}