# Game-Off-2025_Wizard-And-Virtue-Endless-Souls
The Game Wizard And Virtue Endless Souls(WAVES) for game jam Game Off 2025

this is a 2D game***

## 开发规范 (Development Guidelines)

### 功能开发规范

**重要原则：功能逻辑与状态管理分离**

#### 1. 功能函数设计原则

- **功能函数必须独立**：每个功能函数只负责单一功能，不包含状态管理逻辑
- **禁止打包式函数**：不要创建类似 `StartTurn()`、`EndTurn()` 这样的打包式函数，它们会混合多个功能
- **函数命名清晰**：使用动词命名，如 `DrawCards()`、`EmitHandWave()`、`DiscardPendingCards()`
- **提供无返回值包装函数**：为所有功能函数提供无返回值的包装函数，方便在UnityEvent中调用
  - 包装函数命名：与功能函数同名，无返回值（如 `DrawCards()`）
  - 带返回值函数命名：添加 `WithResult` 后缀（如 `DrawCardsWithResult()`）

#### 2. 状态系统设计原则

- **状态系统只负责流程管理**：`GameStateManager` 只管理状态转换，不包含功能逻辑
- **通过 UnityEvent 调用功能**：在 Inspector 中配置 UnityEvent，将功能函数绑定到状态事件
- **状态转换自动处理**：状态系统自动处理状态转换和循环（如回合结束自动进入下一回合开始）

#### 3. 开发流程

1. **设计功能函数**：
   - 在对应的系统类中创建独立的功能函数
   - 函数只负责单一功能，参数明确，返回值清晰
   - 示例：`CardSystem.DrawCards(int drawCount = -1)`、`HandWaveGridManager.EmitHandWave()`

2. **配置状态系统**：
   - 在 `GameStateManager` 的 Inspector 中配置状态事件
   - 将功能函数绑定到对应的状态事件（如 On Turn Start、On Turn End）
   - 通过 UnityEvent 的拖拽方式绑定，无需编写额外代码

3. **测试和调试**：
   - 功能函数可以独立测试
   - 状态系统可以独立测试
   - 两者通过 UnityEvent 解耦，便于维护和扩展

#### 4. 示例

**正确示例**：
```csharp
// CardSystem.cs - 独立的功能函数（带返回值，供代码调用）
public int DrawCardsWithResult(int drawCount = -1) { ... }

// CardSystem.cs - 无返回值包装函数（供UnityEvent调用）
public void DrawCards() { DrawCardsWithResult(-1); }
public void DrawCards(int drawCount) { DrawCardsWithResult(drawCount); }

// HandWaveGridManager.cs - 独立的功能函数（带返回值，供代码调用）
public Wave EmitHandWaveWithResult() { ... }
public int DiscardPendingCardsWithResult() { ... }

// HandWaveGridManager.cs - 无返回值包装函数（供UnityEvent调用）
public void EmitHandWave() { EmitHandWaveWithResult(); }
public void DiscardPendingCards() { DiscardPendingCardsWithResult(); }

// GameStateManager.cs - 状态管理，通过UnityEvent调用功能
[SerializeField] private UnityEvent onTurnStart = new UnityEvent();
// 在Inspector中绑定：onTurnStart -> CardSystem.DrawCards()（无返回值版本）
```

**错误示例**：
```csharp
// ❌ 不要创建打包式函数
public void StartTurn() { DrawCards(); ... }  // 错误：混合了状态和功能
public Wave EndTurnWithResult() { EmitHandWave(); DiscardPendingCards(); ... }  // 错误：打包多个功能
```

#### 5. 当前系统的功能函数

**CardSystem**：
- `InitializeGame()` - 游戏初始化（无返回值）
- `DrawCards()` - 抽牌功能（无返回值包装，供UnityEvent调用）
- `DrawCards(int drawCount)` - 抽牌功能（无返回值包装，指定数量）
- `DrawCardsWithResult(int drawCount = -1)` - 抽牌功能（返回实际抽取的牌数，供代码调用）

**HandWaveGridManager**：
- `EmitHandWave()` - 发出手牌波（无返回值包装，供UnityEvent调用）
- `EmitHandWaveWithResult()` - 发出手牌波（返回发出的波，供代码调用）
- `DiscardPendingCards()` - 将待使用的卡牌放入弃牌堆（无返回值包装，供UnityEvent调用）
- `DiscardPendingCardsWithResult()` - 将待使用的卡牌放入弃牌堆（返回成功数量，供代码调用）
- `GenerateHitSequenceFromEmittedWave()` - 从发出的波生成有序波峰伤害列表（无返回值包装，供UnityEvent调用）
- `GenerateHitSequenceFromEmittedWaveWithResult()` - 从发出的波生成有序波峰伤害列表（返回伤害序列，供代码调用）

**DamageSystem**：
- `ProcessHitSequence(List<PeakHit> hitSequence)` - 处理有序波峰伤害列表（无返回值包装，供UnityEvent调用）
- `ProcessHitSequenceWithResult(List<PeakHit> hitSequence)` - 处理有序波峰伤害列表（返回处理结果，供代码调用）
- `ProcessHitSequenceAsync(List<PeakHit> hitSequence)` - 异步处理伤害序列（支持延迟，用于动画）

**EnemyWaveManager**：
- `SetEnemyWave(Wave wave)` - 设置当前敌人的波
- `LoadPresetWave(int presetIndex)` - 加载预设波
- `LoadRandomPresetWave()` - 随机加载一个预设波
- `ClearEnemyWave()` - 清空当前敌人的波

**DamageSystemHelper**：
- `ProcessEmittedWave()` - 完整流程：发出玩家波 -> 获取敌人波 -> 配对 -> 生成伤害序列 -> 处理伤害序列（无返回值包装，供UnityEvent调用）

**GameStateManager**：
- `EnterGameStart()` - 进入游戏开始状态
- `EnterTurnStart()` - 进入回合开始状态
- `EnterTurnEnd()` - 进入回合结束状态（自动循环到下一回合开始）

**使用说明**：
- 在UnityEvent中绑定：使用无返回值版本（如 `DrawCards()`、`EmitHandWave()`）
- 在代码中调用：使用带返回值版本（如 `DrawCardsWithResult()`、`EmitHandWaveWithResult()`）

## 伤害系统 (Damage System)

### 概述

伤害系统负责处理波系统输出的有序波峰伤害列表，逐个依次命中结算。系统设计遵循"有序波峰伤害列表逐个结算"的原则，支持表现层按波峰顺序播放扣血动画。

### 系统架构

#### 核心组件

1. **PeakHit** (`Assets/Scripts/DamageSystem/PeakHit.cs`)
   - 波峰伤害数据
   - 包含：`target`（目标实体）、`damage`（伤害值）、`orderIndex`（序号/时间顺序）

2. **HealthComponent** (`Assets/Scripts/DamageSystem/HealthComponent.cs`)
   - 统一的生命组件，用于玩家和敌人
   - 功能：回血、扣血、死亡、护盾
   - 护盾独立于血量计算，先扣除护盾，护盾不足时扣除生命值
   - 提供事件：`OnHealthChanged`、`OnShieldChanged`、`OnDamageTaken`、`OnHealed`、`OnDeath`

3. **TargetManager** (`Assets/Scripts/DamageSystem/TargetManager.cs`)
   - 目标管理器，管理玩家和敌人的引用
   - 支持自动查找（通过Tag）或手动设置
   - 根据攻击方向查找目标：`true` = 攻向敌人，`false` = 攻向玩家

4. **WaveHitSequenceGenerator** (`Assets/Scripts/WaveSystem/WaveHitSequenceGenerator.cs`)
   - 波伤害序列生成器（静态类）
   - 将波转换为有序波峰伤害列表
   - 根据攻击方向确定目标，使用强度绝对值确定伤害值，使用波峰位置确定序号

5. **DamageSystem** (`Assets/Scripts/DamageSystem/DamageSystem.cs`)
   - 伤害结算系统
   - 处理有序波峰伤害列表，逐个依次命中结算
   - 不区分玩家或敌人，只通过 `target` 和组件系统找到目标
   - 支持同步和异步处理（异步支持延迟，用于动画）
   - 提供事件：`OnHitSequenceStart`、`OnHitProcessed`（供表现层播放动画）、`OnHitSequenceComplete`

6. **DamageSystemHelper** (`Assets/Scripts/DamageSystem/DamageSystemHelper.cs`)
   - 伤害系统辅助类
   - 提供便捷方法，用于在 UnityEvent 中连接波系统和伤害系统
   - `ProcessEmittedWave()`：完整流程（发出波 -> 生成伤害序列 -> 处理伤害序列）

### 核心流程

1. **发出玩家波**：
   - 从手牌波格表管理器发出玩家的波（首个波峰对齐到0号位）

2. **获取敌人波**：
   - 从敌人波管理器获取当前敌人的波（支持预设占位符）

3. **配对两个波**：
   - 使用 `WavePairing.PairWaves()` 配对玩家波和敌人波
   - 配对后可能生成1个或2个结果波（根据攻击方向分类）
   - 相同位置的波峰会进行配对计算（强度相加，方向根据规则确定）

4. **生成伤害序列**：
   - 从配对后的波列表生成有序波峰伤害列表
   - 每个波峰转换为 `PeakHit`（包含目标、伤害值、序号）
   - 列表按 `orderIndex` 升序排序

5. **处理伤害序列**：
   - 遍历 `hitSequence`，逐个依次命中结算
   - 对于每个 `PeakHit`：
     - 通过 `target` 确定对象和其 `HealthComponent`
     - 若目标不存在或已经死亡，跳过
     - 调用目标扣血函数
     - 若目标在这次伤害后死亡，立刻更新死亡状态
   - 触发 `OnHitProcessed` 事件（供表现层播放动画）

### 使用示例

#### 1. 基本使用（通过 DamageSystemHelper）

```csharp
// 在 GameStateManager 的 UnityEvent 中绑定：
// onTurnEnd -> DamageSystemHelper.ProcessEmittedWave()

// 完整流程：
// 1. 发出玩家波
// 2. 获取敌人波（从 EnemyWaveManager）
// 3. 配对两个波
// 4. 生成伤害序列
// 5. 处理伤害序列
```

#### 2. 设置敌人波

```csharp
// 方式1：使用预设波
enemyWaveManager.LoadPresetWave(0); // 加载第一个预设波
enemyWaveManager.LoadRandomPresetWave(); // 随机加载预设波

// 方式2：设置自定义波
Wave customWave = new Wave();
customWave.AddPeak(0, 5, true); // 位置0，强度5，攻向敌人
enemyWaveManager.SetEnemyWave(customWave);
```

#### 3. 分步使用

