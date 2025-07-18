using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// イベント発生時にトリガー可能なカードの管理を行うクラス
/// </summary>
public class CardTriggerManager : MonoBehaviour
{
    [System.Serializable]
    public class TriggerableCard
    {
        public EnhancedCard card;
        public CardEffect effect;
        public Player owner;
        public bool canTrigger;
        public string triggerReason;
        
        public TriggerableCard(EnhancedCard card, CardEffect effect, Player owner, bool canTrigger, string reason = "")
        {
            this.card = card;
            this.effect = effect;
            this.owner = owner;
            this.canTrigger = canTrigger;
            this.triggerReason = reason;
        }
    }
    
    [System.Serializable]
    public class EventContext
    {
        public OccupationTrigger triggerType;
        public Player currentPlayer;
        public ActionSpace actionSpace;
        public ResourceType resourceType;
        public int resourceAmount;
        public object customData;
        
        public EventContext(OccupationTrigger trigger, Player player = null)
        {
            triggerType = trigger;
            currentPlayer = player;
        }
    }
    
    [System.Serializable]
    public class TakeEventContext : EventContext
    {
        public ResourceType resourceType;
        public int amount;
        public ActionSpace sourceActionSpace;
        public string takeMethod; // "action", "card_effect", "trade", etc.
        
        public TakeEventContext(Player player, ResourceType resource, int amt, ActionSpace source = null, string method = "action") 
            : base(OccupationTrigger.OnTake, player)
        {
            resourceType = resource;
            amount = amt;
            sourceActionSpace = source;
            takeMethod = method;
        }
    }
    
    [System.Serializable]
    public class ReceiveEventContext : EventContext
    {
        public ResourceType resourceType;
        public int amount;
        public Player sourcePlayer; // リソースの提供者（トレードの場合など）
        public string receiveMethod; // "direct", "trade", "card_effect", "passive", etc.
        
        public ReceiveEventContext(Player player, ResourceType resource, int amt, Player source = null, string method = "direct") 
            : base(OccupationTrigger.OnReceive, player)
        {
            resourceType = resource;
            amount = amt;
            sourcePlayer = source;
            receiveMethod = method;
        }
    }
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    
    /// <summary>
    /// 指定されたイベントでトリガー可能なカードの一覧を取得
    /// </summary>
    /// <param name="triggerType">トリガータイプ</param>
    /// <param name="context">イベントコンテキスト</param>
    /// <returns>トリガー可能なカードのリスト</returns>
    public List<TriggerableCard> GetTriggerableCards(OccupationTrigger triggerType, EventContext context = null)
    {
        List<TriggerableCard> triggerableCards = new List<TriggerableCard>();
        
        if (gameManager == null) return triggerableCards;
        
        // 全プレイヤーの出ているカードをチェック
        foreach (Player player in gameManager.GetPlayers())
        {
            // プレイ済みのEnhancedCardをチェック
            triggerableCards.AddRange(GetTriggerableCardsFromPlayer(player, triggerType, context));
        }
        
        return triggerableCards;
    }
    
    /// <summary>
    /// 特定のプレイヤーのトリガー可能なカードを取得
    /// </summary>
    private List<TriggerableCard> GetTriggerableCardsFromPlayer(Player player, OccupationTrigger triggerType, EventContext context)
    {
        List<TriggerableCard> playerTriggerableCards = new List<TriggerableCard>();
        
        // プレイ済みカードから EnhancedCard を取得
        List<EnhancedCard> enhancedCards = GetEnhancedCardsFromPlayer(player);
        
        foreach (EnhancedCard card in enhancedCards)
        {
            // 指定されたトリガータイプの効果を取得
            List<CardEffect> matchingEffects = card.GetEffectsByTrigger(triggerType);
            
            foreach (CardEffect effect in matchingEffects)
            {
                bool canTrigger = CanTriggerEffect(card, effect, player, context);
                string reason = GetTriggerReason(card, effect, player, context, canTrigger);
                
                playerTriggerableCards.Add(new TriggerableCard(card, effect, player, canTrigger, reason));
            }
        }
        
        return playerTriggerableCards;
    }
    
    /// <summary>
    /// プレイヤーの出ているEnhancedCardを取得
    /// </summary>
    private List<EnhancedCard> GetEnhancedCardsFromPlayer(Player player)
    {
        List<EnhancedCard> enhancedCards = new List<EnhancedCard>();
        
        // 職業カードから EnhancedCard を取得
        foreach (OccupationCard occupation in player.GetOccupations())
        {
            if (occupation is EnhancedCard enhancedOccupation)
            {
                enhancedCards.Add(enhancedOccupation);
            }
        }
        
        // 進歩カードから EnhancedCard を取得
        foreach (ImprovementCard improvement in player.GetImprovements())
        {
            if (improvement is EnhancedCard enhancedImprovement)
            {
                enhancedCards.Add(enhancedImprovement);
            }
        }
        
        // プレイ済みカードから EnhancedCard を取得
        foreach (Card card in player.GetPlayedCards())
        {
            if (card is EnhancedCard enhancedCard)
            {
                enhancedCards.Add(enhancedCard);
            }
        }
        
        return enhancedCards;
    }
    
