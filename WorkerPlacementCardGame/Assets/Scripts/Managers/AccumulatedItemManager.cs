using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// アクションスペースの累積アイテムを管理するシステム
/// アクションスペースの固有効果から完全に分離された累積物管理
/// </summary>
public class AccumulatedItemManager : MonoBehaviour
{
    [Header("累積ルール設定")]
    [SerializeField] private List<AccumulationRule> standardRules = new List<AccumulationRule>();
    
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // アクションスペースID別の累積プール
    private Dictionary<string, AccumulatedItemPool> accumulatedPools = new Dictionary<string, AccumulatedItemPool>();
    
    // イベント
    public System.Action<string, ResourceType, int> OnItemAccumulated;
    public System.Action<string, Dictionary<ResourceType, int>> OnItemsConsumed;
    public System.Action<string> OnPoolCleared;
    
    void Start()
    {
        InitializeStandardRules();
        DebugLog("AccumulatedItemManager initialized");
    }
    
    /// <summary>
    /// 標準的な累積ルールを初期化
    /// </summary>
    private void InitializeStandardRules()
    {
        standardRules.Clear();
        
        // 標準的な累積ルール（Agricola準拠）
        standardRules.Add(new AccumulationRule("forest", ResourceType.Wood, 3, "森：木材3個/ラウンド"));
        standardRules.Add(new AccumulationRule("clay_pit", ResourceType.Clay, 1, "土取り場：土1個/ラウンド"));
        standardRules.Add(new AccumulationRule("reed_pond", ResourceType.Reed, 1, "葦の沼：葦1個/ラウンド"));
        standardRules.Add(new AccumulationRule("fishing", ResourceType.Food, 1, "漁場：食料1個/ラウンド"));
        standardRules.Add(new AccumulationRule("sheep_market", ResourceType.Sheep, 1, "羊市場：羊1匹/ラウンド"));
        standardRules.Add(new AccumulationRule("boar_market", ResourceType.Boar, 1, "猪市場：猪1匹/ラウンド"));
        standardRules.Add(new AccumulationRule("cattle_market", ResourceType.Cattle, 1, "牛市場：牛1匹/ラウンド"));
        
        DebugLog($"Standard rules initialized: {standardRules.Count} rules");
    }
    
    /// <summary>
    /// 累積アイテムを追加
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースの一意ID</param>
    /// <param name="resourceType">累積するリソース種別</param>
    /// <param name="amount">累積量</param>
    /// <param name="sourceId">累積の原因（標準ルール、カード効果など）</param>
    public void AddAccumulatedItem(string actionSpaceId, ResourceType resourceType, int amount, string sourceId = "")
    {
        if (string.IsNullOrEmpty(actionSpaceId) || amount <= 0)
        {
            DebugLog($"Invalid parameters: actionSpaceId={actionSpaceId}, amount={amount}");
            return;
        }
        
        // プールが存在しない場合は作成
        if (!accumulatedPools.ContainsKey(actionSpaceId))
        {
            accumulatedPools[actionSpaceId] = new AccumulatedItemPool(actionSpaceId);
        }
        
        // アイテムを追加
        accumulatedPools[actionSpaceId].AddItem(resourceType, amount, sourceId);
        
        // イベント発火
        OnItemAccumulated?.Invoke(actionSpaceId, resourceType, amount);
        
        DebugLog($"累積追加: {actionSpaceId} に {GetResourceJapaneseName(resourceType)} x{amount} (from: {sourceId})");
    }
    
    /// <summary>
    /// 累積アイテムを取得（消費）
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースの一意ID</param>
    /// <returns>累積されていたアイテム（消費される）</returns>
    public Dictionary<ResourceType, int> ConsumeAccumulatedItems(string actionSpaceId)
    {
        if (string.IsNullOrEmpty(actionSpaceId))
        {
            return new Dictionary<ResourceType, int>();
        }
        
        if (accumulatedPools.ContainsKey(actionSpaceId))
        {
            var items = accumulatedPools[actionSpaceId].ConsumeAll();
            
            // イベント発火
            OnItemsConsumed?.Invoke(actionSpaceId, items);
            
            if (items.Count > 0)
            {
                DebugLog($"累積消費: {actionSpaceId} から {GetItemsSummary(items)}");
            }
            
            return items;
        }
        
        return new Dictionary<ResourceType, int>();
    }
    
    /// <summary>
    /// 累積アイテムを確認（消費しない）
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースの一意ID</param>
    /// <returns>現在累積されているアイテム</returns>
    public Dictionary<ResourceType, int> GetAccumulatedItems(string actionSpaceId)
    {
        if (string.IsNullOrEmpty(actionSpaceId))
        {
            return new Dictionary<ResourceType, int>();
        }
        
        return accumulatedPools.ContainsKey(actionSpaceId) ? 
               accumulatedPools[actionSpaceId].GetItems() : 
               new Dictionary<ResourceType, int>();
    }
    
