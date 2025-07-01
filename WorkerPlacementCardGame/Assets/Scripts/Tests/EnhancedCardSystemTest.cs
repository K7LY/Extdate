using UnityEngine;
using System.Collections.Generic;

public class EnhancedCardSystemTest : MonoBehaviour
{
    [Header("テスト設定")]
    public bool runTestsOnStart = true;
    public bool detailedLogging = true;
    
    [Header("テスト結果")]
    public int totalTests = 0;
    public int passedTests = 0;
    public int failedTests = 0;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    public void RunAllTests()
    {
        Debug.Log("=== 拡張カードシステム 統合テスト開始 ===");
        
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;
        
        TestCardCreation();
        TestTagSystem();
        TestEffectSystem();
        TestPlayConditions();
        TestPlayCosts();
        TestTriggerSystem();
        TestOccupationCards();
        TestImprovementCards();
        TestCardInteractions();
        
        Debug.Log($"\n=== テスト結果: {passedTests}/{totalTests} 成功 ===");
        if (failedTests > 0)
        {
            Debug.LogWarning($"失敗したテスト: {failedTests}個");
        }
    }
    
    private void TestCardCreation()
    {
        LogTestStart("カード作成テスト");
        
        // 職業カード作成
        var farmer = EnhancedCardFactory.CreateSampleOccupationCard("テスト農夫", "TEST_OCC_001", OccupationType.Farmer);
        AssertNotNull(farmer, "職業カード作成");
        AssertEqual(farmer.cardName, "テスト農夫", "職業カード名設定");
        AssertEqual(farmer.cardID, "TEST_OCC_001", "職業カードID設定");
        AssertEqual(farmer.occupationType, OccupationType.Farmer, "職業タイプ設定");
        
        // 進歩カード作成
        var oven = EnhancedCardFactory.CreateSampleImprovementCard("テストかまど", "TEST_IMP_001", ImprovementCategory.Minor);
        AssertNotNull(oven, "進歩カード作成");
        AssertEqual(oven.cardName, "テストかまど", "進歩カード名設定");
        AssertEqual(oven.cardID, "TEST_IMP_001", "進歩カードID設定");
        AssertEqual(oven.category, ImprovementCategory.Minor, "進歩カテゴリ設定");
        
        LogTestEnd("カード作成テスト");
    }
    
    private void TestTagSystem()
    {
        LogTestStart("タグシステムテスト");
        
        var card = EnhancedCardFactory.CreateSampleOccupationCard("タグテスト", "TAG_TEST_001", OccupationType.Farmer);
        
        // タグ追加テスト
        card.AddTag("テストタグ", "テスト用のタグ");
        AssertTrue(card.HasTag("テストタグ"), "タグ追加・検索");
        AssertTrue(card.HasTag("職業"), "デフォルトタグ存在確認");
        
        // タグ重複追加テスト
        int tagCountBefore = card.GetTagNames().Count;
        card.AddTag("テストタグ", "重複追加テスト");
        int tagCountAfter = card.GetTagNames().Count;
        AssertEqual(tagCountBefore, tagCountAfter, "タグ重複追加防止");
        
        // タグ削除テスト
        card.RemoveTag("テストタグ");
        AssertFalse(card.HasTag("テストタグ"), "タグ削除");
        
        LogTestEnd("タグシステムテスト");
    }
    
    private void TestEffectSystem()
    {
        LogTestStart("効果システムテスト");
        
        var card = EnhancedCardFactory.CreateSampleOccupationCard("効果テスト", "EFFECT_TEST_001", OccupationType.Carpenter);
        
        // 効果追加テスト
        var testEffect = EnhancedCardFactory.CreateResourceGainEffect("test_effect", ResourceType.Wood, 3);
        card.AddEffect(testEffect);
        
        var retrievedEffect = card.GetEffect("test_effect");
        AssertNotNull(retrievedEffect, "効果追加・取得");
        AssertEqual(retrievedEffect.effectID, "test_effect", "効果ID確認");
        
        // 効果削除テスト
        card.RemoveEffect("test_effect");
        var deletedEffect = card.GetEffect("test_effect");
        AssertNull(deletedEffect, "効果削除");
        
        // トリガー別効果取得テスト
        var immediateEffects = card.GetEffectsByTrigger(OccupationTrigger.Immediate);
        AssertTrue(immediateEffects.Count > 0, "トリガー別効果取得");
        
        LogTestEnd("効果システムテスト");
    }
    
    private void TestPlayConditions()
    {
        LogTestStart("プレイ条件テスト");
        
        // テスト用プレイヤー作成
        var player = CreateTestPlayer();
        player.AddResource(ResourceType.Wood, 5);
        player.AddResource(ResourceType.Clay, 3);
        
        // リソース条件テスト
        var condition = EnhancedCardFactory.CreateResourceCondition("test_condition", ResourceType.Wood, 3);
        AssertTrue(condition.CanMeet(player), "リソース条件満足");
        
        var failCondition = EnhancedCardFactory.CreateResourceCondition("fail_condition", ResourceType.Stone, 5);
        AssertFalse(failCondition.CanMeet(player), "リソース条件未満足");
        
        LogTestEnd("プレイ条件テスト");
    }
    