```csharp
// 步骤1：生成伤害序列
List<PeakHit> hitSequence = handWaveGridManager.GenerateHitSequenceFromEmittedWaveWithResult();

// 步骤2：处理伤害序列
damageSystem.ProcessHitSequence(hitSequence);
```

#### 4. 异步处理（支持动画）

```csharp
// 使用异步处理，每个伤害之间有延迟
damageSystem.ProcessHitSequenceAsync(hitSequence);
```

#### 5. 监听动画事件

```csharp
// 在表现层监听 OnHitProcessed 事件，按波峰顺序播放扣血动画
damageSystem.OnHitProcessed.AddListener((hit, remainingHealth, remainingShield) => {
    // 播放扣血动画
    PlayDamageAnimation(hit.target, hit.damage);
});
```

### 设计原则

1. **配对机制**：玩家波和敌人波必须先进行配对，配对后的结果波才转换为伤害序列
2. **有序结算**：伤害序列必须按 `orderIndex` 升序排序，逐个依次结算
3. **通用性**：不区分玩家或敌人，只通过 `target` 和组件系统找到目标
4. **表现与逻辑分离**：通过事件系统预留接口，供表现层按波峰顺序播放动画
5. **护盾机制**：护盾独立于血量计算，先扣除护盾，护盾不足时扣除生命值
6. **敌人波管理**：使用预设占位符，未来可替换为随机或数据库实现

## 卡牌系统 (Card System)

### 概述

这是一个通用的卡牌系统，支持任意GameObject作为卡牌，可以适配不同类型的游戏。系统包含以下核心组件：

- **卡组 (Deck)**: 局外卡组，支持添加和删除卡牌
- **牌堆 (Draw Pile)**: 局内待抽取的牌堆，随机排列
- **手牌 (Hand)**: 局内当前持有的牌
- **弃牌堆 (Discard Pile)**: 局内使用过的牌

### 系统架构

#### 核心类

1. **CardDeck** (`Assets/Scripts/CardSystem/CardDeck.cs`)
   - 管理局外卡组（传统方式：直接存储Prefab引用）
   - 支持添加、删除、清空卡牌
   - 提供卡组副本创建功能

2. **CardPileManager** (`Assets/Scripts/CardSystem/CardPileManager.cs`)
   - 管理局内的三个牌堆（牌堆、手牌、弃牌堆）
   - 处理抽牌、弃牌、洗牌逻辑
   - 管理卡牌实例的创建和销毁

3. **CardSystem** (`Assets/Scripts/CardSystem/CardSystem.cs`)
   - 主控制器，协调卡组和牌堆管理器
   - 处理游戏初始化、回合抽牌等核心逻辑
   - 提供统一的API接口
   - 支持传统卡组和动态卡组数据两种方式

4. **CardPrefabRegistry** (`Assets/Scripts/CardSystem/CardPrefabRegistry.cs`) - 新增
   - 卡牌Prefab注册表（ScriptableObject）
   - 将卡牌ID映射到对应的Prefab
   - 支持在Inspector中编辑ID和Prefab的对应关系

5. **CardDeckData** (`Assets/Scripts/CardSystem/CardDeckData.cs`) - 新增
   - 卡组数据（ScriptableObject）
   - 存储卡牌ID和数量，而不是直接存储Prefab引用
   - 运行时根据此数据动态构建卡组

### 使用方法

#### 1. 设置场景

1. 在场景中创建一个GameObject，命名为 "CardSystem"
2. 添加 `CardSystem` 组件
3. 添加 `CardPileManager` 组件
4. 创建一个空的GameObject作为手牌容器：
   - `HandContainer` - 手牌容器（手牌实例会放在这里）
   - 注意：牌堆和弃牌堆只存储数据（Prefab引用），不需要容器
5. 在 `CardPileManager` 组件中，将手牌容器拖拽到 `Hand Container` 字段

#### 2. 配置卡组

**方法一：在Unity Inspector中配置（适合初始卡组）**
- 在 `CardSystem` 组件的 `Deck` 字段中，添加卡牌Prefab
- 设置 `Cards Per Turn`（每回合抽牌数量，默认5张）

**方法二：代码中动态配置（推荐）**
- 使用 `InitializeDeckFromList()` 或 `InitializeDeckFromArray()` 从列表/数组初始化卡组
- 使用 `AddCardsToDeck()` 批量添加卡牌
- 适合从配置文件、数据库或游戏逻辑中动态加载卡组

**方法三：使用动态卡组数据（最灵活，推荐用于生产环境）**
- 创建卡牌Prefab注册表（CardPrefabRegistry）：
  1. 在Project窗口中右键 → Create → Card System → Card Prefab Registry
  2. 设置卡牌ID和对应的Prefab映射
- 创建卡组数据（CardDeckData）：
  1. 在Project窗口中右键 → Create → Card System → Deck Data
  2. 设置卡组名称和卡牌列表（ID和数量）
- 在 `CardSystem` 组件中设置：
  - `Card Registry`: 拖拽刚才创建的注册表
  - `Deck Data`: 拖拽刚才创建的卡组数据
- 运行时调用 `InitializeGame()` 会自动根据卡组数据构建卡组
- **优势**：
  - 每个卡牌种类只需一个Prefab
  - 卡组数据与Prefab分离，易于管理和修改
  - 支持运行时切换不同的卡组数据
  - 适合从配置文件、数据库或游戏逻辑中动态加载

#### 3. 代码使用示例

**⚠️ 重要：必须通过 CardSystem 调用所有功能**

所有卡牌系统的操作都必须通过 `CardSystem` 组件的方法来调用，不要直接访问 `CardPileManager` 或 `CardDeck`。

```csharp
using CardSystem;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private CardSystem cardSystem;
    
    void Start()
    {
        // ✅ 正确：通过CardSystem调用
        cardSystem.InitializeGame();
        cardSystem.StartTurn();
        cardSystem.UseCard(card);
        cardSystem.AddCardToDeck(cardPrefab);
    }
    
    void OnCardClicked(GameObject card)
    {
        // ✅ 正确：使用CardSystem的方法
        cardSystem.UseCard(card);
    }
    
    // ❌ 错误示例：不要直接调用底层组件
    void WrongWay()
    {
        // cardSystem.PileManager.DrawCards(3);  // 错误！绕过了CardSystem的逻辑
        // cardSystem.Deck.AddCard(cardPrefab);  // 错误！绕过了CardSystem的逻辑
    }
    
void AddCardToDeck(GameObject cardPrefab)
{
    // 添加单张卡牌到卡组
    cardSystem.AddCardToDeck(cardPrefab);
}

void AddMultipleCardsToDeck(List<GameObject> cardPrefabs)
{
    // 批量添加卡牌到卡组
    int addedCount = cardSystem.AddCardsToDeck(cardPrefabs);
    Debug.Log($"成功添加 {addedCount} 张卡牌到卡组");
}

void InitializeDeckFromList(List<GameObject> cardPrefabs)
{
    // 从列表初始化卡组（清空现有卡组后添加）
    int count = cardSystem.InitializeDeckFromList(cardPrefabs);
    Debug.Log($"卡组已初始化，包含 {count} 张卡牌");
}

void AddCardDuringGameplay(GameObject newCard)
{
    // 游戏进行中，将新获得的卡牌添加到卡组和牌堆
    cardSystem.AddCardToDeckAndDrawPile(newCard, shuffle: true);
}

void RemoveCardFromDeck(GameObject cardPrefab)
{
    // 从卡组移除卡牌
    cardSystem.RemoveCardFromDeck(cardPrefab);
}
}
```

### API 文档

#### CardSystem 主要方法

**游戏流程：**
- `InitializeGame()`: 初始化游戏，将卡组洗入牌堆
- `StartTurn()`: 回合开始，抽取默认数量的牌
- `StartTurn(int drawCount)`: 回合开始，抽取指定数量的牌
- `UseCard(GameObject card)`: 使用卡牌，移动到弃牌堆
- `UseCardAt(int handIndex)`: 使用指定索引的手牌

**卡组管理（局外）：**
- `AddCardToDeck(GameObject cardPrefab)`: 添加单张卡牌到卡组
- `AddCardsToDeck(List<GameObject> cardPrefabs)`: 批量添加卡牌到卡组
- `AddCardsToDeck(GameObject[] cardPrefabs)`: 批量添加卡牌到卡组（数组版本）
- `InitializeDeckFromList(List<GameObject> cardPrefabs)`: 从列表初始化卡组（清空后添加）
- `InitializeDeckFromArray(GameObject[] cardPrefabs)`: 从数组初始化卡组（清空后添加）
- `RemoveCardFromDeck(GameObject cardPrefab)`: 从卡组移除卡牌

**牌堆管理（局内，游戏进行中可用）：**
- `AddCardToDrawPile(GameObject cardPrefab, bool shuffle = false)`: 将卡牌直接添加到牌堆
- `AddCardsToDrawPile(List<GameObject> cardPrefabs, bool shuffle = false)`: 批量将卡牌添加到牌堆
- `AddCardsToDrawPile(GameObject[] cardPrefabs, bool shuffle = false)`: 批量将卡牌添加到牌堆（数组版本）
- `AddCardToDeckAndDrawPile(GameObject cardPrefab, bool shuffle = false)`: 同时添加到卡组和牌堆
- `AddCardsToDeckAndDrawPile(List<GameObject> cardPrefabs, bool shuffle = false)`: 批量同时添加到卡组和牌堆
- `AddCardsToDeckAndDrawPile(GameObject[] cardPrefabs, bool shuffle = false)`: 批量同时添加到卡组和牌堆（数组版本）

**查询方法：**
- `GetHandCount()`: 获取当前手牌数量
- `GetDrawPileCount()`: 获取牌堆剩余数量
- `GetDiscardPileCount()`: 获取弃牌堆数量

#### CardDeck 主要方法

- `AddCard(GameObject cardPrefab)`: 添加单张卡牌到卡组
- `AddCards(List<GameObject> cardPrefabs)`: 批量添加卡牌到卡组
- `AddCards(GameObject[] cardPrefabs)`: 批量添加卡牌到卡组（数组版本）
- `InitializeFromList(List<GameObject> cardPrefabs)`: 从列表初始化卡组（清空后添加）
- `InitializeFromArray(GameObject[] cardPrefabs)`: 从数组初始化卡组（清空后添加）
- `RemoveCard(GameObject cardPrefab)`: 从卡组移除卡牌
- `RemoveCardAt(int index)`: 移除指定索引的卡牌
- `Clear()`: 清空卡组
- `IsEmpty()`: 检查卡组是否为空
- `CreateCopy()`: 创建卡组的副本

#### CardPileManager 主要方法

