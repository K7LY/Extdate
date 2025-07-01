using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TriggerableCard
{
    public EnhancedCard card;
    public List<CardEffect> triggerableEffects;
    public Player owner;
    
    public TriggerableCard(EnhancedCard card, List<CardEffect> effects, Player owner)
    {
        this.card = card;
        this.triggerableEffects = effects;
        this.owner = owner;
    }
}

[System.Serializable]
public class TriggerContext
{
    public OccupationTrigger triggerType;
    public Player triggeringPlayer;
    public object contextData;
    public string triggerCondition;
    
    public TriggerContext(OccupationTrigger triggerType, Player player, object context = null, string condition = "")
    {
        this.triggerType = triggerType;
        this.triggeringPlayer = player;
        this.contextData = context;
        this.triggerCondition = condition;
    }
}

public class CardTriggerManager : MonoBehaviour
{
    [Header("デバッグ設定")]
    public bool enableDebugLogging = true;
    public bool showTriggerDetails = false;
    
    [Header("トリガー統計")]
    public int totalTriggersProcessed = 0;
    public int totalEffectsExecuted = 0;
    
    [Header("自動登録システム")]
    public bool enableAutoRegistration = true;
    public int registeredCardsCount = 0;
    
    // プレイ済みカードの管理
    private Dictionary<Player, List<EnhancedCard>> playerCards = new Dictionary<Player, List<EnhancedCard>>();
    private List<EnhancedCard> allRegisteredCards = new List<EnhancedCard>();
    
    private static CardTriggerManager instance;
    public static CardTriggerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CardTriggerManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("CardTriggerManager");
                    instance = go.AddComponent<CardTriggerManager>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCardRegistry();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeCardRegistry()
    {
        playerCards = new Dictionary<Player, List<EnhancedCard>>();
        allRegisteredCards = new List<EnhancedCard>();
        registeredCardsCount = 0;
        LogDebug("カード登録システムを初期化しました");
    }
    
    /// <summary>
    /// カードを自動登録システムに追加
    /// </summary>
    public void RegisterCard(Player player, EnhancedCard card)
    {
        if (!enableAutoRegistration) return;
        
        if (player == null || card == null)
        {
            LogDebug("プレイヤーまたはカードがnullのため、登録をスキップします");
            return;
        }
        
        // プレイヤーのカードリストを初期化
        if (!playerCards.ContainsKey(player))
        {
            playerCards[player] = new List<EnhancedCard>();
        }
        
        // 重複チェック
        if (!playerCards[player].Contains(card))
        {
            playerCards[player].Add(card);
            allRegisteredCards.Add(card);
            registeredCardsCount++;
            
            LogDebug($"カード「{card.cardName}」をプレイヤー「{player.playerName}」に自動登録しました");
        }
    }
    
    /// <summary>
    /// カードを自動登録システムから削除
    /// </summary>
    public void UnregisterCard(Player player, EnhancedCard card)
    {
        if (player == null || card == null) return;
        
        if (playerCards.ContainsKey(player))
        {
            if (playerCards[player].Remove(card))
            {
                allRegisteredCards.Remove(card);
                registeredCardsCount--;
                LogDebug($"カード「{card.cardName}」をプレイヤー「{player.playerName}」から削除しました");
            }
        }
    }
    
    /// <summary>
    /// プレイヤーの全カードを削除
    /// </summary>
    public void UnregisterAllPlayerCards(Player player)
    {
        if (playerCards.ContainsKey(player))
        {
            var playerCardList = playerCards[player];
            foreach (var card in playerCardList)
            {
                allRegisteredCards.Remove(card);
                registeredCardsCount--;
            }
            playerCards[player].Clear();
            LogDebug($"プレイヤー「{player.playerName}」の全カードを削除しました");
        }
    }
    
