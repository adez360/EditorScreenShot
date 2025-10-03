// Assets/360/Editor/EditorScreenShotUI.cs
// UI drawing for EditorScreenShot

using UnityEngine;
using UnityEditor;
using FreecamPreview;
using EditorScreenShot.Runtime;

namespace EditorScreenShot
{
    public static class EditorScreenShotUI
    {
        // Draw top bar
        public static void DrawTopBar(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(Loc.T("Lang"), GUILayout.Width(70));
                Lang newLang = (Lang)EditorGUILayout.EnumPopup(data.lang, GUILayout.Width(200));
                if (newLang != data.lang)
                {
                    data.lang = newLang;
                    Loc.Language = data.lang;
                    EditorPrefs.SetInt("ESS.Lang", (int)data.lang);
                }
                GUILayout.FlexibleSpace();
            }
        }

        // Draw sticky header
        public static void DrawStickyHeader(EditorScreenShotData data)
        {
            using (new EditorGUI.DisabledScope(data.camera == null))
            {
                var (outW, outH) = EditorScreenShotOutput.GetCurrentWH(data);
                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = EditorScreenShotSettings.Hex("#66FFB3"); // bright green
                // Add camera icon to capture button
                Texture2D cameraIcon = Resources.Load<Texture2D>("icon");
                var buttonStyle = new GUIStyle(BigPrimaryStyle())
                {
                    imagePosition = ImagePosition.ImageAbove,
                    alignment = TextAnchor.MiddleCenter
                };
                if (GUILayout.Button(new GUIContent(Loc.T("CaptureImage"), cameraIcon), buttonStyle))
                    EditorScreenShotOutput.SaveCurrentFrame(data, outW, outH);
                GUI.backgroundColor = prev;
            }

            GUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(data.camera == null))
                {
                    string label = data.sceneSyncOn ? Loc.T("SceneSyncOn") : Loc.T("SceneSyncOff");
                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = data.sceneSyncOn ? new Color(0.23f,0.65f,0.42f) : new Color(0.45f,0.45f,0.45f);
                    if (GUILayout.Button(label, GUILayout.Height(24), GUILayout.Width(0), GUILayout.ExpandWidth(true)))
                        EditorScreenShotSceneSync.ToggleSceneSync(data, !data.sceneSyncOn);
                    GUI.backgroundColor = prev;
                }

