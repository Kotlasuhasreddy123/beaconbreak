// Build-only declarations used to syntax-check the Unity front end without an
// editor installation. This file must stay outside the Unity Assets folder.
using System;

namespace UnityEngine
{
    public class Object
    {
        public static T FindObjectOfType<T>() { return default(T); }
        public static T FindAnyObjectByType<T>() { return default(T); }
        public static void DontDestroyOnLoad(Object value) { }
        public static void Destroy(Object value) { }
    }
    public class MonoBehaviour : Object { }
    public class GameObject : Object
    {
        public GameObject(string name) { }
        public T AddComponent<T>() { return default(T); }
    }
    public enum RuntimeInitializeLoadType { AfterSceneLoad }
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType value) { }
    }
    public static class Application { public static int targetFrameRate; }
    public static class Time { public static float unscaledDeltaTime; }
    public static class Screen { public static int width = 1280, height = 720; }
    public enum TextureFormat { RGBA32 }
    public class Texture2D : Object
    {
        public Texture2D(int width, int height, TextureFormat format, bool mipmaps) { }
        public void SetPixel(int x, int y, Color color) { }
        public void Apply() { }
    }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float red, float green, float blue) { r = red; g = green; b = blue; a = 1f; }
        public static Color white { get { return new Color(1f, 1f, 1f); } }
    }
    public struct Vector3
    {
        public Vector3(float x, float y, float z) { }
        public static Vector3 zero { get { return new Vector3(); } }
    }
    public struct Quaternion { public static Quaternion identity { get { return new Quaternion(); } } }
    public struct Matrix4x4
    {
        public static Matrix4x4 TRS(Vector3 position, Quaternion rotation, Vector3 scale)
        { return new Matrix4x4(); }
    }
    public struct Rect { public Rect(float x, float y, float width, float height) { } }
    public enum FontStyle { Bold }
    public class GUIStyleState { public Color textColor; }
    public class GUIStyle
    {
        public int fontSize;
        public FontStyle fontStyle;
        public bool wordWrap;
        public GUIStyleState normal = new GUIStyleState();
        public GUIStyle() { }
        public GUIStyle(GUIStyle other) { }
    }
    public class GUISkin { public GUIStyle label = new GUIStyle(); }
    public static class GUI
    {
        public static Matrix4x4 matrix;
        public static GUISkin skin = new GUISkin();
        public static Color color;
        public static void Label(Rect position, string text, GUIStyle style) { }
        public static bool Button(Rect position, string text) { return false; }
        public static string TextField(Rect position, string text) { return text; }
        public static void DrawTexture(Rect position, Texture2D texture) { }
    }
    public enum KeyCode
    {
        Return, R, A, D, W, J, K, L, U, LeftArrow, RightArrow, UpArrow,
        Keypad1, Keypad2, Keypad3, Keypad0
    }
    public static class Input
    {
        public static bool GetKey(KeyCode key) { return false; }
        public static bool GetKeyDown(KeyCode key) { return false; }
    }
    public static class Mathf
    {
        public static float Min(float a, float b) { return Math.Min(a, b); }
        public static int Clamp(int v, int a, int b) { return Math.Max(a, Math.Min(b, v)); }
    }
    public static class PlayerPrefs
    {
        public static int GetInt(string key, int value) { return value; }
        public static string GetString(string key, string value) { return value; }
        public static void SetInt(string key, int value) { }
        public static void SetString(string key, string value) { }
        public static void Save() { }
    }
    public static class JsonUtility
    {
        public static T FromJson<T>(string value) { return default(T); }
        public static string ToJson(object value) { return "{}"; }
    }
    public static class Debug { public static void Log(string message) { } }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { }
}

namespace UnityEditor
{
    public sealed class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string path) { }
    }
    public sealed class EditorBuildSettingsScene
    {
        public EditorBuildSettingsScene(string path, bool enabled) { }
    }
    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes;
    }
    public static class AssetDatabase
    {
        public static void Refresh() { }
    }
}

namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup { EmptyScene }
    public enum NewSceneMode { Single }
    public static class EditorSceneManager
    {
        public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup setup, NewSceneMode mode)
        { return new UnityEngine.SceneManagement.Scene(); }
        public static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path) { }
    }
}
