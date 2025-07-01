using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum GameState
{
    Setup,
    PlayerTurn,
    EndOfRound,
    GameOver
}

public enum TurnPhase
{
    PlaceWorkers,
    PlayCards,
    EndTurn
}

public class GameManager : MonoBehaviour
{
    [Header("ゲーム設定（Agricola風）")]
    public int maxRounds = 14;
    public int maxPlayers = 4;
    public int startingHandSize = 7;
    
    [Header("収穫ラウンド")]
    public int[] harvestRounds = { 4, 7, 9, 11, 13, 14 };
    
    [Header("乞食カード")]
    [SerializeField] private Dictionary<Player, int> beggingCards = new Dictionary<Player, int>();
    
    [Header("ゲーム状態")]
    public GameState currentGameState = GameState.Setup;
    public TurnPhase currentTurnPhase = TurnPhase.PlaceWorkers;
    public int currentRound = 1;
    public int currentPlayerIndex = 0;
    
    [Header("プレイヤー")]
    [SerializeField] private List<Player> players = new List<Player>();
    
    [Header("ゲームボード")]
    [SerializeField] private List<ActionSpace> actionSpaces = new List<ActionSpace>();
    
    [Header("カードシステム")]
    public CardDeck mainDeck;
    
    [Header("システム管理")]
    private ActionSpaceManager actionSpaceManager;
    private ResourceConverter resourceConverter;
    
    // イベント
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<TurnPhase> OnTurnPhaseChanged;
    public System.Action<Player> OnPlayerTurnStart;
    public System.Action<int> OnRoundChanged;
    public System.Action<Player> OnGameWon;
    
    // 現在のプレイヤー
    public Player CurrentPlayer => players.Count > 0 ? players[currentPlayerIndex] : null;
    
    void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // システム管理者を初期化
        InitializeManagers();
        
        // プレイヤーを設定
        SetupPlayers();
        
        // アクションスペースを見つける
        actionSpaces = FindObjectsOfType<ActionSpace>().ToList();
        
        // デッキを初期化
        if (mainDeck != null)
        {
            mainDeck.InitializeDeck();
        }
        
