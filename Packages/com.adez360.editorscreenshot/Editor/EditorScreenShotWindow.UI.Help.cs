using UnityEngine;
using UnityEditor;
using FreecamPreview;
using EditorScreenShot;

namespace EditorScreenShot
{
    public partial class EditorScreenShotWindow
    {
    void DrawHelpAndReset()
    {
        var help = new GUIStyle(EditorStyles.helpBox) { fontSize = 12, richText = true, alignment = TextAnchor.MiddleLeft };
        GUILayout.Label(Loc.T("KeysHelp"), help);

        GUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            // Use built-in or custom icon
            Texture2D resetIcon = EditorGUIUtility.IconContent("Refresh").image as Texture2D;
            if (GUILayout.Button(new GUIContent(Loc.T("Reset"), resetIcon), GUILayout.Width(120)))
                EditorScreenShotSettings.ResetToDefault(_data);
        }
    }
    }
}

