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
    [Header("ゲーム設定")]
    public int maxRounds = 6;
    public int maxPlayers = 4;
    public int startingHandSize = 5;
    public int victoryPointsToWin = 15;
    
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
        
        // 勝利条件をチェック
        if (CheckWinCondition())
        {
            EndGame();
            return;
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
    
    private bool CheckWinCondition()
    {
        foreach (Player player in players)
        {
            if (player.GetVictoryPoints() >= victoryPointsToWin)
            {
                return true;
            }
        }
        return false;
    }
    
    private void EndGame()
    {
        ChangeGameState(GameState.GameOver);
        
        // 勝者を決定
        Player winner = players.OrderByDescending(p => p.GetVictoryPoints()).First();
        OnGameWon?.Invoke(winner);
        
        Debug.Log($"ゲーム終了！勝者: {winner.playerName} (勝利点: {winner.GetVictoryPoints()})");
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