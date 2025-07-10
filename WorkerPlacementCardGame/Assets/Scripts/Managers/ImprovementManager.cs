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
    /// 小さい進歩カードをプレイする（包括的な実装）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="maxCount">プレイできる最大カード数</param>
    public void PlayMinorImprovement(Player player, int maxCount = 1)
    {
        if (player == null)
        {
            DebugLog("プレイヤーが指定されていません");
            return;
        }
        
        DebugLog($"=== {player.playerName}の小さい進歩カードプレイ開始 ===");
        DebugLog($"最大プレイ可能枚数: {maxCount}枚");
        
        // プレイ可能な小さい進歩カードを取得
        var playableCards = GetPlayableMinorImprovements(player);
        
        if (playableCards.Count == 0)
        {
            DebugLog($"{player.playerName}はプレイ可能な小さい進歩カードを持っていません");
            ShowNoPlayableCardsMessage(player, ImprovementCategory.Minor);
            return;
        }
        
        DebugLog($"プレイ可能な小さい進歩カード: {playableCards.Count}枚");
        foreach (var card in playableCards)
        {
            DebugLog($"  - {card.cardName} (コスト: {GetResourceCostString(card.GetPlayCost())})");
        }
        
        // カード選択処理を開始
        StartImprovementSelection(player, playableCards, maxCount, ImprovementCategory.Minor);
    }
    
    /// <summary>
    /// 大きい進歩カードをプレイする（包括的な実装）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="maxCount">プレイできる最大カード数</param>
    public void PlayMajorImprovement(Player player, int maxCount = 1)
    {
        if (player == null)
        {
            DebugLog("プレイヤーが指定されていません");
            return;
        }
        
        DebugLog($"=== {player.playerName}の大きい進歩カードプレイ開始 ===");
        DebugLog($"最大プレイ可能枚数: {maxCount}枚");
        
        // プレイ可能な大きい進歩カードを取得
        var playableCards = GetPlayableMajorImprovements(player);
        
        if (playableCards.Count == 0)
        {
            DebugLog($"{player.playerName}はプレイ可能な大きい進歩カードがありません");
            ShowNoPlayableCardsMessage(player, ImprovementCategory.Major);
            return;
        }
        
        DebugLog($"プレイ可能な大きい進歩カード: {playableCards.Count}枚");
        foreach (var card in playableCards)
        {
            DebugLog($"  - {card.cardName} (コスト: {GetResourceCostString(card.GetPlayCost())})");
        }
        
        // カード選択処理を開始
        StartImprovementSelection(player, playableCards, maxCount, ImprovementCategory.Major);
    }
    
    /// <summary>
    /// プレイ可能な小さい進歩カードを取得
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>プレイ可能な小さい進歩カードのリスト</returns>
    public List<EnhancedImprovementCard> GetPlayableMinorImprovements(Player player)
    {
        if (player == null) return new List<EnhancedImprovementCard>();
        
        var playerCards = player.GetMinorImprovements().Cast<EnhancedImprovementCard>().ToList();
        var playableCards = new List<EnhancedImprovementCard>();
        
        foreach (var card in playerCards)
        {
            if (CanPlayImprovement(player, card))
            {
                playableCards.Add(card);
            }
        }
        
        return playableCards;
    }
    
    /// <summary>
    /// プレイ可能な大きい進歩カードを取得
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>プレイ可能な大きい進歩カードのリスト</returns>
    public List<EnhancedImprovementCard> GetPlayableMajorImprovements(Player player)
    {
        if (player == null) return new List<EnhancedImprovementCard>();
        
        var playableCards = new List<EnhancedImprovementCard>();
        
        foreach (var card in availableMajorImprovements)
        {
            if (CanPlayImprovement(player, card) && !player.HasImprovementByName(card.cardName))
            {
                playableCards.Add(card);
            }
        }
        
        return playableCards;
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
    /// 進歩カード選択処理を開始
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="availableCards">選択可能なカード</param>
    /// <param name="maxCount">最大選択可能数</param>
    /// <param name="category">進歩カテゴリ</param>
    private void StartImprovementSelection(Player player, List<EnhancedImprovementCard> availableCards, int maxCount, ImprovementCategory category)
    {
        DebugLog($"進歩カード選択開始: {category}, 選択可能{availableCards.Count}枚, 最大{maxCount}枚");
        
        // UIシステムに進歩カード選択開始を通知
        OnImprovementSelectionStarted?.Invoke(player, availableCards);
        
        // 現在はAIによる自動選択を実装（将来的にはUIによる手動選択に置き換え）
        if (gameManager != null && gameManager.IsAIPlayer(player))
        {
            // AI自動選択
            var selectedCards = SelectCardsForAI(player, availableCards, maxCount);
            ProcessSelectedImprovements(player, selectedCards, category);
        }
        else
        {
            // 人間プレイヤーの場合は簡易自動選択（UIが実装されるまでの暫定処理）
            DebugLog($"UIによる手動選択は未実装のため、自動選択を行います");
            var selectedCards = SelectCardsForAI(player, availableCards, Math.Min(1, maxCount));
            ProcessSelectedImprovements(player, selectedCards, category);
        }
    }
    
    /// <summary>
    /// AIプレイヤー用のカード選択ロジック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="availableCards">選択可能なカード</param>
    /// <param name="maxCount">最大選択数</param>
    /// <returns>選択されたカード</returns>
    private List<EnhancedImprovementCard> SelectCardsForAI(Player player, List<EnhancedImprovementCard> availableCards, int maxCount)
    {
        var selectedCards = new List<EnhancedImprovementCard>();
        var sortedCards = availableCards.OrderByDescending(card => EvaluateCardForAI(player, card)).ToList();
        
        int selectedCount = 0;
        foreach (var card in sortedCards)
        {
            if (selectedCount >= maxCount) break;
            
            if (CanPlayImprovement(player, card))
            {
                selectedCards.Add(card);
                selectedCount++;
                DebugLog($"AI選択: {card.cardName} (評価値: {EvaluateCardForAI(player, card)})");
            }
        }
        
        return selectedCards;
    }
    
    /// <summary>
    /// AIのためのカード評価値を計算
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">評価するカード</param>
    /// <returns>評価値（高いほど優先）</returns>
    private float EvaluateCardForAI(Player player, EnhancedImprovementCard card)
    {
        float value = 0f;
        
        // 勝利点を重視
        value += card.GetVictoryPoints() * 10f;
        
        // リソースコストの評価（低コストほど高評価）
        var cost = card.GetPlayCost();
        float totalCost = cost.Values.Sum();
        value += (10f - totalCost) * 2f;
        
        // カード固有の価値評価
        switch (card.cardName)
        {
            case "土のかまど":
                value += 15f; // 基本的な調理設備として高価値
                break;
            case "暖炉":
                value += 20f; // 多くの他のカードの前提条件
                break;
            case "かご":
                value += 8f; // 野菜収穫時のボーナス
                break;
            case "陶器":
                value += 12f; // 食料変換効率向上
                break;
        }
        
        return value;
    }
    
    /// <summary>
    /// 選択された進歩カードを処理
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="selectedCards">選択されたカード</param>
    /// <param name="category">進歩カテゴリ</param>
    private void ProcessSelectedImprovements(Player player, List<EnhancedImprovementCard> selectedCards, ImprovementCategory category)
    {
        if (selectedCards.Count == 0)
        {
            DebugLog($"{player.playerName}は進歩カードを選択しませんでした");
            return;
        }
        
        DebugLog($"=== 進歩カードプレイ処理開始 ===");
        
        foreach (var card in selectedCards)
        {
            if (ExecuteImprovementPlay(player, card))
            {
                // 統計更新
                if (category == ImprovementCategory.Minor)
                {
                    totalMinorImprovementsPlayed++;
                    OnMinorImprovementPlayed?.Invoke(player, card);
                }
                else
                {
                    totalMajorImprovementsPlayed++;
                    OnMajorImprovementPlayed?.Invoke(player, card);
                }
                
                DebugLog($"✅ {player.playerName}が進歩「{card.cardName}」をプレイしました");
            }
            else
            {
                DebugLog($"❌ {player.playerName}の進歩「{card.cardName}」のプレイに失敗しました");
            }
        }
        
        DebugLog($"=== 進歩カードプレイ処理完了 ===");
        ShowPlayedImprovementsSummary(player, selectedCards);
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
    /// プレイできる進歩カードがない場合のメッセージ表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="category">進歩カテゴリ</param>
    private void ShowNoPlayableCardsMessage(Player player, ImprovementCategory category)
    {
        string categoryName = category == ImprovementCategory.Minor ? "小さい進歩" : "大きい進歩";
        
        DebugLog($"=== {categoryName}カードプレイ不可能 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"理由分析:");
        
        if (category == ImprovementCategory.Minor)
        {
            var allMinorCards = player.GetMinorImprovements().Cast<EnhancedImprovementCard>().ToList();
            DebugLog($"  手札の{categoryName}カード数: {allMinorCards.Count}枚");
            
            foreach (var card in allMinorCards)
            {
                var reason = GetUnplayableReason(player, card);
                DebugLog($"    {card.cardName}: {reason}");
            }
        }
        else
        {
            DebugLog($"  利用可能な{categoryName}カード数: {availableMajorImprovements.Count}枚");
            
            foreach (var card in availableMajorImprovements)
            {
                if (player.HasImprovementByName(card.cardName))
                {
                    DebugLog($"    {card.cardName}: 既にプレイ済み");
                }
                else
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