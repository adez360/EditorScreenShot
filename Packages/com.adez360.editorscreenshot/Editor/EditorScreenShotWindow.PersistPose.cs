using UnityEngine;
using UnityEditor;

public partial class EditorScreenShotWindow
{
    void PersistPoseAfterSceneSync(Vector3 pos, Quaternion rot, bool ortho, float fov, float orthoSize)
    {
        if (!_cam) return;

        var over = _cam.GetComponent<ESSposeOverride>();
        if (!over) over = _cam.gameObject.AddComponent<ESSposeOverride>();

        var freecam = _cam.GetComponent<Freecam>();
        Behaviour brain = _cam.GetComponent("CinemachineBrain") as Behaviour;

        // 覆寫 40 幀，並在覆寫前主動校準 Freecam 的內部平滑目標（tyaw/tpitch/troll）
        if (freecam)
        {
            EditorScreenShot.Runtime.ControllerCalibration.TryCalibrateControllerToRotation(freecam, rot);
        }
        over.Setup(pos, rot, ortho, fov, orthoSize, 40, freecam, freecam, brain);
    }
}


