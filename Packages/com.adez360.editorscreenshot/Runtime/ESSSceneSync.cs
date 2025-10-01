// ESSSceneSync.cs
// Runtime helper (works only in Editor). Keeps Camera pose synced to SceneView
// while pausing other camera controllers. Safe to include in builds (does nothing).
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using EditorScreenShot.Runtime;

public class ESSSceneSync : MonoBehaviour
{
    public Camera targetCam;
    public MonoBehaviour freecamController;     // optional: your Freecam script
    public Behaviour cinemachineBrain;          // optional
    public bool syncing { get; private set; }

    readonly List<Behaviour> _paused = new List<Behaviour>();
    bool _clearedOnce;

    public void Begin(Camera cam, MonoBehaviour freecam, Behaviour brain, params Behaviour[] alsoPause)
    {
        targetCam = cam;
        freecamController = freecam;
        cinemachineBrain = brain;

        syncing = true;
        enabled = true;

        _paused.Clear();
        PauseIfNonNull(freecam as Behaviour);
        PauseIfNonNull(brain);
        if (alsoPause != null) foreach (var b in alsoPause) PauseIfNonNull(b);

        // zero velocity-like fields once to remove momentum
        ControllerCalibration.TryZeroVelocityLikeFields(freecam);

        // first snap immediately
        SnapToSceneView();
        
        // Ensure component is in correct state
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (!enabled) enabled = true;
        
        // Use EditorApplication.update for edit mode
        UnityEditor.EditorApplication.update += OnEditorUpdate;
    }

    public void End()
    {
        syncing = false;
        enabled = false;
        ResumePaused();
        
        // Remove EditorApplication.update callback
        UnityEditor.EditorApplication.update -= OnEditorUpdate;
    }

    void PauseIfNonNull(Behaviour b) { if (b && b.enabled) { b.enabled = false; _paused.Add(b); } }
    void ResumePaused() { for (int i = 0; i < _paused.Count; i++) if (_paused[i]) _paused[i].enabled = true; _paused.Clear(); }

#if UNITY_EDITOR
    void LateUpdate()
    {
        if (!syncing) return;
        if (!targetCam) { End(); return; }
        
        // Ensure camera is still valid and enabled
        if (!targetCam.gameObject.activeInHierarchy || !targetCam.enabled)
        {
            End();
            return;
        }
        
        SnapToSceneView();
    }
    
    void Update()
    {
        // LateUpdate may be unreliable in edit mode, use Update as backup
        if (!syncing) return;
        if (!targetCam) { End(); return; }
        
        if (!targetCam.gameObject.activeInHierarchy || !targetCam.enabled)
        {
            End();
            return;
        }
        
        SnapToSceneView();
    }
    
    void OnEditorUpdate()
    {
        // Use EditorApplication.update for edit mode sync
        if (!syncing) return;
        if (!targetCam) { End(); return; }
        
        if (!targetCam.gameObject.activeInHierarchy || !targetCam.enabled)
        {
            End();
            return;
        }
        
        SnapToSceneView();
    }

    void SnapToSceneView()
    {
        // Try multiple ways to get SceneView
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if (sv == null)
        {
            var allSceneViews = UnityEditor.SceneView.sceneViews;
            if (allSceneViews != null && allSceneViews.Count > 0)
                sv = allSceneViews[0] as UnityEditor.SceneView;
        }
        
        if (sv == null || sv.camera == null) return;

        // one-time cleanup of controllers' internals (yaw/pitch etc.)
        if (!_clearedOnce && freecamController)
        {
            ControllerCalibration.TryCalibrateYawPitch(freecamController, sv.rotation);
            _clearedOnce = true;
        }

        var t = targetCam.transform;
        t.position = sv.camera.transform.position;
        t.rotation = sv.rotation;

        targetCam.orthographic = sv.orthographic;
        if (sv.orthographic) targetCam.orthographicSize = sv.size;
        else                  targetCam.fieldOfView      = sv.cameraSettings.fieldOfView;
        
        // Force mark as dirty to save changes
        UnityEditor.EditorUtility.SetDirty(targetCam);
        UnityEditor.EditorUtility.SetDirty(t);
        
        // Force repaint SceneView to show updates
        sv.Repaint();
    }

    static float NormalizeAngle(float a){ a%=360f; if(a>180f)a-=360f; if(a<-180f)a+=360f; return a; }
#else
    void LateUpdate() { /* no-op in player build */ }
#endif
}
