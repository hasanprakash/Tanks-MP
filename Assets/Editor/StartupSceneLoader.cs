using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class StartupSceneLoader
{
    static StartupSceneLoader()
    {
        // Register the callback to load the startup scene when Unity starts
        EditorApplication.playModeStateChanged += LoadStartupScene;
    }

    private static void LoadStartupScene(PlayModeStateChange stateChange)
    {
        if (stateChange == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (stateChange == PlayModeStateChange.EnteredPlayMode)
        {
            if (SceneManager.GetActiveScene().buildIndex != 0) // could also get the active scene from EditorSceneManager
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}
