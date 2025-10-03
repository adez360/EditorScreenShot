using UnityEngine;
using UnityEditor;
using EditorScreenShot;
using EditorScreenShot.Runtime;

namespace EditorScreenShot
{
    public partial class EditorScreenShotWindow
    {
    void PersistPoseAfterSceneSync(Vector3 pos, Quaternion rot, bool ortho, float fov, float orthoSize)
    {
        if (!_data.camera) return;

        // In play mode, we want to keep the current camera pose, not override it
        if (Application.isPlaying)
        {
            // Use delayCall to ensure this runs after ESSceneSync.End()
            EditorApplication.delayCall += () =>
            {
                if (!_data.camera) return;
                
                // Force set the camera transform to maintain current pose
                _data.camera.transform.position = pos;
                _data.camera.transform.rotation = rot;
                _data.camera.orthographic = ortho;
                if (ortho)
                    _data.camera.orthographicSize = orthoSize;
                else
                    _data.camera.fieldOfView = fov;
                
            };
            return;
        }

        // In edit mode, use the pose override system
        var over = _data.camera.GetComponent<ESSposeOverride>();
        if (!over) over = _data.camera.gameObject.AddComponent<ESSposeOverride>();

        var freecam = _data.camera.GetComponent<Freecam>();
        Behaviour brain = _data.camera.GetComponent("CinemachineBrain") as Behaviour;

        // Override for 40 frames, and actively calibrate Freecam's internal smooth targets (tyaw/tpitch/troll) before override
        if (freecam)
        {
            EditorScreenShot.Runtime.ControllerCalibration.TryCalibrateControllerToRotation(freecam, rot);
        }
        over.Setup(pos, rot, ortho, fov, orthoSize, 40, freecam, freecam, brain);
    }
    }
}

