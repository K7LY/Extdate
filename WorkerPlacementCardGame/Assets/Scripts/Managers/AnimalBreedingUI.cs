using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// 動物繁殖UIの管理クラス
/// </summary>
public class AnimalBreedingUI : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject breedingPanel;
    [SerializeField] private GameObject analysisPanel;
    [SerializeField] private GameObject reductionPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI analysisText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button executeButton;
    [SerializeField] private Button cancelButton;
    
    [Header("動物削減UI")]
    [SerializeField] private Transform reductionOptionsParent;
    [SerializeField] private GameObject animalReductionPrefab;
    
    [Header("設定")]
    [SerializeField] private bool enableDebugLog = true;
    
    // 現在の状態
    private AnimalBreedingManager breedingManager;
    private Player currentPlayer;
    private List<AnimalReductionOption> currentOptions;
    private Dictionary<ResourceType, int> playerSelections = new Dictionary<ResourceType, int>();
    private List<AnimalReductionUIItem> reductionUIItems = new List<AnimalReductionUIItem>();
    
    void Start()
    {
        // AnimalBreedingManagerを見つけて接続
        breedingManager = FindObjectOfType<AnimalBreedingManager>();
        if (breedingManager != null)
        {
            // イベント購読
            breedingManager.OnBreedingAnalysisComplete += OnBreedingAnalysisComplete;
            breedingManager.OnCapacityShortage += OnCapacityShortage;
            breedingManager.OnBreedingComplete += OnBreedingComplete;
            breedingManager.OnBreedingCancelled += OnBreedingCancelled;
        }
        
        // ボタンイベント設定
        if (executeButton != null)
            executeButton.onClick.AddListener(OnExecuteButtonClicked);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        
        // 初期状態では非表示
        SetPanelVisibility(false);
    }
    
    void OnDestroy()
    {
        // イベント購読解除
        if (breedingManager != null)
        {
            breedingManager.OnBreedingAnalysisComplete -= OnBreedingAnalysisComplete;
            breedingManager.OnCapacityShortage -= OnCapacityShortage;
            breedingManager.OnBreedingComplete -= OnBreedingComplete;
            breedingManager.OnBreedingCancelled -= OnBreedingCancelled;
        }
    }
    
    /// <summary>
    /// 繁殖分析完了時の処理
    /// </summary>
    private void OnBreedingAnalysisComplete(Player player, List<BreedableAnimal> breedableAnimals)
    {
        currentPlayer = player;
        
        DebugLog($"繁殖分析完了: {player.playerName}");
        
        SetPanelVisibility(true);
        analysisPanel.SetActive(true);
        reductionPanel.SetActive(false);
        
        if (titleText != null)
            titleText.text = $"{player.playerName}の動物繁殖";
        
        // 分析結果を表示
        string analysisResult = "繁殖可能な動物:\n";
        foreach (var animal in breedableAnimals)
        {
            string status = animal.hasCapacity ? "✅" : "❌ 容量不足";
            analysisResult += $"• {GetAnimalName(animal.animalType)}: {animal.currentCount}匹 {status}\n";
        }
        
        if (analysisText != null)
            analysisText.text = analysisResult;
        
        // 容量に問題がなければ自動実行ボタンを表示
        if (breedableAnimals.All(a => a.hasCapacity))
        {
            if (instructionText != null)
                instructionText.text = "容量に問題ありません。繁殖を実行しますか？";
            if (executeButton != null)
                executeButton.interactable = true;
        }
    }
    
    /// <summary>
    /// 容量不足時の処理
    /// </summary>
    private void OnCapacityShortage(Player player, List<AnimalReductionOption> options, int requiredCapacity)
    {
        currentPlayer = player;
        currentOptions = options;
        playerSelections.Clear();
        
        DebugLog($"容量不足: {player.playerName}, 必要容量: {requiredCapacity}");
        
        // UIパネル切り替え
        analysisPanel.SetActive(false);
        reductionPanel.SetActive(true);
        
        if (titleText != null)
            titleText.text = $"{player.playerName}の動物削減選択";
        
        if (instructionText != null)
            instructionText.text = $"容量が{requiredCapacity}匹分不足しています。\n削減する動物を選択してください:";
        
        // 削減オプションUIを作成
        CreateReductionOptionsUI(options, requiredCapacity);
        
        // 実行ボタンを無効化（選択完了まで）
        if (executeButton != null)
            executeButton.interactable = false;
    }
    
    /// <summary>
    /// 削減オプションUIを作成
    /// </summary>
    private void CreateReductionOptionsUI(List<AnimalReductionOption> options, int requiredCapacity)
    {
        // 既存のUIアイテムをクリア
        ClearReductionUI();
        
        foreach (var option in options)
        {
            // プレハブからUIアイテムを作成
            GameObject itemObj = Instantiate(animalReductionPrefab, reductionOptionsParent);
            AnimalReductionUIItem uiItem = itemObj.GetComponent<AnimalReductionUIItem>();
            
            if (uiItem != null)
            {
                // UIアイテムを初期化
                uiItem.Initialize(option, OnReductionSelectionChanged);
                reductionUIItems.Add(uiItem);
                
                // 初期選択量は0
                playerSelections[option.animalType] = 0;
            }
        }
    }
    
    /// <summary>
    /// 削減選択が変更された時の処理
    /// </summary>
    private void OnReductionSelectionChanged(ResourceType animalType, int amount)
    {
        playerSelections[animalType] = amount;
        
        // 必要容量を満たしているかチェック
        int totalReduction = playerSelections.Values.Sum();
        int requiredCapacity = breedingManager.GetRequiredCapacity();
        
        bool canExecute = totalReduction >= requiredCapacity;
        
        if (executeButton != null)
            executeButton.interactable = canExecute;
        
        // 状況をテキストで表示
        if (instructionText != null)
        {
            string statusText = $"必要容量: {requiredCapacity}匹\n";
            statusText += $"現在の削減量: {totalReduction}匹\n";
            
            if (canExecute)
            {
                statusText += "✅ 削減完了！実行ボタンを押してください";
            }
            else
            {
                statusText += $"❌ あと{requiredCapacity - totalReduction}匹削減してください";
            }
            
            instructionText.text = statusText;
        }
        
        DebugLog($"削減選択変更: 合計{totalReduction}匹 (必要: {requiredCapacity}匹)");
    }
    
    /// <summary>
    /// 実行ボタンクリック時の処理
    /// </summary>
    private void OnExecuteButtonClicked()
    {
        if (breedingManager == null)
        {
            DebugLog("❌ AnimalBreedingManagerが見つかりません");
            return;
        }
        
        var state = breedingManager.GetCurrentState();
        
        if (state == BreedingState.PlayerChoice)
        {
            // プレイヤーの削減選択を実行
            bool success = breedingManager.ExecutePlayerReduction(new Dictionary<ResourceType, int>(playerSelections));
            
            if (!success)
            {
                DebugLog("❌ 削減実行に失敗しました");
                return;
            }
        }
        else if (state == BreedingState.CapacityCheck)
        {
            // 容量に問題ない場合の直接実行
            // この場合は実際には既にExecuteBreeding()が呼ばれているはず
        }
    }
    
    /// <summary>
    /// キャンセルボタンクリック時の処理
    /// </summary>
    private void OnCancelButtonClicked()
    {
        if (breedingManager != null)
        {
            breedingManager.CancelBreeding();
        }
    }
    
    /// <summary>
    /// 繁殖完了時の処理
    /// </summary>
    private void OnBreedingComplete(Player player, List<BreedableAnimal> successfulBreeding)
    {
        DebugLog($"繁殖完了: {player.playerName}, {successfulBreeding.Count}種類成功");
        
        // 結果を表示
        string resultText = "繁殖結果:\n";
        foreach (var animal in successfulBreeding)
        {
            resultText += $"✅ {GetAnimalName(animal.animalType)}: +1匹 (合計{animal.currentCount}匹)\n";
        }
        
        if (analysisText != null)
            analysisText.text = resultText;
        
        // 少し待ってからパネルを非表示
        Invoke(nameof(HidePanel), 2.0f);
    }
    
    /// <summary>
    /// 繁殖キャンセル時の処理
    /// </summary>
    private void OnBreedingCancelled(Player player)
    {
        DebugLog($"繁殖キャンセル: {player.playerName}");
        HidePanel();
    }
    
    /// <summary>
    /// パネルを非表示
    /// </summary>
    private void HidePanel()
    {
        SetPanelVisibility(false);
        ClearReductionUI();
        currentPlayer = null;
        currentOptions = null;
        playerSelections.Clear();
    }
    
    /// <summary>
    /// 削減UIをクリア
    /// </summary>
    private void ClearReductionUI()
    {
        foreach (var item in reductionUIItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        reductionUIItems.Clear();
    }
    
    /// <summary>
    /// パネルの表示/非表示を設定
    /// </summary>
    private void SetPanelVisibility(bool visible)
    {
        if (breedingPanel != null)
            breedingPanel.SetActive(visible);
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
            Debug.Log($"[AnimalBreedingUI] {message}");
        }
    }
    
    /// <summary>
    /// 手動でインタラクティブ繁殖を開始（テスト用）
    /// </summary>
    [ContextMenu("テスト: インタラクティブ繁殖開始")]
    public void TestStartInteractiveBreeding()
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null && gameManager.CurrentPlayer != null)
        {
            if (breedingManager != null)
            {
                breedingManager.StartInteractiveBreeding(gameManager.CurrentPlayer);
            }
        }
        else
        {
            Debug.LogWarning("GameManagerまたは現在のプレイヤーが見つかりません");
        }
    }
}

