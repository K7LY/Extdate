using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 個別のアクションスペースの累積アイテムプール
/// 特定のアクションスペースに累積されるアイテムを管理
/// </summary>
[System.Serializable]
public class AccumulatedItemPool
{
    [Header("プール情報")]
    public string actionSpaceId;
    
    [Header("累積アイテム")]
    [SerializeField] private List<ResourceEntry> resourceEntries = new List<ResourceEntry>();
    
    [Header("履歴管理")]
    [SerializeField] private List<AccumulationEntry> history = new List<AccumulationEntry>();
    
    [Header("設定")]
    [SerializeField] private int maxHistoryEntries = 100;
    
    // 内部管理用Dictionary（高速アクセス）
    private Dictionary<ResourceType, int> items = new Dictionary<ResourceType, int>();
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースの一意ID</param>
    public AccumulatedItemPool(string actionSpaceId)
    {
        this.actionSpaceId = actionSpaceId;
        items = new Dictionary<ResourceType, int>();
        history = new List<AccumulationEntry>();
        resourceEntries = new List<ResourceEntry>();
    }
    
    /// <summary>
    /// アイテムを追加
    /// </summary>
    /// <param name="resourceType">リソース種別</param>
    /// <param name="amount">追加量</param>
    /// <param name="sourceId">追加元ID</param>
    public void AddItem(ResourceType resourceType, int amount, string sourceId = "")
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Invalid amount: {amount} for resource: {resourceType}");
            return;
        }
        
        // Dictionaryに追加
        if (!items.ContainsKey(resourceType))
        {
            items[resourceType] = 0;
        }
        items[resourceType] += amount;
        
        // SerializableなEntryも更新
        UpdateResourceEntries();
        
        // 履歴に記録
        AddToHistory(new AccumulationEntry
        {
            resourceType = resourceType,
            amount = amount,
            sourceId = sourceId,
            timestamp = System.DateTime.Now,
            actionType = AccumulationAction.Add
        });
        
        Debug.Log($"[AccumulatedItemPool] {actionSpaceId}: 追加 {resourceType} x{amount} (from: {sourceId})");
    }
    
    /// <summary>
    /// 全アイテムを消費（取得と同時にクリア）
    /// </summary>
    /// <returns>消費されたアイテム</returns>
    public Dictionary<ResourceType, int> ConsumeAll()
    {
        var result = new Dictionary<ResourceType, int>(items);
        
        // 消費履歴を記録
        foreach (var item in result)
        {
            AddToHistory(new AccumulationEntry
            {
                resourceType = item.Key,
                amount = -item.Value,
                sourceId = "consumed",
                timestamp = System.DateTime.Now,
                actionType = AccumulationAction.Consume
            });
        }
        
        // クリア
        items.Clear();
        UpdateResourceEntries();
        
        if (result.Count > 0)
        {
            Debug.Log($"[AccumulatedItemPool] {actionSpaceId}: 消費 {GetItemsSummary(result)}");
        }
        
        return result;
    }
    
    /// <summary>
    /// 特定のリソースを消費
    /// </summary>
    /// <param name="resourceType">消費するリソース種別</param>
    /// <param name="amount">消費量</param>
    /// <returns>実際に消費された量</returns>
    public int ConsumeResource(ResourceType resourceType, int amount)
    {
        if (!items.ContainsKey(resourceType) || amount <= 0)
        {
            return 0;
        }
        
        int actualAmount = Mathf.Min(items[resourceType], amount);
        items[resourceType] -= actualAmount;
        
        if (items[resourceType] <= 0)
        {
            items.Remove(resourceType);
        }
        
        UpdateResourceEntries();
        
        // 履歴に記録
        AddToHistory(new AccumulationEntry
        {
            resourceType = resourceType,
            amount = -actualAmount,
            sourceId = "partial_consume",
            timestamp = System.DateTime.Now,
            actionType = AccumulationAction.Consume
        });
        
        Debug.Log($"[AccumulatedItemPool] {actionSpaceId}: 部分消費 {resourceType} x{actualAmount}");
        
        return actualAmount;
    }
    
    /// <summary>
    /// 現在のアイテムを取得（消費しない）
    /// </summary>
    /// <returns>現在の累積アイテム</returns>
    public Dictionary<ResourceType, int> GetItems()
    {
        return new Dictionary<ResourceType, int>(items);
    }
    
    /// <summary>
    /// 特定のリソースの累積量を取得
    /// </summary>
    /// <param name="resourceType">リソース種別</param>
    /// <returns>累積量</returns>
    public int GetResourceAmount(ResourceType resourceType)
    {
        return items.ContainsKey(resourceType) ? items[resourceType] : 0;
    }
    
    /// <summary>
    /// 累積アイテムが存在するかチェック
    /// </summary>
    /// <returns>アイテムが存在する場合true</returns>
    public bool HasItems()
    {
        return items.Count > 0;
    }
    
    /// <summary>
    /// 累積アイテムの総数を取得
    /// </summary>
    /// <returns>全アイテムの合計数</returns>
    public int GetTotalItemCount()
    {
        return items.Values.Sum();
    }
    
    /// <summary>
    /// プールをクリア
    /// </summary>
    public void Clear()
    {
        if (items.Count > 0)
        {
            // クリア履歴を記録
            AddToHistory(new AccumulationEntry
            {
                resourceType = ResourceType.Wood, // ダミー
                amount = 0,
                sourceId = "clear_all",
                timestamp = System.DateTime.Now,
                actionType = AccumulationAction.Clear
            });
            
            Debug.Log($"[AccumulatedItemPool] {actionSpaceId}: 全クリア");
        }
        
        items.Clear();
        UpdateResourceEntries();
    }
    
    /// <summary>
    /// 履歴を取得
    /// </summary>
    /// <returns>累積履歴のリスト</returns>
    public List<AccumulationEntry> GetHistory()
    {
        return new List<AccumulationEntry>(history);
    }
    
    /// <summary>
    /// 最新の履歴エントリを取得
    /// </summary>
    /// <param name="count">取得する履歴の数</param>
    /// <returns>最新の履歴エントリ</returns>
    public List<AccumulationEntry> GetRecentHistory(int count = 10)
    {
        return history.OrderByDescending(h => h.timestamp).Take(count).ToList();
    }
    
    /// <summary>
    /// 特定のリソースの履歴を取得
    /// </summary>
    /// <param name="resourceType">リソース種別</param>
    /// <returns>該当リソースの履歴</returns>
    public List<AccumulationEntry> GetResourceHistory(ResourceType resourceType)
    {
        return history.Where(h => h.resourceType == resourceType).ToList();
    }
    
    /// <summary>
    /// 履歴をクリア
    /// </summary>
    public void ClearHistory()
    {
        history.Clear();
        Debug.Log($"[AccumulatedItemPool] {actionSpaceId}: 履歴クリア");
    }
    
    /// <summary>
    /// プールの状態を文字列で取得
    /// </summary>
    /// <returns>プールの状態文字列</returns>
    public string GetStatusString()
    {
        if (items.Count == 0)
        {
            return "空";
        }
        
        return GetItemsSummary(items);
    }
    
    /// <summary>
    /// デバッグ用：プールの詳細情報を表示
    /// </summary>
    public void ShowDetailedStatus()
    {
        Debug.Log($"=== AccumulatedItemPool Status: {actionSpaceId} ===");
        Debug.Log($"累積アイテム: {GetStatusString()}");
        Debug.Log($"総アイテム数: {GetTotalItemCount()}");
        Debug.Log($"履歴エントリ数: {history.Count}");
        
        // 最新の履歴を表示
        var recentHistory = GetRecentHistory(5);
        if (recentHistory.Count > 0)
        {
            Debug.Log("最新の履歴:");
            foreach (var entry in recentHistory)
            {
                Debug.Log($"  {entry.timestamp:HH:mm:ss} - {entry.actionType}: {entry.resourceType} x{entry.amount} (from: {entry.sourceId})");
            }
        }
    }
    
    // プライベートメソッド
    private void UpdateResourceEntries()
    {
        resourceEntries.Clear();
        
        foreach (var item in items)
        {
            resourceEntries.Add(new ResourceEntry
            {
                resourceType = item.Key,
                amount = item.Value
            });
        }
    }
    
    private void AddToHistory(AccumulationEntry entry)
    {
        history.Add(entry);
        
        // 履歴が上限を超えた場合、古いものを削除
        while (history.Count > maxHistoryEntries)
        {
            history.RemoveAt(0);
        }
    }
    
    private string GetItemsSummary(Dictionary<ResourceType, int> items)
    {
        var summary = items.Select(item => $"{GetResourceJapaneseName(item.Key)} x{item.Value}");
        return string.Join(", ", summary);
    }
    
    private string GetResourceJapaneseName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood: return "木材";
            case ResourceType.Clay: return "土";
            case ResourceType.Reed: return "葦";
            case ResourceType.Stone: return "石";
            case ResourceType.Grain: return "穀物";
            case ResourceType.Vegetable: return "野菜";
            case ResourceType.Sheep: return "羊";
            case ResourceType.Boar: return "猪";
            case ResourceType.Cattle: return "牛";
            case ResourceType.Food: return "食料";
            default: return resourceType.ToString();
        }
    }
}

/// <summary>
/// Serializable用のリソースエントリ
/// </summary>
[System.Serializable]
public class ResourceEntry
{
    public ResourceType resourceType;
    public int amount;
}