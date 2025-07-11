using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public enum ActionType
{
    GainResources,      // リソース獲得
    AddField,           // 畑追加
    SowField,           // 種まき（拡張）
    SowGrain,           // 種まき  
    FamilyGrowth,       // 家族の成長
    HouseExpansion,     // 住居拡張
    HouseRenovation,    // 住居改築
    BuildFences,        // 柵の建設
    BuildStables,       // 小屋の建設
    TakeAnimals,        // 動物の獲得
    PlayOccupation,     // 職業カード
    PlayImprovement,    // 改良カード
    StartingPlayer,     // スタートプレイヤー
    TradeResources,     // リソース交換
    SpecialAction,      // 特殊アクション
    Special             // 特殊アクション（互換性のため残す）
}

public class ActionSpace : MonoBehaviour
{
    [Header("基本情報")]
    public string actionId;           // 累積システムで使用する一意ID
    public string actionName;
    public string description;
    public ActionType actionType;
    public bool allowMultipleWorkers = false;
    public int maxWorkers = 1;
    
    [Header("固有効果のみ")]
    public List<ActionEffect> coreEffects = new List<ActionEffect>();
    public List<ResourceRequirement> resourceRequirements = new List<ResourceRequirement>();
    public int cardsToDraw = 0;
    public int victoryPoints = 0;
    
    [Header("累積システム（従来互換）")]
    [System.Obsolete("Use AccumulatedItemManager instead")]
    public Dictionary<ResourceType, int> resourceGain = new Dictionary<ResourceType, int>();
    [System.Obsolete("Use AccumulatedItemManager instead")]
    public List<ResourceReward> resourceRewards = new List<ResourceReward>();
    
    [Header("配置されたワーカー")]
    [SerializeField] private List<Worker> placedWorkers = new List<Worker>();
    
    [Header("ビジュアル")]
    public SpriteRenderer backgroundRenderer;
    public Color defaultColor = Color.white;
    public Color occupiedColor = Color.gray;
    
    // イベント
    public System.Action<Worker> OnWorkerPlaced;
    public System.Action<Worker> OnWorkerRemoved;
    
    // 累積システムへの参照（読み取り専用）
    private AccumulatedItemManager accumulatedItemManager;
    
    void Awake()
    {
        if (backgroundRenderer == null)
            backgroundRenderer = GetComponent<SpriteRenderer>();
            
        // Collider2Dを追加（クリック検出用）
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();
    }
    
    void Start()
    {
        // 一意IDの自動生成
        if (string.IsNullOrEmpty(actionId))
        {
            actionId = AccumulationUtils.NormalizeActionSpaceId(actionName);
            if (string.IsNullOrEmpty(actionId))
            {
                actionId = $"{actionName}_{GetInstanceID()}";
            }
        }
        
        // 累積システムへの参照を取得
        accumulatedItemManager = FindObjectOfType<AccumulatedItemManager>();
        
        UpdateVisual();
    }
    
    public bool CanPlaceWorker()
    {
        return placedWorkers.Count < maxWorkers;
    }
    
    public bool PlaceWorker(Worker worker)
    {
        if (!CanPlaceWorker())
            return false;
            
        // 1. 累積アイテムを取得して消費
        if (accumulatedItemManager != null)
        {
            var accumulatedItems = accumulatedItemManager.ConsumeAccumulatedItems(actionId);
            foreach (var item in accumulatedItems)
            {
                worker.owner.AddResource(item.Key, item.Value);
                Debug.Log($"累積アイテム獲得: {GetResourceJapaneseName(item.Key)} x{item.Value}");
            }
        }
        
        // 2. 固有効果を実行
        ExecuteCoreEffects(worker.owner);
        
        // 3. 従来のシステムとの互換性（段階的削除予定）
        ExecuteLegacyResourceGain(worker.owner);
        
        // 4. ワーカー配置処理
        placedWorkers.Add(worker);
        worker.SetActionSpace(this);
        worker.transform.position = GetWorkerPosition(placedWorkers.Count - 1);
        
        // 5. 職業効果のトリガー
        worker.owner.OnActionExecuted(this);
        
        UpdateVisual();
        OnWorkerPlaced?.Invoke(worker);
        
        return true;
    }
    
