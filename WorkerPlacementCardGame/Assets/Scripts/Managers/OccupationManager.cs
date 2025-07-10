using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 職業カードを管理するシステム
/// 現在はプレースホルダー実装
/// TODO: 将来的に完全な職業カードシステムを実装予定
/// </summary>
public class OccupationManager : MonoBehaviour
{
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("職業カード設定")]
    [SerializeField] private List<OccupationCardData> availableOccupations = new List<OccupationCardData>();
    
    // イベント
    public System.Action<Player, OccupationCardData> OnOccupationPlayed;
    
    void Start()
    {
        InitializeOccupations();
        DebugLog("OccupationManager initialized (Placeholder)");
    }
    
    /// <summary>
    /// 利用可能な職業カードを初期化
    /// </summary>
    private void InitializeOccupations()
    {
        // TODO: 将来的にここで職業カードのデータを読み込む
        // プレースホルダーとしていくつかの職業を追加
        availableOccupations.Add(new OccupationCardData("農夫", "畑での作業時に追加ボーナスを得る"));
        availableOccupations.Add(new OccupationCardData("大工", "建設アクション時にコストを削減"));
        availableOccupations.Add(new OccupationCardData("パン屋", "穀物を食料に変換できる"));
        availableOccupations.Add(new OccupationCardData("牧場主", "動物の世話で追加効果を得る"));
        availableOccupations.Add(new OccupationCardData("商人", "リソース交換で有利なレートを得る"));
        
        DebugLog($"Occupation cards initialized: {availableOccupations.Count} cards");
    }
    
    /// <summary>
    /// 職業カードをプレイする
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="count">プレイできるカード数</param>
    public void PlayOccupation(Player player, int count = 1)
    {
        if (player == null)
        {
            DebugLog("Player is null in PlayOccupation");
            return;
        }
        
        // TODO: 将来的に実際の職業カードの選択・プレイロジックを実装
        DebugLog($"{player.playerName}が職業カードを{count}枚プレイできます");
        
        // プレースホルダー処理：利用可能な職業があるかチェック
        if (availableOccupations.Count == 0)
        {
            DebugLog("利用可能な職業カードがありません（プレースホルダー）");
            ShowOccupationPlaceholder(player, count);
            return;
        }
        
        // プレースホルダー処理：簡単なカード選択シミュレーション
        for (int i = 0; i < count && i < availableOccupations.Count; i++)
        {
            var card = availableOccupations[i];
            DebugLog($"  職業カード「{card.cardName}」をプレイ可能");
            
            // イベント発火
            OnOccupationPlayed?.Invoke(player, card);
        }
    }
    
    /// <summary>
    /// 職業カードのプレースホルダー表示
    /// </summary>
    private void ShowOccupationPlaceholder(Player player, int count)
    {
        DebugLog($"=== 職業カード選択画面（プレースホルダー） ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"選択可能枚数: {count}枚");
        DebugLog($"※ 実際の職業カードシステムは未実装です");
        DebugLog($"※ 将来的にUIでカード選択画面を表示予定");
        
        // 利用可能な職業カードをリスト表示
        DebugLog("利用可能な職業カード:");
        for (int i = 0; i < availableOccupations.Count && i < count; i++)
        {
            var card = availableOccupations[i];
            DebugLog($"  {i + 1}. {card.cardName} - {card.description}");
        }
        
        // プレイヤーのイベントを発火して、UIシステムに通知
        player.OnOccupationPlayable?.Invoke(count);
    }
    
    /// <summary>
    /// 利用可能な職業カードを取得
    /// </summary>
    public List<OccupationCardData> GetAvailableOccupations()
    {
        return new List<OccupationCardData>(availableOccupations);
    }
    
    /// <summary>
    /// 特定の職業カードをプレイヤーに追加（テスト用）
    /// </summary>
    public bool AddOccupationToPlayer(Player player, string occupationName)
    {
        var occupation = availableOccupations.Find(o => o.cardName == occupationName);
        if (occupation != null)
        {
            DebugLog($"{player.playerName}が職業「{occupationName}」を獲得しました");
            OnOccupationPlayed?.Invoke(player, occupation);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// デバッグ用：職業カードシステムの状態を表示
    /// </summary>
    [ContextMenu("Show Occupation Status")]
    public void ShowOccupationStatus()
    {
        DebugLog("=== 職業カードシステム状態 ===");
        DebugLog($"利用可能な職業カード: {availableOccupations.Count}枚");
        DebugLog($"ステータス: プレースホルダー実装");
        
        DebugLog("職業カードリスト:");
        foreach (var occupation in availableOccupations)
        {
            DebugLog($"  - {occupation.cardName}: {occupation.description}");
        }
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[OccupationManager] {message}");
        }
    }
}

/// <summary>
/// 職業カードのプレースホルダークラス
/// TODO: 将来的に完全な職業カードクラスを実装予定
/// </summary>
[System.Serializable]
public class OccupationCardData
{
    public string cardName;
    public string description;
    public OccupationType occupationType;
    public bool isActive;
    
    public OccupationCardData(string name, string desc, OccupationType type = OccupationType.General)
    {
        this.cardName = name;
        this.description = desc;
        this.occupationType = type;
        this.isActive = false;
    }
}

/// <summary>
/// 職業の種類
/// </summary>
public enum OccupationType
{
    General,        // 一般職業
    Farmer,         // 農業関連
    Builder,        // 建設関連
    Trader,         // 商業関連
    Artisan,        // 手工業関連
    Cook            // 料理関連
}