- `InitializeDrawPile(List<GameObject> deckCards)`: 初始化牌堆
- `DrawCards(int count)`: 从牌堆抽取指定数量的牌
- `DiscardCard(GameObject card)`: 将卡牌移动到弃牌堆
- `DiscardCardAt(int index)`: 将指定索引的手牌移动到弃牌堆
- `ReshuffleDiscardPileToDrawPile()`: 将弃牌堆洗回牌堆
- `AddCardToDrawPile(GameObject cardPrefab, bool shuffle = false)`: 直接向牌堆添加卡牌
- `AddCardsToDrawPile(List<GameObject> cardPrefabs, bool shuffle = false)`: 批量向牌堆添加卡牌
- `AddCardsToDrawPile(GameObject[] cardPrefabs, bool shuffle = false)`: 批量向牌堆添加卡牌（数组版本）
- `ClearAllPiles()`: 清空所有牌堆

### 游戏流程

1. **游戏初始化**
   - 调用 `InitializeGame()`，将卡组中的卡牌实例化并洗牌放入牌堆

2. **回合开始**
   - 调用 `StartTurn()`，从牌堆抽取指定数量的牌到手牌
   - 如果牌堆为空，自动将弃牌堆洗回牌堆

3. **使用卡牌**
   - 调用 `UseCard(card)` 或 `UseCardAt(index)`，将手牌移动到弃牌堆

4. **牌堆循环**
   - 当牌堆为空时，系统会自动将弃牌堆洗回牌堆，实现无限循环

### 特性

- **通用性**: 支持任意GameObject作为卡牌（3D对象、2D Sprite、UI元素等），不限制卡牌的具体实现
- **灵活性**: 支持动态管理卡组，可以批量添加、从列表初始化、游戏进行中添加新卡牌
- **自动化**: 牌堆为空时自动洗回弃牌堆
- **可扩展**: 清晰的架构设计，易于扩展新功能
- **性能优化**: 牌堆和弃牌堆只存储Prefab引用（数据），只有手牌会实例化，减少内存占用
- **动态管理**: 支持游戏进行中动态添加卡牌到卡组和牌堆，无需重新初始化游戏

### 注意事项

1. **⚠️ 必须通过 CardSystem 调用所有功能**：所有操作都必须通过 `CardSystem` 组件的方法来调用，不要直接访问 `CardPileManager` 或 `CardDeck`。直接访问底层组件会绕过自动逻辑（如牌堆为空时自动洗回）、错误处理和日志记录。

2. 卡牌Prefab可以是任意GameObject，系统只负责管理其位置和显示状态
3. **重要设计**：牌堆和弃牌堆只存储Prefab引用（数据），不会实例化GameObject；只有手牌会实例化
4. 抽牌时：从牌堆取出Prefab引用，实例化后放入手牌容器
5. 弃牌时：销毁手牌实例，将对应的Prefab引用存入弃牌堆
6. 洗牌时：只移动Prefab引用，不涉及实例操作
7. 所有调试信息都带有 `[CardSystem]` 或 `[CardPileManager]` 前缀，方便在日志中搜索
8. 系统使用Fisher-Yates算法进行洗牌，确保随机性

### 快速测试

详细的测试方案请参考：`Assets/Scripts/CardSystem/测试方案.md`

**方法一：使用测试脚本（推荐）**
1. 创建几个简单的测试卡牌Prefab（Cube或UI Image即可）
2. 在场景中创建CardSystem GameObject，添加CardSystem和CardPileManager组件
3. 创建HandContainer并赋值
4. 在CardSystem GameObject上添加 `CardSystemTester` 组件
5. 在CardSystemTester组件中配置测试卡牌Prefab列表
6. 在Inspector中右键点击CardSystemTester组件，选择 "运行所有测试"

**方法二：手动测试**
1. 创建几个简单的测试卡牌Prefab（Cube或UI Image即可）
2. 在场景中创建CardSystem GameObject，添加CardSystem和CardPileManager组件
3. 创建HandContainer并赋值
4. 在代码中调用 `InitializeGame()` 和 `StartTurn()` 进行测试

### 扩展建议

如果需要扩展功能，可以考虑：
- 添加事件系统（如OnCardDrawn、OnCardDiscarded等）
- 支持卡牌效果系统
- 添加卡牌动画和音效
- 实现卡牌筛选和搜索功能

---

## 波系统 (Wave System)

### 概述

波系统是游戏的核心数值模型，用于表示和计算波的交互。波由多个波峰组成，波峰可以位于波的任意位置，波中可以有空位。系统支持两个波之间的配对计算，根据波峰的位置、强度和攻击方向生成新的波。

### 系统架构

#### 核心类

1. **WavePeak** (`Assets/Scripts/WaveSystem/WavePeak.cs`)
   - 表示波的最小单位 - 波峰
   - 包含位置（Position）、强度值（Value）、攻击方向（AttackDirection）属性
   - 强度值为整数，可正可负，正负只表示数值符号，不表示方向
   - 攻击方向使用bool表示（true=攻向敌人，false=攻向玩家）

2. **Wave** (`Assets/Scripts/WaveSystem/Wave.cs`)
   - 管理波峰集合的容器
   - 使用Dictionary<int, WavePeak>存储波峰，key为位置，支持空位
   - **重要特性**：同一个波中的所有波峰必须具有相同的攻击方向
   - 提供AttackDirection属性，表示波的整体攻击方向
   - 如果尝试添加不同方向的波峰，会报错并拒绝添加
   - 提供FromPeaks静态方法，从波峰列表/数组直接创建波
   - 提供丰富的访问接口，支持UI显示需求
   - 支持添加、删除、查询波峰等操作

3. **WavePairing** (`Assets/Scripts/WaveSystem/WavePairing.cs`)
   - 静态工具类，处理两个波之间的配对逻辑
   - 相同位置的波峰会进行配对计算
   - 返回配对后生成的新波列表（可能包含1个或2个波）

### 基本概念

#### 波峰 (WavePeak)

波峰是波的最小单位，具有以下属性：
- **位置 (Position)**: 波峰在波中的位置（整数）
- **强度 (Value)**: 波峰的强度值（整数，可正可负）
- **攻击方向 (AttackDirection)**: 攻击方向（bool，true=攻向敌人）

#### 波 (Wave)

波是由多个波峰组成的集合：
- 波峰可以位于任意位置
- 波中可以有空位（某些位置没有波峰）
- **重要约束**：同一个波中的所有波峰必须具有相同的攻击方向
- 如果尝试添加不同方向的波峰，会报错并拒绝添加
- 使用Dictionary存储，以位置为key，支持高效查询
- 波具有AttackDirection属性，表示波的整体攻击方向（null表示空波）

#### 配对规则

当两个波进行配对时：
1. **位置匹配**: 相同位置的波峰会同时进行配对
2. **强度计算**: 两个波峰的强度相加（正负波会相互抵消，同号波会相互叠加）
3. **方向继承**: 
   - 如果攻击方向相同，直接继承该方向
   - 如果攻击方向相反，继承绝对值更大的波峰的攻击方向
   - 如果绝对值相等，默认使用第一个波峰的方向
4. **结果生成**: 根据攻击方向分类，生成1个或2个新波（不同方向的波峰会分开）

### 使用方法

#### 1. 创建波和波峰

```csharp
using WaveSystem;
using UnityEngine;

public class WaveExample : MonoBehaviour
{
    void Start()
    {
        // 创建一个波
        Wave wave = new Wave();

        // 方法1：直接添加波峰对象
        WavePeak peak1 = new WavePeak(position: 0, value: 10, attackDirection: true); // true=攻向敌人
        wave.AddPeak(peak1);

        // 方法2：使用便捷方法添加
        wave.AddPeak(position: 1, value: -5, attackDirection: false); // false=攻向玩家
        wave.AddPeak(position: 3, value: 8, attackDirection: true); // true=攻向敌人

        // 注意：位置2是空位，波中可以有空位
    }
}
```

#### 2. 查询和访问波峰

```csharp
// 检查指定位置是否有波峰
if (wave.HasPeakAt(1))
{
    // 获取波峰
    WavePeak peak = wave.GetPeak(1);
    Debug.Log($"位置1的波峰强度: {peak.Value}");
}

// 使用TryGetPeak方法（推荐）
if (wave.TryGetPeak(1, out WavePeak peak))
{
    Debug.Log($"位置1的波峰强度: {peak.Value}");
}

// 获取所有波峰（按位置排序）
List<WavePeak> sortedPeaks = wave.GetSortedPeaks();

// 获取指定范围内的波峰
List<WavePeak> peaksInRange = wave.GetPeaksInRange(minPosition: 0, maxPosition: 5);

// 遍历所有波峰
foreach (var peak in wave.Peaks)
{
    Debug.Log($"位置: {peak.Position}, 强度: {peak.Value}");
}

// 从波峰列表/数组创建波
List<WavePeak> peaksList = new List<WavePeak>
{
    new WavePeak(0, 10, true),
    new WavePeak(1, 5, true),
    new WavePeak(2, 8, true)
};
Wave waveFromList = Wave.FromPeaks(peaksList);

// 从数组创建
WavePeak[] peaksArray = peaksList.ToArray();
Wave waveFromArray = Wave.FromPeaks(peaksArray);

// 检查波的方向
if (wave.AttackDirection.HasValue)
{
    Debug.Log($"波的方向: {(wave.AttackDirection.Value ? "攻向敌人" : "攻向玩家")}");
}

// 注意：尝试添加不同方向的波峰会报错
Wave wave2 = new Wave();
wave2.AddPeak(0, 10, true);  // 成功
wave2.AddPeak(1, 5, false);  // 失败！会报错并拒绝添加
```

#### 3. 配对两个波

```csharp
// 创建两个波
Wave waveA = new Wave();
waveA.AddPeak(0, 10, true);   // 位置0，强度10，攻向敌人
waveA.AddPeak(1, 5, true);    // 位置1，强度5，攻向敌人

Wave waveB = new Wave();
waveB.AddPeak(0, -8, false);  // 位置0，强度-8，攻向玩家
waveB.AddPeak(2, 3, true);    // 位置2，强度3，攻向敌人

// 配对两个波
List<Wave> resultWaves = WavePairing.PairWaves(waveA, waveB);

// 结果可能包含1个或2个波（根据攻击方向分类）
foreach (Wave resultWave in resultWaves)
{
    Debug.Log($"结果波: {resultWave}");
    // 遍历结果波中的波峰
    foreach (var peak in resultWave.GetSortedPeaks())
    {
        Debug.Log($"  位置{peak.Position}: 强度{peak.Value}, 方向{(peak.AttackDirection ? "敌人" : "玩家")}");
    }
}
```

#### 4. 配对计算示例

**示例1：相同位置，方向相同**
- 波A: 位置0，强度10，攻向敌人
- 波B: 位置0，强度5，攻向敌人
- 结果: 位置0，强度15（10+5），攻向敌人