/// <summary>
/// 個別の動物削減UI項目
/// </summary>
public class AnimalReductionUIItem : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI animalNameText;
    [SerializeField] private TextMeshProUGUI currentCountText;
    [SerializeField] private TextMeshProUGUI maxReductionText;
    [SerializeField] private Slider reductionSlider;
    [SerializeField] private TextMeshProUGUI selectedAmountText;
    
    private AnimalReductionOption option;
    private System.Action<ResourceType, int> onSelectionChanged;
    
    /// <summary>
    /// UIアイテムを初期化
    /// </summary>
    public void Initialize(AnimalReductionOption animalOption, System.Action<ResourceType, int> callback)
    {
        option = animalOption;
        onSelectionChanged = callback;
        
        // UI要素を設定
        if (animalNameText != null)
            animalNameText.text = GetAnimalName(option.animalType);
        
        if (currentCountText != null)
            currentCountText.text = $"現在: {option.currentCount}匹";
        
        if (maxReductionText != null)
            maxReductionText.text = $"最大削減: {option.maxReduction}匹";
        
        if (reductionSlider != null)
        {
            reductionSlider.minValue = 0;
            reductionSlider.maxValue = option.maxReduction;
            reductionSlider.value = 0;
            reductionSlider.wholeNumbers = true;
            reductionSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        
        UpdateSelectedAmountText(0);
    }
    
    /// <summary>
    /// スライダー値変更時の処理
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        int amount = Mathf.RoundToInt(value);
        UpdateSelectedAmountText(amount);
        onSelectionChanged?.Invoke(option.animalType, amount);
    }
    
    /// <summary>
    /// 選択量テキストを更新
    /// </summary>
    private void UpdateSelectedAmountText(int amount)
    {
        if (selectedAmountText != null)
        {
            selectedAmountText.text = $"削減: {amount}匹";
        }
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
}