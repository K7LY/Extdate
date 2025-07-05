# ActionSpace改善版API設計

## 🎯 設計方針

**内部的には処理を分離、外部的にはActionSpaceのメソッドでアクセス**

- 内部：累積アイテム管理システム（`AccumulatedItemManager`）を使用
- 外部：`ActionSpace`のAPIを通じて累積アイテムを管理
- 目的：「このアクションスペースには何が累積されているか」を簡単に把握

## 🔧 改善されたActionSpaceクラス

```csharp
public class ActionSpace : MonoBehaviour
{
    [Header("アクション情報")]
    public string actionId;        // 一意のID（累積アイテム管理で使用）
    public string actionName;
    public ActionType actionType;
    public bool allowMultipleWorkers = false;
    public int maxWorkers = 1;
    
    [Header("固有効果")]
    public List<ActionEffect> coreEffects = new List<ActionEffect>();
    public List<ResourceRequirement> resourceRequirements = new List<ResourceRequirement>();
    public int cardsToDraw = 0;
    public int victoryPoints = 0;
    
    // 累積アイテム管理（内部使用）
    private AccumulatedItemManager accumulatedItemManager;
    
    void Start()
    {
        // 一意のIDを自動生成（設定されていない場合）
        if (string.IsNullOrEmpty(actionId))
        {
            actionId = $"{actionName}_{GetInstanceID()}";
        }
        
        accumulatedItemManager = FindObjectOfType<AccumulatedItemManager>();
    }
    
    // ==================== 累積アイテム管理API ====================
    
    /// <summary>
    /// 累積アイテムを取得（消費しない）
    /// </summary>
    public Dictionary<ResourceType, int> GetAccumulatedItems()
    {
        return accumulatedItemManager?.GetAccumulatedItems(actionId) ?? new Dictionary<ResourceType, int>();
    }
    
    /// <summary>
    /// 特定のリソースの累積量を取得
    /// </summary>
    public int GetAccumulatedAmount(ResourceType resourceType)
    {
        var items = GetAccumulatedItems();
        return items.ContainsKey(resourceType) ? items[resourceType] : 0;
    }
    
    /// <summary>
    /// 累積アイテムを追加
    /// </summary>
    public void AddAccumulatedItem(ResourceType resourceType, int amount, string sourceId = "")
    {
        accumulatedItemManager?.AddAccumulatedItem(actionId, resourceType, amount, sourceId);
    }
    
    /// <summary>
    /// 累積アイテムを消費（取得と同時にリセット）
    /// </summary>
    public Dictionary<ResourceType, int> ConsumeAccumulatedItems()
    {
        return accumulatedItemManager?.ConsumeAccumulatedItems(actionId) ?? new Dictionary<ResourceType, int>();
    }
    
    /// <summary>
    /// 累積アイテムが存在するかチェック
    /// </summary>
    public bool HasAccumulatedItems()
    {
        return GetAccumulatedItems().Count > 0;
    }
    
    /// <summary>
    /// 累積アイテムの合計数を取得
    /// </summary>
    public int GetAccumulatedItemCount()
    {
        return GetAccumulatedItems().Values.Sum();
    }
    
    /// <summary>
    /// 累積アイテムをクリア
    /// </summary>
    public void ClearAccumulatedItems()
    {
        accumulatedItemManager?.ClearAccumulatedItems(actionId);
    }
    
    /// <summary>
    /// 累積アイテムの履歴を取得
    /// </summary>
    public List<AccumulatedItemEntry> GetAccumulatedItemHistory()
    {
        return accumulatedItemManager?.GetAccumulatedItemHistory(actionId) ?? new List<AccumulatedItemEntry>();
    }
    
    // ==================== ワーカー配置処理 ====================
    
    public bool PlaceWorker(Worker worker)
    {
        if (!CanPlaceWorker())
            return false;
            
        placedWorkers.Add(worker);
        worker.SetActionSpace(this);
        worker.transform.position = GetWorkerPosition(placedWorkers.Count - 1);
        
        // 1. 固有効果を実行
        ExecuteCoreAction(worker.owner);
        
        // 2. 累積アイテムを取得
        var accumulatedItems = ConsumeAccumulatedItems();
        foreach (var item in accumulatedItems)
        {
            worker.owner.AddResource(item.Key, item.Value);
            Debug.Log($"{worker.owner.playerName}が{GetResourceJapaneseName(item.Key)}を{item.Value}個獲得しました（累積）");
        }
        
        // 3. 職業効果のトリガー
        worker.owner.OnActionExecuted(this);
        
        UpdateVisual();
        OnWorkerPlaced?.Invoke(worker);
        
        return true;
    }
    
    private void ExecuteCoreAction(Player player)
    {
        // 固有効果のみを実行（累積アイテムは除く）
        switch (actionType)
        {
            case ActionType.AddField:
                ExecuteAddField(player);
                break;
            case ActionType.SowField:
                ExecuteSowField(player);
                break;
            case ActionType.FamilyGrowth:
                ExecuteFamilyGrowth(player);
                break;
            case ActionType.HouseExpansion:
                ExecuteHouseExpansion(player);
                break;
            case ActionType.HouseRenovation:
                ExecuteHouseRenovation(player);
                break;
            case ActionType.BuildFences:
                ExecuteBuildFences(player);
                break;
            case ActionType.BuildStables:
                ExecuteBuildStables(player);
                break;
            case ActionType.PlayOccupation:
                ExecutePlayOccupation(player);
                break;
            case ActionType.PlayImprovement:
                ExecutePlayImprovement(player);
                break;
            case ActionType.StartingPlayer:
                ExecuteStartingPlayer(player);
                break;
            case ActionType.TradeResources:
                ExecuteTradeResources(player);
                break;
            case ActionType.SpecialAction:
                ExecuteSpecialAction(player);
                break;
        }
        
        // 勝利点があれば追加
        if (victoryPoints > 0)
        {
            player.AddVictoryPoints(victoryPoints);
        }
    }
    
    // ==================== 便利メソッド ====================
    
    /// <summary>
    /// このアクションスペースの全リソース（固有効果＋累積アイテム）を取得
    /// </summary>
    public Dictionary<ResourceType, int> GetAllAvailableResources()
    {
        var result = new Dictionary<ResourceType, int>();
        
        // 累積アイテムを追加
        var accumulatedItems = GetAccumulatedItems();
        foreach (var item in accumulatedItems)
        {
            result[item.Key] = item.Value;
        }
        
        return result;
    }
    
    /// <summary>
    /// デバッグ用：アクションスペースの状態を表示
    /// </summary>
    [ContextMenu("Show Action Space Status")]
    public void ShowActionSpaceStatus()
    {
        Debug.Log($"=== {actionName} ({actionId}) ===");
        Debug.Log($"アクションタイプ: {actionType}");
        Debug.Log($"配置されたワーカー: {placedWorkers.Count}/{maxWorkers}");
        
        var accumulatedItems = GetAccumulatedItems();
        if (accumulatedItems.Count > 0)
        {
            Debug.Log("累積アイテム:");
            foreach (var item in accumulatedItems)
            {
                Debug.Log($"  {GetResourceJapaneseName(item.Key)}: {item.Value}個");
            }
        }
        else
        {
            Debug.Log("累積アイテム: なし");
        }
        
        var history = GetAccumulatedItemHistory();
        if (history.Count > 0)
        {
            Debug.Log($"累積履歴: {history.Count}件");
        }
    }
    
    // ==================== 従来の処理（削除予定） ====================
    
    // 既存のresourceGainとresourceRewardsは段階的に削除
    // 一時的に互換性のために残す
    [System.Obsolete("Use accumulated item system instead")]
    public Dictionary<ResourceType, int> resourceGain = new Dictionary<ResourceType, int>();
    
    [System.Obsolete("Use accumulated item system instead")]
    public List<ResourceReward> resourceRewards = new List<ResourceReward>();
}
```

