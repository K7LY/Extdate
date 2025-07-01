using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum CardEffectType
{
    // 基本効果
    ImmediateResource,  // 即座のリソース獲得
    PassiveBonus,       // 継続的ボーナス
    ActionModifier,     // アクション修正
    SpecialAbility,     // 特殊能力
    
    // 進歩カード専用
    CookingFacility,    // 料理設備
    StorageFacility,    // 貯蔵設備
    ConversionBonus,    // 変換ボーナス
    
    // 職業カード専用
    TriggerEffect,      // トリガー効果
    PermanentBonus,     // 永続ボーナス
    ConditionalEffect   // 条件付き効果
}

[System.Serializable]
public class CardEffect
{
    [Header("基本情報")]
    public CardEffectType effectType;
    public string description;
    
    [Header("リソース効果")]
    public Dictionary<ResourceType, int> resourceGain = new Dictionary<ResourceType, int>();
    public Dictionary<ResourceType, int> resourceCost = new Dictionary<ResourceType, int>();
    
    [Header("ボーナス効果")]
    public string bonusType;
    public int bonusAmount;
    public bool isPermanent = false;
    
    [Header("条件")]
    public string triggerCondition;
    public ActionType triggeredByAction;
    public ResourceType triggeredByResource;
    
    [Header("特殊効果")]
    public bool allowsMultipleUse = false;
    public int maxUsesPerRound = 1;
    public int currentUses = 0;
    
    public bool CanActivate()
    {
        if (!allowsMultipleUse && currentUses >= maxUsesPerRound)
            return false;
        
        return true;
    }
    
    public void Activate()
    {
        if (CanActivate())
        {
            currentUses++;
        }
    }
    
    public void ResetUses()
    {
        currentUses = 0;
    }
}