using UnityEngine;
using System.Collections.Generic;

public class ResourceConverter : MonoBehaviour
{
    [Header("変換レート")]
    [SerializeField] private Dictionary<ResourceType, int> grainToFoodRate = new Dictionary<ResourceType, int>();
    [SerializeField] private Dictionary<ResourceType, int> animalToFoodRate = new Dictionary<ResourceType, int>();
    
    void Awake()
    {
        InitializeConversionRates();
    }
    
    private void InitializeConversionRates()
    {
        // 穀物の変換レート（製パン）
        grainToFoodRate[ResourceType.Grain] = 2; // 穀物1個 → 食料2個
        
        // 動物の変換レート（屠殺）
        animalToFoodRate[ResourceType.Sheep] = 2;  // 羊1匹 → 食料2個
        animalToFoodRate[ResourceType.Boar] = 3;   // 猪1匹 → 食料3個
        animalToFoodRate[ResourceType.Cattle] = 4; // 牛1匹 → 食料4個
    }
    
    // 穀物を食料に変換（製パン）
    public int ConvertGrainToFood(Player player, int grainAmount)
    {
        if (player.GetResource(ResourceType.Grain) >= grainAmount)
        {
            player.SpendResource(ResourceType.Grain, grainAmount);
            int foodGained = grainAmount * grainToFoodRate[ResourceType.Grain];
            player.AddResource(ResourceType.Food, foodGained);
            
            Debug.Log($"{player.playerName}が穀物{grainAmount}個を食料{foodGained}個に変換しました");
            return foodGained;
        }
        return 0;
    }
    
    // 動物を食料に変換（屠殺）
    public int ConvertAnimalToFood(Player player, ResourceType animalType, int animalAmount)
    {
        if (!animalToFoodRate.ContainsKey(animalType))
        {
            Debug.LogWarning($"{animalType}は食料に変換できません");
            return 0;
        }
        
        if (player.GetResource(animalType) >= animalAmount)
        {
            player.SpendResource(animalType, animalAmount);
            int foodGained = animalAmount * animalToFoodRate[animalType];
            player.AddResource(ResourceType.Food, foodGained);
            
            string animalName = GetAnimalJapaneseName(animalType);
            Debug.Log($"{player.playerName}が{animalName}{animalAmount}匹を食料{foodGained}個に変換しました");
            return foodGained;
        }
        return 0;
    }
    
    // 自動的に食料を確保する（収穫時の食料不足対応）
    public int AutoConvertForFood(Player player, int foodNeeded)
    {
        int totalFoodGained = 0;
        int remainingNeeded = foodNeeded;
        
        // 1. まず穀物から変換
        int availableGrain = player.GetResource(ResourceType.Grain);
        if (availableGrain > 0 && remainingNeeded > 0)
        {
            int grainToUse = Mathf.Min(availableGrain, Mathf.CeilToInt(remainingNeeded / 2f));
            int foodFromGrain = ConvertGrainToFood(player, grainToUse);
            totalFoodGained += foodFromGrain;
            remainingNeeded -= foodFromGrain;
        }
        
        // 2. 次に羊から変換
        if (remainingNeeded > 0)
        {
            int availableSheep = player.GetResource(ResourceType.Sheep);
            if (availableSheep > 0)
            {
                int sheepToUse = Mathf.Min(availableSheep, Mathf.CeilToInt(remainingNeeded / 2f));
                int foodFromSheep = ConvertAnimalToFood(player, ResourceType.Sheep, sheepToUse);
                totalFoodGained += foodFromSheep;
                remainingNeeded -= foodFromSheep;
            }
        }
        
        // 3. 次に猪から変換
        if (remainingNeeded > 0)
        {
            int availableBoar = player.GetResource(ResourceType.Boar);
            if (availableBoar > 0)
            {
                int boarToUse = Mathf.Min(availableBoar, Mathf.CeilToInt(remainingNeeded / 3f));
                int foodFromBoar = ConvertAnimalToFood(player, ResourceType.Boar, boarToUse);
                totalFoodGained += foodFromBoar;
                remainingNeeded -= foodFromBoar;
            }
        }
        
        // 4. 最後に牛から変換
        if (remainingNeeded > 0)
        {
            int availableCattle = player.GetResource(ResourceType.Cattle);
            if (availableCattle > 0)
            {
                int cattleToUse = Mathf.Min(availableCattle, Mathf.CeilToInt(remainingNeeded / 4f));
                int foodFromCattle = ConvertAnimalToFood(player, ResourceType.Cattle, cattleToUse);
                totalFoodGained += foodFromCattle;
                remainingNeeded -= foodFromCattle;
            }
        }
        
        if (totalFoodGained > 0)
        {
            Debug.Log($"{player.playerName}が自動変換で食料{totalFoodGained}個を確保しました");
        }
        
        return totalFoodGained;
    }
    
    // プレイヤーが手動で変換できる最大食料量を計算
    public int CalculateMaxPossibleFood(Player player)
    {
        int maxFood = 0;
        
        // 現在の食料
        maxFood += player.GetResource(ResourceType.Food);
        
        // 穀物から変換可能な食料
        maxFood += player.GetResource(ResourceType.Grain) * grainToFoodRate[ResourceType.Grain];
        
        // 動物から変換可能な食料
        maxFood += player.GetResource(ResourceType.Sheep) * animalToFoodRate[ResourceType.Sheep];
        maxFood += player.GetResource(ResourceType.Boar) * animalToFoodRate[ResourceType.Boar];
        maxFood += player.GetResource(ResourceType.Cattle) * animalToFoodRate[ResourceType.Cattle];
        
        return maxFood;
    }
    
    // 変換オプションを取得（UI用）
    public List<ConversionOption> GetConversionOptions(Player player)
    {
        List<ConversionOption> options = new List<ConversionOption>();
        
        // 穀物変換オプション
        int availableGrain = player.GetResource(ResourceType.Grain);
        if (availableGrain > 0)
        {
            options.Add(new ConversionOption
            {
                sourceType = ResourceType.Grain,
                sourceAmount = 1,
                targetType = ResourceType.Food,
                targetAmount = grainToFoodRate[ResourceType.Grain],
                maxConversions = availableGrain,
                description = "穀物を食料に変換（製パン）"
            });
        }
        
        // 動物変換オプション
        foreach (var animalType in animalToFoodRate.Keys)
        {
            int availableAnimals = player.GetResource(animalType);
            if (availableAnimals > 0)
            {
                options.Add(new ConversionOption
                {
                    sourceType = animalType,
                    sourceAmount = 1,
                    targetType = ResourceType.Food,
                    targetAmount = animalToFoodRate[animalType],
                    maxConversions = availableAnimals,
                    description = $"{GetAnimalJapaneseName(animalType)}を食料に変換"
                });
            }
        }
        
        return options;
    }
    
    private string GetAnimalJapaneseName(ResourceType animalType)
    {
        switch (animalType)
        {
            case ResourceType.Sheep: return "羊";
            case ResourceType.Boar: return "猪";
            case ResourceType.Cattle: return "牛";
            default: return animalType.ToString();
        }
    }
}

[System.Serializable]
public class ConversionOption
{
    public ResourceType sourceType;
    public int sourceAmount;
    public ResourceType targetType;
    public int targetAmount;
    public int maxConversions;
    public string description;
}