        // ゲーム開始
        StartGame();
    }
    
    private void InitializeManagers()
    {
        // ActionSpaceManager を取得または作成
        actionSpaceManager = FindObjectOfType<ActionSpaceManager>();
        if (actionSpaceManager == null)
        {
            GameObject managerObj = new GameObject("ActionSpaceManager");
            actionSpaceManager = managerObj.AddComponent<ActionSpaceManager>();
        }
        
        // ResourceConverter を取得または作成
        resourceConverter = FindObjectOfType<ResourceConverter>();
        if (resourceConverter == null)
        {
            GameObject converterObj = new GameObject("ResourceConverter");
            resourceConverter = converterObj.AddComponent<ResourceConverter>();
        }
    }
    
    private void SetupPlayers()
    {
        Player[] foundPlayers = FindObjectsOfType<Player>();
        players = foundPlayers.ToList();
        
        // プレイヤーが見つからない場合、デフォルトプレイヤーを作成
        if (players.Count == 0)
        {
            CreateDefaultPlayers();
        }
        
        // 各プレイヤーの初期カードを配る
        foreach (Player player in players)
        {
            DealStartingCards(player);
        }
    }
    
    private void CreateDefaultPlayers()
    {
        // プレイヤー1
        GameObject player1Obj = new GameObject("Player 1");
        Player player1 = player1Obj.AddComponent<Player>();
        player1.playerName = "プレイヤー1";
        player1.playerColor = Color.blue;
        players.Add(player1);
        
        // プレイヤー2（AI）
        GameObject player2Obj = new GameObject("Player 2");
        Player player2 = player2Obj.AddComponent<Player>();
        player2.playerName = "AI";
        player2.playerColor = Color.red;
        player2.isAI = true;
        players.Add(player2);
    }
    
    private void DealStartingCards(Player player)
    {
        if (mainDeck != null)
        {
            for (int i = 0; i < startingHandSize; i++)
            {
                Card drawnCard = mainDeck.DrawCard();
                if (drawnCard != null)
                {
                    player.AddCardToHand(drawnCard);
                }
            }
        }
    }
    
    public void StartGame()
    {
        ChangeGameState(GameState.PlayerTurn);
        StartPlayerTurn();
    }
    
    public void StartPlayerTurn()
    {
        ChangeTurnPhase(TurnPhase.PlaceWorkers);
        
        // 新ラウンドの場合、アクションスペースを解放・補充
        if (currentPlayerIndex == 0)
        {
            if (actionSpaceManager != null)
            {
                actionSpaceManager.ActivateActionSpacesForRound(currentRound);
                actionSpaceManager.ReplenishActionSpaces();
            }
        }
        
        OnPlayerTurnStart?.Invoke(CurrentPlayer);
        
        // AIプレイヤーの場合、自動的にターンを実行
        if (CurrentPlayer.isAI)
        {
            StartCoroutine(ExecuteAITurn());
        }
    }
    
    private System.Collections.IEnumerator ExecuteAITurn()
    {
        yield return new WaitForSeconds(1f); // AIの思考時間をシミュレート
        
        // 簡単なAIロジック
        ExecuteSimpleAI();
        
        yield return new WaitForSeconds(0.5f);
        EndPlayerTurn();
    }
    
    private void ExecuteSimpleAI()
    {
        Player aiPlayer = CurrentPlayer;
        
        // 利用可能なアクションスペースにワーカーを配置
        List<ActionSpace> availableSpaces = actionSpaces.Where(space => space.CanPlaceWorker()).ToList();
        
        while (aiPlayer.GetAvailableWorkers() > 0 && availableSpaces.Count > 0)
        {
            ActionSpace randomSpace = availableSpaces[Random.Range(0, availableSpaces.Count)];
            if (aiPlayer.PlaceWorker(randomSpace))
            {
                availableSpaces.Remove(randomSpace);
            }
            else
            {
                break;
            }
        }
    }
    
    public void EndPlayerTurn()
    {
        CurrentPlayer.EndTurn();
        
        // 次のプレイヤーに移る
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        
        // 全プレイヤーが終了した場合、ラウンド終了
        if (currentPlayerIndex == 0)
        {
            EndRound();
        }
        else
        {
            StartPlayerTurn();
        }
    }
    
    private void EndRound()
    {
        ChangeGameState(GameState.EndOfRound);
        
        // 全ワーカーを回収
        foreach (Player player in players)
        {
            player.ReturnAllWorkers();
        }
        
        // アクションスペースをリセット
        foreach (ActionSpace actionSpace in actionSpaces)
        {
            actionSpace.RemoveAllWorkers();
        }
        
        // 収穫フェーズかチェック
        if (IsHarvestRound(currentRound))
        {
            ExecuteHarvest();
        }
        
        // 次のラウンドへ
        currentRound++;
        OnRoundChanged?.Invoke(currentRound);
        
        if (currentRound > maxRounds)
        {
            EndGame();
        }
        else
        {
            ChangeGameState(GameState.PlayerTurn);
            StartPlayerTurn();
        }
    }
    
    private bool IsHarvestRound(int round)
    {
        return System.Array.Exists(harvestRounds, r => r == round);
    }
    
    private void ExecuteHarvest()
    {
        Debug.Log("収穫フェーズを実行中...");
        
        foreach (Player player in players)
        {
            // 1. 収穫
            player.HarvestCrops();
            
            // 2. 餌やり
            int beggingCardsReceived = player.FeedFamily();
            if (beggingCardsReceived > 0)
            {
                if (!beggingCards.ContainsKey(player))
                    beggingCards[player] = 0;
                beggingCards[player] += beggingCardsReceived;
                
                Debug.Log($"{player.playerName}が乞食カードを{beggingCardsReceived}枚受け取りました");
            }
            
            // 3. 動物の繁殖
            player.BreedAnimals();
        }
    }
    
    private void EndGame()
    {
        ChangeGameState(GameState.GameOver);
        
        // Agricola風のスコア計算
        CalculateFinalScores();
        
        // 勝者を決定
        Player winner = players.OrderByDescending(p => p.GetVictoryPoints()).First();
        OnGameWon?.Invoke(winner);
        
        Debug.Log($"ゲーム終了！勝者: {winner.playerName} (勝利点: {winner.GetVictoryPoints()})");
    }
    
    private void CalculateFinalScores()
    {
        foreach (Player player in players)
        {
            int totalScore = 0;
            
            // 家族メンバー：1人につき3点
            totalScore += player.GetFamilyMembers() * 3;
            
            // 住居の種類：木0点、土1点、石2点（1部屋につき）
            int houseScore = 0;
            switch (player.GetHouseType())
            {
                case Player.HouseType.Wood: houseScore = 0; break;
                case Player.HouseType.Clay: houseScore = 1; break;
                case Player.HouseType.Stone: houseScore = 2; break;
            }
            totalScore += houseScore * player.GetRooms();
            
            // 畑の点数（0-1: -1点, 2: 1点, 3: 2点, 4: 3点, 5+: 4点）
            totalScore += CalculateCategoryScore(player.GetFields());
            
            // 牧場の点数
            totalScore += CalculateCategoryScore(player.GetPastures());
            
            // 穀物の点数
            totalScore += CalculateCategoryScore(player.GetResource(ResourceType.Grain));
            
            // 野菜の点数
            totalScore += CalculateCategoryScore(player.GetResource(ResourceType.Vegetable));
            
            // 羊の点数
            totalScore += CalculateCategoryScore(player.GetResource(ResourceType.Sheep));
            
            // 猪の点数
            totalScore += CalculateCategoryScore(player.GetResource(ResourceType.Boar));
            
            // 牛の点数
            totalScore += CalculateCategoryScore(player.GetResource(ResourceType.Cattle));
            
            // 乞食カードのペナルティ：1枚につき-3点
            if (beggingCards.ContainsKey(player))
            {
                totalScore -= beggingCards[player] * 3;
            }
            
            // プレイしたカードからの点数
            foreach (Card card in player.GetPlayedCards())
            {
                totalScore += card.victoryPoints;
            }
            
            player.SetVictoryPoints(totalScore);
            
            Debug.Log($"{player.playerName}の最終スコア: {totalScore}点");
        }
    }
    
    private int CalculateCategoryScore(int amount)
    {
        // Agricolaのスコアリング表に基づく
        if (amount == 0) return -1;
        if (amount == 1) return -1;
        if (amount == 2) return 1;
        if (amount == 3) return 2;
        if (amount == 4) return 3;
        return 4; // 5以上
    }
    
    public void OnActionSpaceClicked(ActionSpace actionSpace)
    {
        if (currentGameState != GameState.PlayerTurn || currentTurnPhase != TurnPhase.PlaceWorkers)
            return;
            
        if (CurrentPlayer.isAI)
            return;
            
        // プレイヤーがワーカーを配置
        if (CurrentPlayer.PlaceWorker(actionSpace))
        {
            Debug.Log($"{CurrentPlayer.playerName}が{actionSpace.actionName}にワーカーを配置しました");
            
            // ワーカーがなくなった場合、ターン終了
            if (CurrentPlayer.GetAvailableWorkers() == 0)
            {
                EndPlayerTurn();
            }
        }
    }
    
    private void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
    
    private void ChangeTurnPhase(TurnPhase newPhase)
    {
        currentTurnPhase = newPhase;
        OnTurnPhaseChanged?.Invoke(newPhase);
    }
    
    // ゲームリセット
    public void RestartGame()
    {
        currentRound = 1;
        currentPlayerIndex = 0;
        
        foreach (Player player in players)
        {
            player.ReturnAllWorkers();
        }
        
        InitializeGame();
    }
}