    private void TestPlayCosts()
    {
        LogTestStart("プレイコストテスト");
        
        // テスト用プレイヤー作成
        var player = CreateTestPlayer();
        player.AddResource(ResourceType.Wood, 5);
        player.AddResource(ResourceType.Clay, 3);
        
        // リソースコストテスト
        var cost = EnhancedCardFactory.CreateResourceCost("test_cost", ResourceType.Wood, 2);
        AssertTrue(cost.CanPay(player), "リソースコスト支払い可能");
        
        int woodBefore = player.GetResource(ResourceType.Wood);
        cost.Pay(player);
        int woodAfter = player.GetResource(ResourceType.Wood);
        AssertEqual(woodBefore - 2, woodAfter, "リソースコスト支払い実行");
        
        LogTestEnd("プレイコストテスト");
    }
    
    private void TestTriggerSystem()
    {
        LogTestStart("トリガーシステムテスト");
        
        var player = CreateTestPlayer();
        var card = EnhancedCardFactory.CreateSampleOccupationCard("トリガーテスト", "TRIGGER_TEST_001", OccupationType.Fisherman);
        
        // 即座効果テスト
        int foodBefore = player.GetResource(ResourceType.Food);
        card.PlayCard(player);
        int foodAfter = player.GetResource(ResourceType.Food);
        AssertTrue(foodAfter > foodBefore, "即座効果発動");
        
        // 収穫トリガーテスト
        int foodBeforeHarvest = player.GetResource(ResourceType.Food);
        card.TriggerEffect(player, OccupationTrigger.OnHarvest);
        int foodAfterHarvest = player.GetResource(ResourceType.Food);
        AssertTrue(foodAfterHarvest > foodBeforeHarvest, "収穫トリガー効果発動");
        
        LogTestEnd("トリガーシステムテスト");
    }
    
    private void TestOccupationCards()
    {
        LogTestStart("職業カードテスト");
        
        var player = CreateTestPlayer();
        var farmer = EnhancedCardFactory.CreateSampleOccupationCard("テスト農夫", "OCC_TEST_001", OccupationType.Farmer);
        
        // 職業カードプレイテスト
        AssertTrue(farmer.CanPlay(player), "職業カードプレイ可能");
        
        farmer.PlayCard(player);
        AssertTrue(player.HasOccupationByName("テスト農夫"), "職業カード追加確認");
        
        // 重複プレイ防止テスト
        AssertFalse(farmer.CanPlay(player), "職業カード重複プレイ防止");
        
        LogTestEnd("職業カードテスト");
    }
    
    private void TestImprovementCards()
    {
        LogTestStart("進歩カードテスト");
        
        var player = CreateTestPlayer();
        player.AddResource(ResourceType.Wood, 5);
        player.AddResource(ResourceType.Clay, 5);
        player.AddResource(ResourceType.Stone, 5);
        
        var oven = EnhancedCardFactory.CreateSampleImprovementCard("テストかまど", "IMP_TEST_001", ImprovementCategory.Minor);
        
        // 進歩カードプレイテスト
        AssertTrue(oven.CanPlay(player), "進歩カードプレイ可能");
        
        int victoryPointsBefore = player.GetVictoryPoints();
        oven.PlayCard(player);
        int victoryPointsAfter = player.GetVictoryPoints();
        
        AssertTrue(player.HasImprovementByName("テストかまど"), "進歩カード追加確認");
        AssertTrue(victoryPointsAfter > victoryPointsBefore, "進歩カード勝利点追加");
        
        // 料理設備テスト
        if (oven.canCookGrain)
        {
            player.AddResource(ResourceType.Grain, 3);
            int foodBefore = player.GetResource(ResourceType.Food);
            int grainBefore = player.GetResource(ResourceType.Grain);
            
            oven.CookResourceEnhanced(player, ResourceType.Grain, 1);
            
            int foodAfter = player.GetResource(ResourceType.Food);
            int grainAfter = player.GetResource(ResourceType.Grain);
            
            AssertEqual(grainBefore - 1, grainAfter, "料理でのグレイン消費");
            AssertTrue(foodAfter > foodBefore, "料理での食料獲得");
        }
        
        LogTestEnd("進歩カードテスト");
    }
    
    private void TestCardInteractions()
    {
        LogTestStart("カード連携テスト");
        
        var player = CreateTestPlayer();
        player.AddResource(ResourceType.Wood, 10);
        player.AddResource(ResourceType.Clay, 10);
        
        // 複数カードのプレイ
        var carpenter = EnhancedCardFactory.CreateSampleOccupationCard("大工", "CARPENTER_TEST", OccupationType.Carpenter);
        var oven = EnhancedCardFactory.CreateSampleImprovementCard("かまど", "OVEN_TEST", ImprovementCategory.Minor);
        
        carpenter.PlayCard(player);
        oven.PlayCard(player);
        
        // タグ検索テスト
        AssertTrue(carpenter.HasTag("建築"), "職業カードタグ確認");
        AssertTrue(oven.HasTag("料理設備"), "進歩カードタグ確認");
        
        // 効果リセットテスト
        carpenter.ResetAllEffectUses();
        oven.ResetAllEffectUses();
        
        LogTestEnd("カード連携テスト");
    }
    
    // テストヘルパーメソッド
    private Player CreateTestPlayer()
    {
        var player = new GameObject("TestPlayer").AddComponent<Player>();
        player.InitializePlayer("テストプレイヤー", 0);
        return player;
    }
    
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
}