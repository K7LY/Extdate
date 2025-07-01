using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardTag
{
    public string tagName;
    public string description;
    
    public CardTag(string name, string desc = "")
    {
        tagName = name;
        description = desc;
    }
    
    public override string ToString()
    {
        return tagName;
    }
}

[System.Serializable]
public class CardEffect
{
    [Header("効果情報")]
    public string effectID;
    public string effectDescription;
    public CardEffectType effectType;
    
    [Header("トリガー")]
    public OccupationTrigger triggerType;
    public string triggerCondition;
    
    [Header("効果内容")]
    public Dictionary<ResourceType, int> resourceGain = new Dictionary<ResourceType, int>();
    public Dictionary<ResourceType, int> resourceCost = new Dictionary<ResourceType, int>();
    public int victoryPointModifier = 0;
    public string specialEffectData;
    
    [Header("制限")]
    public bool isOneTimeUse = false;
    public int maxUsesPerRound = -1; // -1は無制限
    public int currentUses = 0;
    
    public bool CanActivate()
    {
        if (isOneTimeUse && currentUses > 0) return false;
        if (maxUsesPerRound != -1 && currentUses >= maxUsesPerRound) return false;
        return true;
    }
    
    public void Use()
    {
        currentUses++;
    }
    
    public void ResetUses()
    {
        if (!isOneTimeUse)
        {
            currentUses = 0;
        }
    }
}

[System.Serializable]
public class PlayCondition
{
    [Header("条件情報")]
    public string conditionID;
    public string conditionDescription;
    
    [Header("リソース条件")]
    public Dictionary<ResourceType, int> requiredResources = new Dictionary<ResourceType, int>();
    
    [Header("プレイヤー状態条件")]
    public int minimumFamilyMembers = 0;
    public int minimumRooms = 0;
    public int minimumFields = 0;
    public int minimumPastures = 0;
    
    [Header("カード条件")]
    public List<string> requiredOccupations = new List<string>();
    public List<string> requiredImprovements = new List<string>();
    public List<string> requiredTags = new List<string>();
    
    [Header("特殊条件")]
    public string customCondition;
    
    public bool CanMeet(Player player)
    {
        // リソース条件チェック
        foreach (var requirement in requiredResources)
        {
            if (player.GetResource(requirement.Key) < requirement.Value)
                return false;
        }
        
        // プレイヤー状態条件チェック
        if (player.GetFamilyMembers() < minimumFamilyMembers) return false;
        if (player.GetRooms() < minimumRooms) return false;
        if (player.GetFields() < minimumFields) return false;
        if (player.GetPastures() < minimumPastures) return false;
        
        // カード条件チェック
        foreach (var requiredOccupation in requiredOccupations)
        {
            if (!player.HasOccupationByName(requiredOccupation)) return false;
        }
        
        foreach (var requiredImprovement in requiredImprovements)
        {
            if (!player.HasImprovementByName(requiredImprovement)) return false;
        }
        
        // タグ条件チェック
        foreach (var requiredTag in requiredTags)
        {
            if (!player.HasCardWithTag(requiredTag)) return false;
        }
        
        return true;
    }
}

[System.Serializable]
public class PlayCost
{
    [Header("コスト情報")]
    public string costID;
    public string costDescription;
    
    [Header("リソースコスト")]
    public Dictionary<ResourceType, int> resourceCosts = new Dictionary<ResourceType, int>();
    
    [Header("その他のコスト")]
    public int workerCost = 0;
    public int victoryPointCost = 0;
    public bool requiresFood = false;
    public int foodCost = 0;
    
    [Header("特殊コスト")]
    public string customCost;
    
    public bool CanPay(Player player)
    {
        // リソースコストチェック
        foreach (var cost in resourceCosts)
        {
            if (player.GetResource(cost.Key) < cost.Value)
                return false;
        }
        
        // ワーカーコストチェック
        if (player.GetAvailableWorkers() < workerCost)
            return false;
        
        // 勝利点コストチェック
        if (player.GetVictoryPoints() < victoryPointCost)
            return false;
        
        // 食料コストチェック
        if (requiresFood && player.GetResource(ResourceType.Food) < foodCost)
            return false;
        
        return true;
    }
    
    public void Pay(Player player)
    {
        // リソースコストを支払う
        foreach (var cost in resourceCosts)
        {
            player.SpendResource(cost.Key, cost.Value);
        }
        
        // 勝利点コストを支払う
        if (victoryPointCost > 0)
        {
            player.AddVictoryPoints(-victoryPointCost);
        }
        
        // 食料コストを支払う
        if (requiresFood && foodCost > 0)
        {
            player.SpendResource(ResourceType.Food, foodCost);
        }
    }
}

