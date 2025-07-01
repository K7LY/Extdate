using UnityEngine;

public class FivePlayerGameTestRunner : MonoBehaviour
{
    [Header("テスト実行設定")]
    public bool runOnStart = true;
    public bool createRequiredComponents = true;
    
    void Start()
    {
        if (runOnStart)
        {
            SetupAndRunTest();
        }
    }
    
    [ContextMenu("Setup and Run 5-Player Test")]
    public void SetupAndRunTest()
    {
        Debug.Log("🚀 5人プレイテストランナー開始");
        
        if (createRequiredComponents)
        {
            CreateRequiredComponents();
        }
        
        // FivePlayerGameTestコンポーネントを追加して実行
        if (GetComponent<FivePlayerGameTest>() == null)
        {
            gameObject.AddComponent<FivePlayerGameTest>();
        }
        
        Debug.Log("✅ 5人プレイテスト準備完了");
    }
    
    private void CreateRequiredComponents()
    {
        Debug.Log("🔧 必要なコンポーネントを作成中...");
        
        // GameManagerの作成
        if (FindObjectOfType<GameManager>() == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
            Debug.Log("  GameManager作成完了");
        }
        
        // GameSetupの作成
        if (FindObjectOfType<GameSetup>() == null)
        {
            GameObject gameSetupObj = new GameObject("GameSetup");
            gameSetupObj.AddComponent<GameSetup>();
            Debug.Log("  GameSetup作成完了");
        }
        
        // ActionSpaceManagerの作成
        if (FindObjectOfType<ActionSpaceManager>() == null)
        {
            GameObject actionSpaceManagerObj = new GameObject("ActionSpaceManager");
            actionSpaceManagerObj.AddComponent<ActionSpaceManager>();
            Debug.Log("  ActionSpaceManager作成完了");
        }
        
        // ResourceConverterの作成
        if (FindObjectOfType<ResourceConverter>() == null)
        {
            GameObject resourceConverterObj = new GameObject("ResourceConverter");
            resourceConverterObj.AddComponent<ResourceConverter>();
            Debug.Log("  ResourceConverter作成完了");
        }
        
        // FivePlayerActionSpaceSetupの作成
        if (FindObjectOfType<FivePlayerActionSpaceSetup>() == null)
        {
            GameObject fivePlayerSetupObj = new GameObject("FivePlayerActionSpaceSetup");
            fivePlayerSetupObj.AddComponent<FivePlayerActionSpaceSetup>();
            Debug.Log("  FivePlayerActionSpaceSetup作成完了");
        }
        
        Debug.Log("✅ 必要なコンポーネント作成完了");
    }
}