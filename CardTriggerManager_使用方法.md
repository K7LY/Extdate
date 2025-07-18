# CardTriggerManager 使用方法

## 概要

`CardTriggerManager` は、イベントが発生した際に出ているカードのうちトリガー可能なカードの一覧を取得し、管理するためのシステムです。

## 主な機能

### 1. トリガー可能なカードの取得

イベント発生時に、全プレイヤーの出ているカードから該当するトリガータイプのカードを検索し、実際にトリガー可能かどうかを判定します。

### 2. 実行可能性の判定

各カードの効果について、以下の条件をチェックします：
- 使用回数制限（一度だけ使用、ラウンド毎の制限など）
- トリガー条件の適合性
- コンテキスト依存の条件（特定プレイヤーのみ、特定アクションのみなど）

### 3. カード効果の実行

トリガー可能と判定されたカードの効果を実際に実行します。

### 4. 自動カード追加機能 ⭐ NEW

カードをプレイ、職業カード獲得、進歩カード獲得時に、自動的にトリガー可能なカードの一覧に追加され、効果が分析されます。

## 使用方法

### 基本的な使用例

```csharp
// GameManagerから使用する場合
GameManager gameManager = FindObjectOfType<GameManager>();

// 1. 収穫時にトリガー可能なカードの一覧を取得
var triggerableCards = gameManager.GetTriggerableCards(OccupationTrigger.OnHarvest);

// 2. 実際に実行可能なカードのみを取得
var activeCards = gameManager.GetActiveTriggerableCards(OccupationTrigger.OnHarvest);

// 3. 全てのトリガー可能なカードを実行
gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnHarvest, currentPlayer);
```

### 特定プレイヤーのカードのみを取得

```csharp
CardTriggerManager cardTriggerManager = FindObjectOfType<CardTriggerManager>();
Player player = gameManager.CurrentPlayer;

// 特定プレイヤーの収穫時トリガー可能カードを取得
var playerCards = cardTriggerManager.GetTriggerableCardsForPlayer(player, OccupationTrigger.OnHarvest);
```

### アクション実行時のトリガー

```csharp
// アクション実行時にトリガー可能なカードを取得
ActionSpace actionSpace = /* 実行されたアクションスペース */;
Player currentPlayer = gameManager.CurrentPlayer;

var actionTriggerableCards = gameManager.GetTriggerableCards(
    OccupationTrigger.OnAction, 
    currentPlayer, 
    actionSpace
);
```

### デバッグ情報の表示

```csharp
// トリガー可能なカードの詳細情報をコンソールに出力
gameManager.DebugPrintTriggerableCards(OccupationTrigger.OnHarvest);

// 全プレイヤーのトリガー可能カード状況をサマリー表示
gameManager.DebugPrintTriggerSummary();
```

## 利用可能なトリガータイプ

- `OccupationTrigger.Immediate` - 即座（カードプレイ時）
- `OccupationTrigger.OnAction` - アクション実行時
- `OccupationTrigger.OnHarvest` - 収穫時
- `OccupationTrigger.OnBreeding` - 繁殖時
- `OccupationTrigger.OnTurnEnd` - ターン終了時
- `OccupationTrigger.OnRoundStart` - ラウンド開始時
- `OccupationTrigger.OnTake` - アイテムを取得したとき ⭐ NEW
- `OccupationTrigger.OnReceive` - アイテムが手持ちに入ったとき ⭐ NEW
- `OccupationTrigger.Passive` - 継続効果

## TriggerableCard クラス

取得されるトリガー可能カードの情報：

```csharp
public class TriggerableCard
{
    public EnhancedCard card;        // カード本体
    public CardEffect effect;        // 該当する効果
    public Player owner;             // カードの所有者
    public bool canTrigger;          // トリガー可能かどうか
    public string triggerReason;     // トリガー可能/不可能な理由
}
```

## EventContext クラス

イベントのコンテキスト情報：

```csharp
public class EventContext
{
    public OccupationTrigger triggerType;  // トリガータイプ
    public Player currentPlayer;           // 現在のプレイヤー
    public ActionSpace actionSpace;        // 実行されたアクションスペース
    public ResourceType resourceType;      // 関連するリソースタイプ
    public int resourceAmount;             // リソースの量
    public object customData;              // カスタムデータ
}
```

## ゲームへの統合

### GameManager での使用

`GameManager` では以下のタイミングで自動的にカード効果がトリガーされます：

1. **収穫フェーズ**: `ExecuteHarvest()` 内で収穫時効果を実行
2. **繁殖フェーズ**: `ExecuteHarvest()` 内で繁殖時効果を実行
3. **アクション実行時**: `OnActionSpaceClicked()` 内でアクション時効果を実行
4. **ターン終了時**: `Player.EndTurn()` 内でターン終了時効果を実行

### 手動でのトリガー

特定のタイミングで手動でカード効果をトリガーしたい場合：

```csharp
// 特定のイベントでカード効果を実行
gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnHarvest, currentPlayer);

// または個別のカードを実行
CardTriggerManager.TriggerableCard triggerableCard = /* 取得したトリガー可能カード */;
cardTriggerManager.ExecuteTriggerableCard(triggerableCard, context);
```

