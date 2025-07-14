using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// インタラクティブ繁殖システムのテストクラス
/// </summary>
public class InteractiveBreedingTest : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private float testDelay = 1.0f;
    
    [Header("参照")]
    [SerializeField] private Player testPlayer;
    [SerializeField] private AnimalBreedingManager breedingManager;
    
    void Start()
    {
        // 自動的に参照を取得
        if (testPlayer == null)
            testPlayer = FindObjectOfType<Player>();
        
        if (breedingManager == null)
            breedingManager = FindObjectOfType<AnimalBreedingManager>();
        
        // テストプレイヤーが見つからない場合は警告
        if (testPlayer == null)
        {
            DebugLog("⚠️ テスト用のPlayerが見つかりません");
        }
        
        if (breedingManager == null)
        {
            DebugLog("⚠️ AnimalBreedingManagerが見つかりません");
        }
    }
    
    /// <summary>
    /// 基本的な繁殖テスト（容量十分）
    /// </summary>
    [ContextMenu("1. 基本繁殖テスト（容量十分）")]
    public void TestBasicBreeding()
    {
        if (!ValidateComponents()) return;
        
        DebugLog("=== 基本繁殖テスト開始 ===");
        
        // テスト状況をセットアップ
        SetupBasicBreedingScenario();
        
        // インタラクティブ繁殖を開始
        breedingManager.StartInteractiveBreeding(testPlayer);
    }
    
    /// <summary>
    /// 容量不足繁殖テスト
    /// </summary>
    [ContextMenu("2. 容量不足繁殖テスト")]
    public void TestCapacityShortageBreeding()
    {
        if (!ValidateComponents()) return;
        
        DebugLog("=== 容量不足繁殖テスト開始 ===");
        
        // 容量不足の状況をセットアップ
        SetupCapacityShortageScenario();
        
        // インタラクティブ繁殖を開始
        breedingManager.StartInteractiveBreeding(testPlayer);
    }
    
    /// <summary>
    /// 複雑な容量不足テスト
    /// </summary>
    [ContextMenu("3. 複雑な容量不足テスト")]
    public void TestComplexCapacityShortage()
    {
        if (!ValidateComponents()) return;
        
        DebugLog("=== 複雑な容量不足テスト開始 ===");
        
        // 複雑な容量不足の状況をセットアップ
        SetupComplexCapacityShortageScenario();
        
        // インタラクティブ繁殖を開始
        breedingManager.StartInteractiveBreeding(testPlayer);
    }
    
    /// <summary>
    /// 自動削減テスト（AIプレイヤー用）
    /// </summary>
    [ContextMenu("4. 自動削減テスト")]
    public void TestAutoReduction()
    {
        if (!ValidateComponents()) return;
        
        DebugLog("=== 自動削減テスト開始 ===");
        
        // 容量不足の状況をセットアップ
        SetupCapacityShortageScenario();
        
        StartCoroutine(AutoReductionCoroutine());
    }
    
    /// <summary>
    /// 基本繁殖シナリオをセットアップ
    /// </summary>
    private void SetupBasicBreedingScenario()
    {
        DebugLog("📋 基本繁殖シナリオ設定中...");
        
        // リソースをクリア
        ClearPlayerResources();
        
        // 動物を設定（繁殖可能な数）
        testPlayer.AddResource(ResourceType.Sheep, 3);    // 羊3匹（繁殖可能）
        testPlayer.AddResource(ResourceType.Cattle, 2);   // 牛2匹（繁殖可能）
        testPlayer.AddResource(ResourceType.Boar, 1);     // 猪1匹（繁殖不可）
        
        // 十分な容量を設定
        testPlayer.AddStables(3);  // 小屋3つ = 3匹容量
        testPlayer.AddFences(4);   // 牧場2つ = 4匹容量（牧場1つあたり2匹）
        // 合計容量: 7匹, 現在の動物: 6匹, 必要容量: +2匹 → 問題なし
        
        LogPlayerStatus("基本繁殖シナリオ");
    }
    
    /// <summary>
    /// 容量不足シナリオをセットアップ
    /// </summary>
    private void SetupCapacityShortageScenario()
    {
        DebugLog("📋 容量不足シナリオ設定中...");
        
        // リソースをクリア
        ClearPlayerResources();
        
        // 動物を設定（多くの動物で容量を圧迫）
        testPlayer.AddResource(ResourceType.Sheep, 3);    // 羊3匹（繁殖可能）
        testPlayer.AddResource(ResourceType.Cattle, 2);   // 牛2匹（繁殖可能）
        testPlayer.AddResource(ResourceType.Boar, 2);     // 猪2匹（繁殖可能）
        
        // 不十分な容量を設定
        testPlayer.AddStables(1);  // 小屋1つ = 1匹容量
        testPlayer.AddFences(2);   // 牧場1つ = 2匹容量
        // 合計容量: 3匹, 現在の動物: 7匹, 必要容量: +3匹 → 容量大幅不足
        
        LogPlayerStatus("容量不足シナリオ");
    }
    
    /// <summary>
    /// 複雑な容量不足シナリオをセットアップ
    /// </summary>
    private void SetupComplexCapacityShortageScenario()
    {
        DebugLog("📋 複雑な容量不足シナリオ設定中...");
        
        // リソースをクリア
        ClearPlayerResources();
        
        // 動物を設定（微妙な容量不足）
        testPlayer.AddResource(ResourceType.Sheep, 4);    // 羊4匹（繁殖可能）
        testPlayer.AddResource(ResourceType.Cattle, 3);   // 牛3匹（繁殖可能）
        testPlayer.AddResource(ResourceType.Boar, 2);     // 猪2匹（繁殖可能）
        
        // ギリギリ不足の容量を設定
        testPlayer.AddStables(2);  // 小屋2つ = 2匹容量
        testPlayer.AddFences(6);   // 牧場3つ = 6匹容量
        // 合計容量: 8匹, 現在の動物: 9匹, 必要容量: +3匹 → 2匹不足
        
        LogPlayerStatus("複雑な容量不足シナリオ");
    }
    
    /// <summary>
    /// 自動削減処理のコルーチン
    /// </summary>
    private IEnumerator AutoReductionCoroutine()
    {
        // インタラクティブ繁殖を開始
        breedingManager.StartInteractiveBreeding(testPlayer);
        
        // 容量不足状態になるまで待機
        yield return new WaitForSeconds(testDelay);
        
        if (breedingManager.GetCurrentState() == BreedingState.PlayerChoice)
        {
            DebugLog("🤖 AI自動削減を実行中...");
            
            // 削減オプションを取得
            var options = breedingManager.GetReductionOptions();
            int requiredCapacity = breedingManager.GetRequiredCapacity();
            
            // 自動削減ロジック（簡単なアルゴリズム）
            var autoReduction = CalculateAutoReduction(options, requiredCapacity);
            
            // 削減を実行
            bool success = breedingManager.ExecutePlayerReduction(autoReduction);
            
            if (success)
            {
                DebugLog("✅ AI自動削減が成功しました");
            }
            else
            {
                DebugLog("❌ AI自動削減が失敗しました");
            }
        }
    }
    
    /// <summary>
    /// 自動削減の計算（簡単なアルゴリズム）
    /// </summary>
    private Dictionary<ResourceType, int> CalculateAutoReduction(List<AnimalReductionOption> options, int requiredCapacity)
    {
        var reduction = new Dictionary<ResourceType, int>();
        int remainingReduction = requiredCapacity;
        
        // 動物の価値順（低価値から削減）
        var animalPriority = new[] { ResourceType.Sheep, ResourceType.Boar, ResourceType.Cattle };
        
        foreach (var animalType in animalPriority)
        {
            if (remainingReduction <= 0) break;
            
            var option = options.Find(o => o.animalType == animalType);
            if (option != null)
            {
                int reductionAmount = Mathf.Min(remainingReduction, option.maxReduction);
                if (reductionAmount > 0)
                {
                    reduction[animalType] = reductionAmount;
                    remainingReduction -= reductionAmount;
                    
                    DebugLog($"🤖 AI削減: {GetAnimalName(animalType)} {reductionAmount}匹");
                }
            }
        }
        
        return reduction;
    }
    
    /// <summary>
    /// プレイヤーのリソースをクリア
    /// </summary>
    private void ClearPlayerResources()
    {
        // 動物をクリア
        var animalTypes = new[] { ResourceType.Sheep, ResourceType.Boar, ResourceType.Cattle };
        foreach (var animalType in animalTypes)
        {
            int count = testPlayer.GetResource(animalType);
            if (count > 0)
            {
                testPlayer.SpendResource(animalType, count);
            }
        }
        
        // 施設をリセット（簡易版）
        // 注意: 実際のゲームでは、より適切なリセット方法が必要
    }
    
    /// <summary>
    /// プレイヤーの状態をログ出力
    /// </summary>
    private void LogPlayerStatus(string scenario)
    {
        DebugLog($"--- {scenario} ---");
        DebugLog($"羊: {testPlayer.GetResource(ResourceType.Sheep)}匹");
        DebugLog($"牛: {testPlayer.GetResource(ResourceType.Cattle)}匹");
        DebugLog($"猪: {testPlayer.GetResource(ResourceType.Boar)}匹");
        
        int totalAnimals = testPlayer.GetResource(ResourceType.Sheep) + 
                          testPlayer.GetResource(ResourceType.Cattle) + 
                          testPlayer.GetResource(ResourceType.Boar);
        
        DebugLog($"総動物数: {totalAnimals}匹");
        DebugLog($"牧場数: {testPlayer.GetPastures()}");
        DebugLog($"小屋数: {testPlayer.GetStables()}");
        
        // 簡易容量計算
        int estimatedCapacity = testPlayer.GetPastures() * 2 + testPlayer.GetStables();
        DebugLog($"推定容量: {estimatedCapacity}匹");
        DebugLog($"容量不足: {Mathf.Max(0, totalAnimals - estimatedCapacity)}匹");
    }
    
    /// <summary>
    /// コンポーネントの妥当性チェック
    /// </summary>
    private bool ValidateComponents()
    {
        if (testPlayer == null)
        {
            DebugLog("❌ TestPlayerが設定されていません");
            return false;
        }
        
        if (breedingManager == null)
        {
            DebugLog("❌ AnimalBreedingManagerが見つかりません");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 動物名を取得
    /// </summary>
    private string GetAnimalName(ResourceType animalType)
    {
        switch (animalType)
        {
            case ResourceType.Sheep: return "羊";
            case ResourceType.Boar: return "猪";
            case ResourceType.Cattle: return "牛";
            default: return animalType.ToString();
        }
    }
    
    /// <summary>
    /// デバッグログ出力
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[InteractiveBreedingTest] {message}");
        }
    }
    
    /// <summary>
    /// 現在の繁殖状態を表示
    /// </summary>
    [ContextMenu("現在の繁殖状態を表示")]
    public void ShowCurrentBreedingStatus()
    {
        if (breedingManager != null)
        {
            breedingManager.ShowSystemStatus();
        }
        
        if (testPlayer != null)
        {
            LogPlayerStatus("現在の状態");
        }
    }
    
    /// <summary>
    /// 繁殖処理をキャンセル
    /// </summary>
    [ContextMenu("繁殖処理をキャンセル")]
    public void CancelCurrentBreeding()
    {
        if (breedingManager != null)
        {
            breedingManager.CancelBreeding();
        }
    }
}