    public void RemoveWorker()
    {
        if (placedWorkers.Count > 0)
        {
            Worker worker = placedWorkers[placedWorkers.Count - 1];
            placedWorkers.RemoveAt(placedWorkers.Count - 1);
            worker.SetActionSpace(null);
            
            UpdateVisual();
            OnWorkerRemoved?.Invoke(worker);
        }
    }
    
    public void RemoveAllWorkers()
    {
        while (placedWorkers.Count > 0)
        {
            RemoveWorker();
        }
    }
    
    private Vector3 GetWorkerPosition(int index)
    {
        // ワーカーを配置する位置を計算
        Vector3 basePosition = transform.position;
        if (maxWorkers == 1)
        {
            return basePosition;
        }
        else
        {
            // 複数ワーカー対応の場合の位置計算
            float spacing = 0.5f;
            return basePosition + Vector3.right * (index - (maxWorkers - 1) * 0.5f) * spacing;
        }
    }
    
    /// <summary>
    /// 固有効果のみを実行（累積アイテムは関与しない）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    private void ExecuteCoreEffects(Player player)
    {
        // 新しいActionEffectシステムによる効果実行
        foreach (var effect in coreEffects)
        {
            if (effect.CanExecute(player))
            {
                effect.Execute(player);
            }
        }
        
        // アクションタイプ別の処理（固有効果のみ）
        switch (actionType)
        {
            case ActionType.AddField:
                ExecuteAddField(player);
                break;
            case ActionType.SowField:
                ExecuteSowField(player);
                break;
            case ActionType.SowGrain:
                ExecuteSowGrain(player);
                break;
            case ActionType.FamilyGrowth:
                ExecuteFamilyGrowth(player);
                break;
            case ActionType.HouseExpansion:
                ExecuteHouseExpansion(player);
                break;
            case ActionType.HouseRenovation:
                ExecuteHouseRenovation(player);
                break;
            case ActionType.BuildFences:
                ExecuteBuildFences(player);
                break;
            case ActionType.BuildStables:
                ExecuteBuildStables(player);
                break;
            case ActionType.TakeAnimals:
                ExecuteTakeAnimals(player);
                break;
            case ActionType.PlayOccupation:
                ExecutePlayOccupation(player);
                break;
            case ActionType.PlayImprovement:
                ExecutePlayImprovement(player);
                break;
            case ActionType.StartingPlayer:
                ExecuteStartingPlayer(player);
                break;
            case ActionType.TradeResources:
                ExecuteTradeResources(player);
                break;
            case ActionType.SpecialAction:
                ExecuteSpecialAction(player);
                break;
            case ActionType.Special:
                ExecuteSpecialAction(player);
                break;
        }
        
        // 勝利点があれば追加
        if (victoryPoints > 0)
        {
            player.AddVictoryPoints(victoryPoints);
        }
    }
    
    /// <summary>
    /// 従来のリソース獲得システムとの互換性（段階的削除予定）
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    [System.Obsolete("This method will be removed in future versions")]
    private void ExecuteLegacyResourceGain(Player player)
    {
        // 従来の resourceGain システム
        foreach (var resource in resourceGain)
        {
            player.AddResource(resource.Key, resource.Value);
            Debug.Log($"[Legacy] {player.playerName}が{GetResourceJapaneseName(resource.Key)}を{resource.Value}個獲得");
        }
        
        // 従来の resourceRewards システム
        foreach (var reward in resourceRewards)
        {
            player.AddResource(reward.resourceType, reward.amount);
            Debug.Log($"[Legacy] {player.playerName}が{GetResourceJapaneseName(reward.resourceType)}を{reward.amount}個獲得");
        }
    }
    
