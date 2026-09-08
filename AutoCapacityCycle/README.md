# Auto Capacity Cycle（自动容量循环）

《城市：天际线》一代 MOD：**垃圾填埋场 / 垃圾转运设施 / 公墓** 达到容量上限时自动触发原版"清空"流程，清空完成后自动恢复正常运营，全程无需手动操作；玩家随时可在游戏内 `Options → Mods` 关闭。

## 功能

- **垃圾填埋场 / 垃圾转运设施（Level3）**
  垃圾量达到容量上限时，自动设置 `Building.Flags.Downgrading` 触发原版 `GarbageMove` 卡车清空转移，并拦截原版 `CapacityFull` / `LandfillFull` / `WasteTransferFacilityFull` 红色告警；空仓后自动清除 `Downgrading` 恢复正常收垃圾。
- **公墓**（仅 `m_graveCount > 0` 的 `CemeteryAI`；火葬场不受影响）
  尸体数达到 `m_graveCount` 上限时自动触发原版灵车 `DeadMove` 清空流程；空仓后自动清除 `Downgrading` 恢复正常收尸。
- 发电 / 回收等处理型设施（`m_electricityProduction != 0` 或 `m_materialProduction != 0`）不参与——原版本就对它们不做"装满"检查。

## 游戏内开关（Options → Mods → Auto Capacity Cycle）

两个独立复选框，默认开启、即时生效、XML 本地持久化：

1. 垃圾填埋场 / 垃圾转运设施：满仓自动清空
2. 公墓：满仓自动清空（火葬场不受影响）

## 编译与部署

### 环境

- 目标框架：`.NET Framework 3.5`（net35）
- 依赖（需手动放入本目录 `Libs/`，源码仓库不包含）：
  - `Assembly-CSharp.dll`、`ColossalManaged.dll`、`ICities.dll`、`UnityEngine.dll` —— 从游戏安装目录 `Cities_Data/Managed/` 复制
  - `CitiesHarmony.Harmony.dll` —— 从创意工坊 "CitiesHarmony" MOD 文件夹复制（编译期引用，不随 MOD 分发）
  - `CitiesHarmony.API.dll` —— 从任一已订阅的基于 CitiesHarmony 的 MOD 文件夹复制（需随 MOD 一起分发）

### 构建 & 自动部署

```bat
dotnet build
```

构建成功后会自动把 `AutoCapacityCycle.dll` 与 `CitiesHarmony.API.dll` 复制到：

```
%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\AutoCapacityCycle\
```

游戏内启用 CitiesHarmony 与本 MOD 即可生效。

## 技术实现

- 通过 **Harmony 2**（CitiesHarmony 提供运行时）以静态方法 `Prefix`/`Postfix` 注入：
  - `LandfillSiteAI.CheckCapacity`（Prefix：满仓自动置 Downgrading 并拦截原版满仓标记）
  - `LandfillSiteAI.SimulationStepActive`（Postfix：空仓后清除 Downgrading）
  - `CemeteryAI.CheckCapacity`（Prefix，同上）
  - `CemeteryAI.SimulationStepActive`（Postfix，同上）
- 依据游戏原版反编译逻辑，容量与"满"的判定与原版 `CheckCapacity`/`IsFull` 完全一致。

## 说明 / 已知机制

- "清空"走的是原版转移机制：垃圾/尸体会被卡车/灵车运往**其它有空间或有处理能力的设施**，不会凭空消失。若全城接收能力不足，清空可能停滞（原版机制如此，非 MOD 缺陷）。
- 卸载或关闭 MOD 不修改存档数据；再次启用即恢复。
