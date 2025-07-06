using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 細かな収穫フェーズトリガーシステムのテストクラス
/// </summary>
public class DetailedHarvestTriggerTest : MonoBehaviour
{
    [Header("テスト設定")]
    public bool runTestOnStart = true;
    public bool enableDetailedLogs = true;
    
    [Header("テスト用プレイヤー")]
    public Player testPlayer;
    
    [Header("テスト用カード")]
    public List<OccupationCard> testOccupationCards = new List<OccupationCard>();
    
    private GameManager gameManager;
    
    void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunDetailedHarvestTriggerTests());
        }
    }
    
    System.Collections.IEnumerator RunDetailedHarvestTriggerTests()
    {
        yield return new WaitForSeconds(1f);
        
        Debug.Log("=== 細かな収穫フェーズトリガーシステムのテスト開始 ===");
        
        // 1. 基本セットアップ
        SetupTest();
        
        // 2. 各フェーズのトリガーテスト
        yield return StartCoroutine(TestBeforeHarvestTrigger());
        yield return StartCoroutine(TestHarvestStartTrigger());
        yield return StartCoroutine(TestFieldPhaseTrigger());
        yield return StartCoroutine(TestFeedingPhaseTrigger());
        yield return StartCoroutine(TestBreedingPhaseTrigger());
        yield return StartCoroutine(TestHarvestEndTrigger());
        
        // 3. 統合テスト
        yield return StartCoroutine(TestIntegratedHarvestFlow());
        
        Debug.Log("=== 細かな収穫フェーズトリガーシステムのテスト完了 ===");
    }
    
    void SetupTest()
    {
        Debug.Log("--- テスト環境セットアップ ---");
        
        // GameManagerを取得
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            GameObject managerObj = new GameObject("GameManager");
            gameManager = managerObj.AddComponent<GameManager>();
        }
        
        // テストプレイヤーを設定
        if (testPlayer == null)
        {
            GameObject playerObj = new GameObject("TestPlayer");
            testPlayer = playerObj.AddComponent<Player>();
            testPlayer.playerName = "テストプレイヤー";
        }
        
        // プレイヤーの初期状態を設定
        testPlayer.AddResource(ResourceType.Grain, 5);
        testPlayer.AddResource(ResourceType.Vegetable, 3);
        testPlayer.AddResource(ResourceType.Food, 2);
        testPlayer.AddResource(ResourceType.Sheep, 3);
        testPlayer.AddResource(ResourceType.Wood, 4);
        
        // 畑を追加
        testPlayer.AddField(2, 2);
        testPlayer.AddField(3, 2);
        testPlayer.Sow(ResourceType.Grain, 2, 2);
        testPlayer.Sow(ResourceType.Vegetable, 3, 2);
        
        Debug.Log($"テストプレイヤー初期状態: 穀物{testPlayer.GetResource(ResourceType.Grain)}, 野菜{testPlayer.GetResource(ResourceType.Vegetable)}, 食料{testPlayer.GetResource(ResourceType.Food)}");
    }
    
    System.Collections.IEnumerator TestBeforeHarvestTrigger()
    {
        Debug.Log("\n--- BeforeHarvestトリガーテスト ---");
        
        // 農夫カードを作成（収穫前に穀物+1）
        var farmerCard = CreateTestOccupationCard("農夫", OccupationType.Farmer, OccupationTrigger.BeforeHarvest);
        testPlayer.AddOccupation(farmerCard);
        
        int grainBefore = testPlayer.GetResource(ResourceType.Grain);
        Debug.Log($"収穫前の穀物: {grainBefore}個");
        
        // BeforeHarvestトリガーを発動
        gameManager.ExecuteAllTriggerableCards(OccupationTrigger.BeforeHarvest, testPlayer);
        
        int grainAfter = testPlayer.GetResource(ResourceType.Grain);
        Debug.Log($"収穫前フェーズ後の穀物: {grainAfter}個");
        Debug.Log($"BeforeHarvestトリガー効果: {grainAfter - grainBefore > 0}");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    System.Collections.IEnumerator TestHarvestStartTrigger()
    {
        Debug.Log("\n--- HarvestStartトリガーテスト ---");
        
        // 学者カードを作成（収穫開始時に効率化ボーナス）
        var scholarCard = CreateTestOccupationCard("学者", OccupationType.Scholar, OccupationTrigger.HarvestStart);
        testPlayer.AddOccupation(scholarCard);
        
        int bonusBefore = testPlayer.GetTempBonus("harvest_efficiency");
        Debug.Log($"収穫開始前の効率化ボーナス: {bonusBefore}");
        
        // HarvestStartトリガーを発動
        gameManager.ExecuteAllTriggerableCards(OccupationTrigger.HarvestStart, testPlayer);
        
        int bonusAfter = testPlayer.GetTempBonus("harvest_efficiency");
        Debug.Log($"収穫開始後の効率化ボーナス: {bonusAfter}");
        Debug.Log($"HarvestStartトリガー効果: {bonusAfter > bonusBefore}");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    System.Collections.IEnumerator TestFieldPhaseTrigger()
    {
        Debug.Log("\n--- FieldPhaseトリガーテスト ---");
        
        // 職人カードを作成（畑フェーズで野菜→食料変換）
        var craftsmanCard = CreateTestOccupationCard("職人", OccupationType.Craftsman, OccupationTrigger.FieldPhase);
        testPlayer.AddOccupation(craftsmanCard);
        
        int vegetableBefore = testPlayer.GetResource(ResourceType.Vegetable);
        int foodBefore = testPlayer.GetResource(ResourceType.Food);
        Debug.Log($"畑フェーズ前: 野菜{vegetableBefore}個, 食料{foodBefore}個");
        
        // FieldPhaseトリガーを発動
        gameManager.ExecuteAllTriggerableCards(OccupationTrigger.FieldPhase, testPlayer);
        
        int foodAfter = testPlayer.GetResource(ResourceType.Food);
        Debug.Log($"畑フェーズ後: 食料{foodAfter}個");
        Debug.Log($"FieldPhaseトリガー効果: {foodAfter > foodBefore}");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    System.Collections.IEnumerator TestFeedingPhaseTrigger()
    {
        Debug.Log("\n--- FeedingPhaseトリガーテスト ---");
        
        // 漁師カードを作成（食料供給フェーズで追加食料）
        var fishermanCard = CreateTestOccupationCard("漁師", OccupationType.Fisherman, OccupationTrigger.FeedingPhase);
        testPlayer.AddOccupation(fishermanCard);
        
        int foodBefore = testPlayer.GetResource(ResourceType.Food);
        Debug.Log($"食料供給フェーズ前: 食料{foodBefore}個");
        
        // FeedingPhaseトリガーを発動
        gameManager.ExecuteAllTriggerableCards(OccupationTrigger.FeedingPhase, testPlayer);
        
        int foodAfter = testPlayer.GetResource(ResourceType.Food);
        Debug.Log($"食料供給フェーズ後: 食料{foodAfter}個");
        Debug.Log($"FeedingPhaseトリガー効果: {foodAfter > foodBefore}");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    System.Collections.IEnumerator TestBreedingPhaseTrigger()
    {
        Debug.Log("\n--- BreedingPhaseトリガーテスト ---");
        
        // 羊飼いカードを作成（繁殖フェーズで追加羊）
        var shepherdCard = CreateTestOccupationCard("羊飼い", OccupationType.Shepherd, OccupationTrigger.BreedingPhase);
        testPlayer.AddOccupation(shepherdCard);
        
        int sheepBefore = testPlayer.GetResource(ResourceType.Sheep);
        Debug.Log($"繁殖フェーズ前: 羊{sheepBefore}匹");
        
        // BreedingPhaseトリガーを発動
        gameManager.ExecuteAllTriggerableCards(OccupationTrigger.BreedingPhase, testPlayer);
        
        int sheepAfter = testPlayer.GetResource(ResourceType.Sheep);
        Debug.Log($"繁殖フェーズ後: 羊{sheepAfter}匹");
        Debug.Log($"BreedingPhaseトリガー効果: {sheepAfter > sheepBefore}");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    System.Collections.IEnumerator TestHarvestEndTrigger()
    {
        Debug.Log("\n--- HarvestEndトリガーテスト ---");
        
        // 顧問カードを作成（収穫終了時に勝利点）
        var advisorCard = CreateTestOccupationCard("顧問", OccupationType.Advisor, OccupationTrigger.HarvestEnd);
        testPlayer.AddOccupation(advisorCard);
        
        int victoryPointsBefore = testPlayer.GetVictoryPoints();
        Debug.Log($"収穫終了前: 勝利点{victoryPointsBefore}点");
        
        // HarvestEndトリガーを発動
        gameManager.ExecuteAllTriggerableCards(OccupationTrigger.HarvestEnd, testPlayer);
        
        int victoryPointsAfter = testPlayer.GetVictoryPoints();
        Debug.Log($"収穫終了後: 勝利点{victoryPointsAfter}点");
        Debug.Log($"HarvestEndトリガー効果: {victoryPointsAfter > victoryPointsBefore}");
        
        yield return new WaitForSeconds(0.5f);
    }
    
    System.Collections.IEnumerator TestIntegratedHarvestFlow()
    {
        Debug.Log("\n--- 統合収穫フローテスト ---");
        
        // 新しいテストプレイヤーを作成
        GameObject integratedPlayerObj = new GameObject("IntegratedTestPlayer");
        Player integratedPlayer = integratedPlayerObj.AddComponent<Player>();
        integratedPlayer.playerName = "統合テストプレイヤー";
        
        // 初期リソース設定
        integratedPlayer.AddResource(ResourceType.Grain, 3);
        integratedPlayer.AddResource(ResourceType.Vegetable, 2);
        integratedPlayer.AddResource(ResourceType.Food, 1);
        integratedPlayer.AddResource(ResourceType.Sheep, 2);
        
        // 畑設定
        integratedPlayer.AddField(0, 0);
        integratedPlayer.Sow(ResourceType.Grain, 0, 0);
        
        // 複数の職業カードを追加
        integratedPlayer.AddOccupation(CreateTestOccupationCard("農夫", OccupationType.Farmer, OccupationTrigger.BeforeHarvest));
        integratedPlayer.AddOccupation(CreateTestOccupationCard("漁師", OccupationType.Fisherman, OccupationTrigger.FeedingPhase));
        integratedPlayer.AddOccupation(CreateTestOccupationCard("羊飼い", OccupationType.Shepherd, OccupationTrigger.BreedingPhase));
        
        Debug.Log($"統合テスト開始前: 穀物{integratedPlayer.GetResource(ResourceType.Grain)}, 食料{integratedPlayer.GetResource(ResourceType.Food)}, 羊{integratedPlayer.GetResource(ResourceType.Sheep)}");
        
        // 完全な収穫フローを実行
        List<Player> playerList = new List<Player> { integratedPlayer };
        
        // GameManagerのExecuteHarvestを直接呼び出すため、playersリストを一時的に設定
        var originalPlayers = gameManager.GetPlayers();
        gameManager.GetType().GetField("players", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(gameManager, playerList);
        
        // 統合収穫フローを実行
        gameManager.GetType().GetMethod("ExecuteHarvest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(gameManager, null);
        
        // 元のプレイヤーリストを復元
        gameManager.GetType().GetField("players", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(gameManager, originalPlayers);
        
        Debug.Log($"統合テスト完了後: 穀物{integratedPlayer.GetResource(ResourceType.Grain)}, 食料{integratedPlayer.GetResource(ResourceType.Food)}, 羊{integratedPlayer.GetResource(ResourceType.Sheep)}");
        
        yield return new WaitForSeconds(1f);
    }
    
    /// <summary>
    /// テスト用の職業カードを作成
    /// </summary>
    OccupationCard CreateTestOccupationCard(string name, OccupationType type, OccupationTrigger trigger)
    {
        var card = ScriptableObject.CreateInstance<OccupationCard>();
        card.cardName = name;
        card.occupationType = type;
        card.trigger = trigger;
        card.effectDescription = $"{name}の{trigger}効果";
        return card;
    }
    
    /// <summary>
    /// エディタ上でのテスト実行
    /// </summary>
    [ContextMenu("細かな収穫トリガーテスト実行")]
    public void RunTestInEditor()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(RunDetailedHarvestTriggerTests());
        }
        else
        {
            Debug.LogWarning("このテストはPlay Mode中に実行してください");
        }
    }
    
    /// <summary>
    /// 新しいトリガーシステムの使用例を表示
    /// </summary>
    [ContextMenu("新しいトリガーシステムの使用例")]
    public void ShowUsageExamples()
    {
        Debug.Log("=== 新しい収穫トリガーシステムの使用例 ===");
        Debug.Log("");
        Debug.Log("1. BeforeHarvest - 収穫の直前");
        Debug.Log("   用途: 畑の準備、道具の手入れ、事前ボーナス");
        Debug.Log("   例: 「収穫前に畑を整備して穀物+1個獲得」");
        Debug.Log("");
        Debug.Log("2. HarvestStart - 収穫の開始時");
        Debug.Log("   用途: 収穫効率の向上、全体的な指導効果");
        Debug.Log("   例: 「収穫開始時に効率化ボーナス獲得」");
        Debug.Log("");
        Debug.Log("3. FieldPhase - 畑フェーズ（作物収穫）");
        Debug.Log("   用途: 収穫量の増加、作物の品質向上");
        Debug.Log("   例: 「畑フェーズで野菜から食料を追加獲得」");
        Debug.Log("");
        Debug.Log("4. FeedingPhase - 食料供給フェーズ");
        Debug.Log("   用途: 食料不足の補填、変換効率の向上");
        Debug.Log("   例: 「食料供給フェーズで穀物→食料変換効率UP」");
        Debug.Log("");
        Debug.Log("5. BreedingPhase - 繁殖フェーズ");
        Debug.Log("   用途: 繁殖効率の向上、動物の追加獲得");
        Debug.Log("   例: 「繁殖フェーズで全動物の繁殖効率UP」");
        Debug.Log("");
        Debug.Log("6. HarvestEnd - 収穫終了時");
        Debug.Log("   用途: 収穫後の整理、次ラウンドの準備");
        Debug.Log("   例: 「収穫終了時に勝利点獲得」");
        Debug.Log("");
        Debug.Log("従来のOnHarvestとOnBreedingトリガーも互換性のため継続サポート");
    }
}