    /// <summary>
    /// 累積アイテムの確認（読み取り専用）
    /// </summary>
    /// <returns>現在累積されているアイテム</returns>
    public Dictionary<ResourceType, int> GetAccumulatedItems()
    {
        return accumulatedItemManager?.GetAccumulatedItems(actionId) ?? new Dictionary<ResourceType, int>();
    }
    
    /// <summary>
    /// 累積アイテムが存在するかチェック
    /// </summary>
    /// <returns>累積アイテムが存在する場合true</returns>
    public bool HasAccumulatedItems()
    {
        return GetAccumulatedItems().Count > 0;
    }
    
    /// <summary>
    /// 累積アイテムの総数を取得
    /// </summary>
    /// <returns>累積アイテムの総数</returns>
    public int GetAccumulatedItemCount()
    {
        return GetAccumulatedItems().Values.Sum();
    }
    
    /// <summary>
    /// 特定のリソースの累積量を取得
    /// </summary>
    /// <param name="resourceType">リソース種別</param>
    /// <returns>累積量</returns>
    public int GetAccumulatedAmount(ResourceType resourceType)
    {
        var items = GetAccumulatedItems();
        return items.ContainsKey(resourceType) ? items[resourceType] : 0;
    }
    
    private void ExecuteAddField(Player player)
    {
        player.AddField();
        Debug.Log($"{player.playerName}が畑を追加しました");
    }
    
    private void ExecuteSowField(Player player)
    {
        // 拡張版種まき：穀物または野菜を選択して種まき
        int grainSeeds = player.GetResource(ResourceType.Grain);
        int vegetableSeeds = player.GetResource(ResourceType.Vegetable);
        int availableFields = player.GetEmptyFields();
        
        if (availableFields > 0 && (grainSeeds > 0 || vegetableSeeds > 0))
        {
            // 簡易AI: 穀物を優先して種まき
            bool sowingSuccess = false;
            var fieldPositions = player.GetAllFieldPositions();
            
            foreach (var position in fieldPositions)
            {
                var field = player.GetFieldAt(position);
                if (field != null && field.IsEmpty())
                {
                    if (grainSeeds > 0 && player.Sow(ResourceType.Grain, position))
                    {
                        Debug.Log($"{player.playerName}が座標({position.x}, {position.y})に穀物の種まきをしました");
                        sowingSuccess = true;
                        break;
                    }
                    else if (vegetableSeeds > 0 && player.Sow(ResourceType.Vegetable, position))
                    {
                        Debug.Log($"{player.playerName}が座標({position.x}, {position.y})に野菜の種まきをしました");
                        sowingSuccess = true;
                        break;
                    }
                }
            }
            
            if (!sowingSuccess)
            {
                Debug.Log($"{player.playerName}は種まきできません（空きの畑がない）");
            }
        }
        else
        {
            Debug.Log($"{player.playerName}は種まきできません（畑または種が不足）");
        }
    }
    
    private void ExecuteSowGrain(Player player)
    {
        // プレイヤーの穀物を使って種まき
        if (player.GetResource(ResourceType.Grain) > 0 && player.GetFields() > 0)
        {
            bool sowingSuccess = false;
            var fieldPositions = player.GetAllFieldPositions();
            
            foreach (var position in fieldPositions)
            {
                var field = player.GetFieldAt(position);
                if (field != null && field.IsEmpty())
                {
                    if (player.Sow(ResourceType.Grain, position))
                    {
                        Debug.Log($"{player.playerName}が座標({position.x}, {position.y})に穀物の種まきをしました");
                        sowingSuccess = true;
                        break;
                    }
                }
            }
            
            if (!sowingSuccess)
            {
                Debug.Log($"{player.playerName}は種まきできません（空きの畑がない）");
            }
        }
        else
        {
            Debug.Log($"{player.playerName}は種まきできません（穀物または畑が不足）");
        }
    }
    
