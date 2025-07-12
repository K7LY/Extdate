using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 職業カードを管理する包括的なシステム
/// 職業カードを出すアクションの詳細な処理を実装
/// </summary>
public class EnhancedOccupationManager : MonoBehaviour
{
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("職業カード設定")]
    [SerializeField] private List<EnhancedOccupationCard> availableOccupations = new List<EnhancedOccupationCard>();
    
    [Header("プレイ設定")]
    [SerializeField] private bool allowMultipleSelection = true;
    [SerializeField] private int maxCardsPerAction = 1;
    
    [Header("職業アクション統計")]
    [SerializeField] private int totalOccupationsPlayed = 0;
    
    // システム依存性
    private CardLibrary cardLibrary;
    private GameManager gameManager;
    private CardTriggerManager cardTriggerManager;
    
    // イベント
    public System.Action<Player, EnhancedOccupationCard> OnOccupationPlayed;
    public System.Action<Player, List<EnhancedOccupationCard>> OnOccupationSelectionStarted;
    public System.Action<Player> OnOccupationSelectionCancelled;
    public System.Action<Player, int> OnInsufficientFood;
    public System.Action<Player> OnNoOccupationCards;
    
    void Awake()
    {
        cardLibrary = FindObjectOfType<CardLibrary>();
        gameManager = FindObjectOfType<GameManager>();
        cardTriggerManager = FindObjectOfType<CardTriggerManager>();
    }
    
    void Start()
    {
        InitializeOccupations();
        DebugLog("EnhancedOccupationManager initialized with comprehensive occupation card processing");
    }
    
    /// <summary>
    /// 利用可能な職業カードを初期化
    /// </summary>
    private void InitializeOccupations()
    {
        if (cardLibrary != null)
        {
            // CardLibraryから職業カードを取得
            availableOccupations = cardLibrary.GetAllOccupations().Cast<EnhancedOccupationCard>().ToList();
            DebugLog($"職業カード初期化完了: {availableOccupations.Count}枚");
        }
        else
        {
            DebugLog("CardLibraryが見つかりません。代替の職業カードを作成します。");
            CreateDefaultOccupations();
        }
    }
    
    /// <summary>
    /// デフォルトの職業カードを作成（CardLibraryが無い場合の代替）
    /// </summary>
    private void CreateDefaultOccupations()
    {
        // 基本職業カードの例
        var farmer = ScriptableObject.CreateInstance<EnhancedOccupationCard>();
        farmer.Initialize("農夫", "OCCUPATION_001", OccupationType.Farmer);
        farmer.SetEffectDescription("畑での作業時に追加ボーナスを得る");
        availableOccupations.Add(farmer);
        
        var carpenter = ScriptableObject.CreateInstance<EnhancedOccupationCard>();
        carpenter.Initialize("大工", "OCCUPATION_002", OccupationType.Carpenter);
        carpenter.SetEffectDescription("建設アクション時にコストを削減");
        availableOccupations.Add(carpenter);
        
        var baker = ScriptableObject.CreateInstance<EnhancedOccupationCard>();
        baker.Initialize("パン職人", "OCCUPATION_003", OccupationType.Baker);
        baker.SetEffectDescription("穀物を食料に変換できる");
        availableOccupations.Add(baker);
        
        DebugLog("デフォルト職業カードを作成しました");
    }
    
    /// <summary>
    /// 職業カードをプレイする（メインメソッド）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="foodCost">食料コスト（基本2個）</param>
    /// <param name="maxCount">プレイできる最大カード数</param>
    public void PlayOccupation(Player player, int foodCost = 2, int maxCount = 1)
    {
        if (player == null)
        {
            DebugLog("プレイヤーが指定されていません");
            return;
        }
        
        DebugLog($"=== {player.playerName}の職業カードプレイ開始 ===");
        DebugLog($"食料コスト: {foodCost}個");
        DebugLog($"最大プレイ可能枚数: {maxCount}枚");
        
        // 手札の職業カードを取得
        var handOccupations = GetHandOccupations(player);
        
        if (handOccupations.Count == 0)
        {
            DebugLog($"{player.playerName}は手札に職業カードがありません");
            ShowNoOccupationCardsMessage(player);
            OnNoOccupationCards?.Invoke(player);
            return;
        }
        
        // 食料の可用性をチェック
        if (!CanPlayOccupation(player, foodCost))
        {
            DebugLog($"{player.playerName}は食料が不足しています（必要: {foodCost}個, 所持: {player.GetResource(ResourceType.Food)}個）");
            ShowInsufficientFoodMessage(player, foodCost);
            OnInsufficientFood?.Invoke(player, foodCost);
            return;
        }
        
        // プレイヤーに職業カード選択肢を提示
        ShowOccupationSelectionToPlayer(player, handOccupations, foodCost, maxCount);
    }
    
    /// <summary>
    /// 手札の職業カードを取得
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>手札の職業カードリスト</returns>
    public List<EnhancedOccupationCard> GetHandOccupations(Player player)
    {
        if (player == null) return new List<EnhancedOccupationCard>();
        
        // プレイヤーの手札から職業カードを取得
        var handOccupations = player.GetOccupationCards().Cast<EnhancedOccupationCard>().ToList();
        
        DebugLog($"{player.playerName}の手札職業カード: {handOccupations.Count}枚");
        
        return handOccupations;
    }
    
    /// <summary>
    /// 職業カードをプレイできるかチェック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="foodCost">必要な食料コスト</param>
    /// <returns>プレイ可能な場合true</returns>
    public bool CanPlayOccupation(Player player, int foodCost)
    {
        if (player == null) return false;
        
        // 食料の可用性チェック
        return player.GetResource(ResourceType.Food) >= foodCost;
    }
    
    /// <summary>
    /// プレイヤーに職業カード選択肢を表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="handOccupations">手札の職業カード</param>
    /// <param name="foodCost">食料コスト</param>
    /// <param name="maxCount">最大選択可能数</param>
    private void ShowOccupationSelectionToPlayer(Player player, List<EnhancedOccupationCard> handOccupations, int foodCost, int maxCount)
    {
        DebugLog($"=== {player.playerName}の職業カード選択 ===");
        DebugLog($"手札職業カード数: {handOccupations.Count}枚");
        DebugLog($"食料コスト: {foodCost}個");
        DebugLog($"最大選択可能数: {maxCount}枚");
        
        // カード一覧を表示
        for (int i = 0; i < handOccupations.Count; i++)
        {
            var card = handOccupations[i];
            DebugLog($"  [{i + 1}] {card.cardName}");
            DebugLog($"      効果: {card.GetEffectDescription()}");
            DebugLog($"      状態: プレイ可能");
        }
        
        // 食料コスト情報を表示
        DebugLog($"現在の食料: {player.GetResource(ResourceType.Food)}個");
        DebugLog($"必要な食料: {foodCost}個");
        DebugLog($"支払い後の食料: {player.GetResource(ResourceType.Food) - foodCost}個");
        
        // UIシステムに職業カード選択開始を通知
        OnOccupationSelectionStarted?.Invoke(player, handOccupations);
        
        DebugLog("プレイヤーの選択を待機中... (UIが実装されるまでは手動で ExecuteSelectedOccupation を呼び出してください)");
    }
    
    /// <summary>
    /// プレイヤーが選択した職業カードを実行
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="selectedCard">選択されたカード</param>
    /// <param name="foodCost">食料コスト</param>
    /// <returns>実行に成功した場合true</returns>
    public bool ExecuteSelectedOccupation(Player player, EnhancedOccupationCard selectedCard, int foodCost)
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
        
        DebugLog($"=== 選択された職業カードの実行 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"選択カード: {selectedCard.cardName}");
        DebugLog($"食料コスト: {foodCost}個");
        
        // 最終的な実行条件を確認
        if (!CanPlayOccupation(player, foodCost))
        {
            DebugLog($"❌ 食料が不足しています（必要: {foodCost}個, 所持: {player.GetResource(ResourceType.Food)}個）");
            return false;
        }
        
        // 手札にカードがあるかチェック
        var handOccupations = GetHandOccupations(player);
        if (!handOccupations.Contains(selectedCard))
        {
            DebugLog($"❌ 選択されたカード「{selectedCard.cardName}」は手札にありません");
            return false;
        }
        
        // 職業カードを実行
        if (ExecuteOccupationPlay(player, selectedCard, foodCost))
        {
            // 統計更新とイベント発火
            totalOccupationsPlayed++;
            OnOccupationPlayed?.Invoke(player, selectedCard);
            
            DebugLog($"✅ {player.playerName}が職業「{selectedCard.cardName}」をプレイしました");
            ShowPlayedOccupationSummary(player, selectedCard, foodCost);
            return true;
        }
        else
        {
            DebugLog($"❌ {player.playerName}の職業「{selectedCard.cardName}」のプレイに失敗しました");
            return false;
        }
    }
    
    /// <summary>
    /// 職業カードの実際のプレイを実行
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">プレイする職業カード</param>
    /// <param name="foodCost">食料コスト</param>
    /// <returns>プレイに成功した場合true</returns>
    private bool ExecuteOccupationPlay(Player player, EnhancedOccupationCard card, int foodCost)
    {
        try
        {
            // 1. 食料コストを支払う
            if (foodCost > 0)
            {
                player.SpendResource(ResourceType.Food, foodCost);
                DebugLog($"  食料コスト支払い: {foodCost}個");
            }
            else
            {
                DebugLog($"  食料コスト: 無料");
            }
            
            // 2. 職業カードをプレイ
            bool playSuccess = card.Play(player);
            
            if (playSuccess)
            {
                // 3. 手札から職業カードを除去
                player.RemoveOccupationCardFromHand(card);
                
                // 4. プレイヤーに職業を登録
                player.AddOccupation(card);
                
                // 5. 職業カードのトリガー効果を登録
                if (cardTriggerManager != null)
                {
                    cardTriggerManager.RegisterOccupationTriggers(player, card);
                }
                
                return true;
            }
            else
            {
                // プレイに失敗した場合は食料を返却
                if (foodCost > 0)
                {
                    player.AddResource(ResourceType.Food, foodCost);
                }
                return false;
            }
        }
        catch (System.Exception ex)
        {
            DebugLog($"職業カードプレイ中にエラーが発生しました: {ex.Message}");
            
            // エラー時は食料を返却
            if (foodCost > 0)
            {
                player.AddResource(ResourceType.Food, foodCost);
            }
            return false;
        }
    }
    
    /// <summary>
    /// 職業カードがない場合のメッセージ表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    private void ShowNoOccupationCardsMessage(Player player)
    {
        DebugLog($"=== 職業カードプレイ不可能 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"理由: 手札に職業カードがありません");
        DebugLog($"手札の職業カード数: {GetHandOccupations(player).Count}枚");
        DebugLog($"=== 分析完了 ===");
    }
    
    /// <summary>
    /// 食料不足時のメッセージ表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="foodCost">必要な食料コスト</param>
    private void ShowInsufficientFoodMessage(Player player, int foodCost)
    {
        DebugLog($"=== 食料不足 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"必要な食料: {foodCost}個");
        DebugLog($"所持している食料: {player.GetResource(ResourceType.Food)}個");
        DebugLog($"不足している食料: {foodCost - player.GetResource(ResourceType.Food)}個");
        DebugLog($"=== 分析完了 ===");
    }
    
    /// <summary>
    /// プレイした職業カードのサマリーを表示
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="card">プレイした職業カード</param>
    /// <param name="foodCost">支払った食料コスト</param>
    private void ShowPlayedOccupationSummary(Player player, EnhancedOccupationCard card, int foodCost)
    {
        DebugLog($"=== 職業カードプレイ完了 ===");
        DebugLog($"プレイヤー: {player.playerName}");
        DebugLog($"職業カード: {card.cardName}");
        DebugLog($"効果: {card.GetEffectDescription()}");
        DebugLog($"支払った食料: {foodCost}個");
        DebugLog($"残り食料: {player.GetResource(ResourceType.Food)}個");
        DebugLog($"総職業数: {player.GetOccupations().Count}個");
        DebugLog($"=== 完了 ===");
    }
    
    /// <summary>
    /// 職業カード選択をキャンセル
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    public void CancelOccupationSelection(Player player)
    {
        if (player == null) return;
        
        DebugLog($"{player.playerName}が職業カード選択をキャンセルしました");
        OnOccupationSelectionCancelled?.Invoke(player);
    }
    
    /// <summary>
    /// 職業カードシステムの統計を取得
    /// </summary>
    /// <returns>統計情報</returns>
    public OccupationSystemStats GetSystemStats()
    {
        return new OccupationSystemStats
        {
            totalOccupationsPlayed = totalOccupationsPlayed,
            availableOccupationsCount = availableOccupations.Count
        };
    }
    
    /// <summary>
    /// デバッグ用：職業カードシステムの状態を表示
    /// </summary>
    [ContextMenu("Show Occupation System Status")]
    public void ShowOccupationSystemStatus()
    {
        DebugLog("=== 職業カードシステム状態 ===");
        DebugLog($"利用可能な職業カード: {availableOccupations.Count}枚");
        DebugLog($"総プレイ数: {totalOccupationsPlayed}枚");
        DebugLog($"ステータス: 完全実装");
        
        DebugLog("利用可能な職業カード:");
        foreach (var occupation in availableOccupations)
        {
            DebugLog($"  - {occupation.cardName}: {occupation.GetEffectDescription()}");
        }
        DebugLog("=== 状態確認完了 ===");
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[EnhancedOccupationManager] {message}");
        }
    }
}

/// <summary>
/// 職業カードシステムの統計情報
/// </summary>
[System.Serializable]
public class OccupationSystemStats
{
    public int totalOccupationsPlayed;
    public int availableOccupationsCount;
}