using UnityEngine;
using System.Collections.Generic;

public class EnhancedCardFactory : MonoBehaviour
{
    [Header("デバッグ用テストボタン")]
    public bool createTestCards = false;
    
    private void Start()
    {
        if (createTestCards)
        {
            CreateSampleCards();
        }
    }
    
    public static EnhancedOccupationCard CreateSampleOccupationCard(string cardName, string cardID, OccupationType occupationType)
    {
        var card = ScriptableObject.CreateInstance<EnhancedOccupationCard>();
        
        // 基本情報
        card.cardName = cardName;
        card.cardID = cardID;
        card.displayDescription = $"{cardName}の職業カードです。";
        card.occupationType = occupationType;
        
        // タグ追加
        card.AddTag("職業", "職業カードを示すタグ");
        card.AddTag(occupationType.ToString(), $"{occupationType}系統の職業");
        
        // 効果を追加
        var immediateEffect = new CardEffect
        {
            effectID = $"{cardID}_immediate",
            effectDescription = "カードプレイ時の即座効果",
            effectType = CardEffectType.ResourceModification,
            triggerType = OccupationTrigger.Immediate
        };
        
        // 職業タイプ別の効果設定
        switch (occupationType)
        {
            case OccupationType.Farmer:
                immediateEffect.resourceGain.Add(ResourceType.Grain, 1);
                immediateEffect.specialEffectData = "sowing_bonus";
                card.AddTag("農業", "農業関連の職業");
                break;
                
            case OccupationType.Carpenter:
                immediateEffect.resourceGain.Add(ResourceType.Wood, 2);
                immediateEffect.specialEffectData = "building_discount";
                card.AddTag("建築", "建築関連の職業");
                break;
                
            case OccupationType.Baker:
                immediateEffect.resourceGain.Add(ResourceType.Food, 2);
                immediateEffect.specialEffectData = "cooking_efficiency";
                card.AddTag("料理", "料理関連の職業");
                break;
                
            case OccupationType.Fisherman:
                immediateEffect.resourceGain.Add(ResourceType.Food, 1);
                immediateEffect.specialEffectData = "fishing_bonus";
                card.AddTag("食料", "食料獲得関連の職業");
                break;
                
            case OccupationType.Shepherd:
                immediateEffect.resourceGain.Add(ResourceType.Sheep, 1);
                immediateEffect.specialEffectData = "sheep_bonus";
                card.AddTag("牧畜", "牧畜関連の職業");
                break;
        }
        
        card.AddEffect(immediateEffect);
        
        // 収穫時効果も追加
        var harvestEffect = new CardEffect
        {
            effectID = $"{cardID}_harvest",
            effectDescription = "収穫時の効果",
            effectType = CardEffectType.ResourceModification,
            triggerType = OccupationTrigger.OnHarvest,
            maxUsesPerRound = 1
        };
        
        harvestEffect.resourceGain.Add(ResourceType.Food, 1);
        card.AddEffect(harvestEffect);
        
        return card;
    }
    
    public static EnhancedImprovementCard CreateSampleImprovementCard(string cardName, string cardID, ImprovementCategory category)
    {
        var card = ScriptableObject.CreateInstance<EnhancedImprovementCard>();
        
        // 基本情報
        card.cardName = cardName;
        card.cardID = cardID;
        card.displayDescription = $"{cardName}の進歩カードです。";
        card.category = category;
        card.victoryPointsWorth = category == ImprovementCategory.Major ? 2 : 1;
        
        // タグ追加
        card.AddTag("進歩", "進歩カードを示すタグ");
        card.AddTag(category.ToString(), $"{category}進歩");
        
        // プレイコスト設定
        var playCost = new PlayCost
        {
            costID = $"{cardID}_cost",
            costDescription = category == ImprovementCategory.Major ? "大きな進歩のコスト" : "小さな進歩のコスト"
        };
        
        if (category == ImprovementCategory.Major)
        {
            playCost.resourceCosts.Add(ResourceType.Clay, 2);
            playCost.resourceCosts.Add(ResourceType.Stone, 1);
        }
        else
        {
            playCost.resourceCosts.Add(ResourceType.Wood, 1);
        }
        
        card.AddPlayCost(playCost);
        
        // カードタイプ別の設定
        if (cardName.Contains("かまど") || cardName.Contains("料理"))
        {
            card.improvementType = ImprovementType.CookingFacility;
            card.canCookGrain = true;
            card.grainToFoodRatio = category == ImprovementCategory.Major ? 3 : 2;
            card.AddTag("料理設備", "料理関連の設備");
            
            var cookingEffect = new CardEffect
            {
                effectID = $"{cardID}_cooking",
                effectDescription = "料理効果",
                effectType = CardEffectType.SpecialAbility,
                triggerType = OccupationTrigger.OnAction,
                specialEffectData = "cooking_conversion",
                triggerCondition = "料理"
            };
            card.AddEffect(cookingEffect);
        }
        else if (cardName.Contains("貯蔵") || cardName.Contains("バスケット"))
        {
            card.improvementType = ImprovementType.StorageFacility;
            card.storageTypes.Add(ResourceType.Grain);
            card.storageTypes.Add(ResourceType.Vegetable);
            card.storageCapacity = 3;
            card.AddTag("貯蔵設備", "貯蔵関連の設備");
            
            var storageEffect = new CardEffect
            {
                effectID = $"{cardID}_storage",
                effectDescription = "貯蔵効果",
                effectType = CardEffectType.PassiveEffect,
                triggerType = OccupationTrigger.Passive,
                specialEffectData = "storage_bonus"
            };
            card.AddEffect(storageEffect);
        }
        else
        {
            card.improvementType = ImprovementType.SpecialImprovement;
            card.AddTag("特殊", "特殊効果を持つ進歩");
            
            var specialEffect = new CardEffect
            {
                effectID = $"{cardID}_special",
                effectDescription = "特殊効果",
                effectType = CardEffectType.VictoryPointModification,
                triggerType = OccupationTrigger.Immediate,
                victoryPointModifier = 1
            };
            card.AddEffect(specialEffect);
        }
        
        return card;
    }
    
