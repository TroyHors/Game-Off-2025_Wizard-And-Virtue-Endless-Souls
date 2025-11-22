# Game-Off-2025_Wizard-And-Virtue-Endless-Souls
The Game Wizard And Virtue Endless Souls(WAVES) for game jam Game Off 2025

this is a 2D game***
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
   - 攻击方向使用bool表示（true=攻向玩家，false=不攻向玩家）

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
- **攻击方向 (AttackDirection)**: 攻击方向（bool，true=攻向玩家）

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
        WavePeak peak1 = new WavePeak(position: 0, value: 10, attackDirection: true);
        wave.AddPeak(peak1);

        // 方法2：使用便捷方法添加
        wave.AddPeak(position: 1, value: -5, attackDirection: false);
        wave.AddPeak(position: 3, value: 8, attackDirection: true);

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
    Debug.Log($"波的方向: {(wave.AttackDirection.Value ? "攻向玩家" : "不攻向玩家")}");
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
waveA.AddPeak(0, 10, true);   // 位置0，强度10，攻向玩家
waveA.AddPeak(1, 5, true);    // 位置1，强度5，攻向玩家

Wave waveB = new Wave();
waveB.AddPeak(0, -8, false);  // 位置0，强度-8，不攻向玩家
waveB.AddPeak(2, 3, true);    // 位置2，强度3，攻向玩家

// 配对两个波
List<Wave> resultWaves = WavePairing.PairWaves(waveA, waveB);

// 结果可能包含1个或2个波（根据攻击方向分类）
foreach (Wave resultWave in resultWaves)
{
    Debug.Log($"结果波: {resultWave}");
    // 遍历结果波中的波峰
    foreach (var peak in resultWave.GetSortedPeaks())
    {
        Debug.Log($"  位置{peak.Position}: 强度{peak.Value}, 方向{(peak.AttackDirection ? "玩家" : "其他")}");
    }
}
```

#### 4. 配对计算示例

**示例1：相同位置，方向相同**
- 波A: 位置0，强度10，攻向玩家
- 波B: 位置0，强度5，攻向玩家
- 结果: 位置0，强度15（10+5），攻向玩家

**示例2：相同位置，方向相反，强度抵消**
- 波A: 位置0，强度10，攻向玩家
- 波B: 位置0，强度-10，不攻向玩家
- 结果: 位置0，强度0（10+(-10)），该波峰会保留在新波中（强度为0的波峰也会被存储）

**示例3：相同位置，方向相反，强度不抵消**
- 波A: 位置0，强度10，攻向玩家
- 波B: 位置0，强度-5，不攻向玩家
- 结果: 位置0，强度5（10+(-5)），攻向玩家（因为10的绝对值大于5）

**示例4：不同位置**
- 波A: 位置0，强度10，攻向玩家
- 波B: 位置1，强度5，不攻向玩家
- 结果: 两个波峰分别保留，生成2个波（一个包含位置0，一个包含位置1）

### API 文档

#### WavePeak 主要属性

- `int Position`: 波峰的位置
- `int Value`: 波峰的强度值（整数，可正可负）
- `bool AttackDirection`: 攻击方向（true=攻向玩家，false=不攻向玩家）

#### WavePeak 主要方法

- `WavePeak(int position, int value, bool attackDirection)`: 构造函数
- `WavePeak Clone()`: 创建波峰的副本

#### Wave 主要属性

- `int PeakCount`: 获取波中波峰的数量
- `bool IsEmpty`: 检查波是否为空
- `bool? AttackDirection`: 波的攻击方向（null=空波，true=攻向玩家，false=不攻向玩家）
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