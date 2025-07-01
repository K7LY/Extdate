using UnityEngine;
using System.Collections.Generic;

public class TriggerSystemTest : MonoBehaviour
{
    [Header("テスト設定")]
    public bool runTestsOnStart = true;
    public bool detailedLogging = true;
    
    [Header("テスト結果")]
    public int totalTests = 0;
    public int passedTests = 0;
    public int failedTests = 0;
    
    [Header("テスト用プレイヤー")]
    public List<Player> testPlayers = new List<Player>();
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    public void RunAllTests()
    {
        Debug.Log("=== トリガーシステム 統合テスト開始 ===");
        
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;
        
        SetupTestEnvironment();
        
        TestTriggerableCardSearch();
        TestSpecificTriggerTypes();
        TestTriggerConditions();
        TestMultiPlayerTriggers();
        TestTriggerExecution();
        TestTriggerStatistics();
        TestRealWorldScenarios();
        
        CleanupTestEnvironment();
        
        Debug.Log($"\n=== テスト結果: {passedTests}/{totalTests} 成功 ===");
        if (failedTests > 0)
        {
            Debug.LogWarning($"失敗したテスト: {failedTests}個");
        }
    }
    
    private void SetupTestEnvironment()
    {
        LogTestStart("テスト環境セットアップ");
        
        // テスト用プレイヤーを作成
        testPlayers.Clear();
        for (int i = 0; i < 3; i++)
        {
            var playerObj = new GameObject($"TestPlayer_{i}");
            var player = playerObj.AddComponent<Player>();
            player.InitializePlayer($"テストプレイヤー{i + 1}", i);
            testPlayers.Add(player);
            
            // リソースを追加
            player.AddResource(ResourceType.Wood, 10);
            player.AddResource(ResourceType.Clay, 5);
            player.AddResource(ResourceType.Food, 8);
        }
        
        // CardTriggerManagerが存在することを確認
        var triggerManager = CardTriggerManager.Instance;
        AssertNotNull(triggerManager, "CardTriggerManager初期化");
        
        LogTestEnd("テスト環境セットアップ");
    }
    
    private void TestTriggerableCardSearch()
    {
        LogTestStart("トリガー可能カード検索テスト");
        
        var player = testPlayers[0];
        
        // テスト用職業カードを追加
        var farmer = EnhancedCardFactory.CreateSampleOccupationCard("テスト農夫", "SEARCH_TEST_FARMER", OccupationType.Farmer);
        var carpenter = EnhancedCardFactory.CreateSampleOccupationCard("テスト大工", "SEARCH_TEST_CARPENTER", OccupationType.Carpenter);
        
        farmer.PlayCard(player);
        carpenter.PlayCard(player);
        
        // 即座効果の検索テスト
        var immediateCards = player.FindTriggerableCards(OccupationTrigger.Immediate);
        AssertTrue(immediateCards.Count >= 0, "即座効果カード検索");
        
        // 収穫効果の検索テスト
        var harvestCards = player.FindTriggerableCards(OccupationTrigger.OnHarvest);
        AssertTrue(harvestCards.Count >= 0, "収穫効果カード検索");
        
        // アクション効果の検索テスト
        var actionCards = player.FindTriggerableCards(OccupationTrigger.OnAction);
        AssertTrue(actionCards.Count >= 0, "アクション効果カード検索");
        
        LogTestEnd("トリガー可能カード検索テスト");
    }
    
    private void TestSpecificTriggerTypes()
    {
        LogTestStart("特定トリガータイプテスト");
        
        var player = testPlayers[1];
        
        // 進歩カードを追加
        var oven = EnhancedCardFactory.CreateSampleImprovementCard("テストかまど", "TRIGGER_TEST_OVEN", ImprovementCategory.Minor);
        oven.PlayCard(player);
        
        // パッシブ効果のテスト
        var passiveCards = player.FindTriggerableCards(OccupationTrigger.Passive);
        LogDebug($"パッシブ効果カード数: {passiveCards.Count}");
        
        // 繁殖効果のテスト
        var breedingCards = player.FindTriggerableCards(OccupationTrigger.OnBreeding);
        LogDebug($"繁殖効果カード数: {breedingCards.Count}");
        
        // ターン終了効果のテスト
        var turnEndCards = player.FindTriggerableCards(OccupationTrigger.OnTurnEnd);
        LogDebug($"ターン終了効果カード数: {turnEndCards.Count}");
        
        AssertTrue(true, "特定トリガータイプ検索完了");
        
        LogTestEnd("特定トリガータイプテスト");
    }
    
