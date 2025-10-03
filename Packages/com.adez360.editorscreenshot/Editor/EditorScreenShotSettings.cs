// Assets/360/Editor/EditorScreenShotSettings.cs
// Settings management for EditorScreenShot

using UnityEngine;
using UnityEditor;
using FreecamPreview;
using EditorScreenShot.Runtime;

namespace EditorScreenShot
{
    public static class EditorScreenShotSettings
    {
        // Load settings from EditorPrefs
        public static void LoadSettings(EditorScreenShotData data)
        {
            data.outDir = EditorPrefs.GetString("ESS.OutDir", OutputPathResolver.GetDefaultScreenshotDir());
            data.png = EditorPrefs.GetBool("ESS.PNG", true);
            data.jpgQuality = EditorPrefs.GetInt("ESS.JPGQ", 95);
            data.pngKeepAlpha = EditorPrefs.GetBool("ESS.PNGAlpha", true);
            data.lang = (Lang)EditorPrefs.GetInt("ESS.Lang", (int)Loc.GetSystemDefaultLanguage());
            data.lastSavedPath = EditorPrefs.GetString("ESS.LastPath", null);
            data.fileNameTemplate = EditorPrefs.GetString("ESS.FNameTpl", "{scene}_{yyyyMMdd_HHmmss}");
            data.rtMSAA = EditorPrefs.GetInt("ESS.RTMSAA", 1);

            data.showSafeFrame = EditorPrefs.GetBool("ESS.SF.Show", false);
            data.sfThirds = EditorPrefs.GetBool("ESS.SF.Thirds", true);
            data.sfDiagonals = EditorPrefs.GetBool("ESS.SF.Diag", false);
            data.sfCenterCross = EditorPrefs.GetBool("ESS.SF.Center", true);
            data.sfTitleSafe = EditorPrefs.GetFloat("ESS.SF.TS", 0.90f);
            data.sfLineWidth = EditorPrefs.GetFloat("ESS.SF.LW", 2f);
            data.sfLineColor = LoadColor("ESS.SF.Color", new Color(1,1,1,0.9f));
            data.sfMaskAlpha = EditorPrefs.GetFloat("ESS.SF.Mask", 0.35f);
        }

        // Save settings to EditorPrefs
        public static void SaveSettings(EditorScreenShotData data)
        {
            EditorPrefs.SetString("ESS.OutDir", data.outDir);
            EditorPrefs.SetBool("ESS.PNG", data.png);
            EditorPrefs.SetInt("ESS.JPGQ", data.jpgQuality);
            EditorPrefs.SetBool("ESS.PNGAlpha", data.pngKeepAlpha);
            EditorPrefs.SetInt("ESS.Lang", (int)data.lang);
            EditorPrefs.SetString("ESS.FNameTpl", data.fileNameTemplate);
            EditorPrefs.SetInt("ESS.RTMSAA", data.rtMSAA);
            if (!string.IsNullOrEmpty(data.lastSavedPath)) 
                EditorPrefs.SetString("ESS.LastPath", data.lastSavedPath);

            EditorPrefs.SetBool("ESS.SF.Show", data.showSafeFrame);
            EditorPrefs.SetBool("ESS.SF.Thirds", data.sfThirds);
            EditorPrefs.SetBool("ESS.SF.Diag", data.sfDiagonals);
            EditorPrefs.SetBool("ESS.SF.Center", data.sfCenterCross);
            EditorPrefs.SetFloat("ESS.SF.TS", data.sfTitleSafe);
            EditorPrefs.SetFloat("ESS.SF.LW", data.sfLineWidth);
            SaveColor("ESS.SF.Color", data.sfLineColor);
            EditorPrefs.SetFloat("ESS.SF.Mask", data.sfMaskAlpha);
        }

        // Reset settings to default
        public static void ResetToDefault(EditorScreenShotData data)
        {
            data.preset = AspectPreset.FHD_16_9;
            data.portrait = false;
            data.customW = 1920;
            data.customH = 1080;
            data.png = true;
            data.jpgQuality = 95;
            data.pngKeepAlpha = true;
            data.lastSavedPath = null;
            data.fileNameTemplate = "{scene}_{yyyyMMdd_HHmmss}";
            data.rtMSAA = 1;

            data.showSafeFrame = true;
            data.sfThirds = true;
            data.sfDiagonals = false;
            data.sfCenterCross = true;
            data.sfTitleSafe = 0.90f;
            data.sfLineWidth = 2f;
            data.sfLineColor = new Color(1,1,1,0.9f);
            data.sfMaskAlpha = 0.8f;
        }

        // Color helper methods
        static Color LoadColor(string key, Color def)
        {
            Color c = def;
            c.r = EditorPrefs.GetFloat(key + ".r", def.r);
            c.g = EditorPrefs.GetFloat(key + ".g", def.g);
            c.b = EditorPrefs.GetFloat(key + ".b", def.b);
            c.a = EditorPrefs.GetFloat(key + ".a", def.a);
            return c;
        }

        static void SaveColor(string key, Color c)
        {
            EditorPrefs.SetFloat(key + ".r", c.r);
            EditorPrefs.SetFloat(key + ".g", c.g);
            EditorPrefs.SetFloat(key + ".b", c.b);
            EditorPrefs.SetFloat(key + ".a", c.a);
        }

        // Hex color helper
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var col)) 
                return col;
            return new Color(0.15f, 0.6f, 0.35f);
        }
    }
}
