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
}