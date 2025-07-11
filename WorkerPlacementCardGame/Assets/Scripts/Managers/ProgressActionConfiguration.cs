using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 進歩を出すアクションの設定と管理システム
/// 様々な種類の進歩アクションを定義し、効果を設定する
/// </summary>
[CreateAssetMenu(fileName = "ProgressActionConfiguration", menuName = "GameData/Progress Action Configuration")]
public class ProgressActionConfiguration : ScriptableObject
{
    [Header("進歩アクション設定")]
    [SerializeField] private List<ProgressActionSetup> progressActions = new List<ProgressActionSetup>();
    
    [Header("デバッグ設定")]
    [SerializeField] private bool enableDebugLogs = true;
    
    /// <summary>
    /// 設定されている進歩アクションを取得
    /// </summary>
    /// <returns>進歩アクション設定のリスト</returns>
    public List<ProgressActionSetup> GetProgressActions()
    {
        return new List<ProgressActionSetup>(progressActions);
    }
    
    /// <summary>
    /// 特定の進歩アクション設定を取得
    /// </summary>
    /// <param name="actionName">アクション名</param>
    /// <returns>該当する進歩アクション設定</returns>
    public ProgressActionSetup GetProgressAction(string actionName)
    {
        return progressActions.FirstOrDefault(action => action.actionName == actionName);
    }
    
    /// <summary>
    /// デフォルトの進歩アクション設定を作成
    /// </summary>
    [ContextMenu("Create Default Progress Actions")]
    public void CreateDefaultProgressActions()
    {
        progressActions.Clear();
        
        // 小さい進歩専用アクション
        var minorProgressAction = new ProgressActionSetup
        {
            actionName = "小さな進歩",
            actionId = "minor_improvement_action",
            progressType = ProgressActionType.MinorImprovement,
            maxCardsPerUse = 1,
            description = "小さい進歩カードを1枚プレイできます",
            effects = new List<ActionEffect>
            {
                new ActionEffect(ActionEffectType.PlayMinorImprovement, ResourceType.Wood, 1)
            }
        };
        progressActions.Add(minorProgressAction);
        
        // 大きい進歩専用アクション
        var majorProgressAction = new ProgressActionSetup
        {
            actionName = "大きな進歩",
            actionId = "major_improvement_action",
            progressType = ProgressActionType.MajorImprovement,
            maxCardsPerUse = 1,
            description = "大きい進歩カードを1枚プレイできます",
            effects = new List<ActionEffect>
            {
                new ActionEffect(ActionEffectType.PlayMajorImprovement, ResourceType.Wood, 1)
            }
        };
        progressActions.Add(majorProgressAction);
        
        // 職業＋小進歩の複合アクション
        var occupationMinorAction = new ProgressActionSetup
        {
            actionName = "職業・小進歩",
            actionId = "occupation_minor_action",
            progressType = ProgressActionType.Mixed,
            maxCardsPerUse = 1,
            allowPlayerChoice = true,
            description = "職業カードまたは小さい進歩カードを1枚プレイできます",
            effects = new List<ActionEffect>
            {
                new ActionEffect(ActionEffectType.PlayOccupation, ResourceType.Wood, 1),
                new ActionEffect(ActionEffectType.PlayMinorImprovement, ResourceType.Wood, 1)
            }
        };
        progressActions.Add(occupationMinorAction);
        
        // 家族成長＋小進歩の複合アクション
        var familyGrowthMinorAction = new ProgressActionSetup
        {
            actionName = "家族の成長・小進歩",
            actionId = "family_growth_minor_action",
            progressType = ProgressActionType.FamilyGrowthWithImprovement,
            maxCardsPerUse = 1,
            description = "家族を1人増やし、小さい進歩カードを1枚プレイできます",
            effects = new List<ActionEffect>
            {
                new ActionEffect(ActionEffectType.FamilyGrowth, ResourceType.Wood, 1),
                new ActionEffect(ActionEffectType.PlayMinorImprovement, ResourceType.Wood, 1)
            }
        };
        progressActions.Add(familyGrowthMinorAction);
        
        // リソース獲得＋進歩の複合アクション
        var resourceProgressAction = new ProgressActionSetup
        {
            actionName = "改築・大進歩",
            actionId = "renovation_major_action",
            progressType = ProgressActionType.RenovationWithImprovement,
            maxCardsPerUse = 1,
            description = "住居を改築し、大きい進歩カードを1枚プレイできます",
            effects = new List<ActionEffect>
            {
                new ActionEffect(ActionEffectType.HouseRenovation, ResourceType.Clay, 1),
                new ActionEffect(ActionEffectType.PlayMajorImprovement, ResourceType.Wood, 1)
            }
        };
        progressActions.Add(resourceProgressAction);
        
        DebugLog("デフォルトの進歩アクション設定を作成しました");
    }
    
