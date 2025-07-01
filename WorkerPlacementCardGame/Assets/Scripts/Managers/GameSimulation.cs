using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameSimulation : MonoBehaviour
{
    [Header("シミュレーション設定")]
    public bool autoRunSimulation = true;
    public float simulationSpeed = 1f;
    
    private GameManager gameManager;
    private List<Player> players;
    private List<ActionSpace> actionSpaces;
    
    void Start()
    {
        if (autoRunSimulation)
        {
            StartSimulation();
        }
    }
    
    public void StartSimulation()
    {
        Debug.Log("=== 2人プレイゲームシミュレーション開始 ===");
        
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManagerが見つかりません");
            return;
        }
        
        // 初期状態をチェック
        CheckInitialSetup();
        
        // ゲームフローをシミュレート
        SimulateGameFlow();
    }
    
    private void CheckInitialSetup()
    {
        Debug.Log("--- 初期設定チェック ---");
        
        // プレイヤーの確認
        players = FindObjectsOfType<Player>().ToList();
        Debug.Log($"プレイヤー数: {players.Count}");
        
        foreach (Player player in players)
        {
            Debug.Log($"{player.playerName}:");
            Debug.Log($"  家族数: {player.GetFamilyMembers()}");
            Debug.Log($"  部屋数: {player.GetRooms()}");
            Debug.Log($"  住居タイプ: {player.GetHouseType()}");
            Debug.Log($"  利用可能ワーカー: {player.GetAvailableWorkers()}");
            
            // 初期リソースチェック
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int amount = player.GetResource(resourceType);
                if (amount > 0)
                {
                    Debug.Log($"  {resourceType}: {amount}");
                }
            }
        }
        
        // アクションスペースの確認
        actionSpaces = FindObjectsOfType<ActionSpace>().ToList();
        Debug.Log($"アクションスペース数: {actionSpaces.Count}");
        
        foreach (ActionSpace space in actionSpaces)
        {
            Debug.Log($"  {space.actionName} ({space.actionType})");
        }
    }
    
    private void SimulateGameFlow()
    {
        Debug.Log("--- ゲームフローシミュレーション ---");
        
        // ラウンド1-3をシミュレート（収穫前）
        for (int round = 1; round <= 3; round++)
        {
            Debug.Log($"\n=== ラウンド {round} ===");
            SimulateRound(round);
        }
        
        // ラウンド4（最初の収穫）
        Debug.Log($"\n=== ラウンド 4（最初の収穫） ===");
        SimulateRound(4);
        SimulateHarvest(4);
        
        // 問題点の特定
        IdentifyIssues();
    }
    
    private void SimulateRound(int roundNumber)
    {
        Debug.Log($"--- ラウンド {roundNumber} 開始 ---");
        
        // 各プレイヤーのターンをシミュレート
        foreach (Player player in players)
        {
            SimulatePlayerTurn(player, roundNumber);
        }
        
        Debug.Log($"--- ラウンド {roundNumber} 終了 ---");
    }
    
    private void SimulatePlayerTurn(Player player, int round)
    {
        Debug.Log($"{player.playerName}のターン開始");
        
        int availableWorkers = player.GetAvailableWorkers();
        Debug.Log($"利用可能な家族: {availableWorkers}人");
        
        // 利用可能なアクションスペースを取得
        List<ActionSpace> availableSpaces = actionSpaces.Where(space => space.CanPlaceWorker()).ToList();
        
        for (int i = 0; i < availableWorkers && availableSpaces.Count > 0; i++)
        {
            // 簡単なAI：最初の利用可能なスペースを選択
            ActionSpace selectedSpace = availableSpaces[0];
            
            Debug.Log($"  家族を{selectedSpace.actionName}に配置");
            
            // ワーカー配置をシミュレート
            bool placed = player.PlaceWorker(selectedSpace);
            if (placed)
            {
                Debug.Log($"    配置成功");
                availableSpaces.Remove(selectedSpace);
                
                // リソース状況を確認
                LogPlayerResources(player);
            }
            else
            {
                Debug.Log($"    配置失敗");
            }
        }
        
        Debug.Log($"{player.playerName}のターン終了\n");
    }
    
    private void SimulateHarvest(int round)
    {
        Debug.Log($"=== 収穫フェーズ（ラウンド {round}） ===");
        
        foreach (Player player in players)
        {
            Debug.Log($"{player.playerName}の収穫:");
            
            // 1. 作物の収穫
            int fieldsBefore = player.GetFields();
            player.HarvestCrops();
            Debug.Log($"  畑の収穫: {fieldsBefore}畑から穀物獲得");
            
            // 2. 食料供給
            int foodNeeded = player.GetFoodNeeded();
            int foodAvailable = player.GetResource(ResourceType.Food);
            Debug.Log($"  食料需要: {foodNeeded}, 利用可能: {foodAvailable}");
            
            int beggingCards = player.FeedFamily();
            if (beggingCards > 0)
            {
                Debug.Log($"  ⚠️ 乞食カード {beggingCards}枚受取");
            }
            else
            {
                Debug.Log($"  ✅ 家族への食料供給完了");
            }
            
            // 3. 動物の繁殖
            int sheepBefore = player.GetResource(ResourceType.Sheep);
            int boarBefore = player.GetResource(ResourceType.Boar);
            int cattleBefore = player.GetResource(ResourceType.Cattle);
            
            player.BreedAnimals();
            
            int sheepAfter = player.GetResource(ResourceType.Sheep);
            int boarAfter = player.GetResource(ResourceType.Boar);
            int cattleAfter = player.GetResource(ResourceType.Cattle);
            
            if (sheepAfter > sheepBefore) Debug.Log($"  羊が繁殖: {sheepBefore} → {sheepAfter}");
            if (boarAfter > boarBefore) Debug.Log($"  猪が繁殖: {boarBefore} → {boarAfter}");
            if (cattleAfter > cattleBefore) Debug.Log($"  牛が繁殖: {cattleBefore} → {cattleAfter}");
            
            LogPlayerResources(player);
        }
        
        // 全ワーカーを回収
        foreach (Player player in players)
        {
            player.ReturnAllWorkers();
        }
        
        Debug.Log("収穫フェーズ終了\n");
    }
    
    private void LogPlayerResources(Player player)
    {
        List<string> resources = new List<string>();
        
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = player.GetResource(resourceType);
            if (amount > 0)
            {
                resources.Add($"{resourceType}:{amount}");
            }
        }
        
        Debug.Log($"    リソース: {string.Join(", ", resources)}");
        Debug.Log($"    農場: 畑{player.GetFields()}, 牧場{player.GetPastures()}, 柵{player.GetFences()}, 小屋{player.GetStables()}");
    }
    
    private void IdentifyIssues()
    {
        Debug.Log("\n=== 発見された問題点 ===");
        
        List<string> issues = new List<string>();
        
        // 1. カードシステムの確認
        CardDeck deck = FindObjectOfType<CardDeck>();
        if (deck == null)
        {
            issues.Add("❌ CardDeckが見つかりません");
        }
        
        // 2. UIシステムの確認
        GameUI gameUI = FindObjectOfType<GameUI>();
        if (gameUI == null)
        {
            issues.Add("❌ GameUIが見つかりません");
        }
        
        // 3. プレイヤーの初期状態確認
        foreach (Player player in players)
        {
            if (player.GetFamilyMembers() != 2)
            {
                issues.Add($"❌ {player.playerName}の初期家族数が正しくありません (現在: {player.GetFamilyMembers()}, 期待値: 2)");
            }
            
            if (player.GetRooms() != 2)
            {
                issues.Add($"❌ {player.playerName}の初期部屋数が正しくありません (現在: {player.GetRooms()}, 期待値: 2)");
            }
            
            if (player.GetHouseType() != Player.HouseType.Wood)
            {
                issues.Add($"❌ {player.playerName}の初期住居タイプが正しくありません (現在: {player.GetHouseType()}, 期待値: Wood)");
            }
        }
        
        // 4. アクションスペースの確認
        var requiredActions = new[]
        {
            ActionType.GainResources,
            ActionType.AddField,
            ActionType.FamilyGrowth,
            ActionType.HouseExpansion,
            ActionType.BuildFences,
            ActionType.TakeAnimals
        };
        
        foreach (ActionType actionType in requiredActions)
        {
            if (!actionSpaces.Any(space => space.actionType == actionType))
            {
                issues.Add($"❌ {actionType}のアクションスペースが見つかりません");
            }
        }
        
        // 5. リソース変換システムの確認
        bool hasResourceConversion = CheckResourceConversionSystem();
        if (!hasResourceConversion)
        {
            issues.Add("❌ リソース変換システム（穀物→食料など）が不足しています");
        }
        
        // 6. ゲームの進行制御確認
        if (gameManager.currentRound != 1)
        {
            issues.Add($"❌ ゲーム開始時のラウンドが正しくありません (現在: {gameManager.currentRound}, 期待値: 1)");
        }
        
        // 7. ActionSpaceManagerの確認
        ActionSpaceManager actionSpaceManager = FindObjectOfType<ActionSpaceManager>();
        if (actionSpaceManager == null)
        {
            issues.Add("❌ ActionSpaceManagerが見つかりません");
        }
        
        // 8. ResourceConverterの確認
        ResourceConverter resourceConverter = FindObjectOfType<ResourceConverter>();
        if (resourceConverter == null)
        {
            issues.Add("❌ ResourceConverterが見つかりません");
        }
        
        // 結果出力
        if (issues.Count == 0)
        {
            Debug.Log("✅ 重大な問題は見つかりませんでした");
        }
        else
        {
            Debug.Log($"発見された問題: {issues.Count}件");
            foreach (string issue in issues)
            {
                Debug.Log(issue);
            }
        }
        
        // 改善提案
        Debug.Log("\n=== 改善提案 ===");
        LogImprovementSuggestions();
    }
    
    private bool CheckResourceConversionSystem()
    {
        // リソース変換システムの存在確認
        ResourceConverter converter = FindObjectOfType<ResourceConverter>();
        return converter != null;
    }
    
    private void LogImprovementSuggestions()
    {
        Debug.Log("💡 リソース変換システム（穀物や動物を食料に変換）");
        Debug.Log("💡 職業カードシステムの実装");
        Debug.Log("💡 小さな進歩カードシステムの実装");
        Debug.Log("💡 より詳細な農場ボードビジュアル");
        Debug.Log("💡 アニメーション効果");
        Debug.Log("💡 サウンド効果");
        Debug.Log("💡 段階的なアクションスペース解放システム");
        Debug.Log("💡 スタートプレイヤートークンの実装");
        Debug.Log("💡 より高度なAI戦略");
        Debug.Log("💡 ゲーム設定の調整機能");
    }
    
    // 手動でシミュレーションを実行するためのメソッド
    [ContextMenu("Run Simulation")]
    public void RunManualSimulation()
    {
        StartSimulation();
    }
}