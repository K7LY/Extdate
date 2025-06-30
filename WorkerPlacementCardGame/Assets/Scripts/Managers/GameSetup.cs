using UnityEngine;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour
{
    [Header("セットアップ設定")]
    public bool createSampleCards = true;
    public bool createSampleActionSpaces = true;
    public bool createSampleDeck = true;
    
    [Header("プレハブ")]
    public GameObject actionSpacePrefab;
    public Transform gameBoard;
    
    void Start()
    {
        SetupGame();
    }
    
    public void SetupGame()
    {
        if (createSampleCards)
        {
            CreateSampleCards();
        }
        
        if (createSampleActionSpaces)
        {
            CreateSampleActionSpaces();
        }
        
        if (createSampleDeck)
        {
            CreateSampleDeck();
        }
        
        Debug.Log("ゲームセットアップが完了しました");
    }
    
    private void CreateSampleCards()
    {
        // 木材採取カード
        Card woodCard = ScriptableObject.CreateInstance<Card>();
        woodCard.cardName = "木材採取";
        woodCard.description = "木材を2個獲得する";
        woodCard.cardType = CardType.Action;
        woodCard.resourceGain[ResourceType.Wood] = 2;
        
        // 石材採取カード
        Card stoneCard = ScriptableObject.CreateInstance<Card>();
        stoneCard.cardName = "石材採取";
        stoneCard.description = "石材を2個獲得する";
        stoneCard.cardType = CardType.Action;
        stoneCard.resourceGain[ResourceType.Stone] = 2;
        
        // 食料生産カード
        Card foodCard = ScriptableObject.CreateInstance<Card>();
        foodCard.cardName = "食料生産";
        foodCard.description = "食料を3個獲得する";
        foodCard.cardType = CardType.Action;
        foodCard.resourceGain[ResourceType.Food] = 3;
        
        // 金貨獲得カード
        Card goldCard = ScriptableObject.CreateInstance<Card>();
        goldCard.cardName = "商取引";
        goldCard.description = "金貨を1個獲得する";
        goldCard.cardType = CardType.Action;
        goldCard.resourceGain[ResourceType.Gold] = 1;
        
        // 建物カード
        Card buildingCard = ScriptableObject.CreateInstance<Card>();
        buildingCard.cardName = "家の建設";
        buildingCard.description = "勝利点2を獲得。木材1個が必要";
        buildingCard.cardType = CardType.Building;
        buildingCard.cost[ResourceType.Wood] = 1;
        buildingCard.victoryPoints = 2;
        
        Debug.Log("サンプルカードを作成しました");
    }
    
    private void CreateSampleActionSpaces()
    {
        if (gameBoard == null)
        {
            GameObject boardObj = new GameObject("GameBoard");
            gameBoard = boardObj.transform;
        }
        
        // 木材採取場所
        CreateActionSpace("森", ActionType.GainResources, new Vector3(-3, 1, 0), 
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } });
        
        // 採石場
        CreateActionSpace("採石場", ActionType.GainResources, new Vector3(-1, 1, 0), 
            new Dictionary<ResourceType, int> { { ResourceType.Stone, 1 } });
        
        // 農場
        CreateActionSpace("農場", ActionType.GainResources, new Vector3(1, 1, 0), 
            new Dictionary<ResourceType, int> { { ResourceType.Food, 2 } });
        
        // 市場
        CreateActionSpace("市場", ActionType.Trade, new Vector3(3, 1, 0), 
            new Dictionary<ResourceType, int> { { ResourceType.Gold, 1 } });
        
        // カード獲得場所
        CreateActionSpace("図書館", ActionType.DrawCards, new Vector3(0, -1, 0), 
            new Dictionary<ResourceType, int>(), 2);
        
        Debug.Log("サンプルアクションスペースを作成しました");
    }
    
    private void CreateActionSpace(string name, ActionType actionType, Vector3 position, 
        Dictionary<ResourceType, int> resourceGain, int cardsToDraw = 0)
    {
        GameObject actionSpaceObj;
        
        if (actionSpacePrefab != null)
        {
            actionSpaceObj = Instantiate(actionSpacePrefab, position, Quaternion.identity, gameBoard);
        }
        else
        {
            actionSpaceObj = CreateDefaultActionSpaceObject(position);
        }
        
        actionSpaceObj.name = name;
        
        ActionSpace actionSpace = actionSpaceObj.GetComponent<ActionSpace>();
        if (actionSpace == null)
        {
            actionSpace = actionSpaceObj.AddComponent<ActionSpace>();
        }
        
        actionSpace.actionName = name;
        actionSpace.description = GetActionDescription(actionType, resourceGain, cardsToDraw);
        actionSpace.actionType = actionType;
        actionSpace.resourceGain = resourceGain;
        actionSpace.cardsToDraw = cardsToDraw;
        
        // ビジュアル設定
        SpriteRenderer renderer = actionSpaceObj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = actionSpaceObj.AddComponent<SpriteRenderer>();
        }
        
        // 簡単な色分け
        Color spaceColor = GetActionSpaceColor(actionType);
        renderer.color = spaceColor;
        
        // 基本的な四角形スプライトを作成
        renderer.sprite = CreateSquareSprite();
        
        // テキスト表示
        CreateActionSpaceText(actionSpaceObj, name);
    }
    
    private GameObject CreateDefaultActionSpaceObject(Vector3 position)
    {
        GameObject obj = new GameObject("ActionSpace");
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one;
        
        return obj;
    }
    
    private string GetActionDescription(ActionType actionType, Dictionary<ResourceType, int> resourceGain, int cardsToDraw)
    {
        string description = "";
        
        switch (actionType)
        {
            case ActionType.GainResources:
                foreach (var resource in resourceGain)
                {
                    description += $"{GetResourceJapaneseName(resource.Key)} +{resource.Value} ";
                }
                break;
            case ActionType.DrawCards:
                description = $"カード {cardsToDraw}枚ドロー";
                break;
            case ActionType.Trade:
                description = "トレード";
                break;
            default:
                description = actionType.ToString();
                break;
        }
        
        return description.Trim();
    }
    
    private Color GetActionSpaceColor(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.GainResources: return new Color(0.8f, 0.9f, 0.8f); // 薄い緑
            case ActionType.DrawCards: return new Color(0.8f, 0.8f, 0.9f); // 薄い青
            case ActionType.Trade: return new Color(0.9f, 0.9f, 0.8f); // 薄い黄色
            case ActionType.PlaceBuilding: return new Color(0.9f, 0.8f, 0.8f); // 薄い赤
            default: return Color.white;
        }
    }
    
    private Sprite CreateSquareSprite()
    {
        // 1x1ピクセルの白いテクスチャを作成
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        // スプライトを作成
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
    }
    
    private void CreateActionSpaceText(GameObject parent, string text)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform);
        textObj.transform.localPosition = Vector3.zero;
        
        // TextMeshがない場合の簡単なテキスト表示
        // 実際のプロジェクトではTextMeshProを使用することを推奨
    }
    
    private void CreateSampleDeck()
    {
        CardDeck deck = ScriptableObject.CreateInstance<CardDeck>();
        
        // サンプルカードをデッキに追加
        // 実際のプロジェクトでは、作成したカードをここで追加する
        
        // GameManagerにデッキを設定
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.mainDeck = deck;
        }
        
        Debug.Log("サンプルデッキを作成しました");
    }
    
    private string GetResourceJapaneseName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood: return "木材";
            case ResourceType.Stone: return "石材";
            case ResourceType.Food: return "食料";
            case ResourceType.Gold: return "金貨";
            case ResourceType.Workers: return "ワーカー";
            default: return resourceType.ToString();
        }
    }
}