using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 厩による容量倍増機能を追加したPastureManagerの拡張
/// </summary>
public class PastureManagerWithStableBonus : PastureManager
{
    [Header("厩による容量倍増設定")]
    [SerializeField] private float stableCapacityMultiplier = 2.0f; // 厩1つごとの容量倍率
    [SerializeField] private bool enableStableBonus = true; // 厩ボーナスの有効/無効
    
    protected override void Start()
    {
        base.Start();
        
        // TileManagerの構造物変更イベントを購読
        if (tileManager != null)
        {
            tileManager.OnStructureAdded += OnStructureChanged;
            tileManager.OnStructureRemoved += OnStructureChanged;
        }
    }
    
    void OnDestroy()
    {
        // イベントの購読解除
        if (tileManager != null)
        {
            tileManager.OnStructureAdded -= OnStructureChanged;
            tileManager.OnStructureRemoved -= OnStructureChanged;
        }
    }
    
    /// <summary>
    /// 構造物変更時の処理
    /// </summary>
    private void OnStructureChanged(Vector2Int position, StructureType structureType)
    {
        // 厩が追加/削除された場合、牧場の容量を再計算
        if (structureType == StructureType.Stable)
        {
            UpdatePastureCapacities();
        }
    }
    
    /// <summary>
    /// 牧場を作成（厩ボーナス付き）
    /// </summary>
    protected override void CreatePasture(List<Vector2Int> tiles)
    {
        if (tiles.Count == 0) return;
        
        var pasture = new PastureWithStableBonus(nextPastureId++);
        pasture.tiles = new List<Vector2Int>(tiles);
        
        // 基本容量の計算
        pasture.baseCapacity = tiles.Count * defaultCapacityPerTile;
        
        // 厩による容量倍増を計算
        pasture.stableCount = CountStablesInPasture(tiles);
        pasture.capacity = CalculateCapacityWithStableBonus(pasture.baseCapacity, pasture.stableCount);
        
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
        
        if (enableDebugLog)
        {
            Debug.Log($"PastureManager: 牧場{pasture.id}を作成しました " +
                     $"(面積:{pasture.GetArea()}, 基本容量:{pasture.baseCapacity}, " +
                     $"厩数:{pasture.stableCount}, 最終容量:{pasture.capacity})");
        }
    }
    
    /// <summary>
    /// 牧場内の厩の数を数える
    /// </summary>
    private int CountStablesInPasture(List<Vector2Int> tiles)
    {
        int stableCount = 0;
        
        foreach (var tile in tiles)
        {
            if (tileManager.HasTile(tile))
            {
                var tileData = tileManager.GetTile(tile);
                if (tileData.HasStructure(StructureType.Stable))
                {
                    stableCount++;
                }
            }
        }
        
        return stableCount;
    }
    
    /// <summary>
    /// 厩ボーナスを含む容量を計算
    /// </summary>
    private int CalculateCapacityWithStableBonus(int baseCapacity, int stableCount)
    {
        if (!enableStableBonus || stableCount == 0)
        {
            return baseCapacity;
        }
        
        // 厩1つごとに容量を2倍にする（累積）
        float multiplier = Mathf.Pow(stableCapacityMultiplier, stableCount);
        return Mathf.RoundToInt(baseCapacity * multiplier);
    }
    
    /// <summary>
    /// すべての牧場の容量を再計算
    /// </summary>
    private void UpdatePastureCapacities()
    {
        foreach (var pasture in pastures.Values.Cast<PastureWithStableBonus>())
        {
            int oldCapacity = pasture.capacity;
            
            // 厩の数を再計算
            pasture.stableCount = CountStablesInPasture(pasture.tiles);
            
            // 新しい容量を計算
            pasture.capacity = CalculateCapacityWithStableBonus(pasture.baseCapacity, pasture.stableCount);
            
            if (oldCapacity != pasture.capacity)
            {
                OnPastureUpdated?.Invoke(pasture.id, pasture);
                
                if (enableDebugLog)
                {
                    Debug.Log($"PastureManager: 牧場{pasture.id}の容量を更新しました " +
                             $"({oldCapacity} → {pasture.capacity}, 厩数:{pasture.stableCount})");
                }
            }
        }
    }
    
