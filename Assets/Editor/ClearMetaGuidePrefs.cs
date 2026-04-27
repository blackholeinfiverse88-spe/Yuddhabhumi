using UnityEditor;

[InitializeOnLoad]
public class ClearUnityEditorPrefs
{
    static ClearUnityEditorPrefs()
    {
        EditorPrefs.DeleteKey("Meta.XR.Guides.ActiveGuide");
        EditorPrefs.DeleteKey("SceneHierarchyExpandedState");
        EditorPrefs.DeleteKey("SceneHierarchyWindow");
        UnityEngine.Debug.Log("✔ Editor Prefs cleaned.");
    }
}
