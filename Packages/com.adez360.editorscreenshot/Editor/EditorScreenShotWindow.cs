
// Assets/360/Editor/EditorScreenShotWindow.cs
// Editor screenshot panel (Built-in RP).
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

public partial class EditorScreenShotWindow : EditorWindow
{
    // ───────── Singleton for global actions
    static EditorScreenShotWindow _inst;

    // ───────── Data
    enum AspectPreset { UHD_4K_16_9, FHD_16_9, QHD_16_9, HD_16_9, Square_1_1, Custom }

    Camera _cam;
    Freecam _freecam;                 // optional
    FisheyeImageEffect _fisheye;      // optional

    AspectPreset _preset = AspectPreset.FHD_16_9;
    bool _portrait = false;
    int _customW = 1920, _customH = 1080;

    bool _png = true;
    int _jpgQuality = 95;
    string _outDir;
    bool _pngKeepAlpha = true;        // transparent background for PNG
    bool _openAfterSave = false;
    string _lastSavedPath = null;

    string _fileNameTemplate = "{scene}_{yyyyMMdd_HHmmss}";
    int _rtMSAA = 1; // 1/2/4/8

    Lang _lang = Loc.GetSystemDefaultLanguage();

    bool fCameraSettings = true;
    bool fOutputSettings = true;
    bool fSceneView = true;

    // Scene View Safe Frame
    bool  _showSafeFrame    = false;
    bool  _sfThirds         = true;
    bool  _sfDiagonals      = false;
    bool  _sfCenterCross    = true;
    float _sfTitleSafe      = 0.90f;
    float _sfLineWidth      = 2f;
    Color _sfLineColor      = new Color(1f, 1f, 1f, 0.9f);
    float _sfMaskAlpha      = 0.35f;
    bool  _sfOnlyWhenTarget = false;

    Dictionary<string, float> _speedFieldSnapshot;

    // Scene Sync
    bool _sceneSyncOn = false;
    ESSSceneSync _sceneSync;

    // Scroll
    Vector2 _scroll;

    // ───────── Menu
    [MenuItem("Tools/360/EditorScreenShot")]
    public static void Open()
    {
        var w = GetWindow<EditorScreenShotWindow>(Loc.T("PanelTitle"));
        w.titleContent = new GUIContent(Loc.T("PanelTitle"));
        w.minSize = new Vector2(420, 520);
    }

    // ───────── Lifecycle
    void OnEnable()
    {
        _inst = this; // register singleton

        _outDir = EditorPrefs.GetString("ESS.OutDir", OutputPathResolver.GetDefaultScreenshotDir());
        _png = EditorPrefs.GetBool("ESS.PNG", true);
        _jpgQuality = EditorPrefs.GetInt("ESS.JPGQ", 95);
        _pngKeepAlpha = EditorPrefs.GetBool("ESS.PNGAlpha", true);
        _openAfterSave = EditorPrefs.GetBool("ESS.OpenAfter", false);
        _lang = (Lang)EditorPrefs.GetInt("ESS.Lang", (int)Loc.GetSystemDefaultLanguage());
        _lastSavedPath = EditorPrefs.GetString("ESS.LastPath", null);
        _fileNameTemplate = EditorPrefs.GetString("ESS.FNameTpl", "{scene}_{yyyyMMdd_HHmmss}");
        _rtMSAA = EditorPrefs.GetInt("ESS.RTMSAA", 1);

        _showSafeFrame = EditorPrefs.GetBool("ESS.SF.Show", false);
        _sfThirds = EditorPrefs.GetBool("ESS.SF.Thirds", true);
        _sfDiagonals = EditorPrefs.GetBool("ESS.SF.Diag", false);
        _sfCenterCross = EditorPrefs.GetBool("ESS.SF.Center", true);
        _sfTitleSafe = EditorPrefs.GetFloat("ESS.SF.TS", 0.90f);
        _sfLineWidth = EditorPrefs.GetFloat("ESS.SF.LW", 2f);
        _sfLineColor = LoadColor("ESS.SF.Color", new Color(1,1,1,0.9f));
        _sfMaskAlpha = EditorPrefs.GetFloat("ESS.SF.Mask", 0.35f);
        _sfOnlyWhenTarget = EditorPrefs.GetBool("ESS.SF.OnlyTarget", false);

        Loc.Language = _lang;
        SceneView.duringSceneGui += OnSceneGUI; // also for local P/O hotkeys

        // Listen to PlayMode changes to sync camera refs
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        // Try to get a camera on first enable
        EditorApplication.delayCall += () => { if (this) SyncCameraRefs(true); };
    }

