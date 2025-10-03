// Assets/360/Editor/EditorScreenShotCamera.cs
// Camera management for EditorScreenShot

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using EditorScreenShot.Runtime;
using FreecamPreview;

namespace EditorScreenShot
{
    public static class EditorScreenShotCamera
    {
        // Sync camera references
        public static void SyncCameraRefs(EditorScreenShotData data, bool preferMain)
        {
            // Only use dedicated EditorScreenShot camera
            Camera editorCamera = FindEditorScreenShotCamera();
            
            if (editorCamera != null)
            {
                data.camera = editorCamera;
                data.freecam = data.camera.GetComponent<Freecam>();
                data.fisheye = data.camera.GetComponent<FisheyeImageEffect>();
                if (data.freecam) SnapshotFreecamSpeeds(data);
                
                // Ensure Scene Sync uses correct camera
                if (data.sceneSyncOn && data.camera)
                {
                    // Re-acquire SceneSync component reference to prevent referencing destroyed objects
                    data.sceneSync = data.camera.GetComponent<ESSSceneSync>();
                    if (data.sceneSync)
                    {
                        // Ensure component is enabled
                        if (!data.sceneSync.enabled)
                        {
                            data.sceneSync.enabled = true;
                            EditorUtility.SetDirty(data.sceneSync);
                        }
                        
                        data.sceneSync.targetCam = data.camera;
                        data.sceneSync.freecamController = data.freecam;
                    }
                }
            }
            else
            {
                // If no dedicated camera found, clear all references
                data.camera = null;
                data.freecam = null;
                data.fisheye = null;
                data.sceneSync = null;
            }
        }
        
        // Find EditorScreenShot camera
        public static Camera FindEditorScreenShotCamera()
        {
            // Directly search for camera named "EditorScreenShot"
            Camera[] allCameras = Camera.allCameras;
            foreach (Camera cam in allCameras)
            {
                if (cam.name == "EditorScreenShot")
                {
                    // Verify camera is valid and not destroyed
                    if (cam.gameObject != null)
                    {
                        // Ensure camera has necessary components
                        ValidateEditorCamera(cam);
                        return cam;
                    }
                }
            }
            
            return null;
        }
        
        // Validate camera components
        public static void ValidateEditorCamera(Camera camera)
        {
            if (camera == null) return;
            
            bool needsRepair = false;
            
            // Check if camera has necessary components (Prefab should already be configured)
            if (!camera.GetComponent<Freecam>())
            {
                Debug.LogWarning("[EditorScreenShot] Camera missing Freecam component, please check Prefab setup");
                needsRepair = true;
            }
            
            if (!camera.GetComponent<FisheyeImageEffect>())
            {
                Debug.LogWarning("[EditorScreenShot] Camera missing FisheyeImageEffect component, please check Prefab setup");
                needsRepair = true;
            }
            
            if (!camera.GetComponent<ESSSceneSync>())
            {
                Debug.LogWarning("[EditorScreenShot] Camera missing ESSSceneSync component, please check Prefab setup");
                needsRepair = true;
            }
            
            // Check camera settings
            if (camera.nearClipPlane != 0.01f)
            {
                Debug.LogWarning("[EditorScreenShot] Camera near clip plane setting incorrect (current: " + camera.nearClipPlane + ", should be: 0.01)");
                needsRepair = true;
            }
            
            if (camera.tag != "MainCamera")
            {
                Debug.LogWarning("[EditorScreenShot] Camera tag incorrect (current: " + camera.tag + ", should be: MainCamera)");
                needsRepair = true;
            }
            
            if (needsRepair)
            {
            }
        }
        
        // Repair camera (deprecated)
        public static void RepairEditorCamera(Camera camera)
        {
            // This method is no longer used, repair functionality changed to delete and recreate camera
            // Keep this method in case other places call it
        }

        // Snapshot freecam speeds
        public static void SnapshotFreecamSpeeds(EditorScreenShotData data)
        {
            data.speedFieldSnapshot = null;
            if (!data.freecam) return;
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var f in data.freecam.GetType().GetFields(flags))
                if (f.FieldType == typeof(float) && f.Name.ToLower().Contains("speed"))
                    (data.speedFieldSnapshot ??= new Dictionary<string, float>())[f.Name] = (float)f.GetValue(data.freecam);
        }

