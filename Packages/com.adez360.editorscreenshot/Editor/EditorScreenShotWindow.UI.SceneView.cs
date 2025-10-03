using UnityEngine;
using UnityEditor;
using FreecamPreview;
using EditorScreenShot;

namespace EditorScreenShot
{
    public partial class EditorScreenShotWindow
    {
    void DrawSceneViewOverlaySettings()
    {
        // Show reference lines
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("ShowSafeFrame"), GUILayout.Width(120));
            _data.showSafeFrame = GUILayout.Toggle(_data.showSafeFrame, "", GUILayout.Width(20));
            GUILayout.FlexibleSpace();
        }

        using (new EditorGUI.DisabledScope(!_data.showSafeFrame))
        {

            // Rule of thirds
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SFThirds"), GUILayout.Width(120));
                _data.sfThirds = GUILayout.Toggle(_data.sfThirds, "", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
            }

            // Diagonals
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SFDiagonals"), GUILayout.Width(120));
                _data.sfDiagonals = GUILayout.Toggle(_data.sfDiagonals, "", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
            }

            // Center cross
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SFCenter"), GUILayout.Width(120));
                _data.sfCenterCross = GUILayout.Toggle(_data.sfCenterCross, "", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(4);

            // Title Safe %
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("TitleSafe"), GUILayout.Width(120));
                _data.sfTitleSafe = EditorGUILayout.Slider(_data.sfTitleSafe, 0.5f, 0.98f);
            }

            // Line width
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("LineWidth"), GUILayout.Width(120));
                _data.sfLineWidth = EditorGUILayout.Slider(_data.sfLineWidth, 1f, 6f);
            }

            // Line color
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("LineColor"), GUILayout.Width(120));
                _data.sfLineColor = EditorGUILayout.ColorField(_data.sfLineColor);
            }

            // Mask opacity
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("MaskOpacity"), GUILayout.Width(120));
                _data.sfMaskAlpha = EditorGUILayout.Slider(_data.sfMaskAlpha, 0f, 0.85f);
            }
        }

        // Help box
        GUILayout.Space(8);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            var helpStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 11,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            EditorGUILayout.LabelField(Loc.T("SafeFrameTip"), helpStyle);
        }
    }
    }
}