    private void ExecuteFamilyGrowth(Player player)
    {
        if (player.GrowFamily())
        {
            Debug.Log($"{player.playerName}の家族が増えました");
        }
        else
        {
            Debug.Log($"{player.playerName}は家族を増やせません（部屋が足りません）");
        }
    }
    
    private void ExecuteHouseExpansion(Player player)
    {
        // 木材で住居拡張
        if (player.ExpandHouse(1, ResourceType.Wood))
        {
            Debug.Log($"{player.playerName}が住居を拡張しました");
        }
        else
        {
            Debug.Log($"{player.playerName}は住居を拡張できません（リソース不足）");
        }
    }
    
    private void ExecuteHouseRenovation(Player player)
    {
        // 住居の改築
        Player.HouseType currentType = player.GetHouseType();
        Player.HouseType nextType = currentType == Player.HouseType.Wood ? 
            Player.HouseType.Clay : Player.HouseType.Stone;
            
        if (player.RenovateHouse(nextType))
        {
            Debug.Log($"{player.playerName}が住居を改築しました");
        }
        else
        {
            Debug.Log($"{player.playerName}は住居を改築できません");
        }
    }
    
    private void ExecuteBuildFences(Player player)
    {
        // 木材を使って柵を建設
        int woodAvailable = player.GetResource(ResourceType.Wood);
        if (woodAvailable > 0)
        {
            player.SpendResource(ResourceType.Wood, woodAvailable);
            player.AddFences(woodAvailable);
            Debug.Log($"{player.playerName}が柵を{woodAvailable}本建設しました");
        }
    }
    
    private void ExecuteBuildStables(Player player)
    {
        // 木材2個で小屋1つ建設
        if (player.GetResource(ResourceType.Wood) >= 2)
        {
            player.SpendResource(ResourceType.Wood, 2);
            player.AddStables(1);
            Debug.Log($"{player.playerName}が小屋を建設しました");
        }
    }
    
    private void ExecuteTakeAnimals(Player player)
    {
        // 固有効果による動物獲得（ActionEffectシステムで処理）
        // 従来の resourceGain システムとの互換性
        foreach (var resource in resourceGain)
        {
            if (resource.Key == ResourceType.Sheep || 
                resource.Key == ResourceType.Boar || 
                resource.Key == ResourceType.Cattle)
            {
                if (player.CanHouseAnimals(resource.Key, resource.Value))
                {
                    player.AddResource(resource.Key, resource.Value);
                    Debug.Log($"{player.playerName}が{GetResourceJapaneseName(resource.Key)}を{resource.Value}匹獲得しました");
                }
                else
                {
                    Debug.Log($"{player.playerName}は動物を飼うスペースがありません");
                }
            }
        }
    }
    
    private void ExecutePlayOccupation(Player player)
    {
        Debug.Log($"{player.playerName}が職業カードをプレイできます");
        // 実際の実装では、プレイヤーの手札から職業カードを選択
    }
    
    private void ExecutePlayImprovement(Player player)
    {
        DebugLog($"{player.playerName}が進歩カードをプレイできます");
        
        // ImprovementManagerを取得
        var improvementManager = FindObjectOfType<ImprovementManager>();
        if (improvementManager == null)
        {
            Debug.LogWarning("ImprovementManagerが見つかりません");
            return;
        }
        
        // アクション名に基づいて進歩タイプを決定
        switch (actionName)
        {
            case "小さな進歩":
                improvementManager.PlayImprovement(player, true, false, 1);
                break;
                
            case "大きな進歩":
                improvementManager.PlayImprovement(player, false, true, 1);
                break;
                
            case "職業・小進歩":
                // 職業カードも含むが、ここでは小さい進歩のみ対応
                improvementManager.PlayImprovement(player, true, false, 1);
                break;
                
            case "改良":
            case "進歩カード":
                // 両方のオプションを提供（プレイヤーが選択）
                improvementManager.PlayImprovement(player, true, true, 1);
                break;
                
            default:
                // デフォルトは小さな進歩
                improvementManager.PlayImprovement(player, true, false, 1);
                break;
        }
    }
    