## テスト

`CardTriggerTest` クラスを使用してシステムをテストできます：

1. シーンに `CardTriggerTest` コンポーネントを追加
2. インスペクターで `Run Test On Start` をチェック
3. ゲームを実行してコンソールでテスト結果を確認

または、インスペクターの右クリックメニューから手動でテストメソッドを実行できます。

## 注意事項

1. `CardTriggerManager` は `GameManager` によって自動的に作成・管理されます
2. カードの効果は `EnhancedCard` クラスを継承している必要があります
3. トリガー条件は各カードの `CheckTriggerCondition` メソッドで定義します
4. 使用回数制限は `CardEffect.CanActivate()` メソッドで管理されます

## 拡張

新しいトリガータイプを追加する場合：

1. `OccupationTrigger` enum に新しい値を追加
2. `CardTriggerManager.CanTriggerEffect()` に新しい条件判定を追加
3. `GameManager` の適切なタイミングで新しいトリガーを呼び出し

これにより、ゲーム中のさまざまなイベントに対応したカード効果システムを構築できます。

---

## 🆕 自動カード追加機能

### 概要

v2.0から、カードを出したとき（プレイ、職業カード獲得、進歩カード獲得）に自動的にトリガー可能なカードの一覧に追加される仕組みが実装されました。

### 自動追加のタイミング

1. **カードプレイ時**: `Player.PlayCard()` でカードをプレイした瞬間
2. **職業カード獲得時**: `Player.AddOccupation()` で職業カードを獲得した瞬間
3. **進歩カード獲得時**: `Player.AddImprovement()` で進歩カードを獲得した瞬間

### 自動実行される処理

#### 1. 即座トリガー効果の実行
```csharp
// Immediateトリガーの効果は自動的に実行される
if (effect.triggerType == OccupationTrigger.Immediate)
{
    ExecuteEffect(player, effect);
}
```

#### 2. 効果の詳細分析
```csharp
// 新しいカードの全効果を分析し、ログに出力
cardTriggerManager.AnalyzeNewCard(card, player);
```

#### 3. トリガー可能一覧への追加
- `playedCards`, `occupations`, `improvements` リストに追加されたカードは
- `CardTriggerManager.GetEnhancedCardsFromPlayer()` で自動的に検出される

### 使用例

```csharp
// カードをプレイするだけで自動的に処理される
player.PlayCard(someCard);
// → 即座効果が実行され、他の効果がトリガー可能一覧に追加される

// 職業カードを獲得するだけで自動的に処理される
player.AddOccupation(occupationCard);
// → カードの効果が分析され、トリガー可能一覧に追加される

// 進歩カードを獲得するだけで自動的に処理される
player.AddImprovement(improvementCard);
// → カードの効果が分析され、トリガー可能一覧に追加される
```

### デバッグ出力

自動追加機能により、以下のようなデバッグ情報が出力されます：

```
=== 新しいカード「農夫」の効果分析 ===
[利用可能] OnHarvest トリガー: 収穫時に穀物+1個獲得 (効果「収穫時に穀物+1個獲得」がトリガー可能)
[条件待ち] OnAction トリガー: 畑アクション時に追加効果 (トリガー条件を満たしていない)
プレイヤー1が「農夫」を獲得。トリガー可能カード一覧が更新されました。
```

### 利点

1. **自動管理**: 手動でトリガー可能一覧を管理する必要がない
2. **即座実行**: Immediateトリガー効果が自動的に実行される
3. **詳細分析**: 新しいカードの効果が自動的に分析される
4. **デバッグ支援**: 詳細なログでカードの状態を把握できる

### 技術的詳細

#### Player.RegisterCardEffectsToTriggerManager()
```csharp
private void RegisterCardEffectsToTriggerManager(EnhancedCard card, GameManager gameManager)
{
    CardTriggerManager triggerManager = gameManager.GetCardTriggerManager();
    if (triggerManager != null)
    {
        // 新しいカードの効果を詳細分析
        triggerManager.AnalyzeNewCard(card, this);
    }
}
```

#### CardTriggerManager.AnalyzeNewCard()
```csharp
public void AnalyzeNewCard(EnhancedCard card, Player owner)
{
    foreach (var effect in card.effects)
    {
        EventContext context = new EventContext(effect.triggerType, owner);
        bool canTrigger = CanTriggerEffect(card, effect, owner, context);
        string reason = GetTriggerReason(card, effect, owner, context, canTrigger);
        
        string status = canTrigger ? "[利用可能]" : "[条件待ち]";
        Debug.Log($"{status} {effect.triggerType}トリガー: {effect.effectDescription} ({reason})");
    }
}
```

この自動追加機能により、カードシステムがより使いやすく、デバッグしやすくなりました。

---

## 🆕 新しいトリガータイプ: OnTake と OnReceive

### 概要

v3.0から、リソース取得・受取に関する新しいトリガータイプが追加されました：

