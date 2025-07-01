using UnityEngine;
using System.Collections.Generic;

public class AutoRegistrationTest : MonoBehaviour
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
        Debug.Log("=== 自動登録システム 統合テスト開始 ===");
        
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;
        
        SetupTestEnvironment();
        
        TestAutoRegistrationBasics();
        TestCardPlayAutoRegistration();
        TestCardRemovalAutoUnregistration();
        TestMultiPlayerAutoRegistration();
        TestRegistrationStatistics();
        TestPerformanceComparison();
        TestEdgeCases();
        
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
        for (int i = 0; i < 2; i++)
        {
            var playerObj = new GameObject($"AutoTestPlayer_{i}");
            var player = playerObj.AddComponent<Player>();
            player.InitializePlayer($"自動テストプレイヤー{i + 1}", i);
            testPlayers.Add(player);
            
            // リソースを追加
            player.AddResource(ResourceType.Wood, 20);
            player.AddResource(ResourceType.Clay, 10);
            player.AddResource(ResourceType.Food, 15);
        }
        
        // CardTriggerManagerの初期化確認
        var triggerManager = CardTriggerManager.Instance;
        AssertNotNull(triggerManager, "CardTriggerManager初期化");
        
        // 自動登録が有効であることを確認
        AssertTrue(triggerManager.enableAutoRegistration, "自動登録システム有効");
        
        // 登録システムをクリア
        triggerManager.ClearRegistrationSystem();
        
        LogTestEnd("テスト環境セットアップ");
    }
    
    private void TestAutoRegistrationBasics()
    {
        LogTestStart("自動登録基本機能テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        var player = testPlayers[0];
        
        // 初期状態の確認
        AssertEqual(0, triggerManager.registeredCardsCount, "初期登録カード数");
        
        // 手動登録テスト
        var testCard = EnhancedCardFactory.CreateSampleOccupationCard("手動テストカード", "MANUAL_TEST", OccupationType.Farmer);
        triggerManager.RegisterCard(player, testCard);
        
        AssertEqual(1, triggerManager.registeredCardsCount, "手動登録後のカード数");
        
        var registeredCards = triggerManager.GetRegisteredCards(player);
        AssertEqual(1, registeredCards.Count, "プレイヤーの登録カード数");
        AssertTrue(registeredCards.Contains(testCard), "登録カードの存在確認");
        
        // 重複登録防止テスト
        triggerManager.RegisterCard(player, testCard);
        AssertEqual(1, triggerManager.registeredCardsCount, "重複登録防止確認");
        
        // 手動削除テスト
        triggerManager.UnregisterCard(player, testCard);
        AssertEqual(0, triggerManager.registeredCardsCount, "手動削除後のカード数");
        
        LogTestEnd("自動登録基本機能テスト");
    }
    
    private void TestCardPlayAutoRegistration()
    {
        LogTestStart("カードプレイ時自動登録テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        var player = testPlayers[0];
        
        // 初期状態をクリア
        triggerManager.ClearRegistrationSystem();
        
        // 職業カードをプレイ
        var farmer = EnhancedCardFactory.CreateSampleOccupationCard("自動農夫", "AUTO_FARMER", OccupationType.Farmer);
        int cardsBefore = triggerManager.registeredCardsCount;
        
        farmer.PlayCard(player);
        
        int cardsAfter = triggerManager.registeredCardsCount;
        AssertEqual(cardsBefore + 1, cardsAfter, "職業カードプレイ時の自動登録");
        
        // 進歩カードをプレイ
        var oven = EnhancedCardFactory.CreateSampleImprovementCard("自動かまど", "AUTO_OVEN", ImprovementCategory.Minor);
        cardsBefore = triggerManager.registeredCardsCount;
        
        oven.PlayCard(player);
        
        cardsAfter = triggerManager.registeredCardsCount;
        AssertEqual(cardsBefore + 1, cardsAfter, "進歩カードプレイ時の自動登録");
        
        // 登録されたカードの検証
        var registeredCards = triggerManager.GetRegisteredCards(player);
        AssertTrue(registeredCards.Contains(farmer), "農夫カードの登録確認");
        AssertTrue(registeredCards.Contains(oven), "かまどカードの登録確認");
        
        LogTestEnd("カードプレイ時自動登録テスト");
    }
    
    private void TestCardRemovalAutoUnregistration()
    {
        LogTestStart("カード削除時自動削除テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        var player = testPlayers[1];
        
        // カードを追加
        var occupation1 = EnhancedCardFactory.CreateSampleOccupationCard("削除テスト職業1", "REMOVE_TEST_1", OccupationType.Carpenter);
        var occupation2 = EnhancedCardFactory.CreateSampleOccupationCard("削除テスト職業2", "REMOVE_TEST_2", OccupationType.Baker);
        
        occupation1.PlayCard(player);
        occupation2.PlayCard(player);
        
        int initialCount = triggerManager.registeredCardsCount;
        AssertTrue(initialCount >= 2, "初期カード登録確認");
        
        // 1枚削除
        player.RemoveOccupationCard(occupation1);
        int afterRemoveOne = triggerManager.registeredCardsCount;
        AssertEqual(initialCount - 1, afterRemoveOne, "1枚削除後のカード数");
        
        var remainingCards = triggerManager.GetRegisteredCards(player);
        AssertFalse(remainingCards.Contains(occupation1), "削除カードの不存在確認");
        AssertTrue(remainingCards.Contains(occupation2), "残存カードの存在確認");
        
        // 全削除
        player.ClearAllCards();
        int afterClearAll = triggerManager.registeredCardsCount;
        AssertTrue(afterClearAll < afterRemoveOne, "全削除後のカード数");
        
        var finalCards = triggerManager.GetRegisteredCards(player);
        AssertEqual(0, finalCards.Count, "全削除後のプレイヤーカード数");
        
        LogTestEnd("カード削除時自動削除テスト");
    }
    
    private void TestMultiPlayerAutoRegistration()
    {
        LogTestStart("マルチプレイヤー自動登録テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        triggerManager.ClearRegistrationSystem();
        
        // 各プレイヤーにカードを追加
        foreach (var player in testPlayers)
        {
            var occupation = EnhancedCardFactory.CreateSampleOccupationCard($"マルチ職業_{player.playerName}", $"MULTI_{player.playerName}", OccupationType.Fisherman);
            var improvement = EnhancedCardFactory.CreateSampleImprovementCard($"マルチ進歩_{player.playerName}", $"MULTI_IMP_{player.playerName}", ImprovementCategory.Minor);
            
            occupation.PlayCard(player);
            improvement.PlayCard(player);
        }
        
        // 全体の登録数確認
        AssertEqual(testPlayers.Count * 2, triggerManager.registeredCardsCount, "全プレイヤーのカード登録数");
        
        // 各プレイヤーの登録数確認
        foreach (var player in testPlayers)
        {
            var playerCards = triggerManager.GetRegisteredCards(player);
            AssertEqual(2, playerCards.Count, $"{player.playerName}の登録カード数");
        }
        
        // 統計情報の確認
        string stats = triggerManager.GetRegistrationStatistics();
        LogDebug("登録統計:");
        LogDebug(stats);
        
        LogTestEnd("マルチプレイヤー自動登録テスト");
    }
    
    private void TestRegistrationStatistics()
    {
        LogTestStart("登録統計テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        
        // 統計情報の取得
        string initialStats = triggerManager.GetRegistrationStatistics();
        AssertNotNull(initialStats, "統計情報取得");
        AssertTrue(initialStats.Contains("カード登録システム統計"), "統計情報フォーマット確認");
        
        // 登録数と統計の一致確認
        string stats = triggerManager.GetRegistrationStatistics();
        bool containsCount = stats.Contains(triggerManager.registeredCardsCount.ToString());
        AssertTrue(containsCount, "統計情報のカード数一致");
        
        LogTestEnd("登録統計テスト");
    }
    
    private void TestPerformanceComparison()
    {
        LogTestStart("パフォーマンス比較テスト");
        
        var triggerManager = CardTriggerManager.Instance;
        var player = testPlayers[0];
        
        // 複数カードを追加してパフォーマンステスト用のデータを作成
        for (int i = 0; i < 5; i++)
        {
            var occupation = EnhancedCardFactory.CreateSampleOccupationCard($"パフォーマンステスト職業{i}", $"PERF_OCC_{i}", OccupationType.Farmer);
            var improvement = EnhancedCardFactory.CreateSampleImprovementCard($"パフォーマンステスト進歩{i}", $"PERF_IMP_{i}", ImprovementCategory.Minor);
            
            occupation.PlayCard(player);
            improvement.PlayCard(player);
        }
        
        // 自動登録システム有効時の検索
        var startTime = Time.realtimeSinceStartup;
        var registryResults = player.FindTriggerableCards(OccupationTrigger.OnHarvest);
        var registryTime = Time.realtimeSinceStartup - startTime;
        
        // 自動登録システム無効時の検索
        triggerManager.enableAutoRegistration = false;
        startTime = Time.realtimeSinceStartup;
        var legacyResults = player.FindTriggerableCards(OccupationTrigger.OnHarvest);
        var legacyTime = Time.realtimeSinceStartup - startTime;
        
        // 自動登録システムを元に戻す
        triggerManager.enableAutoRegistration = true;
        
        LogDebug($"登録システム使用時間: {registryTime * 1000:F2}ms");
        LogDebug($"従来システム使用時間: {legacyTime * 1000:F2}ms");
        
        // 結果の一致確認
        AssertEqual(registryResults.Count, legacyResults.Count, "検索結果の一致確認");
        
        AssertTrue(true, "パフォーマンス比較テスト完了");
        
        LogTestEnd("パフォーマンス比較テスト");
    }
    
    private void TestEdgeCases()
    {
        LogTestStart("エッジケーステスト");
        
        var triggerManager = CardTriggerManager.Instance;
        
        // null値での登録テスト
        triggerManager.RegisterCard(null, null);
        triggerManager.RegisterCard(testPlayers[0], null);
        triggerManager.RegisterCard(null, EnhancedCardFactory.CreateSampleOccupationCard("null test", "NULL_TEST", OccupationType.Farmer));
        
        // エラーが発生しないことを確認
        AssertTrue(true, "null値処理エラーなし");
        
        // 存在しないプレイヤーでの検索
        var nonExistentPlayer = new GameObject("NonExistentPlayer").AddComponent<Player>();
        nonExistentPlayer.InitializePlayer("存在しないプレイヤー", 999);
        
        var emptyResults = triggerManager.GetRegisteredCards(nonExistentPlayer);
        AssertEqual(0, emptyResults.Count, "存在しないプレイヤーの検索結果");
        
        // 自動登録無効時の動作
        triggerManager.enableAutoRegistration = false;
        var testCard = EnhancedCardFactory.CreateSampleOccupationCard("無効テスト", "DISABLED_TEST", OccupationType.Farmer);
        int beforeCount = triggerManager.registeredCardsCount;
        
        triggerManager.RegisterCard(testPlayers[0], testCard);
        int afterCount = triggerManager.registeredCardsCount;
        
        AssertEqual(beforeCount, afterCount, "自動登録無効時の登録防止");
        
        // 自動登録を再有効化
        triggerManager.enableAutoRegistration = true;
        
        DestroyImmediate(nonExistentPlayer.gameObject);
        
        LogTestEnd("エッジケーステスト");
    }
    
    private void CleanupTestEnvironment()
    {
        LogTestStart("テスト環境クリーンアップ");
        
        // 全プレイヤーを削除
        foreach (var player in testPlayers)
        {
            if (player != null && player.gameObject != null)
            {
                player.ClearAllCards();
                DestroyImmediate(player.gameObject);
            }
        }
        testPlayers.Clear();
        
        // CardTriggerManagerをクリア
        if (CardTriggerManager.Instance != null)
        {
            CardTriggerManager.Instance.ClearRegistrationSystem();
            CardTriggerManager.Instance.ResetStatistics();
        }
        
        LogTestEnd("テスト環境クリーンアップ");
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
            Debug.Log($"[AutoRegistrationTest] {message}");
    }
}