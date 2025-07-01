using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ActionSpaceManager : MonoBehaviour
{
    [Header("段階的解放設定")]
    [SerializeField] private List<ActionSpacePhase> phases = new List<ActionSpacePhase>();
    
    private List<ActionSpace> allActionSpaces = new List<ActionSpace>();
    private HashSet<ActionSpace> activeActionSpaces = new HashSet<ActionSpace>();
    
    void Start()
    {
        InitializeActionSpaces();
        SetupPhases();
    }
    
    private void InitializeActionSpaces()
    {
        allActionSpaces = FindObjectsOfType<ActionSpace>().ToList();
        Debug.Log($"発見されたアクションスペース: {allActionSpaces.Count}個");
        
        // 初期は全て非アクティブに
        foreach (ActionSpace space in allActionSpaces)
        {
            space.gameObject.SetActive(false);
        }
    }
    
    private void SetupPhases()
    {
        phases.Clear();
        
        // フェーズ1: 初期アクションスペース（ラウンド1から利用可能）
        phases.Add(new ActionSpacePhase
        {
            startRound = 1,
            actionSpaceNames = new List<string>
            {
                "森", "土取り場", "葦の沼", "漁場", "日雇い労働者",
                "穀物の種", "畑を耕す", "スタートプレイヤー", "住居の拡張"
            }
        });
        
        // フェーズ2: 基本発展（ラウンド2から）
        phases.Add(new ActionSpacePhase
        {
            startRound = 2,
            actionSpaceNames = new List<string>
            {
                "柵の建設", "羊市場"
            }
        });
        
        // フェーズ3: 農業発展（ラウンド3から）
        phases.Add(new ActionSpacePhase
        {
            startRound = 3,
            actionSpaceNames = new List<string>
            {
                "種まきと製パン"
            }
        });
        
        // フェーズ4: 家族成長（ラウンド5から）
        phases.Add(new ActionSpacePhase
        {
            startRound = 5,
            actionSpaceNames = new List<string>
            {
                "家族の成長"
            }
        });
        
        // フェーズ5: 住居改築（ラウンド6から）
        phases.Add(new ActionSpacePhase
        {
            startRound = 6,
            actionSpaceNames = new List<string>
            {
                "住居の改築"
            }
        });
        
        // フェーズ6: 高級動物（ラウンド8から）
        phases.Add(new ActionSpacePhase
        {
            startRound = 8,
            actionSpaceNames = new List<string>
            {
                "猪市場"
            }
        });
        
        // フェーズ7: 最高級動物（ラウンド10から）
        phases.Add(new ActionSpacePhase
        {
            startRound = 10,
            actionSpaceNames = new List<string>
            {
                "牛市場"
            }
        });
    }
    
    public void ActivateActionSpacesForRound(int round)
    {
        Debug.Log($"ラウンド {round} のアクションスペースを解放中...");
        
        foreach (ActionSpacePhase phase in phases)
        {
            if (phase.startRound == round)
            {
                foreach (string spaceName in phase.actionSpaceNames)
                {
                    ActionSpace space = allActionSpaces.FirstOrDefault(s => s.actionName == spaceName);
                    if (space != null && !activeActionSpaces.Contains(space))
                    {
                        space.gameObject.SetActive(true);
                        activeActionSpaces.Add(space);
                        Debug.Log($"  📍 {spaceName} を解放しました");
                    }
                }
            }
        }
    }
    
    public List<ActionSpace> GetActiveActionSpaces()
    {
        return activeActionSpaces.ToList();
    }
    
    public bool IsActionSpaceActive(ActionSpace actionSpace)
    {
        return activeActionSpaces.Contains(actionSpace);
    }
    
    // アクションスペースにリソースを補充
    public void ReplenishActionSpaces()
    {
        foreach (ActionSpace space in activeActionSpaces)
        {
            ReplenishActionSpace(space);
        }
    }
    
    private void ReplenishActionSpace(ActionSpace space)
    {
        // アクションスペースのタイプに応じてリソースを補充
        switch (space.actionName)
        {
            case "森":
                // 森は毎ラウンド3木材が追加される
                space.resourceGain[ResourceType.Wood] += 3;
                break;
            case "土取り場":
                space.resourceGain[ResourceType.Clay] += 1;
                break;
            case "葦の沼":
                space.resourceGain[ResourceType.Reed] += 1;
                break;
            case "漁場":
                space.resourceGain[ResourceType.Food] += 1;
                break;
            case "羊市場":
                space.resourceGain[ResourceType.Sheep] += 1;
                break;
            case "猪市場":
                space.resourceGain[ResourceType.Boar] += 1;
                break;
            case "牛市場":
                space.resourceGain[ResourceType.Cattle] += 1;
                break;
        }
    }
    
    // デバッグ用：全アクションスペースの状態を表示
    [ContextMenu("Show Action Space Status")]
    public void ShowActionSpaceStatus()
    {
        Debug.Log("=== アクションスペース状態 ===");
        Debug.Log($"総数: {allActionSpaces.Count}, アクティブ: {activeActionSpaces.Count}");
        
        foreach (ActionSpace space in allActionSpaces)
        {
            string status = activeActionSpaces.Contains(space) ? "🟢 アクティブ" : "🔴 非アクティブ";
            Debug.Log($"{space.actionName}: {status}");
        }
    }
}

[System.Serializable]
public class ActionSpacePhase
{
    public int startRound;
    public List<string> actionSpaceNames = new List<string>();
}