                // Lock object button
                using (new EditorGUI.DisabledScope(data.camera == null || data.freecam == null || !Application.isPlaying))
                {
                    bool hasTarget = data.freecam && data.freecam.lockTarget;
                    bool isActive = hasTarget && data.freecam.LockActive;
                    Color prevColor = GUI.backgroundColor;
                    
                    string buttonText;
                    if (!Application.isPlaying)
                    {
                        // Show "Play Mode Only" in edit mode
                        buttonText = Loc.T("PlayModeOnly");
                        GUI.backgroundColor = new Color(0.45f, 0.45f, 0.45f); // Gray - disabled
                    }
                    else
                    {
                        // Normal state in play mode
                        GUI.backgroundColor = isActive 
                            ? new Color(0.23f, 0.65f, 0.42f)  // Green - locked
                            : (hasTarget 
                                ? new Color(0.85f, 0.55f, 0.15f)  // Orange - has target but not locked
                                : new Color(0.45f, 0.45f, 0.45f)); // Gray - no target
                        
                        buttonText = isActive ? Loc.T("Locking") : (hasTarget ? Loc.T("Unlocked") : Loc.T("NoTarget"));
                    }
                    
                    if (GUILayout.Button(buttonText, GUILayout.Height(24), GUILayout.Width(0), GUILayout.ExpandWidth(true)))
                    {
                        if (hasTarget) 
                        { 
                            bool newLockState = !data.freecam.lockLookAt;
                            data.freecam.lockLookAt = newLockState;
                            
                            // Auto-disable scene sync when lock feature is enabled
                            if (newLockState && data.sceneSyncOn)
                            {
                                EditorScreenShotSceneSync.ToggleSceneSync(data, false);
                            }
                        }
                        else 
                        { 
                            EditorUtility.DisplayDialog(Loc.T("LockDialog"), Loc.T("SelectTargetFirst"), Loc.T("OK")); 
                        }
                    }
                    GUI.backgroundColor = prevColor;
                }
            }
        }

        // Draw status block
        public static void DrawStatusBlock(EditorScreenShotData data)
        {
            var label = new GUIStyle(EditorStyles.label) { richText = true, fontSize = 12 };

            // Status
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("Status"), GUILayout.Width(120));
                Color prevColor = GUI.color;
                
                // Check if camera and all required components exist
                bool cameraOk = data.camera != null;
                bool freecamOk = data.camera != null && data.camera.GetComponent<Freecam>() != null;
                bool fisheyeOk = data.camera != null && data.camera.GetComponent<FisheyeImageEffect>() != null;
                bool sceneSyncOk = data.camera != null && data.camera.GetComponent<ESSSceneSync>() != null;
                bool displayConflict = data.camera != null && EditorScreenShotCamera.HasDisplayConflict(data.camera);
                
                bool allComponentsOk = cameraOk && freecamOk && fisheyeOk && sceneSyncOk;
                bool needsRepair = !allComponentsOk || displayConflict;
                
                if (!cameraOk)
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField(Loc.T("NoCamera"), EditorStyles.boldLabel);
                }
                else if (!allComponentsOk)
                {
                    GUI.color = Color.yellow;
                    string missingComponents = "";
                    if (!freecamOk) missingComponents += "Freecam ";
                    if (!fisheyeOk) missingComponents += "Fisheye ";
                    if (!sceneSyncOk) missingComponents += "SceneSync ";
                    EditorGUILayout.LabelField($"{Loc.T("MissingComponents")}: {missingComponents.Trim()}", EditorStyles.boldLabel);
                }
                else if (displayConflict)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(Loc.T("DisplayConflict"), EditorStyles.boldLabel);
                }
                else
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField(Loc.T("Normal"), EditorStyles.boldLabel);
                }
                
                GUI.color = prevColor;
                
                GUILayout.FlexibleSpace();
                
                // Repair button
                using (new EditorGUI.DisabledScope(!needsRepair))
                {
                    if (GUILayout.Button(Loc.T("Repair"), GUILayout.Height(20), GUILayout.Width(50)))
                    {
                        if (displayConflict)
                        {
                            // Fix Display conflict
                            EditorScreenShotCamera.FixDisplayConflicts(data.camera);
                        }
                        else
                        {
                            // Fix component issues
                            // Stop Scene Sync first
                            if (data.sceneSyncOn)
                            {
                                EditorScreenShotSceneSync.ToggleSceneSync(data, false);
                            }
                            
                            // Clear all references
                            data.camera = null;
                            data.freecam = null;
                            data.fisheye = null;
                            data.sceneSync = null;
                            
                            // Delete existing camera and create new one
                            Camera[] allCameras = Camera.allCameras;
                            foreach (Camera cam in allCameras)
                            {
                                if (cam != null && cam.name == "EditorScreenShot")
                                {
                                    UnityEngine.Object.DestroyImmediate(cam.gameObject);
                                    break;
                                }
                            }
                            
                            // Create new camera
                            EditorScreenShotWindow.EnsureEditorScreenShotSetup();
                            EditorScreenShotCamera.SyncCameraRefs(data, true);
                            
                            // Ensure ESSSceneSync component is properly enabled
                            if (data.camera != null)
                            {
                                var sceneSync = data.camera.GetComponent<ESSSceneSync>();
                                if (sceneSync != null)
                                {
                                    sceneSync.enabled = true;
                                    EditorUtility.SetDirty(sceneSync);
                                }
                            }
                        }
                    }
                }
            }

            // Camera speed
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("CameraSpeed"), GUILayout.Width(120));
                string spd = data.freecam ? data.freecam.CurrentSpeed.ToString("0.00") : "N/A";
                var speedLabel = new GUIStyle(EditorStyles.label) { richText = true };
                GUILayout.Label($"<color=#59FFA6>{spd}</color>", speedLabel);
                
                GUILayout.FlexibleSpace();
                
                // Reset button
                using (new EditorGUI.DisabledScope(data.freecam == null))
                {
                    if (GUILayout.Button(Loc.T("Reset"), GUILayout.Height(20), GUILayout.Width(50)))
                        EditorScreenShotCamera.ResetFreecamSpeedRobust(data);
                }
            }

            // Lock object selection
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("LockObject"), GUILayout.Width(120));
                
                using (new EditorGUI.DisabledScope(data.camera == null))
                {
                    Transform newT = (Transform)EditorGUILayout.ObjectField(
                        data.freecam ? data.freecam.lockTarget : null,
                        typeof(Transform), true, GUILayout.ExpandWidth(true));

                    if (data.freecam && newT != data.freecam.lockTarget)
                        data.freecam.lockTarget = newT;
                }
            }
        }

        // Draw camera row
        public static void DrawCameraRow(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("Camera"), GUILayout.Width(120));
                
                if (data.camera != null)
                {
                    // Show dedicated camera name
                    EditorGUILayout.LabelField(data.camera.name, EditorStyles.boldLabel);
                }
                else
                {
                    // Show hint message
                    EditorGUILayout.LabelField("EditorScreenShot camera not found", EditorStyles.helpBox);
                }
            }

            // Display Target 設定
            using (new EditorGUI.DisabledScope(data.camera == null))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(Loc.T("Display"), GUILayout.Width(120));
                    int currentDisplay = data.camera ? data.camera.targetDisplay : 0;
                    int newDisplay = EditorGUILayout.IntPopup(currentDisplay, 
                        new string[] { "Display 1", "Display 2", "Display 3", "Display 4", "Display 5", "Display 6", "Display 7", "Display 8" },
                        new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, GUILayout.ExpandWidth(true));
                    
                    if (data.camera && newDisplay != currentDisplay)
                    {
                        data.camera.targetDisplay = newDisplay;
                        EditorUtility.SetDirty(data.camera);
                    }
                    
                    // Check and show conflict warning
                    if (data.camera && EditorScreenShotCamera.HasDisplayConflict(data.camera))
                    {
                        Color prevColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.2f); // Orange background
                        Color prevTextColor = GUI.contentColor;
                        GUI.contentColor = Color.white; // White text
                        
                        if (GUILayout.Button(Loc.T("FixConflict"), GUILayout.Height(20), GUILayout.Width(80)))
                        {
                            EditorScreenShotCamera.FixDisplayConflicts(data.camera);
                        }
                        
                        GUI.backgroundColor = prevColor;
                        GUI.contentColor = prevTextColor;
                    }
                }
            }
        }

        // Draw lens block merged
        public static void DrawLensBlockMerged(EditorScreenShotData data)
        {
            if (!data.camera)
            {
                EditorGUILayout.HelpBox(Loc.T("Camera") + " = null", MessageType.Info);
                return;
            }

            // FOV
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("FOV"), GUILayout.Width(120));
                float fov = data.camera.fieldOfView;
                float newFov = EditorGUILayout.Slider(fov, 1f, 179f);
                if (!Mathf.Approximately(newFov, fov)) data.camera.fieldOfView = newFov;
            }

            // Physical lens (if enabled)
            if (data.camera.usePhysicalProperties)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(Loc.T("Focal"), GUILayout.Width(120));
                    float focal = data.camera.focalLength;
                    float sensorH = data.camera.sensorSize.y;
                    float newFocal = EditorGUILayout.Slider(focal, 1f, 300f);
                    if (!Mathf.Approximately(newFocal, focal))
                    {
                        data.camera.focalLength = newFocal;
                        data.camera.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan((0.5f * sensorH) / newFocal);
                    }
                }
            }

            // Fisheye
            using (new EditorGUI.DisabledScope(data.camera == null))
            {
                // Fisheye component should already be configured in Prefab
                if (!data.fisheye) data.fisheye = data.camera.GetComponent<FisheyeImageEffect>();
                
                if (data.fisheye)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(Loc.T("Fisheye"), GUILayout.Width(120));
                        float strength = data.fisheye.strength;
                        float newStrength = EditorGUILayout.Slider(strength, 0f, 1f);
                        
                        // Update fisheye strength when slider value changes
                        if (!Mathf.Approximately(newStrength, strength))
                        {
                            data.fisheye.strength = newStrength;
                            // Auto-disable fisheye when strength < 0.01
                            data.fisheye.enabled = newStrength >= 0.01f;
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(Loc.T("Fisheye"), GUILayout.Width(120));
                        EditorGUILayout.LabelField(Loc.T("ComponentMissing"), EditorStyles.helpBox);
                    }
                }
            }
        }

        // Draw output toggles row
        public static void DrawOutputTogglesRow(EditorScreenShotData data)
        {
            // Transparent background option moved below format
        }

        // Draw resolution row
        public static void DrawResolutionRow(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("Resolution"), GUILayout.Width(120));
                string[] names = { Loc.T("UHD_4K_16_9"), Loc.T("FHD_16_9"), Loc.T("QHD_16_9"), Loc.T("HD_16_9"), Loc.T("Square_1_1"), Loc.T("Custom") };
                int sel = (int)data.preset, newSel = EditorGUILayout.Popup(sel, names);
                if (newSel != sel) data.preset = (AspectPreset)newSel;
                data.portrait = GUILayout.Toggle(data.portrait, Loc.T("Portrait"), GUILayout.Width(90));
            }
            if (data.preset == AspectPreset.Custom)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(Loc.T("WidthShort"), GUILayout.Width(30));
                    data.customW = Mathf.Max(8, EditorGUILayout.IntField(data.customW, GUILayout.Width(80)));
                    GUILayout.Space(8);
                    GUILayout.Label(Loc.T("HeightShort"), GUILayout.Width(30));
                    data.customH = Mathf.Max(8, EditorGUILayout.IntField(data.customH, GUILayout.Width(80)));
                    GUILayout.FlexibleSpace();
                }
            }
            var (cw, ch) = EditorScreenShotOutput.GetCurrentWH(data);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(120); // Align with label width
                var label = new GUIStyle(EditorStyles.label) { richText = true };
                GUILayout.Label($"<color=#59FFA6>{cw}x{ch}</color>", label);
            }
        }

        // Draw format row
        public static void DrawFormatRow(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("Format"), GUILayout.Width(120));
                data.png = GUILayout.Toggle(data.png, "PNG", "Button", GUILayout.Width(60));
                bool jpg = GUILayout.Toggle(!data.png, "JPEG", "Button", GUILayout.Width(60));
                if (jpg) data.png = false;
                GUILayout.FlexibleSpace();
            }
            
            // Transparent background option
            if (data.png)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(Loc.T("TransparentBG"), GUILayout.Width(120));
                    data.pngKeepAlpha = GUILayout.Toggle(data.pngKeepAlpha, "", GUILayout.Width(20));
                    GUILayout.FlexibleSpace();
                }
            }
            
            if (!data.png)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(Loc.T("JPEGQuality"), GUILayout.Width(120));
                    data.jpgQuality = EditorGUILayout.IntSlider(data.jpgQuality, 1, 100);
                }
            }
        }

        // Draw path row
        public static void DrawPathRow(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SaveLocation"), GUILayout.Width(120));
                EditorGUILayout.SelectableLabel(data.outDir, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                // Add folder icon to browse button
                Texture2D browseIcon = EditorGUIUtility.IconContent("Folder").image as Texture2D;
                if (GUILayout.Button(new GUIContent(Loc.T("Browse"), browseIcon), GUILayout.Width(90)))
                {
                    var p = EditorUtility.OpenFolderPanel(Loc.T("Browse"), data.outDir, "");
                    if (!string.IsNullOrEmpty(p)) data.outDir = p;
                }
            }
        }

        // Draw file name template row
        public static void DrawFileNameTemplateRow(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("FileNameTemplate"), GUILayout.Width(120));
                data.fileNameTemplate = EditorGUILayout.TextField(data.fileNameTemplate);
            }
        }

        // Draw quality row
        public static void DrawQualityRow(EditorScreenShotData data)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("MSAA"), GUILayout.Width(120));
                int sel = MSAAIndexFrom(data.rtMSAA);
                int newSel = EditorGUILayout.Popup(sel, new[] { "1x", "2x", "4x", "8x" }, GUILayout.Width(100));
                data.rtMSAA = MSAAValueFrom(newSel);
                GUILayout.FlexibleSpace();
            }
        }


        // UI helper methods
        public static GUIStyle StyledBox() 
        { 
            var s = new GUIStyle("box"); 
            s.padding = new RectOffset(10,10,10,10); 
            return s; 
        }
        
        public static GUIStyle FoldoutTitleStyle()
        { 
            var f = new GUIStyle(EditorStyles.foldoutHeader){fontStyle = FontStyle.Bold}; 
            return f; 
        }
        
        public static GUIStyle BigPrimaryStyle()
        { 
            return new GUIStyle(GUI.skin.button){ 
                fontSize=16, 
                fixedHeight=88, 
                alignment=TextAnchor.MiddleCenter, 
                wordWrap=false, 
                padding=new RectOffset(8,8,12,12) 
            }; 
        }
        
        public static int MSAAIndexFrom(int v) => v switch{1=>0,2=>1,4=>2,8=>3,_=>0};
        public static int MSAAValueFrom(int i) => i switch{0=>1,1=>2,2=>4,3=>8,_=>1};

        // Draw rect border
        public static void DrawRectBorder(Rect r, float th, Color c)
        {
            EditorGUI.DrawRect(new Rect(r.xMin, r.yMin, r.width, th), c);
            EditorGUI.DrawRect(new Rect(r.xMin, r.yMax - th, r.width, th), c);
            EditorGUI.DrawRect(new Rect(r.xMin, r.yMin, th, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - th, r.yMin, th, r.height), c);
        }

        // AA line
        public static void AALine(Vector2 a, Vector2 b, Color lineColor, float lineWidth)
        {
            Handles.color = lineColor;
            Handles.DrawAAPolyLine(lineWidth, new Vector3[] { a, b });
        }

        // Scale rect
        public static Rect ScaleRect(Rect r, float s)
        {
            float w = r.width * s, h = r.height * s;
            return new Rect(r.x + (r.width - w) * 0.5f, r.y + (r.height - h) * 0.5f, w, h);
        }


    }
}
