// Assets/360/Editor/EditorScreenShotWindow.cs
// Editor screenshot panel (Built-in RP) - Refactored with modular architecture
// - Sticky header: Capture / Scene Sync / Open Folder (not scrollable)
// - Scroll body: Status, Camera Settings (incl. Lens & Fisheye), Output Settings, Scene View overlay, Help/Reset
// - PNG/JPEG export (+ PNG alpha background), file name template, MSAA
// - Hotkeys: P = capture, O = toggle Scene Sync (global via Shortcut Manager; also local fallbacks)
// Requires: Localization (Loc/Lang), optional Freecam / FisheyeImageEffect / ESSSceneSync

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.ShortcutManagement;     // Shortcut Manager
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using FreecamPreview; // Loc + Lang
using EditorScreenShot.Runtime;
using EditorScreenShot;

namespace EditorScreenShot
{
    public partial class EditorScreenShotWindow : EditorWindow
    {
    // ───────── Singleton for global actions
    static EditorScreenShotWindow _inst;

    // ───────── Data
    EditorScreenShotData _data = new EditorScreenShotData();

    // ───────── Menu
    [MenuItem("Tools/360/EditorScreenShot")]
    public static void Open()
    {
        var w = GetWindow<EditorScreenShotWindow>(Loc.T("PanelTitle"));
        w.titleContent = new GUIContent(Loc.T("PanelTitle"));
        w.minSize = new Vector2(420, 520);
        
        // Auto-create EditorScreenShot object and camera
        EnsureEditorScreenShotSetup();
    }

    // ───────── Lifecycle
    void OnEnable()
    {
        _inst = this; // register singleton

        // Load settings
        EditorScreenShotSettings.LoadSettings(_data);

        Loc.Language = _data.lang;
        SceneView.duringSceneGui += OnSceneGUI; // also for local P/O hotkeys

        // Listen to PlayMode changes to sync camera refs
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        
        // Detect and ensure EditorScreenShot setup exists
        EditorApplication.delayCall += () => { 
            if (this) {
                EnsureEditorScreenShotSetup();
                EditorScreenShotCamera.SyncCameraRefs(_data, true); 
            }
        };
    }

    void OnDisable()
    {
        // Save settings
        EditorScreenShotSettings.SaveSettings(_data);

        SceneView.duringSceneGui -= OnSceneGUI;
        if (_data.sceneSyncOn) EditorScreenShotSceneSync.ToggleSceneSync(_data, false, PersistPoseAfterSceneSync);

        if (_inst == this) _inst = null; // release singleton

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    // ───────── GUI
    void OnGUI()
    {
        Loc.Language = _data.lang;

        EditorScreenShotUI.DrawTopBar(_data);
        GUILayout.Space(6);

        // Sticky header (not scrollable)
        using (new EditorGUILayout.VerticalScope(EditorScreenShotUI.StyledBox()))
        {
            EditorScreenShotUI.DrawStickyHeader(_data); // Capture + Scene Sync + Open Folder
        }

        GUILayout.Space(6);

        // Scroll body
        _data.scroll = EditorGUILayout.BeginScrollView(_data.scroll);

        using (new EditorGUILayout.VerticalScope(EditorScreenShotUI.StyledBox()))
        {
            EditorScreenShotUI.DrawStatusBlock(_data);
        }

        GUILayout.Space(6);
        using (new EditorGUILayout.VerticalScope(EditorScreenShotUI.StyledBox()))
        {
            _data.fCameraSettings = EditorGUILayout.Foldout(_data.fCameraSettings, Loc.T("CameraSettings"), true, EditorScreenShotUI.FoldoutTitleStyle());
            if (_data.fCameraSettings)
            {
                GUILayout.Space(4);
                EditorScreenShotUI.DrawCameraRow(_data);
                GUILayout.Space(4);
                EditorScreenShotUI.DrawLensBlockMerged(_data); // FOV / Physical / Fisheye
            }
        }

        using (new EditorGUILayout.VerticalScope(EditorScreenShotUI.StyledBox()))
        {
            _data.fOutputSettings = EditorGUILayout.Foldout(_data.fOutputSettings, Loc.T("OutputSettings"), true, EditorScreenShotUI.FoldoutTitleStyle());
            if (_data.fOutputSettings)
            {
                GUILayout.Space(4);
                EditorScreenShotUI.DrawOutputTogglesRow(_data);
                GUILayout.Space(4);
                EditorScreenShotUI.DrawResolutionRow(_data);
                GUILayout.Space(4);
                EditorScreenShotUI.DrawFormatRow(_data);
                GUILayout.Space(4);
                EditorScreenShotUI.DrawPathRow(_data);
                GUILayout.Space(4);
                EditorScreenShotUI.DrawFileNameTemplateRow(_data);
                GUILayout.Space(4);
                EditorScreenShotUI.DrawQualityRow(_data); // MSAA
            }
        }

        using (new EditorGUILayout.VerticalScope(EditorScreenShotUI.StyledBox()))
        {
            _data.fSceneView = EditorGUILayout.Foldout(_data.fSceneView, Loc.T("SceneViewSection"), true, EditorScreenShotUI.FoldoutTitleStyle());
            if (_data.fSceneView) DrawSceneViewOverlaySettings();
        }

        GUILayout.Space(6);
        DrawHelpAndReset();

        EditorGUILayout.EndScrollView();

        // Window-focused hotkeys (fallback)
        EditorScreenShotHotkeys.HandleWindowHotkeys(_data, PersistPoseAfterSceneSync);

        Repaint();
    }

    void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        // Refresh camera refs after PlayMode change
        EditorApplication.delayCall += () => { 
            if (this) {
                EditorScreenShotCamera.SyncCameraRefs(_data, true);
                // Force repaint to update status
                Repaint();
                SceneView.RepaintAll();
            }
        };
    }

