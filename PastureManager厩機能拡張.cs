using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 既存のPastureManagerに厩による容量倍増機能を直接追加
/// </summary>
public partial class PastureManager : MonoBehaviour
{
    [Header("厩による容量倍増設定")]
    [SerializeField] private float stableCapacityMultiplier = 2.0f; // 厩1つごとの容量倍率
    [SerializeField] private bool enableStableBonus = true; // 厩ボーナスの有効/無効
    
    /// <summary>
    /// 拡張版牧場クラス（厩情報付き）
    /// </summary>
    [System.Serializable]
    public class PastureExtended : Pasture
    {
        public int baseCapacity;  // 基本容量（厩なしの状態）
        public int stableCount;   // 厩の数
        
        public PastureExtended(int pastureId) : base(pastureId)
        {
            baseCapacity = 0;
            stableCount = 0;
        }
    }
    
    /// <summary>
    /// 牧場を作成（厩ボーナス対応版）
    /// </summary>
    private void CreatePastureWithStableBonus(List<Vector2Int> tiles)
    {
        if (tiles.Count == 0) return;
        
        var pasture = new PastureExtended(nextPastureId++);
        pasture.tiles = new List<Vector2Int>(tiles);
        
        // 基本容量の計算
        pasture.baseCapacity = tiles.Count * defaultCapacityPerTile;
        
        // 厩による容量倍増を計算
        pasture.stableCount = CountStablesInArea(tiles);
        pasture.capacity = CalculateStableCapacity(pasture.baseCapacity, pasture.stableCount);
        
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
    /// 指定エリア内の厩の数を数える
    /// </summary>
    public int CountStablesInArea(List<Vector2Int> tiles)
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
    public int CalculateStableCapacity(int baseCapacity, int stableCount)
    {
        if (!enableStableBonus || stableCount == 0)
        {
            return baseCapacity;
        }
        
        // 厩1つごとに容量を2倍にする（累積）
        // 例：基本容量8匹、厩2個 → 8 × 2^2 = 32匹
        float multiplier = Mathf.Pow(stableCapacityMultiplier, stableCount);
        return Mathf.RoundToInt(baseCapacity * multiplier);
    }
    
    /// <summary>
    /// 牧場に厩を追加
    /// </summary>
    public bool AddStable(int pastureId, Vector2Int position)
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
        
        if (success)
        {
            // 牧場の容量を再計算
            RecalculatePastureCapacity(pastureId);
            
            if (enableDebugLog)
            {
                Debug.Log($"PastureManager: 牧場{pastureId}の位置({position.x},{position.y})に厩を追加しました");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 牧場から厩を削除
    /// </summary>
    public bool RemoveStable(int pastureId, Vector2Int position)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場{pastureId}が存在しません");
            return false;
        }
        
        // TileManagerを通じて厩を削除
        bool success = tileManager.RemoveStructure(position, StructureType.Stable);
        
        if (success)
        {
            // 牧場の容量を再計算
            RecalculatePastureCapacity(pastureId);
            
            if (enableDebugLog)
            {
                Debug.Log($"PastureManager: 位置({position.x},{position.y})から厩を削除しました");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 特定牧場の容量を再計算
    /// </summary>
    public void RecalculatePastureCapacity(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            return;
        }
        
        var pasture = pastures[pastureId];
        int oldCapacity = pasture.capacity;
        
        if (pasture is PastureExtended extendedPasture)
        {
            // 厩の数を再計算
            extendedPasture.stableCount = CountStablesInArea(pasture.tiles);
            
            // 新しい容量を計算
            pasture.capacity = CalculateStableCapacity(extendedPasture.baseCapacity, extendedPasture.stableCount);
        }
        else
        {
            // 通常の牧場の場合、厩ボーナスを適用して再計算
            int stableCount = CountStablesInArea(pasture.tiles);
            int baseCapacity = pasture.tiles.Count * defaultCapacityPerTile;
            pasture.capacity = CalculateStableCapacity(baseCapacity, stableCount);
        }
        
        if (oldCapacity != pasture.capacity)
        {
            OnPastureUpdated?.Invoke(pastureId, pasture);
            
            if (enableDebugLog)
            {
                Debug.Log($"PastureManager: 牧場{pastureId}の容量を更新しました ({oldCapacity} → {pasture.capacity})");
            }
        }
    }
    
    /// <summary>
    /// すべての牧場の容量を再計算
    /// </summary>
    public void RecalculateAllPastureCapacities()
    {
        foreach (var pastureId in pastures.Keys.ToList())
        {
            RecalculatePastureCapacity(pastureId);
        }
    }
    
    /// <summary>
    /// 牧場の厩情報を取得
    /// </summary>
    public (int baseCapacity, int stableCount, int finalCapacity, float multiplier) GetPastureCapacityInfo(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            return (0, 0, 0, 1.0f);
        }
        
        var pasture = pastures[pastureId];
        int stableCount = CountStablesInArea(pasture.tiles);
        int baseCapacity = pasture.tiles.Count * defaultCapacityPerTile;
        float multiplier = stableCount > 0 ? (float)pasture.capacity / baseCapacity : 1.0f;
        
        return (baseCapacity, stableCount, pasture.capacity, multiplier);
    }
    
    /// <summary>
    /// 牧場の詳細情報を表示（厩情報付き）
    /// </summary>
    public void PrintPastureInfoWithStables(int pastureId)
    {
        if (!pastures.ContainsKey(pastureId))
        {
            Debug.LogWarning($"PastureManager: 牧場{pastureId}が存在しません");
            return;
        }
        
        var pasture = pastures[pastureId];
        var (baseCapacity, stableCount, finalCapacity, multiplier) = GetPastureCapacityInfo(pastureId);
        
        Debug.Log($"=== 牧場{pastureId}の情報（厩ボーナス付き） ===");
        Debug.Log($"面積: {pasture.GetArea()}タイル");
        Debug.Log($"基本容量: {baseCapacity}匹");
        Debug.Log($"厩数: {stableCount}個");
        Debug.Log($"容量倍率: {multiplier:F1}倍");
        Debug.Log($"最終容量: {finalCapacity}匹");
        Debug.Log($"現在の動物数: {pasture.GetTotalAnimalCount()}匹");
        Debug.Log($"空き容量: {finalCapacity - pasture.GetTotalAnimalCount()}匹");
        
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
/// 厩機能のデモとテスト用
/// </summary>
public class StableSystemDemo : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PastureManager pastureManager;
    [SerializeField] private TileManager tileManager;
    
    void Update()
    {
        // キーボードショートカット
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DemoBasicStableSystem();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DemoMultipleStables();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DemoStableRemoval();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            DemoCapacityComparison();
        }
    }
    
    /// <summary>
    /// 基本的な厩システムのデモ
    /// </summary>
    [ContextMenu("1: 基本厩システムデモ")]
    public void DemoBasicStableSystem()
    {
        Debug.Log("=== 基本厩システムデモ ===");
        
        // 2×2牧場を作成
        pastureManager.AddFence(1, 1, TileSide.Bottom);
        pastureManager.AddFence(1, 1, TileSide.Left);
        pastureManager.AddFence(2, 1, TileSide.Bottom);
        pastureManager.AddFence(2, 1, TileSide.Right);
        pastureManager.AddFence(1, 2, TileSide.Top);
        pastureManager.AddFence(1, 2, TileSide.Left);
        pastureManager.AddFence(2, 2, TileSide.Top);
        pastureManager.AddFence(2, 2, TileSide.Right);
        
        Debug.Log("厩なしの状態:");
        pastureManager.PrintPastureInfoWithStables(1);
        
        // 厩を1つ追加
        pastureManager.AddStable(1, new Vector2Int(1, 1));
        
        Debug.Log("厩1つ追加後:");
        pastureManager.PrintPastureInfoWithStables(1);
    }
    
    /// <summary>
    /// 複数厩のデモ
    /// </summary>
    [ContextMenu("2: 複数厩デモ")]
    public void DemoMultipleStables()
    {
        Debug.Log("=== 複数厩デモ ===");
        
        var pastures = pastureManager.GetAllPastures();
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が見つかりません。先に牧場を作成してください。");
            return;
        }
        
        var pasture = pastures[0];
        
        // 厩を2つ目追加
        if (pasture.tiles.Count > 1)
        {
            pastureManager.AddStable(pasture.id, pasture.tiles[1]);
            
            Debug.Log("厩2つ目追加後:");
            pastureManager.PrintPastureInfoWithStables(pasture.id);
        }
        
        // 厩を3つ目追加（可能な場合）
        if (pasture.tiles.Count > 2)
        {
            pastureManager.AddStable(pasture.id, pasture.tiles[2]);
            
            Debug.Log("厩3つ目追加後:");
            pastureManager.PrintPastureInfoWithStables(pasture.id);
        }
    }
    
    /// <summary>
    /// 厩削除のデモ
    /// </summary>
    [ContextMenu("3: 厩削除デモ")]
    public void DemoStableRemoval()
    {
        Debug.Log("=== 厩削除デモ ===");
        
        var pastures = pastureManager.GetAllPastures();
        if (pastures.Count == 0)
        {
            Debug.LogWarning("牧場が見つかりません。");
            return;
        }
        
        var pasture = pastures[0];
        
        Debug.Log("削除前:");
        pastureManager.PrintPastureInfoWithStables(pasture.id);
        
        // 最初のタイルから厩を削除
        if (pasture.tiles.Count > 0)
        {
            pastureManager.RemoveStable(pasture.id, pasture.tiles[0]);
            
            Debug.Log("厩削除後:");
            pastureManager.PrintPastureInfoWithStables(pasture.id);
        }
    }
    
    /// <summary>
    /// 容量比較デモ
    /// </summary>
    [ContextMenu("4: 容量比較デモ")]
    public void DemoCapacityComparison()
    {
        Debug.Log("=== 容量比較デモ ===");
        
        // 容量の変化を表で表示
        Debug.Log("厩数別容量表（基本容量8匹の場合）:");
        Debug.Log("厩数 | 倍率 | 最終容量");
        Debug.Log("-----|------|--------");
        
        for (int stables = 0; stables <= 4; stables++)
        {
            int baseCapacity = 8;
            int finalCapacity = pastureManager.CalculateStableCapacity(baseCapacity, stables);
            float multiplier = stables > 0 ? (float)finalCapacity / baseCapacity : 1.0f;
            
            Debug.Log($" {stables}個  | {multiplier:F1}倍 | {finalCapacity}匹");
        }
    }
}