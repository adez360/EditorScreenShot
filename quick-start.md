# VPM包转换快速开始

## 🚀 5分钟快速转换

### 步骤1：获取模板项目
1. 访问 [VRChat Package Template](https://github.com/vrchat-community/template-package)
2. 点击 **"Use this Template"** 创建新仓库
3. 克隆到本地：`git clone https://github.com/你的用户名/你的仓库名.git`

### 步骤2：准备你的插件
将你的Unity插件文件按以下结构组织：
```
Assets/
└── YourPlugin/
    ├── Editor/          # 编辑器脚本
    ├── Runtime/         # 运行时脚本
    └── Resources/       # 资源文件（可选）
```

### 步骤3：使用Package Maker工具
1. 在Unity中打开项目
2. 菜单：`VRChat SDK / Utilities / Package Maker`
3. 拖入你的插件文件夹到"Target Folder"
4. 输入包ID：`com.你的名字.插件名`
5. 选择相关VRChat包
6. 点击 **"Convert Assets to Package"**

### 步骤4：测试和发布
1. 检查 `Packages/你的包名/package.json` 文件
2. 测试包功能是否正常
3. 设置GitHub Actions自动化发布

## ⚠️ 重要注意事项

### 路径问题
如果你的代码中有硬编码路径，需要更新：

**❌ 错误方式：**
```csharp
var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/YourPlugin/icon.png");
```

**✅ 正确方式：**
```csharp
var texture = Resources.Load<Texture2D>("icon");
```

### 包ID命名
- 使用反向域名格式：`com.你的域名.包名`
- 如果没有域名：`com.你的用户名.包名`
- 必须唯一，不能与现有包冲突

### Assembly Definition
转换后会自动创建.asmdef文件，确保：
- Runtime脚本引用正确
- Editor脚本引用Runtime包
- 没有循环依赖

## 🔧 常见问题解决

### 问题1：转换后出现编译错误
**原因：** 硬编码路径问题
**解决：** 使用Resources.Load或GUID方式

### 问题2：包无法在VRChat中加载
**原因：** 依赖配置错误
**解决：** 检查package.json中的vpmDependencies

### 问题3：编辑器工具不显示
**原因：** Assembly Definition配置问题
**解决：** 检查Editor.asmdef文件

## 📋 转换检查清单

- [ ] 插件文件正确组织在Editor和Runtime文件夹
- [ ] 使用Package Maker工具转换
- [ ] 检查并修复硬编码路径
- [ ] 验证package.json配置
- [ ] 测试包功能
- [ ] 设置自动化发布

## ⌨️ 快捷键说明

### 编辑器快捷键
- **O键**：切换场景同步开关
- **P键**：捕获当前帧截图
- **Ctrl+Alt+E**：打开编辑器截图工具面板

### 运行时快捷键（Play Mode）
- **O键**：切换场景同步
- **P键**：捕获截图
- **R键**：切换Freecam锁定模式

### 快捷键使用说明
1. 在Unity编辑器中，快捷键在Scene View和Game View中都有效
2. 运行时快捷键需要在场景中添加`ESSPlayHotkeys`组件
3. 快捷键可以在`Edit > Shortcuts`菜单中自定义修改

## 🎯 下一步

转换完成后，你可以：
1. 自定义包信息（名称、描述、版本）
2. 设置GitHub Actions自动化发布
3. 创建包文档和说明
4. 发布到VRChat社区

需要帮助？查看详细指南：
- [完整转换指南](convert-to-vpm.md)
- [包模板设置](package-template-setup.md)
- [自动化脚本](automation-scripts.md)
