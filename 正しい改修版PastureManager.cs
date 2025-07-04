using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 柵の向きを定義（正規化版：2方向に統一）
/// </summary>
[System.Serializable]
public enum FenceDirection
{
    Horizontal, // 水平柵（タイル間の水平境界）
    Vertical    // 垂直柵（タイル間の垂直境界）
}

/// <summary>
/// ユーザー用の4方向指定（内部的には正規化される）
/// </summary>
[System.Serializable]
public enum TileSide
{
    Top,    // タイルの上辺
    Bottom, // タイルの下辺
    Left,   // タイルの左辺
    Right   // タイルの右辺
}

/// <summary>
/// 柵の情報を管理するクラス（正規化版）
/// </summary>
[System.Serializable]
public class Fence
{
    public Vector2Int position;     // 柵の正規化された位置
    public FenceDirection direction; // 柵の向き（水平/垂直）
    public bool isActive;           // 柵が有効かどうか
    
    public Fence(Vector2Int pos, FenceDirection dir)
    {
        position = pos;
        direction = dir;
        isActive = true;
    }
    
    /// <summary>
    /// ユーザー指定の座標と方向から正規化された柵を作成
    /// </summary>
    public static Fence CreateNormalized(Vector2Int tilePos, TileSide side)
    {
        var (normalizedPos, normalizedDir) = NormalizeFence(tilePos, side);
        return new Fence(normalizedPos, normalizedDir);
    }
    
    /// <summary>
    /// 柵の座標と方向を正規化（物理的に同じ柵を統一表現にする）
    /// </summary>
    public static (Vector2Int position, FenceDirection direction) NormalizeFence(Vector2Int tilePos, TileSide side)
    {
        switch (side)
        {
            case TileSide.Top:
                // タイル(x,y)の上辺 = 座標(x,y+1)の水平柵
                return (new Vector2Int(tilePos.x, tilePos.y + 1), FenceDirection.Horizontal);
                
            case TileSide.Bottom:
                // タイル(x,y)の下辺 = 座標(x,y)の水平柵
                return (new Vector2Int(tilePos.x, tilePos.y), FenceDirection.Horizontal);
                
            case TileSide.Left:
                // タイル(x,y)の左辺 = 座標(x,y)の垂直柵
                return (new Vector2Int(tilePos.x, tilePos.y), FenceDirection.Vertical);
                
            case TileSide.Right:
                // タイル(x,y)の右辺 = 座標(x+1,y)の垂直柵
                return (new Vector2Int(tilePos.x + 1, tilePos.y), FenceDirection.Vertical);
                
            default:
                return (tilePos, FenceDirection.Horizontal);
        }
    }
    
    /// <summary>
    /// 柵のIDを取得（正規化された位置と向きから）
    /// </summary>
    public string GetFenceId()
    {
        return $"{position.x}_{position.y}_{direction}";
    }
    
    /// <summary>
    /// この柵が分離する2つのタイルの座標を取得
    /// </summary>
    public (Vector2Int, Vector2Int) GetAdjacentTiles()
    {
        if (direction == FenceDirection.Horizontal)
        {
            // 水平柵：上下のタイルを分離
            return (new Vector2Int(position.x, position.y - 1), new Vector2Int(position.x, position.y));
        }
        else
        {
            // 垂直柵：左右のタイルを分離
            return (new Vector2Int(position.x - 1, position.y), new Vector2Int(position.x, position.y));
        }
    }
    
    /// <summary>
    /// この柵が指定された2つのタイル間にあるかチェック
    /// </summary>
    public bool IsBetween(Vector2Int tile1, Vector2Int tile2)
    {
        var (adjTile1, adjTile2) = GetAdjacentTiles();
        return (adjTile1 == tile1 && adjTile2 == tile2) || 
               (adjTile1 == tile2 && adjTile2 == tile1);
    }
    
    /// <summary>
    /// 柵の説明を取得
    /// </summary>
    public string GetDescription()
    {
        string directionName = direction == FenceDirection.Horizontal ? "水平柵" : "垂直柵";
        return $"座標({position.x},{position.y})の{directionName}";
    }
}

/// <summary>
/// 牧場の情報を管理するクラス
/// </summary>
[System.Serializable]
public class Pasture
{
    public int id;                              // 牧場ID
    public List<Vector2Int> tiles;              // 牧場に含まれるタイルのリスト
    public List<string> boundaryFences;         // 牧場の境界となる柵のIDリスト
    public int capacity;                        // 収容可能な動物数
    public Dictionary<AnimalType, int> animals; // 牧場にいる動物
    
