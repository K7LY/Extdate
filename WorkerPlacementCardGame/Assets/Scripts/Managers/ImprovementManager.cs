using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 進歩カード（小さい進歩・大きい進歩）を管理するシステム
/// 現在はプレースホルダー実装
/// TODO: 将来的に完全な進歩カードシステムを実装予定
/// </summary>
public class ImprovementManager : MonoBehaviour
{
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("進歩カード設定")]
    [SerializeField] private List<MinorImprovementCard> availableMinorImprovements = new List<MinorImprovementCard>();
    [SerializeField] private List<MajorImprovementCard> availableMajorImprovements = new List<MajorImprovementCard>();
    
    // イベント
    public System.Action<Player, MinorImprovementCard> OnMinorImprovementPlayed;
    public System.Action<Player, MajorImprovementCard> OnMajorImprovementPlayed;
    
    void Start()
    {
        InitializeImprovements();
        DebugLog("ImprovementManager initialized (Placeholder)");
    }
    
    /// <summary>
    /// 利用可能な改良カードを初期化
    /// </summary>
    private void InitializeImprovements()
    {
        // TODO: 将来的にここで進歩カードのデータを読み込む
        DebugLog("Improvement cards initialization - placeholder");
    }
    
    /// <summary>
    /// 小さい進歩カードをプレイする
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="count">プレイできるカード数</param>
    public void PlayMinorImprovement(Player player, int count = 1)
    {
        if (player == null)
        {
            DebugLog("Player is null in PlayMinorImprovement");
            return;
        }
        
        // TODO: 将来的に実際の小さい進歩カードの選択・プレイロジックを実装
        DebugLog($"{player.playerName}が小さい進歩カードを{count}枚プレイできます");
        
        // プレースホルダー処理：利用可能な小さい進歩があるかチェック
        if (availableMinorImprovements.Count == 0)
        {
            DebugLog("利用可能な小さい進歩カードがありません（プレースホルダー）");
            ShowMinorImprovementPlaceholder(player, count);
            return;
        }
        
        // プレースホルダー処理：簡単なカード選択シミュレーション
        for (int i = 0; i < count && i < availableMinorImprovements.Count; i++)
        {
            var card = availableMinorImprovements[i];
            DebugLog($"  小さい進歩カード「{card.cardName}」をプレイ可能");
            
            // イベント発火
            OnMinorImprovementPlayed?.Invoke(player, card);
        }
    }
    
    /// <summary>
    /// 大きい進歩カードをプレイする
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="count">プレイできるカード数</param>
    public void PlayMajorImprovement(Player player, int count = 1)
    {
        if (player == null)
        {
            DebugLog("Player is null in PlayMajorImprovement");
            return;
        }
        
        // TODO: 将来的に実際の大きい進歩カードの選択・プレイロジックを実装
        DebugLog($"{player.playerName}が大きい進歩カードを{count}枚プレイできます");
        
        // プレースホルダー処理
        ShowMajorImprovementPlaceholder(player, count);
    }
    
    /// <summary>
    /// 小さい進歩カードのプレースホルダー表示
    /// </summary>
    private void ShowMinorImprovementPlaceholder(Player player, int count)
    {
        DebugLog($"=== 小さい進歩カード選択画面（プレースホルダー） ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"選択可能枚数: {count}枚");
        DebugLog($"※ 実際の進歩カードシステムは未実装です");
        DebugLog($"※ 将来的にUIでカード選択画面を表示予定");
        
        // プレイヤーのイベントを発火して、UIシステムに通知
        player.OnMinorImprovementPlayable?.Invoke(count);
    }
    
    /// <summary>
    /// 大きい進歩カードのプレースホルダー表示
    /// </summary>
    private void ShowMajorImprovementPlaceholder(Player player, int count)
    {
        DebugLog($"=== 大きい進歩カード選択画面（プレースホルダー） ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"選択可能枚数: {count}枚");
        DebugLog($"※ 実際の進歩カードシステムは未実装です");
    }
    
    /// <summary>
    /// 利用可能な小さい進歩カードを取得
    /// </summary>
    public List<MinorImprovementCard> GetAvailableMinorImprovements()
    {
        return new List<MinorImprovementCard>(availableMinorImprovements);
    }
    
    /// <summary>
    /// 利用可能な大きい進歩カードを取得
    /// </summary>
    public List<MajorImprovementCard> GetAvailableMajorImprovements()
    {
        return new List<MajorImprovementCard>(availableMajorImprovements);
    }
    
    /// <summary>
    /// デバッグ用：進歩カードシステムの状態を表示
    /// </summary>
    [ContextMenu("Show Improvement Status")]
    public void ShowImprovementStatus()
    {
        DebugLog("=== 進歩カードシステム状態 ===");
        DebugLog($"小さい進歩カード: {availableMinorImprovements.Count}枚");
        DebugLog($"大きい進歩カード: {availableMajorImprovements.Count}枚");
        DebugLog($"ステータス: プレースホルダー実装");
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ImprovementManager] {message}");
        }
    }
}

/// <summary>
/// 小さい進歩カードのプレースホルダークラス
/// TODO: 将来的に完全な進歩カードクラスを実装予定
/// </summary>
[System.Serializable]
public class MinorImprovementCard
{
    public string cardName;
    public string description;
    public int cost;
    public ResourceType costType;
    
    public MinorImprovementCard(string name, string desc, int cost = 0, ResourceType costType = ResourceType.Wood)
    {
        this.cardName = name;
        this.description = desc;
        this.cost = cost;
        this.costType = costType;
    }
}

/// <summary>
/// 大きい進歩カードのプレースホルダークラス
/// TODO: 将来的に完全な進歩カードクラスを実装予定
/// </summary>
[System.Serializable]
public class MajorImprovementCard
{
    public string cardName;
    public string description;
    public int cost;
    public ResourceType costType;
    
    public MajorImprovementCard(string name, string desc, int cost = 0, ResourceType costType = ResourceType.Wood)
    {
        this.cardName = name;
        this.description = desc;
        this.cost = cost;
        this.costType = costType;
    }
}