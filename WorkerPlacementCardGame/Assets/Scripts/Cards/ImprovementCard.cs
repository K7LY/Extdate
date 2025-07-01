using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum ImprovementType
{
    Minor,  // 小さな進歩
    Major   // 大きな進歩
}

[System.Serializable]
public enum ImprovementCategory
{
    // 基本カテゴリ
    Cooking,        // 料理
    Building,       // 建築
    Farming,        // 農業
    AnimalHusbandry,// 牧畜
    Storage,        // 貯蔵
    
    // 専門カテゴリ
    Crafting,       // 工芸
    Trading,        // 交易
    Knowledge,      // 知識
    Infrastructure, // インフラ
    Luxury          // 贅沢品
}

[CreateAssetMenu(fileName = "New Improvement Card", menuName = "Agricola/Improvement Card")]
public class ImprovementCard : Card
{
    [Header("進歩情報")]
    public ImprovementType improvementType;
    public ImprovementCategory category;
    public bool requiresCooking = false;
    public bool isUnique = true;
    
    [Header("効果")]
    public string effectDescription;
    public List<CardEffect> effects = new List<CardEffect>();
    
    [Header("料理効果")]
    public bool canCookGrain = false;
    public bool canCookVegetable = false;
    public int grainToFoodRatio = 2; // 穀物1個 → 食料2個
    public int vegetableToFoodRatio = 3; // 野菜1個 → 食料3個
    
    [Header("特殊効果")]
    public int additionalActionSpaces = 0;
    public bool allowsExtraWorkerPlacement = false;
    public List<ResourceType> storageTypes = new List<ResourceType>();
    public int storageCapacity = 0;
    
    public override bool CanPlay(Player player)
    {
        if (!base.CanPlay(player)) return false;
        
        // ユニーク性チェック
        if (isUnique && player.HasImprovement(this)) return false;
        
        // 料理設備の前提条件チェック
        if (requiresCooking && !player.HasCookingFacility()) return false;
        
        return true;
    }
    
    public override void PlayCard(Player player)
    {
        if (!CanPlay(player)) return;
        
        base.PlayCard(player);
        
        // 進歩の効果を適用
        ApplyImprovementEffect(player);
        
        // プレイヤーに進歩を登録
        player.AddImprovement(this);
        
        Debug.Log($"{player.playerName}が進歩「{cardName}」をプレイしました");
    }
    