    public Pasture(int pastureId)
    {
        id = pastureId;
        tiles = new List<Vector2Int>();
        boundaryFences = new List<string>();
        animals = new Dictionary<AnimalType, int>();
        capacity = 0;
    }
    
    /// <summary>
    /// 牧場の面積を取得
    /// </summary>
    public int GetArea()
    {
        return tiles.Count;
    }
    
    /// <summary>
    /// 動物の総数を取得
    /// </summary>
    public int GetTotalAnimalCount()
    {
        return animals.Values.Sum();
    }
    
    /// <summary>
    /// 動物を追加できるかチェック
    /// </summary>
    public bool CanAddAnimal(AnimalType animalType, int count = 1)
    {
        return GetTotalAnimalCount() + count <= capacity;
    }
    
    /// <summary>
    /// 動物を追加
    /// </summary>
    public bool AddAnimal(AnimalType animalType, int count = 1)
    {
        if (!CanAddAnimal(animalType, count)) return false;
        
        if (!animals.ContainsKey(animalType))
            animals[animalType] = 0;
        
        animals[animalType] += count;
        return true;
    }
    
    /// <summary>
    /// 動物を削除
    /// </summary>
    public bool RemoveAnimal(AnimalType animalType, int count = 1)
    {
        if (!animals.ContainsKey(animalType) || animals[animalType] < count)
            return false;
        
        animals[animalType] -= count;
        if (animals[animalType] <= 0)
            animals.Remove(animalType);
        
        return true;
    }
}

/// <summary>
/// 牧場管理システムのメインクラス（正規化版）
/// </summary>
public class PastureManager : MonoBehaviour
{
    [Header("TileManager参照")]
    [SerializeField] private TileManager tileManager;
    
    [Header("柵管理")]
    [SerializeField] private Dictionary<string, Fence> fences = new Dictionary<string, Fence>();
    
    [Header("牧場管理")]
    [SerializeField] private Dictionary<int, Pasture> pastures = new Dictionary<int, Pasture>();
    [SerializeField] private int nextPastureId = 1;
    
    [Header("設定")]
    [SerializeField] private int defaultCapacityPerTile = 2; // タイル1つあたりの動物収容数
    
    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLog = true;
    
    // イベント
    public System.Action<string, Fence> OnFenceAdded;
    public System.Action<string> OnFenceRemoved;
    public System.Action<int, Pasture> OnPastureCreated;
    public System.Action<int> OnPastureDestroyed;
    public System.Action<int, Pasture> OnPastureUpdated;
    
    void Start()
    {
        if (tileManager == null)
        {
            tileManager = FindObjectOfType<TileManager>();
            if (tileManager == null)
            {
                Debug.LogError("PastureManager: TileManagerが見つかりません！");
                return;
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log("PastureManager: 初期化完了（正規化版）");
        }
    }
    
    /// <summary>
    /// 柵を追加（ユーザー用：4方向指定）
    /// </summary>
    public bool AddFence(Vector2Int tilePos, TileSide side)
    {
        var fence = Fence.CreateNormalized(tilePos, side);
        string fenceId = fence.GetFenceId();
        
        if (fences.ContainsKey(fenceId))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 柵は既に存在します - タイル({tilePos.x},{tilePos.y})の{GetSideName(side)}");
            }
            return false;
        }
        