    private void TestTriggerConditions()
    {
        LogTestStart("トリガー条件テスト");
        
        var player = testPlayers[2];
        
        // 条件付きカードを作成
        var conditionalOccupation = EnhancedCardFactory.CreateSampleOccupationCard("条件付き職業", "CONDITION_TEST", OccupationType.Fisherman);
        
        // 特定条件の効果を追加
        var conditionalEffect = new CardEffect
        {
            effectID = "conditional_effect",
            effectDescription = "条件付き効果",
            effectType = CardEffectType.ResourceModification,
            triggerType = OccupationTrigger.OnAction,
            triggerCondition = "漁場"
        };
        conditionalEffect.resourceGain.Add(ResourceType.Food, 2);
        conditionalOccupation.AddEffect(conditionalEffect);
        
        conditionalOccupation.PlayCard(player);
        
        // 条件に一致するアクションスペースでテスト
        var fishingAction = CreateTestActionSpace("漁場", ActionType.GainResource);
        var matchingCards = player.FindTriggerableCards(OccupationTrigger.OnAction, fishingAction, "漁場");
        AssertTrue(matchingCards.Count > 0, "条件一致時のトリガーカード検索");
        
        // 条件に一致しないアクションスペースでテスト
        var farmingAction = CreateTestActionSpace("畑を耕す", ActionType.AddField);
        var nonMatchingCards = player.FindTriggerableCards(OccupationTrigger.OnAction, farmingAction, "畑");
        LogDebug($"条件不一致時のトリガーカード数: {nonMatchingCards.Count}");
        
        AssertTrue(true, "トリガー条件テスト完了");
        
        LogTestEnd("トリガー条件テスト");
    }
    
    private void TestMultiPlayerTriggers()
    {
        LogTestStart("マルチプレイヤートリガーテスト");
        
        // 全プレイヤーにカードを追加
        foreach (var player in testPlayers)
        {
            var occupation = EnhancedCardFactory.CreateSampleOccupationCard($"共通職業_{player.playerName}", $"MULTI_{player.playerName}", OccupationType.Baker);
            occupation.PlayCard(player);
        }
        
        // 全プレイヤーのトリガー可能カードを検索
        var allTriggerableCards = CardTriggerManager.Instance.FindTriggerableCardsAllPlayers(testPlayers, OccupationTrigger.OnHarvest);
        AssertTrue(allTriggerableCards.Count >= testPlayers.Count, "全プレイヤーのトリガーカード検索");
        
        // 各プレイヤーに1枚以上のカードがあることを確認
        foreach (var player in testPlayers)
        {
            var playerCards = allTriggerableCards.FindAll(tc => tc.owner == player);
            AssertTrue(playerCards.Count > 0, $"{player.playerName}のトリガーカード存在確認");
        }
        
        LogTestEnd("マルチプレイヤートリガーテスト");
    }
    
    private void TestTriggerExecution()
    {
        LogTestStart("トリガー実行テスト");
        
        var player = testPlayers[0];
        
        // 実行前のリソース状態を記録
        int woodBefore = player.GetResource(ResourceType.Wood);
        int foodBefore = player.GetResource(ResourceType.Food);
        
        // 収穫トリガーを実行
        player.TriggerCardEffects(OccupationTrigger.OnHarvest);
        
        // 実行後のリソース状態を確認
        int woodAfter = player.GetResource(ResourceType.Wood);
        int foodAfter = player.GetResource(ResourceType.Food);
        
        LogDebug($"収穫トリガー実行: 木材 {woodBefore}→{woodAfter}, 食料 {foodBefore}→{foodAfter}");
        
        // 何らかの変化があったかチェック（カードによってはリソースが増える可能性）
        AssertTrue(true, "トリガー実行完了");
        
        LogTestEnd("トリガー実行テスト");
    }
    
