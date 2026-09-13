# CatClaw.Shared — 猫爪家族共享代码库

猫爪音乐（`../CatClawMusic`）与猫爪影视（`../CatClawVideo`）的跨应用共享代码。

## 项目结构

| 项目 | 目标框架 | 内容 |
|---|---|---|
| `src/CatClaw.Shared.Core` | `net11.0`（纯 C#，无 MAUI） | JS 运行时基类等与 UI 无关的共享逻辑 |
| `src/CatClaw.Shared.Maui` | `net11.0;net11.0-android;net11.0-windows10.0.19041.0`（Windows 条件） | 转换器、行为等 MAUI 层共享代码 |

## 接入方式

两个 App 通过跨仓库相对路径 ProjectReference 引用（两仓库固定平级放在 `D:\Code` 下）：

- App 的 Core/Data 工程 → 引用 `CatClaw.Shared.Core`
- App 的 Maui 工程 → 引用 `CatClaw.Shared.Maui`（传递引用 Core）

## 引用规则（重要）

1. **插件 SDK 接口不下沉**：`IJsRuntimeService` 等被已发布插件（`.ccp`）按类型标识消费的接口
   必须留在各宿主 Core 内，共享库只承载"实现基类"。App 侧用
   `sealed class JsRuntimeService : JsRuntimeServiceBase, 本应用 IJsRuntimeService`
   子类桥接，差异（递归上限 / 超时 / 文案）通过构造参数表达。
2. **XAML 兼容占位**：MAUI 层类型上移后，App 内保留同名子类
   （`class DownloadStatusConverter : CatClaw.Shared.Maui.Converters.DownloadStatusConverter`），
   既有 XAML xmlns 与资源字典零改动。
3. **共享库内不得出现应用专属文案/品牌名**，需要差异时一律走构造参数或抽象成员。
4. **依赖版本对齐**：Jint/Acornima/MAUI 版本必须与两个 App 的引用保持一致，升级需三处同步。

## 工具

- `tools/scan_dup.py` — 跨仓库归一化相似度扫描（去命名空间/注释/品牌名后做哈希与 Jaccard 比对），
  用于发现新的共享候选。运行：`python tools/scan_dup.py`