    void OnDisable()
    {
        // persist prefs
        EditorPrefs.SetString("ESS.OutDir", _outDir);
        EditorPrefs.SetBool("ESS.PNG", _png);
        EditorPrefs.SetInt("ESS.JPGQ", _jpgQuality);
        EditorPrefs.SetBool("ESS.PNGAlpha", _pngKeepAlpha);
        EditorPrefs.SetBool("ESS.OpenAfter", _openAfterSave);
        EditorPrefs.SetInt("ESS.Lang", (int)_lang);
        EditorPrefs.SetString("ESS.FNameTpl", _fileNameTemplate);
        EditorPrefs.SetInt("ESS.RTMSAA", _rtMSAA);
        if (!string.IsNullOrEmpty(_lastSavedPath)) EditorPrefs.SetString("ESS.LastPath", _lastSavedPath);

        EditorPrefs.SetBool("ESS.SF.Show", _showSafeFrame);
        EditorPrefs.SetBool("ESS.SF.Thirds", _sfThirds);
        EditorPrefs.SetBool("ESS.SF.Diag", _sfDiagonals);
        EditorPrefs.SetBool("ESS.SF.Center", _sfCenterCross);
        EditorPrefs.SetFloat("ESS.SF.TS", _sfTitleSafe);
        EditorPrefs.SetFloat("ESS.SF.LW", _sfLineWidth);
        SaveColor("ESS.SF.Color", _sfLineColor);
        EditorPrefs.SetFloat("ESS.SF.Mask", _sfMaskAlpha);
        EditorPrefs.SetBool("ESS.SF.OnlyTarget", _sfOnlyWhenTarget);

        SceneView.duringSceneGui -= OnSceneGUI;
        if (_sceneSyncOn) ToggleSceneSync(false);

        if (_inst == this) _inst = null; // release singleton

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    // ───────── GUI
    void OnGUI()
    {
        Loc.Language = _lang;

        DrawTopBar();
        GUILayout.Space(6);

        // Sticky header (not scrollable)
        using (new EditorGUILayout.VerticalScope(StyledBox()))
        {
            DrawStickyHeader(); // Capture + Scene Sync + Open Folder
        }

        GUILayout.Space(6);

        // Scroll body
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        using (new EditorGUILayout.VerticalScope(StyledBox()))
        {
            DrawStatusBlock();
        }

        GUILayout.Space(6);
        using (new EditorGUILayout.VerticalScope(StyledBox()))
        {
            fCameraSettings = EditorGUILayout.Foldout(fCameraSettings, Loc.T("CameraSettings"), true, FoldoutTitleStyle());
            if (fCameraSettings)
            {
                GUILayout.Space(4);
                DrawCameraRow();
                GUILayout.Space(4);
                DrawLockTargetRow();
                GUILayout.Space(4);
                DrawLensBlockMerged(); // FOV / Physical / Fisheye
            }
        }

        using (new EditorGUILayout.VerticalScope(StyledBox()))
        {
            fOutputSettings = EditorGUILayout.Foldout(fOutputSettings, Loc.T("OutputSettings"), true, FoldoutTitleStyle());
            if (fOutputSettings)
            {
                GUILayout.Space(4);
                DrawOutputTogglesRow();
                GUILayout.Space(4);
                DrawResolutionRow();
                GUILayout.Space(4);
                DrawFormatRow();
                GUILayout.Space(4);
                DrawPathRow();
                GUILayout.Space(4);
                DrawFileNameTemplateRow();
                GUILayout.Space(4);
                DrawQualityRow(); // MSAA
            }
        }

        using (new EditorGUILayout.VerticalScope(StyledBox()))
        {
            fSceneView = EditorGUILayout.Foldout(fSceneView, Loc.T("SceneViewSection"), true, FoldoutTitleStyle());
            if (fSceneView) DrawSceneViewOverlaySettings();
        }

        GUILayout.Space(6);
        DrawHelpAndReset();

        EditorGUILayout.EndScrollView();

        // Window-focused hotkeys (fallback)
        HandleWindowHotkeys();

        Repaint();
    }

    void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        // Refresh camera refs after PlayMode change
        EditorApplication.delayCall += () => { 
            if (this) {
                SyncCameraRefs(true);
                // Force repaint to update status
                Repaint();
                SceneView.RepaintAll();
            }
        };
    }

    void SyncCameraRefs(bool preferMain)
    {
        Camera chosen = _cam;
        if (preferMain && (!chosen || chosen != Camera.main))
            chosen = Camera.main ? Camera.main : chosen;

        if (!chosen && Camera.allCamerasCount > 0)
        {
            var cams = Camera.allCameras;
            chosen = cams != null && cams.Length > 0 ? cams[0] : null;
        }

        _cam = chosen;
        _freecam = _cam ? _cam.GetComponent<Freecam>() : null;
        _fisheye = _cam ? _cam.GetComponent<FisheyeImageEffect>() : null;
        if (_freecam) SnapshotFreecamSpeeds();
        
        // Ensure Scene Sync uses correct camera
        if (_sceneSyncOn && _sceneSync && _cam)
        {
            _sceneSync.targetCam = _cam;
            _sceneSync.freecamController = _freecam;
        }
    }

