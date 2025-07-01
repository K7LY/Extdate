using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardLibrary : MonoBehaviour
{
    [Header("カードライブラリ")]
    public List<OccupationCard> allOccupations = new List<OccupationCard>();
    public List<ImprovementCard> allMinorImprovements = new List<ImprovementCard>();
    public List<ImprovementCard> allMajorImprovements = new List<ImprovementCard>();
    
    [Header("デッキ設定")]
    public int occupationsPerPlayer = 7;
    public int minorImprovementsPerPlayer = 7;
    
    void Start()
    {
        if (allOccupations.Count == 0 || allMinorImprovements.Count == 0 || allMajorImprovements.Count == 0)
        {
            CreateDefaultCards();
        }
    }
    
    public void CreateDefaultCards()
    {
        Debug.Log("🃏 デフォルトカードライブラリを作成中...");
        
        CreateOccupationCards();
        CreateMinorImprovementCards();
        CreateMajorImprovementCards();
        
        Debug.Log($"✅ カードライブラリ作成完了: 職業{allOccupations.Count}枚, 小進歩{allMinorImprovements.Count}枚, 大進歩{allMajorImprovements.Count}枚");
    }
    
    private void CreateOccupationCards()
    {
        allOccupations.Clear();
        
        // 基本職業カードの作成
        allOccupations.Add(CreateOccupationCard(
            "農夫", OccupationType.Farmer, OccupationTrigger.OnHarvest,
            "収穫時に追加穀物1個を獲得", 0, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "大工", OccupationType.Carpenter, OccupationTrigger.OnAction,
            "住居拡張時に木材コスト-1", 0, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "パン職人", OccupationType.Baker, OccupationTrigger.Passive,
            "穀物→食料変換効率+1", 0, 0,
            new Dictionary<ResourceType, int> { { ResourceType.Grain, 1 } }
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "漁師", OccupationType.Fisherman, OccupationTrigger.OnAction,
            "食料獲得アクション時に追加食料1個", 0, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "羊飼い", OccupationType.Shepherd, OccupationTrigger.OnBreeding,
            "羊の繁殖時に追加羊1匹", 1, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "石工", OccupationType.Stonecutter, OccupationTrigger.Immediate,
            "即座に石材2個獲得", 0, 0,
            new Dictionary<ResourceType, int> { { ResourceType.Stone, 2 } }
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "織工", OccupationType.Weaver, OccupationTrigger.Passive,
            "羊1匹→食料3個に変換可能", 0, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "商人", OccupationType.Trader, OccupationTrigger.Passive,
            "リソース交換効率+50%", 0, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "森林管理人", OccupationType.Forester, OccupationTrigger.OnAction,
            "木材獲得アクション時に追加木材1個", 0, 0,
            new Dictionary<ResourceType, int>()
        ));
        
        allOccupations.Add(CreateOccupationCard(
            "動物育種家", OccupationType.Breeder, OccupationTrigger.OnBreeding,
            "すべての動物繁殖効率+1", 1, 0,
            new Dictionary<ResourceType, int>()
        ));
    }
    
    private void CreateMinorImprovementCards()
    {
        allMinorImprovements.Clear();
        
        // 小さな進歩カードの作成
        allMinorImprovements.Add(CreateImprovementCard(
            "かまど", ImprovementType.Minor, ImprovementCategory.Cooking,
            "穀物1個→食料2個に変換", false,
            new Dictionary<ResourceType, int> { { ResourceType.Clay, 2 } },
            true, false, 2, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "バスケット", ImprovementType.Minor, ImprovementCategory.Storage,
            "穀物・野菜の貯蔵+3", false,
            new Dictionary<ResourceType, int> { { ResourceType.Reed, 2 } },
            false, false, 0, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "陶器", ImprovementType.Minor, ImprovementCategory.Crafting,
            "毎ラウンド食料1個", false,
            new Dictionary<ResourceType, int> { { ResourceType.Clay, 2 } },
            false, false, 0, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "農具", ImprovementType.Minor, ImprovementCategory.Farming,
            "種まき効率+1", false,
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } },
            false, false, 0, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "柵材料", ImprovementType.Minor, ImprovementCategory.AnimalHusbandry,
            "即座に柵4本", false,
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } },
            false, false, 0, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "小さな舟", ImprovementType.Minor, ImprovementCategory.Trading,
            "毎ラウンド食料1個", false,
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } },
            false, false, 0, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "鋤", ImprovementType.Minor, ImprovementCategory.Farming,
            "畑追加時にボーナス穀物1個", false,
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } },
            false, false, 0, 0
        ));
        
        allMinorImprovements.Add(CreateImprovementCard(
            "手車", ImprovementType.Minor, ImprovementCategory.Infrastructure,
            "リソース運搬能力+2", false,
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } },
            false, false, 0, 0
        ));
    }
    
    private void CreateMajorImprovementCards()
    {
        allMajorImprovements.Clear();
        
        // 大きな進歩カードの作成
        allMajorImprovements.Add(CreateImprovementCard(
            "料理場", ImprovementType.Major, ImprovementCategory.Cooking,
            "穀物1個→食料2個、野菜1個→食料3個", false,
            new Dictionary<ResourceType, int> { { ResourceType.Clay, 4 } },
            true, true, 2, 3
        ));
        
        allMajorImprovements.Add(CreateImprovementCard(
            "石のかまど", ImprovementType.Major, ImprovementCategory.Cooking,
            "穀物1個→食料3個、野菜1個→食料4個、毎ラウンド食料1個", true,
            new Dictionary<ResourceType, int> { { ResourceType.Clay, 3 }, { ResourceType.Stone, 1 } },
            true, true, 3, 4
        ));
        
        allMajorImprovements.Add(CreateImprovementCard(
            "井戸", ImprovementType.Major, ImprovementCategory.Infrastructure,
            "家族の成長が容易", false,
            new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 }, { ResourceType.Stone, 3 } },
            false, false, 0, 0
        ));
        
        allMajorImprovements.Add(CreateImprovementCard(
            "粘土炉", ImprovementType.Major, ImprovementCategory.Cooking,
            "穀物1個→食料5個", true,
            new Dictionary<ResourceType, int> { { ResourceType.Clay, 2 }, { ResourceType.Stone, 1 } },
            true, false, 5, 0
        ));
        
        allMajorImprovements.Add(CreateImprovementCard(
            "ジョッキ", ImprovementType.Major, ImprovementCategory.Luxury,
            "勝利点+4", false,
            new Dictionary<ResourceType, int> { { ResourceType.Clay, 2 } },
            false, false, 0, 0
        ));
    }
    
    private OccupationCard CreateOccupationCard(string name, OccupationType type, OccupationTrigger trigger,
        string description, int minFamily, int minRooms, Dictionary<ResourceType, int> costs)
    {
        var card = ScriptableObject.CreateInstance<OccupationCard>();
        card.cardName = name;
        card.occupationType = type;
        card.trigger = trigger;
        card.effectDescription = description;
        card.minimumFamilyMembers = minFamily;
        card.minimumRooms = minRooms;
        card.cost = costs;
        
        return card;
    }
    
    private ImprovementCard CreateImprovementCard(string name, ImprovementType type, ImprovementCategory category,
        string description, bool requiresCooking, Dictionary<ResourceType, int> costs,
        bool canCookGrain, bool canCookVegetable, int grainRate, int vegetableRate)
    {
        var card = ScriptableObject.CreateInstance<ImprovementCard>();
        card.cardName = name;
        card.improvementType = type;
        card.category = category;
        card.effectDescription = description;
        card.requiresCooking = requiresCooking;
        card.cost = costs;
        card.canCookGrain = canCookGrain;
        card.canCookVegetable = canCookVegetable;
        card.grainToFoodRatio = grainRate;
        card.vegetableToFoodRatio = vegetableRate;
        
        return card;
    }
    
    // プレイヤーへのカード配布
    public void DistributeCardsToPlayers(List<Player> players)
    {
        Debug.Log("🎴 プレイヤーにカードを配布中...");
        
        foreach (var player in players)
        {
            // 職業カードの配布
            var occupationDeck = new List<OccupationCard>(allOccupations);
            ShuffleList(occupationDeck);
            
            for (int i = 0; i < occupationsPerPlayer && i < occupationDeck.Count; i++)
            {
                player.AddCardToHand(occupationDeck[i]);
            }
            
            // 小さな進歩カードの配布
            var minorDeck = new List<ImprovementCard>(allMinorImprovements);
            ShuffleList(minorDeck);
            
            for (int i = 0; i < minorImprovementsPerPlayer && i < minorDeck.Count; i++)
            {
                player.AddCardToHand(minorDeck[i]);
            }
            
            Debug.Log($"  {player.playerName}: 職業{occupationsPerPlayer}枚, 小進歩{minorImprovementsPerPlayer}枚");
        }
        
        Debug.Log("✅ カード配布完了");
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    [ContextMenu("Create Default Cards")]
    public void CreateDefaultCardsManual()
    {
        CreateDefaultCards();
    }
    
    // デバッグ用：カード効果テスト
    [ContextMenu("Test Card Effects")]
    public void TestCardEffects()
    {
        Debug.Log("🧪 カード効果テスト開始");
        
        Player testPlayer = FindObjectOfType<Player>();
        if (testPlayer == null)
        {
            Debug.LogError("テスト用プレイヤーが見つかりません");
            return;
        }
        
        // 農夫カードのテスト
        var farmerCard = allOccupations.FirstOrDefault(o => o.occupationType == OccupationType.Farmer);
        if (farmerCard != null && farmerCard.CanPlay(testPlayer))
        {
            Debug.Log("農夫カードをテスト中...");
            farmerCard.PlayCard(testPlayer);
            
            // 収穫をトリガーして効果をテスト
            testPlayer.HarvestCrops();
        }
        
        // かまどカードのテスト
        var fireplaceCard = allMinorImprovements.FirstOrDefault(i => i.cardName == "かまど");
        if (fireplaceCard != null)
        {
            Debug.Log("かまどカードをテスト中...");
            
            // 必要リソースを与える
            testPlayer.AddResource(ResourceType.Clay, 2);
            
            if (fireplaceCard.CanPlay(testPlayer))
            {
                fireplaceCard.PlayCard(testPlayer);
                
                // 料理テスト
                testPlayer.AddResource(ResourceType.Grain, 3);
                testPlayer.CookResource(ResourceType.Grain, 2);
            }
        }
        
        Debug.Log("✅ カード効果テスト完了");
    }
}