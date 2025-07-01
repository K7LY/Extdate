using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// CardTriggerManagerの使用例とテストを行うクラス
/// </summary>
public class CardTriggerTest : MonoBehaviour
{
    [Header("テスト設定")]
    public bool runTestOnStart = false;
    public bool enableDebugOutput = true;
    
    private GameManager gameManager;
    private CardTriggerManager cardTriggerManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        cardTriggerManager = FindObjectOfType<CardTriggerManager>();
        
        if (runTestOnStart)
        {
            StartCoroutine(RunTestsAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator RunTestsAfterDelay()
    {
        // ゲームが初期化されるまで待機
        yield return new WaitForSeconds(2f);
        
        RunCardTriggerTests();
    }
    
    /// <summary>
    /// カードトリガーシステムのテストを実行
    /// </summary>
    public void RunCardTriggerTests()
    {
        if (gameManager == null || cardTriggerManager == null)
        {
            Debug.LogError("GameManagerまたはCardTriggerManagerが見つかりません");
            return;
        }
        
        Debug.Log("=== CardTriggerManager テスト開始 ===");
        
        // テスト1: 収穫時のトリガー可能カードを取得
        TestHarvestTriggers();
        
        // テスト2: アクション実行時のトリガー可能カードを取得
        TestActionTriggers();
        
        // テスト3: 繁殖時のトリガー可能カードを取得
        TestBreedingTriggers();
        
        // テスト4: 特定プレイヤーのトリガー可能カードのみを取得
        TestPlayerSpecificTriggers();
        
        Debug.Log("=== CardTriggerManager テスト終了 ===");
    }
    
    /// <summary>
    /// 収穫時のトリガーテスト
    /// </summary>
    private void TestHarvestTriggers()
    {
        Debug.Log("--- 収穫時トリガーテスト ---");
        
        // 全プレイヤーの収穫時トリガー可能カードを取得
        var triggerableCards = gameManager.GetTriggerableCards(OccupationTrigger.OnHarvest);
        
        Debug.Log($"収穫時にトリガー可能なカード数: {triggerableCards.Count}");
        
        foreach (var triggerableCard in triggerableCards)
        {
            string status = triggerableCard.canTrigger ? "[実行可能]" : "[実行不可]";
            Debug.Log($"{status} {triggerableCard.owner.playerName}: {triggerableCard.card.cardName} - {triggerableCard.effect.effectDescription}");
            Debug.Log($"  理由: {triggerableCard.triggerReason}");
        }
        
        // 実際にトリガー可能なカードのみを取得
        var activeTriggerableCards = gameManager.GetActiveTriggerableCards(OccupationTrigger.OnHarvest);
        Debug.Log($"実際に実行可能なカード数: {activeTriggerableCards.Count}");
        
        if (enableDebugOutput)
        {
            gameManager.DebugPrintTriggerableCards(OccupationTrigger.OnHarvest);
        }
    }
    
    /// <summary>
    /// アクション実行時のトリガーテスト
    /// </summary>
    private void TestActionTriggers()
    {
        Debug.Log("--- アクション実行時トリガーテスト ---");
        
        // アクションスペースを取得（テスト用）
        ActionSpace[] actionSpaces = FindObjectsOfType<ActionSpace>();
        if (actionSpaces.Length > 0)
        {
            ActionSpace testActionSpace = actionSpaces[0];
            Player currentPlayer = gameManager.CurrentPlayer;
            
            var triggerableCards = gameManager.GetTriggerableCards(OccupationTrigger.OnAction, currentPlayer, testActionSpace);
            
            Debug.Log($"アクション「{testActionSpace.actionName}」実行時にトリガー可能なカード数: {triggerableCards.Count}");
            
            foreach (var triggerableCard in triggerableCards)
            {
                string status = triggerableCard.canTrigger ? "[実行可能]" : "[実行不可]";
                Debug.Log($"{status} {triggerableCard.owner.playerName}: {triggerableCard.card.cardName} - {triggerableCard.effect.effectDescription}");
            }
        }
        else
        {
            Debug.Log("テスト用のアクションスペースが見つかりません");
        }
    }
    
    /// <summary>
    /// 繁殖時のトリガーテスト
    /// </summary>
    private void TestBreedingTriggers()
    {
        Debug.Log("--- 繁殖時トリガーテスト ---");
        
        var triggerableCards = gameManager.GetTriggerableCards(OccupationTrigger.OnBreeding);
        
        Debug.Log($"繁殖時にトリガー可能なカード数: {triggerableCards.Count}");
        
        foreach (var triggerableCard in triggerableCards)
        {
            string status = triggerableCard.canTrigger ? "[実行可能]" : "[実行不可]";
            Debug.Log($"{status} {triggerableCard.owner.playerName}: {triggerableCard.card.cardName} - {triggerableCard.effect.effectDescription}");
        }
    }
    
    /// <summary>
    /// 特定プレイヤーのトリガーテスト
    /// </summary>
    private void TestPlayerSpecificTriggers()
    {
        Debug.Log("--- 特定プレイヤートリガーテスト ---");
        
        List<Player> players = gameManager.GetPlayers();
        if (players.Count > 0)
        {
            Player testPlayer = players[0];
            
            var playerTriggerableCards = cardTriggerManager.GetTriggerableCardsForPlayer(testPlayer, OccupationTrigger.OnHarvest);
            
            Debug.Log($"{testPlayer.playerName}の収穫時トリガー可能カード数: {playerTriggerableCards.Count}");
            
            foreach (var triggerableCard in playerTriggerableCards)
            {
                string status = triggerableCard.canTrigger ? "[実行可能]" : "[実行不可]";
                Debug.Log($"{status} {triggerableCard.card.cardName} - {triggerableCard.effect.effectDescription}");
            }
        }
    }
    
    /// <summary>
    /// 手動でトリガー可能カードを確認するメソッド（インスペクター用）
    /// </summary>
    [ContextMenu("収穫時トリガー可能カードを表示")]
    public void ShowHarvestTriggerableCards()
    {
        if (gameManager != null)
        {
            gameManager.DebugPrintTriggerableCards(OccupationTrigger.OnHarvest);
        }
    }
    
    [ContextMenu("アクション時トリガー可能カードを表示")]
    public void ShowActionTriggerableCards()
    {
        if (gameManager != null)
        {
            gameManager.DebugPrintTriggerableCards(OccupationTrigger.OnAction, gameManager.CurrentPlayer);
        }
    }
    
    [ContextMenu("繁殖時トリガー可能カードを表示")]
    public void ShowBreedingTriggerableCards()
    {
        if (gameManager != null)
        {
            gameManager.DebugPrintTriggerableCards(OccupationTrigger.OnBreeding);
        }
    }
    
    /// <summary>
    /// 実際にカード効果を実行するテスト
    /// </summary>
    [ContextMenu("収穫時カード効果を実行")]
    public void ExecuteHarvestCardEffects()
    {
        if (gameManager != null)
        {
            Debug.Log("収穫時のカード効果を実行します...");
            gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnHarvest, gameManager.CurrentPlayer);
        }
    }
    
    /// <summary>
    /// 使用例を表示するメソッド
    /// </summary>
    [ContextMenu("使用例を表示")]
    public void ShowUsageExample()
    {
        Debug.Log(@"
=== CardTriggerManager 使用例 ===

1. イベント発生時にトリガー可能なカードを取得:
   var triggerableCards = gameManager.GetTriggerableCards(OccupationTrigger.OnHarvest);

2. 実際に実行可能なカードのみを取得:
   var activeCards = gameManager.GetActiveTriggerableCards(OccupationTrigger.OnHarvest);

3. 全てのトリガー可能カードを実行:
   gameManager.ExecuteAllTriggerableCards(OccupationTrigger.OnHarvest, currentPlayer);

4. 特定プレイヤーのトリガー可能カードを取得:
   var playerCards = cardTriggerManager.GetTriggerableCardsForPlayer(player, OccupationTrigger.OnHarvest);

5. デバッグ情報を表示:
   gameManager.DebugPrintTriggerableCards(OccupationTrigger.OnHarvest);

利用可能なトリガータイプ:
- OccupationTrigger.Immediate (即座)
- OccupationTrigger.OnAction (アクション実行時)
- OccupationTrigger.OnHarvest (収穫時)
- OccupationTrigger.OnBreeding (繁殖時)
- OccupationTrigger.OnTurnEnd (ターン終了時)
- OccupationTrigger.OnRoundStart (ラウンド開始時)
- OccupationTrigger.Passive (継続効果)
        ");
    }
}