    void DrawTopBar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label(Loc.T("Lang"), GUILayout.Width(70));
            Lang newLang = (Lang)EditorGUILayout.EnumPopup(_lang, GUILayout.Width(200));
            if (newLang != _lang)
            {
                _lang = newLang;
                Loc.Language = _lang;
                EditorPrefs.SetInt("ESS.Lang", (int)_lang);
            }
            GUILayout.FlexibleSpace();
        }
    }

    // ───────── Sticky header (Capture + Scene Sync + Open folder)
    void DrawStickyHeader()
    {
        using (new EditorGUI.DisabledScope(_cam == null))
        {
            var (outW, outH) = GetCurrentWH();
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = Hex("#66FFB3"); // bright green
            // Add camera icon to capture button
            Texture2D cameraIcon = Resources.Load<Texture2D>("icon");
            var buttonStyle = new GUIStyle(BigPrimaryStyle())
            {
                imagePosition = ImagePosition.ImageAbove,
                alignment = TextAnchor.MiddleCenter
            };
            if (GUILayout.Button(new GUIContent(Loc.T("CaptureImage"), cameraIcon), buttonStyle))
                SaveCurrentFrame(outW, outH);
            GUI.backgroundColor = prev;
        }

        GUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(_cam == null))
            {
                string label = _sceneSyncOn ? Loc.T("SceneSyncOn") : Loc.T("SceneSyncOff");
                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = _sceneSyncOn ? new Color(0.23f,0.65f,0.42f) : new Color(0.45f,0.45f,0.45f);
                if (GUILayout.Button(label, GUILayout.Height(24), GUILayout.Width(0), GUILayout.ExpandWidth(true)))
                    ToggleSceneSync(!_sceneSyncOn);
                GUI.backgroundColor = prev;
            }

            // Add folder icon to open folder button
            Texture2D folderIcon = EditorGUIUtility.IconContent("Folder").image as Texture2D;
            if (GUILayout.Button(new GUIContent(Loc.T("OpenFolder"), folderIcon), GUILayout.Height(24), GUILayout.Width(0), GUILayout.ExpandWidth(true)))
            {
                if (!string.IsNullOrEmpty(_lastSavedPath) && File.Exists(_lastSavedPath))
                    EditorUtility.RevealInFinder(_lastSavedPath);
                else
                    EditorUtility.RevealInFinder(_outDir);
            }
        }
    }

    // ───────── Local hotkeys when window focused
    void HandleWindowHotkeys()
    {
        var e = Event.current;
        if (e == null || e.type != EventType.KeyDown) return;
        if (EditorGUIUtility.editingTextField) return;

        if (e.keyCode == KeyCode.P && _cam != null)
        {
            var (outW, outH) = GetCurrentWH();
            SaveCurrentFrame(outW, outH);
            e.Use();
        }
        else if (e.keyCode == KeyCode.O)
        {
            ToggleSceneSync(!_sceneSyncOn);
            e.Use();
        }
    }

    // ───────── Scene Sync
    void ToggleSceneSync(bool enable)
    {
        if (!_cam) { _sceneSyncOn = false; return; }

#if UNITY_EDITOR
        if (enable)
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null || sv.camera == null)
            {
                EditorUtility.DisplayDialog("Scene Sync", "Open a Scene view first.", "OK");
                _sceneSyncOn = false;
                return;
            }

            // Ensure camera refs are up to date
            SyncCameraRefs(true);
            if (!_cam) { _sceneSyncOn = false; return; }

            _sceneSync = _cam.GetComponent<ESSSceneSync>();
            if (!_sceneSync) _sceneSync = _cam.gameObject.AddComponent<ESSSceneSync>();

            Behaviour brain = _cam.GetComponent("CinemachineBrain") as Behaviour;
            _sceneSync.Begin(_cam, _freecam, brain);
            _sceneSyncOn = true;
            
            // Ensure component works in edit mode
            if (!_sceneSync.enabled) _sceneSync.enabled = true;
            if (!_sceneSync.gameObject.activeInHierarchy) _sceneSync.gameObject.SetActive(true);
            
            // Force repaint to update UI
            Repaint();
            SceneView.RepaintAll();
        }
        else
        {
#if UNITY_EDITOR
            // Get current SceneView pose before ending sync
            var sv = SceneView.lastActiveSceneView;
            Vector3 pos = sv != null && sv.camera != null ? sv.camera.transform.position : (_cam ? _cam.transform.position : Vector3.zero);
            Quaternion rot = sv != null ? sv.rotation : (_cam ? _cam.transform.rotation : Quaternion.identity);
            bool ortho = sv != null && sv.orthographic;
            float fov = sv != null ? sv.cameraSettings.fieldOfView : (_cam ? _cam.fieldOfView : 60f);
            float orthoSize = sv != null ? sv.size : (_cam ? _cam.orthographicSize : 5f);
#endif
            if (_sceneSync) _sceneSync.End();
#if UNITY_EDITOR
            PersistPoseAfterSceneSync(pos, rot, ortho, fov, orthoSize);
#endif
            _sceneSyncOn = false;
        }
