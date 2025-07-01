using UnityEngine;
using System.Collections.Generic;

public class FivePlayerActionSpaceSetup : MonoBehaviour
{
    [Header("5人プレイ対応設定")]
    public bool enableFivePlayerMode = true;
    public GameObject actionSpacePrefab;
    
    private GameSetup gameSetup;
    private ActionSpaceManager actionSpaceManager;
    
    void Start()
    {
        if (enableFivePlayerMode)
        {
            SetupFivePlayerActionSpaces();
        }
    }
    
    void SetupFivePlayerActionSpaces()
    {
        Debug.Log("🔧 5人プレイ用アクションスペース追加設定開始");
        
        gameSetup = FindObjectOfType<GameSetup>();
        actionSpaceManager = FindObjectOfType<ActionSpaceManager>();
        
        if (gameSetup == null)
        {
            Debug.LogError("GameSetupが見つかりません");
            return;
        }
        
        // 既存のアクションスペースを確認
        ActionSpace[] existingSpaces = FindObjectsOfType<ActionSpace>();
        Debug.Log($"既存アクションスペース数: {existingSpaces.Length}個");
        
        // 5人プレイ用の追加アクションスペースを作成
        CreateAdditionalActionSpaces();
        
        Debug.Log("✅ 5人プレイ用アクションスペース追加完了");
    }
    
