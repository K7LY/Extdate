using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 累積ルールの定義
/// どのアクションスペースに、どのリソースを、どれだけ累積するかを定義
/// </summary>
[System.Serializable]
public class AccumulationRule
{
    [Header("基本設定")]
    public string actionSpaceId;
    public ResourceType resourceType;
    public int amount;
    
    [Header("状態")]
    public bool isActive = true;
    
    [Header("説明")]
    public string description;
    
    [Header("条件設定")]
    public bool hasCondition = false;
    public string conditionDescription;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public AccumulationRule()
    {
        isActive = true;
        hasCondition = false;
    }
    
    /// <summary>
    /// コンストラクタ（基本パラメータ）
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースID</param>
    /// <param name="resourceType">リソース種別</param>
    /// <param name="amount">累積量</param>
    public AccumulationRule(string actionSpaceId, ResourceType resourceType, int amount)
    {
        this.actionSpaceId = actionSpaceId;
        this.resourceType = resourceType;
        this.amount = amount;
        this.isActive = true;
        this.hasCondition = false;
        this.description = $"{actionSpaceId}: {resourceType} x{amount}";
    }
    
    /// <summary>
    /// コンストラクタ（説明付き）
    /// </summary>
    /// <param name="actionSpaceId">アクションスペースID</param>
    /// <param name="resourceType">リソース種別</param>
    /// <param name="amount">累積量</param>
    /// <param name="description">説明</param>
    public AccumulationRule(string actionSpaceId, ResourceType resourceType, int amount, string description)
    {
        this.actionSpaceId = actionSpaceId;
        this.resourceType = resourceType;
        this.amount = amount;
        this.isActive = true;
        this.hasCondition = false;
        this.description = description;
    }
    
    /// <summary>
    /// ルールが有効かチェック
    /// </summary>
    /// <returns>有効な場合true</returns>
    public bool IsValid()
    {
        return isActive && !string.IsNullOrEmpty(actionSpaceId) && amount > 0;
    }
    
    /// <summary>
    /// ルールを無効化
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
    }
    
    /// <summary>
    /// ルールを有効化
    /// </summary>
    public void Activate()
    {
        isActive = true;
    }
}

/// <summary>
/// 累積履歴エントリ
/// 累積アイテムの追加・消費の履歴を記録
/// </summary>
[System.Serializable]
public class AccumulationEntry
{
    [Header("アクション情報")]
    public AccumulationAction actionType;
    public ResourceType resourceType;
    public int amount;
    
    [Header("メタデータ")]
    public string sourceId;
    public System.DateTime timestamp;
    
    [Header("追加情報")]
    public string notes;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public AccumulationEntry()
    {
        timestamp = System.DateTime.Now;
    }
    
    /// <summary>
    /// エントリの文字列表現を取得
    /// </summary>
    /// <returns>エントリの文字列</returns>
    public override string ToString()
    {
        string actionText = actionType switch
        {
            AccumulationAction.Add => "追加",
            AccumulationAction.Consume => "消費",
            AccumulationAction.Clear => "クリア",
            _ => "不明"
        };
        
        return $"{timestamp:HH:mm:ss} - {actionText}: {resourceType} x{amount} (from: {sourceId})";
    }
}

/// <summary>
/// 累積アクションの種類
/// </summary>
public enum AccumulationAction
{
    Add,        // 追加
    Consume,    // 消費
    Clear       // クリア
}

/// <summary>
/// アクションスペースの固有効果
/// 累積アイテムとは独立した、アクションスペース本来の効果を定義
/// </summary>
[System.Serializable]
public class ActionEffect
{
    [Header("効果設定")]
    public ActionEffectType effectType;
    public ResourceType resourceType;
    public int amount;
    
    [Header("説明")]
    public string description;
    
    [Header("条件設定")]
    public bool hasCondition = false;
    public string conditionDescription;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ActionEffect()
    {
        hasCondition = false;
    }
    
    /// <summary>
    /// コンストラクタ（基本パラメータ）
    /// </summary>
    /// <param name="effectType">効果種別</param>
    /// <param name="resourceType">リソース種別</param>
    /// <param name="amount">効果量</param>
    public ActionEffect(ActionEffectType effectType, ResourceType resourceType, int amount)
    {
        this.effectType = effectType;
        this.resourceType = resourceType;
        this.amount = amount;
        this.hasCondition = false;
        this.description = $"{effectType}: {resourceType} x{amount}";
    }
    
