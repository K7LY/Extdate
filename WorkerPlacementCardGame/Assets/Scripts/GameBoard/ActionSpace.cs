using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum ActionType
{
    GainResources,
    DrawCards,
    PlaceBuilding,
    Trade,
    Special
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
            case ActionType.DrawCards:
                ExecuteDrawCards(player);
                break;
            case ActionType.PlaceBuilding:
                ExecutePlaceBuilding(player);
                break;
            case ActionType.Trade:
                ExecuteTrade(player);
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
    
    private void ExecuteDrawCards(Player player)
    {
        if (cardsToDraw > 0)
        {
            player.DrawCards(cardsToDraw);
        }
    }
    
    private void ExecutePlaceBuilding(Player player)
    {
        // 建物配置のロジック
        // ここでは基本的なリソース獲得のみ実装
        ExecuteResourceGain(player);
    }
    
    private void ExecuteTrade(Player player)
    {
        // トレードのロジック
        // プレイヤーが選択できるトレードオプションを表示
        ExecuteResourceGain(player);
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