    /// <summary>
    /// 効果がトリガー可能かどうかを判定
    /// </summary>
    private bool CanTriggerEffect(EnhancedCard card, CardEffect effect, Player player, EventContext context)
    {
        // 基本的な使用可能性チェック
        if (!effect.CanActivate())
        {
            return false;
        }
        
        // トリガー条件の詳細チェック
        if (!card.CheckTriggerCondition(effect, player, context))
        {
            return false;
        }
        
        // コンテキスト依存の条件チェック
        if (context != null)
        {
            // 特定プレイヤーのみの効果の場合
            if (context.currentPlayer != null && context.currentPlayer != player && 
                effect.specialEffectData != null && effect.specialEffectData.Contains("self_only"))
            {
                return false;
            }
            
            // アクション依存の効果の場合
            if (context.actionSpace != null && effect.triggerCondition != null &&
                !effect.triggerCondition.Contains(context.actionSpace.actionName))
            {
                return false;
            }
            
            // リソース依存の効果の場合
            if (context.resourceType != ResourceType.None && effect.resourceGain.Count > 0 &&
                !effect.resourceGain.ContainsKey(context.resourceType))
            {
                return false;
            }
            
            // OnTakeトリガーの特殊条件チェック
            if (context is TakeEventContext takeContext)
            {
                // 特定のリソースタイプのみに反応する効果
                if (effect.triggerCondition != null && !string.IsNullOrEmpty(effect.triggerCondition))
                {
                    if (!effect.triggerCondition.Contains(takeContext.resourceType.ToString()))
                    {
                        return false;
                    }
                }
                
                // 特定の取得方法のみに反応する効果
                if (effect.specialEffectData != null && effect.specialEffectData.Contains("take_method:"))
                {
                    string requiredMethod = effect.specialEffectData.Split(':')[1];
                    if (takeContext.takeMethod != requiredMethod)
                    {
                        return false;
                    }
                }
            }
            
            // OnReceiveトリガーの特殊条件チェック
            if (context is ReceiveEventContext receiveContext)
            {
                // 特定のリソースタイプのみに反応する効果
                if (effect.triggerCondition != null && !string.IsNullOrEmpty(effect.triggerCondition))
                {
                    if (!effect.triggerCondition.Contains(receiveContext.resourceType.ToString()))
                    {
                        return false;
                    }
                }
                
                // 最小受け取り量の条件チェック
                if (effect.specialEffectData != null && effect.specialEffectData.Contains("min_amount:"))
                {
                    string[] parts = effect.specialEffectData.Split(':');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int minAmount))
                    {
                        if (receiveContext.amount < minAmount)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// トリガー可能/不可能な理由を取得
    /// </summary>
    private string GetTriggerReason(EnhancedCard card, CardEffect effect, Player player, EventContext context, bool canTrigger)
    {
        if (canTrigger)
        {
            return $"効果「{effect.effectDescription}」がトリガー可能";
        }
        
        if (!effect.CanActivate())
        {
            if (effect.isOneTimeUse && effect.currentUses > 0)
            {
                return "一度だけ使用可能な効果が既に使用済み";
            }
            
            if (effect.maxUsesPerRound != -1 && effect.currentUses >= effect.maxUsesPerRound)
            {
                return $"ラウンド使用制限に達している（{effect.currentUses}/{effect.maxUsesPerRound}）";
            }
        }
        
        if (context != null && context.currentPlayer != null && context.currentPlayer != player &&
            effect.specialEffectData != null && effect.specialEffectData.Contains("self_only"))
        {
            return "自分のみ対象の効果のため他プレイヤーには発動しない";
        }
        
        return "トリガー条件を満たしていない";
    }
    
    /// <summary>
    /// 特定のプレイヤーのトリガー可能なカードのみを取得
    /// </summary>
    public List<TriggerableCard> GetTriggerableCardsForPlayer(Player player, OccupationTrigger triggerType, EventContext context = null)
    {
        if (context == null)
        {
            context = new EventContext(triggerType, player);
        }
        
        return GetTriggerableCardsFromPlayer(player, triggerType, context);
    }
    
    /// <summary>
    /// 実際にトリガー可能なカードのみをフィルタリング
    /// </summary>
    public List<TriggerableCard> GetActiveTriggerableCards(OccupationTrigger triggerType, EventContext context = null)
    {
        return GetTriggerableCards(triggerType, context)
            .Where(tc => tc.canTrigger)
            .ToList();
    }
    
    /// <summary>
    /// カード効果を実際に実行
    /// </summary>
    public void ExecuteTriggerableCard(TriggerableCard triggerableCard, EventContext context = null)
    {
        if (!triggerableCard.canTrigger)
        {
            Debug.LogWarning($"トリガー不可能なカード効果の実行を試行: {triggerableCard.triggerReason}");
            return;
        }
        
        triggerableCard.card.TriggerEffect(triggerableCard.owner, context?.triggerType ?? OccupationTrigger.Immediate, context);
        
        Debug.Log($"{triggerableCard.owner.playerName}の「{triggerableCard.card.cardName}」の効果「{triggerableCard.effect.effectDescription}」が発動しました");
    }
    
    /// <summary>
    /// 全てのトリガー可能なカードを一括実行
    /// </summary>
    public void ExecuteAllTriggerableCards(OccupationTrigger triggerType, EventContext context = null)
    {
        List<TriggerableCard> triggerableCards = GetActiveTriggerableCards(triggerType, context);
        
        foreach (TriggerableCard triggerableCard in triggerableCards)
        {
            ExecuteTriggerableCard(triggerableCard, context);
        }
        
        Debug.Log($"{triggerType}イベントで{triggerableCards.Count}個のカード効果が発動しました");
    }
    
    /// <summary>
    /// デバッグ用：トリガー可能なカードの情報を表示
    /// </summary>
    public void DebugPrintTriggerableCards(OccupationTrigger triggerType, EventContext context = null)
    {
        List<TriggerableCard> triggerableCards = GetTriggerableCards(triggerType, context);
        
        Debug.Log($"=== {triggerType} イベントのトリガー可能カード一覧 ===");
        
        foreach (TriggerableCard tc in triggerableCards)
        {
            string status = tc.canTrigger ? "[可能]" : "[不可]";
            Debug.Log($"{status} {tc.owner.playerName}: {tc.card.cardName} - {tc.effect.effectDescription} ({tc.triggerReason})");
        }
        
        Debug.Log($"総数: {triggerableCards.Count}個（実行可能: {triggerableCards.Count(tc => tc.canTrigger)}個）");
    }
    
    /// <summary>
    /// 新しく追加されたカードのトリガー可能な効果を確認し、ログに出力
    /// </summary>
    public void AnalyzeNewCard(EnhancedCard card, Player owner)
    {
        Debug.Log($"=== 新しいカード「{card.cardName}」の効果分析 ===");
        
        foreach (var effect in card.effects)
        {
            EventContext context = new EventContext(effect.triggerType, owner);
            bool canTrigger = CanTriggerEffect(card, effect, owner, context);
            string reason = GetTriggerReason(card, effect, owner, context, canTrigger);
            
            string status = canTrigger ? "[利用可能]" : "[条件待ち]";
            Debug.Log($"{status} {effect.triggerType}トリガー: {effect.effectDescription} ({reason})");
        }
    }
    
    /// <summary>
    /// 職業カードのトリガーを登録（プレースホルダー）
    /// </summary>
    /// <param name="player">プレイヤー</param>
    /// <param name="occupation">職業カード</param>
    public void RegisterOccupationTriggers(Player player, OccupationCard occupation)
    {
        // 現在の実装では、職業カードは Player.AddOccupation() で自動的に管理されるため、
        // 特別な登録処理は不要です。
        // このメソッドは将来的な拡張のためのプレースホルダーです。
        
        Debug.Log($"職業カード「{occupation.cardName}」のトリガーを{player.playerName}に登録しました");
        
        // 職業カードの効果を分析表示
        if (occupation is EnhancedCard enhancedOccupation)
        {
            AnalyzeNewCard(enhancedOccupation, player);
        }
    }
    
    /// <summary>
    /// 全プレイヤーの現在のトリガー可能カード状況をサマリー表示
    /// </summary>
    public void DebugPrintTriggerSummary()
    {
        if (gameManager == null) return;
        
        Debug.Log("=== 全プレイヤーのトリガー可能カード サマリー ===");
        
        foreach (Player player in gameManager.GetPlayers())
        {
            List<EnhancedCard> enhancedCards = GetEnhancedCardsFromPlayer(player);
            int totalEffects = 0;
            int availableEffects = 0;
            
            foreach (EnhancedCard card in enhancedCards)
            {
                foreach (var effect in card.effects)
                {
                    totalEffects++;
                    EventContext context = new EventContext(effect.triggerType, player);
                    if (CanTriggerEffect(card, effect, player, context))
                    {
                        availableEffects++;
                    }
                }
            }
            
            Debug.Log($"🎮 {player.playerName}: {enhancedCards.Count}枚のカード, {totalEffects}個の効果 (利用可能: {availableEffects}個)");
        }
    }
}