    /// <summary>
    /// 効果を実行
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    public void Execute(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning("Player is null in ActionEffect.Execute");
            return;
        }
        
        switch (effectType)
        {
            case ActionEffectType.GainResource:
                player.AddResource(resourceType, amount);
                Debug.Log($"[ActionEffect] {player.playerName}が{GetResourceJapaneseName(resourceType)}を{amount}個獲得");
                break;
                
            case ActionEffectType.SpendResource:
                if (player.GetResource(resourceType) >= amount)
                {
                    player.SpendResource(resourceType, amount);
                    Debug.Log($"[ActionEffect] {player.playerName}が{GetResourceJapaneseName(resourceType)}を{amount}個消費");
                }
                else
                {
                    Debug.LogWarning($"[ActionEffect] {player.playerName}は{GetResourceJapaneseName(resourceType)}が不足しています");
                }
                break;
                
            case ActionEffectType.DrawCard:
                // カードドロー効果（実装は後で）
                Debug.Log($"[ActionEffect] {player.playerName}がカードを{amount}枚ドロー");
                break;
                
            case ActionEffectType.AddField:
                for (int i = 0; i < amount; i++)
                {
                    player.AddField();
                }
                Debug.Log($"[ActionEffect] {player.playerName}が畑を{amount}個追加");
                break;
                
            case ActionEffectType.AddVictoryPoints:
                player.AddVictoryPoints(amount);
                Debug.Log($"[ActionEffect] {player.playerName}が勝利点を{amount}点獲得");
                break;
                
            case ActionEffectType.FamilyGrowth:
                if (player.GrowFamily())
                {
                    Debug.Log($"[ActionEffect] {player.playerName}の家族が増えました");
                }
                else
                {
                    Debug.Log($"[ActionEffect] {player.playerName}は家族を増やせません");
                }
                break;
                
            case ActionEffectType.HouseExpansion:
                if (player.ExpandHouse(amount, ResourceType.Wood))
                {
                    Debug.Log($"[ActionEffect] {player.playerName}が住居を拡張しました");
                }
                else
                {
                    Debug.Log($"[ActionEffect] {player.playerName}は住居を拡張できません");
                }
                break;
                
            case ActionEffectType.PlayMinorImprovement:
                // 小さい進歩カードプレイ（別のスクリプトで実装予定）
                PlayMinorImprovementCard(player, amount);
                break;
                
            case ActionEffectType.PlayOccupation:
                // 職業カードプレイ（別のスクリプトで実装予定）
                PlayOccupationCard(player, amount);
                break;
                
            default:
                Debug.LogWarning($"[ActionEffect] Unknown effect type: {effectType}");
                break;
        }
    }
    
    /// <summary>
    /// 効果が実行可能かチェック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>実行可能な場合true</returns>
    public bool CanExecute(Player player)
    {
        if (player == null) return false;
        
        switch (effectType)
        {
            case ActionEffectType.SpendResource:
                return player.GetResource(resourceType) >= amount;
                
            case ActionEffectType.FamilyGrowth:
                return player.GetAvailableRooms() > player.GetFamilySize();
                
            case ActionEffectType.HouseExpansion:
                return player.GetResource(ResourceType.Wood) >= amount;
                
            default:
                return true;
        }
    }
    
    /// <summary>
    /// 小さい進歩カードをプレイする処理
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="amount">プレイできるカード数</param>
    private void PlayMinorImprovementCard(Player player, int amount)
    {
        // 別のスクリプトで実装予定の進歩カード管理システムを呼び出し
        var improvementManager = Object.FindObjectOfType<ImprovementManager>();
        if (improvementManager != null)
        {
            improvementManager.PlayMinorImprovement(player, amount);
        }
        else
        {
            // プレースホルダー処理：ImprovementManagerが未実装の場合
            Debug.Log($"[ActionEffect] {player.playerName}が小さい進歩カードを{amount}枚プレイできます（ImprovementManager未実装）");
            
            // 仮実装：プレイヤーのOnMinorImprovementPlayableイベントを発火
            if (player != null)
            {
                player.OnMinorImprovementPlayable?.Invoke(amount);
            }
        }
    }
    
    /// <summary>
    /// 職業カードをプレイする処理
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="amount">プレイできるカード数</param>
    private void PlayOccupationCard(Player player, int amount)
    {
        // 別のスクリプトで実装予定の職業カード管理システムを呼び出し
        var occupationManager = Object.FindObjectOfType<OccupationManager>();
        if (occupationManager != null)
        {
            occupationManager.PlayOccupation(player, amount);
        }
        else
        {
            // プレースホルダー処理：OccupationManagerが未実装の場合
            Debug.Log($"[ActionEffect] {player.playerName}が職業カードを{amount}枚プレイできます（OccupationManager未実装）");
            
            // 仮実装：プレイヤーのOnOccupationPlayableイベントを発火
            if (player != null)
            {
                player.OnOccupationPlayable?.Invoke(amount);
            }
        }
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
/// アクション効果の種類
/// </summary>
public enum ActionEffectType
{
    GainResource,       // リソース獲得
    SpendResource,      // リソース消費
    DrawCard,           // カードドロー
    AddField,           // 畑追加
    AddVictoryPoints,   // 勝利点追加
    FamilyGrowth,       // 家族成長
    HouseExpansion,     // 住居拡張
    HouseRenovation,    // 住居改築
    BuildFences,        // 柵建設
    BuildStables,       // 小屋建設
    TakeAnimals,        // 動物獲得
    PlayOccupation,     // 職業カードプレイ
    PlayImprovement,    // 改良カードプレイ
    PlayMinorImprovement, // 小さい進歩カードプレイ
    StartingPlayer,     // スタートプレイヤー
    TradeResources,     // リソース交換
    SpecialAction       // 特殊アクション
}

/// <summary>
/// 累積システムの設定
/// </summary>
[System.Serializable]
public class AccumulationSettings
{
    [Header("全般設定")]
    public bool enableAccumulation = true;
    public bool enableDebugLogs = true;
    
    [Header("履歴設定")]
    public int maxHistoryEntries = 100;
    public bool enableHistoryLogging = true;
    
    [Header("パフォーマンス設定")]
    public bool enableEventCallbacks = true;
    public int maxPoolsPerActionSpace = 1;
    
    /// <summary>
    /// デフォルト設定を取得
    /// </summary>
    /// <returns>デフォルト設定</returns>
    public static AccumulationSettings GetDefault()
    {
        return new AccumulationSettings
        {
            enableAccumulation = true,
            enableDebugLogs = true,
            maxHistoryEntries = 100,
            enableHistoryLogging = true,
            enableEventCallbacks = true,
            maxPoolsPerActionSpace = 1
        };
    }
}

/// <summary>
/// 累積システムのユーティリティクラス
/// </summary>
public static class AccumulationUtils
{
    /// <summary>
    /// アクションスペースIDを正規化
    /// </summary>
    /// <param name="actionSpaceName">アクションスペース名</param>
    /// <returns>正規化されたID</returns>
    public static string NormalizeActionSpaceId(string actionSpaceName)
    {
        if (string.IsNullOrEmpty(actionSpaceName))
            return string.Empty;
        
        // 日本語名を英語IDに変換
        switch (actionSpaceName)
        {
            case "森": return "forest";
            case "土取り場": return "clay_pit";
            case "葦の沼": return "reed_pond";
            case "漁場": return "fishing";
            case "羊市場": return "sheep_market";
            case "猪市場": return "boar_market";
            case "牛市場": return "cattle_market";
            case "穀物の種": return "grain_seeds";
            case "畑を耕す": return "plow_field";
            case "種まきと製パン": return "sow_and_bake";
            case "家族の成長": return "family_growth";
            case "住居の拡張": return "house_expansion";
            case "住居の改築": return "house_renovation";
            case "柵の建設": return "build_fences";
            case "スタートプレイヤー": return "starting_player";
            case "日雇い労働者": return "day_laborer";
            default: return actionSpaceName.ToLower().Replace(" ", "_");
        }
    }
    
    /// <summary>
    /// リソースタイプの日本語名を取得
    /// </summary>
    /// <param name="resourceType">リソースタイプ</param>
    /// <returns>日本語名</returns>
    public static string GetResourceJapaneseName(ResourceType resourceType)
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
    
    /// <summary>
    /// アイテム辞書の概要文字列を作成
    /// </summary>
    /// <param name="items">アイテム辞書</param>
    /// <returns>概要文字列</returns>
    public static string CreateItemsSummary(Dictionary<ResourceType, int> items)
    {
        if (items == null || items.Count == 0)
            return "なし";
        
        var summary = new List<string>();
        foreach (var item in items)
        {
            summary.Add($"{GetResourceJapaneseName(item.Key)} x{item.Value}");
        }
        
        return string.Join(", ", summary);
    }
}