using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 進歩カード（小さい進歩・大きい進歩）を管理する包括的なシステム
/// 進歩を出すアクションの詳細な処理を実装
/// </summary>
public class ImprovementManager : MonoBehaviour
{
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("進歩カード設定")]
    [SerializeField] private List<EnhancedImprovementCard> availableMinorImprovements = new List<EnhancedImprovementCard>();
    [SerializeField] private List<EnhancedImprovementCard> availableMajorImprovements = new List<EnhancedImprovementCard>();
    
    [Header("プレイ設定")]
    [SerializeField] private bool allowMultipleSelection = true;
    [SerializeField] private bool requireFullResourcePayment = true;
    [SerializeField] private int maxCardsPerAction = 1;
    
    [Header("進歩アクション統計")]
    [SerializeField] private int totalMinorImprovementsPlayed = 0;
    [SerializeField] private int totalMajorImprovementsPlayed = 0;
    
    // システム依存性
    private CardLibrary cardLibrary;
    private GameManager gameManager;
    
    // イベント
    public System.Action<Player, EnhancedImprovementCard> OnMinorImprovementPlayed;
    public System.Action<Player, EnhancedImprovementCard> OnMajorImprovementPlayed;
    public System.Action<Player, List<EnhancedImprovementCard>> OnImprovementSelectionStarted;
    public System.Action<Player> OnImprovementSelectionCancelled;
    
    void Awake()
    {
        cardLibrary = FindObjectOfType<CardLibrary>();
        gameManager = FindObjectOfType<GameManager>();
    }
    
    void Start()
    {
        InitializeImprovements();
        DebugLog("ImprovementManager initialized with comprehensive progress action processing");
    }
    
    /// <summary>
    /// 利用可能な進歩カードを初期化
    /// </summary>
    private void InitializeImprovements()
    {
        if (cardLibrary != null)
        {
            // CardLibraryから進歩カードを取得
            availableMinorImprovements = cardLibrary.allMinorImprovements.Cast<EnhancedImprovementCard>().ToList();
            availableMajorImprovements = cardLibrary.allMajorImprovements.Cast<EnhancedImprovementCard>().ToList();
            
            DebugLog($"進歩カード初期化完了: 小進歩{availableMinorImprovements.Count}枚, 大進歩{availableMajorImprovements.Count}枚");
        }
        else
        {
            DebugLog("CardLibraryが見つかりません。代替の進歩カードを作成します。");
            CreateDefaultImprovements();
        }
    }
    
    /// <summary>
    /// デフォルトの進歩カードを作成（CardLibraryが無い場合の代替）
    /// </summary>
    private void CreateDefaultImprovements()
    {
        // 小さい進歩カードの例
        var clayOven = ScriptableObject.CreateInstance<EnhancedImprovementCard>();
        clayOven.Initialize("土のかまど", "MINOR_001", ImprovementCategory.Minor);
        clayOven.SetPlayCost(new Dictionary<ResourceType, int> { { ResourceType.Clay, 2 } });
        clayOven.SetVictoryPoints(1);
        availableMinorImprovements.Add(clayOven);
        
        var basket = ScriptableObject.CreateInstance<EnhancedImprovementCard>();
        basket.Initialize("かご", "MINOR_002", ImprovementCategory.Minor);
        basket.SetPlayCost(new Dictionary<ResourceType, int> { { ResourceType.Reed, 2 } });
        availableMinorImprovements.Add(basket);
        
        // 大きい進歩カードの例
        var fireplace = ScriptableObject.CreateInstance<EnhancedImprovementCard>();
        fireplace.Initialize("暖炉", "MAJOR_001", ImprovementCategory.Major);
        fireplace.SetPlayCost(new Dictionary<ResourceType, int> 
        { 
            { ResourceType.Clay, 2 }, 
            { ResourceType.Stone, 3 } 
        });
        fireplace.SetVictoryPoints(1);
        availableMajorImprovements.Add(fireplace);
        
        DebugLog("デフォルト進歩カードを作成しました");
    }
    
    /// <summary>
    /// 進歩カードをプレイする（プレイヤー選択システム）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="allowMinor">小さい進歩を許可するか</param>
    /// <param name="allowMajor">大きい進歩を許可するか</param>
    /// <param name="maxCount">プレイできる最大カード数</param>
    public void PlayImprovement(Player player, bool allowMinor, bool allowMajor, int maxCount = 1)
    {
        if (player == null)
        {
            DebugLog("プレイヤーが指定されていません");
            return;
        }
        
        DebugLog($"=== {player.playerName}の進歩カードプレイ開始 ===");
        DebugLog($"小さい進歩: {(allowMinor ? "許可" : "不許可")}");
        DebugLog($"大きい進歩: {(allowMajor ? "許可" : "不許可")}");
        DebugLog($"最大プレイ可能枚数: {maxCount}枚");
        
        // 利用可能な進歩カードを取得
        var availableCards = GetAvailableImprovements(player, allowMinor, allowMajor);
        
        if (availableCards.Count == 0)
        {
            DebugLog($"{player.playerName}はプレイ可能な進歩カードがありません");
            ShowNoAvailableCardsMessage(player, allowMinor, allowMajor);
            return;
        }
        
        // プレイヤーに選択肢を提示
        ShowImprovementSelectionToPlayer(player, availableCards, maxCount);
    }
    
    /// <summary>
    /// 小さい進歩カードをプレイする（後方互換性）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="maxCount">プレイできる最大カード数</param>
    public void PlayMinorImprovement(Player player, int maxCount = 1)
    {
        PlayImprovement(player, true, false, maxCount);
    }
    
    /// <summary>
    /// 大きい進歩カードをプレイする（後方互換性）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="maxCount">プレイできる最大カード数</param>
    public void PlayMajorImprovement(Player player, int maxCount = 1)
    {
        PlayImprovement(player, false, true, maxCount);
    }
    
    /// <summary>
    /// 利用可能な進歩カードを取得（未プレイの大進歩 + 手札の小進歩）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="includeMinor">小さい進歩を含めるか</param>
    /// <param name="includeMajor">大きい進歩を含めるか</param>
    /// <returns>利用可能な進歩カードのリスト（選択肢として表示用）</returns>
    public List<EnhancedImprovementCard> GetAvailableImprovements(Player player, bool includeMinor, bool includeMajor)
    {
        if (player == null) return new List<EnhancedImprovementCard>();
        
        var availableCards = new List<EnhancedImprovementCard>();
        
        // 小さい進歩カード（手札から）
        if (includeMinor)
        {
            var playerMinorCards = player.GetMinorImprovements().Cast<EnhancedImprovementCard>().ToList();
            foreach (var card in playerMinorCards)
            {
                availableCards.Add(card);
            }
        }
        
        // 大きい進歩カード（未プレイのもの）
        if (includeMajor)
        {
            foreach (var card in availableMajorImprovements)
            {
                if (!player.HasImprovementByName(card.cardName))
                {
                    availableCards.Add(card);
                }
            }
        }
        
        return availableCards;
    }
    
    /// <summary>
    /// プレイ可能な小さい進歩カードを取得（後方互換性）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>プレイ可能な小さい進歩カードのリスト</returns>
    public List<EnhancedImprovementCard> GetPlayableMinorImprovements(Player player)
    {
        var availableCards = GetAvailableImprovements(player, true, false);
        return availableCards.Where(card => CanPlayImprovement(player, card)).ToList();
    }
    
    /// <summary>
    /// プレイ可能な大きい進歩カードを取得（後方互換性）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>プレイ可能な大きい進歩カードのリスト</returns>
    public List<EnhancedImprovementCard> GetPlayableMajorImprovements(Player player)
    {
        var availableCards = GetAvailableImprovements(player, false, true);
        return availableCards.Where(card => CanPlayImprovement(player, card)).ToList();
    }
    
    /// <summary>
    /// 進歩カードがプレイ可能かチェック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">チェックする進歩カード</param>
    /// <returns>プレイ可能な場合true</returns>
    public bool CanPlayImprovement(Player player, EnhancedImprovementCard card)
    {
        if (player == null || card == null) return false;
        
        // 1. 基本的なプレイ条件をチェック
        if (!card.CanPlay(player))
        {
            return false;
        }
        
        // 2. リソースコストをチェック
        if (requireFullResourcePayment && !HasSufficientResources(player, card))
        {
            return false;
        }
        
        // 3. 既にプレイ済みかチェック（大きい進歩の場合）
        if (card.category == ImprovementCategory.Major && player.HasImprovementByName(card.cardName))
        {
            return false;
        }
        
        // 4. 特殊な前提条件をチェック
        if (!CheckSpecialPrerequisites(player, card))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// プレイヤーが十分なリソースを持っているかチェック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">チェックする進歩カード</param>
    /// <returns>十分なリソースがある場合true</returns>
    private bool HasSufficientResources(Player player, EnhancedImprovementCard card)
    {
        var cost = card.GetPlayCost();
        foreach (var requirement in cost)
        {
            if (player.GetResource(requirement.Key) < requirement.Value)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// 特殊な前提条件をチェック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">チェックする進歩カード</param>
    /// <returns>前提条件を満たしている場合true</returns>
    private bool CheckSpecialPrerequisites(Player player, EnhancedImprovementCard card)
    {
        // カード固有の前提条件をチェック
        switch (card.cardName)
        {
            case "暖炉":
                // 暖炉は土のかまどまたは石のかまどが必要
                return player.HasImprovementByName("土のかまど") || player.HasImprovementByName("石のかまど");
                
            case "調理場":
                // 調理場はかまどが必要
                return player.HasImprovementByName("土のかまど") || 
                       player.HasImprovementByName("石のかまど") || 
                       player.HasImprovementByName("暖炉");
                
            case "陶器":
                // 陶器は土のかまどが必要
                return player.HasImprovementByName("土のかまど");
                
            default:
                return true; // 特殊な前提条件なし
        }
    }
    
    /// <summary>
    /// プレイヤーに進歩カード選択肢を表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="availableCards">利用可能なカード</param>
    /// <param name="maxCount">最大選択可能数</param>
    private void ShowImprovementSelectionToPlayer(Player player, List<EnhancedImprovementCard> availableCards, int maxCount)
    {
        DebugLog($"=== {player.playerName}の進歩カード選択 ===");
        DebugLog($"利用可能カード数: {availableCards.Count}枚");
        DebugLog($"最大選択可能数: {maxCount}枚");
        
        // カード一覧を表示
        for (int i = 0; i < availableCards.Count; i++)
        {
            var card = availableCards[i];
            var category = card.category == ImprovementCategory.Minor ? "小進歩" : "大進歩";
            var cost = GetResourceCostString(card.GetPlayCost());
            var playable = CanPlayImprovement(player, card) ? "プレイ可能" : "プレイ不可";
            var reason = CanPlayImprovement(player, card) ? "" : $"（{GetUnplayableReason(player, card)}）";
            
            DebugLog($"  [{i + 1}] {card.cardName} ({category})");
            DebugLog($"      コスト: {cost}");
            DebugLog($"      勝利点: {card.GetVictoryPoints()}点");
            DebugLog($"      状態: {playable} {reason}");
        }
        
        // UIシステムに進歩カード選択開始を通知
        OnImprovementSelectionStarted?.Invoke(player, availableCards);
        
        DebugLog("プレイヤーの選択を待機中... (UIが実装されるまでは手動で ExecuteSelectedImprovement を呼び出してください)");
    }
    
    /// <summary>
    /// プレイヤーが選択した進歩カードを実行
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="selectedCard">選択されたカード</param>
    /// <returns>実行に成功した場合true</returns>
    public bool ExecuteSelectedImprovement(Player player, EnhancedImprovementCard selectedCard)
    {
        if (player == null)
        {
            DebugLog("プレイヤーが指定されていません");
            return false;
        }
        
        if (selectedCard == null)
        {
            DebugLog("選択されたカードが指定されていません");
            return false;
        }
        
        DebugLog($"=== 選択された進歩カードの実行 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"選択カード: {selectedCard.cardName}");
        
        // 実行条件とコストを確認
        if (!CanPlayImprovement(player, selectedCard))
        {
            var reason = GetUnplayableReason(player, selectedCard);
            DebugLog($"❌ カードをプレイできません: {reason}");
            return false;
        }
        
        // カードを実行
        if (ExecuteImprovementPlay(player, selectedCard))
        {
            // 統計更新とイベント発火
            if (selectedCard.category == ImprovementCategory.Minor)
            {
                totalMinorImprovementsPlayed++;
                OnMinorImprovementPlayed?.Invoke(player, selectedCard);
            }
            else
            {
                totalMajorImprovementsPlayed++;
                OnMajorImprovementPlayed?.Invoke(player, selectedCard);
            }
            
            DebugLog($"✅ {player.playerName}が進歩「{selectedCard.cardName}」をプレイしました");
            ShowPlayedImprovementsSummary(player, new List<EnhancedImprovementCard> { selectedCard });
            return true;
        }
        else
        {
            DebugLog($"❌ {player.playerName}の進歩「{selectedCard.cardName}」のプレイに失敗しました");
            return false;
        }
    }
    
    /// <summary>
    /// 進歩カードの実際のプレイを実行
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">プレイする進歩カード</param>
    /// <returns>プレイに成功した場合true</returns>
    private bool ExecuteImprovementPlay(Player player, EnhancedImprovementCard card)
    {
        // 最終的なプレイ可能性をチェック
        if (!CanPlayImprovement(player, card))
        {
            DebugLog($"進歩カード「{card.cardName}」はプレイできません");
            return false;
        }
        
        try
        {
            // リソースコストを支払う
            var cost = card.GetPlayCost();
            foreach (var requirement in cost)
            {
                player.SpendResource(requirement.Key, requirement.Value);
                DebugLog($"  コスト支払い: {AccumulationUtils.GetResourceJapaneseName(requirement.Key)} x{requirement.Value}");
            }
            
            // カードをプレイ
            bool playSuccess = card.Play(player);
            
            if (playSuccess)
            {
                // 大きい進歩の場合は利用可能カードから除去
                if (card.category == ImprovementCategory.Major)
                {
                    availableMajorImprovements.Remove(card);
                }
                
                return true;
            }
            else
            {
                // プレイに失敗した場合はリソースを返却
                foreach (var requirement in cost)
                {
                    player.AddResource(requirement.Key, requirement.Value);
                }
                return false;
            }
        }
        catch (System.Exception ex)
        {
            DebugLog($"進歩カードプレイ中にエラーが発生しました: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 利用可能な進歩カードがない場合のメッセージ表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="includeMinor">小さい進歩を含めるか</param>
    /// <param name="includeMajor">大きい進歩を含めるか</param>
    private void ShowNoAvailableCardsMessage(Player player, bool includeMinor, bool includeMajor)
    {
        DebugLog($"=== 進歩カードプレイ不可能 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"対象: 小進歩{(includeMinor ? "○" : "×")} 大進歩{(includeMajor ? "○" : "×")}");
        DebugLog($"理由分析:");
        
        if (includeMinor)
        {
            var allMinorCards = player.GetMinorImprovements().Cast<EnhancedImprovementCard>().ToList();
            DebugLog($"  手札の小さい進歩カード数: {allMinorCards.Count}枚");
            
            if (allMinorCards.Count == 0)
            {
                DebugLog($"    小さい進歩カードを手札に持っていません");
            }
            else
            {
                foreach (var card in allMinorCards)
                {
                    var reason = GetUnplayableReason(player, card);
                    DebugLog($"    {card.cardName}: {reason}");
                }
            }
        }
        
        if (includeMajor)
        {
            var availableMajorCards = availableMajorImprovements.Where(card => !player.HasImprovementByName(card.cardName)).ToList();
            DebugLog($"  利用可能な大きい進歩カード数: {availableMajorCards.Count}枚");
            
            if (availableMajorCards.Count == 0)
            {
                DebugLog($"    未プレイの大きい進歩カードがありません");
            }
            else
            {
                foreach (var card in availableMajorCards)
                {
                    var reason = GetUnplayableReason(player, card);
                    DebugLog($"    {card.cardName}: {reason}");
                }
            }
        }
        
        DebugLog($"=== 分析完了 ===");
    }
    
    /// <summary>
    /// カードがプレイできない理由を取得
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">チェックするカード</param>
    /// <returns>プレイできない理由</returns>
    private string GetUnplayableReason(Player player, EnhancedImprovementCard card)
    {
        if (!card.CanPlay(player))
        {
            return "基本プレイ条件を満たしていない";
        }
        
        if (!HasSufficientResources(player, card))
        {
            var cost = card.GetPlayCost();
            var missingResources = new List<string>();
            foreach (var requirement in cost)
            {
                int available = player.GetResource(requirement.Key);
                if (available < requirement.Value)
                {
                    int shortage = requirement.Value - available;
                    missingResources.Add($"{AccumulationUtils.GetResourceJapaneseName(requirement.Key)}{shortage}個不足");
                }
            }
            return $"リソース不足: {string.Join(", ", missingResources)}";
        }
        
        if (!CheckSpecialPrerequisites(player, card))
        {
            return "特殊な前提条件を満たしていない";
        }
        
        return "不明な理由";
    }
    
    /// <summary>
    /// プレイした進歩カードの概要を表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="playedCards">プレイしたカード</param>
    private void ShowPlayedImprovementsSummary(Player player, List<EnhancedImprovementCard> playedCards)
    {
        if (playedCards.Count == 0) return;
        
        DebugLog($"=== {player.playerName}の進歩カードプレイ結果 ===");
        DebugLog($"プレイしたカード数: {playedCards.Count}枚");
        
        int totalVictoryPoints = 0;
        foreach (var card in playedCards)
        {
            int vp = card.GetVictoryPoints();
            totalVictoryPoints += vp;
            DebugLog($"  ✅ {card.cardName} (勝利点: {vp})");
        }
        
        DebugLog($"獲得勝利点合計: {totalVictoryPoints}点");
        DebugLog($"現在の総進歩カード数: {player.GetImprovements().Count}個");
        DebugLog($"=== 結果表示完了 ===");
    }
    
    /// <summary>
    /// リソースコストの文字列表現を取得
    /// </summary>
    /// <param name="cost">リソースコスト</param>
    /// <returns>コスト文字列</returns>
    private string GetResourceCostString(Dictionary<ResourceType, int> cost)
    {
        if (cost == null || cost.Count == 0) return "なし";
        
        var costParts = cost.Select(kvp => 
            $"{AccumulationUtils.GetResourceJapaneseName(kvp.Key)} x{kvp.Value}"
        ).ToList();
        
        return string.Join(", ", costParts);
    }
    
    /// <summary>
    /// 利用可能な小さい進歩カードを取得（CardLibrary互換）
    /// </summary>
    public List<MinorImprovementCard> GetAvailableMinorImprovements()
    {
        // 後方互換性のために従来のAPIを維持
        return availableMinorImprovements.Cast<MinorImprovementCard>().ToList();
    }
    
    /// <summary>
    /// 利用可能な大きい進歩カードを取得（CardLibrary互換）
    /// </summary>
    public List<MajorImprovementCard> GetAvailableMajorImprovements()
    {
        // 後方互換性のために従来のAPIを維持
        return availableMajorImprovements.Cast<MajorImprovementCard>().ToList();
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
        DebugLog($"統計:");
        DebugLog($"  小進歩プレイ済み: {totalMinorImprovementsPlayed}枚");
        DebugLog($"  大進歩プレイ済み: {totalMajorImprovementsPlayed}枚");
        DebugLog($"設定:");
        DebugLog($"  複数選択: {allowMultipleSelection}");
        DebugLog($"  完全リソース支払い要求: {requireFullResourcePayment}");
        DebugLog($"  最大カード数/アクション: {maxCardsPerAction}");
        DebugLog($"ステータス: 包括的実装完了");
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ImprovementManager] {message}");
        }
    }
}

// 後方互換性のために従来のクラスを維持
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