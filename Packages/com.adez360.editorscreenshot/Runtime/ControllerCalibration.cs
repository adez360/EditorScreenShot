using UnityEngine;
using System.Reflection;

namespace EditorScreenShot.Runtime
{
    /// <summary>
    /// Utilities to zero velocities and calibrate yaw/pitch or rotation on arbitrary camera controllers via reflection.
    /// </summary>
    public static class ControllerCalibration
    {
        public static void TryZeroVelocityLikeFields(MonoBehaviour ctrl)
        {
            if (!ctrl) return;
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var f in ctrl.GetType().GetFields(flags))
            {
                string n = f.Name.ToLower();
                if (f.FieldType == typeof(Vector3) && (n.Contains("velocity") || n.Contains("vel") || n.Contains("angular")))
                    f.SetValue(ctrl, Vector3.zero);
            }
        }

        public static void TryCalibrateYawPitch(MonoBehaviour ctrl, Quaternion worldRot)
        {
            if (!ctrl) return;
            var t = ctrl.GetType();

            Vector3 e = worldRot.eulerAngles;
            float yaw = e.y;
            float pitch = NormalizeAngle(e.x);

            InvokeIfExists(t, ctrl, "SetYawPitch", new object[] { yaw, pitch });
            InvokeIfExists(t, ctrl, "SnapToRotation", new object[] { worldRot });
            SetPropIfExists(t, ctrl, "Yaw", yaw);
            SetPropIfExists(t, ctrl, "Pitch", pitch);
            SetFieldIfExists(t, ctrl, "yaw", yaw);
            SetFieldIfExists(t, ctrl, "pitch", pitch);
        }

        public static void TryCalibrateControllerToRotation(MonoBehaviour ctrl, Quaternion worldRot)
        {
            if (!ctrl) return;
            var t = ctrl.GetType();

            Vector3 e = worldRot.eulerAngles;
            float yaw = e.y;
            float pitch = NormalizeAngle(e.x);

            InvokeIfExists(t, ctrl, "SetYawPitch", new object[] { yaw, pitch });
            InvokeIfExists(t, ctrl, "SetAngles",   new object[] { yaw, pitch });
            InvokeIfExists(t, ctrl, "SnapToRotation", new object[] { worldRot });
            InvokeIfExists(t, ctrl, "SetRotation",    new object[] { worldRot });
            InvokeIfExists(t, ctrl, "SetLookAngles",  new object[] { yaw, pitch });

            SetPropIfExists(t, ctrl, "Yaw",   yaw);
            SetPropIfExists(t, ctrl, "Pitch", pitch);
            SetPropIfExists(t, ctrl, "YawPitch", new Vector2(yaw, pitch));
            SetPropIfExists(t, ctrl, "Angles",   new Vector2(yaw, pitch));
            SetPropIfExists(t, ctrl, "Rotation", worldRot);
            SetPropIfExists(t, ctrl, "TargetRotation", worldRot);

            SetFieldIfExists(t, ctrl, "yaw", yaw);
            SetFieldIfExists(t, ctrl, "m_Yaw", yaw);
            SetFieldIfExists(t, ctrl, "pitch", pitch);
            SetFieldIfExists(t, ctrl, "m_Pitch", pitch);
            // also set target/smoothed fields if present to avoid drift after resume
            SetFieldIfExists(t, ctrl, "tyaw", yaw);
            SetFieldIfExists(t, ctrl, "tpitch", pitch);
            SetFieldIfExists(t, ctrl, "troll", 0f);
            SetFieldIfExists(t, ctrl, "look", new Vector2(yaw, pitch));
            SetFieldIfExists(t, ctrl, "lookAngles", new Vector2(yaw, pitch));
            SetFieldIfExists(t, ctrl, "angles", new Vector2(yaw, pitch));
            SetFieldIfExists(t, ctrl, "rotationEuler", e);
            SetFieldIfExists(t, ctrl, "m_RotationEuler", e);
            SetFieldIfExists(t, ctrl, "targetRotation", worldRot);
            SetFieldIfExists(t, ctrl, "m_TargetRotation", worldRot);
            SetFieldIfExists(t, ctrl, "desiredRotation", worldRot);
            SetFieldIfExists(t, ctrl, "m_DesiredRotation", worldRot);
        }

        static bool InvokeIfExists(System.Type t, object inst, string name, object[] args)
        {
            var m = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m == null) return false;
            var ps = m.GetParameters();
            if (ps.Length != (args?.Length ?? 0)) return false;
            try { m.Invoke(inst, args); } catch { }
            return true;
        }

        static bool SetPropIfExists(System.Type t, object inst, string name, object val)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p == null || !p.CanWrite) return false;
            try { p.SetValue(inst, val, null); } catch { }
            return true;
        }

        static bool SetFieldIfExists(System.Type t, object inst, string name, object val)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f == null) return false;
            try { f.SetValue(inst, val); } catch { }
            return true;
        }

        static float NormalizeAngle(float a)
        {
            a %= 360f; if (a > 180f) a -= 360f; if (a < -180f) a += 360f; return a;
        }
    }
}


