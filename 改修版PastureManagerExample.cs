using UnityEngine;

/// <summary>
/// 改修版PastureManagerの使用例とテスト（4方向対応）
/// </summary>
public class PastureManagerExample : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PastureManager pastureManager;
    [SerializeField] private TileManager tileManager;
    
    [Header("テスト設定")]
    [SerializeField] private bool runTestOnStart = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            // 少し待ってからテストを実行
            Invoke(nameof(RunAllTests), 1.0f);
        }
    }
    
    void Update()
    {
        // キーボードショートカット
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TestBasicFencePlacement();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            TestSquarePasture();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            TestComplexShapes();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            TestPartialFences();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            ClearAllFences();
        }
    }
    
    /// <summary>
    /// すべてのテストを実行
    /// </summary>
    public void RunAllTests()
    {
        Debug.Log("=== 改修版PastureManager テスト開始 ===");
        
        TestBasicFencePlacement();
        Invoke(nameof(TestSquarePasture), 2.0f);
        Invoke(nameof(TestComplexShapes), 4.0f);
        Invoke(nameof(TestPartialFences), 6.0f);
    }
    
    /// <summary>
    /// テスト1: 基本的な柵配置（4方向個別指定）
    /// </summary>
    [ContextMenu("テスト1: 基本的な柵配置")]
    public void TestBasicFencePlacement()
    {
        Debug.Log("=== テスト1: 基本的な柵配置（4方向個別指定） ===");
        
        ClearAllFences();
        
        // タイル(1,1)の各辺に個別に柵を配置
        Debug.Log("タイル(1,1)の各辺に柵を配置:");
        
        pastureManager.AddFence(1, 1, FenceDirection.Top);
        Debug.Log("  - 上辺に柵を配置");
        
        pastureManager.AddFence(1, 1, FenceDirection.Bottom);
        Debug.Log("  - 下辺に柵を配置");
        
        pastureManager.AddFence(1, 1, FenceDirection.Left);
        Debug.Log("  - 左辺に柵を配置");
        
        pastureManager.AddFence(1, 1, FenceDirection.Right);
        Debug.Log("  - 右辺に柵を配置");
        
        Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");
        
        // 各方向の柵の存在確認
        Debug.Log("柵の存在確認:");
        Debug.Log($"  上辺: {pastureManager.HasFence(1, 1, FenceDirection.Top)}");
        Debug.Log($"  下辺: {pastureManager.HasFence(1, 1, FenceDirection.Bottom)}");
        Debug.Log($"  左辺: {pastureManager.HasFence(1, 1, FenceDirection.Left)}");
        Debug.Log($"  右辺: {pastureManager.HasFence(1, 1, FenceDirection.Right)}");
        
        pastureManager.PrintAllPasturesInfo();
    }
    
    /// <summary>
    /// テスト2: 2×2正方形牧場の作成
    /// </summary>
    [ContextMenu("テスト2: 2×2正方形牧場")]
    public void TestSquarePasture()
    {
        Debug.Log("=== テスト2: 2×2正方形牧場の作成 ===");
        
        ClearAllFences();
        
        // 完全に囲まれた2×2牧場を作成
        // タイル: (1,1), (2,1), (1,2), (2,2)
        
        Debug.Log("2×2牧場の境界柵を配置中...");
        
        // 下境界
        pastureManager.AddFence(1, 1, FenceDirection.Bottom);
        pastureManager.AddFence(2, 1, FenceDirection.Bottom);
        Debug.Log("  下境界の柵を配置");
        
        // 上境界
        pastureManager.AddFence(1, 2, FenceDirection.Top);
        pastureManager.AddFence(2, 2, FenceDirection.Top);
        Debug.Log("  上境界の柵を配置");
        
        // 左境界
        pastureManager.AddFence(1, 1, FenceDirection.Left);
        pastureManager.AddFence(1, 2, FenceDirection.Left);
        Debug.Log("  左境界の柵を配置");
        
        // 右境界
        pastureManager.AddFence(2, 1, FenceDirection.Right);
        pastureManager.AddFence(2, 2, FenceDirection.Right);
        Debug.Log("  右境界の柵を配置");
        
        Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");
        Debug.Log("牧場が自動検出されるはずです...");
        
        // 牧場情報を表示
        var pastures = pastureManager.GetAllPastures();
        Debug.Log($"検出された牧場数: {pastures.Count}");
        
        if (pastures.Count > 0)
        {
            var pasture = pastures[0];
            Debug.Log($"牧場面積: {pasture.GetArea()}タイル");
            Debug.Log($"牧場容量: {pasture.capacity}匹");
            
            // 動物を追加してテスト
            pastureManager.AddAnimalToPasture(pasture.id, AnimalType.Sheep, 3);
            pastureManager.AddAnimalToPasture(pasture.id, AnimalType.Cattle, 2);
            Debug.Log("羊3匹と牛2匹を追加しました");
        }
        
        pastureManager.PrintAllPasturesInfo();
    }
    
    /// <summary>
    /// テスト3: 複雑な形状の牧場
    /// </summary>
    [ContextMenu("テスト3: 複雑な形状")]
    public void TestComplexShapes()
    {
        Debug.Log("=== テスト3: 複雑な形状の牧場（L字型） ===");
        
        ClearAllFences();
        
        // L字型の牧場を作成
        // ■■□
        // ■□□
        // タイル: (0,0), (1,0), (0,1)
        
        Debug.Log("L字型牧場の境界柵を配置中...");
        
        // タイル(0,0)の境界
        pastureManager.AddFence(0, 0, FenceDirection.Bottom);  // 下辺
        pastureManager.AddFence(0, 0, FenceDirection.Left);    // 左辺
        pastureManager.AddFence(0, 0, FenceDirection.Top);     // 上辺
        
        // タイル(1,0)の境界
        pastureManager.AddFence(1, 0, FenceDirection.Bottom);  // 下辺
        pastureManager.AddFence(1, 0, FenceDirection.Right);   // 右辺
        pastureManager.AddFence(1, 0, FenceDirection.Top);     // 上辺
        
        // タイル(0,1)の境界
        pastureManager.AddFence(0, 1, FenceDirection.Left);    // 左辺
        pastureManager.AddFence(0, 1, FenceDirection.Top);     // 上辺
        pastureManager.AddFence(0, 1, FenceDirection.Right);   // 右辺
        
        Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");
        
        var pastures = pastureManager.GetAllPastures();
        Debug.Log($"検出された牧場数: {pastures.Count}");
        
        pastureManager.PrintAllPasturesInfo();
    }
    
    /// <summary>
    /// テスト4: 部分的な柵配置（牧場にならない例）
    /// </summary>
    [ContextMenu("テスト4: 部分的な柵")]
    public void TestPartialFences()
    {
        Debug.Log("=== テスト4: 部分的な柵配置（牧場にならない例） ===");
        
        ClearAllFences();
        
        // 完全に囲まれていない場合のテスト
        Debug.Log("不完全な囲いを作成中（牧場にならないはず）...");
        
        // タイル(1,1)の一部だけに柵を配置
        pastureManager.AddFence(1, 1, FenceDirection.Top);     // 上辺のみ
        pastureManager.AddFence(1, 1, FenceDirection.Left);    // 左辺のみ
        
        Debug.Log("上辺と左辺のみに柵を配置");
        Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");
        
        var pastures = pastureManager.GetAllPastures();
        Debug.Log($"検出された牧場数: {pastures.Count}（0になるはず）");
        
        // 追加で右辺と下辺も配置して完全な囲いにする
        Debug.Log("右辺と下辺を追加して完全な囲いにします...");
        pastureManager.AddFence(1, 1, FenceDirection.Right);   // 右辺
        pastureManager.AddFence(1, 1, FenceDirection.Bottom);  // 下辺
        
        pastures = pastureManager.GetAllPastures();
        Debug.Log($"完全な囲い後の牧場数: {pastures.Count}（1になるはず）");
        
        pastureManager.PrintAllPasturesInfo();
    }
    
    /// <summary>
    /// テスト5: 柵の削除テスト
    /// </summary>
    [ContextMenu("テスト5: 柵の削除")]
    public void TestFenceRemoval()
    {
        Debug.Log("=== テスト5: 柵の削除テスト ===");
        
        // まず2×2牧場を作成
        TestSquarePasture();
        
        Debug.Log("牧場を作成した後、一部の柵を削除します...");
        
        // 上辺の一部を削除
        pastureManager.RemoveFence(1, 2, FenceDirection.Top);
        Debug.Log("タイル(1,2)の上辺柵を削除");
        
        var pastures = pastureManager.GetAllPastures();
        Debug.Log($"削除後の牧場数: {pastures.Count}（0になるはず - 囲いが不完全）");
        
        pastureManager.PrintAllPasturesInfo();
    }
    
    /// <summary>
    /// すべての柵をクリア
    /// </summary>
    [ContextMenu("すべての柵をクリア")]
    public void ClearAllFences()
    {
        Debug.Log("すべての柵をクリアします...");
        
        var allFences = pastureManager.GetAllFences();
        foreach (var fence in allFences)
        {
            pastureManager.RemoveFence(fence.position, fence.direction);
        }
        
        Debug.Log("柵のクリア完了");
    }
    
    /// <summary>
    /// 現在の状況を表示
    /// </summary>
    [ContextMenu("現在の状況を表示")]
    public void ShowCurrentStatus()
    {
        Debug.Log("=== 現在の状況 ===");
        pastureManager.PrintAllPasturesInfo();
    }
    
    /// <summary>
    /// デモンストレーション：段階的な牧場構築
    /// </summary>
    [ContextMenu("デモ: 段階的牧場構築")]
    public void DemonstrateStepByStepConstruction()
    {
        Debug.Log("=== デモ: 段階的な牧場構築 ===");
        
        ClearAllFences();
        
        Debug.Log("ステップ1: タイル(2,2)の上辺に柵を配置");
        pastureManager.AddFence(2, 2, FenceDirection.Top);
        
        Debug.Log("ステップ2: タイル(2,2)の右辺に柵を配置");
        pastureManager.AddFence(2, 2, FenceDirection.Right);
        
        Debug.Log("ステップ3: タイル(2,2)の下辺に柵を配置");
        pastureManager.AddFence(2, 2, FenceDirection.Bottom);
        
        Debug.Log("ステップ4: タイル(2,2)の左辺に柵を配置（完全な囲いが完成）");
        pastureManager.AddFence(2, 2, FenceDirection.Left);
        
        Debug.Log("1×1牧場が完成しました！");
        pastureManager.PrintAllPasturesInfo();
    }
}