using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 柵の向きを定義（改修版：4方向対応）
/// </summary>
[System.Serializable]
public enum FenceDirection
{
    Top,    // タイルの上辺
    Bottom, // タイルの下辺
    Left,   // タイルの左辺
    Right   // タイルの右辺
}

/// <summary>
/// 柵の情報を管理するクラス（改修版）
/// </summary>
[System.Serializable]
public class Fence
{
    public Vector2Int position;     // 柵の位置（基準タイルの座標）
    public FenceDirection direction; // 柵の向き（上下左右）
    public bool isActive;           // 柵が有効かどうか
    
    public Fence(Vector2Int pos, FenceDirection dir)
    {
        position = pos;
        direction = dir;
        isActive = true;
    }
    
    public Fence(int x, int y, FenceDirection dir)
    {
        position = new Vector2Int(x, y);
        direction = dir;
        isActive = true;
    }
    
    /// <summary>
    /// 柵のIDを取得（位置と向きから一意に決まる）
    /// </summary>
    public string GetFenceId()
    {
        return $"{position.x}_{position.y}_{direction}";
    }
    
    /// <summary>
    /// この柵が分離する2つのタイルの座標を取得（改修版）
    /// </summary>
    public (Vector2Int, Vector2Int) GetAdjacentTiles()
    {
        switch (direction)
        {
            case FenceDirection.Top:
                // 上辺：このタイルと上のタイルを分離
                return (position, new Vector2Int(position.x, position.y + 1));
                
            case FenceDirection.Bottom:
                // 下辺：このタイルと下のタイルを分離
                return (position, new Vector2Int(position.x, position.y - 1));
                
            case FenceDirection.Left:
                // 左辺：このタイルと左のタイルを分離
                return (position, new Vector2Int(position.x - 1, position.y));
                
            case FenceDirection.Right:
                // 右辺：このタイルと右のタイルを分離
                return (position, new Vector2Int(position.x + 1, position.y));
                
            default:
                return (position, position);
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
    /// 方向の日本語名を取得
    /// </summary>
    public string GetDirectionName()
    {
        switch (direction)
        {
            case FenceDirection.Top: return "上辺";
            case FenceDirection.Bottom: return "下辺";
            case FenceDirection.Left: return "左辺";
            case FenceDirection.Right: return "右辺";
            default: return direction.ToString();
        }
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
/// 牧場管理システムのメインクラス（改修版）
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
            Debug.Log("PastureManager: 初期化完了（4方向対応版）");
        }
    }
    
    /// <summary>
    /// 柵を追加
    /// </summary>
    public bool AddFence(Vector2Int position, FenceDirection direction)
    {
        var fence = new Fence(position, direction);
        string fenceId = fence.GetFenceId();
        
        if (fences.ContainsKey(fenceId))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 柵は既に存在します - タイル({position.x},{position.y})の{fence.GetDirectionName()}");
            }
            return false;
        }
        