        // Reset freecam speed robustly
        public static void ResetFreecamSpeedRobust(EditorScreenShotData data)
        {
            if (!data.freecam) return;
            var t = data.freecam.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var m = t.GetMethod("ResetSpeed", flags);
            if (m != null && m.GetParameters().Length == 0) 
            { 
                m.Invoke(data.freecam, null); 
                EditorUtility.SetDirty(data.freecam); 
                return; 
            }

            var mSet = t.GetMethod("SetSpeed", flags) ?? t.GetMethod("SetMoveSpeed", flags) ?? t.GetMethod("SetBaseSpeed", flags);
            if (mSet != null && mSet.GetParameters().Length == 1 && mSet.GetParameters()[0].ParameterType == typeof(float))
            { 
                mSet.Invoke(data.freecam, new object[] { 5f }); 
                EditorUtility.SetDirty(data.freecam); 
                return; 
            }

            var p = t.GetProperty("CurrentSpeed", flags) ?? t.GetProperty("Speed", flags) ?? t.GetProperty("MoveSpeed", flags);
            if (p != null && p.CanWrite && p.PropertyType == typeof(float))
            { 
                p.SetValue(data.freecam, 5f, null); 
                EditorUtility.SetDirty(data.freecam); 
                return; 
            }

            if (data.speedFieldSnapshot != null)
            {
                foreach (var kv in data.speedFieldSnapshot)
                {
                    var f = t.GetField(kv.Key, flags);
                    if (f != null && f.FieldType == typeof(float))
                        f.SetValue(data.freecam, kv.Value);
                }
                EditorUtility.SetDirty(data.freecam);
                return;
            }

            string[] names = { "currentSpeed", "speed", "moveSpeed", "baseSpeed", "normalSpeed", "walkSpeed", "targetSpeed" };
            foreach (var n in names)
            {
                var f = t.GetField(n, flags);
                if (f != null && f.FieldType == typeof(float)) f.SetValue(data.freecam, 5f);
            }
            EditorUtility.SetDirty(data.freecam);
        }

        // Check for display conflicts
        public static bool HasDisplayConflict(Camera editorCamera)
        {
            if (editorCamera == null) return false;
            
            int targetDisplay = editorCamera.targetDisplay;
            Camera[] allCameras = Camera.allCameras;
            
            foreach (Camera cam in allCameras)
            {
                if (cam != editorCamera && cam.enabled && cam.targetDisplay == targetDisplay)
                {
                    return true;
                }
            }
            
            return false;
        }

        // Fix display conflicts
        public static void FixDisplayConflicts(Camera editorCamera)
        {
            if (editorCamera == null) return;
            
            int currentDisplay = editorCamera.targetDisplay;
            Camera[] allCameras = Camera.allCameras;
            List<Camera> conflictingCameras = new List<Camera>();
            
            // Find all cameras using the same Display
            foreach (Camera cam in allCameras)
            {
                if (cam != editorCamera && cam.enabled && cam.targetDisplay == currentDisplay)
                {
                    conflictingCameras.Add(cam);
                }
            }
            
            if (conflictingCameras.Count > 0)
            {
                // Show dialog for user to choose repair method
                int choice = EditorUtility.DisplayDialogComplex(Loc.T("DisplayConflictRepair"), 
                    string.Format(Loc.T("DetectedCamerasUsingDisplay"), conflictingCameras.Count, currentDisplay + 1) + "\n" +
                    Loc.T("CloseOtherCamerasOrChangeDisplay") + "\n\n" +
                    Loc.T("ChooseRepairMethod"), 
                    Loc.T("CloseOtherCameras"), Loc.T("Cancel"), Loc.T("ChangeDisplayChannel"));
                
                if (choice == 0) // Close other cameras
                {
                    // Disable all conflicting camera GameObjects
                    foreach (Camera cam in conflictingCameras)
                    {
                        if (cam != null && cam.gameObject != null)
                        {
                            cam.gameObject.SetActive(false);
                            EditorUtility.SetDirty(cam.gameObject);
                        }
                    }
                    
                    EditorUtility.DisplayDialog(Loc.T("RepairComplete"), 
                        string.Format(Loc.T("ClosedCamerasCount"), conflictingCameras.Count) + "\n\n" +
                        Loc.T("NoteCamerasDisabled"), Loc.T("OK"));
                }
                else if (choice == 2) // Change EditorScreenShot (now the third button)
                {
                    // Find available Display
                    int newDisplay = FindAvailableDisplay(editorCamera);
                    if (newDisplay != -1)
                    {
                        editorCamera.targetDisplay = newDisplay;
                        EditorUtility.SetDirty(editorCamera);
                        
                        EditorUtility.DisplayDialog(Loc.T("RepairComplete"), 
                            string.Format(Loc.T("MovedToDisplay"), newDisplay + 1), Loc.T("OK"));
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(Loc.T("RepairFailed"), 
                            Loc.T("AllDisplaysOccupied"), Loc.T("OK"));
                    }
                }
                // choice == 1 is cancel, do nothing
            }
        }
        
        // Find available display
        public static int FindAvailableDisplay(Camera editorCamera)
        {
            Camera[] allCameras = Camera.allCameras;
            bool[] displayUsed = new bool[8]; // Display 0-7
            
            // Mark all used Displays
            foreach (Camera cam in allCameras)
            {
                if (cam != editorCamera && cam.enabled)
                {
                    int display = cam.targetDisplay;
                    if (display >= 0 && display < 8)
                    {
                        displayUsed[display] = true;
                    }
                }
            }
            
            // Find first available Display
            for (int i = 0; i < 8; i++)
            {
                if (!displayUsed[i])
                {
                    return i;
                }
            }
            
            return -1; // No available Display
        }
    }
}