**示例2：相同位置，方向相反，强度抵消**
- 波A: 位置0，强度10，攻向敌人
- 波B: 位置0，强度-10，攻向玩家
- 结果: 位置0，强度0（10+(-10)），该波峰会保留在新波中（强度为0的波峰也会被存储）

**示例3：相同位置，方向相反，强度不抵消**
- 波A: 位置0，强度10，攻向敌人
- 波B: 位置0，强度-5，攻向玩家
- 结果: 位置0，强度5（10+(-5)），攻向敌人（因为10的绝对值大于5）

**示例4：不同位置**
- 波A: 位置0，强度10，攻向敌人
- 波B: 位置1，强度5，攻向玩家
- 结果: 两个波峰分别保留，生成2个波（一个包含位置0，一个包含位置1）

### API 文档

#### WavePeak 主要属性

- `int Position`: 波峰的位置
- `int Value`: 波峰的强度值（整数，可正可负）
- `bool AttackDirection`: 攻击方向（true=攻向敌人，false=攻向玩家）

#### WavePeak 主要方法

- `WavePeak(int position, int value, bool attackDirection)`: 构造函数
- `WavePeak Clone()`: 创建波峰的副本

#### Wave 主要属性

- `int PeakCount`: 获取波中波峰的数量
- `bool IsEmpty`: 检查波是否为空
- `bool? AttackDirection`: 波的攻击方向（null=空波，true=攻向敌人，false=攻向玩家）
- `IReadOnlyCollection<int> Positions`: 获取所有波峰的位置
- `IReadOnlyCollection<WavePeak> Peaks`: 获取所有波峰
- `IReadOnlyDictionary<int, WavePeak> PeakDictionary`: 获取所有波峰的键值对

#### Wave 主要方法

**添加和删除：**
- `void AddPeak(WavePeak peak)`: 添加波峰到波中（如果方向不一致会报错并拒绝添加）
- `void AddPeak(int position, int value, bool attackDirection)`: 在指定位置添加波峰（如果方向不一致会报错并拒绝添加）
- `bool RemovePeak(int position)`: 移除指定位置的波峰
- `void Clear()`: 清空所有波峰

**查询：**
- `bool HasPeakAt(int position)`: 检查指定位置是否存在波峰
- `WavePeak GetPeak(int position)`: 获取指定位置的波峰（不存在返回null）
- `bool TryGetPeak(int position, out WavePeak peak)`: 尝试获取指定位置的波峰
- `int GetMinPosition()`: 获取波的最小位置
- `int GetMaxPosition()`: 获取波的最大位置

**访问接口（用于UI显示）：**
- `List<WavePeak> GetSortedPeaks()`: 获取所有波峰的列表（按位置排序）
- `List<WavePeak> GetPeaksInRange(int minPosition, int maxPosition)`: 获取指定范围内的所有波峰（按位置排序）

**其他：**
- `Wave Clone()`: 创建波的副本
- `static Wave FromPeaks(IEnumerable<WavePeak> peaks)`: 从波峰列表创建波
- `static Wave FromPeaks(WavePeak[] peaks)`: 从波峰数组创建波

#### WavePairing 主要方法

- `List<Wave> PairWaves(Wave waveA, Wave waveB)`: 配对两个波，返回生成的新波列表

### 特性

- **稀疏存储**: 使用Dictionary存储波峰，支持空位，节省内存
- **高效查询**: 基于位置的O(1)查询性能
- **灵活配对**: 支持任意两个波的配对计算
- **方向分类**: 自动根据攻击方向分类生成新波
- **UI友好**: 提供丰富的访问接口，方便UI显示
- **数据安全**: 提供只读集合接口，防止意外修改

### 注意事项

1. **方向一致性约束**: 同一个波中的所有波峰必须具有相同的攻击方向。如果尝试添加不同方向的波峰，系统会报错并拒绝添加。这是为什么配对后根据方向生成新波的原因。
2. **配对计算粒度**: 所有判定都在最小单位（波峰）层面进行，不是作为一整条波计算
3. **强度为0的波峰**: 配对后如果波峰强度为0，该波峰仍会保留在新波中（强度为0的波峰也会被存储）
4. **方向相反时的规则**: 当两个波峰方向相反时，继承绝对值更大的波峰的方向；如果绝对值相等，使用第一个波峰的方向
5. **结果波数量**: 配对后可能生成1个或2个新波，取决于结果波峰的攻击方向是否一致
6. **空位处理**: 波中可以有空位，只有存在波峰的位置才会参与配对计算
7. **不进行碰撞检测**: 本系统只负责数据结构管理和配对计算，不包含碰撞检测逻辑，碰撞检测由其他系统负责调用配对逻辑

### 快速测试

**使用测试脚本（推荐）**

1. 在场景中创建一个空的GameObject，命名为 "WaveSystemTester"
2. 添加 `WaveSystemTester` 组件
3. 在Inspector中右键点击组件，可以看到所有可用的测试方法
4. 每个测试都可以单独运行，查看详细的数值变化和运行过程
5. 查看Console窗口的测试结果，每个测试都会显示：
   - 测试前的初始状态
   - 操作过程
   - 测试后的结果状态
   - 预期值和实际值的对比
   - 测试是否通过

**测试覆盖范围（共24个测试）**

**WavePeak测试（2个）：**
- 测试1: WavePeak创建 - 验证波峰的基本属性
- 测试2: WavePeak克隆 - 验证克隆功能

**Wave测试（11个）：**
- 测试3: Wave创建 - 验证波的初始状态
- 测试4: Wave添加波峰 - 验证添加功能和状态变化
- 测试5: Wave移除波峰 - 验证移除功能和状态变化
- 测试6: Wave查询波峰 - 验证各种查询方法
- 测试7: Wave空状态 - 验证空状态检查
- 测试8: Wave克隆 - 验证波的克隆功能
- 测试9: Wave排序 - 验证排序功能
- 测试10: Wave范围查询 - 验证范围查询功能
- 测试11: Wave最小最大位置 - 验证位置计算
- 测试22: 波方向一致性检查 - 验证方向一致性约束
- 测试23: 波特定位置波峰插入 - 验证在不同位置插入波峰

**WavePairing测试（10个）：**
- 测试12: 配对-同位置同方向 - 验证相同方向波峰的叠加
- 测试13: 配对-同位置反方向 - 验证相反方向波峰的计算
- 测试14: 配对-完全抵消 - 验证强度为0的波峰保留
- 测试15: 配对-反方向不同强度 - 验证方向继承规则
- 测试16: 配对-不同位置 - 验证不同位置波峰的处理
- 测试17: 配对-空波 - 验证空波的处理
- 测试18: 配对-一个空波 - 验证单空波的处理
- 测试19: 配对-零值波峰 - 验证零值波峰的处理
- 测试20: 配对-多波峰复杂场景 - 验证复杂场景的配对
- 测试21: 配对-方向分类 - 验证方向分类功能

**Wave创建测试（1个）：**
- 测试24: 根据给定数据生成波 - 验证FromPeaks静态方法

**测试输出说明**

每个测试都会详细显示：
- 测试前的数据状态（使用 `PrintWaveDetails` 方法显示波的完整信息）
- 操作步骤和中间结果
- 测试后的数据状态
- 预期值和实际值的对比
- 测试通过/失败的结果（绿色=通过，红色=失败）

**使用建议**

- 建议按顺序运行测试，从简单的WavePeak测试开始
- 每个测试都是独立的，可以单独运行和调试
- 通过查看Console输出，可以直观地看到每个操作对数据的影响
- 如果某个测试失败，可以查看详细的数值变化来定位问题

### 手牌波格表系统 (Hand Wave Grid System)

#### 概述

手牌波格表系统用于在Unity中可视化和管理手牌波的合成。系统包含以下组件：

- **HandWaveGridManager**: 管理手牌波和格表系统的主控制器
- **WaveCardComponent**: 波牌组件，可以挂载到GameObject/Prefab上
- **WaveGridSlot**: 格子组件，处理波牌的放置和移除

#### 系统架构

1. **HandWaveGridManager** (`Assets/Scripts/WaveSystem/HandWaveGridManager.cs`)
   - 管理手牌波和格表系统
   - 处理波牌的放置、撤回和发出
   - 维护格子字典，管理所有格子的状态

2. **WaveCardComponent** (`Assets/Scripts/WaveSystem/WaveCardComponent.cs`)
   - 波牌组件，可以挂载到GameObject/Prefab上
   - 承载波牌数据（WaveData格式）
   - 在Awake时从WaveData创建Wave对象

3. **WaveGridSlot** (`Assets/Scripts/WaveSystem/WaveGridSlot.cs`)
   - 格子组件，挂载到格子的GameObject上
   - 处理波牌的放置和移除
   - 维护格子的占用状态

#### 使用方法

##### 1. 设置场景

**步骤1：创建格表管理器**
1. 在场景中创建一个GameObject，命名为 "HandWaveGridManager"
2. 添加 `HandWaveGridManager` 组件
3. 设置 `Min Grid Position` 和 `Max Grid Position`（例如：-10 到 10）

**步骤2：创建格表容器**
1. 创建一个空的GameObject作为格表容器，命名为 "GridContainer"
2. 将容器拖拽到 `HandWaveGridManager` 组件的 `Grid Container` 字段
3. 在容器下创建多个子GameObject作为格子（可以使用Unity的Grid Layout Group自动排列）

**步骤3：设置格子**
1. 为每个格子GameObject添加 `WaveGridSlot` 组件
2. 在Inspector中设置每个格子的 `Grid Position`（必须与手牌波的位置对应，例如：-10, -9, ..., 0, ..., 9, 10）
3. 调整 `Card Placement Offset` 来设置波牌放置时的位置偏移

**步骤4：创建波牌Prefab**
1. 创建一个GameObject作为波牌Prefab
2. 添加 `WaveCardComponent` 组件
3. 在Inspector中设置 `Wave Data`：
   - `Attack Direction`: 设置为 `true`（朝向敌人）
   - `Peak Data`: 添加波峰数据（位置和强度值）
4. 保存为Prefab

##### 2. 代码使用示例

```csharp
using WaveSystem;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private HandWaveGridManager gridManager;
    [SerializeField] private WaveCardComponent cardPrefab;
    
    void Start()
    {
        // 获取手牌波
        Wave handWave = gridManager.HandWave;
        
        // 在指定位置放置波牌
        WaveCardComponent card = Instantiate(cardPrefab);
        int gridPosition = 5;  // 格子位置
        bool success = gridManager.PlaceCardAtPosition(card, gridPosition);
        
        if (success)
        {
            Debug.Log("波牌放置成功");
        }
    }
    
    void OnCardClicked(WaveCardComponent card, int gridPosition)
    {
        // 放置波牌
        gridManager.PlaceCardAtPosition(card, gridPosition);
    }
    
    void OnCardRemoved(int gridPosition)
    {
        // 撤回波牌
        gridManager.WithdrawCardFromPosition(gridPosition);
    }
    
    void OnEmitWave()
    {
        // 发出手牌波
        Wave emittedWave = gridManager.EmitHandWave();
        Debug.Log($"发出的波包含 {emittedWave.PeakCount} 个波峰");
    }
    
    void OnReset()
    {
        // 重置手牌波
        gridManager.ResetHandWave();
    }
}
```