    public void CreateSampleCards()
    {
        Debug.Log("=== 拡張カードシステムのサンプルカード作成開始 ===");
        
        // サンプル職業カード作成
        var farmer = CreateSampleOccupationCard("農夫", "OCC_001", OccupationType.Farmer);
        var carpenter = CreateSampleOccupationCard("大工", "OCC_002", OccupationType.Carpenter);
        var baker = CreateSampleOccupationCard("パン職人", "OCC_003", OccupationType.Baker);
        
        Debug.Log($"職業カード作成完了:");
        Debug.Log(farmer.GetCardInfo());
        Debug.Log(carpenter.GetCardInfo());
        Debug.Log(baker.GetCardInfo());
        
        // サンプル進歩カード作成
        var oven = CreateSampleImprovementCard("かまど", "IMP_001", ImprovementCategory.Minor);
        var storage = CreateSampleImprovementCard("バスケット", "IMP_002", ImprovementCategory.Minor);
        var well = CreateSampleImprovementCard("井戸", "IMP_003", ImprovementCategory.Major);
        
        Debug.Log($"\n進歩カード作成完了:");
        Debug.Log(oven.GetCardInfo());
        Debug.Log(storage.GetCardInfo());
        Debug.Log(well.GetCardInfo());
        
        Debug.Log("=== 拡張カードシステムのサンプルカード作成完了 ===");
    }
    
    // 特定の効果を持つカードを作成するヘルパーメソッド群
    public static CardEffect CreateResourceGainEffect(string effectID, ResourceType resourceType, int amount, OccupationTrigger trigger = OccupationTrigger.Immediate)
    {
        var effect = new CardEffect
        {
            effectID = effectID,
            effectDescription = $"{resourceType}を{amount}個獲得",
            effectType = CardEffectType.ResourceModification,
            triggerType = trigger
        };
        effect.resourceGain.Add(resourceType, amount);
        return effect;
    }
    
    public static CardEffect CreateVictoryPointEffect(string effectID, int points, OccupationTrigger trigger = OccupationTrigger.Immediate)
    {
        return new CardEffect
        {
            effectID = effectID,
            effectDescription = $"勝利点{points}を獲得",
            effectType = CardEffectType.VictoryPointModification,
            triggerType = trigger,
            victoryPointModifier = points
        };
    }
    
    public static CardTag CreateTag(string tagName, string description = "")
    {
        return new CardTag(tagName, description);
    }
    
    public static PlayCondition CreateResourceCondition(string conditionID, ResourceType resourceType, int amount)
    {
        var condition = new PlayCondition
        {
            conditionID = conditionID,
            conditionDescription = $"{resourceType}を{amount}個以上所持"
        };
        condition.requiredResources.Add(resourceType, amount);
        return condition;
    }
    
    public static PlayCost CreateResourceCost(string costID, ResourceType resourceType, int amount)
    {
        var cost = new PlayCost
        {
            costID = costID,
            costDescription = $"{resourceType}を{amount}個支払う"
        };
        cost.resourceCosts.Add(resourceType, amount);
        return cost;
    }
    
