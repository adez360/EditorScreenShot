# Unity插件转VPM包详细步骤

## 第一步：准备环境

### 1.1 安装VRChat SDK
确保你的Unity项目中已安装VRChat SDK：
- Worlds SDK
- UdonSharp
- VRChat Creator Companion

### 1.2 获取模板项目
1. 访问 https://github.com/vrchat-community/template-package
2. 点击 "Use this Template" 按钮
3. 创建新的仓库（例如：my-vrchat-package）
4. 克隆到本地

## 第二步：导入你的插件

### 2.1 组织插件文件
将你的插件文件按以下结构组织：
```
Assets/
├── YourPluginName/
│   ├── Editor/          # 编辑器脚本
│   │   ├── YourEditorScripts.cs
│   │   └── ...
│   ├── Runtime/         # 运行时脚本
│   │   ├── YourRuntimeScripts.cs
│   │   └── ...
│   ├── Resources/       # 资源文件（可选）
│   │   └── ...
│   └── ...
```

### 2.2 导入到Unity项目
1. 打开模板Unity项目
2. 将你的插件文件夹复制到Assets目录
3. 确保Unity正确识别所有文件

## 第三步：使用Package Maker工具

### 3.1 打开Package Maker
1. 在Unity菜单栏选择：`VRChat SDK / Utilities / Package Maker`
2. 等待工具窗口打开

### 3.2 配置转换设置
1. 在"Target Folder"字段中拖入你的插件文件夹
2. 在"Package ID"字段输入包ID（格式：com.yourname.packagename）
3. 从"Related VRChat Package"下拉菜单选择相关包
4. 确认设置无误后点击"Convert Assets to Package"

### 3.3 确认转换
1. 仔细阅读确认对话框中的更改说明
2. 点击"确认"开始转换
3. 等待转换完成

## 第四步：自定义包信息

### 4.1 编辑包清单
打开 `Packages/YourPackageName/package.json` 并修改：
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
  "vpmDependencies": {
    "com.vrchat.worlds": ">=2022.2.0"
  }
}
```

### 4.2 创建Assembly Definition文件
为你的脚本创建.asmdef文件：
```json
{
  "name": "YourPackageName.Runtime",
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

## 第五步：处理路径问题

### 5.1 更新硬编码路径
如果你的代码中有硬编码路径，需要更新：

**旧方式（不推荐）：**
```csharp
var asset = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/YourPlugin/icon.png");
```

**新方式1 - 使用Resources：**
```csharp
var asset = Resources.Load<Texture2D>("icon");
```

**新方式2 - 使用GUID：**
```csharp
string guid = "your-asset-guid-here";
string path = AssetDatabase.GUIDToAssetPath(guid);
var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
```

### 5.2 移动资源到Resources文件夹
1. 在包内创建Resources文件夹
2. 将需要动态加载的资源移入
3. 更新代码使用Resources.Load

## 第六步：测试和验证

### 6.1 本地测试
1. 在Unity中测试包的功能
2. 确保没有编译错误
3. 验证所有功能正常工作

### 6.2 创建测试场景
1. 创建测试场景验证包功能
2. 确保在VRChat环境中正常工作
3. 记录任何问题并修复

## 第七步：设置自动化发布

### 7.1 配置GitHub Actions
1. 在仓库设置中创建变量：
   - `PACKAGE_NAME`: com.yourname.packagename
2. 启用GitHub Pages
3. 设置源为"GitHub Actions"

### 7.2 发布版本
1. 更新package.json中的版本号
2. 运行"Build Release" action
3. 检查生成的发布

## 常见问题解决

### 问题1：路径错误
**症状：** 资源加载失败
**解决：** 使用Resources.Load或GUID方式

### 问题2：Assembly引用错误
**症状：** 编译错误
**解决：** 检查.asmdef文件配置

### 问题3：依赖问题
**症状：** 包无法加载
**解决：** 检查vpmDependencies配置

## 完成检查清单

- [ ] 插件文件正确组织在Editor和Runtime文件夹
- [ ] 创建了正确的package.json文件
- [ ] 创建了Assembly Definition文件
- [ ] 更新了所有硬编码路径
- [ ] 测试了包的功能
- [ ] 设置了自动化发布
- [ ] 创建了发布版本

完成这些步骤后，你的Unity插件就成功转换为VPM包了！