##### 3. 重要说明

- **手牌波方向**: 手牌波永远是朝向敌人的（AttackDirection = true）
- **波牌方向**: 波牌的方向也应该是朝向敌人的（true）
- **格子位置**: 格子的位置必须与手牌波的位置对应，例如如果手牌波的位置范围是 -10 到 10，那么格子也应该有对应的位置
- **最尾端位置**: 当波牌放置在格子中时，使用格子的位置作为波牌的最尾端位置（TailEndPosition）与手牌波配对
- **手牌波不偏移**: 在合成过程中，手牌波本身不进行任何偏移，只有波牌会根据格子位置进行偏移
- **发出波**: 发出波时，会将手牌波的首个波峰对齐到0号位

##### 4. Unity侧设置（无需代码）

以下操作可以在Unity Inspector中直接完成：

1. **创建格表布局**:
   - 使用Unity的 `Grid Layout Group` 组件自动排列格子
   - 或者手动排列格子GameObject

2. **设置波牌Prefab**:
   - 在Inspector中直接编辑 `WaveCardComponent` 的 `Wave Data`
   - 添加波峰数据（位置和强度值）

3. **调整格子位置**:
   - 在Inspector中设置每个 `WaveGridSlot` 的 `Grid Position`
   - 调整 `Card Placement Offset` 来微调波牌放置位置

4. **调试**:
   - 启用 `Debug Print Wave Details` 来在控制台打印手牌波详情
   - 使用 `Refresh Grid` 上下文菜单来手动刷新格表

### 扩展建议

如果需要扩展功能，可以考虑：
- 添加波的运动和传播逻辑
- 实现波的碰撞检测系统
- 添加波的视觉效果和动画
- 支持波的持久化和序列化
- 实现波的合并和拆分功能
- 添加波牌的拖拽放置功能
- 实现格表的动态生成

### 波显示系统 (Wave Visualization System)

#### 概述

波显示系统用于在Unity UI中可视化显示手牌波和敌人波的波形。系统采用局部正弦片段的方式绘制波峰，支持手牌波对齐slot、敌人波独立显示等功能。

#### 核心组件

1. **WaveVisualizer** (`Assets/Scripts/WaveSystem/WaveVisualizer.cs`)
   - 通用波显示器组件
   - **自动挂载在波显示容器上，无需手动添加**
   - 负责根据波数据绘制波形

#### 配置方法（非常简单）

**重要：WaveVisualizer 组件会自动创建，无需手动挂载！你只需要设置容器即可。**

##### 1. 配置手牌波显示

1. **创建波显示容器**：
   - 在场景中创建一个UI GameObject（例如：Image或空GameObject），命名为 "HandWaveContainer"
   - 添加 `RectTransform` 组件（如果是空GameObject会自动添加）

2. **在 HandWaveGridManager 中设置**：
   - 找到 `HandWaveGridManager` 组件
   - 在 "波显示设置" 部分
   - 将 "HandWaveContainer" 拖拽到 `Wave Container` 字段

3. **完成！** 
   - 系统会在运行时自动在 `Wave Container` 上创建 `WaveVisualizer` 组件
   - 你可以在Hierarchy中看到 `HandWaveContainer` 下自动添加了 `WaveVisualizer` 组件

##### 2. 配置敌人波显示

1. **创建波显示容器**：
   - 在场景中创建一个UI GameObject（例如：Image或空GameObject），命名为 "EnemyWaveContainer"
   - 添加 `RectTransform` 组件（如果是空GameObject会自动添加）

2. **在 EnemyWaveManager 中设置**：
   - 找到 `EnemyWaveManager` 组件（通常挂载在敌人实体Prefab上）
   - 在 "波显示设置" 部分
   - 将 "EnemyWaveContainer" 拖拽到 `Wave Container` 字段

3. **完成！**
   - 系统会在运行时自动在 `Wave Container` 上创建 `WaveVisualizer` 组件

##### 3. 调整显示参数（可选）

如果需要调整波显示的外观，可以在运行时查看 `WaveVisualizer` 组件（自动创建在容器上）：
- `Peak Unit Height`: 强度为1的波峰高度（默认50）
- `Peak Width`: 每个波峰的宽度（默认100）
- `Line Width`: 波显示线条宽度（默认2）
- `Line Color`: 波显示线条颜色（默认白色）

**注意**：这些参数在运行时可以调整，但不会保存。如果需要永久保存，可以在代码中设置默认值。

#### 工作原理

1. **手牌波显示**：
   - 战斗开始时，系统自动获取所有slot的中心x坐标
   - 每个波峰点与对应slot的x值对齐（一一对应）
   - 波数据变化时（放置/撤回波牌）自动更新显示

2. **敌人波显示**：
   - 使用与手牌波相同的尺寸参数
   - 不对齐slot，使用默认间距
   - 敌人波数据变化时自动更新显示

3. **波峰绘制**：
   - 每个波峰是一个局部正弦片段（0到π），不是一个完整的sine周期
   - 不考虑两个波峰间的连接
   - 正值波峰向上，负值向下
   - 波峰高度与强度线性关系（强度1=基准高度，强度2=2倍高度）
   - 空位和强度为0的波峰绘制为直线
   - 初始图像是一条直线（长度是波长，即波峰数）

#### 配置总结

**你只需要做两件事**：
1. 创建一个UI GameObject作为容器（例如：Image或空GameObject）
2. 将这个容器拖拽到 `HandWaveGridManager` 或 `EnemyWaveManager` 的 `Wave Container` 字段

**系统会自动完成**：
- 在容器上创建 `WaveVisualizer` 组件
- 配置对齐设置（手牌波对齐slot，敌人波不对齐）
- 在波数据变化时自动更新显示

**不需要手动挂载任何组件！**

---

## 地图生成系统 (Map Generation System)

### 概述

地图生成系统用于生成爬塔类游戏的地图结构。系统采用"拓扑生成 + 内容填充"的两阶段设计,生成从下到上的有向无环图(DAG)结构,支持多条可选路线、分叉汇合、路径合理性约束等功能。

### 系统架构

#### 核心组件

1. **MapNode** (`Assets/Scripts/MapSystem/MapNode.cs`)
   - 地图节点数据结构
   - 包含:层数、列位置、节点类型、邻接关系、Boss/起点标记
   - 支持向上和向下的邻居查询

2. **MapTopology** (`Assets/Scripts/MapSystem/MapTopology.cs`)
   - 地图拓扑结构(DAG)
   - 管理所有节点和连接关系
   - 提供连通性检查、路径查找、统计信息等功能

3. **MapGenerationConfig** (`Assets/Scripts/MapSystem/MapGenerationConfig.cs`)
   - 地图生成配置(ScriptableObject)
   - 可在Inspector中配置不同章节/难度的参数
   - 包含拓扑生成参数、内容填充参数、路径合理性约束参数

4. **TopologyGenerator** (`Assets/Scripts/MapSystem/TopologyGenerator.cs`)
   - 拓扑生成器(静态类)
   - 负责生成地图的节点和路径结构
   - 实现分叉汇合控制、连通性保证、死节点修复等逻辑

5. **ContentFiller** (`Assets/Scripts/MapSystem/ContentFiller.cs`)
   - 内容填充器(静态类)
   - 在既定拓扑上为每个节点分配类型和事件
   - 支持全局配比 + 层级权重的分配策略
   - 提供路径合理性约束接口

6. **MapManager** (`Assets/Scripts/MapSystem/MapManager.cs`)
   - 地图管理器(MonoBehaviour)
   - 负责地图的生成、管理和运行时行为
   - 处理玩家移动、节点访问记录、事件触发等

7. **PathConstraints** (`Assets/Scripts/MapSystem/PathConstraints.cs`)
   - 路径合理性约束的示例实现
   - 包含最少休息节点约束、最多连续精英节点约束、重要节点分布约束等

### 核心概念

#### 地图结构

- **层级网格**: 地图由高度H(层数)和宽度W(每层最大节点数)定义
- **有向无环图(DAG)**: 只允许从第i层节点连接到第i+1层节点,不允许回退和横向循环
- **起点节点**: 底层的一个或多个起点节点
- **Boss节点**: 顶层的Boss节点,所有路径的终点

#### 生成流程

1. **拓扑生成阶段**:
   - 生成底层起点节点
   - 从下往上逐层生成节点和连接
   - 控制分叉和汇合的数量
   - 生成顶层Boss节点
   - 验证和修复连通性问题

2. **内容填充阶段**:
   - 按权重随机分配节点类型
   - 验证路径合理性并调整
   - 应用路径约束(如最少营火数、最多连续精英数等)

#### 节点类型

- 节点类型由内容填充系统分配,作为标记用于导向对应的事件系统
- 节点类型配置在Inspector中设置,包括:
  - 全局权重(在所有层级中的基础权重)
  - 层级权重曲线(按层数调整权重)
  - 最小/最大出现次数

### 使用方法

#### 1. 创建地图生成配置

1. 在Project窗口中右键 → Create → Map System → Map Generation Config
2. 配置基础参数:
   - `Height`: 地图高度(层数)
   - `Width`: 地图宽度(每层最大节点数)
3. 配置拓扑生成参数:
   - `Start Node Count`: 底层起点节点数量
   - `Min/Max Nodes Per Layer`: 每层最小/最大节点数
   - `Avg Out Degree`: 每个节点平均出度
   - `Connection Span`: 连接跨度
   - `Branch/Merge Probability`: 分叉/汇合概率
4. 配置内容填充参数:
   - 在`Node Type Configs`数组中添加节点类型配置
   - 为每个类型设置全局权重、层级权重曲线、最小/最大出现次数

#### 2. 设置场景

1. 在场景中创建一个GameObject,命名为 "MapManager"
2. 添加 `MapManager` 组件
3. 将创建的地图生成配置拖拽到 `Config` 字段
4. 设置 `Map Seed`(可选, -1表示随机)
5. 勾选 `Generate On Start` 以在游戏开始时自动生成地图

#### 3. 代码使用示例