    /// <summary>
    /// 牧場に厩を追加
    /// </summary>
    public bool AddStableToPasture(int pastureId, Vector2Int position)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場{pastureId}が存在しません");
            return false;
        }
        
        var pasture = pastures[pastureId];
        if (!pasture.tiles.Contains(position))
        {
            Debug.LogWarning($"PastureManager: 位置({position.x},{position.y})は牧場{pastureId}に含まれていません");
            return false;
        }
        
        // TileManagerを通じて厩を追加
        bool success = tileManager.AddStructure(position, StructureType.Stable);
        
        if (success && enableDebugLog)
        {
            Debug.Log($"PastureManager: 牧場{pastureId}の位置({position.x},{position.y})に厩を追加しました");
        }
        
        return success;
    }
    
    /// <summary>
    /// 牧場から厩を削除
    /// </summary>
    public bool RemoveStableFromPasture(int pastureId, Vector2Int position)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場{pastureId}が存在しません");
            return false;
        }
        
        // TileManagerを通じて厩を削除
        bool success = tileManager.RemoveStructure(position, StructureType.Stable);
        
        if (success && enableDebugLog)
        {
            Debug.Log($"PastureManager: 位置({position.x},{position.y})から厩を削除しました");
        }
        
        return success;
    }
    
    /// <summary>
    /// 特定牧場の厩情報を取得
    /// </summary>
    public PastureStableInfo GetPastureStableInfo(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            return null;
        }
        
        var pasture = pastures[pastureId] as PastureWithStableBonus;
        if (pasture == null) return null;
        
        return new PastureStableInfo
        {
            pastureId = pastureId,
            baseCapacity = pasture.baseCapacity,
            stableCount = pasture.stableCount,
            finalCapacity = pasture.capacity,
            capacityMultiplier = pasture.stableCount > 0 ? (float)pasture.capacity / pasture.baseCapacity : 1.0f
        };
    }
    
    /// <summary>
    /// 牧場の詳細情報を表示（厩情報付き）
    /// </summary>
    public override void PrintPastureInfo(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場{pastureId}が存在しません");
            return;
        }
        
        var pasture = pastures[pastureId] as PastureWithStableBonus;
        if (pasture == null)
        {
            base.PrintPastureInfo(pastureId);
            return;
        }
        
        Debug.Log($"=== 牧場{pastureId}の情報（厩ボーナス付き） ===");
        Debug.Log($"面積: {pasture.GetArea()}タイル");
        Debug.Log($"基本容量: {pasture.baseCapacity}匹");
        Debug.Log($"厩数: {pasture.stableCount}個");
        Debug.Log($"容量倍率: {(pasture.stableCount > 0 ? (float)pasture.capacity / pasture.baseCapacity : 1.0f):F1}倍");
        Debug.Log($"最終容量: {pasture.capacity}匹");
        Debug.Log($"現在の動物数: {pasture.GetTotalAnimalCount()}匹");
        Debug.Log($"空き容量: {pasture.capacity - pasture.GetTotalAnimalCount()}匹");
        
        if (pasture.animals.Count > 0)
        {
            Debug.Log("動物の内訳:");
            foreach (var animal in pasture.animals)
            {
                Debug.Log($"  {animal.Key}: {animal.Value}匹");
            }
        }
        
        // 厩の位置を表示
        var stablePositions = new List<Vector2Int>();
        foreach (var tile in pasture.tiles)
        {
            if (tileManager.HasTile(tile))
            {
                var tileData = tileManager.GetTile(tile);
                if (tileData.HasStructure(StructureType.Stable))
                {
                    stablePositions.Add(tile);
                }
            }
        }
        
        if (stablePositions.Count > 0)
        {
            Debug.Log("厩の位置:");
            foreach (var pos in stablePositions)
            {
                Debug.Log($"  ({pos.x}, {pos.y})");
            }
        }
    }
}

/// <summary>
/// 厩ボーナス付きの牧場クラス
/// </summary>
[System.Serializable]
public class PastureWithStableBonus : Pasture
{
    public int baseCapacity;  // 基本容量（厩なしの状態）
    public int stableCount;   // 厩の数
    
    public PastureWithStableBonus(int pastureId) : base(pastureId)
    {
        baseCapacity = 0;
        stableCount = 0;
    }
}

/// <summary>
/// 牧場の厩情報
/// </summary>
[System.Serializable]
public class PastureStableInfo
{
    public int pastureId;
    public int baseCapacity;
    public int stableCount;
    public int finalCapacity;
    public float capacityMultiplier;
}

/// <summary>
/// 厩による容量倍増のデモとテスト
/// </summary>
public class StableBonusDemo : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PastureManagerWithStableBonus pastureManager;
    [SerializeField] private TileManager tileManager;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CreatePastureWithStables();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestStableAddition();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestStableRemoval();
        }
    }
    
    /// <summary>
    /// 厩付き牧場を作成
    /// </summary>
    [ContextMenu("1: 厩付き牧場作成")]
    public void CreatePastureWithStables()
    {
        Debug.Log("=== 厩付き牧場作成デモ ===");
        
        // 2×2牧場を作成
        pastureManager.CreateDemo2x2Pasture();
        
        // 厩を追加
        pastureManager.AddStableToPasture(1, new Vector2Int(1, 1));
        pastureManager.AddStableToPasture(1, new Vector2Int(2, 2));
        
        // 牧場情報を表示
        pastureManager.PrintPastureInfo(1);
    }
    
    /// <summary>
    /// 厩追加テスト
    /// </summary>
    [ContextMenu("2: 厩追加テスト")]
    public void TestStableAddition()
    {
        Debug.Log("=== 厩追加テスト ===");
        
        var pastures = pastureManager.GetAllPastures();
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が見つかりません");
            return;
        }
        
        var pasture = pastures[0];
        if (pasture.tiles.Count > 0)
        {
            var targetTile = pasture.tiles[0];
            pastureManager.AddStableToPasture(pasture.id, targetTile);
            
            Debug.Log($"牧場{pasture.id}の({targetTile.x},{targetTile.y})に厩を追加しました");
            pastureManager.PrintPastureInfo(pasture.id);
        }
    }
    
    /// <summary>
    /// 厩削除テスト
    /// </summary>
    [ContextMenu("3: 厩削除テスト")]
    public void TestStableRemoval()
    {
        Debug.Log("=== 厩削除テスト ===");
        
        var pastures = pastureManager.GetAllPastures();
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が見つかりません");
            return;
        }
        
        var pasture = pastures[0];
        if (pasture.tiles.Count > 0)
        {
            var targetTile = pasture.tiles[0];
            pastureManager.RemoveStableFromPasture(pasture.id, targetTile);
            
            Debug.Log($"({targetTile.x},{targetTile.y})から厩を削除しました");
            pastureManager.PrintPastureInfo(pasture.id);
        }
    }
}