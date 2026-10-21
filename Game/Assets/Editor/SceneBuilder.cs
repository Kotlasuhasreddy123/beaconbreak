using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class SceneBuilder
    {
        [MenuItem("Co-op Fighter/Create Main Scene")]
        public static void CreateMainScene()
        {
            const string folder = "Assets/Scenes";
            const string path = "Assets/Scenes/Main.unity";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            // The bootstrap creates the presentation and game loop at runtime,
            // so the scene intentionally has no authored objects.
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.Refresh();
            Debug.Log("Created the game scene and added it to Build Settings: " + path);
        }
    }
}
