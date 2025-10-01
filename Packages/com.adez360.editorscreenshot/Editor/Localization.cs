// Localization.cs
// Simple 3-language dictionary (EN/ZH/JA). Avoids tuple/target-typed new for Unity C# compatibility.

using System.Collections.Generic;
using System.Globalization;

namespace FreecamPreview
{
    public enum Lang { English, Chinese, ChineseSimplified, Japanese }

    public static class Loc
    {
        static Lang _lang = Lang.Chinese;
        public static Lang Language { get => _lang; set => _lang = value; }

        // key -> [en, zh, zh-cn, ja]
        static readonly Dictionary<string, string[]> dict = new Dictionary<string, string[]>
        {
            // Window / Toolbar
            { "PanelTitle", new[] { "EditorScreenShot", "EditorScreenShot", "EditorScreenShot", "EditorScreenShot" } },
            { "Lang",       new[] { "Language", "語言", "语言", "言語" } },

            // Big actions / Quick row
            { "CaptureImage",   new[] { "Capture Image", "拍攝截圖", "拍摄截图", "キャプチャ" } },
            { "OpenFolder",     new[] { "Open Folder", "開啟資料夾", "打开文件夹", "フォルダを開く" } },
            { "ReloadCamera",   new[] { "Reload Camera", "重新載入相機", "重新加载相机", "カメラを再読み込み" } },
            { "AlignFromScene", new[] { "Sync Scene View", "同步scene視角", "同步场景视图", "シーンビューを同期" } },

            // Groups
            { "Settings",        new[] { "Settings", "設定", "设置", "設定" } },
            { "CameraSettings",  new[] { "Camera Settings", "相機設定", "相机设置", "カメラ設定" } },
            { "OutputSettings",  new[] { "Output Settings", "輸出設定", "输出设置", "出力設定" } },
            { "LensAdvanced",    new[] { "Lens (Advanced)", "鏡頭（進階）", "镜头（高级）", "レンズ（詳細）" } },
            { "SceneViewSection",new[] { "Scene View", "Scene View", "场景视图", "シーンビュー" } },

            // Scene Sync
            { "SceneSyncOn",     new[] { "Scene Sync: ON", "場景同步：開", "场景同步：开", "シーン同期：ON" } },
            { "SceneSyncOff",    new[] { "Scene Sync: OFF", "場景同步：關", "场景同步：关", "シーン同期：OFF" } },

            // Common fields
            { "Camera",        new[] { "Camera", "相機", "相机", "カメラ" } },
            { "Browse",        new[] { "Browse", "瀏覽", "浏览", "参照" } },
            { "Resolution",    new[] { "Resolution", "解析度", "分辨率", "解像度" } },
            { "Portrait",      new[] { "Portrait", "直式", "竖屏", "縦向き" } },
            { "Format",        new[] { "Format", "格式", "格式", "フォーマット" } },
            { "SaveLocation",  new[] { "Save Location", "儲存位置", "保存位置", "保存先" } },
            { "Transparent",   new[] { "Transparent BG", "透明背景", "透明背景", "透明背景" } },
            { "OpenAfterSave", new[] { "Open after save", "存檔後開啟", "保存后打开", "保存後に開く" } },

            // Short labels
            { "WidthShort",  new[] { "W", "W", "W", "W" } },
            { "HeightShort", new[] { "H", "H", "H", "H" } },

            // Optics / Distortion
            { "FOV",      new[] { "FOV", "視角", "视角", "視野角" } },
            { "Focal",    new[] { "Focal Length (mm)", "焦距 (mm)", "焦距 (mm)", "焦点距離 (mm)" } },
            { "Fisheye",  new[] { "Fisheye", "魚眼", "鱼眼", "フィッシュアイ" } },
            { "Enable",   new[] { "Enable", "啟用", "启用", "有効化" } },
            { "Strength", new[] { "Strength", "強度", "强度", "強さ" } },

            // Scene View overlay (no Action Safe)
            { "ShowSafeFrame", new[] { "Show Safe Frame", "顯示參考線", "显示参考线", "セーフフレーム表示" } },
            { "SFThirds",      new[] { "Thirds Grid", "三分線", "三分线", "三分割グリッド" } },
            { "SFDiagonals",   new[] { "Diagonals", "對角線", "对角线", "対角線" } },
            { "SFCenter",      new[] { "Center Cross", "中心十字", "中心十字", "センタークロス" } },
            { "TitleSafe",     new[] { "Title Safe %", "Title Safe %", "标题安全区 %", "タイトルセーフ %" } },
            { "LineWidth",     new[] { "Line Width", "線寬", "线宽", "線幅" } },
            { "LineColor",     new[] { "Line Color", "線色", "线色", "線色" } },
            { "MaskOpacity",   new[] { "Mask Opacity", "遮罩透明度", "遮罩透明度", "マスク不透明度" } },
            { "SFOnlyTarget",  new[] { "Only when target camera/lock", "僅在有目標/目標相機時顯示", "仅在锁定目标/相机时显示", "対象がある時のみ表示" } },

            // Aspect names
            { "UHD_4K_16_9", new[] { "UHD 4K 16:9", "UHD 4K 16:9", "UHD 4K 16:9", "UHD 4K 16:9" } },
            { "FHD_16_9",    new[] { "FHD 16:9", "FHD 16:9", "FHD 16:9", "FHD 16:9" } },
            { "QHD_16_9",    new[] { "QHD 16:9", "QHD 16:9", "QHD 16:9", "QHD 16:9" } },
            { "HD_16_9",     new[] { "HD 16:9", "HD 16:9", "HD 16:9", "HD 16:9" } },
            { "Square_1_1",  new[] { "Square 1:1", "正方形 1:1", "正方形 1:1", "正方形 1:1" } },
            { "Custom",      new[] { "Custom", "自訂", "自定义", "カスタム" } },

            // Status / Help
            { "Status",           new[] { "Status", "狀態", "状态", "ステータス" } },
            { "Speed",            new[] { "Speed", "速度", "速度", "速度" } },
            { "Lock",             new[] { "Lock", "鎖定", "锁定", "ロック" } },
            { "LockTarget",       new[] { "Lock Target", "鎖定目標", "锁定目标", "ロック対象" } },
            { "NoLockTarget",     new[] { "No lock target set.", "尚未設定鎖定目標。", "尚未设置锁定目标。", "ロック対象が未設定です。" } },
            { "LockBtnOn",        new[] { "Lock: ON", "鎖定：開", "锁定：开", "ロック：ON" } },
            { "LockBtnOff",       new[] { "Lock: OFF", "鎖定：關", "锁定：关", "ロック：OFF" } },
            { "LockBtnNoTarget",  new[] { "Lock: OFF (no target)", "鎖定：關（無目標）", "锁定：关（无目标）", "ロック：OFF（対象なし）" } },
            { "ResetSpeed",       new[] { "Reset Speed", "重設速度", "重置速度", "速度をリセット" } },
            { "Reset",            new[] { "Reset", "重設", "重置", "リセット" } },
            { "KeysHelp",         new[] {
                "WASD: move | Q/E: up/down | RMB: look (hold) | Shift/Ctrl: sprint/slow | Z/C: roll | X: level | R: toggle aim | O: scene sync | P: screenshot | Ctrl+Shift+E: open panel",
                "WASD: 移動｜Q/E: 上/下｜右鍵: 環視(按住)｜Shift/Ctrl: 快/慢｜Z/C: 滾轉｜X: 水平回中｜R: 切換對準｜O: 場景同步｜P: 截圖｜Ctrl+Shift+E: 開啟面板",
                "WASD: 移动｜Q/E: 上/下｜右键: 环视(按住)｜Shift/Ctrl: 快/慢｜Z/C: 滚转｜X: 水平回中｜R: 切换对准｜O: 场景同步｜P: 截图｜Ctrl+Shift+E: 打开面板",
                "WASD: 移動 | Q/E: 上下 | 右クリック: 視点(長押し) | Shift/Ctrl: 速/遅 | Z/C: ロール | X: 水平 | R: 切替ターゲット | O: シーン同期 | P: スクショ | Ctrl+Shift+E: パネル開く"
            } },

            // Quality
            { "MSAA",             new[] { "RT MSAA", "RT MSAA", "RT MSAA", "RT MSAA" } },
            { "FileNameTemplate", new[] { "File Name", "檔名模板", "文件名模板", "ファイル名テンプレート" } },
        };

