using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FivePlayerGameTest : MonoBehaviour
{
    [Header("5人プレイテスト設定")]
    public bool runFivePlayerTest = true;
    public bool testResourceScarcity = true;
    public bool testActionSpaceCompetition = true;
    public bool testBalanceIssues = true;
    public float testSpeed = 0.3f;
    
    private GameManager gameManager;
    private ActionSpaceManager actionSpaceManager;
    private ResourceConverter resourceConverter;
    private List<Player> players = new List<Player>();
    
    void Start()
    {
        if (runFivePlayerTest)
        {
            StartCoroutine(RunFivePlayerTest());
        }
    }
    
    IEnumerator RunFivePlayerTest()
    {
        Debug.Log("👥 === 5人プレイゲームテスト開始 ===");
        
        yield return new WaitForSeconds(1f);
        
        // 5人プレイヤーの作成
        yield return StartCoroutine(SetupFivePlayers());
        
        // 初期化テスト
        yield return StartCoroutine(TestFivePlayerInitialization());
        
        // リソース競争テスト
        if (testResourceScarcity)
        {
            yield return StartCoroutine(TestResourceScarcity());
        }
        
        // アクションスペース競争テスト
        if (testActionSpaceCompetition)
        {
            yield return StartCoroutine(TestActionSpaceCompetition());
        }
        
        // バランス問題テスト
        if (testBalanceIssues)
        {
            yield return StartCoroutine(TestGameBalance());
        }
        
        // 完全5人ゲームシミュレーション
        yield return StartCoroutine(SimulateFullFivePlayerGame());
        
        Debug.Log("🎉 === 5人プレイゲームテスト完了 ===");
    }
    
    IEnumerator SetupFivePlayers()
    {
        Debug.Log("👤 5人プレイヤーセットアップ開始");
        
        // 既存プレイヤーを削除
        Player[] existingPlayers = FindObjectsOfType<Player>();
        foreach (Player player in existingPlayers)
        {
            DestroyImmediate(player.gameObject);
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // 5人のプレイヤーを作成
        string[] playerNames = { "アリス", "ボブ", "チャーリー", "ダイアナ", "エドワード" };
        Color[] playerColors = { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta };
        
        for (int i = 0; i < 5; i++)
        {
            GameObject playerObj = new GameObject($"Player_{i + 1}");
            Player player = playerObj.AddComponent<Player>();
            player.playerName = playerNames[i];
            player.playerColor = playerColors[i];
            player.isAI = i > 0; // プレイヤー1は人間、他はAI
            
            players.Add(player);
            Debug.Log($"  プレイヤー作成: {player.playerName} ({(player.isAI ? "AI" : "人間")})");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFivePlayerInitialization()
    {
        Debug.Log("🔧 5人プレイ初期化テスト");
        
        // システムコンポーネントの取得
        gameManager = FindObjectOfType<GameManager>();
        actionSpaceManager = FindObjectOfType<ActionSpaceManager>();
        resourceConverter = FindObjectOfType<ResourceConverter>();
        
        // 基本検証
        Assert(players.Count == 5, "5人のプレイヤーが作成されている");
        Assert(gameManager != null, "GameManagerが存在する");
        
        // 各プレイヤーの初期状態確認
        foreach (Player player in players)
        {
            Assert(player.GetFamilyMembers() == 2, $"{player.playerName}の初期家族数が2人");
            Assert(player.GetRooms() == 2, $"{player.playerName}の初期部屋数が2部屋");
            Assert(player.GetAvailableWorkers() == 2, $"{player.playerName}の利用可能ワーカーが2人");
        }
        
        Debug.Log("✅ 5人プレイ初期化テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestResourceScarcity()
    {
        Debug.Log("🏆 リソース競争テスト開始");
        
        // アクションスペースの確認
        List<ActionSpace> actionSpaces = FindObjectsOfType<ActionSpace>().ToList();
        Debug.Log($"利用可能アクションスペース: {actionSpaces.Count}個");
        
        // 各リソースタイプのアクションスペース数を確認
        var resourceSpaces = actionSpaces.Where(space => space.actionType == ActionType.GainResources).ToList();
        Debug.Log($"リソース獲得スペース: {resourceSpaces.Count}個");
        
        // 5人分のワーカー（合計10人）に対してスペースが足りるかテスト
        int totalWorkers = players.Sum(p => p.GetAvailableWorkers());
        Debug.Log($"総ワーカー数: {totalWorkers}人");
        Debug.Log($"総アクションスペース: {actionSpaces.Count}個");
        
        if (actionSpaces.Count < totalWorkers)
        {
            Debug.LogWarning("⚠️ アクションスペースが不足する可能性があります");
        }
        
        // リソース競争シミュレーション
        yield return StartCoroutine(SimulateResourceCompetition());
        
        Debug.Log("✅ リソース競争テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator SimulateResourceCompetition()
    {
        Debug.Log("-- リソース競争シミュレーション --");
        
        // 全プレイヤーが木材を取りに行くシナリオ
        ActionSpace forestSpace = FindObjectsOfType<ActionSpace>()
            .FirstOrDefault(space => space.actionName == "森");
        
        if (forestSpace != null)
        {
            Debug.Log("🌲 全プレイヤーが森を狙うシナリオ");
            
            int playersWhoGotWood = 0;
            foreach (Player player in players)
            {
                if (forestSpace.CanPlaceWorker())
                {
                    bool placed = player.PlaceWorker(forestSpace);
                    if (placed)
                    {
                        playersWhoGotWood++;
                        Debug.Log($"  {player.playerName}: 木材獲得成功");
                    }
                }
                else
                {
                    Debug.Log($"  {player.playerName}: 木材獲得失敗（先着に負けた）");
                }
                
                yield return new WaitForSeconds(testSpeed / 2);
            }
            
            Debug.Log($"結果: {playersWhoGotWood}/5人が木材を獲得");
            
            // クリーンアップ
            foreach (Player player in players)
            {
                player.ReturnAllWorkers();
            }
        }
    }
    
    IEnumerator TestActionSpaceCompetition()
    {
        Debug.Log("🎯 アクションスペース競争テスト開始");
        
        // 人気の高いアクションスペースのテスト
        string[] popularActions = { "森", "家族の成長", "住居の拡張", "スタートプレイヤー" };
        
        foreach (string actionName in popularActions)
        {
            yield return StartCoroutine(TestSpecificActionCompetition(actionName));
        }
        
                 Debug.Log("✅ アクションスペース競争テスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestSpecificActionCompetition(string actionName)
    {
        Debug.Log($"-- {actionName} 競争テスト --");
        
        ActionSpace targetSpace = FindObjectsOfType<ActionSpace>()
            .FirstOrDefault(space => space.actionName == actionName);
        
        if (targetSpace == null)
        {
            Debug.LogWarning($"⚠️ {actionName} アクションスペースが見つかりません");
            yield break;
        }
        
        // 全プレイヤーが同じアクションを狙う
        int successfulPlacements = 0;
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            
            if (targetSpace.CanPlaceWorker())
            {
                bool placed = player.PlaceWorker(targetSpace);
                if (placed)
                {
                    successfulPlacements++;
                    Debug.Log($"  {player.playerName}: ✅ 配置成功（{i + 1}番目）");
                }
            }
            else
            {
                Debug.Log($"  {player.playerName}: ❌ 配置失敗（既に占領済み）");
            }
            
            yield return new WaitForSeconds(testSpeed / 3);
        }
        
        Debug.Log($"{actionName} 結果: {successfulPlacements}/5人が成功");
        
        // スペースが1人制限の場合の競争激化を確認
        if (successfulPlacements == 1)
        {
            Debug.Log($"💥 {actionName}は高競争アクション（1人制限）");
        }
        
        // クリーンアップ
        foreach (Player player in players)
        {
            player.ReturnAllWorkers();
        }
        
        yield return new WaitForSeconds(testSpeed / 2);
    }
    
    IEnumerator TestGameBalance()
    {
        Debug.Log("⚖️ ゲームバランステスト開始");
        
        // 食料供給テスト（5人全員に十分な食料があるか）
        yield return StartCoroutine(TestFoodSupplyBalance());
        
        // アクションスペース拡張の必要性テスト
        yield return StartCoroutine(TestActionSpaceScaling());
        
        // ゲーム時間テスト
        yield return StartCoroutine(TestGameDuration());
        
        Debug.Log("✅ ゲームバランステスト完了");
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestFoodSupplyBalance()
    {
        Debug.Log("-- 食料供給バランステスト --");
        
        // 全プレイヤーに基本的なリソースを与える
        foreach (Player player in players)
        {
            player.AddResource(ResourceType.Grain, 2);
            player.AddResource(ResourceType.Sheep, 1);
        }
        
        yield return new WaitForSeconds(testSpeed);
        
        // 収穫フェーズをシミュレート
        int totalBeggingCards = 0;
        foreach (Player player in players)
        {
            int beggingCards = player.FeedFamily();
            totalBeggingCards += beggingCards;
            
            if (beggingCards > 0)
            {
                Debug.Log($"  {player.playerName}: 乞食カード {beggingCards}枚");
            }
            else
            {
                Debug.Log($"  {player.playerName}: 食料供給成功");
            }
        }
        
        Debug.Log($"総乞食カード数: {totalBeggingCards}枚");
        
        if (totalBeggingCards > 5)
        {
            Debug.LogWarning("⚠️ 5人プレイでは食料供給が厳しすぎる可能性があります");
        }
    }
    
    IEnumerator TestActionSpaceScaling()
    {
        Debug.Log("-- アクションスペーススケーリングテスト --");
        
        // 現在のアクションスペース数と推奨数の比較
        List<ActionSpace> actionSpaces = FindObjectsOfType<ActionSpace>().ToList();
        int currentSpaces = actionSpaces.Count;
        int totalWorkers = players.Sum(p => p.GetAvailableWorkers());
        
        // 理想的には総ワーカー数の1.2～1.5倍のアクションスペースが必要
        int recommendedSpaces = Mathf.RoundToInt(totalWorkers * 1.3f);
        
        Debug.Log($"現在のアクションスペース: {currentSpaces}個");
        Debug.Log($"推奨アクションスペース: {recommendedSpaces}個");
        Debug.Log($"総ワーカー数: {totalWorkers}人");
        
        if (currentSpaces < recommendedSpaces)
        {
            Debug.LogWarning($"⚠️ アクションスペースを{recommendedSpaces - currentSpaces}個追加することを推奨");
        }
        else
        {
            Debug.Log("✅ アクションスペース数は適切");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator TestGameDuration()
    {
        Debug.Log("-- ゲーム時間テスト --");
        
        // 5人プレイの推定ゲーム時間を計算
        int roundsPerGame = 14;
        int playersCount = 5;
        int averageWorkersPerPlayer = 3; // 中盤以降の平均
        float timePerWorkerPlacement = 10f; // 秒
        
        float estimatedGameTime = roundsPerGame * playersCount * averageWorkersPerPlayer * timePerWorkerPlacement / 60f;
        
        Debug.Log($"推定ゲーム時間: {estimatedGameTime:F1}分");
        
        if (estimatedGameTime > 90)
        {
            Debug.LogWarning("⚠️ ゲーム時間が長すぎる可能性があります（90分超）");
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator SimulateFullFivePlayerGame()
    {
        Debug.Log("🎮 完全5人ゲームシミュレーション開始");
        
        // ラウンド1-7をシミュレート（2回目の収穫まで）
        for (int round = 1; round <= 7; round++)
        {
            Debug.Log($"\n=== ラウンド {round} (5人プレイ) ===");
            
            yield return StartCoroutine(SimulateFivePlayerRound(round));
            
            // 収穫ラウンドの処理
            if (round == 4 || round == 7)
            {
                yield return StartCoroutine(SimulateFivePlayerHarvest(round));
            }
            
            yield return new WaitForSeconds(testSpeed);
        }
        
        // 最終状態の分析
        yield return StartCoroutine(AnalyzeFinalGameState());
        
        Debug.Log("✅ 完全5人ゲームシミュレーション完了");
    }
    
    IEnumerator SimulateFivePlayerRound(int round)
    {
        // アクションスペース解放
        if (actionSpaceManager != null)
        {
            actionSpaceManager.ActivateActionSpacesForRound(round);
        }
        
        List<ActionSpace> availableSpaces = FindObjectsOfType<ActionSpace>()
            .Where(space => space.gameObject.activeInHierarchy)
            .ToList();
        
        Debug.Log($"利用可能アクションスペース: {availableSpaces.Count}個");
        
        // 各プレイヤーのターン
        foreach (Player player in players)
        {
            yield return StartCoroutine(SimulateFivePlayerTurn(player, round, availableSpaces));
        }
        
        // ラウンド終了後の統計
        LogRoundStatistics(round, availableSpaces);
    }
    
    IEnumerator SimulateFivePlayerTurn(Player player, int round, List<ActionSpace> availableSpaces)
    {
        Debug.Log($"{player.playerName}のターン (R{round})");
        
        int workersPlaced = 0;
        int availableWorkers = player.GetAvailableWorkers();
        
        // 戦略的にアクションスペースを選択（簡易AI）
        List<ActionSpace> playerPreferences = GetPlayerActionPreferences(player, availableSpaces, round);
        
        for (int i = 0; i < availableWorkers && playerPreferences.Count > 0; i++)
        {
            ActionSpace selectedSpace = playerPreferences[0];
            playerPreferences.RemoveAt(0);
            
            if (selectedSpace.CanPlaceWorker())
            {
                bool placed = player.PlaceWorker(selectedSpace);
                if (placed)
                {
                    workersPlaced++;
                    Debug.Log($"  📍 {selectedSpace.actionName}に配置");
                    availableSpaces.Remove(selectedSpace);
                }
            }
            
            yield return new WaitForSeconds(testSpeed / 4);
        }
        
        Debug.Log($"  {player.playerName}: {workersPlaced}/{availableWorkers}ワーカー配置完了");
        LogPlayerStatus(player);
        
        yield return new WaitForSeconds(testSpeed / 3);
    }
    
    private List<ActionSpace> GetPlayerActionPreferences(Player player, List<ActionSpace> availableSpaces, int round)
    {
        // 簡易戦略AI：プレイヤーの状況に応じて優先度を決定
        var preferences = new List<(ActionSpace space, int priority)>();
        
        foreach (ActionSpace space in availableSpaces.Where(s => s.CanPlaceWorker()))
        {
            int priority = CalculateActionPriority(player, space, round);
            preferences.Add((space, priority));
        }
        
        return preferences.OrderByDescending(p => p.priority).Select(p => p.space).ToList();
    }
    
    private int CalculateActionPriority(Player player, ActionSpace space, int round)
    {
        int priority = 0;
        
        // 基本的な優先度計算
        switch (space.actionType)
        {
            case ActionType.GainResources:
                priority = 5; // 基本的な重要度
                if (space.actionName == "森") priority += 2; // 木材は重要
                if (space.actionName == "日雇い労働者") priority += 3; // 食料は最重要
                break;
                
            case ActionType.FamilyGrowth:
                priority = player.GetFamilyMembers() < 4 ? 8 : 2; // 家族成長は重要だが限度あり
                break;
                
            case ActionType.AddField:
                priority = player.GetFields() < 3 ? 6 : 1; // 畑は適度に必要
                break;
                
            case ActionType.HouseExpansion:
                priority = player.GetRooms() <= player.GetFamilyMembers() ? 7 : 1;
                break;
                
            case ActionType.StartingPlayer:
                priority = 4; // 手番調整
                break;
                
            default:
                priority = 3;
                break;
        }
        
        // ラウンド後半では食料確保を優先
        if (round >= 3 && space.actionName.Contains("食料"))
        {
            priority += 3;
        }
        
        return priority + Random.Range(-1, 2); // 少しランダム性を追加
    }
    
    IEnumerator SimulateFivePlayerHarvest(int round)
    {
        Debug.Log($"🌾 5人プレイ収穫フェーズ (ラウンド{round})");
        
        int totalBeggingCards = 0;
        List<string> harvestResults = new List<string>();
        
        foreach (Player player in players)
        {
            // 収穫実行
            int grainBefore = player.GetResource(ResourceType.Grain);
            player.HarvestCrops();
            int grainAfter = player.GetResource(ResourceType.Grain);
            
            // 餌やり
            int beggingCards = player.FeedFamily();
            totalBeggingCards += beggingCards;
            
            // 動物繁殖
            player.BreedAnimals();
            
            string result = $"{player.playerName}: 穀物+{grainAfter - grainBefore}";
            if (beggingCards > 0)
            {
                result += $", 乞食カード{beggingCards}枚";
            }
            
            harvestResults.Add(result);
            
            yield return new WaitForSeconds(testSpeed / 3);
        }
        
        Debug.Log("収穫結果:");
        foreach (string result in harvestResults)
        {
            Debug.Log($"  {result}");
        }
        
        Debug.Log($"総乞食カード数: {totalBeggingCards}枚");
        
        if (totalBeggingCards > 8)
        {
            Debug.LogWarning("⚠️ 5人プレイでは食料供給圧力が高すぎます");
        }
        
        // ワーカー回収
        foreach (Player player in players)
        {
            player.ReturnAllWorkers();
        }
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    IEnumerator AnalyzeFinalGameState()
    {
        Debug.Log("\n📊 === 5人プレイ最終分析 ===");
        
        // プレイヤー順位付け
        var playerRankings = players.OrderByDescending(p => CalculateCurrentScore(p)).ToList();
        
        Debug.Log("現在の順位:");
        for (int i = 0; i < playerRankings.Count; i++)
        {
            Player player = playerRankings[i];
            int score = CalculateCurrentScore(player);
            Debug.Log($"  {i + 1}位: {player.playerName} ({score}点)");
        }
        
        // バランス分析
        AnalyzeGameBalance();
        
        yield return new WaitForSeconds(testSpeed);
    }
    
    private int CalculateCurrentScore(Player player)
    {
        // 簡易スコア計算（実際のAgricolaスコアリングの簡略版）
        int score = 0;
        score += player.GetFamilyMembers() * 3;
        score += player.GetFields();
        score += player.GetResource(ResourceType.Grain);
        score += player.GetResource(ResourceType.Sheep);
        score += player.GetResource(ResourceType.Boar) * 2;
        score += player.GetResource(ResourceType.Cattle) * 3;
        
        return score;
    }
    
    private void AnalyzeGameBalance()
    {
        Debug.Log("\n⚖️ バランス分析:");
        
        // リソース分布の分析
        var resourceTotals = new Dictionary<ResourceType, int>();
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int total = players.Sum(p => p.GetResource(resourceType));
            resourceTotals[resourceType] = total;
            
            if (total > 0)
            {
                Debug.Log($"  {resourceType}総計: {total}個 (平均: {total / 5f:F1}個/人)");
            }
        }
        
        // 発展度の分析
        float avgFamilySize = players.Average(p => p.GetFamilyMembers());
        float avgFields = players.Average(p => p.GetFields());
        float avgRooms = players.Average(p => p.GetRooms());
        
        Debug.Log($"\n平均発展度:");
        Debug.Log($"  家族サイズ: {avgFamilySize:F1}人");
        Debug.Log($"  畑数: {avgFields:F1}個");
        Debug.Log($"  部屋数: {avgRooms:F1}個");
        
        // 競争激しさの分析
        List<ActionSpace> actionSpaces = FindObjectsOfType<ActionSpace>().ToList();
        int occupiedSpaces = actionSpaces.Count(space => !space.CanPlaceWorker());
        float occupationRate = (float)occupiedSpaces / actionSpaces.Count * 100f;
        
        Debug.Log($"\nアクションスペース占有率: {occupationRate:F1}%");
        
        if (occupationRate > 80f)
        {
            Debug.LogWarning("⚠️ アクションスペースの競争が激しすぎます");
        }
        else if (occupationRate < 50f)
        {
            Debug.Log("💡 アクションスペースに余裕があります");
        }
        else
        {
            Debug.Log("✅ アクションスペースの競争度は適切です");
        }
    }
    
    private void LogRoundStatistics(int round, List<ActionSpace> availableSpaces)
    {
        int usedSpaces = availableSpaces.Count(space => !space.CanPlaceWorker());
        int totalSpaces = availableSpaces.Count;
        
        Debug.Log($"ラウンド{round}統計: {usedSpaces}/{totalSpaces}スペース使用");
        
        if (usedSpaces == totalSpaces)
        {
            Debug.Log("💥 全アクションスペースが使用されました！");
        }
    }
    
    private void LogPlayerStatus(Player player)
    {
        var resources = new List<string>();
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = player.GetResource(resourceType);
            if (amount > 0)
            {
                resources.Add($"{resourceType}:{amount}");
            }
        }
        
        Debug.Log($"    {player.playerName}: " +
                 $"家族{player.GetFamilyMembers()}人, " +
                 $"畑{player.GetFields()}個, " +
                 $"部屋{player.GetRooms()}個 " +
                 $"[{string.Join(",", resources)}]");
    }
    
    private void Assert(bool condition, string message)
    {
        if (condition)
        {
            Debug.Log($"✅ {message}");
        }
        else
        {
            Debug.LogError($"❌ {message}");
        }
    }
    
    [ContextMenu("Run 5-Player Test")]
    public void RunFivePlayerTestManual()
    {
        StartCoroutine(RunFivePlayerTest());
    }
}