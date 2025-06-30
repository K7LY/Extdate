using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum ActionType
{
    GainResources,      // リソース獲得
    AddField,           // 畑追加
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
    Special             // 特殊アクション
}

public class ActionSpace : MonoBehaviour
{
    [Header("アクション情報")]
    public string actionName;
    public string description;
    public ActionType actionType;
    public bool allowMultipleWorkers = false;
    public int maxWorkers = 1;
    
    [Header("リソース効果")]
    public Dictionary<ResourceType, int> resourceGain = new Dictionary<ResourceType, int>();
    public int cardsToDraw = 0;
    public int victoryPoints = 0;
    
    [Header("配置されたワーカー")]
    [SerializeField] private List<Worker> placedWorkers = new List<Worker>();
    
    [Header("ビジュアル")]
    public SpriteRenderer backgroundRenderer;
    public Color defaultColor = Color.white;
    public Color occupiedColor = Color.gray;
    
    // イベント
    public System.Action<Worker> OnWorkerPlaced;
    public System.Action<Worker> OnWorkerRemoved;
    
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
            
        placedWorkers.Add(worker);
        worker.SetActionSpace(this);
        worker.transform.position = GetWorkerPosition(placedWorkers.Count - 1);
        
        // アクションを実行
        ExecuteAction(worker.owner);
        
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
    
    private void ExecuteAction(Player player)
    {
        switch (actionType)
        {
            case ActionType.GainResources:
                ExecuteResourceGain(player);
                break;
            case ActionType.AddField:
                ExecuteAddField(player);
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
    
    private void ExecuteResourceGain(Player player)
    {
        foreach (var resource in resourceGain)
        {
            player.AddResource(resource.Key, resource.Value);
        }
    }
    
    private void ExecuteAddField(Player player)
    {
        player.AddField();
        Debug.Log($"{player.playerName}が畑を追加しました");
    }
    
    private void ExecuteSowGrain(Player player)
    {
        // プレイヤーの穀物を使って種まき
        if (player.GetResource(ResourceType.Grain) > 0 && player.GetFields() > 0)
        {
            player.SowGrain(1);
            Debug.Log($"{player.playerName}が種まきをしました");
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
        // アクションスペースで指定された動物を獲得
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
        Debug.Log($"{player.playerName}が改良カードをプレイできます");
        // 実際の実装では、小さな進歩または大きな進歩カードを選択
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
        switch (resourceType)
        {
            case ResourceType.Wood: return "木材";
            case ResourceType.Clay: return "土";
            case ResourceType.Reed: return "葦";
            case ResourceType.Stone: return "石";
            case ResourceType.Grain: return "穀物";
            case ResourceType.Vegetable: return "野菜";
            case ResourceType.Sheep: return "羊";
            case ResourceType.Boar: return "猪";
            case ResourceType.Cattle: return "牛";
            case ResourceType.Food: return "食料";
            default: return resourceType.ToString();
        }
    }
    
    private void ExecuteSpecialAction(Player player)
    {
        // 特殊アクション
        // 継承先で実装するか、設定可能にする
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