```csharp
using MapSystem;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    
    void Start()
    {
        // 生成地图(使用指定种子)
        mapManager.GenerateMap(seed: 12345);
        
        // 获取当前节点
        MapNode currentNode = mapManager.CurrentNode;
        Debug.Log($"当前位置: {currentNode}");
        
        // 获取可以移动到的上层节点
        List<MapNode> availableNodes = mapManager.GetAvailableNextNodes();
        foreach (var node in availableNodes)
        {
            Debug.Log($"可以移动到: {node}");
        }
    }
    
    void OnNodeSelected(int nodeId)
    {
        // 移动到指定节点
        bool success = mapManager.MoveToNode(nodeId);
        if (success)
        {
            MapNode node = mapManager.CurrentNode;
            Debug.Log($"移动到: {node.NodeType}");
            
            // 根据节点类型触发对应事件
            TriggerNodeEvent(node.NodeType);
            
            // 检查是否到达Boss
            if (mapManager.IsAtBoss())
            {
                Debug.Log("到达Boss!");
            }
        }
    }
    
    void TriggerNodeEvent(string nodeType)
    {
        // 根据节点类型导向对应的事件系统
        // 这里只是示例,实际实现需要连接到游戏事件系统
        switch (nodeType)
        {
            case "Combat":
                // 触发战斗事件
                break;
            case "Elite":
                // 触发精英战斗事件
                break;
            case "Rest":
                // 触发营火事件
                break;
            case "Shop":
                // 触发商店事件
                break;
            case "Event":
                // 触发随机事件
                break;
        }
    }
}
```

#### 4. 路径合理性约束

系统提供了路径合理性约束接口,可以创建自定义约束:

```csharp
using MapSystem;

// 创建约束列表
List<ContentFiller.IPathConstraint> constraints = new List<ContentFiller.IPathConstraint>();

// 添加最少休息节点约束
constraints.Add(new MinRestNodesConstraint("Rest", minCount: 2));

// 添加最多连续精英节点约束
constraints.Add(new MaxConsecutiveEliteNodesConstraint("Elite", maxConsecutive: 2));

// 添加重要节点分布约束
string[] importantTypes = { "Shop", "Rest" };
constraints.Add(new ImportantNodesDistributionConstraint(importantTypes, minEarlyRatio: 0.2f, minLateRatio: 0.2f));

// 在生成地图时应用约束(需要在MapManager中扩展)
```

### API 文档

#### MapManager 主要方法

**地图生成:**
- `GenerateMap(int seed = -1)`: 生成地图(使用指定种子)
- `RegenerateMap(int newSeed = -1)`: 使用新种子重新生成地图
- `ResetMap()`: 重置地图(清除访问记录,重置玩家位置)

**节点查询:**
- `CurrentNode`: 获取当前玩家所在节点
- `CurrentNodeId`: 获取当前节点ID
- `GetAvailableNextNodes()`: 获取可以移动到的上层节点列表
- `GetNodeInfo(int nodeId)`: 获取指定节点的信息字符串
- `IsNodeVisited(int nodeId)`: 检查节点是否已访问

**移动和状态:**
- `MoveToNode(int targetNodeId)`: 移动到指定节点
- `IsAtBoss()`: 检查是否到达Boss节点

**事件:**
- `OnNodeEntered`: 节点进入事件(System.Action<MapNode>)
- `OnMapGenerated`: 地图生成完成事件(System.Action<MapTopology>)
- `OnNodeStateChanged`: 节点状态改变事件(System.Action)

#### MapTopology 主要方法

**节点管理:**
- `CreateNode(int layer, int column)`: 创建新节点
- `AddNode(MapNode node)`: 添加节点(用于从数据恢复)
- `GetNode(int nodeId)`: 获取指定节点
- `GetNodesAtLayer(int layer)`: 获取指定层的所有节点

**连接管理:**
- `AddEdge(int fromNodeId, int toNodeId)`: 添加有向边

**查询和验证:**
- `CheckConnectivity()`: 检查从起点到Boss的连通性
- `FindDeadEndNodes()`: 查找死节点(无法继续向上)
- `GetAllPathsToBoss()`: 获取从起点到Boss的所有路径
- `GetStatistics()`: 获取统计信息

#### TopologyGenerator 主要方法

- `Generate(MapGenerationConfig config, int seed = -1)`: 生成地图拓扑结构

#### ContentFiller 主要方法

- `FillContent(MapTopology topology, MapGenerationConfig config, int seed = -1, List<IPathConstraint> constraints = null)`: 填充节点内容

**接口:**
- `IPathConstraint`: 路径合理性约束接口
  - `CheckPath(List<int> path, MapTopology topology)`: 检查路径是否满足约束
  - `GetDescription()`: 获取约束描述

### 地图可视化系统

#### 概述

地图可视化系统用于在Unity UI中动态生成和显示地图。系统支持节点图像配置、树状连接线显示、节点状态可视化(当前/已访问/可访问/不可访问)和点击交互。

#### 核心组件

1. **MapNodeVisual** (`Assets/Scripts/MapSystem/MapNodeVisual.cs`)
   - 单个节点的可视化组件
   - 管理节点的UI显示、状态颜色、点击交互
   - 支持多种节点状态:正常、已访问、当前、可访问、不可访问

2. **MapVisualizer** (`Assets/Scripts/MapSystem/MapVisualizer.cs`)
   - 地图可视化管理器
   - 负责动态生成节点和连接线
   - 自动布局节点位置
   - 监听地图状态变化并更新可视化

3. **MapGenerationConfig可视化配置**
   - `NodeTypeVisualConfig[]`: 节点类型到图像素材的映射
   - `startNodeSprite`: 起点节点图像
   - `bossNodeSprite`: Boss节点图像
   - `defaultNodeSprite`: 默认节点图像

#### 使用方法

##### 1. 配置节点图像

1. 在`MapGenerationConfig`的Inspector中配置:
   - 在`Node Visual Configs`数组中添加节点类型可视化配置
   - 为每个节点类型设置对应的Sprite和节点大小
   - 设置起点节点图像、Boss节点图像和默认节点图像

##### 2. 创建节点和连接线Prefab

**创建节点Prefab:**
1. 在场景中创建一个UI Image GameObject
2. 添加`MapNodeVisual`组件
3. 可选:添加子对象作为背景或状态指示器
4. 保存为Prefab

**创建连接线Prefab:**
1. 在场景中创建一个UI Image GameObject(用于绘制连接线)
2. 设置Image的Color和Material(如果需要)
3. 保存为Prefab

##### 3. 设置场景

1. 在Canvas下创建一个GameObject,命名为 "MapVisualizer"
2. 添加`MapVisualizer`组件
3. 创建两个空的GameObject作为容器:
   - `NodeContainer` - 节点容器(添加RectTransform组件)
   - `LineContainer` - 连接线容器(添加RectTransform组件)
4. 在`MapVisualizer`组件中设置:
   - `Map Manager`: 拖拽MapManager组件
   - `Node Container`: 拖拽节点容器
   - `Line Container`: 拖拽连接线容器
   - `Node Prefab`: 拖拽节点Prefab
   - `Line Prefab`: 拖拽连接线Prefab
5. 配置布局参数:
   - `Layer Spacing`: 层间距(默认150)
   - `Node Spacing`: 节点间距(默认120)
   - `Line Width`: 连接线宽度(默认2)
   - `Line Color`: 连接线颜色
   - `Visited Line Color`: 已访问连接线颜色

##### 4. 代码使用示例

```csharp
using MapSystem;
using UnityEngine;

public class MapUIController : MonoBehaviour
{
    [SerializeField] private MapVisualizer mapVisualizer;
    [SerializeField] private MapManager mapManager;

    void Start()
    {
        // 监听节点点击事件
        if (mapVisualizer != null)
        {
            mapVisualizer.OnNodeClicked += HandleNodeClicked;
        }

        // 监听地图生成完成事件
        if (mapManager != null)
        {
            mapManager.OnMapGenerated += HandleMapGenerated;
        }
    }

    void HandleMapGenerated(MapTopology topology)
    {
        // 地图生成完成后,可视化系统会自动更新
        // 如果需要手动刷新,可以调用:
        // mapVisualizer.GenerateVisualization();
    }

    void HandleNodeClicked(MapNode node)
    {
        Debug.Log($"点击了节点: {node.NodeType}");
        // 根据节点类型触发对应事件
        TriggerNodeEvent(node.NodeType);
    }

    void TriggerNodeEvent(string nodeType)
    {
        // 连接到游戏事件系统
        switch (nodeType)
        {
            case "Combat":
                // 触发战斗事件
                break;
            case "Elite":
                // 触发精英战斗事件
                break;
            case "Rest":
                // 触发营火事件
                break;
            case "Shop":
                // 触发商店事件
                break;
        }
    }
}
```

#### 节点状态

系统支持以下节点状态,并自动更新颜色:

- **Normal(正常)**: 白色,未访问的普通节点
- **Visited(已访问)**: 灰色,已访问过的节点
- **Current(当前)**: 黄色,玩家当前所在的节点
- **Available(可访问)**: 绿色,可以移动到的上层节点
- **Unavailable(不可访问)**: 深灰色,不可访问的节点

#### 连接线显示

- 系统自动绘制从每个节点到其上层邻居的连接线
- 已访问的连接线会显示为不同的颜色(更暗)
- 连接线使用UI Image绘制,支持自定义宽度和颜色

#### API 文档

##### MapVisualizer 主要方法

- `GenerateVisualization()`: 生成地图可视化
- `UpdateNodeStates()`: 更新节点状态
- `RefreshVisualization()`: 刷新可视化(更新节点状态)
- `ClearVisualization()`: 清除所有可视化元素

**事件:**
- `OnNodeClicked`: 节点点击事件(System.Action<MapNode>)

##### MapNodeVisual 主要方法

- `Initialize(MapNode node, Sprite sprite, Vector2 size)`: 初始化节点可视化
- `SetState(NodeState state)`: 设置节点状态
- `GetState()`: 获取节点状态
- `SetSprite(Sprite sprite)`: 设置节点图像
- `GetWorldPosition()`: 获取节点世界位置
- `GetCenterPosition()`: 获取节点中心位置

**事件:**
- `OnNodeClicked`: 节点点击事件(System.Action<MapNodeVisual>)

#### 注意事项

1. **Prefab要求**: 节点Prefab必须包含`MapNodeVisual`组件,连接线Prefab必须包含`Image`和`RectTransform`组件
2. **自动更新**: 可视化系统会自动监听地图生成和状态改变事件,无需手动调用更新
3. **布局计算**: 节点位置根据层数和列位置自动计算,支持自定义层间距和节点间距
4. **性能考虑**: 对于大型地图(节点数>100),建议使用对象池优化节点和连接线的创建/销毁
5. **UI层级**: 连接线容器应该在节点容器之前(在Hierarchy中更靠上),确保连接线在节点下方显示

### 设计原则

