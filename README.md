# Moon Game Framework

![](https://img.shields.io/badge/version-v0.1-green.svg)
![](https://img.shields.io/badge/license-MIT-blue.svg)

## 环境

- unity 2021.3
- .net standard 2.1

## 大纲

<!-- TODO 引入html不方便，看怎么整吧 -->

<!-- [mindmap](Docs/src/mgf_mindmap.html) -->

**Core**
- Main
  - 程序入口
  - Mono回调
  - 开启协程
- Service
  - 简易服务器定位
  - 代替单例
- Event
  - 基于EventHandler/EventArgs的事件系统
  - 可全局使用、也可局部使用
- Memory
  - SharedPool，只考虑管理c#对象
  - ObjectPool，取自UGUI，可以管理任何对象
- [Assets](https://github.com/Sarofc/com.saro.moonasset)
  - 基于XAsset4.x改写
  - 管理Editor、Runtime的资源加载
  - 一键打包
  - 打包流程扩展
  - 资源依赖分析
  - 资源分组策略
  - 内置资源打包
  - 资源更新（WIP）
- VirtualFileSystem
  - 减少小文件数量
  - 提高IO效率
- Net
  - 文件下载器
  - Http封装(WIP)
  - TCP、KCP封装(WIP)
- Collections
  - TBinaryHeap
  - TLRUCache
  - TMultiMap
  - TSortedMultiMap
  - TLinkedList
- Utils
  - 插值
  - 曲线
  - 加密
  - 哈希
  - 反射
  - 动画曲线

**Vendor**
- UniTask
- NewtonsoftJson
<!-- - Protobuff-net -->

**Common**
- [DataTable](https://github.com/Sarofc/GTable)
  - excel生成二进制数据
  - 支持1-4个int作为key
  - 代码自动生成
  - 异步加载
- Audio
  - UnityFMOD
  - Wwise/FMOD(TODO)
- UI（WIP）
  - 自动组件绑定，代码生成
  - 可扩展的UI窗体动画组件
  - UI窗体管理、窗体组件抽象
  - UI层级管理，可穿插粒子特效
  - 半自动引用计数的资源加载
  - 自动反注册ui事件
  <!-- - UI模板预制体（TODO） -->
- Localization
  - 支持自定义数据源
  - 支持本地化各种类型的资源
- Hotupdate
  - hybridclr
  - Lua（废弃）
- XConsole
  - 游戏内控制台
  - 支持大量log显示
  - log折叠、log等级分类
  - 命令绑定，通过字符串调用
  - 命令输入补全提示

**Workflow**
- CodeGen
  - EventArgs、UI等代码模板
- [读PSD文件，生成UGUI预制件](https://github.com/Sarofc/com.saro.psd2ugui)（TODO 仍需考量做到什么程度）
- Editor
  - 多种Attributes扩展编辑器功能
  - 提供模型预览窗口

<!-- **Gameplay**
- 游戏性标记（GameplayTag）
- 角色属性系统（GameplayAttribute）
- 技能系统（ability、buff）（WIP）
- Motion系统（动编）（WIP）
- 路径系统（cinemachine的）
- 场景交互系统（3dGamekit的）
- 2D纸娃娃换装系统
- 任务系统（WIP）
- 剧情脚本系统（TODO） -->

## 单元测试

使用TestRunner，测试代码位于Tests~文件夹下

## Demo

[tetris-ecs](https://github.com/Sarofc/tetris-ecs-unity)

## 参考

- [GameFramework](https://github.com/EllanJiang/GameFramework)
- [UFlux](https://github.com/yimengfan/BDFramework.Core)
- [Loxondon Framework](https://github.com/vovgou/loxodon-framework)
- [ET](https://github.com/egametang/ET)

## 引用

- [UniTask](https://github.com/Cysharp/UniTask)
- [NewtonsoftJson](https://docs.unity3d.com/2019.4/Documentation/Manual/com.unity.nuget.newtonsoft-json.html)