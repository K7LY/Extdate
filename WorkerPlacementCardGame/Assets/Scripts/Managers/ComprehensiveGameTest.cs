using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ComprehensiveGameTest : MonoBehaviour
{
    [Header("テスト設定")]
    public bool runFullGameTest = true;
    public bool testResourceConversion = true;
    public bool testFeedingSystem = true;
    public float testSpeed = 0.5f;
    
    private GameManager gameManager;
    private ResourceConverter resourceConverter;
    private ActionSpaceManager actionSpaceManager;
    
    void Start()
    {
        if (runFullGameTest)
        {
            StartCoroutine(RunComprehensiveTest());
        }
    }
    
    IEnumerator RunComprehensiveTest()
    {
        Debug.Log("🧪 === 包括的ゲームテスト開始 ===");
        
        yield return new WaitForSeconds(1f);
        
        // 初期化テスト
        yield return StartCoroutine(TestInitialization());
        
        // リソース変換システムテスト
        if (testResourceConversion)
        {
            yield return StartCoroutine(TestResourceConversionSystem());
        }
        
        // 餌やりシステムテスト
        if (testFeedingSystem)
        {
            yield return StartCoroutine(TestFeedingSystem());
        }
        
        // 完全ゲームフローテスト
        yield return StartCoroutine(TestCompleteGameFlow());
        
        Debug.Log("🎉 === 包括的ゲームテスト完了 ===");
    }
    
    IEnumerator TestInitialization()
    {
        Debug.Log("🔧 初期化テスト開始");
        
        gameManager = FindObjectOfType<GameManager>();
        resourceConverter = FindObjectOfType<ResourceConverter>();
        actionSpaceManager = FindObjectOfType<ActionSpaceManager>();
        
        // 必要なコンポーネントの存在確認
        Assert(gameManager != null, "GameManager が存在する");
        Assert(resourceConverter != null, "ResourceConverter が存在する");
        Assert(actionSpaceManager != null, "ActionSpaceManager が存在する");
        
        // プレイヤーの初期状態確認
        List<Player> players = FindObjectsOfType<Player>().ToList();
        Assert(players.Count >= 2, $"2人以上のプレイヤーが存在する (実際: {players.Count})");
        
        foreach (Player player in players)
        {
            Assert(player.GetFamilyMembers() == 2, $"{player.playerName}の初期家族数が2人");
            Assert(player.GetRooms() == 2, $"{player.playerName}の初期部屋数が2部屋");
            Assert(player.GetHouseType() == Player.HouseType.Wood, $"{player.playerName}の初期住居が木の家");
        }
        
        Debug.Log("✅ 初期化テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestResourceConversionSystem()
    {
        Debug.Log("🔄 リソース変換システムテスト開始");
        
        List<Player> players = FindObjectsOfType<Player>().ToList();
        Player testPlayer = players[0];
        
        // テスト用リソースを追加
        testPlayer.AddResource(ResourceType.Grain, 5);
        testPlayer.AddResource(ResourceType.Sheep, 3);
        testPlayer.AddResource(ResourceType.Boar, 2);
        testPlayer.AddResource(ResourceType.Cattle, 1);
        
        yield return new WaitForSeconds(testSpeed);
        
        // 穀物→食料変換テスト
        int initialFood = testPlayer.GetResource(ResourceType.Food);
        int convertedFood = resourceConverter.ConvertGrainToFood(testPlayer, 2);
        Assert(convertedFood == 4, "穀物2個→食料4個の変換");
        Assert(testPlayer.GetResource(ResourceType.Food) == initialFood + 4, "食料が正しく増加");
        Assert(testPlayer.GetResource(ResourceType.Grain) == 3, "穀物が正しく減少");
        
        yield return new WaitForSeconds(testSpeed);
        
        // 動物→食料変換テスト
        int currentFood = testPlayer.GetResource(ResourceType.Food);
        int sheepFood = resourceConverter.ConvertAnimalToFood(testPlayer, ResourceType.Sheep, 1);
        Assert(sheepFood == 2, "羊1匹→食料2個の変換");
        Assert(testPlayer.GetResource(ResourceType.Food) == currentFood + 2, "羊変換で食料増加");
        
        yield return new WaitForSeconds(testSpeed);
        
        // 最大食料計算テスト
        int maxPossibleFood = resourceConverter.CalculateMaxPossibleFood(testPlayer);
        Debug.Log($"最大取得可能食料: {maxPossibleFood}個");
        
        Debug.Log("✅ リソース変換システムテスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFeedingSystem()
    {
        Debug.Log("🍽️ 餌やりシステムテスト開始");
        
        List<Player> players = FindObjectsOfType<Player>().ToList();
        Player testPlayer = players[1];
        
        // 家族を3人に増やす（食料需要: 6個）
        testPlayer.GrowFamily();
        Assert(testPlayer.GetFamilyMembers() == 3, "家族が3人に増加");
        
        yield return new WaitForSeconds(testSpeed);
        
        // シナリオ1: 食料不足（自動変換なし）
        // 食料を全て削除
        int currentFood = testPlayer.GetResource(ResourceType.Food);
        if (currentFood > 0)
        {
            testPlayer.SpendResource(ResourceType.Food, currentFood);
        }
        
        int beggingCards1 = testPlayer.FeedFamily();
        Assert(beggingCards1 == 6, "食料0で6枚の乞食カード");
        
        yield return new WaitForSeconds(testSpeed);
        
        // シナリオ2: 部分的食料不足（自動変換あり）
        testPlayer.AddResource(ResourceType.Food, 2);      // 食料2個
        testPlayer.AddResource(ResourceType.Grain, 2);     // 穀物2個（→食料4個変換可能）
        
        int beggingCards2 = testPlayer.FeedFamily();
        Assert(beggingCards2 == 0, "食料2個+穀物変換で乞食カード0枚");
        
        yield return new WaitForSeconds(testSpeed);
        
        // シナリオ3: 十分な食料
        testPlayer.AddResource(ResourceType.Food, 10);
        int beggingCards3 = testPlayer.FeedFamily();
        Assert(beggingCards3 == 0, "十分な食料で乞食カード0枚");
        
        Debug.Log("✅ 餌やりシステムテスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestCompleteGameFlow()
    {
        Debug.Log("🎮 完全ゲームフローテスト開始");
        
        // ラウンド1-4をテスト（最初の収穫まで）
        for (int round = 1; round <= 4; round++)
        {
            Debug.Log($"--- ラウンド {round} テスト ---");
            
            yield return StartCoroutine(TestRound(round));
            
            // ラウンド4は収穫あり
            if (round == 4)
            {
                yield return StartCoroutine(TestHarvest(round));
            }
            
            yield return new WaitForSeconds(testSpeed);
        }
        
        // ゲーム状態の確認
        Assert(gameManager.currentRound == 5, "ラウンド5に進行");
        
        Debug.Log("✅ 完全ゲームフローテスト完了");
    }
    
    IEnumerator TestRound(int round)
    {
        // アクションスペースの解放確認
        if (actionSpaceManager != null)
        {
            actionSpaceManager.ActivateActionSpacesForRound(round);
            List<ActionSpace> activeSpaces = actionSpaceManager.GetActiveActionSpaces();
            Debug.Log($"ラウンド{round}: アクティブなアクションスペース {activeSpaces.Count}個");
        }
        
        yield return new WaitForSeconds(testSpeed / 2);
        
        // プレイヤーのターンをシミュレート
        List<Player> players = FindObjectsOfType<Player>().ToList();
        foreach (Player player in players)
        {
            yield return StartCoroutine(SimulatePlayerTurn(player, round));
        }
    }
    
    IEnumerator SimulatePlayerTurn(Player player, int round)
    {
        Debug.Log($"{player.playerName}のターン (ラウンド{round})");
        
        int availableWorkers = player.GetAvailableWorkers();
        List<ActionSpace> activeSpaces = actionSpaceManager?.GetActiveActionSpaces() ?? 
                                        FindObjectsOfType<ActionSpace>().ToList();
        
        for (int i = 0; i < availableWorkers && activeSpaces.Count > 0; i++)
        {
            // 利用可能なスペースから選択
            ActionSpace availableSpace = activeSpaces.FirstOrDefault(space => space.CanPlaceWorker());
            if (availableSpace != null)
            {
                bool placed = player.PlaceWorker(availableSpace);
                if (placed)
                {
                    Debug.Log($"  {availableSpace.actionName}にワーカー配置");
                    activeSpaces.Remove(availableSpace);
                }
            }
            
            yield return new WaitForSeconds(testSpeed / 4);
        }
        
        LogPlayerStatus(player);
    }
    
    IEnumerator TestHarvest(int round)
    {
        Debug.Log($"🌾 収穫フェーズテスト (ラウンド{round})");
        
        List<Player> players = FindObjectsOfType<Player>().ToList();
        
        foreach (Player player in players)
        {
            Debug.Log($"{player.playerName}の収穫:");
            
            // 収穫前の状態記録
            int grainBefore = player.GetResource(ResourceType.Grain);
            int foodBefore = player.GetResource(ResourceType.Food);
            
            // 収穫実行
            player.HarvestCrops();
            int beggingCards = player.FeedFamily();
            player.BreedAnimals();
            
            // 結果の確認
            int grainAfter = player.GetResource(ResourceType.Grain);
            int foodAfter = player.GetResource(ResourceType.Food);
            
            Debug.Log($"  穀物: {grainBefore} → {grainAfter}");
            Debug.Log($"  食料: {foodBefore} → {foodAfter}");
            Debug.Log($"  乞食カード: {beggingCards}枚");
            
            LogPlayerStatus(player);
            
            yield return new WaitForSeconds(testSpeed / 2);
        }
        
        // ワーカー回収
        foreach (Player player in players)
        {
            player.ReturnAllWorkers();
        }
    }
    
    private void LogPlayerStatus(Player player)
    {
        List<string> resources = new List<string>();
        
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = player.GetResource(resourceType);
            if (amount > 0)
            {
                resources.Add($"{resourceType}:{amount}");
            }
        }
        
        Debug.Log($"  {player.playerName}状態: " +
                 $"家族{player.GetFamilyMembers()}人, " +
                 $"部屋{player.GetRooms()}個, " +
                 $"畑{player.GetFields()}個, " +
                 $"リソース[{string.Join(", ", resources)}]");
    }
    
    private void Assert(bool condition, string message)
    {
        if (condition)
        {
            Debug.Log($"✅ {message}");
        }
        else
        {
            Debug.LogError($"❌ {message}");
        }
    }
    
    [ContextMenu("Run Quick Test")]
    public void RunQuickTest()
    {
        StartCoroutine(RunComprehensiveTest());
    }
}