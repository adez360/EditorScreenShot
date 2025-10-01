# VPM包自动化脚本

## GitHub Actions工作流

### 1. 发布构建工作流 (release.yml)

```yaml
name: Build Release

on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Version to build (leave empty to use package.json version)'
        required: false
        type: string

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      
    - name: Setup Unity
      uses: game-ci/unity-setup@v2
      with:
        unity-version: 2019.4.40f1
        
    - name: Build Unity Package
      uses: game-ci/unity-builder@v3
      with:
        targetPlatform: StandaloneWindows64
        projectPath: .
        
    - name: Create Package Archive
      run: |
        cd Packages
        zip -r ../package.zip ${{ vars.PACKAGE_NAME }}
        
    - name: Create Unity Package
      run: |
        # 这里需要Unity命令行工具来创建.unitypackage
        # 具体实现取决于你的需求
        
    - name: Create Release
      uses: actions/create-release@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: v${{ github.event.inputs.version || '1.0.0' }}
        release_name: Release v${{ github.event.inputs.version || '1.0.0' }}
        body: |
          ## 更新内容
          - 新功能和改进
          - 错误修复
          
          ## 安装说明
          1. 在VRChat Creator Companion中添加此仓库
          2. 搜索并安装包
        draft: false
        prerelease: false
        
    - name: Upload Package Archive
      uses: actions/upload-release-asset@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ steps.create_release.outputs.upload_url }}
        asset_path: ./package.zip
        asset_name: package.zip
        asset_content_type: application/zip
```

### 2. 仓库列表构建工作流 (build-listing.yml)

```yaml
name: Build Repo Listing

on:
  release:
    types: [published, edited, deleted]
  workflow_dispatch:

jobs:
  build-listing:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
        
    - name: Build Repo Listing
      run: |
        # 这里需要VPM工具来构建仓库列表
        # 具体实现取决于VPM的要求
        
    - name: Deploy to GitHub Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./Website
```

## 本地构建脚本

### Windows批处理脚本 (build-package.bat)

```batch
@echo off
echo 开始构建VPM包...

REM 设置变量
set PACKAGE_NAME=com.yourname.packagename
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2019.4.40f1\Editor\Unity.exe"
set PROJECT_PATH=%~dp0

echo 包名称: %PACKAGE_NAME%
echo Unity路径: %UNITY_PATH%
echo 项目路径: %PROJECT_PATH%

REM 检查Unity是否存在
if not exist %UNITY_PATH% (
    echo 错误: 找不到Unity编辑器
    pause
    exit /b 1
)

REM 构建包
echo 正在构建包...
%UNITY_PATH% -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod PackageBuilder.BuildPackage

if %ERRORLEVEL% neq 0 (
    echo 错误: 构建失败
    pause
    exit /b 1
)

echo 构建完成！
pause
```

### PowerShell脚本 (build-package.ps1)

```powershell
# VPM包构建脚本
param(
    [string]$PackageName = "com.yourname.packagename",
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2019.4.40f1\Editor\Unity.exe",
    [string]$ProjectPath = $PSScriptRoot
)

Write-Host "开始构建VPM包..." -ForegroundColor Green
Write-Host "包名称: $PackageName" -ForegroundColor Yellow
Write-Host "Unity路径: $UnityPath" -ForegroundColor Yellow
Write-Host "项目路径: $ProjectPath" -ForegroundColor Yellow

# 检查Unity是否存在
if (-not (Test-Path $UnityPath)) {
    Write-Host "错误: 找不到Unity编辑器" -ForegroundColor Red
    exit 1
}

# 检查项目路径
if (-not (Test-Path "$ProjectPath\Packages\$PackageName")) {
    Write-Host "错误: 找不到包文件夹" -ForegroundColor Red
    exit 1
}

# 构建包
Write-Host "正在构建包..." -ForegroundColor Green
& $UnityPath -batchmode -quit -projectPath $ProjectPath -executeMethod PackageBuilder.BuildPackage

if ($LASTEXITCODE -ne 0) {
    Write-Host "错误: 构建失败" -ForegroundColor Red
    exit 1
}

Write-Host "构建完成！" -ForegroundColor Green
```

## Unity编辑器脚本

### 包构建器 (PackageBuilder.cs)

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;

namespace YourPackageName.Editor
{
    public static class PackageBuilder
    {
        [MenuItem("Tools/Build VPM Package")]
        public static void BuildPackage()
        {
            string packageName = "com.yourname.packagename";
            string packagePath = $"Packages/{packageName}";
            string outputPath = "Build";
            
            // 创建输出目录
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            // 读取package.json
            string packageJsonPath = Path.Combine(packagePath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                Debug.LogError($"找不到package.json文件: {packageJsonPath}");
                return;
            }
            
            // 解析版本号
            string packageJson = File.ReadAllText(packageJsonPath);
            var packageData = JsonUtility.FromJson<PackageData>(packageJson);
            string version = packageData.version;
            
            // 创建ZIP文件
            string zipPath = Path.Combine(outputPath, $"{packageName}-{version}.zip");
            CreatePackageZip(packagePath, zipPath);
            
            Debug.Log($"包构建完成: {zipPath}");
        }
        
        private static void CreatePackageZip(string sourcePath, string zipPath)
        {
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(sourcePath, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }
        }
        
        [System.Serializable]
        private class PackageData
        {
            public string name;
            public string version;
            public string displayName;
            public string description;
        }
    }
}
```

## 版本管理脚本

### 版本更新器 (VersionUpdater.cs)

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace YourPackageName.Editor
{
    public static class VersionUpdater
    {
        [MenuItem("Tools/Update Package Version")]
        public static void UpdateVersion()
        {
            string packageName = "com.yourname.packagename";
            string packageJsonPath = $"Packages/{packageName}/package.json";
            
            if (!File.Exists(packageJsonPath))
            {
                Debug.LogError($"找不到package.json文件: {packageJsonPath}");
                return;
            }
            
            // 读取当前版本
            string packageJson = File.ReadAllText(packageJsonPath);
            var versionMatch = Regex.Match(packageJson, @"""version"":\s*""([^""]+)""");
            
            if (!versionMatch.Success)
            {
                Debug.LogError("无法解析版本号");
                return;
            }
            
            string currentVersion = versionMatch.Groups[1].Value;
            Debug.Log($"当前版本: {currentVersion}");
            
            // 这里可以添加版本更新逻辑
            // 例如：自动递增版本号、从用户输入获取新版本等
        }
    }
}
```

## 使用说明

### 1. 设置GitHub Actions
1. 将工作流文件放入 `.github/workflows/` 目录
2. 在仓库设置中创建变量 `PACKAGE_NAME`
3. 启用GitHub Pages

### 2. 使用本地脚本
1. 将脚本文件放入项目根目录
2. 根据需要修改包名称和路径
3. 运行脚本进行本地构建

### 3. 使用Unity编辑器脚本
1. 将脚本放入Editor文件夹
2. 在Unity菜单中使用相应选项
3. 根据需要自定义构建逻辑

这些脚本将帮助你自动化VPM包的构建和发布过程！

