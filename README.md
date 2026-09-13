# Brotato（类土豆兄弟）

2D 幸存者类 Roguelite 游戏，Unity（团结引擎）实现，数据驱动架构。

玩家选好角色、难度和初始武器后进入战斗：角色自动瞄准并攻击最近的敌人，玩家只需走位躲伤害。每波限时生存，击杀掉落金币和经验，升级时三选一强化属性，波次结束进商店买道具。撑完全部波次获胜，血量归零结束。

## 开发环境

- 引擎：团结引擎 2022.3.48t7（Unity 中国版，对应 Unity 2022.3 LTS）
- 语言：C#
- 第三方库：Newtonsoft.Json（JSON 配置解析）
- 版本控制：Plastic SCM

## 玩法流程

```
主菜单 → 选角色 → 选难度 → 选武器 → 战斗（多波次）
                                      ↓ 每波结束
                                    商店买道具
                                      ↓
                              通关 / 死亡 → 结算面板 → 主菜单
```

## 目录结构

```
Assets/
├── Scenes/
│   ├── 01-MainMenu1.scene      主菜单
│   ├── 02-LevelSelect.scene    选角色 + 选难度 + 选武器
│   └── 03-GamePlay.scene       战斗主场景
├── Script/
│   ├── Model/                  数据模型（与 JSON 字段一一对应）
│   │   ├── EnemyData / WeaponData / RoleData / PropData
│   │   ├── PassiveData / WaveData / DifficultyData
│   ├── Enemy/EnemyBase.cs      敌人基类：移动、攻击、掉落、精英化
│   ├── UI1/                    各选择界面的按钮逻辑
│   ├── Player.cs               玩家：移动、受伤、经验、升级、生成武器
│   ├── WeaponBase.cs           武器基类：自动瞄准、冷却、旋转、开火
│   ├── Pistol.cs / MeleeWeapon.cs   远程 / 近战武器
│   ├── Bullet.cs               子弹飞行与碰撞
│   ├── BulletPool.cs / EnemyPool.cs 对象池
│   ├── LevelController.cs      波次调度与刷怪
│   ├── UpgradePanel.cs         升级三选一
│   ├── ShopPanel.cs            商店
│   ├── GamePanel.cs            HUD
│   ├── ResultPanel.cs          结算
│   ├── SettingsPanel.cs        音量设置
│   ├── AudioManager.cs         音频管理（音效 + 背景音乐）
│   ├── SaveManager.cs          存档（PlayerPrefs）
│   ├── PropManager.cs          道具效果
│   ├── GameData.cs             跨场景传递选择结果
│   └── DamageNumber.cs         伤害飘字
├── Editor/BuildScript.cs       命令行打包入口
└── Resources/
    ├── Data/                   JSON 配置表
    ├── Image/  Music/  Prefabs/  Fonts/
```

## 数据驱动设计

游戏的数值全部放在 `Resources/Data/*.json`，改数值不需要动代码：

| 文件 | 内容 |
|---|---|
| `role.json` | 角色：血量、速度、被动、立绘路径 |
| `weapon.json` | 武器：伤害、冷却、范围、远近战、暴击 |
| `enemy.json` | 敌人：血量、速度、伤害、贴图 |
| `level0~5.json` | 各难度下的波次配置、刷怪时间轴、精英怪 |
| `prop.json` | 道具：价格、各项属性加成 |
| `difficulty.json` | 难度列表 |

运行时由 `Model/` 下的数据类反序列化后灌入对应组件，例如武器通过 `WeaponBase.InitFromData(WeaponData)` 初始化。

## 技术要点

- **对象池**：子弹和敌人走 `BulletPool` / `EnemyPool` 复用，避免高频 `Instantiate`/`Destroy` 产生 GC 卡顿。
- **数据驱动**：所有数值外置到 JSON，代码只负责读表和应用。
- **UI 代码生成**：升级面板、商店、设置面板等在运行时用代码构建，不依赖预制体。
- **跨场景传参**：`GameData` 静态类保存玩家在选人界面的选择，战斗场景读取。
- **存档**：`SaveManager` 用 PlayerPrefs 记录最高波次、总击杀、总金币、总局数，主菜单展示。
- **音频**：`AudioManager` 单例 + `DontDestroyOnLoad`，区分音乐和音效两个音源，音量可调并持久化。

## 运行

用团结引擎 2022.3.48t7 打开项目根目录，打开 `Assets/Scenes/01-MainMenu1.scene`，点 Play。

## 打包

编辑器里：菜单 `Build → Build Windows x64`。

命令行：

```bash
<Tuanjie安装路径>/Editor/Tuanjie.exe -quit -batchmode \
  -projectPath <项目路径> \
  -executeMethod BuildScript.BuildWindows \
  -logFile build.log
```

产物输出到 `Build/Windows/Brotato.exe`。

## 已知限制

- 角色条件型被动（如"装备某类武器时加攻速"）未实现，目前只做了数值型被动。
- 击退、武器特有机能（吸血、贯通、叠加增伤）未实现。
- 波次倒计时结束时场上残留的敌人会延续到下一波。
- 难度差异目前只体现在波次配置和精英怪上。
