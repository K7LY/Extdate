using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum OccupationType
{
    // 基本職業
    Farmer,         // 農夫
    Carpenter,      // 大工
    Baker,          // パン職人
    Fisherman,      // 漁師
    Shepherd,       // 羊飼い
    
    // 専門職業
    Stonecutter,    // 石工
    Weaver,         // 織工
    Trader,         // 商人
    Forester,       // 森林管理人
    Breeder,        // 動物育種家
    
    // 高度職業
    Scholar,        // 学者
    Craftsman,      // 職人
    Merchant,       // 大商人
    Advisor,        // 顧問
    Chief           // 族長
}

[System.Serializable]
public enum OccupationTrigger
{
    Immediate,      // 即座
    OnAction,       // アクション実行時
    OnHarvest,      // 収穫時
    OnBreeding,     // 繁殖時
    OnTurnEnd,      // ターン終了時
    OnRoundStart,   // ラウンド開始時
    OnTake,         // アイテムを取得したとき
    OnReceive,      // アイテムが手持ちに入ったとき
    Passive         // 継続効果
}

[CreateAssetMenu(fileName = "New Occupation Card", menuName = "Agricola/Occupation Card")]
public class OccupationCard : Card
{
    [Header("職業情報")]
    public OccupationType occupationType;
    public OccupationTrigger trigger;
    public bool isPlayedOnce = false;
    
    [Header("効果")]
    public string effectDescription;
    public List<CardEffect> effects = new List<CardEffect>();
    
    [Header("プレイ条件")]
    public int minimumFamilyMembers = 0;
    public int minimumRooms = 0;
    public List<ResourceType> requiredResources = new List<ResourceType>();
    public int requiredResourceAmount = 0;
    
    public override bool CanPlay(Player player)
    {
        if (!base.CanPlay(player)) return false;
        
        // 家族数チェック
        if (player.GetFamilyMembers() < minimumFamilyMembers) return false;
        
        // 部屋数チェック
        if (player.GetRooms() < minimumRooms) return false;
        
        // 必要リソースチェック
        foreach (var resourceType in requiredResources)
        {
            if (player.GetResource(resourceType) < requiredResourceAmount) return false;
        }
        
        return true;
    }
    
    public override void PlayCard(Player player)
    {
        if (!CanPlay(player)) return;
        
        base.PlayCard(player);
        
        // 職業の効果を適用
        ApplyOccupationEffect(player);
        
        // プレイヤーに職業を登録
        player.AddOccupation(this);
        
        Debug.Log($"{player.playerName}が職業「{cardName}」をプレイしました");
    }
    
    private void ApplyOccupationEffect(Player player)
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
                    
                case CardEffectType.ActionModifier:
                    ApplyActionModifierEffect(player, effect);
                    break;
                    