    /// <summary>
    /// 登録済みカードから検索（高速版）
    /// </summary>
    public List<TriggerableCard> FindTriggerableCardsFromRegistry(Player player, OccupationTrigger triggerType, object context = null, string condition = "")
    {
        var triggerableCards = new List<TriggerableCard>();
        
        if (player == null)
        {
            LogDebug("プレイヤーがnullのため、登録済みカード検索をスキップします");
            return triggerableCards;
        }
        
        // 登録済みカードから検索
        if (playerCards.ContainsKey(player))
        {
            foreach (var card in playerCards[player])
            {
                var triggerableEffects = GetTriggerableEffects(card, triggerType, player, context, condition);
                if (triggerableEffects.Count > 0)
                {
                    triggerableCards.Add(new TriggerableCard(card, triggerableEffects, player));
                }
            }
        }
        
        LogDebug($"登録済みカードから プレイヤー{player.playerName}で{triggerType}トリガーに該当するカード: {triggerableCards.Count}枚");
        
        return triggerableCards;
    }
    
    /// <summary>
    /// 指定されたプレイヤーの全カードから、特定トリガーで発動可能なカードを検索
    /// </summary>
    public List<TriggerableCard> FindTriggerableCards(Player player, OccupationTrigger triggerType, object context = null, string condition = "")
    {
        // 登録システムが有効な場合は高速検索を使用
        if (enableAutoRegistration)
        {
            return FindTriggerableCardsFromRegistry(player, triggerType, context, condition);
        }
        
        // 従来の検索方法（登録システム無効時）
        var triggerableCards = new List<TriggerableCard>();
        
        if (player == null)
        {
            LogDebug("プレイヤーがnullのため、トリガー検索をスキップします");
            return triggerableCards;
        }
        
        // 職業カードから検索
        var occupations = player.GetOccupations();
        if (occupations != null)
        {
            foreach (var occupation in occupations)
            {
                var enhancedOccupation = occupation as EnhancedOccupationCard;
                if (enhancedOccupation != null)
                {
                    var triggerableEffects = GetTriggerableEffects(enhancedOccupation, triggerType, player, context, condition);
                    if (triggerableEffects.Count > 0)
                    {
                        triggerableCards.Add(new TriggerableCard(enhancedOccupation, triggerableEffects, player));
                    }
                }
            }
        }
        
        // 進歩カードから検索
        var improvements = player.GetImprovements();
        if (improvements != null)
        {
            foreach (var improvement in improvements)
            {
                var enhancedImprovement = improvement as EnhancedImprovementCard;
                if (enhancedImprovement != null)
                {
                    var triggerableEffects = GetTriggerableEffects(enhancedImprovement, triggerType, player, context, condition);
                    if (triggerableEffects.Count > 0)
                    {
                        triggerableCards.Add(new TriggerableCard(enhancedImprovement, triggerableEffects, player));
                    }
                }
            }
        }
        
        LogDebug($"プレイヤー{player.playerName}で{triggerType}トリガーに該当するカード: {triggerableCards.Count}枚");
        
        return triggerableCards;
    }
    
    /// <summary>
    /// 全プレイヤーから特定トリガーで発動可能なカードを検索
    /// </summary>
    public List<TriggerableCard> FindTriggerableCardsAllPlayers(List<Player> players, OccupationTrigger triggerType, object context = null, string condition = "")
    {
        var allTriggerableCards = new List<TriggerableCard>();
        
        if (players == null || players.Count == 0)
        {
            LogDebug("プレイヤーリストが空のため、トリガー検索をスキップします");
            return allTriggerableCards;
        }
        
        foreach (var player in players)
        {
            var playerTriggerableCards = FindTriggerableCards(player, triggerType, context, condition);
            allTriggerableCards.AddRange(playerTriggerableCards);
        }
        
        LogDebug($"全プレイヤーで{triggerType}トリガーに該当するカード: {allTriggerableCards.Count}枚");
        
        return allTriggerableCards;
    }
    