    /// <summary>
    /// 進歩アクションをActionSpaceに適用
    /// </summary>
    /// <param name="actionSpace">適用先のActionSpace</param>
    /// <param name="actionName">進歩アクション名</param>
    public void ApplyProgressActionToActionSpace(ActionSpace actionSpace, string actionName)
    {
        var progressAction = GetProgressAction(actionName);
        if (progressAction == null)
        {
            DebugLog($"進歩アクション'{actionName}'が見つかりません");
            return;
        }
        
        // ActionSpaceに効果を設定
        actionSpace.actionName = progressAction.actionName;
        actionSpace.description = progressAction.description;
        actionSpace.actionType = ActionType.PlayImprovement;
        
        // 効果をActionSpaceに追加
        actionSpace.coreEffects.Clear();
        actionSpace.coreEffects.AddRange(progressAction.effects);
        
        DebugLog($"進歩アクション'{actionName}'をActionSpaceに適用しました");
    }
    
    /// <summary>
    /// 進歩アクションの効果を実行
    /// </summary>
    /// <param name="actionName">進歩アクション名</param>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="improvementManager">ImprovementManager</param>
    /// <returns>実行に成功した場合true</returns>
    public bool ExecuteProgressAction(string actionName, Player player, ImprovementManager improvementManager)
    {
        var progressAction = GetProgressAction(actionName);
        if (progressAction == null)
        {
            DebugLog($"進歩アクション'{actionName}'が見つかりません");
            return false;
        }
        
        DebugLog($"=== 進歩アクション実行: {actionName} ===");
        DebugLog($"プレイヤー: {player.playerName}");
        
        try
        {
            switch (progressAction.progressType)
            {
                case ProgressActionType.MinorImprovement:
                    return ExecuteMinorImprovementAction(progressAction, player, improvementManager);
                    
                case ProgressActionType.MajorImprovement:
                    return ExecuteMajorImprovementAction(progressAction, player, improvementManager);
                    
                case ProgressActionType.Mixed:
                    return ExecuteMixedAction(progressAction, player, improvementManager);
                    
                case ProgressActionType.FamilyGrowthWithImprovement:
                    return ExecuteFamilyGrowthWithImprovementAction(progressAction, player, improvementManager);
                    
                case ProgressActionType.RenovationWithImprovement:
                    return ExecuteRenovationWithImprovementAction(progressAction, player, improvementManager);
                    
                default:
                    DebugLog($"未対応の進歩アクションタイプ: {progressAction.progressType}");
                    return false;
            }
        }
        catch (System.Exception ex)
        {
            DebugLog($"進歩アクション実行中にエラーが発生しました: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 小さい進歩アクションを実行
    /// </summary>
    private bool ExecuteMinorImprovementAction(ProgressActionSetup action, Player player, ImprovementManager improvementManager)
    {
        DebugLog("小さい進歩アクション実行");
        improvementManager.PlayImprovement(player, true, false, action.maxCardsPerUse);
        return true;
    }
    
    /// <summary>
    /// 大きい進歩アクションを実行
    /// </summary>
    private bool ExecuteMajorImprovementAction(ProgressActionSetup action, Player player, ImprovementManager improvementManager)
    {
        DebugLog("大きい進歩アクション実行");
        improvementManager.PlayImprovement(player, false, true, action.maxCardsPerUse);
        return true;
    }
    
    /// <summary>
    /// 職業・小進歩の複合アクションを実行
    /// </summary>
    private bool ExecuteMixedAction(ProgressActionSetup action, Player player, ImprovementManager improvementManager)
    {
        DebugLog("職業・小進歩複合アクション実行");
        
        if (action.allowPlayerChoice)
        {
            // プレイヤーに選択肢を提示（UIが実装されるまでは小さい進歩のみ対応）
            DebugLog("職業カードと小さい進歩の選択が可能ですが、現在は小さい進歩のみ実装されています");
            improvementManager.PlayImprovement(player, true, false, action.maxCardsPerUse);
        }
        else
        {
            // 選択肢なしの場合は小さい進歩をプレイ
            improvementManager.PlayImprovement(player, true, false, action.maxCardsPerUse);
        }
        
        return true;
    }
    
    /// <summary>
    /// 家族成長＋小進歩アクションを実行
    /// </summary>
    private bool ExecuteFamilyGrowthWithImprovementAction(ProgressActionSetup action, Player player, ImprovementManager improvementManager)
    {
        DebugLog("家族成長＋小進歩アクション実行");
        
        // 1. 家族成長を実行
        bool familyGrowthSuccess = player.GrowFamily();
        if (familyGrowthSuccess)
        {
            DebugLog($"{player.playerName}の家族が増えました");
        }
        else
        {
            DebugLog($"{player.playerName}は家族を増やせません（部屋不足）");
        }
        
        // 2. 小さい進歩カードをプレイ
        improvementManager.PlayImprovement(player, true, false, action.maxCardsPerUse);
        
        return true; // 小さい進歩は家族成長の成否に関係なく実行
    }
    
    /// <summary>
    /// 改築＋大進歩アクションを実行
    /// </summary>
    private bool ExecuteRenovationWithImprovementAction(ProgressActionSetup action, Player player, ImprovementManager improvementManager)
    {
        DebugLog("改築＋大進歩アクション実行");
        
        // 1. 住居改築を実行
        Player.HouseType currentType = player.GetHouseType();
        Player.HouseType nextType = currentType == Player.HouseType.Wood ? 
            Player.HouseType.Clay : Player.HouseType.Stone;
            
        bool renovationSuccess = player.RenovateHouse(nextType);
        if (renovationSuccess)
        {
            DebugLog($"{player.playerName}が住居を改築しました: {currentType} → {nextType}");
        }
        else
        {
            DebugLog($"{player.playerName}は住居を改築できません");
        }
        
        // 2. 大きい進歩カードをプレイ
        improvementManager.PlayImprovement(player, false, true, action.maxCardsPerUse);
        
        return true; // 大きい進歩は改築の成否に関係なく実行
    }
    
    /// <summary>
    /// 設定の妥当性をチェック
    /// </summary>
    [ContextMenu("Validate Configuration")]
    public void ValidateConfiguration()
    {
        DebugLog("=== 進歩アクション設定の妥当性チェック ===");
        
        int validActions = 0;
        int invalidActions = 0;
        
        foreach (var action in progressActions)
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(action.actionName))
            {
                DebugLog($"❌ 無効: アクション名が空です");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(action.actionId))
            {
                DebugLog($"❌ 無効: アクションID({action.actionName})が空です");
                isValid = false;
            }
            
            if (action.effects == null || action.effects.Count == 0)
            {
                DebugLog($"❌ 無効: 効果({action.actionName})が設定されていません");
                isValid = false;
            }
            
            if (action.maxCardsPerUse <= 0)
            {
                DebugLog($"⚠️  警告: 最大カード数({action.actionName})が0以下です");
            }
            
            if (isValid)
            {
                validActions++;
                DebugLog($"✅ 有効: {action.actionName}");
            }
            else
            {
                invalidActions++;
            }
        }
        
        DebugLog($"=== チェック完了: 有効{validActions}個, 無効{invalidActions}個 ===");
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ProgressActionConfig] {message}");
        }
    }
}

