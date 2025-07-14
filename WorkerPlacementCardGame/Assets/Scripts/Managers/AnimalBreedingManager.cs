using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 繁殖可能な動物の情報
/// </summary>
[System.Serializable]
public class BreedableAnimal
{
    public ResourceType animalType;
    public int currentCount;
    public bool canBreed;
    public bool hasCapacity;
    
    public BreedableAnimal(ResourceType type, int count, bool breedable, bool capacity)
    {
        animalType = type;
        currentCount = count;
        canBreed = breedable;
        hasCapacity = capacity;
    }
}

/// <summary>
/// 動物削減の選択肢
/// </summary>
[System.Serializable]
public class AnimalReductionOption
{
    public ResourceType animalType;
    public int currentCount;
    public int maxReduction;
    
    public AnimalReductionOption(ResourceType type, int current, int max)
    {
        animalType = type;
        currentCount = current;
        maxReduction = max;
    }
}

/// <summary>
/// 繁殖処理の状態
/// </summary>
public enum BreedingState
{
    NotStarted,     // 開始前
    CapacityCheck,  // 容量チェック中
    PlayerChoice,   // プレイヤー選択待ち
    Processing,     // 繁殖処理中
    Completed       // 完了
}

/// <summary>
/// インタラクティブな動物繁殖管理システム
/// </summary>
public class AnimalBreedingManager : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private bool enableInteractiveBreeding = true;
    [SerializeField] private bool enableDebugLog = true;
    
    [Header("現在の処理状態")]
    [SerializeField] private BreedingState currentState = BreedingState.NotStarted;
    [SerializeField] private Player currentPlayer;
    
    // イベントシステム
    public System.Action<Player, List<BreedableAnimal>> OnBreedingAnalysisComplete;
    public System.Action<Player, List<AnimalReductionOption>, int> OnCapacityShortage;
    public System.Action<Player, List<BreedableAnimal>> OnBreedingComplete;
    public System.Action<Player> OnBreedingCancelled;
    
    // 一時的な繁殖データ
    private List<BreedableAnimal> pendingBreeding = new List<BreedableAnimal>();
    private List<AnimalReductionOption> reductionOptions = new List<AnimalReductionOption>();
    private int requiredCapacity = 0;
    
    /// <summary>
    /// インタラクティブな繁殖処理を開始
    /// </summary>
    public void StartInteractiveBreeding(Player player)
    {
        if (currentState != BreedingState.NotStarted)
        {
            DebugLog($"⚠️ 繁殖処理が既に進行中です（状態: {currentState}）");
            return;
        }
        
        currentPlayer = player;
        currentState = BreedingState.CapacityCheck;
        
        DebugLog($"🐑=== {player.playerName}のインタラクティブ繁殖開始 ===");
        
        // 繁殖可能な動物を分析
        AnalyzeBreedingPossibilities();
    }
    
    /// <summary>
    /// 繁殖可能性の分析
    /// </summary>
    private void AnalyzeBreedingPossibilities()
    {
        pendingBreeding.Clear();
        
        var animalTypes = new[] { ResourceType.Sheep, ResourceType.Boar, ResourceType.Cattle };
        int totalCapacityNeeded = 0;
        
        foreach (var animalType in animalTypes)
        {
            int count = currentPlayer.GetResource(animalType);
            bool canBreed = count >= 2;
            bool hasCapacity = currentPlayer.CanHouseAnimals(animalType, 1);
            
            if (canBreed)
            {
                totalCapacityNeeded++;
                pendingBreeding.Add(new BreedableAnimal(animalType, count, canBreed, hasCapacity));
                
                DebugLog($"  {GetAnimalName(animalType)}: {count}匹 → 繁殖可能 (容量: {(hasCapacity ? "OK" : "不足")})");
            }
        }
        
        DebugLog($"📊 繁殖分析結果: {pendingBreeding.Count}種類が繁殖可能, 必要容量: {totalCapacityNeeded}");
        
        // イベント発火
        OnBreedingAnalysisComplete?.Invoke(currentPlayer, new List<BreedableAnimal>(pendingBreeding));
        
        // 容量チェック
        CheckCapacityAndProceed(totalCapacityNeeded);
    }
    
    /// <summary>
    /// 容量チェックと処理続行
    /// </summary>
    private void CheckCapacityAndProceed(int totalCapacityNeeded)
    {
        // 容量不足の動物をチェック
        var capacityShortages = pendingBreeding.Where(animal => !animal.hasCapacity).ToList();
        
        if (capacityShortages.Count == 0)
        {
            // 容量に問題なし、直接繁殖実行
            DebugLog("✅ 容量に問題なし、繁殖を実行します");
            ExecuteBreeding();
        }
        else
        {
            // 容量不足、プレイヤーに選択を求める
            int shortageCount = capacityShortages.Count;
            DebugLog($"❌ 容量不足: {shortageCount}種類の動物が繁殖できません");
            
            RequestPlayerChoice(shortageCount);
        }
    }
    
    /// <summary>
    /// プレイヤーに動物削減の選択を求める
    /// </summary>
    private void RequestPlayerChoice(int capacityShortage)
    {
        currentState = BreedingState.PlayerChoice;
        requiredCapacity = capacityShortage;
        
        // 削減可能な動物のオプションを作成
        reductionOptions.Clear();
        var animalTypes = new[] { ResourceType.Sheep, ResourceType.Boar, ResourceType.Cattle };
        
        foreach (var animalType in animalTypes)
        {
            int count = currentPlayer.GetResource(animalType);
            if (count > 0)
            {
                // 繁殖予定の動物は最低2匹残す必要がある
                bool isBreeding = pendingBreeding.Any(b => b.animalType == animalType);
                int minKeep = isBreeding ? 2 : 0;
                int maxReduction = Mathf.Max(0, count - minKeep);
                
                if (maxReduction > 0)
                {
                    reductionOptions.Add(new AnimalReductionOption(animalType, count, maxReduction));
                }
            }
        }
        
        DebugLog($"🤔 プレイヤー選択待ち: {requiredCapacity}匹分の容量確保が必要");
        DebugLog("削減可能な動物:");
        foreach (var option in reductionOptions)
        {
            DebugLog($"  {GetAnimalName(option.animalType)}: {option.currentCount}匹（最大{option.maxReduction}匹削減可能）");
        }
        
        // イベント発火（UI側で処理）
        OnCapacityShortage?.Invoke(currentPlayer, new List<AnimalReductionOption>(reductionOptions), requiredCapacity);
    }
    
    /// <summary>
    /// プレイヤーの動物削減選択を実行
    /// </summary>
    public bool ExecutePlayerReduction(Dictionary<ResourceType, int> reductions)
    {
        if (currentState != BreedingState.PlayerChoice)
        {
            DebugLog("❌ プレイヤー選択状態ではありません");
            return false;
        }
        
        int totalReduction = reductions.Values.Sum();
        
        // 削減量が必要容量を満たしているかチェック
        if (totalReduction < requiredCapacity)
        {
            DebugLog($"❌ 削減量不足: {totalReduction}匹（必要: {requiredCapacity}匹）");
            return false;
        }
        
        // 削減可能かチェック
        foreach (var reduction in reductions)
        {
            var option = reductionOptions.FirstOrDefault(o => o.animalType == reduction.Key);
            if (option == null || reduction.Value > option.maxReduction)
            {
                DebugLog($"❌ {GetAnimalName(reduction.Key)}の削減量が無効: {reduction.Value}匹（最大: {option?.maxReduction ?? 0}匹）");
                return false;
            }
        }
        
        // 削減実行
        DebugLog($"🔧 動物削減を実行: 合計{totalReduction}匹");
        foreach (var reduction in reductions)
        {
            if (reduction.Value > 0)
            {
                currentPlayer.SpendResource(reduction.Key, reduction.Value);
                DebugLog($"  {GetAnimalName(reduction.Key)}: {reduction.Value}匹削減");
            }
        }
        
        // 容量再チェックして繁殖実行
        DebugLog("✅ 容量確保完了、繁殖を実行します");
        ExecuteBreeding();
        
        return true;
    }
    
    /// <summary>
    /// 繁殖処理の実行
    /// </summary>
    private void ExecuteBreeding()
    {
        currentState = BreedingState.Processing;
        
        List<BreedableAnimal> successfulBreeding = new List<BreedableAnimal>();
        
        foreach (var animal in pendingBreeding)
        {
            // 最新の状態で再チェック
            int currentCount = currentPlayer.GetResource(animal.animalType);
            bool canStillBreed = currentCount >= 2;
            bool hasCapacityNow = currentPlayer.CanHouseAnimals(animal.animalType, 1);
            
            if (canStillBreed && hasCapacityNow)
            {
                // 繁殖実行
                currentPlayer.ReceiveResourceDirect(animal.animalType, 1, null, "breeding");
                
                // 結果を記録
                animal.currentCount = currentCount + 1;
                successfulBreeding.Add(animal);
                
                DebugLog($"✅ {GetAnimalName(animal.animalType)}: {currentCount}匹 → {currentCount + 1}匹（繁殖成功）");
            }
            else
            {
                DebugLog($"❌ {GetAnimalName(animal.animalType)}: 繁殖条件を満たさなくなりました");
            }
        }
        
        // 職業効果のトリガー
        currentPlayer.TriggerOccupationEffects(OccupationTrigger.OnBreeding);
        
        DebugLog($"🎉 繁殖完了: {successfulBreeding.Count}種類の動物が繁殖しました");
        
        // イベント発火
        OnBreedingComplete?.Invoke(currentPlayer, successfulBreeding);
        
        // 状態リセット
        CompleteBreeding();
    }
    
    /// <summary>
    /// 繁殖処理をキャンセル
    /// </summary>
    public void CancelBreeding()
    {
        if (currentState == BreedingState.NotStarted || currentState == BreedingState.Completed)
        {
            return;
        }
        
        DebugLog($"❌ {currentPlayer.playerName}の繁殖処理をキャンセルしました");
        
        OnBreedingCancelled?.Invoke(currentPlayer);
        
        ResetBreedingState();
    }
    
    /// <summary>
    /// 繁殖処理完了
    /// </summary>
    private void CompleteBreeding()
    {
        currentState = BreedingState.Completed;
        
        // 少し待ってから状態リセット
        Invoke(nameof(ResetBreedingState), 0.1f);
    }
    
    /// <summary>
    /// 繁殖状態をリセット
    /// </summary>
    private void ResetBreedingState()
    {
        currentState = BreedingState.NotStarted;
        currentPlayer = null;
        pendingBreeding.Clear();
        reductionOptions.Clear();
        requiredCapacity = 0;
    }
    
    /// <summary>
    /// 従来の繁殖処理（後方互換性のため）
    /// </summary>
    public void ExecuteTraditionalBreeding(Player player)
    {
        DebugLog($"🔄 従来方式の繁殖処理: {player.playerName}");
        
        // 各動物種で2匹以上いれば1匹増える（容量チェック付き）
        if (player.GetResource(ResourceType.Sheep) >= 2 && player.CanHouseAnimals(ResourceType.Sheep, 1))
        {
            player.ReceiveResourceDirect(ResourceType.Sheep, 1, null, "breeding");
            DebugLog($"  羊が繁殖しました");
        }
            
        if (player.GetResource(ResourceType.Boar) >= 2 && player.CanHouseAnimals(ResourceType.Boar, 1))
        {
            player.ReceiveResourceDirect(ResourceType.Boar, 1, null, "breeding");
            DebugLog($"  猪が繁殖しました");
        }
            
        if (player.GetResource(ResourceType.Cattle) >= 2 && player.CanHouseAnimals(ResourceType.Cattle, 1))
        {
            player.ReceiveResourceDirect(ResourceType.Cattle, 1, null, "breeding");
            DebugLog($"  牛が繁殖しました");
        }
        
        // 職業効果のトリガー
        player.TriggerOccupationEffects(OccupationTrigger.OnBreeding);
    }
    
    /// <summary>
    /// 現在の処理状態を取得
    /// </summary>
    public BreedingState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 現在処理中のプレイヤーを取得
    /// </summary>
    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }
    
    /// <summary>
    /// 削減オプションを取得
    /// </summary>
    public List<AnimalReductionOption> GetReductionOptions()
    {
        return new List<AnimalReductionOption>(reductionOptions);
    }
    
    /// <summary>
    /// 必要容量を取得
    /// </summary>
    public int GetRequiredCapacity()
    {
        return requiredCapacity;
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
            Debug.Log($"[AnimalBreedingManager] {message}");
        }
    }
    
    /// <summary>
    /// システム状態を表示
    /// </summary>
    [ContextMenu("システム状態表示")]
    public void ShowSystemStatus()
    {
        Debug.Log("=== AnimalBreedingManager 状態 ===");
        Debug.Log($"現在の状態: {currentState}");
        Debug.Log($"処理中プレイヤー: {(currentPlayer != null ? currentPlayer.playerName : "なし")}");
        Debug.Log($"インタラクティブモード: {(enableInteractiveBreeding ? "有効" : "無効")}");
        Debug.Log($"繁殖予定: {pendingBreeding.Count}種類");
        Debug.Log($"削減オプション: {reductionOptions.Count}種類");
        Debug.Log($"必要容量: {requiredCapacity}");
    }
}