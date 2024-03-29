﻿# 工具开发

**杂项**
- [ ] 处理多播委托+=/-=的gc问题（考虑替换城自己写的Delegates，list vs linkedlist，tmp有个FastAction，可以参考）
- [ ] webgl支持
- [ ] exception 调整优化
- [ ] iservice的一些管理器抽象成接口，例如音效尽可能抽象为支持fmod、wwise无缝切换
- [ ] vfs提供readasync接口，span接口

<!-- **async/await最佳实践**
- [ ] 测试一下async有没有延迟一帧，还是说一帧就能完成
- [ ] 中断async方法得最佳实践，从易用性、稳定性多个角度触发
- [ ] 测试，使用extension方式支持的await，是否能await多次呢？例如 IAssetHandle对象 -->

**资源模块**
- [x] 委托 对异步支援有限，不能等待整个方法体完成，例如OnRemoteAsset，ui出现时机有误，删掉所有资源，runtime模式就有问题，这种情况，就预先async加载，然后，委托里同步加载好了
- [x] unity2021测试spriteatlas，有bug，贴图冗余了 =》sbp不能勾选builtin，并且是打包图集，不能打包散图，然后维护一个散图路径到图集路径的关系，通过加载图集，再从图集里加载萨纳土
- [ ] 资源路径长度，有没有必要进行优化，例如，只使用 Res/Runtime 后的路径，其他舍弃
- [x] 自动回收机制，尽可能让帧率更平滑，每帧不要回收过多资源；不开启的话，切场景才会卸载ab。
    - 似乎完全没必要做rc=0时的延迟回收机制，上层模块自己管理就好了，可以考虑对象池这一层来做
- [ ] MoonAsset里的各种数组，可不可以无序？可以的话，直接反向循环，或者进行swap back操作，减少数组copy消耗
- [x] IAssetHandle池化，优先级低，先研究下其必要性，收益不大则不要考虑，先把其他整完后再来处理！=》上层有缓存了，可以不用考虑了
- [ ] 资源热更新，加载流程梳理，使其更健壮
    - [x] 自动补充下载 ui处理，下载时，提示ui
    - [x] 需要新增一个API，用来校验本地资源完整性，通过manifest来处理
    - [x] 缺失资源时，能够自己下载资源，并挂起逻辑，等待下载完毕，边玩边下，全异步化
    - [ ] 分包管理器，提供自动下载，手动下载机制。目的是，闲时可以把资源下全。
    - [ ] 覆盖安装、全新安装，都要删除dlc目录的manifest文件？换了url后，本地的manifest指向的还是旧的url
    - [x] 避免读取到未下载完成的资源。下载时搞个.tmp后缀，下载完成后，去掉
    - [x] 下载器组件测试，包括切片下载
    - [ ] 下载器组件，限速功能有问题，应提供一个默认速度，[最小，最大]速度，设置速度应在这个区间里
    - [ ] 自动补充下载相关异步api需要返回文件状态，例如GetRawFileAsync。同时事件也需要完善下，OnLoadAsset相关的。
- [ ] builtinshader 和 收集的svb不是一个包有没有问题？
- [x] 编辑器下支持读取打包后ab，从dlc目录而非streammingasset目录
- [ ] 大量资源文件，取文件 hash 前2位字符作为文件夹，避免同一文件夹文件数量过多
    - 这个考虑加个开关，需要上层无感知，保存直接走同一接口，保证编辑器、资源服同步，加载需要搞一个fallback机制，保证能共存，实现在LocalAssetLocator即可
    - 资源服上也要考虑
- [x] 打包log，rawbundle的清单也要=》manifest即所有数据了
- [ ] 目前，场景ab的rc貌似是没有管理的，需要考虑卸载场景后，移除其rc，考虑使用一个管理类来处理
- [x] 优化raw bundle打包
  - [x] 1.支持LoadRawAsset在编辑器下的加载？项目自己用宏控制
  - [x] 2.框架只管理硬盘文件（wwise,bank,vfs），叫做rawfile。vfs相关打包加载，项目自己实现
  - [x] 3.rawfile同bundle，维护一个assetpath到rawfile的映射，可以随意appendhash
  - [ ] 4.rawfile新增readalltext接口，现在只有readallbytes接口
- [ ] tmp 打包要剔除掉 resources 目录？改起来貌似不怎么方便
- [ ] api整理，优化

**UI模块**
- [ ] UI估计还是要重构下，需要更清爽、简洁
  - [ ] 和多语言放一起考虑
- [ ] 调研BD的FluxUI
  - [x] 数据绑定的价值？没啥价值的话，就采用原来的UIBinder模式
  - [x] 改装成异步逻辑，整合资源框架自动卸载
  - [x] FluxUI没有Canvas层级管理
  - [x] FluxUI没有Animation，继续完善
  - [x] 异步加载时，需要在ui根层加载一个mask，屏蔽点击
  - [ ] 测试Components组合成window
  - [ ] 异步加载时，可能存在潜在层级问题。如果不会同时打开多个ui，应该还行
  - [x] API整理，代码模板确定
- [ ] 队列机制 弹窗显示，用于规范化各种弹窗，目前的感觉不太行，看看怎么改进
- [ ] UI特效测试，特效mask
- [ ] 全面屏幕适配
- [ ] ui默认组件、资源 一键解包（upm的sample，或者unity package）
- [x] 窗口动画，使用接口，支持dotween、animator
- [ ] sprite大图，封装工具类，加载时显示加载中，加载完成后，再赋值给ui组件
- [ ] UI业务逻辑脚本的Awake等事件，需要在ui框架里try一下，避免crash，async void吞异常

**多语言**
- [ ] 继续思考如何简化工作流
  - [ ] 方便接入已经开发完成的项目
  - [ ] 方便使用，可以快速预览多语言结果
  - [ ] 多语言字典，key到底是id，还是string=》直接枚举int，数据表工具看看要不要改

**音效模块**
- [ ] unity自带的，重构下，支持自动卸载机制？管理器直接使用IAssetLoader。音效一定时间不用后，才会考虑自动卸载。
- [ ] 支持wwise

**表格工具**
 - [x] 异步接口
 - [x] 最好支持，框架点击一个按钮就能直接使用（能不能做成 unity package）=》unity可以直接使用upm了
 - [x] 打表前，删掉自动生成的文件，避免出现一些错误 =》 自己手动删除，更可控一些
 - [ ] 生成的文件，如何处理多dll（考虑弄成一个单独的dll，给其他所有程序集引用，感觉可以建一个asmdef），需要考虑到hybridclr的更新问题

**行为树**
- [x] 编辑器，看怎么处理 =》使用unity自带的graphview
- [ ] 运行时，保持不变，继续优化

**XConsole**
- [x] 重写UI
- [ ] 整理成package
- [ ] 整理代码，测试
- [ ] log过长后，要截断
- [ ] 保存配置，OnApplicationPause平台差异处理

**网络**
- [ ] Transport层抽象
  - [x] LiteNetLib
- [ ] 协议层抽象
  - [x] MemoryPack

**代码热更新**

- [x] 接入正式版版hybirdclr
