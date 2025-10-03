using UnityEditor;
using UnityEngine;
using EditorScreenShot;
using EditorScreenShot.Runtime;

// Auto-coordinate SceneSync and Freecam lock targets on PlayMode changes
// Condition: Control panel window is open (_inst != null)
static class ESSPlaymodeCoordinator
{
#if UNITY_EDITOR
    static Transform s_LastPlayLockTarget;

    [InitializeOnLoadMethod]
    static void Init()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        // Perform scene scan on startup
        EditorApplication.delayCall += StartupScanOnce;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        // Only act when control panel is open
        var wnd = Resources.FindObjectsOfTypeAll<EditorScreenShotWindow>();
        bool panelOpen = wnd != null && wnd.Length > 0;
        if (!panelOpen) return;

        if (change == PlayModeStateChange.ExitingPlayMode)
        {
            // Exiting Play: record Lock Target and ensure SceneSync is enabled (with retry)
            var cam = Camera.main;
            var free = cam ? cam.GetComponent<Freecam>() : null;
            if (free) s_LastPlayLockTarget = free.lockTarget;
            
            // Delay longer to ensure camera and SceneView are ready
            EditorApplication.delayCall += () => EditorApplication.delayCall += () => 
            {
                // Get the EditorScreenShotData from the window instance
                var data = EditorScreenShotWindow.GetCurrentData();
                if (data != null)
                {
                    EditorScreenShotSceneSync.EnsureSceneSyncEnabled(data);
                }
            };
        }
        else if (change == PlayModeStateChange.EnteredPlayMode)
        {
            // Entering Play: restore Freecam Lock Target (if recorded)
            var cam = Camera.main;
            var free = cam ? cam.GetComponent<Freecam>() : null;
            if (free && s_LastPlayLockTarget) free.lockTarget = s_LastPlayLockTarget;

            // Delay longer to ensure camera is ready, then sync once
            EditorApplication.delayCall += () => EditorApplication.delayCall += () => {
                EditorScreenShotWindow.ToggleSceneSyncGlobal();
                EditorApplication.delayCall += () => EditorScreenShotWindow.ToggleSceneSyncGlobal();
            };
        }
    }

    static void StartupScanOnce()
    {
        // Simple scan: remove leftover ESSSceneSync/ESSposeOverride from cameras in edit mode
        // But keep components on EditorScreenShot camera
        if (Application.isPlaying) return;
        foreach (var cam in UnityEngine.Object.FindObjectsOfType<Camera>())
        {
            // Skip EditorScreenShot camera, keep its components
            if (cam.name == "EditorScreenShot") continue;
            
            var sync = cam.GetComponent<ESSSceneSync>();
            if (sync) Object.DestroyImmediate(sync);
            var over = cam.GetComponent<ESSposeOverride>();
            if (over) Object.DestroyImmediate(over);
        }
    }
#endif
}
[InitializeOnLoad]
public static class PlaymodeAutoOpen
{
    static PlaymodeAutoOpen()
    {
        EditorApplication.playModeStateChanged += OnState;
    }

    static void OnState(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.delayCall += () =>
            {
                bool hasFreecam = UnityEngine.Object.FindObjectsOfType<Freecam>(true).Length > 0;
                bool hasFisheye = UnityEngine.Object.FindObjectsOfType<FisheyeImageEffect>(true).Length > 0;
                if (hasFreecam || hasFisheye) EditorScreenShotWindow.Open();
            };
        }
    }
}
