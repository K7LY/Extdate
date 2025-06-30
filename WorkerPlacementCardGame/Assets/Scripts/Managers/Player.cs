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
    
    [Header("ワーカー")]
    public int totalWorkers = 4;
    [SerializeField] private int availableWorkers;
    [SerializeField] private List<Worker> placedWorkers = new List<Worker>();
    
    [Header("カード")]
    [SerializeField] private List<Card> hand = new List<Card>();
    [SerializeField] private List<Card> playedCards = new List<Card>();
    
    [Header("勝利点")]
    [SerializeField] private int victoryPoints = 0;
    
    // イベント
    public System.Action<ResourceType, int> OnResourceChanged;
    public System.Action<int> OnVictoryPointsChanged;
    public System.Action<Card> OnCardPlayed;
    
    void Start()
    {
        InitializeResources();
        availableWorkers = totalWorkers;
    }
    
    private void InitializeResources()
    {
        resources[ResourceType.Wood] = 2;
        resources[ResourceType.Stone] = 2;
        resources[ResourceType.Food] = 3;
        resources[ResourceType.Gold] = 1;
        resources[ResourceType.Workers] = totalWorkers;
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
        availableWorkers = totalWorkers;
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
    
    // ターン終了処理
    public void EndTurn()
    {
        // 必要に応じてターン終了時の処理を追加
    }
}