        // 隣接するタイルが存在するかチェック
        var (tile1, tile2) = fence.GetAdjacentTiles();
        if (!tileManager.HasTile(tile1) || !tileManager.HasTile(tile2))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 隣接するタイルが存在しません - タイル({tilePos.x},{tilePos.y})の{GetSideName(side)}");
            }
            return false;
        }
        
        fences[fenceId] = fence;
        OnFenceAdded?.Invoke(fenceId, fence);
        
        // 牧場の更新
        UpdatePastures();
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 柵を追加しました - タイル({tilePos.x},{tilePos.y})の{GetSideName(side)} [{fence.GetDescription()}]");
        }
        
        return true;
    }
    
    /// <summary>
    /// 柵を追加（座標指定版）
    /// </summary>
    public bool AddFence(int x, int y, TileSide side)
    {
        return AddFence(new Vector2Int(x, y), side);
    }
    
    /// <summary>
    /// 柵を削除（ユーザー用：4方向指定）
    /// </summary>
    public bool RemoveFence(Vector2Int tilePos, TileSide side)
    {
        var fence = Fence.CreateNormalized(tilePos, side);
        string fenceId = fence.GetFenceId();
        
        if (!fences.ContainsKey(fenceId))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 柵が存在しません - タイル({tilePos.x},{tilePos.y})の{GetSideName(side)}");
            }
            return false;
        }
        
        fences.Remove(fenceId);
        OnFenceRemoved?.Invoke(fenceId);
        
        // 牧場の更新
        UpdatePastures();
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 柵を削除しました - タイル({tilePos.x},{tilePos.y})の{GetSideName(side)}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 柵を削除（座標指定版）
    /// </summary>
    public bool RemoveFence(int x, int y, TileSide side)
    {
        return RemoveFence(new Vector2Int(x, y), side);
    }
    
    /// <summary>
    /// 柵が存在するかチェック（ユーザー用：4方向指定）
    /// </summary>
    public bool HasFence(Vector2Int tilePos, TileSide side)
    {
        var fence = Fence.CreateNormalized(tilePos, side);
        return fences.ContainsKey(fence.GetFenceId());
    }
    
    /// <summary>
    /// 柵が存在するかチェック（座標指定版）
    /// </summary>
    public bool HasFence(int x, int y, TileSide side)
    {
        return HasFence(new Vector2Int(x, y), side);
    }
    
    /// <summary>
    /// 2つのタイル間に柵があるかチェック
    /// </summary>
    public bool HasFenceBetween(Vector2Int tile1, Vector2Int tile2)
    {
        // 隣接タイル間の柵をチェック
        Vector2Int diff = tile2 - tile1;
        
        if (diff == Vector2Int.up) // tile2が上
        {
            return HasFence(tile1, TileSide.Top);
        }
        else if (diff == Vector2Int.down) // tile2が下
        {
            return HasFence(tile1, TileSide.Bottom);
        }
        else if (diff == Vector2Int.left) // tile2が左
        {
            return HasFence(tile1, TileSide.Left);
        }
        else if (diff == Vector2Int.right) // tile2が右
        {
            return HasFence(tile1, TileSide.Right);
        }
        
        return false; // 隣接していない
    }
    
    /// <summary>
    /// 柵情報を取得
    /// </summary>
    public Fence GetFence(Vector2Int tilePos, TileSide side)
    {
        var fence = Fence.CreateNormalized(tilePos, side);
        string fenceId = fence.GetFenceId();
        return fences.ContainsKey(fenceId) ? fences[fenceId] : null;
    }
    
    /// <summary>
    /// すべての牧場を更新（柵の変更後に呼ばれる）
    /// </summary>
    public void UpdatePastures()
    {
        // 古い牧場情報をクリア
        var oldPastures = new Dictionary<int, Pasture>(pastures);
        pastures.Clear();
        
        // 囲まれた領域を検出
        var allTiles = tileManager.GetAllTiles().Select(t => t.position).ToList();
        var visited = new HashSet<Vector2Int>();
        
        foreach (var tilePos in allTiles)
        {
            if (visited.Contains(tilePos)) continue;
            
            var enclosedArea = FindEnclosedArea(tilePos, visited);
            if (enclosedArea.Count > 0)
            {
                CreatePasture(enclosedArea);
            }
        }
        
        // 牧場の変更を通知
        foreach (var pasture in pastures.Values)
        {
            OnPastureCreated?.Invoke(pasture.id, pasture);
        }
        
        foreach (var oldPasture in oldPastures.Values)
        {
            if (!pastures.ContainsKey(oldPasture.id))
            {
                OnPastureDestroyed?.Invoke(oldPasture.id);
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 牧場を更新しました。検出された牧場数: {pastures.Count}");
        }
    }
    
    /// <summary>
    /// 指定位置から囲まれた領域を検出（Flood Fill）- 正規化版
    /// </summary>
    private List<Vector2Int> FindEnclosedArea(Vector2Int startPos, HashSet<Vector2Int> globalVisited)
    {
        var area = new List<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var localVisited = new HashSet<Vector2Int>();
        
        queue.Enqueue(startPos);
        localVisited.Add(startPos);
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            area.Add(current);
            globalVisited.Add(current);
            
            // 4方向の隣接タイルをチェック
            var neighbors = new[]
            {
                new Vector2Int(current.x, current.y + 1), // 上
                new Vector2Int(current.x, current.y - 1), // 下
                new Vector2Int(current.x - 1, current.y), // 左
                new Vector2Int(current.x + 1, current.y)  // 右
            };
            
            var sides = new[] { TileSide.Top, TileSide.Bottom, TileSide.Left, TileSide.Right };
            
            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                
                if (localVisited.Contains(neighbor) || !tileManager.HasTile(neighbor))
                    continue;
                
                // 隣接タイルとの間に柵があるかチェック
                bool hasFence = HasFence(current, sides[i]);
                
                if (!hasFence)
                {
                    localVisited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return area;
    }
    
    /// <summary>
    /// 牧場を作成
    /// </summary>
    private void CreatePasture(List<Vector2Int> tiles)
    {
        if (tiles.Count == 0) return;
        
        var pasture = new Pasture(nextPastureId++);
        pasture.tiles = new List<Vector2Int>(tiles);
        pasture.capacity = tiles.Count * defaultCapacityPerTile;
        
        // 境界柵を特定
        foreach (var tile in tiles)
        {
            var sides = new[] { TileSide.Top, TileSide.Bottom, TileSide.Left, TileSide.Right };
            
            foreach (var side in sides)
            {
                CheckAndAddBoundaryFence(pasture, tile, side);
            }
        }
        
        pastures[pasture.id] = pasture;
    }
    
    /// <summary>
    /// 境界柵をチェックして追加
    /// </summary>
    private void CheckAndAddBoundaryFence(Pasture pasture, Vector2Int tile, TileSide side)
    {
        var fence = Fence.CreateNormalized(tile, side);
        string fenceId = fence.GetFenceId();
        
        if (fences.ContainsKey(fenceId))
        {
            var (tile1, tile2) = fence.GetAdjacentTiles();
            bool tile1InPasture = pasture.tiles.Contains(tile1);
            bool tile2InPasture = pasture.tiles.Contains(tile2);
            
            // 一方が牧場内、もう一方が牧場外の場合は境界柵
            if (tile1InPasture != tile2InPasture)
            {
                if (!pasture.boundaryFences.Contains(fenceId))
                {
                    pasture.boundaryFences.Add(fenceId);
                }
            }
        }
    }
    
    /// <summary>
    /// 指定位置の牧場を取得
    /// </summary>
    public Pasture GetPastureAtPosition(Vector2Int position)
    {
        return pastures.Values.FirstOrDefault(p => p.tiles.Contains(position));
    }
    
    /// <summary>
    /// 指定位置の牧場を取得（座標指定版）
    /// </summary>
    public Pasture GetPastureAtPosition(int x, int y)
    {
        return GetPastureAtPosition(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// 牧場に動物を追加
    /// </summary>
    public bool AddAnimalToPasture(int pastureId, AnimalType animalType, int count = 1)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 牧場が存在しません - ID: {pastureId}");
            }
            return false;
        }
        
        var pasture = pastures[pastureId];
        bool success = pasture.AddAnimal(animalType, count);
        
        if (success)
        {
            OnPastureUpdated?.Invoke(pastureId, pasture);
            
            if (enableDebugLog)
            {
                Debug.Log($"PastureManager: 牧場{pastureId}に{animalType}を{count}匹追加しました");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 牧場から動物を削除
    /// </summary>
    public bool RemoveAnimalFromPasture(int pastureId, AnimalType animalType, int count = 1)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 牧場が存在しません - ID: {pastureId}");
            }
            return false;
        }
        
        var pasture = pastures[pastureId];
        bool success = pasture.RemoveAnimal(animalType, count);
        
        if (success)
        {
            OnPastureUpdated?.Invoke(pastureId, pasture);
            
            if (enableDebugLog)
            {
                Debug.Log($"PastureManager: 牧場{pastureId}から{animalType}を{count}匹削除しました");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// すべての牧場情報を取得
    /// </summary>
    public List<Pasture> GetAllPastures()
    {
        return pastures.Values.ToList();
    }
    
    /// <summary>
    /// すべての柵情報を取得
    /// </summary>
    public List<Fence> GetAllFences()
    {
        return fences.Values.ToList();
    }
    
    /// <summary>
    /// 特定牧場の詳細情報を表示
    /// </summary>
    public void PrintPastureInfo(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場{pastureId}が存在しません");
            return;
        }
        
        var pasture = pastures[pastureId];
        Debug.Log($"=== 牧場{pastureId}の情報 ===");
        Debug.Log($"面積: {pasture.GetArea()}タイル");
        Debug.Log($"容量: {pasture.capacity}匹");
        Debug.Log($"現在の動物数: {pasture.GetTotalAnimalCount()}匹");
        
        if (pasture.animals.Count > 0)
        {
            Debug.Log("動物の内訳:");
            foreach (var animal in pasture.animals)
            {
                Debug.Log($"  {animal.Key}: {animal.Value}匹");
            }
        }
        
        Debug.Log($"含まれるタイル数: {pasture.tiles.Count}");
        foreach (var tile in pasture.tiles)
        {
            Debug.Log($"  タイル({tile.x}, {tile.y})");
        }
        
        Debug.Log($"境界柵数: {pasture.boundaryFences.Count}");
        foreach (var fenceId in pasture.boundaryFences)
        {
            if (fences.ContainsKey(fenceId))
            {
                var fence = fences[fenceId];
                Debug.Log($"  柵: {fence.GetDescription()}");
            }
        }
    }
    
    /// <summary>
    /// すべての牧場情報を表示
    /// </summary>
    public void PrintAllPasturesInfo()
    {
        Debug.Log($"=== 全牧場情報（正規化版） ===");
        Debug.Log($"牧場数: {pastures.Count}");
        Debug.Log($"柵数: {fences.Count}");
        
        foreach (var pasture in pastures.Values)
        {
            PrintPastureInfo(pasture.id);
        }
        
        Debug.Log("=== 全柵情報 ===");
        foreach (var fence in fences.Values)
        {
            Debug.Log($"柵: {fence.GetDescription()}");
        }
    }
    
    /// <summary>
    /// デモ用：2×2牧場を作成
    /// </summary>
    public void CreateDemo2x2Pasture()
    {
        Debug.Log("=== 2×2牧場作成デモ（正規化版） ===");
        
        // タイル (1,1) の境界
        AddFence(1, 1, TileSide.Bottom);  // 下辺
        AddFence(1, 1, TileSide.Left);    // 左辺
        
        // タイル (2,1) の境界
        AddFence(2, 1, TileSide.Bottom);  // 下辺
        AddFence(2, 1, TileSide.Right);   // 右辺
        
        // タイル (1,2) の境界
        AddFence(1, 2, TileSide.Top);     // 上辺
        AddFence(1, 2, TileSide.Left);    // 左辺
        
        // タイル (2,2) の境界
        AddFence(2, 2, TileSide.Top);     // 上辺
        AddFence(2, 2, TileSide.Right);   // 右辺
        
        Debug.Log("2×2牧場の作成完了！牧場が自動検出されます。");
        
        // 重複テスト
        Debug.Log("=== 重複テスト ===");
        Debug.Log("タイル(1,2)の下辺に柵を追加してみます（タイル(1,1)の上辺と同じはず）...");
        
        bool result1 = AddFence(1, 2, TileSide.Bottom); // これはタイル(1,1)のTopと同じ
        bool result2 = AddFence(1, 1, TileSide.Top);    // これも同じ柵
        
        Debug.Log($"タイル(1,2)のBottom追加結果: {result1}（falseになるはず - 既に存在）");
        Debug.Log($"タイル(1,1)のTop追加結果: {result2}（falseになるはず - 既に存在）");
    }
    
    /// <summary>
    /// 方向名を取得
    /// </summary>
    private string GetSideName(TileSide side)
    {
        switch (side)
        {
            case TileSide.Top: return "上辺";
            case TileSide.Bottom: return "下辺";
            case TileSide.Left: return "左辺";
            case TileSide.Right: return "右辺";
            default: return side.ToString();
        }
    }
    
    void OnDrawGizmos()
    {
        if (fences == null || pastures == null) return;
        
        // 柵を茶色の線で描画
        Gizmos.color = Color.brown;
        foreach (var fence in fences.Values)
        {
            Vector3 pos = new Vector3(fence.position.x, fence.position.y, 0);
            Vector3 start, end;
            
            if (fence.direction == FenceDirection.Horizontal)
            {
                // 水平柵
                start = pos + new Vector3(-0.5f, 0, 0);
                end = pos + new Vector3(0.5f, 0, 0);
            }
            else
            {
                // 垂直柵
                start = pos + new Vector3(0, -0.5f, 0);
                end = pos + new Vector3(0, 0.5f, 0);
            }
            
            Gizmos.DrawLine(start, end);
        }
        
        // 牧場を色分けして描画
        var colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
        int colorIndex = 0;
        
        foreach (var pasture in pastures.Values)
        {
            Gizmos.color = colors[colorIndex % colors.Length];
            
            foreach (var tile in pasture.tiles)
            {
                Vector3 pos = new Vector3(tile.x, tile.y, 0);
                Gizmos.DrawCube(pos, Vector3.one * 0.3f);
            }
            
            colorIndex++;
        }
    }
}