        // 隣接するタイルが存在するかチェック
        var (tile1, tile2) = fence.GetAdjacentTiles();
        if (!tileManager.HasTile(tile1) || !tileManager.HasTile(tile2))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 隣接するタイルが存在しません - タイル({position.x},{position.y})の{fence.GetDirectionName()}");
            }
            return false;
        }
        
        fences[fenceId] = fence;
        OnFenceAdded?.Invoke(fenceId, fence);
        
        // 牧場の更新
        UpdatePastures();
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 柵を追加しました - タイル({position.x},{position.y})の{fence.GetDirectionName()}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 柵を追加（座標指定版）
    /// </summary>
    public bool AddFence(int x, int y, FenceDirection direction)
    {
        return AddFence(new Vector2Int(x, y), direction);
    }
    
    /// <summary>
    /// 柵を削除
    /// </summary>
    public bool RemoveFence(Vector2Int position, FenceDirection direction)
    {
        var fence = new Fence(position, direction);
        string fenceId = fence.GetFenceId();
        
        if (!fences.ContainsKey(fenceId))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 柵が存在しません - タイル({position.x},{position.y})の{fence.GetDirectionName()}");
            }
            return false;
        }
        
        fences.Remove(fenceId);
        OnFenceRemoved?.Invoke(fenceId);
        
        // 牧場の更新
        UpdatePastures();
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 柵を削除しました - タイル({position.x},{position.y})の{fence.GetDirectionName()}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 柵を削除（座標指定版）
    /// </summary>
    public bool RemoveFence(int x, int y, FenceDirection direction)
    {
        return RemoveFence(new Vector2Int(x, y), direction);
    }
    
    /// <summary>
    /// 柵が存在するかチェック
    /// </summary>
    public bool HasFence(Vector2Int position, FenceDirection direction)
    {
        var fence = new Fence(position, direction);
        return fences.ContainsKey(fence.GetFenceId());
    }
    
    /// <summary>
    /// 柵が存在するかチェック（座標指定版）
    /// </summary>
    public bool HasFence(int x, int y, FenceDirection direction)
    {
        return HasFence(new Vector2Int(x, y), direction);
    }
    
    /// <summary>
    /// 柵情報を取得
    /// </summary>
    public Fence GetFence(Vector2Int position, FenceDirection direction)
    {
        var fence = new Fence(position, direction);
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
    /// 指定位置から囲まれた領域を検出（Flood Fill）- 改修版
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
            
            // 4方向の隣接タイルをチェック（改修版：明確な方向指定）
            CheckAndMoveToNeighbor(current, new Vector2Int(current.x, current.y + 1), FenceDirection.Top, localVisited, queue);
            CheckAndMoveToNeighbor(current, new Vector2Int(current.x, current.y - 1), FenceDirection.Bottom, localVisited, queue);
            CheckAndMoveToNeighbor(current, new Vector2Int(current.x - 1, current.y), FenceDirection.Left, localVisited, queue);
            CheckAndMoveToNeighbor(current, new Vector2Int(current.x + 1, current.y), FenceDirection.Right, localVisited, queue);
        }
        
        return area;
    }
    
    /// <summary>
    /// 隣接タイルへの移動をチェックして、可能ならキューに追加
    /// </summary>
    private void CheckAndMoveToNeighbor(Vector2Int current, Vector2Int neighbor, FenceDirection fenceDirection, 
                                      HashSet<Vector2Int> localVisited, Queue<Vector2Int> queue)
    {
        // 既に訪問済み、またはタイルが存在しない場合はスキップ
        if (localVisited.Contains(neighbor) || !tileManager.HasTile(neighbor))
            return;
        
        // 現在のタイルの指定方向に柵があるかチェック
        bool hasFence = HasFence(current.x, current.y, fenceDirection);
        
        if (!hasFence)
        {
            localVisited.Add(neighbor);
            queue.Enqueue(neighbor);
        }
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
        
        // 境界柵を特定（改修版：4方向対応）
        foreach (var tile in tiles)
        {
            CheckAndAddBoundaryFence(pasture, tile, FenceDirection.Top);
            CheckAndAddBoundaryFence(pasture, tile, FenceDirection.Bottom);
            CheckAndAddBoundaryFence(pasture, tile, FenceDirection.Left);
            CheckAndAddBoundaryFence(pasture, tile, FenceDirection.Right);
        }
        
        pastures[pasture.id] = pasture;
    }
    
    /// <summary>
    /// 境界柵をチェックして追加（改修版）
    /// </summary>
    private void CheckAndAddBoundaryFence(Pasture pasture, Vector2Int tile, FenceDirection direction)
    {
        var fence = new Fence(tile, direction);
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
                Debug.Log($"  柵: タイル({fence.position.x},{fence.position.y})の{fence.GetDirectionName()}");
            }
        }
    }
    
    /// <summary>
    /// すべての牧場情報を表示
    /// </summary>
    public void PrintAllPasturesInfo()
    {
        Debug.Log($"=== 全牧場情報（改修版4方向対応） ===");
        Debug.Log($"牧場数: {pastures.Count}");
        Debug.Log($"柵数: {fences.Count}");
        
        foreach (var pasture in pastures.Values)
        {
            PrintPastureInfo(pasture.id);
        }
        
        Debug.Log("=== 全柵情報 ===");
        foreach (var fence in fences.Values)
        {
            Debug.Log($"柵: タイル({fence.position.x},{fence.position.y})の{fence.GetDirectionName()}");
        }
    }
    
    /// <summary>
    /// デモ用：2×2牧場を作成
    /// </summary>
    public void CreateDemo2x2Pasture()
    {
        Debug.Log("=== 2×2牧場作成デモ ===");
        
        // タイル (1,1) の境界
        AddFence(1, 1, FenceDirection.Bottom);  // 下辺
        AddFence(1, 1, FenceDirection.Left);    // 左辺
        
        // タイル (2,1) の境界
        AddFence(2, 1, FenceDirection.Bottom);  // 下辺
        AddFence(2, 1, FenceDirection.Right);   // 右辺
        
        // タイル (1,2) の境界
        AddFence(1, 2, FenceDirection.Top);     // 上辺
        AddFence(1, 2, FenceDirection.Left);    // 左辺
        
        // タイル (2,2) の境界
        AddFence(2, 2, FenceDirection.Top);     // 上辺
        AddFence(2, 2, FenceDirection.Right);   // 右辺
        
        Debug.Log("2×2牧場の作成完了！牧場が自動検出されます。");
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
            
            switch (fence.direction)
            {
                case FenceDirection.Top:
                    start = pos + new Vector3(-0.5f, 0.5f, 0);
                    end = pos + new Vector3(0.5f, 0.5f, 0);
                    break;
                case FenceDirection.Bottom:
                    start = pos + new Vector3(-0.5f, -0.5f, 0);
                    end = pos + new Vector3(0.5f, -0.5f, 0);
                    break;
                case FenceDirection.Left:
                    start = pos + new Vector3(-0.5f, -0.5f, 0);
                    end = pos + new Vector3(-0.5f, 0.5f, 0);
                    break;
                case FenceDirection.Right:
                    start = pos + new Vector3(0.5f, -0.5f, 0);
                    end = pos + new Vector3(0.5f, 0.5f, 0);
                    break;
                default:
                    continue;
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