/// <summary>
/// 進歩アクションの設定データ
/// </summary>
[System.Serializable]
public class ProgressActionSetup
{
    [Header("基本情報")]
    public string actionName;
    public string actionId;
    public ProgressActionType progressType;
    
    [Header("プレイ設定")]
    public int maxCardsPerUse = 1;
    public bool allowPlayerChoice = false;
    public bool requireResources = true;
    
    [Header("説明")]
    [TextArea(2, 4)]
    public string description;
    
    [Header("効果")]
    public List<ActionEffect> effects = new List<ActionEffect>();
    
    [Header("条件")]
    public List<ProgressActionCondition> conditions = new List<ProgressActionCondition>();
}

/// <summary>
/// 進歩アクションの種類
/// </summary>
public enum ProgressActionType
{
    MinorImprovement,                    // 小さい進歩のみ
    MajorImprovement,                    // 大きい進歩のみ
    Mixed,                               // 職業・小進歩の選択
    FamilyGrowthWithImprovement,         // 家族成長＋進歩
    RenovationWithImprovement,           // 改築＋進歩
    ResourceGainWithImprovement,         // リソース獲得＋進歩
    SpecialProgressAction                // 特殊な進歩アクション
}

/// <summary>
/// 進歩アクションの実行条件
/// </summary>
[System.Serializable]
public class ProgressActionCondition
{
    [Header("条件設定")]
    public ProgressConditionType conditionType;
    public ResourceType requiredResource;
    public int requiredAmount;
    
    [Header("説明")]
    public string description;
    
    /// <summary>
    /// 条件をチェック
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <returns>条件を満たしている場合true</returns>
    public bool CheckCondition(Player player)
    {
        switch (conditionType)
        {
            case ProgressConditionType.HasResource:
                return player.GetResource(requiredResource) >= requiredAmount;
                
            case ProgressConditionType.HasRooms:
                return player.GetAvailableRooms() >= requiredAmount;
                
            case ProgressConditionType.HasFamilyMembers:
                return player.GetFamilySize() >= requiredAmount;
                
            case ProgressConditionType.HasImprovements:
                return player.GetImprovements().Count >= requiredAmount;
                
            default:
                return true;
        }
    }
}

/// <summary>
/// 進歩アクションの条件種別
/// </summary>
public enum ProgressConditionType
{
    None,                   // 条件なし
    HasResource,            // リソース保有
    HasRooms,              // 部屋数
    HasFamilyMembers,      // 家族数
    HasImprovements,       // 進歩カード数
    HasOccupations         // 職業カード数
}