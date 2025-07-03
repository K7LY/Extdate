using UnityEngine;
using System.Collections;

/// <summary>
/// PastureManagerの使用例とテストを示すクラス
/// </summary>
public class PastureManagerExample : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private TileManager tileManager;
    [SerializeField] private PastureManager pastureManager;
    
    [Header("テスト設定")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool enableVisualDebug = true;
    
    void Start()
    {
        if (runTestOnStart)
        {
            StartCoroutine(RunExampleTest());
        }
    }
    
    /// <summary>
    /// 牧場管理システムのテストと使用例
    /// </summary>
    private IEnumerator RunExampleTest()
    {
        yield return new WaitForSeconds(1f); // TileManagerの初期化を待つ
        
        Debug.Log("=== 牧場管理システム テスト開始 ===");
        
        // 基本的な柵の配置テスト
        yield return StartCoroutine(TestBasicFencePlacement());
        yield return new WaitForSeconds(1f);
        
        // 正方形の牧場作成テスト
        yield return StartCoroutine(TestSquarePasture());
        yield return new WaitForSeconds(1f);
        
        // L字型の牧場作成テスト
        yield return StartCoroutine(TestLShapedPasture());
        yield return new WaitForSeconds(1f);
        
        // 動物管理テスト
        yield return StartCoroutine(TestAnimalManagement());
        yield return new WaitForSeconds(1f);
        
        // 複数牧場テスト
        yield return StartCoroutine(TestMultiplePastures());
        
        Debug.Log("=== 牧場管理システム テスト完了 ===");
    }
    
    /// <summary>
    /// 基本的な柵の配置テスト
    /// </summary>
    private IEnumerator TestBasicFencePlacement()
    {
        Debug.Log("--- 基本的な柵の配置テスト ---");
        
        // 水平柵の配置
        bool success1 = pastureManager.AddFence(1, 1, FenceDirection.Horizontal);
        Debug.Log($"水平柵の配置: {(success1 ? "成功" : "失敗")}");
        
        // 垂直柵の配置
        bool success2 = pastureManager.AddFence(1, 1, FenceDirection.Vertical);
        Debug.Log($"垂直柵の配置: {(success2 ? "成功" : "失敗")}");
        
        // 同じ柵の重複配置テスト
        bool success3 = pastureManager.AddFence(1, 1, FenceDirection.Horizontal);
        Debug.Log($"重複柵の配置: {(success3 ? "成功" : "失敗")} (失敗が正しい)");
        
        // 柵の存在確認
        bool exists1 = pastureManager.HasFence(1, 1, FenceDirection.Horizontal);
        bool exists2 = pastureManager.HasFence(1, 1, FenceDirection.Vertical);
        bool exists3 = pastureManager.HasFence(2, 2, FenceDirection.Horizontal);
        
        Debug.Log($"柵の存在確認 - 水平: {exists1}, 垂直: {exists2}, 存在しない柵: {exists3}");
        
        // 柵の削除
        bool removed = pastureManager.RemoveFence(1, 1, FenceDirection.Horizontal);
        Debug.Log($"柵の削除: {(removed ? "成功" : "失敗")}");
        
        yield return null;
    }
    
    /// <summary>
    /// 正方形の牧場作成テスト
    /// </summary>
    private IEnumerator TestSquarePasture()
    {
        Debug.Log("--- 正方形の牧場作成テスト ---");
        
        // 2x2の正方形牧場を作成
        // タイル座標: (1,1), (2,1), (1,2), (2,2)
        // 
        //   0   1   2   3
        // 0 +---+---+---+
        //   |   |   |   |
        // 1 +---+===+===+---+
        //   |   ‖ ■ | ■ ‖   |  ← 牧場エリア
        // 2 +---+===+===+---+
        //   |   ‖ ■ | ■ ‖   |
        // 3 +---+===+===+---+
        //   |   |   |   |
        // 4 +---+---+---+
        
        Debug.Log("配置する柵の詳細:");
        
        // 水平柵（上下の境界）
        bool fence1 = pastureManager.AddFence(1, 1, FenceDirection.Horizontal);  // タイル(1,1)の下境界
        Debug.Log($"水平柵1 (1,1)下境界: {fence1}");
        
        bool fence2 = pastureManager.AddFence(2, 1, FenceDirection.Horizontal);  // タイル(2,1)の下境界
        Debug.Log($"水平柵2 (2,1)下境界: {fence2}");
        
        bool fence3 = pastureManager.AddFence(1, 3, FenceDirection.Horizontal);  // タイル(1,2)の上境界
        Debug.Log($"水平柵3 (1,3)上境界: {fence3}");
        
        bool fence4 = pastureManager.AddFence(2, 3, FenceDirection.Horizontal);  // タイル(2,2)の上境界
        Debug.Log($"水平柵4 (2,3)上境界: {fence4}");
        
        // 垂直柵（左右の境界）
        bool fence5 = pastureManager.AddFence(0, 1, FenceDirection.Vertical);    // タイル(1,1)の左境界
        Debug.Log($"垂直柵5 (0,1)左境界: {fence5}");
        
        bool fence6 = pastureManager.AddFence(0, 2, FenceDirection.Vertical);    // タイル(1,2)の左境界
        Debug.Log($"垂直柵6 (0,2)左境界: {fence6}");
        
        bool fence7 = pastureManager.AddFence(2, 1, FenceDirection.Vertical);    // タイル(2,1)の右境界
        Debug.Log($"垂直柵7 (2,1)右境界: {fence7}");
        
        bool fence8 = pastureManager.AddFence(2, 2, FenceDirection.Vertical);    // タイル(2,2)の右境界
        Debug.Log($"垂直柵8 (2,2)右境界: {fence8}");
        
        Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");
        
        // 牧場情報を表示
        var pastures = pastureManager.GetAllPastures();
        Debug.Log($"検出された牧場数: {pastures.Count}");
        
        if (pastures.Count > 0)
        {
            var pasture = pastures[0];
            Debug.Log($"牧場1 - 面積: {pasture.GetArea()}タイル, 容量: {pasture.capacity}匹");
            pastureManager.PrintPastureInfo(pasture.id);
        }
        else
        {
            Debug.LogWarning("牧場が検出されませんでした！境界が完全に閉じられていない可能性があります。");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// L字型の牧場作成テスト
    /// </summary>
    private IEnumerator TestLShapedPasture()
    {
        Debug.Log("--- L字型の牧場作成テスト ---");
        
        // 既存の牧場をクリア
        ClearAllFences();
        
        // L字型の牧場を作成
        // ■■□
        // ■□□
        // □□□
        
        // L字の境界柵を配置
        pastureManager.AddFence(5, 5, FenceDirection.Horizontal);  // 下辺1
        pastureManager.AddFence(6, 5, FenceDirection.Vertical);    // 下辺から上
        pastureManager.AddFence(6, 6, FenceDirection.Horizontal);  // 中段
        pastureManager.AddFence(7, 6, FenceDirection.Vertical);    // 中段から上
        pastureManager.AddFence(7, 7, FenceDirection.Horizontal);  // 上辺
        pastureManager.AddFence(5, 7, FenceDirection.Vertical);    // 左辺
        pastureManager.AddFence(5, 6, FenceDirection.Vertical);    // 左下部分
        
        var pastures = pastureManager.GetAllPastures();
        Debug.Log($"L字型牧場 - 検出された牧場数: {pastures.Count}");
        
        foreach (var pasture in pastures)
        {
            pastureManager.PrintPastureInfo(pasture.id);
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 動物管理テスト
    /// </summary>
    private IEnumerator TestAnimalManagement()
    {
        Debug.Log("--- 動物管理テスト ---");
        
        var pastures = pastureManager.GetAllPastures();
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が存在しないため、動物管理テストをスキップします");
            yield break;
        }
        
        var pasture = pastures[0];
        int pastureId = pasture.id;
        
        Debug.Log($"牧場{pastureId}での動物管理テスト開始");
        Debug.Log($"初期容量: {pasture.capacity}匹");
        
        // 羊を追加
        bool added1 = pastureManager.AddAnimalToPasture(pastureId, AnimalType.Sheep, 3);
        Debug.Log($"羊3匹追加: {(added1 ? "成功" : "失敗")}");
        
        // 牛を追加
        bool added2 = pastureManager.AddAnimalToPasture(pastureId, AnimalType.Cattle, 2);
        Debug.Log($"牛2匹追加: {(added2 ? "成功" : "失敗")}");
        
        // 容量オーバーテスト
        bool added3 = pastureManager.AddAnimalToPasture(pastureId, AnimalType.Horse, 10);
        Debug.Log($"馬10匹追加（容量オーバー）: {(added3 ? "成功" : "失敗")} (失敗が正しい)");
        
        // 現在の状態を表示
        pastureManager.PrintPastureInfo(pastureId);
        
        // 動物を削除
        bool removed1 = pastureManager.RemoveAnimalFromPasture(pastureId, AnimalType.Sheep, 1);
        Debug.Log($"羊1匹削除: {(removed1 ? "成功" : "失敗")}");
        
        // 最終状態を表示
        pastureManager.PrintPastureInfo(pastureId);
        
        yield return null;
    }
    
    /// <summary>
    /// 複数牧場テスト
    /// </summary>
    private IEnumerator TestMultiplePastures()
    {
        Debug.Log("--- 複数牧場テスト ---");
        
        // 既存の牧場をクリア
        ClearAllFences();
        
        // 第1牧場 (2x2) - 正しい8個の柵で囲む
        Debug.Log("第1牧場 (2x2) を作成中...");
        pastureManager.AddFence(1, 1, FenceDirection.Horizontal);  // 下境界1
        pastureManager.AddFence(2, 1, FenceDirection.Horizontal);  // 下境界2
        pastureManager.AddFence(1, 3, FenceDirection.Horizontal);  // 上境界1
        pastureManager.AddFence(2, 3, FenceDirection.Horizontal);  // 上境界2
        pastureManager.AddFence(0, 1, FenceDirection.Vertical);    // 左境界1
        pastureManager.AddFence(0, 2, FenceDirection.Vertical);    // 左境界2
        pastureManager.AddFence(2, 1, FenceDirection.Vertical);    // 右境界1
        pastureManager.AddFence(2, 2, FenceDirection.Vertical);    // 右境界2
        
        yield return new WaitForSeconds(0.5f);
        
        // 第2牧場 (1x3) - 正しい8個の柵で囲む
        // タイル座標: (5,1), (5,2), (5,3)
        Debug.Log("第2牧場 (1x3) を作成中...");
        pastureManager.AddFence(5, 1, FenceDirection.Horizontal);  // 下境界
        pastureManager.AddFence(5, 4, FenceDirection.Horizontal);  // 上境界
        pastureManager.AddFence(4, 1, FenceDirection.Vertical);    // 左境界1
        pastureManager.AddFence(4, 2, FenceDirection.Vertical);    // 左境界2
        pastureManager.AddFence(4, 3, FenceDirection.Vertical);    // 左境界3
        pastureManager.AddFence(5, 1, FenceDirection.Vertical);    // 右境界1
        pastureManager.AddFence(5, 2, FenceDirection.Vertical);    // 右境界2
        pastureManager.AddFence(5, 3, FenceDirection.Vertical);    // 右境界3
        
        var allPastures = pastureManager.GetAllPastures();
        Debug.Log($"複数牧場テスト - 検出された牧場数: {allPastures.Count}");
        
        // すべての牧場情報を表示
        pastureManager.PrintAllPasturesInfo();
        
        // 各牧場に動物を配置
        for (int i = 0; i < allPastures.Count && i < 2; i++)
        {
            var pasture = allPastures[i];
            AnimalType animalType = i == 0 ? AnimalType.Sheep : AnimalType.Cattle;
            int count = Mathf.Min(2, pasture.capacity);
            
            pastureManager.AddAnimalToPasture(pasture.id, animalType, count);
            Debug.Log($"牧場{pasture.id}に{animalType}を{count}匹配置");
        }
        
        // 最終状態を表示
        pastureManager.PrintAllPasturesInfo();
        
        yield return null;
    }
    
    /// <summary>
    /// すべての柵をクリア（テスト用）
    /// </summary>
    private void ClearAllFences()
    {
        var allFences = pastureManager.GetAllFences();
        foreach (var fence in allFences)
        {
            pastureManager.RemoveFence(fence.position, fence.direction);
        }
        Debug.Log("すべての柵をクリアしました");
    }
    
    /// <summary>
    /// 手動テスト用のパブリックメソッド
    /// </summary>
    [ContextMenu("小さな牧場を作成")]
    public void CreateSmallPasture()
    {
        ClearAllFences();
        
        // 2x2の小さな牧場 - 正しい8個の柵で囲む
        // タイル座標: (1,1), (2,1), (1,2), (2,2)
        
        Debug.Log("2x2牧場の作成 - 8個の柵を配置:");
        
        // 水平柵（上下の境界）
        pastureManager.AddFence(1, 1, FenceDirection.Horizontal);  // 下境界1
        pastureManager.AddFence(2, 1, FenceDirection.Horizontal);  // 下境界2
        pastureManager.AddFence(1, 3, FenceDirection.Horizontal);  // 上境界1
        pastureManager.AddFence(2, 3, FenceDirection.Horizontal);  // 上境界2
        
        // 垂直柵（左右の境界）
        pastureManager.AddFence(0, 1, FenceDirection.Vertical);    // 左境界1
        pastureManager.AddFence(0, 2, FenceDirection.Vertical);    // 左境界2
        pastureManager.AddFence(2, 1, FenceDirection.Vertical);    // 右境界1
        pastureManager.AddFence(2, 2, FenceDirection.Vertical);    // 右境界2
        
        Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");
        Debug.Log("小さな牧場を作成しました");
        pastureManager.PrintAllPasturesInfo();
    }
    
    [ContextMenu("大きな牧場を作成")]
    public void CreateLargePasture()
    {
        ClearAllFences();
        
        // 4x3の大きな牧場
        pastureManager.AddFence(0, 0, FenceDirection.Horizontal);
        pastureManager.AddFence(1, 0, FenceDirection.Horizontal);
        pastureManager.AddFence(2, 0, FenceDirection.Horizontal);
        pastureManager.AddFence(3, 0, FenceDirection.Horizontal);
        
        pastureManager.AddFence(0, 3, FenceDirection.Horizontal);
        pastureManager.AddFence(1, 3, FenceDirection.Horizontal);
        pastureManager.AddFence(2, 3, FenceDirection.Horizontal);
        pastureManager.AddFence(3, 3, FenceDirection.Horizontal);
        
        pastureManager.AddFence(0, 0, FenceDirection.Vertical);
        pastureManager.AddFence(0, 1, FenceDirection.Vertical);
        pastureManager.AddFence(0, 2, FenceDirection.Vertical);
        
        pastureManager.AddFence(4, 0, FenceDirection.Vertical);
        pastureManager.AddFence(4, 1, FenceDirection.Vertical);
        pastureManager.AddFence(4, 2, FenceDirection.Vertical);
        
        Debug.Log("大きな牧場を作成しました");
        pastureManager.PrintAllPasturesInfo();
    }
    
    [ContextMenu("すべての柵をクリア")]
    public void ClearAllFencesMenu()
    {
        ClearAllFences();
    }
    
    [ContextMenu("動物を追加テスト")]
    public void TestAddAnimals()
    {
        var pastures = pastureManager.GetAllPastures();
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が存在しません");
            return;
        }
        
        var pasture = pastures[0];
        pastureManager.AddAnimalToPasture(pasture.id, AnimalType.Sheep, 2);
        pastureManager.AddAnimalToPasture(pasture.id, AnimalType.Cattle, 1);
        
        Debug.Log("動物を追加しました");
        pastureManager.PrintPastureInfo(pasture.id);
    }
}