[CreateAssetMenu(fileName = "New Enhanced Occupation Card", menuName = "Agricola Enhanced/Occupation Card")]
public class EnhancedOccupationCard : EnhancedCard
{
    [Header("職業固有情報")]
    public OccupationType occupationType;
    public bool isStartingOccupation = false;
    public int maxCopiesInGame = 1;
    
    public override bool CanPlay(Player player)
    {
        // 既に同じ職業を持っているかチェック
        if (player.HasOccupationByName(cardName))
        {
            return false;
        }
        
        // 後方互換性のためのコストチェック
        foreach (var costEntry in cost)
        {
            if (player.GetResource(costEntry.Key) < costEntry.Value)
                return false;
        }
        
        return base.CanPlay(player);
    }
    
    public override void PlayCard(Player player)
    {
        // 後方互換性のためのコスト支払い
        foreach (var costEntry in cost)
        {
            player.SpendResource(costEntry.Key, costEntry.Value);
        }
        
        // プレイヤーに職業を追加
        player.AddOccupationCard(this);
        
        base.PlayCard(player);
    }
    
    protected override bool CheckTriggerCondition(CardEffect effect, Player player, object context)
    {
        // 職業カード固有のトリガー条件をチェック
        switch (effect.triggerType)
        {
            case OccupationTrigger.OnAction:
                return CheckActionTrigger(effect, player, context);
            case OccupationTrigger.OnHarvest:
                return CheckHarvestTrigger(effect, player, context);
            case OccupationTrigger.OnBreeding:
                return CheckBreedingTrigger(effect, player, context);
            
            // 新しい収穫フェーズ用トリガー
            case OccupationTrigger.BeforeHarvest:
                return CheckBeforeHarvestTrigger(effect, player, context);
            case OccupationTrigger.HarvestStart:
                return CheckHarvestStartTrigger(effect, player, context);
            case OccupationTrigger.FieldPhase:
                return CheckFieldPhaseTrigger(effect, player, context);
            case OccupationTrigger.FeedingPhase:
                return CheckFeedingPhaseTrigger(effect, player, context);
            case OccupationTrigger.BreedingPhase:
                return CheckBreedingPhaseTrigger(effect, player, context);
            case OccupationTrigger.HarvestEnd:
                return CheckHarvestEndTrigger(effect, player, context);
            
            default:
                return base.CheckTriggerCondition(effect, player, context);
        }
    }
    
    private bool CheckActionTrigger(CardEffect effect, Player player, object context)
    {
        if (context is ActionSpace actionSpace)
        {
            // トリガー条件文字列に基づいてチェック
            if (!string.IsNullOrEmpty(effect.triggerCondition))
            {
                return actionSpace.actionName.Contains(effect.triggerCondition) ||
                       actionSpace.actionType.ToString().Contains(effect.triggerCondition);
            }
        }
        return true;
    }
    
    private bool CheckHarvestTrigger(CardEffect effect, Player player, object context)
    {
        // 収穫トリガーの条件チェック
        return true; // 基本的に全ての収穫で発動
    }
    
    private bool CheckBreedingTrigger(CardEffect effect, Player player, object context)
    {
        // 繁殖トリガーの条件チェック
        return true; // 基本的に全ての繁殖で発動
    }
    
    // 新しい収穫フェーズ用トリガー条件チェック
    private bool CheckBeforeHarvestTrigger(CardEffect effect, Player player, object context)
    {
        // 収穫の直前トリガーの条件チェック
        return true; // 基本的に全ての収穫前で発動
    }
    
    private bool CheckHarvestStartTrigger(CardEffect effect, Player player, object context)
    {
        // 収穫の開始時トリガーの条件チェック
        return true; // 基本的に全ての収穫開始時で発動
    }
    
    private bool CheckFieldPhaseTrigger(CardEffect effect, Player player, object context)
    {
        // 畑フェーズトリガーの条件チェック
        // 畑を持っているかチェック
        if (!string.IsNullOrEmpty(effect.triggerCondition))
        {
            if (effect.triggerCondition.Contains("has_fields"))
            {
                return player.GetFields() > 0;
            }
            if (effect.triggerCondition.Contains("has_crops"))
            {
                return player.GetTotalCropsInFields(ResourceType.Grain) > 0 ||
                       player.GetTotalCropsInFields(ResourceType.Vegetable) > 0;
            }
        }
        return true; // 基本的に全ての畑フェーズで発動
    }
    