    /// <summary>
    /// 特定カードから発動可能な効果を取得
    /// </summary>
    private List<CardEffect> GetTriggerableEffects(EnhancedCard card, OccupationTrigger triggerType, Player player, object context, string condition)
    {
        var triggerableEffects = new List<CardEffect>();
        
        if (card == null || card.effects == null) return triggerableEffects;
        
        foreach (var effect in card.effects)
        {
            // トリガータイプが一致するかチェック
            if (effect.triggerType != triggerType) continue;
            
            // 発動可能かチェック（使用回数制限など）
            if (!effect.CanActivate()) continue;
            
            // トリガー条件をチェック
            if (!CheckTriggerCondition(card, effect, player, context, condition)) continue;
            
            triggerableEffects.Add(effect);
        }
        
        return triggerableEffects;
    }
    
    /// <summary>
    /// トリガー条件をチェック
    /// </summary>
    private bool CheckTriggerCondition(EnhancedCard card, CardEffect effect, Player player, object context, string condition)
    {
        // 条件文字列が指定されている場合
        if (!string.IsNullOrEmpty(condition) && !string.IsNullOrEmpty(effect.triggerCondition))
        {
            // 条件文字列が一致するかチェック
            if (!effect.triggerCondition.Contains(condition) && !condition.Contains(effect.triggerCondition))
            {
                return false;
            }
        }
        
        // カード固有の条件チェック（職業カードまたは進歩カード）
        if (card is EnhancedOccupationCard occupationCard)
        {
            return CheckOccupationTriggerCondition(occupationCard, effect, player, context);
        }
        else if (card is EnhancedImprovementCard improvementCard)
        {
            return CheckImprovementTriggerCondition(improvementCard, effect, player, context);
        }
        
        return true; // デフォルトは発動可能
    }
    
    /// <summary>
    /// 職業カードのトリガー条件チェック
    /// </summary>
    private bool CheckOccupationTriggerCondition(EnhancedOccupationCard card, CardEffect effect, Player player, object context)
    {
        switch (effect.triggerType)
        {
            case OccupationTrigger.OnAction:
                return CheckActionTrigger(effect, context);
            case OccupationTrigger.OnHarvest:
                return true; // 収穫時は基本的に発動
            case OccupationTrigger.OnBreeding:
                return CheckBreedingTrigger(effect, context);
            case OccupationTrigger.OnTurnEnd:
                return true; // ターン終了時は基本的に発動
            default:
                return true;
        }
    }
    
    /// <summary>
    /// 進歩カードのトリガー条件チェック
    /// </summary>
    private bool CheckImprovementTriggerCondition(EnhancedImprovementCard card, CardEffect effect, Player player, object context)
    {
        switch (effect.triggerType)
        {
            case OccupationTrigger.OnAction:
                return CheckActionTrigger(effect, context);
            case OccupationTrigger.OnHarvest:
                return true; // 収穫時は基本的に発動
            case OccupationTrigger.Passive:
                return true; // パッシブ効果は常に有効
            default:
                return true;
        }
    }
    
    /// <summary>
    /// アクショントリガーの条件チェック
    /// </summary>
    private bool CheckActionTrigger(CardEffect effect, object context)
    {
        if (context is ActionSpace actionSpace)
        {
            if (!string.IsNullOrEmpty(effect.triggerCondition))
            {
                return actionSpace.actionName.Contains(effect.triggerCondition) ||
                       actionSpace.actionType.ToString().Contains(effect.triggerCondition);
            }
        }
        return true;
    }
    
    /// <summary>
    /// 繁殖トリガーの条件チェック
    /// </summary>
    private bool CheckBreedingTrigger(CardEffect effect, object context)
    {
        // 繁殖の詳細情報があれば条件チェック可能
        return true;
    }
    