    /// <summary>
    /// OnTakeトリガーを使用した職業カードを作成
    /// </summary>
    public static EnhancedOccupationCard CreateTakeTriggeredOccupation(string cardName, string cardID, OccupationType occupationType, ResourceType triggerResource)
    {
        var card = CreateSampleOccupationCard(cardName, cardID, occupationType);
        
        // OnTakeトリガー効果を追加
        var takeEffect = new CardEffect
        {
            effectID = $"{cardID}_take",
            effectDescription = $"{triggerResource}取得時に追加効果",
            effectType = CardEffectType.ResourceModification,
            triggerType = OccupationTrigger.OnTake,
            triggerCondition = triggerResource.ToString(), // 特定リソースのみに反応
            maxUsesPerRound = 3 // 1ラウンドに3回まで
        };
        
        switch (occupationType)
        {
            case OccupationType.Trader:
                // 商人: 建材取得時に食料獲得
                if (triggerResource == ResourceType.Wood || triggerResource == ResourceType.Clay || triggerResource == ResourceType.Stone)
                {
                    takeEffect.resourceGain.Add(ResourceType.Food, 1);
                    takeEffect.effectDescription = "建材取得時に食料1個獲得";
                }
                break;
                
            case OccupationType.Forester:
                // 森林管理人: 木材取得時に追加木材
                if (triggerResource == ResourceType.Wood)
                {
                    takeEffect.resourceGain.Add(ResourceType.Wood, 1);
                    takeEffect.effectDescription = "木材取得時に追加木材1個獲得";
                }
                break;
                
            default:
                takeEffect.resourceGain.Add(ResourceType.Food, 1);
                break;
        }
        
        card.AddEffect(takeEffect);
        card.AddTag("取得反応", "アイテム取得時に効果を発動");
        
        return card;
    }
    
    /// <summary>
    /// OnReceiveトリガーを使用した職業カードを作成
    /// </summary>
    public static EnhancedOccupationCard CreateReceiveTriggeredOccupation(string cardName, string cardID, OccupationType occupationType, int minAmount = 1)
    {
        var card = CreateSampleOccupationCard(cardName, cardID, occupationType);
        
        // OnReceiveトリガー効果を追加
        var receiveEffect = new CardEffect
        {
            effectID = $"{cardID}_receive",
            effectDescription = $"リソース受取時に追加効果",
            effectType = CardEffectType.VictoryPointModification,
            triggerType = OccupationTrigger.OnReceive,
            specialEffectData = $"min_amount:{minAmount}", // 最小受取量の条件
            maxUsesPerRound = 2 // 1ラウンドに2回まで
        };
        
        switch (occupationType)
        {
            case OccupationType.Scholar:
                // 学者: 大量リソース受取時に勝利点獲得
                receiveEffect.victoryPointModifier = 1;
                receiveEffect.effectDescription = $"{minAmount}個以上のリソース受取時に勝利点1点獲得";
                receiveEffect.specialEffectData = $"min_amount:{minAmount}";
                break;
                
            case OccupationType.Merchant:
                // 大商人: リソース受取時に食料変換
                receiveEffect.effectType = CardEffectType.ResourceModification;
                receiveEffect.resourceGain.Add(ResourceType.Food, 1);
                receiveEffect.effectDescription = $"{minAmount}個以上のリソース受取時に食料1個獲得";
                receiveEffect.victoryPointModifier = 0;
                break;
                
            default:
                receiveEffect.resourceGain.Add(ResourceType.Food, 1);
                receiveEffect.effectDescription = "リソース受取時に食料1個獲得";
                receiveEffect.victoryPointModifier = 0;
                break;
        }
        
        card.AddEffect(receiveEffect);
        card.AddTag("受取反応", "アイテム受取時に効果を発動");
        
        return card;
    }
    
    /// <summary>
    /// 新しいトリガータイプを使用したサンプルカードを作成
    /// </summary>
    public void CreateNewTriggerSampleCards()
    {
        Debug.Log("=== 新しいトリガータイプのサンプルカード作成開始 ===");
        
        // OnTakeトリガーのサンプルカード
        var trader = CreateTakeTriggeredOccupation("商人", "OCC_TRADER", OccupationType.Trader, ResourceType.Wood);
        var forester = CreateTakeTriggeredOccupation("森林管理人", "OCC_FORESTER", OccupationType.Forester, ResourceType.Wood);
        
        Debug.Log("OnTakeトリガーカード作成完了:");
        Debug.Log(trader.GetCardInfo());
        Debug.Log(forester.GetCardInfo());
        
        // OnReceiveトリガーのサンプルカード
        var scholar = CreateReceiveTriggeredOccupation("学者", "OCC_SCHOLAR", OccupationType.Scholar, 3);
        var merchant = CreateReceiveTriggeredOccupation("大商人", "OCC_MERCHANT", OccupationType.Merchant, 2);
        
        Debug.Log("\nOnReceiveトリガーカード作成完了:");
        Debug.Log(scholar.GetCardInfo());
        Debug.Log(merchant.GetCardInfo());
        
        Debug.Log("=== 新しいトリガータイプのサンプルカード作成完了 ===");
    }
}