#else
        _sceneSyncOn = false;
#endif
    }

    // ───────── Status
    void DrawStatusBlock()
    {
        var label = new GUIStyle(EditorStyles.label) { richText = true, fontSize = 12 };
        string spd = _freecam ? _freecam.CurrentSpeed.ToString("0.00") : "N/A";
        GUILayout.Label($"{Loc.T("Status")}:  <color=#59FFA6>{Loc.T("Speed")} {spd}</color>", label);

        GUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            bool hasFreecam = _freecam != null;
            bool hasTarget = hasFreecam && _freecam.lockTarget;
            bool active = hasFreecam && _freecam.LockActive;

            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = active
                ? new Color(0.23f, 0.65f, 0.42f)
                : (hasTarget ? new Color(0.85f, 0.55f, 0.15f)
                             : new Color(0.45f, 0.45f, 0.45f));

            using (new EditorGUI.DisabledScope(!hasFreecam))
            {
                string btnText = active
                    ? $"{Loc.T("LockBtnOn")}" + (hasTarget ? $" ({_freecam.lockTarget.name})" : "")
                    : (hasTarget ? $"{Loc.T("LockBtnOff")} ({_freecam.lockTarget.name})"
                                 : $"{Loc.T("LockBtnNoTarget")}");
                if (GUILayout.Button(btnText, GUILayout.Height(22), GUILayout.Width(0), GUILayout.ExpandWidth(true)))
                {
                    if (hasTarget) _freecam.lockLookAt = !_freecam.lockLookAt;
                    else EditorUtility.DisplayDialog(Loc.T("Lock"), Loc.T("NoLockTarget"), "OK");
                }
            }
            GUI.backgroundColor = prev;

            GUILayout.Space(8);
            using (new EditorGUI.DisabledScope(!hasFreecam))
            {
                if (GUILayout.Button($"{Loc.T("ResetSpeed")}", GUILayout.Height(22), GUILayout.Width(0), GUILayout.ExpandWidth(true)))
                    ResetFreecamSpeedRobust();
            }
        }
    }

    // ───────── Camera row / target / lens
    void DrawCameraRow()
    {
        var cams = Camera.allCameras;
        int idx = Mathf.Max(0, Array.IndexOf(cams, _cam));
        string[] options = new string[cams.Length];
        for (int i = 0; i < cams.Length; i++) options[i] = cams[i].name;

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("Camera"), GUILayout.Width(70));
            int newIdx = cams.Length == 0 ? -1 : EditorGUILayout.Popup(idx, options);
            if (cams.Length > 0 && (newIdx != idx || _cam == null))
            {
                _cam = cams[newIdx];
                _freecam = _cam ? _cam.GetComponent<Freecam>() : null;
                _fisheye = _cam ? _cam.GetComponent<FisheyeImageEffect>() : null;
                SnapshotFreecamSpeeds();
            }
        }

        using (new EditorGUI.DisabledScope(_cam == null))
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Loc.T("ReloadCamera"), GUILayout.Height(20), GUILayout.Width(180)))
                RefreshCameraDisplayToDisplay1(_cam);
        }
    }

    void DrawLockTargetRow()
    {
        using (new EditorGUI.DisabledScope(_cam == null))
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("LockTarget"), GUILayout.Width(90));
            Transform newT = (Transform)EditorGUILayout.ObjectField(
                _freecam ? _freecam.lockTarget : null,
                typeof(Transform), true);

            if (_freecam && newT != _freecam.lockTarget)
                _freecam.lockTarget = newT;
        }
    }

    void DrawLensBlockMerged()
    {
        if (!_cam)
        {
            EditorGUILayout.HelpBox(Loc.T("Camera") + " = null", MessageType.Info);
            return;
        }

        // FOV
        float fov = _cam.fieldOfView;
        float newFov = EditorGUILayout.Slider(Loc.T("FOV"), fov, 1f, 179f);
        if (!Mathf.Approximately(newFov, fov)) _cam.fieldOfView = newFov;

        // Physical lens (if enabled)
        if (_cam.usePhysicalProperties)
        {
            float focal = _cam.focalLength;
            float sensorH = _cam.sensorSize.y;
            float newFocal = EditorGUILayout.Slider(Loc.T("Focal"), focal, 1f, 300f);
            if (!Mathf.Approximately(newFocal, focal))
            {
                _cam.focalLength = newFocal;
                _cam.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan((0.5f * sensorH) / newFocal);
            }
        }

        // Fisheye
        using (new EditorGUI.DisabledScope(_cam == null))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("Fisheye"), GUILayout.Width(70));
                bool has = _fisheye && _fisheye.enabled;
                bool want = EditorGUILayout.ToggleLeft(Loc.T("Enable"), has, GUILayout.Width(80));
                if (want != has)
                {
                    if (want)
                    {
                        _fisheye = _cam.GetComponent<FisheyeImageEffect>();
                        if (!_fisheye) _fisheye = _cam.gameObject.AddComponent<FisheyeImageEffect>();
                        _fisheye.enabled = true;
                    }
                    else if (_fisheye) _fisheye.enabled = false;
                }
            }
            if (_fisheye)
            {
                EditorGUI.indentLevel++;
                _fisheye.strength = EditorGUILayout.Slider(Loc.T("Strength"), _fisheye.strength, 0f, 1f);
                EditorGUI.indentLevel--;
            }
        }
    }

    // ───────── Output settings
    void DrawOutputTogglesRow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (_png)
                _pngKeepAlpha = GUILayout.Toggle(_pngKeepAlpha, Loc.T("Transparent"), GUILayout.Width(160));
            _openAfterSave = GUILayout.Toggle(_openAfterSave, Loc.T("OpenAfterSave"), GUILayout.Width(160));
            GUILayout.FlexibleSpace();
        }
    }

    void DrawResolutionRow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("Resolution"), GUILayout.Width(90));
            string[] names = { Loc.T("UHD_4K_16_9"), Loc.T("FHD_16_9"), Loc.T("QHD_16_9"), Loc.T("HD_16_9"), Loc.T("Square_1_1"), Loc.T("Custom") };
            int sel = (int)_preset, newSel = EditorGUILayout.Popup(sel, names);
            if (newSel != sel) _preset = (AspectPreset)newSel;
            _portrait = GUILayout.Toggle(_portrait, Loc.T("Portrait"), GUILayout.Width(90));
        }
        if (_preset == AspectPreset.Custom)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(Loc.T("WidthShort"), GUILayout.Width(30));
                _customW = Mathf.Max(8, EditorGUILayout.IntField(_customW, GUILayout.Width(80)));
                GUILayout.Space(8);
                GUILayout.Label(Loc.T("HeightShort"), GUILayout.Width(30));
                _customH = Mathf.Max(8, EditorGUILayout.IntField(_customH, GUILayout.Width(80)));
                GUILayout.FlexibleSpace();
            }
        }
        var (cw, ch) = GetCurrentWH();
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(90); // Align with label width
            var label = new GUIStyle(EditorStyles.label) { richText = true };
            GUILayout.Label($"<color=#59FFA6>{cw}x{ch}</color>", label);
        }
    }

    void DrawFormatRow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("Format"), GUILayout.Width(90));
            _png = GUILayout.Toggle(_png, "PNG", "Button", GUILayout.Width(60));
            bool jpg = GUILayout.Toggle(!_png, "JPEG", "Button", GUILayout.Width(60));
            if (jpg) _png = false;
            GUILayout.FlexibleSpace();
        }
        if (!_png)
        {
            _jpgQuality = EditorGUILayout.IntSlider("JPEG Q", _jpgQuality, 1, 100);
        }
    }

    void DrawPathRow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("SaveLocation"), GUILayout.Width(90));
            EditorGUILayout.SelectableLabel(_outDir, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            // Add folder icon to browse button
            Texture2D browseIcon = EditorGUIUtility.IconContent("Folder").image as Texture2D;
            if (GUILayout.Button(new GUIContent(Loc.T("Browse"), browseIcon), GUILayout.Width(90)))
            {
                var p = EditorUtility.OpenFolderPanel(Loc.T("Browse"), _outDir, "");
                if (!string.IsNullOrEmpty(p)) _outDir = p;
            }
        }
    }

    void DrawFileNameTemplateRow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("FileNameTemplate"), GUILayout.Width(90));
            _fileNameTemplate = EditorGUILayout.TextField(_fileNameTemplate);
        }
    }

    void DrawQualityRow()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("MSAA"), GUILayout.Width(90));
            int sel = MSAAIndexFrom(_rtMSAA);
            int newSel = EditorGUILayout.Popup(sel, new[] { "1x", "2x", "4x", "8x" }, GUILayout.Width(100));
            _rtMSAA = MSAAValueFrom(newSel);
            GUILayout.FlexibleSpace();
        }
    }

    // ───────── SceneView overlay + local hotkeys
    void OnSceneGUI(SceneView sv)
    {
        if (sv == null) return;

        // Local single-key hotkeys from SceneView (fallback)
        var e = Event.current;
        if (e != null && e.type == EventType.KeyDown && !EditorGUIUtility.editingTextField)
        {
            if (e.keyCode == KeyCode.P && _cam != null)
            {
                var (outW, outH) = GetCurrentWH();
                SaveCurrentFrame(outW, outH);
                e.Use();
                sv.Repaint();
            }
            else if (e.keyCode == KeyCode.O)
            {
                ToggleSceneSync(!_sceneSyncOn);
                e.Use();
                sv.Repaint();
            }
        }

        // Safe frame drawing
        if (!_showSafeFrame) return;
        if (_sfOnlyWhenTarget && (!_cam || !_freecam || _freecam.lockTarget == null)) return;
        if (e.type != EventType.Repaint) return;

        var (tw, th) = GetCurrentWH(); // target output size
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
        Color mask = new Color(0f, 0f, 0f, Mathf.Clamp01(_sfMaskAlpha));
        EditorGUI.DrawRect(new Rect(0, 0, viewW, inner.yMin), mask);
        EditorGUI.DrawRect(new Rect(0, inner.yMax, viewW, viewH - inner.yMax), mask);
        EditorGUI.DrawRect(new Rect(0, inner.yMin, inner.xMin, inner.height), mask);
        EditorGUI.DrawRect(new Rect(inner.xMax, inner.yMin, viewW - inner.xMax, inner.height), mask);

        // border
        DrawRectBorder(inner, _sfLineWidth, _sfLineColor);

        // thirds
        if (_sfThirds)
        {
            float x1 = inner.xMin + inner.width / 3f;
            float x2 = inner.xMin + inner.width * 2f / 3f;
            float y1 = inner.yMin + inner.height / 3f;
            float y2 = inner.yMin + inner.height * 2f / 3f;
            AALine(new Vector2(x1, inner.yMin), new Vector2(x1, inner.yMax));
            AALine(new Vector2(x2, inner.yMin), new Vector2(x2, inner.yMax));
            AALine(new Vector2(inner.xMin, y1), new Vector2(inner.xMax, y1));
            AALine(new Vector2(inner.xMin, y2), new Vector2(inner.xMax, y2));
        }

        // diagonals
        if (_sfDiagonals)
        {
            AALine(new Vector2(inner.xMin, inner.yMin), new Vector2(inner.xMax, inner.yMax));
            AALine(new Vector2(inner.xMax, inner.yMin), new Vector2(inner.xMin, inner.yMax));
        }

        // center cross
        if (_sfCenterCross)
        {
            float cx = inner.xMin + inner.width * 0.5f;
            float cy = inner.yMin + inner.height * 0.5f;
            AALine(new Vector2(cx - 40, cy), new Vector2(cx + 40, cy));
            AALine(new Vector2(cx, cy - 40), new Vector2(cx, cy + 40));
        }

        // title-safe
        if (_sfTitleSafe < 0.999f)
        {
            Rect ts = ScaleRect(inner, _sfTitleSafe);
            DrawRectBorder(ts, Mathf.Max(1f, _sfLineWidth - 1f), _sfLineColor * new Color(1,1,1,0.7f));
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
            _inst.ToggleSceneSync(!_inst._sceneSyncOn);
            _inst.Repaint();
            SceneView.RepaintAll();
        };
    }

    // Force enable Scene Sync with retry
    public static void EnsureSceneSyncEnabled()
    {
        if (_inst == null) Open();
        int attempts = 0;
        void TryEnable()
        {
            if (_inst == null) return;
            // Ensure camera is available
            if (_inst._cam == null) _inst.SyncCameraRefs(true);
            // Ensure SceneView exists
            var sv = SceneView.lastActiveSceneView;
            if (sv == null || sv.camera == null)
            {
                if (attempts++ < 3) { EditorApplication.delayCall += TryEnable; return; }
                return;
            }
            
            // Force reset: disable then enable
            if (_inst._sceneSyncOn) _inst.ToggleSceneSync(false);
            EditorApplication.delayCall += () => {
                if (_inst != null) {
                    // Ensure camera refs are up to date
                    _inst.SyncCameraRefs(true);
                    _inst.ToggleSceneSync(true);
                    
                    // Force repaint and update SceneView
                    _inst.Repaint();
                    SceneView.RepaintAll();
                    
                    // Extra delay to ensure sync starts
                    EditorApplication.delayCall += () => {
                        if (_inst != null) {
                            _inst.Repaint();
                            SceneView.RepaintAll();
                        }
                    };
                }
            };
        }
        EditorApplication.delayCall += TryEnable;
    }

    public static void CaptureGlobal()
    {
        if (_inst == null) Open();
        EditorApplication.delayCall += () =>
        {
            if (_inst == null) return;
            var cam = _inst._cam != null ? _inst._cam : Camera.main;
            if (cam == null)
            {
                EditorUtility.DisplayDialog("Screenshot", "No Camera selected/found.", "OK");
                return;
            }
            _inst._cam = cam; // sync window
            var (outW, outH) = _inst.GetCurrentWH();
            _inst.SaveCurrentFrame(outW, outH);
        };
    }

    // ───────── Shortcut Manager (global single-key)
    // Appears in Edit ▸ Shortcuts as "EditorScreenShot/*"
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

    // Quick open control panel
    [Shortcut("EditorScreenShot/Open Panel (Ctrl+Alt+E)", KeyCode.E, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
    static void SC_OpenPanel()
    {
        if (EditorGUIUtility.editingTextField) return;
        Open();
    }

    // ───────── Save helpers
    (int, int) GetPresetWH(AspectPreset p) => p switch
    {
        AspectPreset.UHD_4K_16_9 => (3840, 2160),
        AspectPreset.QHD_16_9    => (2560, 1440),
        AspectPreset.FHD_16_9    => (1920, 1080),
        AspectPreset.HD_16_9     => (1280, 720),
        AspectPreset.Square_1_1  => (1024, 1024),
        _                        => (_customW, _customH),
    };

    (int, int) GetCurrentWH()
    {
        var (w, h) = GetPresetWH(_preset);
        if (_preset == AspectPreset.Custom) { w = _customW; h = _customH; }
        if (_portrait) (w, h) = (h, w);
        return (w, h);
    }

    void SaveCurrentFrame(int w, int h)
    {
        if (!_cam) return;

        int maxTex = SystemInfo.maxTextureSize;
        if (w > maxTex || h > maxTex)
        {
            EditorUtility.DisplayDialog("Screenshot", $"Size {w}x{h} exceeds max texture size ({maxTex}).", "OK");
            return;
        }

        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        rt.antiAliasing = Mathf.ClosestPowerOfTwo(Mathf.Clamp(_rtMSAA, 1, 8));

        var prevActive = RenderTexture.active;
        var prevTarget = _cam.targetTexture;
        var prevFlags = _cam.clearFlags;
        var prevBG = _cam.backgroundColor;
        var prevSky = RenderSettings.skybox;

        try
        {
            if (_png && _pngKeepAlpha)
            {
                _cam.clearFlags = CameraClearFlags.SolidColor;
                var c = prevBG; c.a = 0f; // transparent background
                _cam.backgroundColor = c;
                RenderSettings.skybox = null;
            }

            _cam.targetTexture = rt;
            RenderTexture.active = rt;
            _cam.Render();

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            Directory.CreateDirectory(_outDir);
            string ext = _png ? "png" : "jpg";
            string name = BuildFileName(_fileNameTemplate, w, h);
            string path = Path.Combine(_outDir, name + "." + ext);
            File.WriteAllBytes(path, _png ? tex.EncodeToPNG() : tex.EncodeToJPG(Mathf.Clamp(_jpgQuality, 1, 100)));
            _lastSavedPath = path;

            if (_openAfterSave) EditorUtility.OpenWithDefaultApp(path);
            else EditorUtility.RevealInFinder(path);

            DestroyImmediate(tex);
        }
        finally
        {
            _cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            _cam.clearFlags = prevFlags;
            _cam.backgroundColor = prevBG;
            RenderSettings.skybox = prevSky;
            rt.Release(); DestroyImmediate(rt);
        }
    }

    string BuildFileName(string tpl, int w, int h)
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string cam = _cam ? _cam.name : "Cam";
        string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string s = tpl.Replace("{scene}", scene).Replace("{camera}", cam)
                      .Replace("{w}", w.ToString()).Replace("{h}", h.ToString())
                      .Replace("{wxh}", $"{w}x{h}").Replace("{yyyyMMdd_HHmmss}", ts);
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "_");
        return s;
    }

    // ───────── UI helpers
    GUIStyle StyledBox() { var s = new GUIStyle("box"); s.padding = new RectOffset(10,10,10,10); return s; }
    GUIStyle FoldoutTitleStyle(){ var f = new GUIStyle(EditorStyles.foldoutHeader){fontStyle = FontStyle.Bold}; return f; }
    GUIStyle BigPrimaryStyle(){ return new GUIStyle(GUI.skin.button){ fontSize=16, fixedHeight=88, alignment=TextAnchor.MiddleCenter, wordWrap=false, padding=new RectOffset(8,8,12,12) }; }
    int MSAAIndexFrom(int v)=> v switch{1=>0,2=>1,4=>2,8=>3,_=>0};
    int MSAAValueFrom(int i)=> i switch{0=>1,1=>2,2=>4,3=>8,_=>1};

    

    

    void DrawRectBorder(Rect r, float th, Color c)
    {
        EditorGUI.DrawRect(new Rect(r.xMin, r.yMin, r.width, th), c);
        EditorGUI.DrawRect(new Rect(r.xMin, r.yMax - th, r.width, th), c);
        EditorGUI.DrawRect(new Rect(r.xMin, r.yMin, th, r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - th, r.yMin, th, r.height), c);
    }

    void AALine(Vector2 a, Vector2 b)
    {
        Handles.color = _sfLineColor;
        Handles.DrawAAPolyLine(_sfLineWidth, new Vector3[] { a, b });
    }

    Rect ScaleRect(Rect r, float s)
    {
        float w = r.width * s, h = r.height * s;
        return new Rect(r.x + (r.width - w) * 0.5f, r.y + (r.height - h) * 0.5f, w, h);
    }

    void SnapshotFreecamSpeeds()
    {
        _speedFieldSnapshot = null;
        if (!_freecam) return;
        var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        foreach (var f in _freecam.GetType().GetFields(flags))
            if (f.FieldType == typeof(float) && f.Name.ToLower().Contains("speed"))
                (_speedFieldSnapshot ??= new Dictionary<string,float>())[f.Name] = (float)f.GetValue(_freecam);
    }

    void ResetFreecamSpeedRobust()
    {
        if (!_freecam) return;
        var t = _freecam.GetType(); var flags = BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;

        var m = t.GetMethod("ResetSpeed", flags);
        if (m != null && m.GetParameters().Length == 0) { m.Invoke(_freecam, null); EditorUtility.SetDirty(_freecam); return; }

        var mSet = t.GetMethod("SetSpeed", flags) ?? t.GetMethod("SetMoveSpeed", flags) ?? t.GetMethod("SetBaseSpeed", flags);
        if (mSet != null && mSet.GetParameters().Length == 1 && mSet.GetParameters()[0].ParameterType == typeof(float))
        { mSet.Invoke(_freecam, new object[] { 5f }); EditorUtility.SetDirty(_freecam); return; }

        var p = t.GetProperty("CurrentSpeed", flags) ?? t.GetProperty("Speed", flags) ?? t.GetProperty("MoveSpeed", flags);
        if (p != null && p.CanWrite && p.PropertyType == typeof(float))
        { p.SetValue(_freecam, 5f, null); EditorUtility.SetDirty(_freecam); return; }

        if (_speedFieldSnapshot != null)
        {
            foreach (var kv in _speedFieldSnapshot)
            {
                var f = t.GetField(kv.Key, flags);
                if (f != null && f.FieldType == typeof(float))
                    f.SetValue(_freecam, kv.Value);
            }
            EditorUtility.SetDirty(_freecam);
            return;
        }

        string[] names = { "currentSpeed", "speed", "moveSpeed", "baseSpeed", "normalSpeed", "walkSpeed", "targetSpeed" };
        foreach (var n in names)
        {
            var f = t.GetField(n, flags);
            if (f != null && f.FieldType == typeof(float)) f.SetValue(_freecam, 5f);
        }
        EditorUtility.SetDirty(_freecam);
    }

    void ResetPanelHard()
    {
        _preset = AspectPreset.FHD_16_9; _portrait = false;
        _customW = 1920; _customH = 1080;
        _png = true; _jpgQuality = 95; _pngKeepAlpha = true;
        _openAfterSave = false; _lastSavedPath = null;
        _fileNameTemplate = "{scene}_{yyyyMMdd_HHmmss}";
        _rtMSAA = 1;

        _showSafeFrame=true; _sfThirds=true; _sfDiagonals=false; _sfCenterCross=true;
        _sfTitleSafe=0.90f; _sfLineWidth=2f; _sfLineColor=new Color(1,1,1,0.9f); _sfMaskAlpha=0.8f; _sfOnlyWhenTarget=false;
    }

    void RefreshCameraDisplayToDisplay1(Camera cam)
    {
        if (!cam) return;
        cam.targetDisplay = 0;
        cam.enabled = false;
        InternalEditorUtility.RepaintAllViews();
        EditorApplication.delayCall += () =>
        {
            if (!cam) return;
            cam.enabled = true;
            EditorUtility.SetDirty(cam);
            InternalEditorUtility.RepaintAllViews();
            SceneView.RepaintAll();
        };
    }

    // ───────── Pref helpers
    static Color LoadColor(string key, Color def)
    {
        Color c=def; c.r=EditorPrefs.GetFloat(key+".r",def.r);
        c.g=EditorPrefs.GetFloat(key+".g",def.g);
        c.b=EditorPrefs.GetFloat(key+".b",def.b);
        c.a=EditorPrefs.GetFloat(key+".a",def.a);
        return c;
    }
    static void SaveColor(string key, Color c)
    {
        EditorPrefs.SetFloat(key+".r",c.r);
        EditorPrefs.SetFloat(key+".g",c.g);
        EditorPrefs.SetFloat(key+".b",c.b);
        EditorPrefs.SetFloat(key+".a",c.a);
    }

    static Color Hex(string hex){ if(ColorUtility.TryParseHtmlString(hex,out var col)) return col; return new Color(0.15f,0.6f,0.35f); }
}
