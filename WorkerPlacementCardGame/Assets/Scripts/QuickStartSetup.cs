using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unity上でワンクリックでゲームをテストできるクイックスタートスクリプト
/// </summary>
public class QuickStartSetup : MonoBehaviour
{
    [Header("自動セットアップ")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("UI設定")]
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private GameManager gameManager;
    
    [Header("テスト用リソース")]
    [SerializeField] private GameObject uiPrefabParent;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupQuickStart();
        }
    }
    
    [ContextMenu("Quick Start Setup")]
    public void SetupQuickStart()
    {
        Debug.Log("🚀 QuickStart Setup を開始します...");
        
        // 1. Canvas の設定
        EnsureCanvas();
        
        // 2. GameManager の設定
        EnsureGameManager();
        
        // 3. GameUI の設定
        EnsureGameUI();
        
        // 4. 基本的なUI要素の作成
        CreateBasicUIElements();
        
        Debug.Log("✅ QuickStart Setup 完了！");
    }
    
    private void EnsureCanvas()
    {
        if (gameCanvas == null)
        {
            gameCanvas = FindObjectOfType<Canvas>();
            
            if (gameCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                gameCanvas = canvasObj.AddComponent<Canvas>();
                gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                // Canvas Scaler を追加
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                // Graphic Raycaster を追加
                canvasObj.AddComponent<GraphicRaycaster>();
                
                Debug.Log("✅ Canvas を作成しました");
            }
        }
    }
    
    private void EnsureGameManager()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            
            if (gameManager == null)
            {
                GameObject managerObj = new GameObject("GameManager");
                gameManager = managerObj.AddComponent<GameManager>();
                Debug.Log("✅ GameManager を作成しました");
            }
        }
    }
    
    private void EnsureGameUI()
    {
        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
            
            if (gameUI == null)
            {
                GameObject uiObj = new GameObject("GameUI");
                uiObj.transform.SetParent(gameCanvas.transform, false);
                gameUI = uiObj.AddComponent<GameUI>();
                Debug.Log("✅ GameUI を作成しました");
            }
        }
    }
    
    private void CreateBasicUIElements()
    {
        if (uiPrefabParent == null)
        {
            uiPrefabParent = new GameObject("UI_Elements");
            uiPrefabParent.transform.SetParent(gameCanvas.transform, false);
        }
        
        // ゲーム情報パネル
        CreateGameInfoPanel();
        
        // プレイヤー情報表示エリア
        CreatePlayerInfoArea();
        
        // リソース表示エリア
        CreateResourceDisplayArea();
        
        // 手札表示エリア
        CreateHandDisplayArea();
        
        // ボタンエリア
        CreateButtonArea();
        
        // ゲーム終了パネル
        CreateGameOverPanel();
        
        // GameUIコンポーネントに参照を設定
        SetupGameUIReferences();
    }
    
    private void CreateGameInfoPanel()
    {
        GameObject infoPanel = CreatePanel("GameInfoPanel", new Vector2(400, 150));
        SetAnchor(infoPanel, AnchorPresets.TopLeft, new Vector2(10, -10));
        
        gameUI.currentPlayerText = CreateText(infoPanel, "CurrentPlayerText", "現在のプレイヤー: ");
        gameUI.currentRoundText = CreateText(infoPanel, "CurrentRoundText", "ラウンド: 1");
        gameUI.gameStateText = CreateText(infoPanel, "GameStateText", "状態: ゲーム準備中");
        gameUI.turnPhaseText = CreateText(infoPanel, "TurnPhaseText", "フェーズ: ワーカー配置");
        
        SetupVerticalLayout(infoPanel);
    }
    
    private void CreatePlayerInfoArea()
    {
        GameObject playerArea = CreatePanel("PlayerInfoParent", new Vector2(800, 100));
        SetAnchor(playerArea, AnchorPresets.TopCenter, new Vector2(0, -10));
        SetupHorizontalLayout(playerArea);
        
        gameUI.playerInfoParent = playerArea.transform;
    }
    
    private void CreateResourceDisplayArea()
    {
        GameObject resourceArea = CreatePanel("ResourceDisplayParent", new Vector2(1000, 80));
        SetAnchor(resourceArea, AnchorPresets.BottomCenter, new Vector2(0, 100));
        SetupHorizontalLayout(resourceArea);
        
        gameUI.resourceDisplayParent = resourceArea.transform;
    }
    
    private void CreateHandDisplayArea()
    {
        GameObject handArea = CreatePanel("HandDisplayParent", new Vector2(1200, 150));
        SetAnchor(handArea, AnchorPresets.BottomCenter, new Vector2(0, 10));
        SetupHorizontalLayout(handArea);
        
        gameUI.handDisplayParent = handArea.transform;
    }
    
    private void CreateButtonArea()
    {
        GameObject buttonArea = CreatePanel("ButtonArea", new Vector2(200, 100));
        SetAnchor(buttonArea, AnchorPresets.TopRight, new Vector2(-10, -10));
        
        gameUI.endTurnButton = CreateButton(buttonArea, "EndTurnButton", "ターン終了");
        gameUI.restartGameButton = CreateButton(buttonArea, "RestartGameButton", "ゲーム再開");
        
        SetupVerticalLayout(buttonArea);
    }
    
    private void CreateGameOverPanel()
    {
        GameObject gameOverPanel = CreatePanel("GameOverPanel", new Vector2(400, 300));
        SetAnchor(gameOverPanel, AnchorPresets.MiddleCenter, Vector2.zero);
        
        gameUI.winnerText = CreateText(gameOverPanel, "WinnerText", "勝者: ");
        
        gameOverPanel.SetActive(false);
        gameUI.gameOverPanel = gameOverPanel;
    }
    
    private void SetupGameUIReferences()
    {
        // プレハブを作成（簡易版）
        if (gameUI.playerInfoPrefab == null)
        {
            gameUI.playerInfoPrefab = CreatePlayerInfoPrefab();
        }
        
        if (gameUI.resourceDisplayPrefab == null)
        {
            gameUI.resourceDisplayPrefab = CreateResourceDisplayPrefab();
        }
        
        if (gameUI.cardDisplayPrefab == null)
        {
            gameUI.cardDisplayPrefab = CreateCardDisplayPrefab();
        }
    }
    
    private GameObject CreatePlayerInfoPrefab()
    {
        GameObject prefab = CreatePanel("PlayerInfoPrefab", new Vector2(180, 80));
        
        TextMeshProUGUI nameText = CreateText(prefab, "PlayerNameText", "プレイヤー名");
        TextMeshProUGUI vpText = CreateText(prefab, "VictoryPointsText", "VP: 0");
        TextMeshProUGUI workerText = CreateText(prefab, "WorkersText", "ワーカー: 2");
        
        PlayerInfoUI playerInfoUI = prefab.AddComponent<PlayerInfoUI>();
        playerInfoUI.playerNameText = nameText;
        playerInfoUI.victoryPointsText = vpText;
        playerInfoUI.workersText = workerText;
        
        SetupVerticalLayout(prefab);
        
        return prefab;
    }
    
    private GameObject CreateResourceDisplayPrefab()
    {
        GameObject prefab = CreatePanel("ResourceDisplayPrefab", new Vector2(80, 60));
        
        TextMeshProUGUI nameText = CreateText(prefab, "ResourceNameText", "リソース");
        TextMeshProUGUI amountText = CreateText(prefab, "ResourceAmountText", "0");
        
        ResourceDisplayUI resourceUI = prefab.AddComponent<ResourceDisplayUI>();
        resourceUI.resourceNameText = nameText;
        resourceUI.resourceAmountText = amountText;
        
        SetupVerticalLayout(prefab);
        
        return prefab;
    }
    
    private GameObject CreateCardDisplayPrefab()
    {
        GameObject prefab = CreatePanel("CardDisplayPrefab", new Vector2(120, 180));
        
        TextMeshProUGUI nameText = CreateText(prefab, "CardNameText", "カード名");
        TextMeshProUGUI descText = CreateText(prefab, "CardDescriptionText", "説明");
        Button cardButton = CreateButton(prefab, "CardButton", "使用");
        
        CardDisplayUI cardUI = prefab.AddComponent<CardDisplayUI>();
        cardUI.cardNameText = nameText;
        cardUI.cardDescriptionText = descText;
        cardUI.cardButton = cardButton;
        
        SetupVerticalLayout(prefab);
        
        return prefab;
    }
    
    // ヘルパーメソッド
    private GameObject CreatePanel(string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(uiPrefabParent.transform, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        
        return panel;
    }
    
    private TextMeshProUGUI CreateText(GameObject parent, string name, string text)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.fontSize = 14;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        return textComponent;
    }
    
    private Button CreateButton(GameObject parent, string name, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.4f, 0.4f, 0.8f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        TextMeshProUGUI buttonText = CreateText(buttonObj, "Text", text);
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    private void SetupVerticalLayout(GameObject obj)
    {
        VerticalLayoutGroup layout = obj.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.spacing = 5;
        
        ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    
    private void SetupHorizontalLayout(GameObject obj)
    {
        HorizontalLayoutGroup layout = obj.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandWidth = false;
        layout.childControlWidth = true;
        layout.spacing = 10;
        
        ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    
    private void SetAnchor(GameObject obj, AnchorPresets preset, Vector2 offset)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        
        switch (preset)
        {
            case AnchorPresets.TopLeft:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                break;
            case AnchorPresets.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 1);
                break;
            case AnchorPresets.TopRight:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
                break;
            case AnchorPresets.MiddleCenter:
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
            case AnchorPresets.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.pivot = new Vector2(0.5f, 0);
                break;
        }
        
        rect.anchoredPosition = offset;
    }
    
    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleCenter,
        BottomCenter
    }
}