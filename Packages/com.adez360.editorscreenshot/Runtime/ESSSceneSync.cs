using UnityEngine;
using UnityEditor;
using System;

namespace EditorScreenShot.Runtime
{
    /// <summary>
    /// Scene synchronization component that syncs camera with SceneView
    /// </summary>
    public class ESSSceneSync : MonoBehaviour
    {
        [Header("Scene Sync Settings")]
        public bool syncing = false;
        
        [Header("References")]
        public Camera targetCam;
        public Freecam freecamController;
        
        private Camera _camera;
        private Freecam _freecam;
        private Behaviour _cinemachineBrain;
        private bool _wasEnabled;
        
        void Awake()
        {
            _camera = GetComponent<Camera>();
        }
        
        /// <summary>
        /// Begin scene synchronization
        /// </summary>
        public void Begin(Camera camera, Freecam freecam, Behaviour cinemachineBrain)
        {
            if (syncing) return;
            
            _camera = camera;
            _freecam = freecam;
            _cinemachineBrain = cinemachineBrain;
            
            // Store original enabled state
            _wasEnabled = enabled;
            
            // Enable this component
            enabled = true;
            syncing = true;
            
            // Disable other camera controllers
            if (_freecam) _freecam.enabled = false;
            if (_cinemachineBrain) _cinemachineBrain.enabled = false;
            
#if UNITY_EDITOR
            // In edit mode, start EditorApplication.update callback immediately
            if (!Application.isPlaying)
            {
                EditorApplication.update += SyncWithSceneView;
            }
#endif
            
        }
        
        /// <summary>
        /// End scene synchronization
        /// </summary>
        public void End()
        {
            if (!syncing) return;
            
            syncing = false;
            
#if UNITY_EDITOR
            // Remove EditorApplication.update callback in edit mode
            if (!Application.isPlaying)
            {
                EditorApplication.update -= SyncWithSceneView;
            }
#endif
            
            // Re-enable other camera controllers
            // In play mode, don't re-enable Freecam to avoid pose reset
            if (!Application.isPlaying)
            {
                if (_freecam) _freecam.enabled = true;
                if (_cinemachineBrain) _cinemachineBrain.enabled = true;
            }
            else
            {
                // In play mode, only re-enable CinemachineBrain if it was enabled before
                if (_cinemachineBrain) _cinemachineBrain.enabled = true;
                // Keep Freecam disabled to maintain current pose
            }
            
            // Restore original enabled state
            enabled = _wasEnabled;
            
        }
        
        void Update()
        {
            if (!syncing) return;
            
            // In play mode, Update() works normally
            SyncWithSceneView();
        }
        
        void SyncWithSceneView()
        {
            if (!syncing) return;
            
#if UNITY_EDITOR
            // Sync with SceneView
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
            {
                // Sync position and rotation
                _camera.transform.position = sceneView.camera.transform.position;
                _camera.transform.rotation = sceneView.camera.transform.rotation;
                
                // Sync camera settings
                _camera.orthographic = sceneView.orthographic;
                if (sceneView.orthographic)
                {
                    _camera.orthographicSize = sceneView.size;
                }
                else
                {
                    _camera.fieldOfView = sceneView.cameraSettings.fieldOfView;
                }
            }
#endif
        }
        
        void OnDisable()
        {
            // Auto-end sync when component is disabled
            if (syncing)
            {
                End();
            }
        }
        
        void OnDestroy()
        {
            // Clean up when component is destroyed
            if (syncing)
            {
                End();
            }
            
#if UNITY_EDITOR
            // Ensure EditorApplication.update callback is removed
            EditorApplication.update -= SyncWithSceneView;
#endif
        }
    }
}