    void CreateAdditionalActionSpaces()
    {
        // 追加するアクションスペースのリスト
        var additionalSpaces = new List<ActionSpaceConfig>
        {
            // リソース獲得の追加オプション
            new ActionSpaceConfig
            {
                actionName = "森林地帯",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } },
                description = "木材2個を獲得",
                isAvailableFromRound = 1,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "粘土採掘場",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Clay, 2 } },
                description = "粘土2個を獲得",
                isAvailableFromRound = 1,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "葦の収穫",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Reed, 2 } },
                description = "葦2個を獲得",
                isAvailableFromRound = 1,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "採石場",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Stone, 1 } },
                description = "石材1個を獲得",
                isAvailableFromRound = 2,
                maxWorkers = 1
            },
            
            // 食料確保オプションの拡充
            new ActionSpaceConfig
            {
                actionName = "川での釣り",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Food, 2 } },
                description = "食料2個を獲得",
                isAvailableFromRound = 1,
                maxWorkers = 1 // 基本は1人固定
            },
            
            new ActionSpaceConfig
            {
                actionName = "野菜の収穫",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Vegetable, 1 } },
                description = "野菜1個を獲得",
                isAvailableFromRound = 3,
                maxWorkers = 1
            },
            
            // 家族・住居関連の追加オプション
            new ActionSpaceConfig
            {
                actionName = "家族の結束",
                actionType = ActionType.FamilyGrowth,
                description = "家族を1人増やす（部屋が必要）",
                isAvailableFromRound = 5,
                maxWorkers = 1,
                requirements = new Dictionary<ResourceType, int> { { ResourceType.Food, 5 } }
            },
            
            new ActionSpaceConfig
            {
                actionName = "小屋の建設",
                actionType = ActionType.HouseExpansion,
                description = "部屋を1つ追加する",
                isAvailableFromRound = 2,
                maxWorkers = 1,
                requirements = new Dictionary<ResourceType, int> 
                { 
                    { ResourceType.Wood, 5 },
                    { ResourceType.Reed, 2 }
                }
            },
            
            // 農業関連の拡充
            new ActionSpaceConfig
            {
                actionName = "開墾",
                actionType = ActionType.AddField,
                description = "畑を1つ追加する",
                isAvailableFromRound = 3,
                maxWorkers = 1 // 基本は1人固定
            },
            
            new ActionSpaceConfig
            {
                actionName = "種まきの準備",
                actionType = ActionType.SowField,
                description = "穀物または野菜の種をまく",
                isAvailableFromRound = 4,
                maxWorkers = 1
            },
            
            // 畜産関連の拡充
            new ActionSpaceConfig
            {
                actionName = "柵の材料集め",
                actionType = ActionType.BuildFences,
                description = "柵を建設する材料を集める",
                isAvailableFromRound = 6,
                maxWorkers = 1,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Wood, 4 } }
            },
            
            new ActionSpaceConfig
            {
                actionName = "山羊の取引",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Sheep, 2 } },
                description = "羊2匹を獲得",
                isAvailableFromRound = 7,
                maxWorkers = 1
            },
            
            // 特殊アクション
            new ActionSpaceConfig
            {
                actionName = "市場での交易",
                actionType = ActionType.TradeResources,
                description = "リソースを交換する",
                isAvailableFromRound = 8,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "改築の準備",
                actionType = ActionType.HouseRenovation,
                description = "住居の改築を行う",
                isAvailableFromRound = 9,
                maxWorkers = 1
            },
            
            // 終盤の戦略的アクション
            new ActionSpaceConfig
            {
                actionName = "収穫の最適化",
                actionType = ActionType.SpecialAction,
                description = "次の収穫で追加の収穫物を得る",
                isAvailableFromRound = 10,
                maxWorkers = 1
            },
            
            // 5人プレイ用追加アクションスペース
            new ActionSpaceConfig
            {
                actionName = "森の奥地",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } },
                description = "木材1個を獲得",
                isAvailableFromRound = 1,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "小川",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Food, 1 } },
                description = "食料1個を獲得",
                isAvailableFromRound = 1,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "野原",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Vegetable, 1 } },
                description = "野菜1個を獲得",
                isAvailableFromRound = 2,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "石切り場の端",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Stone, 1 } },
                description = "石材1個を獲得",
                isAvailableFromRound = 3,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "畑の拡張",
                actionType = ActionType.AddField,
                description = "畑を1つ追加する（代替）",
                isAvailableFromRound = 4,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "小さな牧場",
                actionType = ActionType.BuildFences,
                description = "柵を建設する（代替）",
                isAvailableFromRound = 5,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "羊の道",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Sheep, 1 } },
                description = "羊1匹を獲得",
                isAvailableFromRound = 6,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "穀物収集",
                actionType = ActionType.GainResources,
                resourceRewards = new Dictionary<ResourceType, int> { { ResourceType.Grain, 2 } },
                description = "穀物2個を獲得",
                isAvailableFromRound = 4,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "手工業",
                actionType = ActionType.TradeResources,
                description = "リソースを食料に変換",
                isAvailableFromRound = 6,
                maxWorkers = 1
            },
            
            new ActionSpaceConfig
            {
                actionName = "近隣の助け",
                actionType = ActionType.SpecialAction,
                description = "小さなリソースボーナスを得る",
                isAvailableFromRound = 7,
                maxWorkers = 1
            }
        };
        
        // アクションスペースを実際に作成
        foreach (var config in additionalSpaces)
        {
            CreateActionSpace(config);
        }
        
        Debug.Log($"追加アクションスペース数: {additionalSpaces.Count}個");
    }
    
    void CreateActionSpace(ActionSpaceConfig config)
    {
        GameObject spaceObj = new GameObject($"ActionSpace_{config.actionName}");
        ActionSpace actionSpace = spaceObj.AddComponent<ActionSpace>();
        
        // 基本設定
        actionSpace.actionName = config.actionName;
        actionSpace.actionType = config.actionType;
        actionSpace.description = config.description;
        actionSpace.maxWorkers = config.maxWorkers;
        
        // リソース報酬の設定
        if (config.resourceRewards != null)
        {
            actionSpace.resourceRewards = new List<ResourceReward>();
            foreach (var reward in config.resourceRewards)
            {
                actionSpace.resourceRewards.Add(new ResourceReward
                {
                    resourceType = reward.Key,
                    amount = reward.Value
                });
            }
        }
        
        // 要件の設定
        if (config.requirements != null)
        {
            actionSpace.resourceRequirements = new List<ResourceRequirement>();
            foreach (var requirement in config.requirements)
            {
                actionSpace.resourceRequirements.Add(new ResourceRequirement
                {
                    resourceType = requirement.Key,
                    amount = requirement.Value
                });
            }
        }
        
        // ラウンド制御の設定
        if (config.isAvailableFromRound > 1)
        {
            spaceObj.SetActive(false); // 初期は非アクティブ
            
            // ActionSpaceManagerに登録
            if (actionSpaceManager != null)
            {
                actionSpaceManager.RegisterDelayedActionSpace(actionSpace, config.isAvailableFromRound);
            }
        }
        
        Debug.Log($"  追加: {config.actionName} (R{config.isAvailableFromRound}～, 最大{config.maxWorkers}人)");
    }
    
    [System.Serializable]
    public class ActionSpaceConfig
    {
        public string actionName;
        public ActionType actionType;
        public string description;
        public int maxWorkers = 1;
        public int isAvailableFromRound = 1;
        public Dictionary<ResourceType, int> resourceRewards;
        public Dictionary<ResourceType, int> requirements;
    }
    
    // 5人プレイ時の特別ルール適用
    public void ApplyFivePlayerRules()
    {
        Debug.Log("📏 5人プレイ特別ルール適用");
        
        // 1. 食料供給圧力の軽減
        AdjustFoodPressure();
        
        // 2. 追加のスタートプレイヤーメリット
        EnhanceStartPlayerBenefits();
        
        Debug.Log("  注意: 収容人数は基本1人固定（カード効果での例外のみ）");
    }
    
    void AdjustFoodPressure()
    {
        // 5人プレイでは食料圧力を軽減するため、一部のアクションで食料ボーナス追加
        ActionSpace[] foodSpaces = System.Array.FindAll(
            FindObjectsOfType<ActionSpace>(),
            space => space.actionName.Contains("日雇い") || space.actionName.Contains("釣り")
        );
        
        foreach (ActionSpace space in foodSpaces)
        {
            // 食料関連アクションの報酬を少し増加
            foreach (var reward in space.resourceRewards)
            {
                if (reward.resourceType == ResourceType.Food)
                {
                    reward.amount += 1; // 食料報酬+1
                    Debug.Log($"  {space.actionName}: 食料報酬を{reward.amount}に増加");
                }
            }
        }
    }
    
    void EnhanceStartPlayerBenefits()
    {
        ActionSpace startPlayerSpace = System.Array.Find(
            FindObjectsOfType<ActionSpace>(),
            space => space.actionName == "スタートプレイヤー"
        );
        
        if (startPlayerSpace != null)
        {
            // スタートプレイヤーアクションに小さなリソースボーナスを追加
            if (startPlayerSpace.resourceRewards == null)
            {
                startPlayerSpace.resourceRewards = new List<ResourceReward>();
            }
            
            startPlayerSpace.resourceRewards.Add(new ResourceReward
            {
                resourceType = ResourceType.Food,
                amount = 1
            });
            
            Debug.Log("  スタートプレイヤー: 食料1個のボーナス追加");
        }
    }
    
    [ContextMenu("Apply 5-Player Rules")]
    public void ApplyFivePlayerRulesManual()
    {
        ApplyFivePlayerRules();
    }
}