    /// <summary>
    /// トリガー可能なカードの効果を全て実行
    /// </summary>
    public void ExecuteTriggerableCards(List<TriggerableCard> triggerableCards, TriggerContext triggerContext)
    {
        if (triggerableCards == null || triggerableCards.Count == 0)
        {
            LogDebug("実行するトリガーカードがありません");
            return;
        }
        
        totalTriggersProcessed++;
        int effectsExecuted = 0;
        
        LogDebug($"=== トリガー実行開始: {triggerContext.triggerType} ===");
        
        foreach (var triggerableCard in triggerableCards)
        {
            LogDebug($"カード「{triggerableCard.card.cardName}」の効果を実行中...");
            
            foreach (var effect in triggerableCard.triggerableEffects)
            {
                try
                {
                    // 効果を実行
                    triggerableCard.card.TriggerEffect(triggerableCard.owner, triggerContext.triggerType, triggerContext.contextData);
                    effectsExecuted++;
                    
                    if (showTriggerDetails)
                    {
                        LogDebug($"  - 効果「{effect.effectDescription}」実行完了");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"効果実行エラー: {e.Message}");
                }
            }
        }
        
        totalEffectsExecuted += effectsExecuted;
        LogDebug($"=== トリガー実行完了: {effectsExecuted}個の効果を実行 ===");
    }
    
    /// <summary>
    /// 特定プレイヤーのトリガーを実行
    /// </summary>
    public void ExecutePlayerTrigger(Player player, OccupationTrigger triggerType, object context = null, string condition = "")
    {
        var triggerContext = new TriggerContext(triggerType, player, context, condition);
        var triggerableCards = FindTriggerableCards(player, triggerType, context, condition);
        ExecuteTriggerableCards(triggerableCards, triggerContext);
    }
    
    /// <summary>
    /// 全プレイヤーのトリガーを実行
    /// </summary>
    public void ExecuteAllPlayersTrigger(List<Player> players, OccupationTrigger triggerType, object context = null, string condition = "")
    {
        if (players == null || players.Count == 0) return;
        
        var triggerContext = new TriggerContext(triggerType, players[0], context, condition);
        var triggerableCards = FindTriggerableCardsAllPlayers(players, triggerType, context, condition);
        ExecuteTriggerableCards(triggerableCards, triggerContext);
    }
    
    /// <summary>
    /// トリガー可能なカードの情報を取得（デバッグ用）
    /// </summary>
    public string GetTriggerableCardsInfo(List<TriggerableCard> triggerableCards)
    {
        if (triggerableCards == null || triggerableCards.Count == 0)
        {
            return "トリガー可能なカードはありません";
        }
        
        var info = $"トリガー可能なカード: {triggerableCards.Count}枚\n";
        
        foreach (var triggerableCard in triggerableCards)
        {
            info += $"- {triggerableCard.card.cardName} (所有者: {triggerableCard.owner.playerName})\n";
            foreach (var effect in triggerableCard.triggerableEffects)
            {
                info += $"  └ {effect.effectDescription}\n";
            }
        }
        
        return info;
    }
    
    /// <summary>
    /// 統計情報をリセット
    /// </summary>
    public void ResetStatistics()
    {
        totalTriggersProcessed = 0;
        totalEffectsExecuted = 0;
    }
    
    /// <summary>
    /// 登録システムの統計情報を取得
    /// </summary>
    public string GetRegistrationStatistics()
    {
        var stats = $"=== カード登録システム統計 ===\n";
        stats += $"自動登録: {(enableAutoRegistration ? "有効" : "無効")}\n";
        stats += $"登録済みカード総数: {registeredCardsCount}\n";
        stats += $"登録プレイヤー数: {playerCards.Count}\n";
        
        foreach (var kvp in playerCards)
        {
            stats += $"- {kvp.Key.playerName}: {kvp.Value.Count}枚\n";
        }
        
        return stats;
    }
    
    /// <summary>
    /// 登録システムをクリア
    /// </summary>
    public void ClearRegistrationSystem()
    {
        playerCards.Clear();
        allRegisteredCards.Clear();
        registeredCardsCount = 0;
        LogDebug("登録システムをクリアしました");
    }
    
    /// <summary>
    /// プレイヤーの登録済みカード一覧を取得
    /// </summary>
    public List<EnhancedCard> GetRegisteredCards(Player player)
    {
        if (playerCards.ContainsKey(player))
        {
            return new List<EnhancedCard>(playerCards[player]);
        }
        return new List<EnhancedCard>();
    }
    
    /// <summary>
    /// デバッグログ出力
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[CardTriggerManager] {message}");
        }
    }
}