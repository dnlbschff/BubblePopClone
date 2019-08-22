using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject.Internal;

namespace Utils.Editor
{
    [InitializeOnLoad]
    public class GameLaunchHelper
    {
        private static readonly string PreviousScenePathKey =
            Application.productName + "GameLaunchHelper.PreviousScenePath";

        private static readonly string InitScenePathKey =
            Application.productName + "GameLaunchHelper.InitScenePathKey";

        static GameLaunchHelper()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private static string PreviousScenePath
        {
            get { return EditorPrefs.GetString(PreviousScenePathKey, string.Empty); }
            set { EditorPrefs.SetString(PreviousScenePathKey, value); }
        }

        private static string InitScenePath
        {
            get { return EditorPrefs.GetString(InitScenePathKey); }
            set { EditorPrefs.SetString(InitScenePathKey, value); }
        }

        [PreferenceItem("Game Launcher")]
        private static void InitScenePrefItem()
        {
            var initScenePath = EditorGUILayout.TextField(InitScenePath);
            if (GUI.changed)
            {
                InitScenePath = initScenePath;
            }
        }

        [MenuItem("Tools/Launch Game %l")]
        public static void RunInit()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                Application.Quit();
            }
            else
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    return;
                }

                if (!ZenjectGraphIsValid())
                {
                    return;
                }

                PreviousScenePath = SceneManager.GetActiveScene().path;
                if (PreviousScenePath != InitScenePath)
                {
                    EditorSceneManager.OpenScene(InitScenePath);
                }


                EditorApplication.isPlaying = true;
            }
        }

        private static void PlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                if (string.IsNullOrEmpty(PreviousScenePath)) return;

                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(PreviousScenePath);
                PreviousScenePath = string.Empty;
            }
        }

        private static bool ZenjectGraphIsValid()
        {
            return ValidateWrapper(() =>
            {
                var numValidated = ValidateAllScenes();
                Debug.Log("Validated all " + numValidated + " active scenes successfully");
            });
        }

        private static int ValidateAllScenes()
        {
            var activeScenePaths = EditorBuildSettings.scenes.Where(x => x.enabled)
                .Select(x => x.path).ToList();

            EditorSceneManager.OpenScene(InitScenePath);
            EditorSceneManager.CloseScene(SceneManager.GetActiveScene(), true);

            foreach (var scenePath in activeScenePaths)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            ZenUnityEditorUtil.ValidateCurrentSceneSetup();

            return activeScenePaths.Count;
        }

        private static bool ValidateWrapper(Action validateAction)
        {
            var originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                validateAction.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
            }
        }
    }
}