    /// <summary>
    /// 標準的な累積ルールを適用
    /// </summary>
    public void ApplyStandardAccumulation()
    {
        int appliedRules = 0;
        
        foreach (var rule in standardRules)
        {
            if (rule.isActive)
            {
                AddAccumulatedItem(rule.actionSpaceId, rule.resourceType, rule.amount, "standard_rule");
                appliedRules++;
            }
        }
        
        DebugLog($"Standard accumulation applied: {appliedRules} rules");
    }
    
    /// <summary>
    /// 特定のアクションスペースの累積をクリア
    /// </summary>
    /// <param name="actionSpaceId">クリアするアクションスペースID</param>
    public void ClearAccumulatedItems(string actionSpaceId)
    {
        if (string.IsNullOrEmpty(actionSpaceId))
        {
            return;
        }
        
        if (accumulatedPools.ContainsKey(actionSpaceId))
        {
            accumulatedPools[actionSpaceId].Clear();
            OnPoolCleared?.Invoke(actionSpaceId);
            DebugLog($"累積クリア: {actionSpaceId}");
        }
    }
    
    /// <summary>
    /// 全ての累積アイテムをクリア
    /// </summary>
    public void ClearAllAccumulatedItems()
    {
        int clearedPools = accumulatedPools.Count;
        accumulatedPools.Clear();
        
        DebugLog($"All accumulation cleared: {clearedPools} pools");
    }
    
    /// <summary>
    /// 累積履歴を取得
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースの一意ID</param>
    /// <returns>累積履歴のリスト</returns>
    public List<AccumulationEntry> GetAccumulationHistory(string actionSpaceId)
    {
        if (string.IsNullOrEmpty(actionSpaceId))
        {
            return new List<AccumulationEntry>();
        }
        
        return accumulatedPools.ContainsKey(actionSpaceId) ? 
               accumulatedPools[actionSpaceId].GetHistory() : 
               new List<AccumulationEntry>();
    }
    
    /// <summary>
    /// 累積ルールの動的な追加
    /// </summary>
    /// <param name="rule">追加する累積ルール</param>
    public void AddAccumulationRule(AccumulationRule rule)
    {
        if (rule != null && !standardRules.Any(r => r.actionSpaceId == rule.actionSpaceId && r.resourceType == rule.resourceType))
        {
            standardRules.Add(rule);
            DebugLog($"累積ルール追加: {rule.description}");
        }
    }
    
    /// <summary>
    /// 累積ルールの無効化
    /// </summary>
    /// <param name="actionSpaceId">対象のアクションスペースID</param>
    /// <param name="resourceType">対象のリソース種別</param>
    public void DeactivateAccumulationRule(string actionSpaceId, ResourceType resourceType)
    {
        var rule = standardRules.FirstOrDefault(r => r.actionSpaceId == actionSpaceId && r.resourceType == resourceType);
        if (rule != null)
        {
            rule.isActive = false;
            DebugLog($"累積ルール無効化: {rule.description}");
        }
    }
    
    /// <summary>
    /// 全ての累積プールの状態を取得
    /// </summary>
    /// <returns>累積プールの状態辞書</returns>
    public Dictionary<string, Dictionary<ResourceType, int>> GetAllAccumulationStatus()
    {
        var result = new Dictionary<string, Dictionary<ResourceType, int>>();
        
        foreach (var pool in accumulatedPools)
        {
            result[pool.Key] = pool.Value.GetItems();
        }
        
        return result;
    }
    
    /// <summary>
    /// デバッグ用：累積システムの状態を表示
    /// </summary>
    [ContextMenu("Show Accumulation Status")]
    public void ShowAccumulationStatus()
    {
        Debug.Log("=== 累積システム状態 ===");
        Debug.Log($"プール数: {accumulatedPools.Count}");
        Debug.Log($"アクティブルール数: {standardRules.Count(r => r.isActive)}");
        
        foreach (var pool in accumulatedPools)
        {
            var items = pool.Value.GetItems();
            if (items.Count > 0)
            {
                Debug.Log($"📦 {pool.Key}: {GetItemsSummary(items)}");
            }
        }
    }
    
    // ユーティリティメソッド
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
    
    private string GetItemsSummary(Dictionary<ResourceType, int> items)
    {
        var summary = items.Select(item => $"{GetResourceJapaneseName(item.Key)} x{item.Value}");
        return string.Join(", ", summary);
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[AccumulatedItemManager] {message}");
        }
    }
}