    private void ApplyImprovementEffect(Player player)
    {
        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                case CardEffectType.ImmediateResource:
                    ApplyImmediateResourceEffect(player, effect);
                    break;
                    
                case CardEffectType.PassiveBonus:
                    ApplyPassiveBonusEffect(player, effect);
                    break;
                    
                case CardEffectType.CookingFacility:
                    ApplyCookingFacilityEffect(player, effect);
                    break;
                    
                case CardEffectType.StorageFacility:
                    ApplyStorageFacilityEffect(player, effect);
                    break;
                    
                case CardEffectType.SpecialAbility:
                    ApplySpecialAbilityEffect(player, effect);
                    break;
            }
        }
        
        // 追加アクションスペースの提供
        if (additionalActionSpaces > 0)
        {
            player.AddAdditionalActionSpaces(additionalActionSpaces);
        }
    }
    
    private void ApplyImmediateResourceEffect(Player player, CardEffect effect)
    {
        foreach (var resource in effect.resourceGain)
        {
            player.AddResource(resource.Key, resource.Value);
        }
    }
    
    private void ApplyPassiveBonusEffect(Player player, CardEffect effect)
    {
        player.AddPassiveEffect(this, effect);
    }
    
    private void ApplyCookingFacilityEffect(Player player, CardEffect effect)
    {
        // 料理設備効果を登録
        player.AddCookingFacility(this);
    }
    
    private void ApplyStorageFacilityEffect(Player player, CardEffect effect)
    {
        // 貯蔵設備効果を登録
        foreach (var resourceType in storageTypes)
        {
            player.AddStorageCapacity(resourceType, storageCapacity);
        }
    }
    
    private void ApplySpecialAbilityEffect(Player player, CardEffect effect)
    {
        // 特殊能力の適用（進歩カード名に基づく）
        switch (cardName)
        {
            case "かまど":
                ApplyFireplaceEffect(player);
                break;
                
            case "料理場":
                ApplyCookingAreaEffect(player);
                break;
                
            case "石のかまど":
                ApplyStoneOvenEffect(player);
                break;
                
            case "井戸":
                ApplyWellEffect(player);
                break;
                
            case "バスケット":
                ApplyBasketEffect(player);
                break;
                
            case "陶器":
                ApplyPotteryEffect(player);
                break;
                
            case "農具":
                ApplyPlowEffect(player);
                break;
                
            case "柵":
                ApplyFenceEffect(player);
                break;
                
            default:
                Debug.Log($"未実装の進歩効果: {cardName}");
                break;
        }
    }
    
    // 個別の進歩効果実装
    private void ApplyFireplaceEffect(Player player)
    {
        // かまど: 穀物を食料に変換可能
        player.AddCookingAbility(ResourceType.Grain, 2);
        Debug.Log($"かまど効果: 穀物1個→食料2個の変換が可能になりました");
    }
    
    private void ApplyCookingAreaEffect(Player player)
    {
        // 料理場: 穀物と野菜を食料に変換可能
        player.AddCookingAbility(ResourceType.Grain, 2);
        player.AddCookingAbility(ResourceType.Vegetable, 3);
        Debug.Log($"料理場効果: 穀物・野菜の調理が可能になりました");
    }
    
    private void ApplyStoneOvenEffect(Player player)
    {
        // 石のかまど: 高効率調理 + 毎ラウンド食料ボーナス
        player.AddCookingAbility(ResourceType.Grain, 3);
        player.AddCookingAbility(ResourceType.Vegetable, 4);
        player.AddTempBonus("stone_oven_bonus", 1);
        Debug.Log($"石のかまど効果: 高効率調理 + 毎ラウンド食料1個");
    }
    
    private void ApplyWellEffect(Player player)
    {
        // 井戸: 家族の成長が容易になる
        player.AddTempBonus("well_family_growth", 1);
        Debug.Log($"井戸効果: 家族の成長が容易になりました");
    }
    
    private void ApplyBasketEffect(Player player)
    {
        // バスケット: リソース持ち越し能力
        player.AddStorageCapacity(ResourceType.Grain, 3);
        player.AddStorageCapacity(ResourceType.Vegetable, 3);
        Debug.Log($"バスケット効果: 穀物・野菜の貯蔵能力+3");
    }
    
    private void ApplyPotteryEffect(Player player)
    {
        // 陶器: 毎ラウンド小さなリソースボーナス
        player.AddTempBonus("pottery_resource_bonus", 1);
        Debug.Log($"陶器効果: 毎ラウンド小さなリソースボーナス");
    }
    
    private void ApplyPlowEffect(Player player)
    {
        // 農具: 種まき効率向上
        player.AddTempBonus("plow_sowing_bonus", 1);
        Debug.Log($"農具効果: 種まき効率が向上しました");
    }
    
    private void ApplyFenceEffect(Player player)
    {
        // 柵: 動物収容能力向上
        player.AddFences(4);
        Debug.Log($"柵効果: 柵4本を獲得しました");
    }
    
    // 料理実行メソッド
    public int CookResource(Player player, ResourceType resourceType, int amount)
    {
        if (!canCookGrain && resourceType == ResourceType.Grain) return 0;
        if (!canCookVegetable && resourceType == ResourceType.Vegetable) return 0;
        
        if (player.GetResource(resourceType) < amount) return 0;
        
        player.SpendResource(resourceType, amount);
        
        int foodGained = 0;
        if (resourceType == ResourceType.Grain)
        {
            foodGained = amount * grainToFoodRatio;
        }
        else if (resourceType == ResourceType.Vegetable)
        {
            foodGained = amount * vegetableToFoodRatio;
        }
        
        player.AddResource(ResourceType.Food, foodGained);
        
        Debug.Log($"{cardName}で{GetResourceJapaneseName(resourceType)}{amount}個を食料{foodGained}個に変換しました");
        
        return foodGained;
    }
    
    private string GetResourceJapaneseName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Grain: return "穀物";
            case ResourceType.Vegetable: return "野菜";
            default: return resourceType.ToString();
        }
    }
}