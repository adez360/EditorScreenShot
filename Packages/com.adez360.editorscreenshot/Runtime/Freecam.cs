using UnityEngine;
using EditorScreenShot.Runtime;

// Force lock cursor + R toggle aim (Legacy Input)
public class Freecam : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f, sprintSpeed = 12f, slowSpeed = 2f;
    public float acceleration = 12f, idleDamping = 20f;
    public float minSpeed = 0.1f, maxSpeed = 50f, scrollSpeedStep = 1f;

    [Header("Look")]
    public bool  holdRightMouseToLook = true;
    public float lookSensitivity = 2f, lookSmoothing = 12f;
    public float pitchMin = -89f, pitchMax = 89f;

    [Header("Roll (Z/C, X to level)")]
    public float rollSpeed = 90f, rollSmoothing = 12f;

    [Header("Keys")]
    public KeyCode forwardKey = KeyCode.W, backKey = KeyCode.S, leftKey = KeyCode.A, rightKey = KeyCode.D;
    public KeyCode upKey = KeyCode.E, downKey = KeyCode.Q, sprintKey = KeyCode.LeftShift, slowKey = KeyCode.LeftControl;
    public KeyCode rollLeftKey = KeyCode.Z, rollRightKey = KeyCode.C, rollResetKey = KeyCode.X;

    [Header("Lock-On (optional)")]
    public Transform lockTarget;
    public bool lockLookAt = false;          // R toggles this
    public bool followTarget = false, zeroRollWhenLocked = true;
    public Vector3 followOffset = new Vector3(0, 1.6f, -3);
    public float followPosSmoothing = 6f;

    [Header("Safety")]
    public int ignoreMouseFramesOnEnter = 2;
    public bool resetAxesOnEnter = true;

    Vector3 vel;
    float yaw, pitch, roll, tyaw, tpitch, troll, curSpeed;
    bool looking; int ignoreFrames;

    public float CurrentSpeed => curSpeed;
    public bool  LockActive   => lockLookAt && lockTarget != null; // For panel display

    void Start()
    {
        var e = transform.rotation.eulerAngles;
        yaw = tyaw = e.y;
        pitch = tpitch = Mathf.DeltaAngle(0, e.x);
        roll = troll = Mathf.DeltaAngle(0, e.z);
        curSpeed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
        HardSetLook(false);
    }

    void Update()
    {
        // ---- Hold RMB to look ----
        if (holdRightMouseToLook)
        {
            if (Input.GetMouseButtonDown(1)) HardSetLook(true);
            if (Input.GetMouseButtonUp(1))   HardSetLook(false);
        }
        else if (!looking) HardSetLook(true);

        // ---- Mouse look ----
        if (looking)
        {
            if (ignoreFrames > 0) { ignoreFrames--; _ = Input.GetAxis("Mouse X"); _ = Input.GetAxis("Mouse Y"); }
            else if (!LockActive)  // Only use mouse when not locked
            {
                float mx = Input.GetAxisRaw("Mouse X");
                float my = Input.GetAxisRaw("Mouse Y");
                tyaw   += mx * lookSensitivity;
                tpitch  = Mathf.Clamp(tpitch - my * lookSensitivity, pitchMin, pitchMax);
            }
        }

        // ---- Auto-aim when locked ----
        if (LockActive)
        {
            Vector3 to = lockTarget.position - transform.position;
            if (to.sqrMagnitude > 1e-6f)
            {
                var a = Quaternion.LookRotation(to, Vector3.up).eulerAngles;
                tyaw = a.y; tpitch = Mathf.DeltaAngle(0f, a.x);
                if (zeroRollWhenLocked) troll = 0f;
            }
        }

        // ---- Roll ----
        float dt = Time.unscaledDeltaTime;
        if (Input.GetKey(rollLeftKey))  troll += rollSpeed * dt;
        if (Input.GetKey(rollRightKey)) troll -= rollSpeed * dt;
        if (Input.GetKeyDown(rollResetKey)) troll = 0f;
        troll = Mathf.DeltaAngle(0f, troll);

        // Smooth rotation
        float kLook = lookSmoothing > 0 ? 1f - Mathf.Exp(-lookSmoothing * dt) : 1f;
        float kRoll = rollSmoothing > 0 ? 1f - Mathf.Exp(-rollSmoothing * dt) : 1f;
        yaw   = Mathf.LerpAngle(yaw,   tyaw,   kLook);
        pitch = Mathf.LerpAngle(pitch, tpitch, kLook);
        roll  = Mathf.LerpAngle(roll,  troll,  kRoll);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f) * Quaternion.AngleAxis(roll, Vector3.forward);

        // ---- Move ----
        float fwd = (Input.GetKey(forwardKey)?1:0) - (Input.GetKey(backKey)?1:0);
        float str = (Input.GetKey(rightKey)?1:0)  - (Input.GetKey(leftKey)?1:0);
        float upv = (Input.GetKey(upKey)?1:0)     - (Input.GetKey(downKey)?1:0);
        Vector3 wishLocal = Vector3.ClampMagnitude(new Vector3(str, upv, fwd), 1f);
        Vector3 wishWorld = transform.TransformDirection(wishLocal);

        float baseSpeed = curSpeed;
        if (Input.GetKey(sprintKey)) baseSpeed = Mathf.Max(baseSpeed, sprintSpeed);
        else if (Input.GetKey(slowKey)) baseSpeed = Mathf.Min(baseSpeed, slowSpeed);

        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.0001f) curSpeed = Mathf.Clamp(curSpeed + wheel * scrollSpeedStep, minSpeed, maxSpeed);

        Vector3 desired = wishWorld * baseSpeed;
        float kAcc = 1f - Mathf.Exp(-acceleration * dt);
        vel = Vector3.Lerp(vel, desired, kAcc);
        if (wishLocal.sqrMagnitude < 1e-6f)
            vel = Vector3.Lerp(vel, Vector3.zero, 1f - Mathf.Exp(-idleDamping * dt));

        transform.position += vel * dt;

        // Follow (optional)
        if (followTarget && lockTarget)
        {
            Vector3 p = lockTarget.TransformPoint(followOffset);
            float kp = 1f - Mathf.Exp(-followPosSmoothing * dt);
            transform.position = Vector3.Lerp(transform.position, p, kp);
        }

        // ---- Hotkeys ----
        if (Input.GetKeyDown(KeyCode.R)) ToggleLockLook();             // R: toggle aim
        if (Input.GetKeyDown(KeyCode.P)) EditorShotService.Current.Capture();
    }

    void LateUpdate()
    {
        // Force cursor state every frame
        if (looking)
        {
            if (Cursor.lockState != CursorLockMode.Locked) Cursor.lockState = CursorLockMode.Locked;
            if (Cursor.visible) Cursor.visible = false;
        }
        else
        {
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible) Cursor.visible = true;
        }
    }

    // Editor Fallback (avoid Editor consuming MouseUp/Down)
    void OnGUI()
    {
        if (!Application.isPlaying || !holdRightMouseToLook) return;
        var e = Event.current; if (e == null) return;
        if (e.type == EventType.MouseDown && e.button == 1) HardSetLook(true);
        if (e.type == EventType.MouseUp   && e.button == 1) HardSetLook(false);
    }

    void HardSetLook(bool enable)
    {
        looking = enable;
        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !enable;
        if (enable)
        {
            if (resetAxesOnEnter) Input.ResetInputAxes();
            ignoreFrames = Mathf.Max(0, ignoreMouseFramesOnEnter);
        }
    }

    public void ToggleLockLook()
    {
        if (!lockTarget) { lockLookAt = false; return; }
        lockLookAt = !lockLookAt;
        if (lockLookAt)
        {
            // Immediately aim once to avoid jumping
            var a = Quaternion.LookRotation(lockTarget.position - transform.position, Vector3.up).eulerAngles;
            tyaw = a.y; tpitch = Mathf.DeltaAngle(0f, a.x);
            if (zeroRollWhenLocked) troll = 0f;
        }
    }
}