    private bool CheckFeedingPhaseTrigger(CardEffect effect, Player player, object context)
    {
        // 食料供給フェーズトリガーの条件チェック
        if (!string.IsNullOrEmpty(effect.triggerCondition))
        {
            if (effect.triggerCondition.Contains("food_shortage"))
            {
                return player.GetFoodNeeded() > player.GetResource(ResourceType.Food);
            }
            if (effect.triggerCondition.Contains("has_grain"))
            {
                return player.GetResource(ResourceType.Grain) > 0;
            }
        }
        return true; // 基本的に全ての食料供給フェーズで発動
    }
    
    private bool CheckBreedingPhaseTrigger(CardEffect effect, Player player, object context)
    {
        // 繁殖フェーズトリガーの条件チェック
        if (!string.IsNullOrEmpty(effect.triggerCondition))
        {
            if (effect.triggerCondition.Contains("has_sheep"))
            {
                return player.GetResource(ResourceType.Sheep) >= 2;
            }
            if (effect.triggerCondition.Contains("has_boar"))
            {
                return player.GetResource(ResourceType.Boar) >= 2;
            }
            if (effect.triggerCondition.Contains("has_cattle"))
            {
                return player.GetResource(ResourceType.Cattle) >= 2;
            }
        }
        return true; // 基本的に全ての繁殖フェーズで発動
    }
    
    private bool CheckHarvestEndTrigger(CardEffect effect, Player player, object context)
    {
        // 収穫終了時トリガーの条件チェック
        return true; // 基本的に全ての収穫終了時で発動
    }
    
    protected override void ExecuteSpecialEffect(Player player, CardEffect effect)
    {
        // 職業カード固有の特殊効果
        switch (occupationType)
        {
            case OccupationType.Farmer:
                ExecuteFarmerSpecialEffect(player, effect);
                break;
            case OccupationType.Carpenter:
                ExecuteCarpenterSpecialEffect(player, effect);
                break;
            case OccupationType.Baker:
                ExecuteBakerSpecialEffect(player, effect);
                break;
            case OccupationType.Fisherman:
                ExecuteFishermanSpecialEffect(player, effect);
                break;
            case OccupationType.Shepherd:
                ExecuteShepherdSpecialEffect(player, effect);
                break;
            default:
                base.ExecuteSpecialEffect(player, effect);
                break;
        }
    }
    
    private void ExecuteFarmerSpecialEffect(Player player, CardEffect effect)
    {
        // 農夫の特殊効果
        switch (effect.specialEffectData)
        {
            case "sowing_bonus":
                player.AddTempBonus("farmer_sowing_bonus", 1);
                break;
            case "harvest_bonus":
                player.AddTempBonus("farmer_harvest_bonus", 1);
                break;
        }
    }
    
    private void ExecuteCarpenterSpecialEffect(Player player, CardEffect effect)
    {
        // 大工の特殊効果
        switch (effect.specialEffectData)
        {
            case "building_discount":
                player.AddTempBonus("carpenter_building_discount", 1);
                break;
            case "wood_bonus":
                player.AddResource(ResourceType.Wood, 1);
                break;
        }
    }
    
    private void ExecuteBakerSpecialEffect(Player player, CardEffect effect)
    {
        // パン職人の特殊効果
        switch (effect.specialEffectData)
        {
            case "cooking_efficiency":
                player.AddTempBonus("baker_cooking_efficiency", 1);
                break;
        }
    }
    
    private void ExecuteFishermanSpecialEffect(Player player, CardEffect effect)
    {
        // 漁師の特殊効果
        switch (effect.specialEffectData)
        {
            case "fishing_bonus":
                player.AddResource(ResourceType.Food, 1);
                break;
        }
    }
    
    private void ExecuteShepherdSpecialEffect(Player player, CardEffect effect)
    {
        // 羊飼いの特殊効果
        switch (effect.specialEffectData)
        {
            case "sheep_bonus":
                if (player.GetResource(ResourceType.Sheep) >= 2)
                {
                    player.AddResource(ResourceType.Sheep, 1);
                }
                break;
        }
    }
}