## 🎮 使用例

### 1. 累積アイテムの確認
```csharp
// 森アクションスペースの累積アイテムを確認
var forestAction = GetActionSpace("forest");
var accumulatedItems = forestAction.GetAccumulatedItems();

Debug.Log($"森の累積アイテム: {accumulatedItems.Count}種類");
foreach (var item in accumulatedItems)
{
    Debug.Log($"  {item.Key}: {item.Value}個");
}

// 特定のリソースの累積量を確認
int woodAmount = forestAction.GetAccumulatedAmount(ResourceType.Wood);
Debug.Log($"累積木材: {woodAmount}個");
```

### 2. カード効果での累積アイテム追加
```csharp
// 「森の恵み」カード効果：森アクションスペースに食料2個を追加
var forestAction = GetActionSpace("forest");
forestAction.AddAccumulatedItem(ResourceType.Food, 2, "forest_blessing_card");

// 「豊作の季節」カード効果：すべての農場アクションスペースに穀物1個を追加
var farmActions = GetActionSpacesByType(ActionType.SowField);
foreach (var action in farmActions)
{
    action.AddAccumulatedItem(ResourceType.Grain, 1, "harvest_season_card");
}
```

### 3. 標準的な累積ルール
```csharp
// ActionSpaceManagerでの標準的な累積ルール
public void ReplenishActionSpaces()
{
    // 各アクションスペースに標準的な累積を追加
    GetActionSpace("forest").AddAccumulatedItem(ResourceType.Wood, 3, "standard_rule");
    GetActionSpace("clay_pit").AddAccumulatedItem(ResourceType.Clay, 1, "standard_rule");
    GetActionSpace("reed_pond").AddAccumulatedItem(ResourceType.Reed, 1, "standard_rule");
    GetActionSpace("fishing").AddAccumulatedItem(ResourceType.Food, 1, "standard_rule");
    GetActionSpace("sheep_market").AddAccumulatedItem(ResourceType.Sheep, 1, "standard_rule");
}
```

