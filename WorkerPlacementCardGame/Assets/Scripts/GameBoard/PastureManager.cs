using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 柵の向きを定義
/// </summary>
[System.Serializable]
public enum FenceDirection
{
    Horizontal, // 水平（タイルの上下境界）
    Vertical    // 垂直（タイルの左右境界）
}

/// <summary>
/// 柵の情報を管理するクラス
/// </summary>
[System.Serializable]
public class Fence
{
    public Vector2Int position;     // 柵の位置（基準タイルの座標）
    public FenceDirection direction; // 柵の向き
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
    /// この柵が分離する2つのタイルの座標を取得
    /// </summary>
    public (Vector2Int, Vector2Int) GetAdjacentTiles()
    {
        if (direction == FenceDirection.Horizontal)
        {
            // 水平柵：上下のタイルを分離
            return (position, new Vector2Int(position.x, position.y + 1));
        }
        else
        {
            // 垂直柵：左右のタイルを分離
            return (position, new Vector2Int(position.x + 1, position.y));
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
/// 牧場管理システムのメインクラス
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
            Debug.Log("PastureManager: 初期化完了");
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
                Debug.LogWarning($"PastureManager: 柵は既に存在します - {fenceId}");
            }
            return false;
        }
        
        // 隣接するタイルが存在するかチェック
        var (tile1, tile2) = fence.GetAdjacentTiles();
        if (!tileManager.HasTile(tile1) || !tileManager.HasTile(tile2))
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PastureManager: 隣接するタイルが存在しません - {fenceId}");
            }
            return false;
        }
        
        fences[fenceId] = fence;
        OnFenceAdded?.Invoke(fenceId, fence);
        
        // 牧場の更新
        UpdatePastures();
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 柵を追加しました - {fenceId}");
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
                Debug.LogWarning($"PastureManager: 柵が存在しません - {fenceId}");
            }
            return false;
        }
        
        fences.Remove(fenceId);
        OnFenceRemoved?.Invoke(fenceId);
        
        // 牧場の更新
        UpdatePastures();
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 柵を削除しました - {fenceId}");
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
    /// 指定位置から囲まれた領域を検出（Flood Fill）
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
            
            for (int i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                
                if (localVisited.Contains(neighbor) || !tileManager.HasTile(neighbor))
                    continue;
                
                // 隣接タイルとの間に柵があるかチェック
                bool hasFence = false;
                
                if (i == 0) // 上方向
                {
                    hasFence = HasFence(current.x, current.y, FenceDirection.Horizontal);
                }
                else if (i == 1) // 下方向
                {
                    hasFence = HasFence(neighbor.x, neighbor.y, FenceDirection.Horizontal);
                }
                else if (i == 2) // 左方向
                {
                    hasFence = HasFence(neighbor.x, neighbor.y, FenceDirection.Vertical);
                }
                else if (i == 3) // 右方向
                {
                    hasFence = HasFence(current.x, current.y, FenceDirection.Vertical);
                }
                
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
            // 各方向の境界をチェック
            CheckAndAddBoundaryFence(pasture, tile, new Vector2Int(tile.x, tile.y), FenceDirection.Horizontal);
            CheckAndAddBoundaryFence(pasture, tile, new Vector2Int(tile.x, tile.y - 1), FenceDirection.Horizontal);
            CheckAndAddBoundaryFence(pasture, tile, new Vector2Int(tile.x, tile.y), FenceDirection.Vertical);
            CheckAndAddBoundaryFence(pasture, tile, new Vector2Int(tile.x - 1, tile.y), FenceDirection.Vertical);
        }
        
        pastures[pasture.id] = pasture;
    }
    
    /// <summary>
    /// 境界柵をチェックして追加
    /// </summary>
    private void CheckAndAddBoundaryFence(Pasture pasture, Vector2Int tile, Vector2Int fencePos, FenceDirection direction)
    {
        var fence = new Fence(fencePos, direction);
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
    /// 牧場情報をデバッグ出力
    /// </summary>
    public void PrintPastureInfo(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場が存在しません - ID: {pastureId}");
            return;
        }
        
        var pasture = pastures[pastureId];
        var info = new System.Text.StringBuilder();
        info.AppendLine($"=== 牧場 {pastureId} ===");
        info.AppendLine($"面積: {pasture.GetArea()}タイル");
        info.AppendLine($"容量: {pasture.capacity}匹");
        info.AppendLine($"現在の動物数: {pasture.GetTotalAnimalCount()}匹");
        
        if (pasture.animals.Count > 0)
        {
            info.AppendLine("動物:");
            foreach (var animal in pasture.animals)
            {
                info.AppendLine($"  {animal.Key}: {animal.Value}匹");
            }
        }
        
        info.AppendLine($"タイル: {string.Join(", ", pasture.tiles.Select(t => $"({t.x},{t.y})"))}");
        info.AppendLine($"境界柵数: {pasture.boundaryFences.Count}");
        
        Debug.Log(info.ToString());
    }
    
    /// <summary>
    /// すべての牧場情報をデバッグ出力
    /// </summary>
    public void PrintAllPasturesInfo()
    {
        if (pastures.Count == 0)
        {
            Debug.Log("PastureManager: 牧場が存在しません");
            return;
        }
        
        Debug.Log($"PastureManager: 検出された牧場数: {pastures.Count}");
        foreach (var pastureId in pastures.Keys)
        {
            PrintPastureInfo(pastureId);
        }
    }
    
    /// <summary>
    /// デバッグ用ギズモ描画
    /// </summary>
    void OnDrawGizmos()
    {
        if (!enableDebugLog) return;
        
        // 柵を描画
        Gizmos.color = Color.brown;
        foreach (var fence in fences.Values)
        {
            if (!fence.isActive) continue;
            
            Vector3 start, end;
            if (fence.direction == FenceDirection.Horizontal)
            {
                start = new Vector3(fence.position.x - 0.5f, 0, fence.position.y + 0.5f);
                end = new Vector3(fence.position.x + 0.5f, 0, fence.position.y + 0.5f);
            }
            else
            {
                start = new Vector3(fence.position.x + 0.5f, 0, fence.position.y - 0.5f);
                end = new Vector3(fence.position.x + 0.5f, 0, fence.position.y + 0.5f);
            }
            
            Gizmos.DrawLine(start, end);
        }
        
        // 牧場を描画
        var colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
        int colorIndex = 0;
        
        foreach (var pasture in pastures.Values)
        {
            Gizmos.color = colors[colorIndex % colors.Length];
            foreach (var tile in pasture.tiles)
            {
                Vector3 center = new Vector3(tile.x, 0.1f, tile.y);
                Gizmos.DrawCube(center, Vector3.one * 0.8f);
            }
            colorIndex++;
        }
    }
}