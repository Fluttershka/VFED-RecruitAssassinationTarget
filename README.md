# Vanilla Factions Expanded - Deserters: Recruit As Assassination
# 边缘世界MOD：逃兵扩展 - 招募刺杀目标视作刺杀成功

该 MOD 专门用于扩展与优化 **Vanilla Factions Expanded - Deserters (VFED)** 模组中的刺杀任务（Plot Missions）逻辑。

---

## 🌟 核心功能

在原版 VFED 中，刺杀任务（针对骑士、执政官、男爵、伯爵等帝国贵族的行动）仅监听贵族的死亡信号（`noble.Killed`）。如果玩家将刺杀目标击倒俘虏并带回殖民地招募，刺杀任务不仅无法完成，反而会在将俘虏打包带离任务地图时提前判定为失败（`noble.LeftMap`），导致逃兵阴谋线卡死。

**本 MOD 彻底解决了这一问题：**

1. **招募即视为刺杀成功**：
   - 当刺杀目标被成功招募为殖民者时，自动触发刺杀任务成功信号。
   - 逃兵网络将立刻结算任务奖励（情报、可见度增益与物品空投），并正常解锁后续更高阶的刺杀阴谋线。
2. **俘虏押送保护（防止提前判定失败）**：
   - 当刺杀目标被击倒、逮捕或装入玩家的远行队押送回殖民地时，拦截错误的 `LeftMap` 逃跑判定，允许玩家安全将俘虏押回基地进行感化招募。
   - 若囚犯中途发动越狱（Prison Break）真正逃跑离开地图，依然会正常判定任务失败。
3. **中途加入与旧档兼容**：
   - 内置后台自动扫描组件，即使在安装本 MOD 前已经招募了刺杀目标导致任务悬挂，加载游戏后也会在数秒内自动检测并触发任务完成与后续推进。
4. **Mod 选项自由配置**：
   - 可以在 **选项 -> Mod选项 -> 逃兵扩展：招募视作刺杀成功** 中自由开启或关闭：
     - 招募为殖民者视作成功（默认开启）
     - 奴役为奴隶视作成功（可选，默认关闭）
     - 押送防提前失败保护（默认开启）
     - 招募完成专属信件提醒（默认开启）

---

## 📂 文件安装位置

MOD 已为您直接部署至游戏根目录的 Mods 文件夹中：
`D:\SteamLibrary\steamapps\common\RimWorld\Mods\VFED_RecruitAssassinationTarget`

同时完整源代码与工程项目备份保留在：
`C:\Users\kuwah\.gemini\antigravity\scratch\VFED_RecruitAssassinationTarget`

---

## 🚀 启用方法

1. 启动 **RimWorld**；
2. 进入游戏主界面的 **“模组 (Mods)”** 菜单；
3. 找到并勾选 **`Vanilla Factions Expanded - Deserters: Recruit As Assassination`**；
4. 确保其排序位于 **`Vanilla Factions Expanded - Deserters`** 之后；
5. 点击重启游戏即可生效！
