[CreateAssetMenu(fileName = "New Enhanced Improvement Card", menuName = "Agricola Enhanced/Improvement Card")]
public class EnhancedImprovementCard : EnhancedCard
{
    [Header("進歩固有情報")]
    public ImprovementType improvementType;
    public ImprovementCategory category;
    public bool isUnique = true;
    
    [Header("進歩専用 - プレイ条件")]
    public List<PlayCondition> playConditions = new List<PlayCondition>();
    
    [Header("進歩専用 - プレイコスト")]
    public List<PlayCost> playCosts = new List<PlayCost>();
    
    [Header("進歩専用 - 勝利点")]
    public int victoryPointsWorth = 0;
    public bool countsAsEndGameScoring = false;
    public string endGameScoringCondition;
    
    [Header("料理設備情報")]
    public bool canCookGrain = false;
    public bool canCookVegetable = false;
    public int grainToFoodRatio = 2;
    public int vegetableToFoodRatio = 3;
    
    [Header("貯蔵設備情報")]
    public List<ResourceType> storageTypes = new List<ResourceType>();
    public int storageCapacity = 0;
    
    public override bool CanPlay(Player player)
    {
        // ユニーク性チェック
        if (isUnique && player.HasImprovementByName(cardName))
        {
            return false;
        }
        
        // プレイ条件をチェック
        foreach (var condition in playConditions)
        {
            if (!condition.CanMeet(player))
            {
                return false;
            }
        }
        
        // プレイコストをチェック
        foreach (var cost in playCosts)
        {
            if (!cost.CanPay(player))
            {
                return false;
            }
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
        // プレイコストを支払う
        foreach (var cost in playCosts)
        {
            cost.Pay(player);
        }
        
        // 後方互換性のためのコスト支払い
        foreach (var costEntry in cost)
        {
            player.SpendResource(costEntry.Key, costEntry.Value);
        }
        
        // プレイヤーに進歩を追加
        player.AddImprovementCard(this);
        
        // 勝利点を追加
        if (victoryPointsWorth > 0)
        {
            player.AddVictoryPoints(victoryPointsWorth);
        }
        
        // 料理設備効果を追加
        if (canCookGrain || canCookVegetable)
        {
            AddCookingFacilityToPlayer(player);
        }
        
        // 貯蔵設備効果を追加
        if (storageTypes.Count > 0 && storageCapacity > 0)
        {
            AddStorageFacilityToPlayer(player);
        }
        
        base.PlayCard(player);
    }
    
    private void AddCookingFacilityToPlayer(Player player)
    {
        player.AddCookingFacility(this);
        
        if (canCookGrain)
        {
            player.AddCookingAbility(ResourceType.Grain, grainToFoodRatio);
        }
        
        if (canCookVegetable)
        {
            player.AddCookingAbility(ResourceType.Vegetable, vegetableToFoodRatio);
        }
        
        Debug.Log($"{cardName}の料理設備効果を追加しました");
    }
    
    private void AddStorageFacilityToPlayer(Player player)
    {
        foreach (var resourceType in storageTypes)
        {
            player.AddStorageCapacity(resourceType, storageCapacity);
        }
        
        Debug.Log($"{cardName}の貯蔵設備効果を追加しました");
    }
    
    protected override void ExecuteSpecialEffect(Player player, CardEffect effect)
    {
        // 進歩カード固有の特殊効果
        switch (effect.specialEffectData)
        {
            case "cooking_conversion":
                ExecuteCookingConversion(player, effect);
                break;
            case "storage_bonus":
                ExecuteStorageBonus(player, effect);
                break;
            case "resource_generation":
                ExecuteResourceGeneration(player, effect);
                break;
            case "action_bonus":
                ExecuteActionBonus(player, effect);
                break;
            case "end_game_scoring":
                ExecuteEndGameScoring(player, effect);
                break;
            default:
                base.ExecuteSpecialEffect(player, effect);
                break;
        }
    }
    
    private void ExecuteCookingConversion(Player player, CardEffect effect)
    {
        // 料理変換の特殊効果
        Debug.Log($"{cardName}の料理変換効果が発動しました");
    }
    
    private void ExecuteStorageBonus(Player player, CardEffect effect)
    {
        // 貯蔵ボーナスの特殊効果
        Debug.Log($"{cardName}の貯蔵ボーナス効果が発動しました");
    }
    
    private void ExecuteResourceGeneration(Player player, CardEffect effect)
    {
        // リソース生成の特殊効果
        foreach (var resource in effect.resourceGain)
        {
            player.AddResource(resource.Key, resource.Value);
        }
        Debug.Log($"{cardName}のリソース生成効果が発動しました");
    }
    
    private void ExecuteActionBonus(Player player, CardEffect effect)
    {
        // アクションボーナスの特殊効果
        Debug.Log($"{cardName}のアクションボーナス効果が発動しました");
    }
    
    private void ExecuteEndGameScoring(Player player, CardEffect effect)
    {
        // ゲーム終了時得点の特殊効果
        if (countsAsEndGameScoring)
        {
            int bonusPoints = CalculateEndGameBonus(player);
            player.AddVictoryPoints(bonusPoints);
            Debug.Log($"{cardName}のゲーム終了時ボーナス: {bonusPoints}点");
        }
    }
    
    private int CalculateEndGameBonus(Player player)
    {
        // ゲーム終了時ボーナスの計算
        switch (endGameScoringCondition)
        {
            case "per_grain":
                return player.GetResource(ResourceType.Grain);
            case "per_vegetable":
                return player.GetResource(ResourceType.Vegetable);
            case "per_animal":
                return player.GetResource(ResourceType.Sheep) + 
                       player.GetResource(ResourceType.Boar) + 
                       player.GetResource(ResourceType.Cattle);
            case "per_improvement":
                return player.GetImprovements().Count;
            case "per_occupation":
                return player.GetOccupations().Count;
            default:
                return 0;
        }
    }
    
    // 料理実行メソッド（EnhancedCard用）
    public int CookResourceEnhanced(Player player, ResourceType resourceType, int amount)
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
    
    // プレイ条件を追加するヘルパーメソッド
    public void AddPlayCondition(PlayCondition condition)
    {
        playConditions.Add(condition);
    }
    
    // プレイコストを追加するヘルパーメソッド
    public void AddPlayCost(PlayCost cost)
    {
        playCosts.Add(cost);
    }
    
    // プレイ条件の詳細情報を取得
    public string GetPlayConditionsDescription()
    {
        var descriptions = new List<string>();
        foreach (var condition in playConditions)
        {
            if (!string.IsNullOrEmpty(condition.conditionDescription))
            {
                descriptions.Add(condition.conditionDescription);
            }
        }
        return string.Join(", ", descriptions);
    }
    
    // プレイコストの詳細情報を取得
    public string GetPlayCostsDescription()
    {
        var descriptions = new List<string>();
        foreach (var cost in playCosts)
        {
            if (!string.IsNullOrEmpty(cost.costDescription))
            {
                descriptions.Add(cost.costDescription);
            }
        }
        return string.Join(", ", descriptions);
    }
}