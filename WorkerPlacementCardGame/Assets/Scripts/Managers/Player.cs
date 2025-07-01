using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Player : MonoBehaviour
{
    [Header("プレイヤー情報")]
    public string playerName;
    public Color playerColor = Color.blue;
    public bool isAI = false;
    
    [Header("リソース")]
    [SerializeField] private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    
    [Header("家族（ワーカー）")]
    public int maxFamilyMembers = 5;
    [SerializeField] private int familyMembers = 2; // 初期は夫婦2人
    [SerializeField] private int availableWorkers;
    [SerializeField] private List<Worker> placedWorkers = new List<Worker>();
    
    [Header("住居")]
    public enum HouseType { Wood, Clay, Stone }
    [SerializeField] private HouseType houseType = HouseType.Wood;
    [SerializeField] private int rooms = 2; // 初期は2部屋
    
    [Header("農場")]
    [SerializeField] private int fields = 0;        // 畑の数
    [SerializeField] private int pastures = 0;      // 牧場の数
    [SerializeField] private int fences = 0;        // 柵の数
    [SerializeField] private int stables = 0;       // 小屋の数
    
    [Header("カード")]
    [SerializeField] private List<Card> hand = new List<Card>();
    [SerializeField] private List<Card> playedCards = new List<Card>();
    [SerializeField] private List<OccupationCard> occupations = new List<OccupationCard>();
    [SerializeField] private List<ImprovementCard> improvements = new List<ImprovementCard>();
    
    [Header("勝利点")]
    [SerializeField] private int victoryPoints = 0;
    
    // イベント
    public System.Action<ResourceType, int> OnResourceChanged;
    public System.Action<int> OnVictoryPointsChanged;
    public System.Action<Card> OnCardPlayed;
    
    void Start()
    {
        InitializeResources();
        availableWorkers = familyMembers;
    }
    
    private void InitializeResources()
    {
        // Agricola風の初期リソース
        resources[ResourceType.Wood] = 0;
        resources[ResourceType.Clay] = 0;
        resources[ResourceType.Reed] = 0;
        resources[ResourceType.Stone] = 0;
        resources[ResourceType.Grain] = 0;
        resources[ResourceType.Vegetable] = 0;
        resources[ResourceType.Sheep] = 0;
        resources[ResourceType.Boar] = 0;
        resources[ResourceType.Cattle] = 0;
        resources[ResourceType.Food] = 0;
    }
    
    // リソース管理
    public int GetResource(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
    
    public void AddResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;
            
        resources[type] += amount;
        resources[type] = Mathf.Max(0, resources[type]);
        
        OnResourceChanged?.Invoke(type, resources[type]);
    }
    
    public bool SpendResource(ResourceType type, int amount)
    {
        if (GetResource(type) >= amount)
        {
            AddResource(type, -amount);
            return true;
        }
        return false;
    }
    
    // ワーカー管理
    public int GetAvailableWorkers()
    {
        return availableWorkers;
    }
    
    public bool PlaceWorker(ActionSpace actionSpace)
    {
        if (availableWorkers > 0 && actionSpace.CanPlaceWorker())
        {
            Worker worker = CreateWorker();
            if (actionSpace.PlaceWorker(worker))
            {
                placedWorkers.Add(worker);
                availableWorkers--;
                return true;
            }
        }
        return false;
    }
    
    public void ReturnAllWorkers()
    {
        foreach (Worker worker in placedWorkers)
        {
            if (worker.currentActionSpace != null)
            {
                worker.currentActionSpace.RemoveWorker();
            }
            Destroy(worker.gameObject);
        }
        placedWorkers.Clear();
        availableWorkers = familyMembers;
    }
    
    // 家族の成長
    public bool GrowFamily()
    {
        if (familyMembers < maxFamilyMembers && familyMembers < rooms)
        {
            familyMembers++;
            availableWorkers = familyMembers;
            return true;
        }
        return false;
    }
    
    // 住居の拡張
    public bool ExpandHouse(int newRooms, ResourceType material)
    {
        int materialCost = newRooms * rooms; // 現在の部屋数 × 新しい部屋数
        int reedCost = newRooms;
        
        if (GetResource(material) >= materialCost && GetResource(ResourceType.Reed) >= reedCost)
        {
            SpendResource(material, materialCost);
            SpendResource(ResourceType.Reed, reedCost);
            rooms += newRooms;
            return true;
        }
        return false;
    }
    
    // 住居の改築
    public bool RenovateHouse(HouseType newType)
    {
        if ((int)newType <= (int)houseType) return false;
        
        ResourceType material = houseType == HouseType.Wood ? ResourceType.Clay : ResourceType.Stone;
        int materialCost = rooms;
        int reedCost = 1;
        
        if (GetResource(material) >= materialCost && GetResource(ResourceType.Reed) >= reedCost)
        {
            SpendResource(material, materialCost);
            SpendResource(ResourceType.Reed, reedCost);
            houseType = newType;
            return true;
        }
        return false;
    }
    
    // 畑の追加
    public bool AddField()
    {
        fields++;
        return true; // Agricolaでは畑の追加は無料
    }
    
    // 種まき
    public bool SowGrain(int amount)
    {
        if (GetResource(ResourceType.Grain) >= amount && fields > 0)
        {
            SpendResource(ResourceType.Grain, amount);
            // 畑に穀物を配置（実装は簡略化）
            return true;
        }
        return false;
    }
    
    public bool SowVegetable(int amount)
    {
        if (GetResource(ResourceType.Vegetable) >= amount && fields > 0)
        {
            SpendResource(ResourceType.Vegetable, amount);
            // 畑に野菜を配置（実装は簡略化）
            return true;
        }
        return false;
    }
    
    public int GetEmptyFields()
    {
        // 簡略化：総畑数を返す（実際には種まき済みの畑を除く必要がある）
        return fields;
    }
    
    // 収穫
    public void HarvestCrops()
    {
        // 簡略化：畑の数だけ穀物を獲得
        if (fields > 0)
        {
            AddResource(ResourceType.Grain, fields);
        }
        
        // 職業効果のトリガー
        TriggerOccupationEffects(OccupationTrigger.OnHarvest);
    }
    
    // 動物の飼育
    public bool CanHouseAnimals(ResourceType animalType, int count)
    {
        // 簡略化：牧場と小屋で動物を飼育可能
        int capacity = pastures * 2 + stables; // 牧場1つにつき2匹、小屋1つにつき1匹
        int currentAnimals = GetResource(ResourceType.Sheep) + GetResource(ResourceType.Boar) + GetResource(ResourceType.Cattle);
        
        return currentAnimals + count <= capacity;
    }
    
    // 動物の繁殖
    public void BreedAnimals()
    {
        // 各動物種で2匹以上いれば1匹増える
        if (GetResource(ResourceType.Sheep) >= 2 && CanHouseAnimals(ResourceType.Sheep, 1))
            AddResource(ResourceType.Sheep, 1);
            
        if (GetResource(ResourceType.Boar) >= 2 && CanHouseAnimals(ResourceType.Boar, 1))
            AddResource(ResourceType.Boar, 1);
            
        if (GetResource(ResourceType.Cattle) >= 2 && CanHouseAnimals(ResourceType.Cattle, 1))
            AddResource(ResourceType.Cattle, 1);
        
        // 職業効果のトリガー
        TriggerOccupationEffects(OccupationTrigger.OnBreeding);
    }
    
    // 食料の必要量を計算
    public int GetFoodNeeded()
    {
        return familyMembers * 2; // 1人につき2食料
    }
    
    // 餌やり（リソース変換システム統合）
    public int FeedFamily()
    {
        int needed = GetFoodNeeded();
        int available = GetResource(ResourceType.Food);
        
        if (available >= needed)
        {
            SpendResource(ResourceType.Food, needed);
            return 0; // 乞食カードなし
        }
        else
        {
            // 利用可能な食料を使用
            if (available > 0)
                SpendResource(ResourceType.Food, available);
            
            int stillNeeded = needed - available;
            
            // リソース変換システムを使用して食料を確保
            ResourceConverter converter = FindObjectOfType<ResourceConverter>();
            if (converter != null)
            {
                int convertedFood = converter.AutoConvertForFood(this, stillNeeded);
                stillNeeded -= convertedFood;
            }
            
            // それでも不足している場合は乞食カード
            return Mathf.Max(0, stillNeeded);
        }
    }
    
    private Worker CreateWorker()
    {
        GameObject workerObj = new GameObject("Worker");
        Worker worker = workerObj.AddComponent<Worker>();
        worker.owner = this;
        return worker;
    }
    
    // カード管理
    public List<Card> GetHand()
    {
        return new List<Card>(hand);
    }
    
    public void AddCardToHand(Card card)
    {
        hand.Add(card);
    }
    
    public bool PlayCard(Card card)
    {
        if (hand.Contains(card) && card.CanPlay(this))
        {
            // コストを支払う
            foreach (var cost in card.cost)
            {
                SpendResource(cost.Key, cost.Value);
            }
            
            // カードを手札から除去してプレイ済みに追加
            hand.Remove(card);
            playedCards.Add(card);
            
            OnCardPlayed?.Invoke(card);
            return true;
        }
        return false;
    }
    
    public void DrawCards(int count)
    {
        // カードドロー処理（デッキシステムが必要）
    }
    
    // 勝利点管理
    public int GetVictoryPoints()
    {
        return victoryPoints;
    }
    
    public void AddVictoryPoints(int points)
    {
        victoryPoints += points;
        OnVictoryPointsChanged?.Invoke(victoryPoints);
    }
    
    public void SetVictoryPoints(int points)
    {
        victoryPoints = points;
        OnVictoryPointsChanged?.Invoke(victoryPoints);
    }
    
    public List<Card> GetPlayedCards()
    {
        return new List<Card>(playedCards);
    }
    
    // ターン終了処理
    public void EndTurn()
    {
        // 必要に応じてターン終了時の処理を追加
    }
    
    // Getter methods for Agricola-style properties
    public int GetFamilyMembers() => familyMembers;
    public int GetRooms() => rooms;
    public HouseType GetHouseType() => houseType;
    public int GetFields() => fields;
    public int GetPastures() => pastures;
    public int GetFences() => fences;
    public int GetStables() => stables;
    
    public void AddFences(int count) 
    { 
        fences += count; 
        // 柵3つで牧場1つとして計算（簡略化）
        pastures = fences / 3;
    }
    
    public void AddStables(int count) 
    { 
        stables += count; 
    }
    
    // 一時的ボーナス管理（5人プレイ特殊アクション用）
    private Dictionary<string, int> tempBonuses = new Dictionary<string, int>();
    
    // カード効果管理
    private List<CardEffect> passiveEffects = new List<CardEffect>();
    private List<CardEffect> actionModifiers = new List<CardEffect>();
    private Dictionary<ResourceType, int> cookingAbilities = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> storageCapacities = new Dictionary<ResourceType, int>();
    private List<ImprovementCard> cookingFacilities = new List<ImprovementCard>();
    
    public void AddTempBonus(string bonusType, int amount)
    {
        if (!tempBonuses.ContainsKey(bonusType))
            tempBonuses[bonusType] = 0;
        tempBonuses[bonusType] += amount;
    }
    
    public int GetTempBonus(string bonusType)
    {
        return tempBonuses.ContainsKey(bonusType) ? tempBonuses[bonusType] : 0;
    }
    
    public void ClearTempBonus(string bonusType)
    {
        if (tempBonuses.ContainsKey(bonusType))
            tempBonuses.Remove(bonusType);
    }
    
    // 職業カード管理
    public void AddOccupation(OccupationCard occupation)
    {
        if (!occupations.Contains(occupation))
        {
            occupations.Add(occupation);
            Debug.Log($"{playerName}が職業「{occupation.cardName}」を獲得しました");
        }
    }
    
    public bool HasOccupation(OccupationType type)
    {
        return occupations.Any(o => o.occupationType == type);
    }
    
    public bool HasOccupationByName(string cardName)
    {
        return occupations.Any(o => o.cardName.Equals(cardName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    public List<OccupationCard> GetOccupations()
    {
        return new List<OccupationCard>(occupations);
    }
    
    // 進歩カード管理
    public void AddImprovement(ImprovementCard improvement)
    {
        if (!improvements.Contains(improvement))
        {
            improvements.Add(improvement);
            Debug.Log($"{playerName}が進歩「{improvement.cardName}」を獲得しました");
        }
    }
    
    public bool HasImprovement(ImprovementCard improvement)
    {
        return improvements.Contains(improvement);
    }
    
    public bool HasImprovementByName(string cardName)
    {
        return improvements.Any(i => i.cardName.Equals(cardName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    public List<ImprovementCard> GetImprovements()
    {
        return new List<ImprovementCard>(improvements);
    }
    
    public bool HasCardWithTag(string tagName)
    {
        // 職業カードのタグをチェック
        foreach (var occupation in occupations)
        {
            if (occupation is EnhancedCard enhancedOccupation && enhancedOccupation.HasTag(tagName))
                return true;
        }
        
        // 進歩カードのタグをチェック
        foreach (var improvement in improvements)
        {
            if (improvement is EnhancedCard enhancedImprovement && enhancedImprovement.HasTag(tagName))
                return true;
        }
        
        // プレイ済みカードのタグをチェック
        foreach (var card in playedCards)
        {
            if (card is EnhancedCard enhancedCard && enhancedCard.HasTag(tagName))
                return true;
        }
        
        return false;
    }
    
    // カード効果管理
    public void AddPassiveEffect(Card source, CardEffect effect)
    {
        passiveEffects.Add(effect);
        Debug.Log($"{playerName}に継続効果「{effect.description}」が追加されました");
    }
    
    public void AddActionModifier(Card source, CardEffect effect)
    {
        actionModifiers.Add(effect);
        Debug.Log($"{playerName}にアクション修正「{effect.description}」が追加されました");
    }
    
    // 料理設備管理
    public void AddCookingFacility(ImprovementCard facility)
    {
        if (!cookingFacilities.Contains(facility))
        {
            cookingFacilities.Add(facility);
        }
    }
    
    public bool HasCookingFacility()
    {
        return cookingFacilities.Count > 0;
    }
    
    public void AddCookingAbility(ResourceType resourceType, int conversionRate)
    {
        if (!cookingAbilities.ContainsKey(resourceType))
            cookingAbilities[resourceType] = 0;
        
        // より高い変換レートを保持
        cookingAbilities[resourceType] = Mathf.Max(cookingAbilities[resourceType], conversionRate);
    }
    
    public int GetCookingRate(ResourceType resourceType)
    {
        return cookingAbilities.ContainsKey(resourceType) ? cookingAbilities[resourceType] : 0;
    }
    
    // 貯蔵能力管理
    public void AddStorageCapacity(ResourceType resourceType, int capacity)
    {
        if (!storageCapacities.ContainsKey(resourceType))
            storageCapacities[resourceType] = 0;
        
        storageCapacities[resourceType] += capacity;
    }
    
    public int GetStorageCapacity(ResourceType resourceType)
    {
        return storageCapacities.ContainsKey(resourceType) ? storageCapacities[resourceType] : 0;
    }
    
    // 追加アクションスペース（進歩カードによる）
    public void AddAdditionalActionSpaces(int count)
    {
        // 実装予定：プレイヤー専用アクションスペースの追加
        Debug.Log($"{playerName}に追加アクションスペース{count}個が追加されました");
    }
    
    // 料理実行
    public int CookResource(ResourceType resourceType, int amount)
    {
        int conversionRate = GetCookingRate(resourceType);
        if (conversionRate == 0) return 0;
        
        if (GetResource(resourceType) < amount) return 0;
        
        SpendResource(resourceType, amount);
        int foodGained = amount * conversionRate;
        AddResource(ResourceType.Food, foodGained);
        
        Debug.Log($"{playerName}が{resourceType}{amount}個を食料{foodGained}個に調理しました");
        return foodGained;
    }
    
    // 職業効果トリガー
    public void TriggerOccupationEffects(OccupationTrigger trigger, object context = null)
    {
        foreach (var occupation in occupations)
        {
            occupation.TriggerEffect(this, trigger, context);
        }
    }
    
    // アクション実行時の職業効果トリガー
    public void OnActionExecuted(ActionSpace actionSpace)
    {
        TriggerOccupationEffects(OccupationTrigger.OnAction, actionSpace);
    }
    
    // ターン終了時の処理
    public void EndTurn()
    {
        TriggerOccupationEffects(OccupationTrigger.OnTurnEnd);
        
        // GameManagerのCardTriggerManagerを使用してターン終了時の効果を発動
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnTurnEnd, this);
        }
        
        // カード効果の使用回数リセット
        foreach (var effect in passiveEffects)
        {
            effect.ResetUses();
        }
        foreach (var effect in actionModifiers)
        {
            effect.ResetUses();
        }
    }
}