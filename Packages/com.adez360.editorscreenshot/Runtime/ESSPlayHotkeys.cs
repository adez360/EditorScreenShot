// Assets/360/EditorScreenShot/Script/ESSPlayHotkeys.cs
// Runtime bridge for single-key hotkeys in Play Mode (Game View).
// P: capture (Editor only, via reflection call to EditorScreenShotWindow.CaptureGlobal)
// O: toggle Scene Sync (Editor only, via reflection call to EditorScreenShotWindow.ToggleSceneSyncGlobal)
// R: best-effort toggle Freecam lock
using UnityEngine;
using System.Reflection;
using System;
using EditorScreenShot.Runtime;

[DisallowMultipleComponent]
[AddComponentMenu("360/EditorScreenShot/ESS Play Hotkeys")]
[DefaultExecutionOrder(-10000)]
public class ESSPlayHotkeys : MonoBehaviour
{
    [Header("Keys (single-key)")]
    public KeyCode captureKey     = KeyCode.P;
    public KeyCode sceneSyncKey   = KeyCode.O;
    public KeyCode freecamLockKey = KeyCode.R;

    [Header("Options")]
    public bool onlyWhenPlaying = true;

    void Update()
    {
        if (onlyWhenPlaying && !Application.isPlaying) return;

        if (Input.GetKeyDown(captureKey))
        {
            EditorShotService.Current.Capture();
        }

        if (Input.GetKeyDown(sceneSyncKey))
        {
            EditorShotService.Current.ToggleSceneSync();
        }

        if (Input.GetKeyDown(freecamLockKey))
        {
            if (!FreecamHandlesOwnToggle())
                ToggleFreecamLockBestEffort();
            // Avoid duplicate R key handling if Freecam has built-in toggle
        }
    }

#if UNITY_EDITOR
    // Find a type by name across loaded assemblies and invoke a static method with no args.
    static bool CallEditorStaticNoArg(string typeName, string methodName)
    {
        var asms = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in asms)
        {
            Type t = asm.GetType(typeName);
            if (t == null)
            {
                // fallback by short name
                foreach (var tt in asm.GetTypes())
                {
                    if (tt.Name == typeName) { t = tt; break; }
                }
            }
            if (t == null) continue;

            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (m != null && m.GetParameters().Length == 0)
            {
                try { m.Invoke(null, null); return true; }
                catch (Exception e) { Debug.LogException(e); return false; }
            }
        }
        // Quiet fail to avoid log spam; uncomment if you want warnings: 
        // Debug.LogWarning($"[ESSPlayHotkeys] {typeName}.{methodName} not found.");
        return false;
    }
#endif

    // ---------- Freecam best-effort toggle ----------
    bool FreecamHandlesOwnToggle()
    {
        var cam = Camera.main;
        Freecam fc = cam ? cam.GetComponent<Freecam>() : null;
        if (!fc) fc = FindObjectOfType<Freecam>();
        if (!fc) return false;

        var t = fc.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        // Freecam.cs already has Input.GetKeyDown(R) calling ToggleLockLook()
        // If method exists or lockLookAt field exists, assume built-in hotkey handling
        if (t.GetMethod("ToggleLockLook", flags) != null) return true;
        if (t.GetField("lockLookAt", flags) != null) return true;
        return false;
    }
    void ToggleFreecamLockBestEffort()
    {
        Freecam fc = null;
        var cam = Camera.main;
        if (cam) fc = cam.GetComponent<Freecam>();
        if (!fc) fc = FindObjectOfType<Freecam>();
        if (!fc) return;

        var t = fc.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Prefer dedicated APIs if present
        var mToggle = t.GetMethod("ToggleLock", flags);
        if (mToggle != null && mToggle.GetParameters().Length == 0)
        {
            mToggle.Invoke(fc, null);
            return;
        }

        var mSet = t.GetMethod("SetLock", flags);
        if (mSet != null && mSet.GetParameters().Length == 1 &&
            mSet.GetParameters()[0].ParameterType == typeof(bool))
        {
            bool cur = GetBoolByAnyName(fc, new[] { "LockActive", "lockLookAt", "lockOn" }, flags) ?? false;
            mSet.Invoke(fc, new object[] { !cur });
            return;
        }

        // Fallback: flip common bool field/property
        if (FlipBoolByAnyName(fc, new[] { "lockLookAt", "LockActive", "lockOn" }, flags)) return;
    }

    bool? GetBoolByAnyName(object obj, string[] names, BindingFlags flags)
    {
        var t = obj.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, flags);
            if (f != null && f.FieldType == typeof(bool)) return (bool)f.GetValue(obj);

            var p = t.GetProperty(n, flags);
            if (p != null && p.PropertyType == typeof(bool) && p.CanRead) return (bool)p.GetValue(obj, null);
        }
        return null;
    }

    bool FlipBoolByAnyName(object obj, string[] names, BindingFlags flags)
    {
        var t = obj.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, flags);
            if (f != null && f.FieldType == typeof(bool))
            {
                bool v = (bool)f.GetValue(obj);
                f.SetValue(obj, !v);
                return true;
            }
            var p = t.GetProperty(n, flags);
            if (p != null && p.PropertyType == typeof(bool) && p.CanWrite)
            {
                bool v = (bool)p.GetValue(obj, null);
                p.SetValue(obj, !v, null);
                return true;
            }
        }
        return false;
    }
}
