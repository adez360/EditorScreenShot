// Assets/360/Editor/EditorScreenShotHotkeys.cs
// Hotkey management for EditorScreenShot

using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace EditorScreenShot
{
    public static class EditorScreenShotHotkeys
    {
        // Handle window hotkeys
        public static void HandleWindowHotkeys(EditorScreenShotData data)
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown) return;
            if (EditorGUIUtility.editingTextField) return;

            if (e.keyCode == KeyCode.P && data.camera != null)
            {
                var (outW, outH) = EditorScreenShotOutput.GetCurrentWH(data);
                EditorScreenShotOutput.SaveCurrentFrame(data, outW, outH);
                e.Use();
            }
            else if (e.keyCode == KeyCode.O)
            {
                EditorScreenShotSceneSync.ToggleSceneSync(data, !data.sceneSyncOn);
                e.Use();
            }
        }

        // Handle scene view hotkeys
        public static void HandleSceneViewHotkeys(EditorScreenShotData data, SceneView sv)
        {
            if (sv == null) return;

            // Local single-key hotkeys from SceneView (fallback)
            var e = Event.current;
            if (e != null && e.type == EventType.KeyDown && !EditorGUIUtility.editingTextField)
            {
                if (e.keyCode == KeyCode.P && data.camera != null)
                {
                    var (outW, outH) = EditorScreenShotOutput.GetCurrentWH(data);
                    EditorScreenShotOutput.SaveCurrentFrame(data, outW, outH);
                    e.Use();
                    sv.Repaint();
                }
                else if (e.keyCode == KeyCode.O)
                {
                    EditorScreenShotSceneSync.ToggleSceneSync(data, !data.sceneSyncOn);
                    e.Use();
                    sv.Repaint();
                }
            }
        }

        // Note: Shortcut definitions are handled in the main EditorScreenShotWindow.cs file
        // to avoid duplicate shortcut registration warnings
    }
}
