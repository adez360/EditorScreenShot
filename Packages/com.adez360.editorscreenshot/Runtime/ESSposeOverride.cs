// ESSposeOverride.cs (runtime helper)
// Forces camera pose for N frames *very late*, and also tries to calibrate the
// controller's internal yaw/pitch/targets so it stops fighting afterwards.

using UnityEngine;
using System.Reflection;
using EditorScreenShot.Runtime;

[DefaultExecutionOrder(32760)] // extremely late
public class ESSposeOverride : MonoBehaviour
{
    Vector3 _pos;
    Quaternion _rot;
    bool _ortho;
    float _fov;
    float _orthoSize;

    int _framesLeft;
    Behaviour _pauseA; // e.g., Freecam
    Behaviour _pauseB; // e.g., CinemachineBrain
    MonoBehaviour _controllerToCalibrate;

    bool _armed;
    bool _calibratedOnce;

    /// <summary>Configure and arm the override for given frames.</summary>
    public void Setup(Vector3 pos, Quaternion rot, bool orthographic, float fov, float orthoSize, int frames,
                      MonoBehaviour controllerToCalibrate, params Behaviour[] toPause)
    {
        _pos = pos; _rot = rot; _ortho = orthographic; _fov = fov; _orthoSize = orthoSize;
        _framesLeft = Mathf.Max(1, frames);
        _controllerToCalibrate = controllerToCalibrate;

        _pauseA = (toPause != null && toPause.Length > 0) ? toPause[0] : null;
        _pauseB = (toPause != null && toPause.Length > 1) ? toPause[1] : null;

        if (_pauseA) _pauseA.enabled = false;
        if (_pauseB) _pauseB.enabled = false;

        _armed = true;
        _calibratedOnce = false;
        enabled = true;
    }

    void OnDisable() { RestoreControllers(); }

    void LateUpdate()
    {
        if (!_armed) return;
        ApplyPose();

        // do calibration once after we have posed (so controller's math aligns)
        if (!_calibratedOnce && _controllerToCalibrate)
        {
            ControllerCalibration.TryCalibrateControllerToRotation(_controllerToCalibrate, _rot);
            _calibratedOnce = true;
        }

        TickDown();
    }

    void OnPreCull() // in case something writes in LateUpdate, we still win before render
    {
        if (!_armed) return;
        ApplyPose();
    }

    void ApplyPose()
    {
        var cam = GetComponent<Camera>();
        if (!cam) { _framesLeft = 0; _armed = false; RestoreControllers(); enabled = false; return; }

        cam.transform.position = _pos;
        cam.transform.rotation = _rot;
        cam.orthographic = _ortho;
        if (_ortho) cam.orthographicSize = _orthoSize; else cam.fieldOfView = _fov;
    }

    void TickDown()
    {
        _framesLeft--;
        if (_framesLeft <= 0)
        {
            _armed = false;
            RestoreControllers();
            // keep component for reuse; disable itself
            enabled = false;
        }
    }

    void RestoreControllers()
    {
        if (_pauseA) _pauseA.enabled = true;
        if (_pauseB) _pauseB.enabled = true;
        _pauseA = null; _pauseB = null;
    }

    static float NormalizeAngle(float a)
    {
        a %= 360f; if (a > 180f) a -= 360f; if (a < -180f) a += 360f; return a;
    }
}
