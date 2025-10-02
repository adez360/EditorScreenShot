using UnityEngine;
using UnityEditor;
using FreecamPreview;

public partial class EditorScreenShotWindow
{
    void DrawSceneViewOverlaySettings()
    {
        // 顯示參考線
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(Loc.T("ShowSafeFrame"), GUILayout.Width(120));
            _showSafeFrame = GUILayout.Toggle(_showSafeFrame, "", GUILayout.Width(20));
            GUILayout.FlexibleSpace();
        }

        using (new EditorGUI.DisabledScope(!_showSafeFrame))
        {

            // 三分線
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SFThirds"), GUILayout.Width(120));
                _sfThirds = GUILayout.Toggle(_sfThirds, "", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
            }

            // 對角線
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SFDiagonals"), GUILayout.Width(120));
                _sfDiagonals = GUILayout.Toggle(_sfDiagonals, "", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
            }

            // 中心十字
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("SFCenter"), GUILayout.Width(120));
                _sfCenterCross = GUILayout.Toggle(_sfCenterCross, "", GUILayout.Width(20));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(4);

            // Title Safe %
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("TitleSafe"), GUILayout.Width(120));
                _sfTitleSafe = EditorGUILayout.Slider(_sfTitleSafe, 0.5f, 0.98f);
            }

            // 線寬
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("LineWidth"), GUILayout.Width(120));
                _sfLineWidth = EditorGUILayout.Slider(_sfLineWidth, 1f, 6f);
            }

            // 線色
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("LineColor"), GUILayout.Width(120));
                _sfLineColor = EditorGUILayout.ColorField(_sfLineColor);
            }

            // 遮罩透明度
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Loc.T("MaskOpacity"), GUILayout.Width(120));
                _sfMaskAlpha = EditorGUILayout.Slider(_sfMaskAlpha, 0f, 0.85f);
            }
        }

        // 說明框
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


