using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardSystemTest : MonoBehaviour
{
    [Header("テスト設定")]
    public bool runOnStart = true;
    public float testSpeed = 0.5f;
    
    private CardLibrary cardLibrary;
    private List<Player> testPlayers = new List<Player>();
    
    void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunCardSystemTest());
        }
    }
    
    IEnumerator RunCardSystemTest()
    {
        Debug.Log("🃏 === 職業・進歩カードシステムテスト開始 ===");
        
        yield return new WaitForSeconds(1f);
        
        // カードライブラリの初期化
        yield return StartCoroutine(InitializeCardLibrary());
        
        // テストプレイヤーの作成
        yield return StartCoroutine(CreateTestPlayers());
        
        // カード配布テスト
        yield return StartCoroutine(TestCardDistribution());
        
        // 職業カード効果テスト
        yield return StartCoroutine(TestOccupationCards());
        
        // 進歩カード効果テスト
        yield return StartCoroutine(TestImprovementCards());
        
        // カード組み合わせテスト
        yield return StartCoroutine(TestCardCombinations());
        
        // 実戦シミュレーション
        yield return StartCoroutine(SimulateCardBasedGame());
        
        Debug.Log("🎉 === 職業・進歩カードシステムテスト完了 ===");
    }
    
    IEnumerator InitializeCardLibrary()
    {
        Debug.Log("📚 カードライブラリ初期化中...");
        
        cardLibrary = FindObjectOfType<CardLibrary>();
        if (cardLibrary == null)
        {
            GameObject libraryObj = new GameObject("CardLibrary");
            cardLibrary = libraryObj.AddComponent<CardLibrary>();
        }
        
        cardLibrary.CreateDefaultCards();
        
        Debug.Log($"✅ カードライブラリ初期化完了: " +
                 $"職業{cardLibrary.allOccupations.Count}枚, " +
                 $"小進歩{cardLibrary.allMinorImprovements.Count}枚, " +
                 $"大進歩{cardLibrary.allMajorImprovements.Count}枚");
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator CreateTestPlayers()
    {
        Debug.Log("👥 テストプレイヤー作成中...");
        
        string[] playerNames = { "アリス", "ボブ", "チャーリー" };
        
        for (int i = 0; i < 3; i++)
        {
            GameObject playerObj = new GameObject($"TestPlayer_{i + 1}");
            Player player = playerObj.AddComponent<Player>();
            player.playerName = playerNames[i];
            player.playerColor = new Color(Random.value, Random.value, Random.value);
            
            testPlayers.Add(player);
            
            Debug.Log($"  プレイヤー作成: {player.playerName}");
            yield return new WaitForSeconds(testSpeed / 3);
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestCardDistribution()
    {
        Debug.Log("🎴 カード配布テスト開始");
        
        cardLibrary.DistributeCardsToPlayers(testPlayers);
        
        foreach (var player in testPlayers)
        {
            var hand = player.GetHand();
            Debug.Log($"  {player.playerName}: 手札{hand.Count}枚");
            
            int occupationCount = hand.Count(c => c is OccupationCard);
            int improvementCount = hand.Count(c => c is ImprovementCard);
            
            Debug.Log($"    職業カード: {occupationCount}枚");
            Debug.Log($"    進歩カード: {improvementCount}枚");
            
            yield return new WaitForSeconds(testSpeed / 2);
        }
        
        Debug.Log("✅ カード配布テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestOccupationCards()
    {
        Debug.Log("💼 職業カード効果テスト開始");
        
        Player testPlayer = testPlayers[0];
        
        // 農夫カードテスト
        yield return StartCoroutine(TestFarmerCard(testPlayer));
        
        // 漁師カードテスト
        yield return StartCoroutine(TestFishermanCard(testPlayer));
        
        // 大工カードテスト
        yield return StartCoroutine(TestCarpenterCard(testPlayer));
        
        // 羊飼いカードテスト
        yield return StartCoroutine(TestShepherdCard(testPlayer));
        
        Debug.Log("✅ 職業カード効果テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFarmerCard(Player player)
    {
        Debug.Log("🌾 農夫カードテスト");
        
        // 農夫カードをプレイ
        var farmerCard = cardLibrary.allOccupations.FirstOrDefault(o => o.occupationType == OccupationType.Farmer);
        if (farmerCard != null)
        {
            Debug.Log($"  {player.playerName}が農夫カードをプレイ");
            farmerCard.PlayCard(player);
            
            // 収穫前の穀物量を記録
            int grainBefore = player.GetResource(ResourceType.Grain);
            Debug.Log($"  収穫前の穀物: {grainBefore}個");
            
            // 畑を追加
            player.AddField();
            
            // 収穫を実行
            player.HarvestCrops();
            
            int grainAfter = player.GetResource(ResourceType.Grain);
            Debug.Log($"  収穫後の穀物: {grainAfter}個");
            Debug.Log($"  農夫効果による追加穀物: {grainAfter - grainBefore - player.GetFields()}個");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFishermanCard(Player player)
    {
        Debug.Log("🎣 漁師カードテスト");
        
        var fishermanCard = cardLibrary.allOccupations.FirstOrDefault(o => o.occupationType == OccupationType.Fisherman);
        if (fishermanCard != null)
        {
            Debug.Log($"  {player.playerName}が漁師カードをプレイ");
            fishermanCard.PlayCard(player);
            
            // 食料獲得アクションをシミュレート
            var fishingAction = CreateTestActionSpace("漁場", ActionType.GainResources);
            fishingAction.resourceGain[ResourceType.Food] = 1;
            
            int foodBefore = player.GetResource(ResourceType.Food);
            Debug.Log($"  アクション前の食料: {foodBefore}個");
            
            // アクションを実行
            player.OnActionExecuted(fishingAction);
            player.AddResource(ResourceType.Food, 1); // 基本効果
            
            int foodAfter = player.GetResource(ResourceType.Food);
            Debug.Log($"  アクション後の食料: {foodAfter}個");
            Debug.Log($"  漁師効果による追加食料: {foodAfter - foodBefore - 1}個");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestCarpenterCard(Player player)
    {
        Debug.Log("🔨 大工カードテスト");
        
        var carpenterCard = cardLibrary.allOccupations.FirstOrDefault(o => o.occupationType == OccupationType.Carpenter);
        if (carpenterCard != null)
        {
            Debug.Log($"  {player.playerName}が大工カードをプレイ");
            carpenterCard.PlayCard(player);
            
            // 住居拡張アクションをシミュレート
            var expansionAction = CreateTestActionSpace("住居の拡張", ActionType.HouseExpansion);
            
            int woodBefore = player.GetResource(ResourceType.Wood);
            player.AddResource(ResourceType.Wood, 5); // テスト用リソース
            Debug.Log($"  拡張前の木材: {player.GetResource(ResourceType.Wood)}個");
            
            // アクションを実行
            player.OnActionExecuted(expansionAction);
            
            int woodAfter = player.GetResource(ResourceType.Wood);
            Debug.Log($"  拡張後の木材: {woodAfter}個");
            Debug.Log($"  大工効果による木材コスト削減確認");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestShepherdCard(Player player)
    {
        Debug.Log("🐑 羊飼いカードテスト");
        
        var shepherdCard = cardLibrary.allOccupations.FirstOrDefault(o => o.occupationType == OccupationType.Shepherd);
        if (shepherdCard != null && player.GetFamilyMembers() >= 1)
        {
            Debug.Log($"  {player.playerName}が羊飼いカードをプレイ");
            shepherdCard.PlayCard(player);
            
            // 羊を追加して繁殖条件を満たす
            player.AddResource(ResourceType.Sheep, 3);
            player.AddFences(6); // 牧場を作る
            
            int sheepBefore = player.GetResource(ResourceType.Sheep);
            Debug.Log($"  繁殖前の羊: {sheepBefore}匹");
            
            // 動物繁殖を実行
            player.BreedAnimals();
            
            int sheepAfter = player.GetResource(ResourceType.Sheep);
            Debug.Log($"  繁殖後の羊: {sheepAfter}匹");
            Debug.Log($"  羊飼い効果による追加羊: {sheepAfter - sheepBefore - 1}匹");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestImprovementCards()
    {
        Debug.Log("🏠 進歩カード効果テスト開始");
        
        Player testPlayer = testPlayers[1];
        
        // かまどカードテスト
        yield return StartCoroutine(TestFireplaceCard(testPlayer));
        
        // 料理場カードテスト
        yield return StartCoroutine(TestCookingAreaCard(testPlayer));
        
        // バスケットカードテスト
        yield return StartCoroutine(TestBasketCard(testPlayer));
        
        Debug.Log("✅ 進歩カード効果テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFireplaceCard(Player player)
    {
        Debug.Log("🔥 かまどカードテスト");
        
        var fireplaceCard = cardLibrary.allMinorImprovements.FirstOrDefault(i => i.cardName == "かまど");
        if (fireplaceCard != null)
        {
            // 必要リソースを与える
            player.AddResource(ResourceType.Clay, 2);
            
            Debug.Log($"  {player.playerName}がかまどカードをプレイ");
            fireplaceCard.PlayCard(player);
            
            // 料理テスト
            player.AddResource(ResourceType.Grain, 3);
            
            int grainBefore = player.GetResource(ResourceType.Grain);
            int foodBefore = player.GetResource(ResourceType.Food);
            
            Debug.Log($"  料理前: 穀物{grainBefore}個, 食料{foodBefore}個");
            
            int cookedFood = player.CookResource(ResourceType.Grain, 2);
            
            int grainAfter = player.GetResource(ResourceType.Grain);
            int foodAfter = player.GetResource(ResourceType.Food);
            
            Debug.Log($"  料理後: 穀物{grainAfter}個, 食料{foodAfter}個");
            Debug.Log($"  変換効率: 穀物2個 → 食料{cookedFood}個");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestCookingAreaCard(Player player)
    {
        Debug.Log("🍳 料理場カードテスト");
        
        var cookingAreaCard = cardLibrary.allMajorImprovements.FirstOrDefault(i => i.cardName == "料理場");
        if (cookingAreaCard != null)
        {
            // 必要リソースを与える
            player.AddResource(ResourceType.Clay, 4);
            
            Debug.Log($"  {player.playerName}が料理場カードをプレイ");
            cookingAreaCard.PlayCard(player);
            
            // 穀物と野菜の料理テスト
            player.AddResource(ResourceType.Grain, 2);
            player.AddResource(ResourceType.Vegetable, 2);
            
            Debug.Log($"  料理前: 穀物{player.GetResource(ResourceType.Grain)}個, " +
                     $"野菜{player.GetResource(ResourceType.Vegetable)}個, " +
                     $"食料{player.GetResource(ResourceType.Food)}個");
            
            int grainFood = player.CookResource(ResourceType.Grain, 1);
            int vegetableFood = player.CookResource(ResourceType.Vegetable, 1);
            
            Debug.Log($"  料理結果: 穀物1個→食料{grainFood}個, 野菜1個→食料{vegetableFood}個");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestBasketCard(Player player)
    {
        Debug.Log("🧺 バスケットカードテスト");
        
        var basketCard = cardLibrary.allMinorImprovements.FirstOrDefault(i => i.cardName == "バスケット");
        if (basketCard != null)
        {
            // 必要リソースを与える
            player.AddResource(ResourceType.Reed, 2);
            
            Debug.Log($"  {player.playerName}がバスケットカードをプレイ");
            basketCard.PlayCard(player);
            
            // 貯蔵能力テスト
            int grainCapacity = player.GetStorageCapacity(ResourceType.Grain);
            int vegetableCapacity = player.GetStorageCapacity(ResourceType.Vegetable);
            
            Debug.Log($"  貯蔵能力: 穀物+{grainCapacity}, 野菜+{vegetableCapacity}");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestCardCombinations()
    {
        Debug.Log("🔄 カード組み合わせテスト開始");
        
        Player testPlayer = testPlayers[2];
        
        // 農夫 + かまど の組み合わせテスト
        yield return StartCoroutine(TestFarmerFireplaceCombination(testPlayer));
        
        Debug.Log("✅ カード組み合わせテスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFarmerFireplaceCombination(Player player)
    {
        Debug.Log("🌾🔥 農夫+かまど組み合わせテスト");
        
        // 農夫カードをプレイ
        var farmerCard = cardLibrary.allOccupations.FirstOrDefault(o => o.occupationType == OccupationType.Farmer);
        if (farmerCard != null)
        {
            farmerCard.PlayCard(player);
        }
        
        // かまどカードをプレイ
        var fireplaceCard = cardLibrary.allMinorImprovements.FirstOrDefault(i => i.cardName == "かまど");
        if (fireplaceCard != null)
        {
            player.AddResource(ResourceType.Clay, 2);
            fireplaceCard.PlayCard(player);
        }
        
        // 畑を追加
        player.AddField();
        
        // 組み合わせ効果のテスト
        Debug.Log($"  収穫前: 穀物{player.GetResource(ResourceType.Grain)}個, 食料{player.GetResource(ResourceType.Food)}個");
        
        // 収穫実行（農夫効果発動）
        player.HarvestCrops();
        
        Debug.Log($"  収穫後: 穀物{player.GetResource(ResourceType.Grain)}個, 食料{player.GetResource(ResourceType.Food)}個");
        
        // 収穫した穀物を料理（かまど効果利用）
        int grainAmount = player.GetResource(ResourceType.Grain);
        if (grainAmount > 0)
        {
            int cookAmount = Mathf.Min(grainAmount, 2);
            int cookedFood = player.CookResource(ResourceType.Grain, cookAmount);
            Debug.Log($"  料理: 穀物{cookAmount}個 → 食料{cookedFood}個");
        }
        
        Debug.Log($"  最終状態: 穀物{player.GetResource(ResourceType.Grain)}個, 食料{player.GetResource(ResourceType.Food)}個");
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator SimulateCardBasedGame()
    {
        Debug.Log("🎲 カードベースゲームシミュレーション開始");
        
        // 3ラウンドの簡易ゲームをシミュレート
        for (int round = 1; round <= 3; round++)
        {
            Debug.Log($"\n--- ラウンド {round} ---");
            
            foreach (var player in testPlayers)
            {
                yield return StartCoroutine(SimulatePlayerTurn(player, round));
            }
            
            // ラウンド終了処理
            foreach (var player in testPlayers)
            {
                player.EndTurn();
            }
            
            yield return new WaitForSeconds(testSpeed);
        }
        
        // 最終スコア計算
        yield return StartCoroutine(CalculateFinalScores());
        
        Debug.Log("✅ カードベースゲームシミュレーション完了");
    }
    
    IEnumerator SimulatePlayerTurn(Player player, int round)
    {
        Debug.Log($"  {player.playerName}のターン");
        
        // 手札からランダムにカードをプレイ
        var hand = player.GetHand();
        var playableCards = hand.Where(c => c.CanPlay(player)).ToList();
        
        if (playableCards.Count > 0 && Random.value < 0.3f) // 30%の確率でカードプレイ
        {
            var selectedCard = playableCards[Random.Range(0, playableCards.Count)];
            
            Debug.Log($"    {selectedCard.cardName}をプレイ");
            selectedCard.PlayCard(player);
        }
        
        // リソース状況を表示
        var resources = new List<string>();
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = player.GetResource(resourceType);
            if (amount > 0)
            {
                resources.Add($"{resourceType}:{amount}");
            }
        }
        
        Debug.Log($"    リソース: [{string.Join(", ", resources)}]");
        Debug.Log($"    職業: {player.GetOccupations().Count}個, 進歩: {player.GetImprovements().Count}個");
        
        yield return new WaitForSeconds(testSpeed / 2);
    }
    
    IEnumerator CalculateFinalScores()
    {
        Debug.Log("\n🏆 最終スコア計算");
        
        foreach (var player in testPlayers)
        {
            int score = 0;
            
            // 家族点
            score += player.GetFamilyMembers() * 3;
            
            // リソース点
            score += player.GetResource(ResourceType.Grain);
            score += player.GetResource(ResourceType.Sheep);
            score += player.GetResource(ResourceType.Boar) * 2;
            score += player.GetResource(ResourceType.Cattle) * 3;
            
            // カード点
            score += player.GetOccupations().Count * 2;
            score += player.GetImprovements().Count * 3;
            
            Debug.Log($"  {player.playerName}: {score}点");
            Debug.Log($"    家族{player.GetFamilyMembers()}人, 職業{player.GetOccupations().Count}個, 進歩{player.GetImprovements().Count}個");
            
            yield return new WaitForSeconds(testSpeed / 3);
        }
    }
    
    private ActionSpace CreateTestActionSpace(string name, ActionType type)
    {
        GameObject obj = new GameObject($"TestAction_{name}");
        ActionSpace actionSpace = obj.AddComponent<ActionSpace>();
        actionSpace.actionName = name;
        actionSpace.actionType = type;
        return actionSpace;
    }
    
    [ContextMenu("Run Card System Test")]
    public void RunCardSystemTestManual()
    {
        StartCoroutine(RunCardSystemTest());
    }
}