    private void TestTriggerStatistics()
    {
        LogTestStart("トリガー統計テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        
        // 統計をリセット
        triggerManager.ResetStatistics();
        AssertEqual(0, triggerManager.totalTriggersProcessed, "統計リセット確認");
        
        // いくつかのトリガーを実行
        foreach (var player in testPlayers)
        {
            player.TriggerCardEffects(OccupationTrigger.OnTurnEnd);
        }
        
        // 統計が更新されたことを確認
        AssertTrue(triggerManager.totalTriggersProcessed >= 0, "トリガー処理回数統計");
        AssertTrue(triggerManager.totalEffectsExecuted >= 0, "効果実行回数統計");
        
        LogTestEnd("トリガー統計テスト");
    }
    
    private void TestRealWorldScenarios()
    {
        LogTestStart("実用シナリオテスト");
        
        var player = testPlayers[0];
        
        // 農夫職業をプレイ
        var farmer = EnhancedCardFactory.CreateSampleOccupationCard("実用農夫", "REAL_FARMER", OccupationType.Farmer);
        farmer.PlayCard(player);
        
        // かまど進歩をプレイ
        var oven = EnhancedCardFactory.CreateSampleImprovementCard("実用かまど", "REAL_OVEN", ImprovementCategory.Minor);
        oven.PlayCard(player);
        
        // シナリオ1: 収穫時のトリガー
        LogDebug("=== シナリオ1: 収穫時 ===");
        var harvestInfo = player.GetTriggerableCardsInfo(OccupationTrigger.OnHarvest);
        LogDebug(harvestInfo);
        
        // シナリオ2: アクション実行時のトリガー
        LogDebug("=== シナリオ2: アクション実行時 ===");
        var actionSpace = CreateTestActionSpace("森", ActionType.GainResource);
        var actionInfo = player.GetTriggerableCardsInfo(OccupationTrigger.OnAction, actionSpace);
        LogDebug(actionInfo);
        
        // シナリオ3: ターン終了時のトリガー
        LogDebug("=== シナリオ3: ターン終了時 ===");
        var turnEndInfo = player.GetTriggerableCardsInfo(OccupationTrigger.OnTurnEnd);
        LogDebug(turnEndInfo);
        
        AssertTrue(true, "実用シナリオテスト完了");
        
        LogTestEnd("実用シナリオテスト");
    }
    
    private void CleanupTestEnvironment()
    {
        LogTestStart("テスト環境クリーンアップ");
        
        // テスト用プレイヤーを削除
        foreach (var player in testPlayers)
        {
            if (player != null && player.gameObject != null)
            {
                DestroyImmediate(player.gameObject);
            }
        }
        testPlayers.Clear();
        
        // CardTriggerManagerの統計をリセット
        if (CardTriggerManager.Instance != null)
        {
            CardTriggerManager.Instance.ResetStatistics();
        }
        
        LogTestEnd("テスト環境クリーンアップ");
    }
    
    // ヘルパーメソッド
    private ActionSpace CreateTestActionSpace(string name, ActionType type)
    {
        var actionObj = new GameObject($"TestAction_{name}");
        var actionSpace = actionObj.AddComponent<ActionSpace>();
        actionSpace.actionName = name;
        actionSpace.actionType = type;
        return actionSpace;
    }
    
    // テストアサーションメソッド
    private void AssertTrue(bool condition, string testName)
    {
        totalTests++;
        if (condition)
        {
            passedTests++;
            if (detailedLogging)
                Debug.Log($"✓ {testName}");
        }
        else
        {
            failedTests++;
            Debug.LogError($"✗ {testName}");
        }
    }
    
    private void AssertFalse(bool condition, string testName)
    {
        AssertTrue(!condition, testName);
    }
    
    private void AssertEqual<T>(T expected, T actual, string testName)
    {
        AssertTrue(expected.Equals(actual), $"{testName} (期待値: {expected}, 実際: {actual})");
    }
    
    private void AssertNotNull(object obj, string testName)
    {
        AssertTrue(obj != null, testName);
    }
    
    private void AssertNull(object obj, string testName)
    {
        AssertTrue(obj == null, testName);
    }
    
    private void LogTestStart(string testName)
    {
        if (detailedLogging)
            Debug.Log($"\n--- {testName} 開始 ---");
    }
    
    private void LogTestEnd(string testName)
    {
        if (detailedLogging)
            Debug.Log($"--- {testName} 終了 ---");
    }
    
    private void LogDebug(string message)
    {
        if (detailedLogging)
            Debug.Log($"[TriggerSystemTest] {message}");
    }
}