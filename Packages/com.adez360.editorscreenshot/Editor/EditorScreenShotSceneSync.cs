// Assets/360/Editor/EditorScreenShotSceneSync.cs
// Scene sync management for EditorScreenShot

using UnityEngine;
using UnityEditor;
using EditorScreenShot.Runtime;
using FreecamPreview;

namespace EditorScreenShot
{
    public static class EditorScreenShotSceneSync
    {
        // Toggle scene sync
        public static void ToggleSceneSync(EditorScreenShotData data, bool enable, System.Action<Vector3, Quaternion, bool, float, float> persistPoseCallback = null)
        {
            if (!data.camera) 
            { 
                data.sceneSyncOn = false; 
                return; 
            }

#if UNITY_EDITOR
            if (enable)
            {
                var sv = SceneView.lastActiveSceneView;
                if (sv == null || sv.camera == null)
                {
                    EditorUtility.DisplayDialog("Scene Sync", "Open a Scene view first.", "OK");
                    data.sceneSyncOn = false;
                    return;
                }

                // Ensure camera refs are up to date
                EditorScreenShotCamera.SyncCameraRefs(data, true);
                if (!data.camera) 
                { 
                    data.sceneSyncOn = false; 
                    return; 
                }

                data.sceneSync = data.camera.GetComponent<ESSSceneSync>();
                if (!data.sceneSync)
                {
                    Debug.LogError("[EditorScreenShot] Camera missing ESSSceneSync component, please check Prefab setup");
                    return;
                }

                // Auto-disable lock feature when scene sync is enabled
                if (data.freecam && data.freecam.lockLookAt)
                {
                    data.freecam.lockLookAt = false;
                }

                Behaviour brain = data.camera.GetComponent("CinemachineBrain") as Behaviour;
                data.sceneSync.Begin(data.camera, data.freecam, brain);
                data.sceneSyncOn = true;
                
                // Ensure component works in edit mode
                if (!data.sceneSync.enabled) data.sceneSync.enabled = true;
                if (!data.sceneSync.gameObject.activeInHierarchy) data.sceneSync.gameObject.SetActive(true);
                
                // Force repaint to update UI
                SceneView.RepaintAll();
            }
            else
            {
#if UNITY_EDITOR
                // Get current camera pose before ending sync
                // In play mode, use camera's current pose; in edit mode, use SceneView pose
                Vector3 pos;
                Quaternion rot;
                bool ortho;
                float fov;
                float orthoSize;
                
                if (Application.isPlaying)
                {
                    // In play mode, use camera's current transform
                    pos = data.camera ? data.camera.transform.position : Vector3.zero;
                    rot = data.camera ? data.camera.transform.rotation : Quaternion.identity;
                    ortho = data.camera ? data.camera.orthographic : false;
                    fov = data.camera ? data.camera.fieldOfView : 60f;
                    orthoSize = data.camera ? data.camera.orthographicSize : 5f;
                }
                else
                {
                    // In edit mode, use SceneView pose
                    var sv = SceneView.lastActiveSceneView;
                    pos = sv != null && sv.camera != null ? sv.camera.transform.position : (data.camera ? data.camera.transform.position : Vector3.zero);
                    rot = sv != null ? sv.rotation : (data.camera ? data.camera.transform.rotation : Quaternion.identity);
                    ortho = sv != null && sv.orthographic;
                    fov = sv != null ? sv.cameraSettings.fieldOfView : (data.camera ? data.camera.fieldOfView : 60f);
                    orthoSize = sv != null ? sv.size : (data.camera ? data.camera.orthographicSize : 5f);
                }
#endif
#if UNITY_EDITOR
                // Call persist pose callback BEFORE ending sync
                if (persistPoseCallback != null)
                {
                    persistPoseCallback(pos, rot, ortho, fov, orthoSize);
                }
#endif
                if (data.sceneSync != null) data.sceneSync.End();
                data.sceneSyncOn = false;
                data.sceneSync = null; // Clear reference
            }
#else
            data.sceneSyncOn = false;
#endif
        }



        // Force enable scene sync with retry
        public static void EnsureSceneSyncEnabled(EditorScreenShotData data)
        {
            if (data == null) return;
            
            int attempts = 0;
            void TryEnable()
            {
                if (data == null) return;
                // Ensure camera is available
                if (data.camera == null) EditorScreenShotCamera.SyncCameraRefs(data, true);
                // Ensure SceneView exists
                var sv = SceneView.lastActiveSceneView;
                if (sv == null || sv.camera == null)
                {
                    if (attempts++ < 3) 
                    { 
                        EditorApplication.delayCall += TryEnable; 
                        return; 
                    }
                    return;
                }
                
                // Force reset: disable then enable
                if (data.sceneSyncOn) ToggleSceneSync(data, false);
                EditorApplication.delayCall += () => {
                    if (data != null) {
                        // Ensure camera refs are up to date
                        EditorScreenShotCamera.SyncCameraRefs(data, true);
                        ToggleSceneSync(data, true);
                        
                        // Force repaint and update SceneView
                        SceneView.RepaintAll();
                        
                        // Extra delay to ensure sync starts
                        EditorApplication.delayCall += () => {
                            if (data != null) {
                                SceneView.RepaintAll();
                            }
                        };
                    }
                };
            }
            EditorApplication.delayCall += TryEnable;
        }
    }
}
