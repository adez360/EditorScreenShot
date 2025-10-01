# VPM包模板设置指南

## 快速开始

### 1. 获取模板项目
```bash
# 方法1：使用GitHub模板
# 访问 https://github.com/vrchat-community/template-package
# 点击 "Use this Template" 创建新仓库

# 方法2：直接克隆
git clone https://github.com/vrchat-community/template-package.git my-vrchat-package
cd my-vrchat-package
```

### 2. 打开Unity项目
1. 启动Unity Hub
2. 点击"Open"或"Add"
3. 选择你的项目文件夹
4. 等待Unity加载项目

### 3. 验证VRChat SDK
确保项目中已安装：
- VRChat SDK - Worlds
- UdonSharp
- VRChat Creator Companion

## Package Maker工具使用

### 工具位置
菜单：`VRChat SDK / Utilities / Package Maker`

### 配置步骤
1. **Target Folder**: 拖入你的插件文件夹
2. **Package ID**: 输入包标识符（如：com.yourname.packagename）
3. **Related VRChat Package**: 选择相关包
4. **Convert Assets to Package**: 点击转换

### 包ID命名规范
- 使用反向域名格式
- 必须唯一
- 建议格式：`com.[你的域名].[包名]`
- 如果没有域名，可以使用：`com.[用户名].[包名]`

## 项目结构说明

### 转换前（Assets文件夹）
```
Assets/
└── YourPlugin/
    ├── Editor/
    │   └── EditorScripts.cs
    ├── Runtime/
    │   └── RuntimeScripts.cs
    └── Resources/
        └── Icons/
```

### 转换后（Packages文件夹）
```
Packages/
└── com.yourname.packagename/
    ├── package.json
    ├── Editor/
    │   ├── EditorScripts.cs
    │   └── com.yourname.packagename.Editor.asmdef
    ├── Runtime/
    │   ├── RuntimeScripts.cs
    │   └── com.yourname.packagename.asmdef
    └── Resources/
        └── Icons/
```

## 重要配置文件

### package.json
```json
{
  "name": "com.yourname.packagename",
  "displayName": "你的包显示名称",
  "version": "1.0.0",
  "description": "包的详细描述",
  "author": {
    "name": "你的名字",
    "email": "your.email@example.com"
  },
  "unity": "2019.4",
  "vpmDependencies": {
    "com.vrchat.worlds": ">=2022.2.0",
    "com.vrchat.udonsharp": ">=1.0.0"
  },
  "url": "https://github.com/yourusername/your-package-repo"
}
```

### Assembly Definition文件
**Runtime版本 (com.yourname.packagename.asmdef):**
```json
{
  "name": "com.yourname.packagename",
  "rootNamespace": "YourPackageName",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

**Editor版本 (com.yourname.packagename.Editor.asmdef):**
```json
{
  "name": "com.yourname.packagename.Editor",
  "rootNamespace": "YourPackageName.Editor",
  "references": [
    "com.yourname.packagename"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

## 路径转换示例

### 资源加载方式对比

**旧方式（Assets路径）：**
```csharp
// 不推荐 - 硬编码路径
var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/YourPlugin/Textures/icon.png");
```

**新方式1（Resources）：**
```csharp
// 推荐 - 使用Resources
var texture = Resources.Load<Texture2D>("icon");
```

**新方式2（GUID）：**
```csharp
// 推荐 - 使用GUID
string guid = "your-asset-guid-here";
string path = AssetDatabase.GUIDToAssetPath(guid);
var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
```

### 编辑器窗口示例
```csharp
using UnityEngine;
using UnityEditor;

namespace YourPackageName.Editor
{
    public class YourEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Your Package/Open Window")]
        public static void ShowWindow()
        {
            GetWindow<YourEditorWindow>("Your Package");
        }
        
        private void OnGUI()
        {
            // 使用Resources加载图标
            var icon = Resources.Load<Texture2D>("icon");
            if (icon != null)
            {
                GUILayout.Label(icon);
            }
        }
    }
}
```

## 测试和验证

### 本地测试清单
- [ ] 包在Unity中正确加载
- [ ] 没有编译错误
- [ ] 所有功能正常工作
- [ ] 编辑器工具正常显示
- [ ] 资源正确加载

### VRChat环境测试
- [ ] 在VRChat世界中测试功能
- [ ] 确保UdonSharp兼容性
- [ ] 验证网络同步功能（如果适用）

## 发布准备

### GitHub仓库设置
1. 创建仓库变量：`PACKAGE_NAME = com.yourname.packagename`
2. 启用GitHub Pages
3. 设置Pages源为"GitHub Actions"

### 版本管理
- 更新package.json中的版本号
- 使用语义化版本控制（如：1.0.0, 1.0.1, 1.1.0）
- 为每个版本创建Git标签

## ⌨️ 快捷键说明

### 编辑器快捷键
- **O键**：切换场景同步开关
- **P键**：捕获当前帧截图
- **Ctrl+Alt+E**：打开编辑器截图工具面板

### 运行时快捷键（Play Mode）
- **O键**：切换场景同步
- **P键**：捕获截图
- **R键**：切换Freecam锁定模式

### 快捷键配置
1. **全局快捷键**：在Unity编辑器中，快捷键在Scene View和Game View中都有效
2. **运行时快捷键**：需要在场景中添加`ESSPlayHotkeys`组件才能使用
3. **自定义快捷键**：可以在`Edit > Shortcuts`菜单中修改快捷键设置
4. **组件配置**：`ESSPlayHotkeys`组件允许自定义按键设置和启用条件

### 快捷键使用场景
- **场景同步**：用于同步编辑器视图和游戏视图的相机位置
- **截图功能**：快速捕获当前帧，支持多种分辨率和格式
- **Freecam控制**：在运行时控制自由相机的锁定状态

完成这些设置后，你的Unity插件就准备好转换为VPM包了！