        /// <summary>
        /// Auto-select appropriate language based on system language
        /// </summary>
        public static Lang GetSystemDefaultLanguage()
        {
            try
            {
                // Get current system language
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                string languageCode = currentCulture.TwoLetterISOLanguageName.ToLower();
                string cultureName = currentCulture.Name.ToLower();

                // Map based on language and region codes
                if (languageCode == "zh")
                {
                    // Simplified Chinese regions
                    if (cultureName.Contains("cn") || cultureName.Contains("hans"))
                        return Lang.ChineseSimplified;
                    // Traditional Chinese regions (Taiwan, Hong Kong, Macau, etc.)
                    else if (cultureName.Contains("tw") || cultureName.Contains("hk") || 
                             cultureName.Contains("mo") || cultureName.Contains("hant"))
                        return Lang.Chinese;
                    // Default to Simplified Chinese
                    else
                        return Lang.ChineseSimplified;
                }
                else if (languageCode == "ja")
                {
                    return Lang.Japanese;
                }
                else
                {
                    // Default to English
                    return Lang.English;
                }
            }
            catch
            {
                // If detection fails, return default language (Traditional Chinese)
                return Lang.Chinese;
            }
        }

        /// <summary>
        /// Get friendly display name for system language
        /// </summary>
        public static string GetSystemLanguageDisplayName()
        {
            try
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                return $"{currentCulture.DisplayName} ({currentCulture.Name})";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string T(string key)
        {
            if (!dict.TryGetValue(key, out var arr)) return key;
            switch (_lang)
            {
                case Lang.English:         return arr[0];
                case Lang.Chinese:         return arr[1];
                case Lang.ChineseSimplified: return arr[2];
                case Lang.Japanese:        return arr[3];
                default:                   return arr[1];
            }
        }
    }
}
