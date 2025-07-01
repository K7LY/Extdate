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
```

## 利用可能なトリガータイプ

- `OccupationTrigger.Immediate` - 即座（カードプレイ時）
- `OccupationTrigger.OnAction` - アクション実行時
- `OccupationTrigger.OnHarvest` - 収穫時
- `OccupationTrigger.OnBreeding` - 繁殖時
- `OccupationTrigger.OnTurnEnd` - ターン終了時
- `OccupationTrigger.OnRoundStart` - ラウンド開始時
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