public abstract class EnhancedCard : ScriptableObject
{
    [Header("基本情報")]
    public string cardName;
    public string cardID;
    [TextArea(3, 5)]
    public string displayDescription;
    public Sprite cardImage;
    
    [Header("タグ")]
    public List<CardTag> tags = new List<CardTag>();
    
    [Header("効果とトリガー")]
    public List<CardEffect> effects = new List<CardEffect>();
    
    [Header("後方互換性")]
    public CardType cardType;
    public string description; // displayDescriptionに移行
    public Dictionary<ResourceType, int> cost = new Dictionary<ResourceType, int>(); // playConditionsに移行
    
    public virtual bool CanPlay(Player player)
    {
        return true;
    }
    
    public virtual void PlayCard(Player player)
    {
        // 効果を適用
        ApplyCardEffects(player);
        
        // CardTriggerManagerに自動登録
        RegisterToTriggerManager(player);
        
        Debug.Log($"{player.playerName}が「{cardName}」をプレイしました");
    }
    
    /// <summary>
    /// CardTriggerManagerに自動登録
    /// </summary>
    protected virtual void RegisterToTriggerManager(Player player)
    {
        if (CardTriggerManager.Instance != null)
        {
            CardTriggerManager.Instance.RegisterCard(player, this);
        }
    }
    
    /// <summary>
    /// CardTriggerManagerから自動削除
    /// </summary>
    protected virtual void UnregisterFromTriggerManager(Player player)
    {
        if (CardTriggerManager.Instance != null)
        {
            CardTriggerManager.Instance.UnregisterCard(player, this);
        }
    }
    
    protected virtual void ApplyCardEffects(Player player)
    {
        foreach (var effect in effects)
        {
            if (effect.triggerType == OccupationTrigger.Immediate)
            {
                ExecuteEffect(player, effect);
            }
        }
    }
    
    public virtual void TriggerEffect(Player player, OccupationTrigger triggerType, object context = null)
    {
        foreach (var effect in effects)
        {
            if (effect.triggerType == triggerType && effect.CanActivate())
            {
                if (CheckTriggerCondition(effect, player, context))
                {
                    ExecuteEffect(player, effect);
                    effect.Use();
                }
            }
        }
    }
    
    protected virtual bool CheckTriggerCondition(CardEffect effect, Player player, object context)
    {
        return true;
    }
    
    protected virtual void ExecuteEffect(Player player, CardEffect effect)
    {
        // リソース獲得
        foreach (var resource in effect.resourceGain)
        {
            player.AddResource(resource.Key, resource.Value);
        }
        
        // リソースコスト
        foreach (var resource in effect.resourceCost)
        {
            player.SpendResource(resource.Key, resource.Value);
        }
        
        // 勝利点修正
        if (effect.victoryPointModifier != 0)
        {
            player.AddVictoryPoints(effect.victoryPointModifier);
        }
        
        // 特殊効果データがあれば処理
        if (!string.IsNullOrEmpty(effect.specialEffectData))
        {
            ExecuteSpecialEffect(player, effect);
        }
    }
    
    protected virtual void ExecuteSpecialEffect(Player player, CardEffect effect)
    {
        // 派生クラスでオーバーライド
    }
    
    // タグ関連メソッド
    public bool HasTag(string tagName)
    {
        return tags.Exists(tag => tag.tagName.Equals(tagName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    public List<string> GetTagNames()
    {
        List<string> tagNames = new List<string>();
        foreach (var tag in tags)
        {
            tagNames.Add(tag.tagName);
        }
        return tagNames;
    }
    
    public void AddTag(string tagName, string description = "")
    {
        if (!HasTag(tagName))
        {
            tags.Add(new CardTag(tagName, description));
        }
    }
    
    public void RemoveTag(string tagName)
    {
        tags.RemoveAll(tag => tag.tagName.Equals(tagName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    // 効果関連メソッド
    public void AddEffect(CardEffect effect)
    {
        effects.Add(effect);
    }
    
    public void RemoveEffect(string effectID)
    {
        effects.RemoveAll(effect => effect.effectID == effectID);
    }
    
    public CardEffect GetEffect(string effectID)
    {
        return effects.Find(effect => effect.effectID == effectID);
    }
    
    public List<CardEffect> GetEffectsByTrigger(OccupationTrigger triggerType)
    {
        return effects.FindAll(effect => effect.triggerType == triggerType);
    }
    
    public void ResetAllEffectUses()
    {
        foreach (var effect in effects)
        {
            effect.ResetUses();
        }
    }
    
    // デバッグ用表示
    public string GetCardInfo()
    {
        var info = $"[{cardID}] {cardName}\n";
        info += $"説明: {displayDescription}\n";
        info += $"タグ: [{string.Join(", ", GetTagNames())}]\n";
        info += $"効果数: {effects.Count}";
        return info;
    }
}