    // ───────── SceneView overlay + local hotkeys
    void OnSceneGUI(SceneView sv)
    {
        if (sv == null) return;

        // Handle scene view hotkeys
        EditorScreenShotHotkeys.HandleSceneViewHotkeys(_data, sv, PersistPoseAfterSceneSync);

        // Safe frame drawing
        if (!_data.showSafeFrame) return;
        var e = Event.current;
        if (e.type != EventType.Repaint) return;

        var (tw, th) = EditorScreenShotOutput.GetCurrentWH(_data); // target output size
        if (tw <= 0 || th <= 0) return;

        float targetAspect = (float)tw / (float)th;
        Rect vp = sv.position;
        float viewW = vp.width;
        float viewH = vp.height;

        Rect inner;
        float viewAspect = viewW / viewH;
        if (viewAspect > targetAspect)
        {
            float ih = viewH - 6f;
            float iw = ih * targetAspect;
            inner = new Rect((viewW - iw) * 0.5f, (viewH - ih) * 0.5f, iw, ih);
        }
        else
        {
            float iw = viewW - 6f;
            float ih = iw / targetAspect;
            inner = new Rect((viewW - iw) * 0.5f, (viewH - ih) * 0.5f, iw, ih);
        }

        Handles.BeginGUI();

        // mask around frame
        Color mask = new Color(0f, 0f, 0f, Mathf.Clamp01(_data.sfMaskAlpha));
        EditorGUI.DrawRect(new Rect(0, 0, viewW, inner.yMin), mask);
        EditorGUI.DrawRect(new Rect(0, inner.yMax, viewW, viewH - inner.yMax), mask);
        EditorGUI.DrawRect(new Rect(0, inner.yMin, inner.xMin, inner.height), mask);
        EditorGUI.DrawRect(new Rect(inner.xMax, inner.yMin, viewW - inner.xMax, inner.height), mask);

        // border
        EditorScreenShotUI.DrawRectBorder(inner, _data.sfLineWidth, _data.sfLineColor);

        // thirds
        if (_data.sfThirds)
        {
            float x1 = inner.xMin + inner.width / 3f;
            float x2 = inner.xMin + inner.width * 2f / 3f;
            float y1 = inner.yMin + inner.height / 3f;
            float y2 = inner.yMin + inner.height * 2f / 3f;
            EditorScreenShotUI.AALine(new Vector2(x1, inner.yMin), new Vector2(x1, inner.yMax), _data.sfLineColor, _data.sfLineWidth);
            EditorScreenShotUI.AALine(new Vector2(x2, inner.yMin), new Vector2(x2, inner.yMax), _data.sfLineColor, _data.sfLineWidth);
            EditorScreenShotUI.AALine(new Vector2(inner.xMin, y1), new Vector2(inner.xMax, y1), _data.sfLineColor, _data.sfLineWidth);
            EditorScreenShotUI.AALine(new Vector2(inner.xMin, y2), new Vector2(inner.xMax, y2), _data.sfLineColor, _data.sfLineWidth);
        }

        // diagonals
        if (_data.sfDiagonals)
        {
            EditorScreenShotUI.AALine(new Vector2(inner.xMin, inner.yMin), new Vector2(inner.xMax, inner.yMax), _data.sfLineColor, _data.sfLineWidth);
            EditorScreenShotUI.AALine(new Vector2(inner.xMax, inner.yMin), new Vector2(inner.xMin, inner.yMax), _data.sfLineColor, _data.sfLineWidth);
        }

        // center cross
        if (_data.sfCenterCross)
        {
            float cx = inner.xMin + inner.width * 0.5f;
            float cy = inner.yMin + inner.height * 0.5f;
            EditorScreenShotUI.AALine(new Vector2(cx - 40, cy), new Vector2(cx + 40, cy), _data.sfLineColor, _data.sfLineWidth);
            EditorScreenShotUI.AALine(new Vector2(cx, cy - 40), new Vector2(cx, cy + 40), _data.sfLineColor, _data.sfLineWidth);
        }

        // title-safe
        if (_data.sfTitleSafe < 0.999f)
        {
            Rect ts = EditorScreenShotUI.ScaleRect(inner, _data.sfTitleSafe);
            EditorScreenShotUI.DrawRectBorder(ts, Mathf.Max(1f, _data.sfLineWidth - 1f), _data.sfLineColor * new Color(1,1,1,0.7f));
        }

        // label
        string label = $"{tw}x{th}";
        var size = EditorStyles.whiteLabel.CalcSize(new GUIContent(label));
        var labR = new Rect(inner.xMin + 6, inner.yMin + 6, size.x + 6, size.y + 2);
        EditorGUI.DrawRect(labR, new Color(0f, 0f, 0f, 0.5f));
        GUI.Label(new Rect(labR.x + 3, labR.y + 1, size.x, size.y), label, EditorStyles.whiteLabel);

        Handles.EndGUI();
    }