                case CardEffectType.SpecialAbility:
                    ApplySpecialAbilityEffect(player, effect);
                    break;
            }
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
        // 継続効果はプレイヤーに登録
        player.AddPassiveEffect(this, effect);
    }
    
    private void ApplyActionModifierEffect(Player player, CardEffect effect)
    {
        // アクション修正効果をプレイヤーに登録
        player.AddActionModifier(this, effect);
    }
    
    private void ApplySpecialAbilityEffect(Player player, CardEffect effect)
    {
        // 特殊能力効果の適用
        switch (occupationType)
        {
            case OccupationType.Farmer:
                ApplyFarmerEffect(player, effect);
                break;
                
            case OccupationType.Carpenter:
                ApplyCarpenterEffect(player, effect);
                break;
                
            case OccupationType.Baker:
                ApplyBakerEffect(player, effect);
                break;
                
            case OccupationType.Fisherman:
                ApplyFishermanEffect(player, effect);
                break;
                
            case OccupationType.Shepherd:
                ApplyShepherdEffect(player, effect);
                break;
                
            default:
                Debug.Log($"未実装の職業効果: {occupationType}");
                break;
        }
    }
    
    private void ApplyFarmerEffect(Player player, CardEffect effect)
    {
        // 農夫: 種まき時に追加穀物を得る
        player.AddTempBonus("farmer_sowing_bonus", 1);
        Debug.Log($"農夫効果: 種まき時に追加穀物を得られます");
    }
    
    private void ApplyCarpenterEffect(Player player, CardEffect effect)
    {
        // 大工: 住居拡張のコストを削減
        player.AddTempBonus("carpenter_building_discount", 1);
        Debug.Log($"大工効果: 住居拡張コストが削減されます");
    }
    
    private void ApplyBakerEffect(Player player, CardEffect effect)
    {
        // パン職人: 穀物から食料への変換効率向上
        player.AddTempBonus("baker_conversion_bonus", 1);
        Debug.Log($"パン職人効果: 穀物→食料変換が効率化されます");
    }
    
    private void ApplyFishermanEffect(Player player, CardEffect effect)
    {
        // 漁師: 食料獲得アクションで追加食料
        player.AddTempBonus("fisherman_food_bonus", 1);
        Debug.Log($"漁師効果: 食料獲得時にボーナスを得られます");
    }
    
    private void ApplyShepherdEffect(Player player, CardEffect effect)
    {
        // 羊飼い: 羊の繁殖効率向上
        player.AddTempBonus("shepherd_breeding_bonus", 1);
        Debug.Log($"羊飼い効果: 羊の繁殖効率が向上します");
    }
    
    // 職業効果のトリガー処理
    public void TriggerEffect(Player player, OccupationTrigger triggerType, object context = null)
    {
        if (trigger != triggerType) return;
        
        switch (triggerType)
        {
            case OccupationTrigger.OnHarvest:
                OnHarvestTrigger(player, context);
                break;
                
            case OccupationTrigger.OnAction:
                OnActionTrigger(player, context);
                break;
                
            case OccupationTrigger.OnBreeding:
                OnBreedingTrigger(player, context);
                break;
                
            case OccupationTrigger.OnTurnEnd:
                OnTurnEndTrigger(player, context);
                break;
                
            case OccupationTrigger.OnTake:
                OnTakeTrigger(player, context);
                break;
                
            case OccupationTrigger.OnReceive:
                OnReceiveTrigger(player, context);
                break;
        }
    }
    
    private void OnHarvestTrigger(Player player, object context)
    {
        switch (occupationType)
        {
            case OccupationType.Farmer:
                // 農夫: 収穫時に追加穀物
                player.AddResource(ResourceType.Grain, 1);
                Debug.Log($"農夫効果発動: 追加穀物1個獲得");
                break;
        }
    }
    
    private void OnActionTrigger(Player player, object context)
    {
        if (context is ActionSpace actionSpace)
        {
            switch (occupationType)
            {
                case OccupationType.Fisherman:
                    if (actionSpace.actionName.Contains("漁") || actionSpace.actionName.Contains("釣"))
                    {
                        player.AddResource(ResourceType.Food, 1);
                        Debug.Log($"漁師効果発動: 追加食料1個獲得");
                    }
                    break;
                    
                case OccupationType.Carpenter:
                    if (actionSpace.actionType == ActionType.HouseExpansion)
                    {
                        player.AddResource(ResourceType.Wood, 1);
                        Debug.Log($"大工効果発動: 木材コスト削減");
                    }
                    break;
            }
        }
    }
    
    private void OnBreedingTrigger(Player player, object context)
    {
        switch (occupationType)
        {
            case OccupationType.Shepherd:
                // 羊飼い: 羊の繁殖時に追加羊
                if (player.GetResource(ResourceType.Sheep) >= 2)
                {
                    player.AddResource(ResourceType.Sheep, 1);
                    Debug.Log($"羊飼い効果発動: 追加羊1匹獲得");
                }
                break;
        }
    }
    
    private void OnTurnEndTrigger(Player player, object context)
    {
        // ターン終了時の効果処理
    }
    
    private void OnTakeTrigger(Player player, object context)
    {
        // アイテムを取得したときの効果処理
        // contextにはTakeEventContextが渡される
        if (context is TakeEventContext takeContext)
        {
            switch (occupationType)
            {
                case OccupationType.Trader:
                    // 商人: アイテム取得時に追加リソース
                    if (takeContext.resourceType == ResourceType.Wood || 
                        takeContext.resourceType == ResourceType.Clay ||
                        takeContext.resourceType == ResourceType.Stone)
                    {
                        player.AddResource(ResourceType.Food, 1);
                        Debug.Log($"商人効果発動: 建材取得時に食料1個獲得");
                    }
                    break;
                    
                case OccupationType.Forester:
                    // 森林管理人: 木材取得時に追加木材
                    if (takeContext.resourceType == ResourceType.Wood)
                    {
                        player.AddResource(ResourceType.Wood, 1);
                        Debug.Log($"森林管理人効果発動: 木材取得時に追加木材1個獲得");
                    }
                    break;
            }
        }
    }
    
    private void OnReceiveTrigger(Player player, object context)
    {
        // アイテムが手持ちに入ったときの効果処理
        // contextにはReceiveEventContextが渡される
        if (context is ReceiveEventContext receiveContext)
        {
            switch (occupationType)
            {
                case OccupationType.Scholar:
                    // 学者: 任意のリソースを受け取ったときに勝利点獲得
                    if (receiveContext.amount >= 3)
                    {
                        player.AddVictoryPoints(1);
                        Debug.Log($"学者効果発動: 大量リソース受取時に勝利点1点獲得");
                    }
                    break;
                    
                case OccupationType.Merchant:
                    // 大商人: 食料以外のリソースを受け取ったときに食料変換
                    if (receiveContext.resourceType != ResourceType.Food && receiveContext.amount >= 2)
                    {
                        player.AddResource(ResourceType.Food, receiveContext.amount / 2);
                        Debug.Log($"大商人効果発動: リソース受取時に食料{receiveContext.amount / 2}個変換獲得");
                    }
                    break;
            }
        }
    }
}