### 4. UI表示での活用
```csharp
// UI更新時に累積アイテムを表示
public void UpdateActionSpaceUI(ActionSpace actionSpace)
{
    var accumulatedItems = actionSpace.GetAccumulatedItems();
    
    // 累積アイテムが存在する場合のみバッジを表示
    if (actionSpace.HasAccumulatedItems())
    {
        accumulatedItemBadge.SetActive(true);
        accumulatedItemText.text = actionSpace.GetAccumulatedItemCount().ToString();
    }
    else
    {
        accumulatedItemBadge.SetActive(false);
    }
}
```

## 🔧 追加するAccumulatedItemManager

```csharp
public class AccumulatedItemManager : MonoBehaviour
{
    private Dictionary<string, AccumulatedItemPool> accumulatedItems = new Dictionary<string, AccumulatedItemPool>();
    
    public void AddAccumulatedItem(string actionSpaceId, ResourceType resourceType, int amount, string sourceId = "")
    {
        if (!accumulatedItems.ContainsKey(actionSpaceId))
        {
            accumulatedItems[actionSpaceId] = new AccumulatedItemPool();
        }
        
        accumulatedItems[actionSpaceId].AddItem(resourceType, amount, sourceId);
    }
    
    public Dictionary<ResourceType, int> GetAccumulatedItems(string actionSpaceId)
    {
        if (accumulatedItems.ContainsKey(actionSpaceId))
        {
            return accumulatedItems[actionSpaceId].GetAllItems();
        }
        return new Dictionary<ResourceType, int>();
    }
    
    public Dictionary<ResourceType, int> ConsumeAccumulatedItems(string actionSpaceId)
    {
        if (accumulatedItems.ContainsKey(actionSpaceId))
        {
            var items = accumulatedItems[actionSpaceId].GetAllItems();
            accumulatedItems[actionSpaceId].Clear();
            return items;
        }
        return new Dictionary<ResourceType, int>();
    }
    
    public void ClearAccumulatedItems(string actionSpaceId)
    {
        if (accumulatedItems.ContainsKey(actionSpaceId))
        {
            accumulatedItems[actionSpaceId].Clear();
        }
    }
    
    public List<AccumulatedItemEntry> GetAccumulatedItemHistory(string actionSpaceId)
    {
        if (accumulatedItems.ContainsKey(actionSpaceId))
        {
            return accumulatedItems[actionSpaceId].GetHistory();
        }
        return new List<AccumulatedItemEntry>();
    }
}
```

## 🚀 メリット

### 1. 使いやすさ
- `actionSpace.GetAccumulatedItems()`で簡単に累積アイテムを取得
- `actionSpace.GetAccumulatedAmount(ResourceType.Wood)`で特定のリソース量を取得
- APIが直感的で覚えやすい

### 2. 内部と外部の分離
- 内部的には`AccumulatedItemManager`で管理
- 外部からは`ActionSpace`のAPIを使用
- 実装の変更が外部に影響しない

### 3. 段階的な移行
- 既存の`resourceGain`システムと並行して使用可能
- 段階的にリファクタリング可能
- 後方互換性を保持

### 4. 柔軟性
- カード効果で任意のアイテムを累積可能
- 履歴管理でデバッグが容易
- 拡張性が高い

## 📝 結論

この設計により、**内部的には処理を分離**しつつ、**外部的にはActionSpaceのメソッドで簡単にアクセス**できるシステムが実現されます。

```csharp
// 使用例：シンプルで直感的
var forestAction = GetActionSpace("forest");
int woodAmount = forestAction.GetAccumulatedAmount(ResourceType.Wood);
forestAction.AddAccumulatedItem(ResourceType.Food, 2, "card_effect");
```

これにより、「アクションスペース上のアイテム量を把握したい」という要求を満たしつつ、柔軟で拡張性の高いシステムを構築できます。