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
   - 管理局外卡组
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