    // ───────── Global actions for shortcuts
    public static void ToggleSceneSyncGlobal()
    {
        if (_inst == null) Open();
        EditorApplication.delayCall += () =>
        {
            if (_inst == null) return;
            EditorScreenShotSceneSync.ToggleSceneSync(_inst._data, !_inst._data.sceneSyncOn, _inst.PersistPoseAfterSceneSync);
            _inst.Repaint();
            SceneView.RepaintAll();
        };
    }

    // Get the current EditorScreenShotData instance
    public static EditorScreenShotData GetCurrentData()
    {
        return _inst?._data;
    }

    public static void CaptureGlobal()
    {
        if (_inst == null) Open();
        EditorApplication.delayCall += () =>
        {
            if (_inst == null) return;
            EditorScreenShotOutput.CaptureGlobal(_inst._data);
        };
    }

    // ───────── Shortcut Manager (global single-key)
    [Shortcut("EditorScreenShot/Capture (P)", KeyCode.P)]
    static void SC_Capture()
    {
        if (EditorGUIUtility.editingTextField) return;
        CaptureGlobal();
    }

    [Shortcut("EditorScreenShot/Toggle Scene Sync (O)", KeyCode.O)]
    static void SC_ToggleSceneSync()
    {
        if (EditorGUIUtility.editingTextField) return;
        ToggleSceneSyncGlobal();
    }