    private void ExecuteStartingPlayer(Player player)
    {
        // スタートプレイヤートークンの獲得
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // gameManager.SetStartingPlayer(player);
            Debug.Log($"{player.playerName}が次のラウンドのスタートプレイヤーになります");
        }
    }
    
    private string GetResourceJapaneseName(ResourceType resourceType)
    {
        return AccumulationUtils.GetResourceJapaneseName(resourceType);
    }
    
    private void DebugLog(string message)
    {
        Debug.Log($"[ActionSpace:{actionName}] {message}");
    }
    
    private void ExecuteTradeResources(Player player)
    {
        // リソース交換：アクション名によって交換内容を決定
        switch (actionName)
        {
            case "市場での交易":
                // 複数の交換オプションから選択（簡易AI）
                ExecuteMarketTrade(player);
                break;
                
            case "手工業":
                // 木材や粘土を食料に変換
                ExecuteCraftingTrade(player);
                break;
                
            default:
                // 基本的な交換：木材3個→食料2個
                if (player.GetResource(ResourceType.Wood) >= 3)
                {
                    player.SpendResource(ResourceType.Wood, 3);
                    player.AddResource(ResourceType.Food, 2);
                    Debug.Log($"{player.playerName}が木材3個を食料2個に交換しました");
                }
                break;
        }
    }
    
    private void ExecuteMarketTrade(Player player)
    {
        Debug.Log($"{player.playerName}が市場で交易します");
        
        // 優先順位に基づく交換（簡易AI）
        if (player.GetResource(ResourceType.Wood) >= 2)
        {
            player.SpendResource(ResourceType.Wood, 2);
            player.AddResource(ResourceType.Food, 2);
            Debug.Log($"  木材2個 → 食料2個");
        }
        else if (player.GetResource(ResourceType.Clay) >= 2)
        {
            player.SpendResource(ResourceType.Clay, 2);
            player.AddResource(ResourceType.Food, 1);
            Debug.Log($"  粘土2個 → 食料1個");
        }
        else
        {
            Debug.Log($"  交換可能なリソースがありません");
        }
    }
    
    private void ExecuteCraftingTrade(Player player)
    {
        Debug.Log($"{player.playerName}が手工業を行います");
        
        // 手工業：木材1個 → 食料1個（効率的な変換）
        if (player.GetResource(ResourceType.Wood) >= 1)
        {
            player.SpendResource(ResourceType.Wood, 1);
            player.AddResource(ResourceType.Food, 1);
            Debug.Log($"  木材1個 → 食料1個");
        }
        else
        {
            Debug.Log($"  手工業に必要な木材がありません");
        }
    }
    
    private void ExecuteSpecialAction(Player player)
    {
        // 特殊アクション：アクション名によって動作を決定
        switch (actionName)
        {
            case "収穫の最適化":
                // 次の収穫で追加ボーナス
                player.AddTempBonus("harvest_bonus", 1);
                Debug.Log($"{player.playerName}が収穫最適化効果を得ました");
                break;
                
            case "近隣の助け":
                // 小さなリソースボーナス（木材1個と食料1個）
                player.AddResource(ResourceType.Wood, 1);
                player.AddResource(ResourceType.Food, 1);
                Debug.Log($"{player.playerName}が近隣の助けを得ました（木材1個、食料1個）");
                break;
                
            default:
                Debug.Log($"{player.playerName}が特殊アクションを実行しました: {actionName}");
                break;
        }
    }
    
    private void UpdateVisual()
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = placedWorkers.Count > 0 ? occupiedColor : defaultColor;
        }
    }
    
    void OnMouseDown()
    {
        // アクションスペースがクリックされた時の処理
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnActionSpaceClicked(this);
        }
    }
}