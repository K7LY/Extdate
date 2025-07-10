using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Field
{
    public Vector2Int position; // プレイヤータイル上の座標
    public Dictionary<ResourceType, int> crops = new Dictionary<ResourceType, int>();
    
    public Field(Vector2Int pos)
    {
        position = pos;
    }
    
    public Field(int x, int y)
    {
        position = new Vector2Int(x, y);
    }
    
    public bool IsEmpty()
    {
        return crops.Count == 0 || crops.Values.All(count => count == 0);
    }
    
    public bool CanPlantCrop(ResourceType cropType, int amount)
    {
        // 各畑には最大3個まで同じ作物を植えられる（Agricola風）
        int currentAmount = crops.ContainsKey(cropType) ? crops[cropType] : 0;
        return currentAmount + amount <= 3;
    }
    
    public bool PlantCrop(ResourceType cropType, int amount)
    {
        if (!CanPlantCrop(cropType, amount)) return false;
        
        if (!crops.ContainsKey(cropType))
            crops[cropType] = 0;
        crops[cropType] += amount;
        return true;
    }
    
    public int HarvestCrop(ResourceType cropType, int maxAmount = int.MaxValue)
    {
        if (!crops.ContainsKey(cropType) || crops[cropType] == 0)
            return 0;
            
        int harvestedAmount = Mathf.Min(crops[cropType], maxAmount);
        crops[cropType] -= harvestedAmount;
        
        if (crops[cropType] == 0)
            crops.Remove(cropType);
            
        return harvestedAmount;
    }
    
    public Dictionary<ResourceType, int> GetAllCrops()
    {
        return new Dictionary<ResourceType, int>(crops);
    }
    
    public int GetCropCount(ResourceType cropType)
    {
        return crops.ContainsKey(cropType) ? crops[cropType] : 0;
    }
    
    public List<ResourceType> GetCropTypes()
    {
        return crops.Keys.ToList();
    }
}

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
    [SerializeField] private int fields = 0;        // 畑の数（後方互換性のため保持）
    [SerializeField] private Dictionary<Vector2Int, Field> fieldMap = new Dictionary<Vector2Int, Field>(); // 座標ベースの畑管理
    [SerializeField] private int pastures = 0;      // 牧場の数
    [SerializeField] private int fences = 0;        // 柵の数
    [SerializeField] private int stables = 0;       // 小屋の数
    
    [Header("プレイヤータイル座標設定")]
    [SerializeField] private int boardMinX = 0;     // プレイヤーボードのX座標最小値（拡張後）
    [SerializeField] private int boardMaxX = 8;     // プレイヤーボードのX座標最大値（拡張後）
    [SerializeField] private int boardMinY = 0;     // プレイヤーボードのY座標最小値（拡張後）
    [SerializeField] private int boardMaxY = 6;     // プレイヤーボードのY座標最大値（拡張後）
    [SerializeField] private int baseBoardMinX = 2; // 基本ボードのX座標最小値
    [SerializeField] private int baseBoardMaxX = 6; // 基本ボードのX座標最大値
    [SerializeField] private int baseBoardMinY = 2; // 基本ボードのY座標最小値
    [SerializeField] private int baseBoardMaxY = 4; // 基本ボードのY座標最大値
    
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
    public System.Action<int> OnMinorImprovementPlayable;
    public System.Action<int> OnOccupationPlayable;
    
    [Header("タイル管理")]
    [SerializeField] private TileManager tileManager; // TileManagerの参照
    
    void Start()
    {
        InitializeResources();
        InitializeFields();
        InitializeTileManager();
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
    
    private void InitializeFields()
    {
        // 畑は初期ゼロ、ゲーム中に追加していく
        fieldMap.Clear();
        fields = 0;
        
        Debug.Log($"{playerName}のプレイヤーボードを初期化しました（初期畑数: 0個）");
        Debug.Log($"基本ボード範囲: X={baseBoardMinX}-{baseBoardMaxX}, Y={baseBoardMinY}-{baseBoardMaxY}");
        Debug.Log($"拡張可能範囲: X={boardMinX}-{boardMaxX}, Y={boardMinY}-{boardMaxY}");
    }
    
    private void InitializeTileManager()
    {
        // TileManagerが設定されていない場合は自動で取得
        if (tileManager == null)
        {
            tileManager = FindObjectOfType<TileManager>();
        }
        
        // TileManagerが見つからない場合は警告
        if (tileManager == null)
        {
            Debug.LogWarning($"{playerName}: TileManagerが見つかりません。TileManagerとの連携機能は無効になります。");
        }
    }
    
    // リソース管理
    public int GetResource(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
    
    /// <summary>
    /// リソースを追加（OnReceiveトリガー付き）
    /// </summary>
    public void AddResource(ResourceType type, int amount)
    {
        AddResourceInternal(type, amount, true);
    }
    
    /// <summary>
    /// リソースを追加（内部用、トリガー制御可能）
    /// </summary>
    private void AddResourceInternal(ResourceType type, int amount, bool triggerReceiveEffects = true)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;
            
        resources[type] += amount;
        resources[type] = Mathf.Max(0, resources[type]);
        
        OnResourceChanged?.Invoke(type, resources[type]);
        
        // OnReceiveトリガーを発動（リソースが手持ちに入ったとき）
        if (amount > 0 && triggerReceiveEffects)
        {
            TriggerOnReceiveEffects(type, amount);
        }
    }
    
    /// <summary>
    /// アクションスペースからリソースを取得する（OnTake→OnReceiveの順で発動）
    /// </summary>
    public void TakeResourceFromAction(ResourceType type, int amount, ActionSpace actionSpace)
    {
        // 1. OnTakeトリガーを発動（アイテムを取得したとき）
        TriggerOnTakeEffects(type, amount, actionSpace, "action");
        
        // 2. 実際にリソースを追加（OnReceiveトリガーなし）
        AddResourceInternal(type, amount, false);
        
        // 3. OnReceiveトリガーを発動（アイテムが手持ちに入ったとき）
        TriggerOnReceiveEffects(type, amount, null, "take_action");
        
        Debug.Log($"{playerName}が{actionSpace.actionName}から{type}{amount}個を取得しました（Take→Receive）");
    }
    
    /// <summary>
    /// カード効果でリソースを取得する（OnTake→OnReceiveの順で発動）
    /// </summary>
    public void TakeResourceFromCardEffect(ResourceType type, int amount, string cardName = "")
    {
        // 1. OnTakeトリガーを発動
        TriggerOnTakeEffects(type, amount, null, "card_effect");
        
        // 2. 実際にリソースを追加（OnReceiveトリガーなし）
        AddResourceInternal(type, amount, false);
        
        // 3. OnReceiveトリガーを発動
        TriggerOnReceiveEffects(type, amount, null, "take_card_effect");
        
        Debug.Log($"{playerName}がカード効果「{cardName}」から{type}{amount}個を取得しました（Take→Receive）");
    }
    
    /// <summary>
    /// 直接リソースを受け取る（OnReceiveトリガーのみ発動）
    /// </summary>
    public void ReceiveResourceDirect(ResourceType type, int amount, Player sourcePlayer = null, string method = "direct")
    {
        // 1. 実際にリソースを追加（OnReceiveトリガーなし）
        AddResourceInternal(type, amount, false);
        
        // 2. OnReceiveトリガーを発動
        TriggerOnReceiveEffects(type, amount, sourcePlayer, method);
        
        string source = sourcePlayer != null ? $"{sourcePlayer.playerName}から" : "";
        Debug.Log($"{playerName}が{source}{type}{amount}個を受け取りました（Receiveのみ）");
    }
    
    /// <summary>
    /// OnTakeトリガー効果を発動（無限ループ防止付き）
    /// </summary>
    private void TriggerOnTakeEffects(ResourceType resourceType, int amount, ActionSpace actionSpace, string method)
    {
        // 無限ループ防止
        if (isTriggeringOnTake)
        {
            Debug.LogWarning($"OnTakeトリガーの無限ループを防止しました: {resourceType} {amount}個");
            return;
        }
        
        isTriggeringOnTake = true;
        
        try
        {
            var takeContext = new CardTriggerManager.TakeEventContext(this, resourceType, amount, actionSpace, method);
            
            // 職業効果のトリガー
            TriggerOccupationEffects(OccupationTrigger.OnTake, takeContext);
            
            // GameManagerのCardTriggerManagerを使用してトリガー効果を発動
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnTake, this, actionSpace);
            }
        }
        finally
        {
            isTriggeringOnTake = false;
        }
    }
    
    /// <summary>
    /// OnReceiveトリガー効果を発動（無限ループ防止付き）
    /// </summary>
    private void TriggerOnReceiveEffects(ResourceType resourceType, int amount, Player sourcePlayer = null, string method = "direct")
    {
        // 無限ループ防止
        if (isTriggeringOnReceive)
        {
            Debug.LogWarning($"OnReceiveトリガーの無限ループを防止しました: {resourceType} {amount}個");
            return;
        }
        
        isTriggeringOnReceive = true;
        
        try
        {
            var receiveContext = new CardTriggerManager.ReceiveEventContext(this, resourceType, amount, sourcePlayer, method);
            
            // 職業効果のトリガー
            TriggerOccupationEffects(OccupationTrigger.OnReceive, receiveContext);
            
            // GameManagerのCardTriggerManagerを使用してトリガー効果を発動
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnReceive, this);
            }
        }
        finally
        {
            isTriggeringOnReceive = false;
        }
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
    
    // 畑の追加（指定座標に）
    public bool AddField(Vector2Int position)
    {
        // 座標が有効かチェック
        if (!IsValidPosition(position))
        {
            Debug.LogWarning($"無効な座標です: ({position.x}, {position.y})");
            Debug.LogWarning($"有効範囲: X={boardMinX}-{boardMaxX}, Y={boardMinY}-{boardMaxY}");
            Debug.LogWarning($"基本ボード: X={baseBoardMinX}-{baseBoardMaxX}, Y={baseBoardMinY}-{baseBoardMaxY}");
            return false;
        }
        
        // 既に畑があるかチェック
        if (fieldMap.ContainsKey(position))
        {
            Debug.LogWarning($"座標({position.x}, {position.y})には既に畑があります");
            return false;
        }
        
        // 畑を追加
        fields++;
        fieldMap[position] = new Field(position);
        
        // TileManagerとの連携：タイルを畑タイプに設定
        if (tileManager != null)
        {
            tileManager.SetTileType(position, TileType.Field);
            Debug.Log($"TileManager統合: 座標({position.x}, {position.y})を畑タイルに設定しました");
        }
        
        // 追加場所の情報を表示
        string areaInfo = IsInBaseBoard(position) ? "基本ボード" : "拡張エリア";
        Debug.Log($"{playerName}が座標({position.x}, {position.y})の{areaInfo}に新しい畑を追加しました（合計: {fields}個）");
        
        return true; // Agricolaでは畑の追加は無料
    }
    
    // 畑の追加（座標指定版）
    public bool AddField(int x, int y)
    {
        return AddField(new Vector2Int(x, y));
    }
    
    // 畑の追加（従来版 - 自動座標選択）
    public bool AddField()
    {
        // 空いている座標を探して畑を追加
        for (int y = boardMinY; y <= boardMaxY; y++)
        {
            for (int x = boardMinX; x <= boardMaxX; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (IsValidPosition(position) && !fieldMap.ContainsKey(position))
                {
                    return AddField(position);
                }
            }
        }
        
        Debug.LogWarning($"{playerName}のプレイヤーボードに畑を追加できる空きがありません");
        return false;
    }
    
           /// <summary>
    /// 種まき - TileManagerに委譲
    /// </summary>
    public bool Sow(ResourceType cropType, Vector2Int position)
    {
        if (tileManager == null)
        {
            Debug.LogWarning($"{playerName}: TileManagerが設定されていないため、種まきできません");
            return false;
        }
        
        return tileManager.Sow(this, cropType, position);
    }

    /// <summary>
    /// 種まき - 座標指定版（x, y個別指定）
    /// </summary>
    public bool Sow(ResourceType cropType, int x, int y)
    {
        return Sow(cropType, new Vector2Int(x, y));
    }
    
    // 座標の有効性をチェック（拡張可能範囲内）
    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= boardMinX && position.x <= boardMaxX && 
               position.y >= boardMinY && position.y <= boardMaxY;
    }
    
    // 基本ボード内かどうかをチェック
    private bool IsInBaseBoard(Vector2Int position)
    {
        return position.x >= baseBoardMinX && position.x <= baseBoardMaxX && 
               position.y >= baseBoardMinY && position.y <= baseBoardMaxY;
    }
    
    // 拡張エリア内かどうかをチェック
    private bool IsInExtensionArea(Vector2Int position)
    {
        return IsValidPosition(position) && !IsInBaseBoard(position);
    }
    

    
    public int GetEmptyFields()
    {
        int emptyCount = 0;
        foreach (Field field in fieldMap.Values)
        {
            if (field.IsEmpty())
            {
                emptyCount++;
            }
        }
        return emptyCount;
    }
    
    /// <summary>
    /// 収穫 - TileManagerに委譲
    /// </summary>
    public void HarvestCrops()
    {
        if (tileManager == null)
        {
            Debug.LogWarning($"{playerName}: TileManagerが設定されていないため、収穫できません");
            return;
        }
        
        var harvestedCrops = tileManager.HarvestCrops(this);
        
        // 職業効果のトリガー
        TriggerOccupationEffects(OccupationTrigger.OnHarvest);
    }
    
    // 畑の状態を確認するメソッド（プレイヤーボードのグリッド表示）
    public void PrintFieldStatus()
    {
        int boardWidth = boardMaxX - boardMinX + 1;
        int boardHeight = boardMaxY - boardMinY + 1;
        
        Debug.Log($"=== {playerName}のプレイヤーボード ===");
        Debug.Log($"基本ボード: {baseBoardMaxX-baseBoardMinX+1}×{baseBoardMaxY-baseBoardMinY+1} (X={baseBoardMinX}-{baseBoardMaxX}, Y={baseBoardMinY}-{baseBoardMaxY})");
        Debug.Log($"拡張可能範囲: {boardWidth}×{boardHeight} (X={boardMinX}-{boardMaxX}, Y={boardMinY}-{boardMaxY})");
        Debug.Log($"畑の総数: {fields}個");
        Debug.Log($"空の畑: {GetEmptyFields()}個");
        Debug.Log("");
        
        // プレイヤーボードをグリッド形式で表示
        Debug.Log("プレイヤーボード配置:");
        for (int y = boardMinY; y <= boardMaxY; y++)
        {
            string row = $"  Y{y}: ";
            for (int x = boardMinX; x <= boardMaxX; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                bool isInBase = IsInBaseBoard(position);
                
                if (fieldMap.ContainsKey(position))
                {
                    Field field = fieldMap[position];
                    if (field.IsEmpty())
                    {
                        row += isInBase ? "[空畑] " : "[空拡] ";
                    }
                    else
                    {
                        var crops = field.GetAllCrops();
                        string cropInfo = string.Join(",", crops.Select(kv => $"{GetResourceShortName(kv.Key)}{kv.Value}"));
                        row += $"[{cropInfo}] ";
                    }
                }
                else
                {
                    row += isInBase ? "[基本] " : "[拡張] ";
                }
            }
            Debug.Log(row);
        }
        
        Debug.Log("");
        string axisLabels = "座標軸: ";
        for (int x = boardMinX; x <= boardMaxX; x++)
        {
            axisLabels += $"X{x}  ";
        }
        Debug.Log(axisLabels);
        Debug.Log("");
        Debug.Log("凡例:");
        Debug.Log("  [基本] = 基本ボード範囲（畑なし）");
        Debug.Log("  [拡張] = 拡張エリア（畑なし）");
        Debug.Log("  [空畑] = 基本ボードの空畑");
        Debug.Log("  [空拡] = 拡張エリアの空畑");
        Debug.Log("  [穀2] = 穀物2個などの作物");
        Debug.Log("");
        Debug.Log("畑の詳細:");
        if (fieldMap.Count == 0)
        {
            Debug.Log("  畑がありません。まず畑を追加してください。");
        }
        else
        {
            foreach (var fieldKV in fieldMap)
            {
                Vector2Int position = fieldKV.Key;
                Field field = fieldKV.Value;
                string areaInfo = IsInBaseBoard(position) ? "基本ボード" : "拡張エリア";
                
                if (field.IsEmpty())
                {
                    Debug.Log($"  座標({position.x},{position.y}) [{areaInfo}]: 空");
                }
                else
                {
                    var crops = field.GetAllCrops();
                    string cropInfo = string.Join(", ", crops.Select(kv => $"{GetResourceName(kv.Key)}×{kv.Value}"));
                    Debug.Log($"  座標({position.x},{position.y}) [{areaInfo}]: {cropInfo}");
                }
            }
        }
        
        Debug.Log("");
        Debug.Log("使用例:");
        Debug.Log("  player.AddField(2, 2);                     // 座標(2,2)の基本ボードに畑を追加");
        Debug.Log("  player.Sow(ResourceType.Grain, 2, 2);      // 穀物1個を座標(2,2)に");
        Debug.Log("  player.AddField(0, 0);                     // 座標(0,0)の拡張エリアに畑を追加");
    }
    
                     // 種まきの使用例を表示するメソッド
    public void ShowSowingExamples()
    {
        Debug.Log($"=== {playerName}の種まき使用例 ===");
        Debug.Log("基本的な使い方（常に作物種類と座標を指定）:");
        Debug.Log("  player.Sow(ResourceType.Grain, 2, 2);        // 穀物1個を座標(2,2)に");
        Debug.Log("  player.Sow(ResourceType.Vegetable, 3, 2);    // 野菜1個を座標(3,2)に");
        Debug.Log("  player.Sow(ResourceType.Wood, 4, 2);         // 木1個を座標(4,2)に");
        Debug.Log("  player.Sow(ResourceType.Reed, 2, 3);         // 葦1個を座標(2,3)に");
        Debug.Log("  player.Sow(ResourceType.Food, 3, 3);         // 食料1個を座標(3,3)に");
        Debug.Log("");
        Debug.Log("畑の管理:");
        Debug.Log("  player.AddField(2, 2);                       // 座標(2,2)の基本ボードに畑を追加");
        Debug.Log("  player.AddField(0, 0);                       // 座標(0,0)の拡張エリアに畑を追加");
        Debug.Log("  player.AddField();                           // 空いている座標に自動で畑を追加");
        Debug.Log("");
        Debug.Log("ルール:");
        Debug.Log("- 各畑には同じ作物を最大3個まで植えることができます");
        Debug.Log($"- 基本ボード: X={baseBoardMinX}-{baseBoardMaxX}, Y={baseBoardMinY}-{baseBoardMaxY}");
        Debug.Log($"- 拡張可能範囲: X={boardMinX}-{boardMaxX}, Y={boardMinY}-{boardMaxY}");
        Debug.Log("- 畑がない座標には種まきできません（まず畑を追加してください）");
        Debug.Log("- 拡張エリアは将来のゲーム拡張で使用可能になります");
    }
    
    // 特定の作物の合計数を畑から取得
    public int GetTotalCropsInFields(ResourceType cropType)
    {
        int total = 0;
        foreach (Field field in fieldMap.Values)
        {
            total += field.GetCropCount(cropType);
        }
        return total;
    }
    
    // 畑に植えられているすべての作物の統計を取得
    public Dictionary<ResourceType, int> GetAllCropsInFields()
    {
        Dictionary<ResourceType, int> allCrops = new Dictionary<ResourceType, int>();
        
        foreach (Field field in fieldMap.Values)
        {
            var fieldCrops = field.GetAllCrops();
            foreach (var cropKV in fieldCrops)
            {
                if (!allCrops.ContainsKey(cropKV.Key))
                    allCrops[cropKV.Key] = 0;
                allCrops[cropKV.Key] += cropKV.Value;
            }
        }
        
        return allCrops;
    }
    
    // 指定座標の畑情報を取得
    public Field GetFieldAt(Vector2Int position)
    {
        return fieldMap.ContainsKey(position) ? fieldMap[position] : null;
    }
    
    // 指定座標の畑情報を取得（座標個別指定版）
    public Field GetFieldAt(int x, int y)
    {
        return GetFieldAt(new Vector2Int(x, y));
    }
    
    // プレイヤーボード上のすべての畑の座標を取得
    public List<Vector2Int> GetAllFieldPositions()
    {
        return fieldMap.Keys.ToList();
    }
    
    // リソース名を日本語で取得（ログ用）
    private string GetResourceName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Grain: return "穀物";
            case ResourceType.Vegetable: return "野菜";
            case ResourceType.Wood: return "木材";
            case ResourceType.Reed: return "葦";
            case ResourceType.Food: return "食料";
            default: return resourceType.ToString();
        }
    }
    
    // リソース名を短縮形で取得（グリッド表示用）
    private string GetResourceShortName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Grain: return "穀";
            case ResourceType.Vegetable: return "野";
            case ResourceType.Wood: return "木";
            case ResourceType.Reed: return "葦";
            case ResourceType.Food: return "食";
            default: return resourceType.ToString().Substring(0, 1);
        }
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
        // 各動物種で2匹以上いれば1匹増える（繁殖による直接受取）
        if (GetResource(ResourceType.Sheep) >= 2 && CanHouseAnimals(ResourceType.Sheep, 1))
            ReceiveResourceDirect(ResourceType.Sheep, 1, null, "breeding");
            
        if (GetResource(ResourceType.Boar) >= 2 && CanHouseAnimals(ResourceType.Boar, 1))
            ReceiveResourceDirect(ResourceType.Boar, 1, null, "breeding");
            
        if (GetResource(ResourceType.Cattle) >= 2 && CanHouseAnimals(ResourceType.Cattle, 1))
            ReceiveResourceDirect(ResourceType.Cattle, 1, null, "breeding");
        
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
            
            // カードの効果を実行（EnhancedCardの場合）
            if (card is EnhancedCard enhancedCard)
            {
                enhancedCard.PlayCard(this);
                
                // GameManagerのCardTriggerManagerに新しいカードを通知
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    // 即座トリガー以外の効果をトリガー可能一覧に追加
                    RegisterCardEffectsToTriggerManager(enhancedCard, gameManager);
                }
            }
            
            OnCardPlayed?.Invoke(card);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// カードの効果をCardTriggerManagerに登録
    /// </summary>
    private void RegisterCardEffectsToTriggerManager(EnhancedCard card, GameManager gameManager)
    {
        // CardTriggerManagerを取得
        CardTriggerManager triggerManager = gameManager.GetComponent<CardTriggerManager>();
        if (triggerManager == null)
        {
            triggerManager = FindObjectOfType<CardTriggerManager>();
        }
        
        if (triggerManager != null)
        {
            // 新しいカードの効果を詳細分析
            triggerManager.AnalyzeNewCard(card, this);
            
            // トリガー可能カード一覧の更新をログ出力
            Debug.Log($"{playerName}が「{card.cardName}」を獲得。トリガー可能カード一覧が更新されました。");
        }
        else
        {
            // フォールバック：基本的なログ出力
            foreach (var effect in card.effects)
            {
                if (effect.triggerType != OccupationTrigger.Immediate)
                {
                    Debug.Log($"カード「{card.cardName}」の{effect.triggerType}トリガー効果「{effect.effectDescription}」がトリガー可能一覧に追加されました");
                }
            }
        }
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
    
    // トリガー実行中フラグ（無限ループ防止）
    private bool isTriggeringOnTake = false;
    private bool isTriggeringOnReceive = false;
    
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
            
            // EnhancedCardの場合、トリガー可能一覧に追加
            if (occupation is EnhancedCard enhancedOccupation)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    RegisterCardEffectsToTriggerManager(enhancedOccupation, gameManager);
                }
            }
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
            
            // EnhancedCardの場合、トリガー可能一覧に追加
            if (improvement is EnhancedCard enhancedImprovement)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    RegisterCardEffectsToTriggerManager(enhancedImprovement, gameManager);
                }
            }
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
    
    /// <summary>
    /// 新しいトリガーシステムのテスト用メソッド
    /// </summary>
    public void TestNewTriggerSystem()
    {
        Debug.Log("=== 新しいトリガーシステムのテスト開始 ===");
        
        // 1. アクションスペースからの取得テスト（Take→Receive）
        Debug.Log("1. アクションスペースからの取得テスト");
        ActionSpace testActionSpace = new ActionSpace();
        testActionSpace.actionName = "テスト木材獲得";
        TakeResourceFromAction(ResourceType.Wood, 2, testActionSpace);
        
        // 2. カード効果での取得テスト（Take→Receive）
        Debug.Log("2. カード効果での取得テスト");
        TakeResourceFromCardEffect(ResourceType.Clay, 1, "テストカード");
        
        // 3. 直接受取テスト（Receiveのみ）
        Debug.Log("3. 直接受取テスト");
        ReceiveResourceDirect(ResourceType.Food, 3, null, "test_direct");
        
        // 4. 通常のAddResourceテスト（Receiveのみ）
        Debug.Log("4. 通常のAddResourceテスト");
        AddResource(ResourceType.Grain, 1);
        
        Debug.Log("=== 新しいトリガーシステムのテスト完了 ===");
    }
    

}