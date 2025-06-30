using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    [Header("ゲーム情報")]
    public TextMeshProUGUI currentPlayerText;
    public TextMeshProUGUI currentRoundText;
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI turnPhaseText;
    
    [Header("プレイヤー情報")]
    public Transform playerInfoParent;
    public GameObject playerInfoPrefab;
    
    [Header("リソース表示")]
    public Transform resourceDisplayParent;
    public GameObject resourceDisplayPrefab;
    
    [Header("ボタン")]
    public Button endTurnButton;
    public Button restartGameButton;
    
    [Header("カード表示")]
    public Transform handDisplayParent;
    public GameObject cardDisplayPrefab;
    
    [Header("ゲーム終了")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    
    private GameManager gameManager;
    private List<PlayerInfoUI> playerInfoUIs = new List<PlayerInfoUI>();
    private List<ResourceDisplayUI> resourceDisplayUIs = new List<ResourceDisplayUI>();
    private List<CardDisplayUI> handCardUIs = new List<CardDisplayUI>();
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager != null)
        {
            // イベントに登録
            gameManager.OnGameStateChanged += UpdateGameStateDisplay;
            gameManager.OnTurnPhaseChanged += UpdateTurnPhaseDisplay;
            gameManager.OnPlayerTurnStart += UpdateCurrentPlayerDisplay;
            gameManager.OnRoundChanged += UpdateRoundDisplay;
            gameManager.OnGameWon += ShowGameOverScreen;
        }
        
        // ボタンイベント設定
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(EndTurn);
            
        if (restartGameButton != null)
            restartGameButton.onClick.AddListener(RestartGame);
        
        InitializeUI();
    }
    
    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= UpdateGameStateDisplay;
            gameManager.OnTurnPhaseChanged -= UpdateTurnPhaseDisplay;
            gameManager.OnPlayerTurnStart -= UpdateCurrentPlayerDisplay;
            gameManager.OnRoundChanged -= UpdateRoundDisplay;
            gameManager.OnGameWon -= ShowGameOverScreen;
        }
    }
    
    private void InitializeUI()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        CreatePlayerInfoUIs();
        CreateResourceDisplayUIs();
        UpdateAllDisplays();
    }
    
    private void CreatePlayerInfoUIs()
    {
        if (playerInfoParent == null || playerInfoPrefab == null || gameManager == null)
            return;
            
        // 既存のUIを削除
        foreach (PlayerInfoUI ui in playerInfoUIs)
        {
            if (ui != null)
                Destroy(ui.gameObject);
        }
        playerInfoUIs.Clear();
        
        // プレイヤー数分作成
        for (int i = 0; i < 4; i++) // 最大4プレイヤー
        {
            GameObject uiObj = Instantiate(playerInfoPrefab, playerInfoParent);
            PlayerInfoUI playerInfoUI = uiObj.GetComponent<PlayerInfoUI>();
            if (playerInfoUI == null)
                playerInfoUI = uiObj.AddComponent<PlayerInfoUI>();
                
            playerInfoUIs.Add(playerInfoUI);
            
            // 初期は非表示
            uiObj.SetActive(false);
        }
    }
    
    private void CreateResourceDisplayUIs()
    {
        if (resourceDisplayParent == null || resourceDisplayPrefab == null)
            return;
            
        // リソース種類分作成
        ResourceType[] resourceTypes = System.Enum.GetValues(typeof(ResourceType)) as ResourceType[];
        
        foreach (ResourceType resourceType in resourceTypes)
        {
            GameObject uiObj = Instantiate(resourceDisplayPrefab, resourceDisplayParent);
            ResourceDisplayUI resourceUI = uiObj.GetComponent<ResourceDisplayUI>();
            if (resourceUI == null)
                resourceUI = uiObj.AddComponent<ResourceDisplayUI>();
                
            resourceUI.SetResourceType(resourceType);
            resourceDisplayUIs.Add(resourceUI);
        }
    }
    
    private void UpdateAllDisplays()
    {
        UpdateGameStateDisplay(gameManager?.currentGameState ?? GameState.Setup);
        UpdateTurnPhaseDisplay(gameManager?.currentTurnPhase ?? TurnPhase.PlaceWorkers);
        UpdateCurrentPlayerDisplay(gameManager?.CurrentPlayer);
        UpdateRoundDisplay(gameManager?.currentRound ?? 1);
        UpdatePlayerInfos();
        UpdateResourceDisplay();
        UpdateHandDisplay();
    }
    
    private void UpdateGameStateDisplay(GameState gameState)
    {
        if (gameStateText != null)
        {
            string stateText = "";
            switch (gameState)
            {
                case GameState.Setup: stateText = "ゲーム準備中"; break;
                case GameState.PlayerTurn: stateText = "プレイヤーターン"; break;
                case GameState.EndOfRound: stateText = "ラウンド終了"; break;
                case GameState.GameOver: stateText = "ゲーム終了"; break;
            }
            gameStateText.text = $"状態: {stateText}";
        }
    }
    
    private void UpdateTurnPhaseDisplay(TurnPhase turnPhase)
    {
        if (turnPhaseText != null)
        {
            string phaseText = "";
            switch (turnPhase)
            {
                case TurnPhase.PlaceWorkers: phaseText = "ワーカー配置"; break;
                case TurnPhase.PlayCards: phaseText = "カードプレイ"; break;
                case TurnPhase.EndTurn: phaseText = "ターン終了"; break;
            }
            turnPhaseText.text = $"フェーズ: {phaseText}";
        }
    }
    
    private void UpdateCurrentPlayerDisplay(Player currentPlayer)
    {
        if (currentPlayerText != null)
        {
            string playerName = currentPlayer?.playerName ?? "なし";
            currentPlayerText.text = $"現在のプレイヤー: {playerName}";
        }
    }
    
    private void UpdateRoundDisplay(int round)
    {
        if (currentRoundText != null)
        {
            currentRoundText.text = $"ラウンド: {round}";
        }
    }
    
    private void UpdatePlayerInfos()
    {
        if (gameManager == null) return;
        
        // 実装される予定のプレイヤー情報更新
    }
    
    private void UpdateResourceDisplay()
    {
        if (gameManager?.CurrentPlayer == null) return;
        
        Player currentPlayer = gameManager.CurrentPlayer;
        
        foreach (ResourceDisplayUI resourceUI in resourceDisplayUIs)
        {
            int amount = currentPlayer.GetResource(resourceUI.ResourceType);
            resourceUI.UpdateDisplay(amount);
        }
    }
    
    private void UpdateHandDisplay()
    {
        if (gameManager?.CurrentPlayer == null || handDisplayParent == null) return;
        
        // 既存の手札UIを削除
        foreach (CardDisplayUI cardUI in handCardUIs)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }
        handCardUIs.Clear();
        
        // 現在のプレイヤーの手札を表示
        List<Card> hand = gameManager.CurrentPlayer.GetHand();
        
        foreach (Card card in hand)
        {
            if (cardDisplayPrefab != null)
            {
                GameObject cardObj = Instantiate(cardDisplayPrefab, handDisplayParent);
                CardDisplayUI cardUI = cardObj.GetComponent<CardDisplayUI>();
                if (cardUI == null)
                    cardUI = cardObj.AddComponent<CardDisplayUI>();
                    
                cardUI.SetCard(card);
                handCardUIs.Add(cardUI);
            }
        }
    }
    
    private void ShowGameOverScreen(Player winner)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (winnerText != null)
            {
                winnerText.text = $"勝者: {winner.playerName}\n勝利点: {winner.GetVictoryPoints()}";
            }
        }
    }
    
    public void EndTurn()
    {
        if (gameManager != null)
        {
            gameManager.EndPlayerTurn();
        }
    }
    
    public void RestartGame()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // リアルタイム更新が必要な場合
        if (gameManager?.CurrentPlayer != null)
        {
            UpdateResourceDisplay();
        }
    }
}

