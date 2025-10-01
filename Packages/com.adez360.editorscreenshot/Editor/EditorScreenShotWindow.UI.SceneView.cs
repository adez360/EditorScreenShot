using UnityEngine;
using UnityEditor;
using FreecamPreview;

public partial class EditorScreenShotWindow
{
    void DrawSceneViewOverlaySettings()
    {
        _showSafeFrame = EditorGUILayout.ToggleLeft(Loc.T("ShowSafeFrame"), _showSafeFrame);
        using (new EditorGUI.DisabledScope(!_showSafeFrame))
        {
            _sfOnlyWhenTarget = EditorGUILayout.ToggleLeft(Loc.T("SFOnlyTarget"), _sfOnlyWhenTarget);

            GUILayout.Space(4);
            _sfThirds = EditorGUILayout.ToggleLeft(Loc.T("SFThirds"), _sfThirds);
            _sfDiagonals = EditorGUILayout.ToggleLeft(Loc.T("SFDiagonals"), _sfDiagonals);
            _sfCenterCross = EditorGUILayout.ToggleLeft(Loc.T("SFCenter"), _sfCenterCross);

            GUILayout.Space(4);
            _sfTitleSafe = EditorGUILayout.Slider(Loc.T("TitleSafe"), _sfTitleSafe, 0.5f, 0.98f);

            GUILayout.Space(4);
            _sfLineWidth = EditorGUILayout.Slider(Loc.T("LineWidth"), _sfLineWidth, 1f, 6f);
            _sfLineColor = EditorGUILayout.ColorField(Loc.T("LineColor"), _sfLineColor);
            _sfMaskAlpha = EditorGUILayout.Slider(Loc.T("MaskOpacity"), _sfMaskAlpha, 0f, 0.85f);
        }
    }
}


