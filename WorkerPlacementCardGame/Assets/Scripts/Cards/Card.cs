using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum CardType
{
    Action,
    Building,
    Worker,
    Resource
}

[System.Serializable]
public enum ResourceType
{
    Wood,
    Stone,
    Food,
    Gold,
    Workers
}

[CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card")]
public class Card : ScriptableObject
{
    [Header("基本情報")]
    public string cardName;
    public string description;
    public Sprite cardArt;
    public CardType cardType;
    
    [Header("コスト")]
    public Dictionary<ResourceType, int> cost = new Dictionary<ResourceType, int>();
    public int workerCost = 0;
    
    [Header("効果")]
    public Dictionary<ResourceType, int> resourceGain = new Dictionary<ResourceType, int>();
    public int victoryPoints = 0;
    public bool isRepeatable = true;
    
    [Header("特殊効果")]
    public string specialEffect;
    
    // カードを使用した時の効果を実行
    public virtual void ExecuteCard(GameManager gameManager, Player player)
    {
        // リソース獲得
        foreach(var resource in resourceGain)
        {
            player.AddResource(resource.Key, resource.Value);
        }
        
        // 勝利点追加
        player.AddVictoryPoints(victoryPoints);
        
        // 特殊効果があれば実行
        if (!string.IsNullOrEmpty(specialEffect))
        {
            ExecuteSpecialEffect(gameManager, player);
        }
    }
    
    protected virtual void ExecuteSpecialEffect(GameManager gameManager, Player player)
    {
        // 継承先で特殊効果を実装
    }
    
    // カードがプレイ可能かチェック
    public virtual bool CanPlay(Player player)
    {
        // コストチェック
        foreach(var costItem in cost)
        {
            if (player.GetResource(costItem.Key) < costItem.Value)
                return false;
        }
        
        // ワーカーコストチェック
        if (player.GetAvailableWorkers() < workerCost)
            return false;
            
        return true;
    }
}