1. **两阶段生成**: 拓扑生成和内容填充分离,便于调试和扩展
2. **参数化配置**: 通过ScriptableObject配置不同章节/难度的参数
3. **路径合理性**: 提供约束接口,确保生成的路径符合游戏节奏
4. **随机种子**: 支持随机种子,相同种子生成相同地图,便于调试和复盘
5. **节点类型分离**: 节点生成和实际节点事件分离,节点类型只作为标记,由独立的事件系统处理

### 注意事项

1. **节点类型配置**: 节点类型需要在Inspector中配置,系统不会自动创建节点类型
2. **事件系统集成**: 节点类型只是标记,需要连接到游戏事件系统来处理实际事件
3. **路径约束**: 路径合理性约束是可选的,如果不满足约束,系统会尝试调整但可能不完全满足
4. **性能考虑**: 对于大型地图(高度>50),路径查找可能较慢,建议优化或限制路径数量
5. **调试信息**: 所有调试信息都带有 `[MapManager]`、`[TopologyGenerator]`、`[ContentFiller]` 等前缀,方便在日志中搜索

### 扩展建议

如果需要扩展功能,可以考虑:
- 添加地图可视化编辑器
- 实现地图序列化和持久化
- 添加更多路径合理性约束
- 实现动态难度调整
- 添加地图预览功能
- 实现地图分享和种子系统

---

## 角色动态创建系统 (Character Dynamic Creation System)

### 概述

角色动态创建系统负责在战斗时动态生成玩家和敌人实体,战斗结束后自动清理。系统采用数据与实体分离的设计,玩家数据长期存在,实体仅在战斗时生成。

### 系统架构

#### 核心组件

1. **PlayerData** (`Assets/Scripts/CharacterSystem/PlayerData.cs`)
   - 玩家数据（ScriptableObject,持久化数据）
   - 存储:最大生命值、当前生命值、资源等
   - 即使实体不在场也可以修改数据（例如地图上的回复事件）

2. **PlayerEntityManager** (`Assets/Scripts/CharacterSystem/PlayerEntityManager.cs`)
   - 玩家实体管理器
   - 负责根据玩家数据动态生成和销毁玩家实体
   - 战斗开始时生成实体,战斗结束时同步数据并销毁实体

3. **EnemyConfig** (`Assets/Scripts/CharacterSystem/EnemyConfig.cs`)
   - 敌人配置（ScriptableObject）
   - 存储一组敌人的配置数据（生命值、波数据等）
   - 每次战斗根据当前关卡需要生成敌人

4. **EnemySpawner** (`Assets/Scripts/CharacterSystem/EnemySpawner.cs`)
   - 敌人生成器
   - 根据敌人配置动态生成敌人实体
   - 支持依次生成:第一次战斗生成第一个配置的敌人,第二次战斗生成第二个配置的敌人

### 使用方法

#### 1. 创建玩家数据

1. 在Project窗口中右键 → Create → Character System → Player Data
2. 配置玩家数据:
   - `Max Health`: 最大生命值（默认100）
   - `Current Health`: 当前生命值（默认100）
   - `Current Resource`: 当前资源值（可用于后续扩展）

#### 2. 创建玩家实体Prefab

**必须挂载的组件:**
- **HealthComponent** (`Assets/Scripts/DamageSystem/HealthComponent.cs`)
  - 生命值组件,用于管理生命值、护盾、死亡等
  - 在Inspector中设置初始最大生命值（会在生成时被玩家数据覆盖）

**可选组件:**
- 模型组件（SpriteRenderer、MeshRenderer等）
- UI组件（用于显示生命值等）
- 其他游戏逻辑组件

**设置Tag:**
- 将GameObject的Tag设置为 "Player"（或与PlayerEntityManager中的playerTag一致）

**示例步骤:**
1. 创建一个GameObject,命名为 "PlayerEntityPrefab"
2. 添加 `HealthComponent` 组件
3. 在 `HealthComponent` 中设置初始最大生命值（可选,会被玩家数据覆盖）
4. 添加玩家模型、UI等组件
5. 设置Tag为 "Player"
6. 保存为Prefab

#### 3. 创建敌人配置

1. 在Project窗口中右键 → Create → Character System → Enemy Config
2. 配置敌人实体设置:
   - `Enemy Entity Prefab`: 拖拽敌人实体Prefab
   - `Enemy Tag`: 设置敌人Tag（默认 "Enemy"）
3. 在 `Enemy Configs` 数组中添加敌人配置:
   - `Enemy Name`: 敌人名称
   - `Max Health`: 最大生命值
   - `Wave Data`: 敌人波数据（可选）
   - `Preset Wave Index`: 预设波索引（-1表示使用自定义waveData）

**敌人依次生成说明:**
- 第一次战斗:生成 `Enemy Configs[0]` 的敌人
- 第二次战斗:生成 `Enemy Configs[1]` 的敌人
- 第三次战斗:生成 `Enemy Configs[2]` 的敌人
- 以此类推
- 如果配置数量不足,会循环使用（例如有3个配置,第4次战斗会使用第1个配置）
- **所有敌人都生成在同一个位置**（使用第一个生成位置或默认位置）

#### 4. 创建敌人实体Prefab

**必须挂载的组件:**
- **HealthComponent** (`Assets/Scripts/DamageSystem/HealthComponent.cs`)
  - 生命值组件,用于管理生命值、护盾、死亡等
  - 在Inspector中设置初始最大生命值（会在生成时被敌人配置覆盖）

**可选但推荐挂载的组件:**
- **EnemyWaveManager** (`Assets/Scripts/WaveSystem/EnemyWaveManager.cs`)
  - 敌人波管理器,用于管理敌人的波数据
  - 如果挂载此组件,系统会自动应用配置中的波数据
  - 在Inspector中配置预设波列表（如果使用预设波索引）

**可选组件:**
- 模型组件（SpriteRenderer、MeshRenderer等）
- UI组件（用于显示生命值等）
- 其他游戏逻辑组件

**设置Tag:**
- 将GameObject的Tag设置为 "Enemy"（或与EnemyConfig中的enemyTag一致）

**示例步骤:**
1. 创建一个GameObject,命名为 "EnemyEntityPrefab"
2. 添加 `HealthComponent` 组件
3. 在 `HealthComponent` 中设置初始最大生命值（可选,会被敌人配置覆盖）
4. 添加 `EnemyWaveManager` 组件（推荐）
5. 在 `EnemyWaveManager` 中配置预设波列表（如果使用预设波索引）
6. 添加敌人模型、UI等组件
7. 设置Tag为 "Enemy"
8. 保存为Prefab

#### 5. 设置场景

**设置玩家实体管理器:**
1. 在场景中创建一个GameObject,命名为 "PlayerEntityManager"
2. 添加 `PlayerEntityManager` 组件
3. 配置组件:
   - `Player Data`: 拖拽创建的PlayerData ScriptableObject
   - `Player Entity Prefab`: 拖拽创建的玩家实体Prefab
   - `Player Spawn Point`: 创建空GameObject作为生成位置,拖拽到此字段（可选,不设置则使用默认位置）
   - `Player Tag`: 设置玩家Tag（默认 "Player"）

**设置敌人生成器:**
1. 在场景中创建一个GameObject,命名为 "EnemySpawner"
2. 添加 `EnemySpawner` 组件
3. 配置组件:
   - `Enemy Config`: 拖拽创建的EnemyConfig ScriptableObject
   - `Spawn Points`: 创建空GameObject作为生成位置,添加到列表（可选,不设置则使用默认位置）
     - **注意**: 所有敌人都生成在同一个位置（使用第一个生成位置或默认位置）
   - `Default Offset`: 默认生成位置偏移（当没有设置生成位置时使用）
   - `Default Spacing`: 默认生成间距（当没有设置生成位置时使用,但所有敌人生成在同一位置,此参数实际不使用）

**设置战斗流程:**
1. 在 `CombatNodeFlow` 组件中（如果存在）:
   - `Player Entity Manager`: 拖拽PlayerEntityManager组件（可选,系统会自动查找）
   - `Enemy Spawner`: 拖拽EnemySpawner组件（可选,系统会自动查找）

#### 6. 代码使用示例

```csharp
using CharacterSystem;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerEntityManager playerEntityManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerData playerData;

    void Start()
    {
        // 在地图上回复生命值（即使实体不在场）
        playerData.Heal(20);
        
        // 在地图上受到伤害（即使实体不在场）
        playerData.TakeDamage(10);
    }

    void OnCombatStart()
    {
        // 生成玩家实体（战斗开始时自动调用）
        playerEntityManager.SpawnPlayerEntity();
        
        // 生成敌人实体（战斗开始时自动调用,依次生成）
        enemySpawner.SpawnEnemies();
    }

    void OnCombatEnd()
    {
        // 销毁玩家实体并同步数据（战斗结束时自动调用）
        playerEntityManager.DestroyPlayerEntity();
        
        // 清除所有敌人实体（战斗结束时自动调用）
        enemySpawner.ClearAllEnemies();
    }

    void OnGameRestart()
    {
        // 重置战斗计数（重新开始游戏时）
        enemySpawner.ResetCombatCounter();
    }
}
```

### 工作流程

#### 玩家实体流程

1. **战斗开始**:
   - `CombatNodeFlow.StartFlow()` 被调用
   - 自动调用 `playerEntityManager.SpawnPlayerEntity()`
   - 从 `PlayerData` 读取数据并应用到实体
   - 实体显示在场景中

2. **战斗进行中**:
   - 玩家实体参与战斗逻辑
   - 生命值变化实时反映在实体上

3. **战斗结束**:
   - `CombatNodeFlow.FinishCombat()` 被调用
   - 自动调用 `playerEntityManager.DestroyPlayerEntity()`
   - 实体数据同步回 `PlayerData`
   - 实体被销毁

4. **地图上**:
   - 即使实体不在场,也可以修改 `PlayerData`
   - 例如:营火节点回复生命值、事件节点受到伤害等

#### 敌人实体流程

1. **战斗开始**:
   - `CombatNodeFlow.StartFlow()` 被调用
   - 自动调用 `enemySpawner.SpawnEnemies()`
   - 根据战斗计数依次生成敌人:
     - 第一次战斗:生成配置[0]的敌人
     - 第二次战斗:生成配置[1]的敌人
     - 以此类推
   - 所有敌人都生成在同一个位置
   - 应用配置的生命值和波数据

2. **战斗进行中**:
   - 敌人实体参与战斗逻辑
   - 生命值变化实时反映在实体上

3. **战斗结束**:
   - `CombatNodeFlow.FinishCombat()` 被调用
   - 自动调用 `enemySpawner.ClearAllEnemies()`
   - 所有敌人实体被销毁

4. **下次战斗**:
   - 战斗计数自动增加
   - 生成下一个配置的敌人

### API 文档

#### PlayerData 主要方法