// プレイヤー情報UI用の簡単なクラス
public class PlayerInfoUI : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI victoryPointsText;
    public TextMeshProUGUI workersText;
    
    public void UpdatePlayerInfo(Player player)
    {
        if (player == null) return;
        
        if (playerNameText != null)
            playerNameText.text = player.playerName;
            
        if (victoryPointsText != null)
            victoryPointsText.text = $"VP: {player.GetVictoryPoints()}";
            
        if (workersText != null)
            workersText.text = $"ワーカー: {player.GetAvailableWorkers()}";
    }
}

// リソース表示UI用のクラス
public class ResourceDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI resourceNameText;
    public TextMeshProUGUI resourceAmountText;
    public Image resourceIcon;
    
    public ResourceType ResourceType { get; private set; }
    
    public void SetResourceType(ResourceType resourceType)
    {
        ResourceType = resourceType;
        
        if (resourceNameText != null)
        {
            resourceNameText.text = GetResourceName(resourceType);
        }
    }
    
    public void UpdateDisplay(int amount)
    {
        if (resourceAmountText != null)
        {
            resourceAmountText.text = amount.ToString();
        }
    }
    
    private string GetResourceName(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood: return "木材";
            case ResourceType.Stone: return "石材";
            case ResourceType.Food: return "食料";
            case ResourceType.Gold: return "金貨";
            case ResourceType.Workers: return "ワーカー";
            default: return resourceType.ToString();
        }
    }
}

// カード表示UI用のクラス
public class CardDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescriptionText;
    public Image cardArtImage;
    public Button cardButton;
    
    private Card currentCard;
    
    void Start()
    {
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    public void SetCard(Card card)
    {
        currentCard = card;
        
        if (cardNameText != null)
            cardNameText.text = card.cardName;
            
        if (cardDescriptionText != null)
            cardDescriptionText.text = card.description;
            
        if (cardArtImage != null && card.cardArt != null)
            cardArtImage.sprite = card.cardArt;
    }
    
    private void OnCardClicked()
    {
        if (currentCard != null)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager?.CurrentPlayer != null)
            {
                gameManager.CurrentPlayer.PlayCard(currentCard);
            }
        }
    }
}