    [Shortcut("EditorScreenShot/Open Panel (Ctrl+Alt+E)", KeyCode.E, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
    static void SC_OpenPanel()
    {
        if (EditorGUIUtility.editingTextField) return;
        Open();
    }

    // ───────── Auto-create EditorScreenShot setup (using Prefab)
    public static void EnsureEditorScreenShotSetup()
    {
        // Check if EditorScreenShot camera already exists
        Camera[] allCameras = Camera.allCameras;
        bool cameraExists = false;
        
        foreach (Camera cam in allCameras)
        {
            if (cam.name == "EditorScreenShot")
            {
                cameraExists = true;
                // Check and handle camera conflicts
                HandleCameraConflicts(cam);
                break;
            }
        }
        
        if (!cameraExists)
        {
            // Load Prefab
            GameObject prefab = LoadEditorScreenShotPrefab();
            if (prefab != null)
            {
                // Instantiate Prefab
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (prefabInstance != null)
                {
                    // Ensure camera name is correct
                    Camera camera = prefabInstance.GetComponent<Camera>();
                    if (camera != null)
                    {
                        // Force set camera name, override Unity's auto-naming
                        camera.name = "EditorScreenShot";
                        prefabInstance.name = "EditorScreenShot";
                        
                        // Prefab should already have all components properly configured
                        // Ensure ESSSceneSync component is enabled
                        var sceneSync = prefabInstance.GetComponent<ESSSceneSync>();
                        if (sceneSync != null)
                        {
                            sceneSync.enabled = true;
                            EditorUtility.SetDirty(sceneSync);
                        }
                        
                        // Mark as dirty to save
                        EditorUtility.SetDirty(camera);
                        EditorUtility.SetDirty(prefabInstance);
                        
                        // Ensure name is correct in next frame (prevent Unity auto-modification)
                        EditorApplication.delayCall += () => {
                            if (camera != null && camera.name != "EditorScreenShot")
                            {
                                camera.name = "EditorScreenShot";
                                prefabInstance.name = "EditorScreenShot";
                                EditorUtility.SetDirty(camera);
                                EditorUtility.SetDirty(prefabInstance);
                            }
                        };
                    }
                }
            }
            else
            {
                Debug.LogError("[EditorScreenShot] Failed to load Prefab, please check if EditorScrenShot.prefab file exists");
            }
        }
    }
    
    static void HandleCameraConflicts(Camera editorCamera)
    {
        if (editorCamera == null) return;
        
        // Check if other cameras are also set as MainCamera
        Camera[] allCameras = Camera.allCameras;
        Camera conflictingCamera = null;
        
        foreach (Camera cam in allCameras)
        {
            if (cam != editorCamera && cam.tag == "MainCamera")
            {
                conflictingCamera = cam;
                break;
            }
        }
        
        if (conflictingCamera != null)
        {
            Debug.LogWarning($"[EditorScreenShot] Camera conflict detected: {conflictingCamera.name} is also set as MainCamera, please fix manually");
        }
        
        // Ensure EditorScreenShot camera is the only MainCamera
        if (editorCamera.tag != "MainCamera")
        {
            Debug.LogWarning("[EditorScreenShot] EditorScreenShot camera tag is incorrect, please click repair button");
        }
        
        // Ensure EditorScreenShot camera is enabled
        if (!editorCamera.enabled)
        {
            Debug.LogWarning("[EditorScreenShot] EditorScreenShot camera is disabled, please click repair button");
        }
    }
    
    static GameObject LoadEditorScreenShotPrefab()
    {
        // Try loading Prefab from multiple possible paths
        string[] possiblePaths = {
            "Assets/Packages/com.adez360.editorscreenshot/Runtime/Prefab/EditorScreenShot.prefab",
            "Packages/com.adez360.editorscreenshot/Runtime/Prefab/EditorScreenShot.prefab"
        };
        
        foreach (string path in possiblePaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }
        }
        
        Debug.LogError("[EditorScreenShot] Cannot find EditorScreenShot.prefab file");
        return null;
    }
    }
}