- **OnTake**: プレイヤーが能動的にアイテムを取得したとき
- **OnReceive**: プレイヤーの手持ちにアイテムが入ったとき（受動的）

### OnTakeトリガー

アクションスペースからの取得、カード効果での取得など、プレイヤーが能動的にリソースを獲得する際に発動します。

#### 使用例

```csharp
// アクションスペースからリソースを取得
player.TakeResourceFromAction(ResourceType.Wood, 2, actionSpace);
// → OnTakeトリガーが発動

// カード効果でリソースを取得
player.TakeResourceFromCardEffect(ResourceType.Clay, 1, "大工の効果");
// → OnTakeトリガーが発動
```

#### 条件設定

```csharp
// 特定のリソースタイプのみに反応
effect.triggerCondition = "Wood"; // 木材取得時のみ

// 特定の取得方法のみに反応
effect.specialEffectData = "take_method:action"; // アクションでの取得のみ
```

### OnReceiveトリガー

リソースが手持ちに入る全ての場合に発動します（OnTakeも含む）。

#### 使用例

```csharp
// 直接リソースを受け取る
player.ReceiveResourceDirect(ResourceType.Food, 3, sourcePlayer, "trade");
// → OnReceiveトリガーが発動

// 通常のAddResourceでも発動
player.AddResource(ResourceType.Grain, 2);
// → OnReceiveトリガーが発動
```

#### 条件設定

```csharp
// 最小受取量の条件
effect.specialEffectData = "min_amount:3"; // 3個以上受け取った場合のみ

// 特定のリソースタイプのみに反応
effect.triggerCondition = "Food"; // 食料受取時のみ
```

### 実装例

#### 商人カード（OnTakeトリガー）

```csharp
var traderCard = EnhancedCardFactory.CreateTakeTriggeredOccupation(
    "商人", "OCC_TRADER", OccupationType.Trader, ResourceType.Wood);
// 建材取得時に食料1個獲得
```

#### 学者カード（OnReceiveトリガー）

```csharp
var scholarCard = EnhancedCardFactory.CreateReceiveTriggeredOccupation(
    "学者", "OCC_SCHOLAR", OccupationType.Scholar, 3);
// 3個以上のリソース受取時に勝利点1点獲得
```

### イベントコンテキスト

#### TakeEventContext

```csharp
public class TakeEventContext : EventContext
{
    public ResourceType resourceType;   // 取得したリソースの種類
    public int amount;                  // 取得した量
    public ActionSpace sourceActionSpace; // 取得元のアクションスペース
    public string takeMethod;           // 取得方法 ("action", "card_effect", etc.)
}
```

#### ReceiveEventContext

```csharp
public class ReceiveEventContext : EventContext
{
    public ResourceType resourceType;   // 受け取ったリソースの種類
    public int amount;                  // 受け取った量
    public Player sourcePlayer;         // リソースの提供者
    public string receiveMethod;        // 受取方法 ("direct", "trade", etc.)
}
```

### 使い分けのガイドライン

#### OnTakeを使用する場合
- アクション実行時の追加効果
- 特定の取得方法に反応する効果
- 能動的な取得行動に対するボーナス

#### OnReceiveを使用する場合
- リソース蓄積に関する効果
- 大量受取時のボーナス
- 受動的な受取も含めた全般的な効果

### Take→Receiveの流れ制御

アクションスペースからの取得やカード効果での取得では、以下の順序で処理されます：

```csharp
// アクションスペースからの取得
player.TakeResourceFromAction(ResourceType.Wood, 2, actionSpace);
// 1. OnTakeトリガー発動
// 2. リソースを実際に追加
// 3. OnReceiveトリガー発動

// カード効果での取得
player.TakeResourceFromCardEffect(ResourceType.Clay, 1, "カード名");
// 1. OnTakeトリガー発動
// 2. リソースを実際に追加
// 3. OnReceiveトリガー発動
```

### 無限ループ防止

トリガー効果によってリソースが追加される際の無限ループを防ぐため、以下の仕組みが実装されています：

- **トリガー実行中フラグ**: 同じトリガータイプが実行中は再発動を防止
- **適切なメソッド選択**: カード効果では`TakeResourceFromCardEffect`を使用
- **トリガー制御**: 内部的にトリガー発動を制御可能

### 注意事項

1. **Take→Receiveの順序**: アクションやカード効果での取得では、OnTakeが先に発動し、その後OnReceiveが発動します
2. **無限ループ防止**: 同じトリガータイプが実行中の場合、再発動は自動的に防止されます
3. **条件設定**: `triggerCondition`と`specialEffectData`で詳細な条件を設定可能
4. **使用回数制限**: `maxUsesPerRound`で1ラウンドあたりの発動回数を制限可能
5. **適切なメソッド選択**: 
   - アクション実行: `TakeResourceFromAction()`
   - カード効果: `TakeResourceFromCardEffect()`
   - 直接受取: `ReceiveResourceDirect()`
   - 通常追加: `AddResource()`（OnReceiveのみ発動）

これらの新しいトリガータイプにより、より細かなリソース管理とカード効果の実装が可能になりました。