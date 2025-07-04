using UnityEngine;

/// <summary>
/// 複数牧場の独立したID管理デモ
/// </summary>
public class MultiplePasturesDemo : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PastureManager pastureManager;
    [SerializeField] private TileManager tileManager;
    
    void Start()
    {
        // 少し待ってからデモを実行
        Invoke(nameof(RunMultiplePasturesDemo), 1.0f);
    }
    
    void Update()
    {
        // キーボードショートカット
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RunMultiplePasturesDemo();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            CreateAdjacentPastures();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            CreateComplexPastureLayout();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            TestPastureModification();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            ClearAllFences();
        }
    }
    
    /// <summary>
    /// 複数牧場デモの実行
    /// </summary>
    [ContextMenu("Q: 複数牧場デモ")]
    public void RunMultiplePasturesDemo()
    {
        Debug.Log("=== 複数牧場の独立ID管理デモ ===");
        
        ClearAllFences();
        
        // 3つの独立した牧場を作成
        CreatePasture1x1(1, 1);    // 牧場1: (1,1)
        CreatePasture2x1(4, 1);    // 牧場2: (4,1)-(5,1) 
        CreatePasture1x2(7, 1);    // 牧場3: (7,1)-(7,2)
        
        Debug.Log("3つの独立した牧場を作成しました");
        
        // 牧場情報を表示
        DisplayAllPastureDetails();
        
        // 各牧場に異なる動物を追加
        AddAnimalsToEachPasture();
    }
    
    /// <summary>
    /// 隣接した牧場デモ
    /// </summary>
    [ContextMenu("W: 隣接牧場デモ")]
    public void CreateAdjacentPastures()
    {
        Debug.Log("=== 隣接した牧場デモ ===");
        
        ClearAllFences();
        
        // 隣接した2つの牧場を作成（共通の柵を持つ）
        Debug.Log("左の牧場（2×1）を作成中...");
        // 左の牧場: (1,1)-(2,1)
        pastureManager.AddFence(1, 1, TileSide.Bottom);  // 下辺
        pastureManager.AddFence(2, 1, TileSide.Bottom);  // 下辺
        pastureManager.AddFence(1, 1, TileSide.Top);     // 上辺
        pastureManager.AddFence(2, 1, TileSide.Top);     // 上辺
        pastureManager.AddFence(1, 1, TileSide.Left);    // 左辺
        pastureManager.AddFence(2, 1, TileSide.Right);   // 右辺（共通境界）
        
        Debug.Log("右の牧場（2×1）を作成中...");
        // 右の牧場: (3,1)-(4,1)
        pastureManager.AddFence(3, 1, TileSide.Bottom);  // 下辺
        pastureManager.AddFence(4, 1, TileSide.Bottom);  // 下辺
        pastureManager.AddFence(3, 1, TileSide.Top);     // 上辺
        pastureManager.AddFence(4, 1, TileSide.Top);     // 上辺
        pastureManager.AddFence(3, 1, TileSide.Left);    // 左辺（共通境界と同じ柵）
        pastureManager.AddFence(4, 1, TileSide.Right);   // 右辺
        
        Debug.Log("隣接した2つの牧場が作成されました（共通の柵で分離）");
        DisplayAllPastureDetails();
    }
    
    /// <summary>
    /// 複雑な牧場レイアウトデモ
    /// </summary>
    [ContextMenu("E: 複雑レイアウトデモ")]
    public void CreateComplexPastureLayout()
    {
        Debug.Log("=== 複雑な牧場レイアウトデモ ===");
        
        ClearAllFences();
        
        // 大小様々な形状の牧場を作成
        Debug.Log("牧場1: 1×1の小さな牧場");
        CreatePasture1x1(1, 1);
        
        Debug.Log("牧場2: 3×2の大きな長方形牧場");
        CreatePasture3x2(3, 1);
        
        Debug.Log("牧場3: L字型の変則牧場");
        CreatePastureL_Shape(7, 1);
        
        Debug.Log("牧場4: 1×3の縦長牧場");
        CreatePasture1x3(1, 3);
        
        Debug.Log("複雑なレイアウトの牧場群が完成しました");
        DisplayAllPastureDetails();
        
        // 各牧場の特性を分析
        AnalyzePastureCharacteristics();
    }
    
    /// <summary>
    /// 牧場の動的変更テスト
    /// </summary>
    [ContextMenu("R: 牧場変更テスト")]
    public void TestPastureModification()
    {
        Debug.Log("=== 牧場の動的変更テスト ===");
        
        // まず2つの牧場を作成
        ClearAllFences();
        CreatePasture1x1(2, 2);
        CreatePasture1x1(4, 2);
        
        Debug.Log("初期状態: 2つの独立した1×1牧場");
        DisplayAllPastureDetails();
        
        Debug.Log("間の柵を削除して2つの牧場を統合します...");
        
        // 統合のための追加柵を配置
        pastureManager.AddFence(3, 2, TileSide.Top);     // 上辺
        pastureManager.AddFence(3, 2, TileSide.Bottom);  // 下辺
        
        Debug.Log("統合後の状態:");
        DisplayAllPastureDetails();
        
        Debug.Log("再び分離します...");
        // 中央に垂直柵を追加して分離
        pastureManager.AddFence(3, 2, TileSide.Left);
        pastureManager.AddFence(3, 2, TileSide.Right);
        
        Debug.Log("分離後の状態:");
        DisplayAllPastureDetails();
    }
    
    /// <summary>
    /// 1×1牧場を作成
    /// </summary>
    private void CreatePasture1x1(int x, int y)
    {
        pastureManager.AddFence(x, y, TileSide.Top);
        pastureManager.AddFence(x, y, TileSide.Bottom);
        pastureManager.AddFence(x, y, TileSide.Left);
        pastureManager.AddFence(x, y, TileSide.Right);
    }
    
    /// <summary>
    /// 2×1牧場を作成
    /// </summary>
    private void CreatePasture2x1(int x, int y)
    {
        // 下辺
        pastureManager.AddFence(x, y, TileSide.Bottom);
        pastureManager.AddFence(x + 1, y, TileSide.Bottom);
        // 上辺
        pastureManager.AddFence(x, y, TileSide.Top);
        pastureManager.AddFence(x + 1, y, TileSide.Top);
        // 左右辺
        pastureManager.AddFence(x, y, TileSide.Left);
        pastureManager.AddFence(x + 1, y, TileSide.Right);
    }
    
    /// <summary>
    /// 1×2牧場を作成
    /// </summary>
    private void CreatePasture1x2(int x, int y)
    {
        // 下辺
        pastureManager.AddFence(x, y, TileSide.Bottom);
        // 上辺
        pastureManager.AddFence(x, y + 1, TileSide.Top);
        // 左右辺
        pastureManager.AddFence(x, y, TileSide.Left);
        pastureManager.AddFence(x, y + 1, TileSide.Left);
        pastureManager.AddFence(x, y, TileSide.Right);
        pastureManager.AddFence(x, y + 1, TileSide.Right);
    }
    
    /// <summary>
    /// 3×2牧場を作成
    /// </summary>
    private void CreatePasture3x2(int x, int y)
    {
        // 下辺
        for (int i = 0; i < 3; i++)
        {
            pastureManager.AddFence(x + i, y, TileSide.Bottom);
        }
        // 上辺
        for (int i = 0; i < 3; i++)
        {
            pastureManager.AddFence(x + i, y + 1, TileSide.Top);
        }
        // 左辺
        pastureManager.AddFence(x, y, TileSide.Left);
        pastureManager.AddFence(x, y + 1, TileSide.Left);
        // 右辺
        pastureManager.AddFence(x + 2, y, TileSide.Right);
        pastureManager.AddFence(x + 2, y + 1, TileSide.Right);
    }
    
    /// <summary>
    /// 1×3牧場を作成
    /// </summary>
    private void CreatePasture1x3(int x, int y)
    {
        // 下辺
        pastureManager.AddFence(x, y, TileSide.Bottom);
        // 上辺
        pastureManager.AddFence(x, y + 2, TileSide.Top);
        // 左右辺
        for (int i = 0; i < 3; i++)
        {
            pastureManager.AddFence(x, y + i, TileSide.Left);
            pastureManager.AddFence(x, y + i, TileSide.Right);
        }
    }
    
    /// <summary>
    /// L字型牧場を作成
    /// </summary>
    private void CreatePastureL_Shape(int x, int y)
    {
        // L字型: ■■
        //       ■□
        
        // タイル(x,y), (x+1,y), (x,y+1)
        
        // 外周の柵
        pastureManager.AddFence(x, y, TileSide.Bottom);      // 下辺1
        pastureManager.AddFence(x + 1, y, TileSide.Bottom);  // 下辺2
        pastureManager.AddFence(x + 1, y, TileSide.Right);   // 右辺1
        pastureManager.AddFence(x + 1, y, TileSide.Top);     // 上辺1
        pastureManager.AddFence(x, y + 1, TileSide.Top);     // 上辺2
        pastureManager.AddFence(x, y + 1, TileSide.Right);   // 右辺2
        pastureManager.AddFence(x, y + 1, TileSide.Left);    // 左辺2
        pastureManager.AddFence(x, y, TileSide.Left);        // 左辺1
    }
    
    /// <summary>
    /// 各牧場に異なる動物を追加
    /// </summary>
    private void AddAnimalsToEachPasture()
    {
        var pastures = pastureManager.GetAllPastures();
        
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が見つかりません");
            return;
        }
        
        var animalTypes = new[] { AnimalType.Sheep, AnimalType.Cattle, AnimalType.Boar, AnimalType.Horse, AnimalType.Chicken };
        
        Debug.Log("=== 各牧場に動物を追加 ===");
        
        for (int i = 0; i < pastures.Count; i++)
        {
            var pasture = pastures[i];
            var animalType = animalTypes[i % animalTypes.Length];
            
            // 牧場容量の半分まで動物を追加
            int animalCount = Mathf.Max(1, pasture.capacity / 2);
            
            pastureManager.AddAnimalToPasture(pasture.id, animalType, animalCount);
            Debug.Log($"牧場{pasture.id}に{animalType}を{animalCount}匹追加しました");
        }
    }
    
    /// <summary>
    /// すべての牧場の詳細情報を表示
    /// </summary>
    private void DisplayAllPastureDetails()
    {
        var pastures = pastureManager.GetAllPastures();
        
        Debug.Log($"=== 牧場詳細情報 ===");
        Debug.Log($"検出された牧場数: {pastures.Count}");
        
        foreach (var pasture in pastures)
        {
            Debug.Log($"【牧場{pasture.id}】面積: {pasture.GetArea()}タイル, 容量: {pasture.capacity}匹, 動物: {pasture.GetTotalAnimalCount()}匹");
            
            // タイル座標を表示
            string tilesList = string.Join(", ", pasture.tiles.ConvertAll(t => $"({t.x},{t.y})"));
            Debug.Log($"  含まれるタイル: {tilesList}");
            
            // 動物詳細
            if (pasture.animals.Count > 0)
            {
                string animalsList = string.Join(", ", pasture.animals.Select(a => $"{a.Key}×{a.Value}"));
                Debug.Log($"  動物詳細: {animalsList}");
            }
        }
    }
    
    /// <summary>
    /// 牧場特性分析
    /// </summary>
    private void AnalyzePastureCharacteristics()
    {
        var pastures = pastureManager.GetAllPastures();
        
        Debug.Log("=== 牧場特性分析 ===");
        
        if (pastures.Count == 0)
        {
            Debug.Log("分析する牧場がありません");
            return;
        }
        
        // 最大・最小面積
        var largestPasture = pastures.OrderByDescending(p => p.GetArea()).First();
        var smallestPasture = pastures.OrderBy(p => p.GetArea()).First();
        
        Debug.Log($"最大牧場: 牧場{largestPasture.id}（{largestPasture.GetArea()}タイル）");
        Debug.Log($"最小牧場: 牧場{smallestPasture.id}（{smallestPasture.GetArea()}タイル）");
        
        // 総面積と平均
        int totalArea = pastures.Sum(p => p.GetArea());
        float averageArea = (float)totalArea / pastures.Count;
        
        Debug.Log($"総牧場面積: {totalArea}タイル");
        Debug.Log($"平均牧場面積: {averageArea:F1}タイル");
        
        // 動物数統計
        int totalAnimals = pastures.Sum(p => p.GetTotalAnimalCount());
        Debug.Log($"総動物数: {totalAnimals}匹");
    }
    
    /// <summary>
    /// すべての柵をクリア
    /// </summary>
    [ContextMenu("T: すべてクリア")]
    public void ClearAllFences()
    {
        Debug.Log("すべての柵をクリアします...");
        
        var allFences = pastureManager.GetAllFences();
        foreach (var fence in allFences)
        {
            // 正規化座標から元のタイル座標を逆算
            if (fence.direction == FenceDirection.Horizontal)
            {
                // 水平柵: 下のタイルのTop
                pastureManager.RemoveFence(fence.position.x, fence.position.y - 1, TileSide.Top);
            }
            else
            {
                // 垂直柵: 左のタイルのRight
                pastureManager.RemoveFence(fence.position.x - 1, fence.position.y, TileSide.Right);
            }
        }
        
        Debug.Log("柵のクリア完了");
    }
}