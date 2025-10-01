# 部署指南 - 從 Assets 到 Unity Package Manager

## 概述

本指南將幫助您將 EditorScreenShot 專案從 Assets 格式轉換為 Unity Package Manager (UPM) 格式。

## 已完成的轉換步驟

### 1. 創建 package.json
- ✅ 定義了包的基本信息（名稱、版本、描述等）
- ✅ 設置了 Unity 版本要求 (2021.3+)
- ✅ 配置了依賴項 (URP 12.0+)
- ✅ 添加了關鍵字和作者信息

### 2. 更新 Assembly Definitions
- ✅ 將 `EditorScreenShot.Runtime` 重命名為 `com.360.editorscreenshot.runtime`
- ✅ 將 `EditorScreenShot.Editor` 重命名為 `com.360.editorscreenshot.editor`
- ✅ 更新了引用關係以使用新的命名空間

### 3. 完善文檔
- ✅ 更新了 README.md 以符合 UPM 標準
- ✅ 更新了 CHANGELOG.md 格式
- ✅ 創建了 MIT LICENSE 文件
- ✅ 添加了 .gitignore 文件

## 部署選項

### 選項 1: 本地包部署

1. **移動包文件夾**：
   ```
   將整個 EditorScreenShot 文件夾移動到：
   YourProject/Packages/com.360.editorscreenshot/
   ```

2. **在 Unity 中驗證**：
   - 打開 Unity Package Manager
   - 選擇 "In Project" 標籤
   - 確認包已正確顯示

### 選項 2: Git 倉庫部署

1. **創建 Git 倉庫**：
   ```bash
   git init
   git add .
   git commit -m "Initial UPM package release"
   ```

2. **推送到遠程倉庫**：
   ```bash
   git remote add origin https://github.com/yourusername/EditorScreenShot.git
   git push -u origin main
   ```

3. **通過 Git URL 安裝**：
   - 在 Unity Package Manager 中選擇 "Add package from git URL"
   - 輸入：`https://github.com/yourusername/EditorScreenShot.git`

### 選項 3: 私有包註冊表

1. **設置私有註冊表**：
   - 使用 Unity Package Manager 的私有註冊表功能
   - 上傳包到您的私有註冊表

2. **配置項目**：
   - 在 `Packages/manifest.json` 中添加註冊表 URL
   - 添加包依賴項

## 驗證部署

### 1. 功能測試
- [ ] 打開 `Tools > 360 > EditorScreenShot`
- [ ] 測試截圖功能
- [ ] 驗證熱鍵功能
- [ ] 檢查多語言支持

### 2. 構建測試
- [ ] 創建新的 Unity 項目
- [ ] 安裝包
- [ ] 驗證所有功能正常工作

### 3. 依賴項檢查
- [ ] 確認 URP 依賴項正確加載
- [ ] 檢查 Assembly 引用無錯誤

## 版本管理

### 更新版本號
1. 修改 `package.json` 中的版本號
2. 更新 `CHANGELOG.md`
3. 創建新的 Git 標籤：
   ```bash
   git tag v1.0.1
   git push origin v1.0.1
   ```

### 發布到 Unity Asset Store
1. 創建 .unitypackage 文件
2. 上傳到 Unity Asset Store
3. 設置價格和描述

## 故障排除

### 常見問題

1. **包未顯示在 Package Manager 中**：
   - 檢查 package.json 語法是否正確
   - 確認文件夾結構符合 UPM 標準

2. **Assembly 引用錯誤**：
   - 檢查 asmdef 文件中的引用名稱
   - 確認所有依賴項都已正確安裝

3. **功能不工作**：
   - 檢查 Unity 版本兼容性
   - 確認 URP 版本符合要求

## 下一步

- [ ] 設置 CI/CD 流程
- [ ] 創建自動化測試
- [ ] 設置文檔網站
- [ ] 準備發布到 Unity Asset Store

## 支援

如有問題，請：
1. 檢查本指南的故障排除部分
2. 查看 GitHub Issues
3. 聯繫維護者