**生命值操作:**
- `Heal(int healAmount)`: 回复生命值（即使实体不在场也可以调用）
- `TakeDamage(int damage)`: 受到伤害（即使实体不在场也可以调用）
- `SetMaxHealth(int newMaxHealth)`: 设置最大生命值
- `ResetHealth()`: 重置生命值

**数据同步:**
- `SyncFromEntity(HealthComponent entityHealth)`: 从实体同步数据到玩家数据（战斗结束时调用）
- `ApplyToEntity(HealthComponent entityHealth)`: 将数据应用到实体（战斗开始时调用）

#### PlayerEntityManager 主要方法

- `SpawnPlayerEntity()`: 生成玩家实体（战斗开始时调用）
- `DestroyPlayerEntity()`: 销毁玩家实体并同步数据（战斗结束时调用）
- `HidePlayerEntity()`: 隐藏玩家实体（不销毁）
- `ShowPlayerEntity()`: 显示玩家实体（如果被隐藏）

#### EnemySpawner 主要方法

- `SpawnEnemies(List<int> configIndices = null)`: 生成敌人实体（战斗开始时调用）
  - 如果不指定 `configIndices`,会按战斗计数依次生成单个敌人
  - 如果指定 `configIndices`,会生成指定配置的敌人
- `ClearAllEnemies()`: 清除所有敌人实体（战斗结束时调用）
- `RemoveEnemy(GameObject enemy)`: 清除指定敌人
- `ResetCombatCounter()`: 重置战斗计数（重新开始游戏时调用）

#### EnemyConfig 主要方法

- `GetEnemyConfig(int index)`: 获取指定索引的敌人配置
- `ConfigCount`: 获取配置数量

### 注意事项

1. **Prefab组件要求**:
   - 玩家实体Prefab必须包含 `HealthComponent` 组件
   - 敌人实体Prefab必须包含 `HealthComponent` 组件
   - 敌人实体Prefab推荐包含 `EnemyWaveManager` 组件（用于管理波数据）

2. **Tag设置**:
   - 玩家实体Prefab的Tag必须设置为 "Player"（或与PlayerEntityManager中的playerTag一致）
   - 敌人实体Prefab的Tag必须设置为 "Enemy"（或与EnemyConfig中的enemyTag一致）

3. **敌人依次生成**:
   - 敌人依次生成指的是以战斗为单位依次生成
   - 第一次战斗生成第一个配置的敌人,第二次战斗生成第二个配置的敌人
   - 所有敌人都生成在同一个位置（使用第一个生成位置或默认位置）
   - 如果配置数量不足,会循环使用

4. **数据同步**:
   - 战斗开始时,玩家数据会自动应用到实体
   - 战斗结束时,实体数据会自动同步回玩家数据
   - 即使实体不在场,也可以修改玩家数据

5. **自动集成**:
   - 如果设置了 `PlayerEntityManager` 和 `EnemySpawner`, `CombatNodeFlow` 会自动调用生成和清理方法
   - 如果没有设置,系统会使用场景中现有的玩家/敌人实体（向后兼容）

6. **调试信息**:
   - 所有调试信息都带有 `[PlayerEntityManager]`、`[EnemySpawner]`、`[PlayerData]` 等前缀,方便在日志中搜索

### 扩展建议

如果需要扩展功能,可以考虑:
- 添加玩家资源系统（能量、金币等）
- 实现敌人AI系统
- 添加角色动画和特效
- 实现角色升级系统

---

## 金币系统 (Coin System)

### 概述

金币系统是一个独立的货币管理系统,提供简单的数值加减逻辑,供其他系统调用。系统采用数据与逻辑分离的设计,使用 ScriptableObject 持久化金币数据,支持金币变化事件通知。

### 系统架构

#### 核心组件

1. **CoinData** (`Assets/Scripts/CurrencySystem/CoinData.cs`)
   - 金币数据（ScriptableObject,持久化数据）
   - 存储当前金币数量
   - 提供增加、减少、检查金币的方法
   - 扣金币时检查是否足够,如果不够返回 false

2. **CoinSystem** (`Assets/Scripts/CurrencySystem/CoinSystem.cs`)
   - 金币系统管理器（MonoBehaviour）
   - 提供金币的增减逻辑,配合 CoinData 使用
   - 提供事件通知（金币变化、花费失败）
   - 扣金币时配合检测金币是否足够的逻辑,如果不够返回给调用者处理

### 使用方法

#### 1. 创建金币数据

1. 在Project窗口中右键 → Create → Currency System → Coin Data
2. 配置金币数据:
   - `Current Coins`: 当前金币数量（默认0）

#### 2. 设置场景

1. 在场景中创建一个GameObject,命名为 "CoinSystem"
2. 添加 `CoinSystem` 组件
3. 在 `Coin System` 组件中,将创建的金币数据拖拽到 `Coin Data` 字段

#### 3. 代码使用示例

**基本使用:**

```csharp
using CurrencySystem;

// 获取 CoinSystem 引用
CoinSystem coinSystem = FindObjectOfType<CoinSystem>();

// 增加金币
coinSystem.AddCoins(100); // 增加100枚金币

// 检查金币是否足够
if (coinSystem.HasEnoughCoins(50))
{
    Debug.Log("金币足够");
}

// 尝试花费金币（推荐方式）
if (coinSystem.TrySpendCoins(50))
{
    Debug.Log("成功花费50枚金币");
    // 执行购买逻辑
}
else
{
    Debug.Log("金币不足,无法购买");
    // 处理金币不足的逻辑（例如显示提示UI）
}

// 直接减少金币（不推荐,不会检查是否足够）
coinSystem.RemoveCoins(30); // 直接减少30枚金币（如果不足会变成0）

// 设置金币数量
coinSystem.SetCoins(200); // 设置金币为200

// 重置金币
coinSystem.ResetCoins(0); // 重置为0
```

**监听事件:**

```csharp
// 监听金币变化事件
coinSystem.OnCoinsChanged.AddListener((currentCoins) =>
{
    Debug.Log($"金币数量变化: {currentCoins}");
    // 更新UI显示
});

// 监听花费失败事件
coinSystem.OnSpendFailed.AddListener((requiredAmount, currentCoins) =>
{
    Debug.Log($"金币不足: 需要 {requiredAmount}, 当前只有 {currentCoins}");
    // 显示提示UI
});
```

**在UnityEvent中使用:**

1. 在Inspector中找到需要调用金币系统的地方（例如商店UI按钮）
2. 将 `CoinSystem` 的 `TrySpendCoins` 方法拖拽到 UnityEvent
3. 设置需要花费的金币数量
4. 如果花费失败,可以监听 `OnSpendFailed` 事件来处理

### API 文档

#### CoinSystem 主要方法

- `AddCoins(int amount)`: 增加金币
  - 参数: `amount` - 增加的金币数量（必须为正数）
  - 返回: 实际增加的金币数量
  - 说明: 成功增加后触发 `OnCoinsChanged` 事件

- `TrySpendCoins(int amount)`: 尝试花费金币（推荐方式）
  - 参数: `amount` - 需要花费的金币数量（必须为正数）
  - 返回: `bool` - 是否成功花费（金币足够返回 true,不足返回 false）
  - 说明: 
    - 如果金币足够,扣除金币并返回 true,触发 `OnCoinsChanged` 事件
    - 如果金币不足,不扣除金币,返回 false,触发 `OnSpendFailed` 事件
    - **这是推荐的方式,因为会自动检查金币是否足够**

- `RemoveCoins(int amount)`: 减少金币（不检查是否足够）
  - 参数: `amount` - 减少的金币数量（必须为正数）
  - 返回: 实际减少的金币数量
  - 说明: 如果金币不足会变成0（不会变成负数）,不推荐使用,建议使用 `TrySpendCoins`

- `HasEnoughCoins(int amount)`: 检查金币是否足够
  - 参数: `amount` - 需要的金币数量
  - 返回: `bool` - 是否足够

- `SetCoins(int amount)`: 设置金币数量
  - 参数: `amount` - 新的金币数量（不能为负数）
  - 说明: 用于初始化或重置

- `ResetCoins(int initialAmount = 0)`: 重置金币
  - 参数: `initialAmount` - 初始金币数量（默认为0）

#### CoinSystem 主要属性

- `CurrentCoins`: 当前金币数量（只读）
- `CoinData`: 金币数据（可读写）
- `OnCoinsChanged`: 金币数量变化事件（UnityEvent<int>）
- `OnSpendFailed`: 尝试花费金币失败事件（UnityEvent<int, int>）

#### CoinData 主要方法

- `AddCoins(int amount)`: 增加金币
- `TrySpendCoins(int amount)`: 尝试花费金币（检查是否足够）
- `RemoveCoins(int amount)`: 减少金币（不检查是否足够）
- `HasEnoughCoins(int amount)`: 检查金币是否足够
- `SetCoins(int amount)`: 设置金币数量
- `ResetCoins(int initialAmount = 0)`: 重置金币

#### CoinData 主要属性

- `CurrentCoins`: 当前金币数量（只读）

### 设计原则

1. **数据与逻辑分离**: CoinData 存储数据,CoinSystem 提供逻辑和事件
2. **检查后扣费**: 扣金币时检查是否足够,如果不够返回 false,由调用者处理
3. **事件通知**: 提供金币变化和花费失败事件,方便UI更新
4. **持久化**: 使用 ScriptableObject 持久化数据,数据在游戏运行期间保持

### 注意事项

1. **推荐使用 TrySpendCoins**: 扣金币时应该使用 `TrySpendCoins` 方法,它会自动检查金币是否足够
2. **处理花费失败**: 如果 `TrySpendCoins` 返回 false,应该处理金币不足的逻辑（例如显示提示UI）
3. **事件监听**: 建议监听 `OnCoinsChanged` 事件来更新UI显示
4. **数据持久化**: CoinData 是 ScriptableObject,数据在游戏运行期间保持,但不会自动保存到磁盘（需要手动保存或使用其他持久化方案）
5. **游戏开始重置**: 在 `GameFlowManager` 中已配置自动重置金币功能,每次游戏开始时（地图生成后）会自动重置金币为初始值
   - 在 `GameFlowManager` 的 Inspector 中可以设置:
     - `Reset Coins On Game Start`: 是否在游戏开始时自动重置金币（默认开启）
     - `Initial Coins`: 游戏开始时的初始金币数量（默认0）
     - `Coin System`: 金币系统引用（如果为空,会自动查找）
6. **调试信息**: 所有调试信息都带有 `[CoinSystem]` 或 `[CoinData]` 前缀,方便在日志中搜索

### 扩展建议

如果需要扩展功能,可以考虑:
- 添加金币上限
- 实现金币历史记录
- 添加多种货币类型（钻石、银币等）
- 实现金币奖励系统
- 添加金币动画和特效
- 添加角色装备系统