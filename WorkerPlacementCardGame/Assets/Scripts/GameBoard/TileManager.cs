using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// タイルの種類を定義
/// </summary>
[System.Serializable]
public enum TileType
{
    Empty,      // 空き地
    House,      // 家
    Field,      // 畑
    Pasture,    // 牧場
    Forest,     // 森
    Mountain,   // 山
    River,      // 川
    Special     // 特殊タイル
}

/// <summary>
/// 構造物の種類を定義
/// </summary>
[System.Serializable]
public enum StructureType
{
    None,           // なし
    Stable,         // 厩
    Fence,          // 柵
    Well,           // 井戸
    Mill,           // 製粉所
    Workshop,       // 作業場
    Granary,        // 穀物倉庫
    Warehouse,      // 倉庫
    Bridge,         // 橋
    Tower,          // 塔
    Garden          // 庭園
}

/// <summary>
/// 植物の種類を定義
/// </summary>
[System.Serializable]
public enum PlantType
{
    None,           // なし
    Grain,          // 穀物
    Vegetable,      // 野菜
    Fruit,          // 果物
    Flower,         // 花
    Herb,           // ハーブ
    Tree,           // 木
    Grass,          // 草
    Wheat,          // 小麦
    Corn,           // トウモロコシ
    Potato,         // ジャガイモ
    Carrot,         // ニンジン
    Apple,          // リンゴ
    Rose,           // バラ
    Oak,            // オーク
    Pine            // 松
}

/// <summary>
/// 動物の種類を定義
/// </summary>
[System.Serializable]
public enum AnimalType
{
    None,           // なし
    Sheep,          // 羊
    Boar,           // 猪
    Cattle,         // 牛
    Horse,          // 馬
    Chicken,        // 鶏
    Pig,            // 豚
    Goat,           // ヤギ
    Duck,           // 鴨
    Rabbit,         // ウサギ
    Dog,            // 犬
    Cat,            // 猫
    Bird,           // 鳥
    Fish            // 魚
}

/// <summary>
/// 個別のタイル情報を管理するクラス
/// </summary>
[System.Serializable]
public class Tile
{
    public Vector2Int position;                                    // タイルの座標
    public TileType tileType;                                     // タイルの種類
    public List<StructureType> structures = new List<StructureType>(); // 構造物（複数可）
    public Dictionary<PlantType, int> plants = new Dictionary<PlantType, int>(); // 植物（種類と数量）
    public Dictionary<AnimalType, int> animals = new Dictionary<AnimalType, int>(); // 動物（種類と数量）
    public int level = 1;                                         // タイルのレベル（拡張性のため）
    public string description = "";                               // タイルの説明
    public Dictionary<string, object> customProperties = new Dictionary<string, object>(); // カスタムプロパティ

    public Tile(Vector2Int pos, TileType type = TileType.Empty)
    {
        position = pos;
        tileType = type;
    }

    public Tile(int x, int y, TileType type = TileType.Empty)
    {
        position = new Vector2Int(x, y);
        tileType = type;
    }

    /// <summary>
    /// 構造物を追加
    /// </summary>
    public bool AddStructure(StructureType structureType)
    {
        if (structureType == StructureType.None) return false;
        
        if (!structures.Contains(structureType))
        {
            structures.Add(structureType);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 構造物を削除
    /// </summary>
    public bool RemoveStructure(StructureType structureType)
    {
        return structures.Remove(structureType);
    }

    /// <summary>
    /// 指定の構造物があるかチェック
    /// </summary>
    public bool HasStructure(StructureType structureType)
    {
        return structures.Contains(structureType);
    }

    /// <summary>
    /// 植物を追加
    /// </summary>
    public bool AddPlant(PlantType plantType, int amount = 1)
    {
        if (plantType == PlantType.None || amount <= 0) return false;

        if (!plants.ContainsKey(plantType))
            plants[plantType] = 0;
        
        plants[plantType] += amount;
        return true;
    }

    /// <summary>
    /// 植物を削除
    /// </summary>
    public bool RemovePlant(PlantType plantType, int amount = 1)
    {
        if (!plants.ContainsKey(plantType) || plants[plantType] < amount)
            return false;

        plants[plantType] -= amount;
        if (plants[plantType] <= 0)
            plants.Remove(plantType);
        
        return true;
    }

    /// <summary>
    /// 植物の数量を取得
    /// </summary>
    public int GetPlantCount(PlantType plantType)
    {
        return plants.ContainsKey(plantType) ? plants[plantType] : 0;
    }

    /// <summary>
    /// 動物を追加
    /// </summary>
    public bool AddAnimal(AnimalType animalType, int amount = 1)
    {
        if (animalType == AnimalType.None || amount <= 0) return false;

        if (!animals.ContainsKey(animalType))
            animals[animalType] = 0;
        
        animals[animalType] += amount;
        return true;
    }

    /// <summary>
    /// 動物を削除
    /// </summary>
    public bool RemoveAnimal(AnimalType animalType, int amount = 1)
    {
        if (!animals.ContainsKey(animalType) || animals[animalType] < amount)
            return false;

        animals[animalType] -= amount;
        if (animals[animalType] <= 0)
            animals.Remove(animalType);
        
        return true;
    }

    /// <summary>
    /// 動物の数量を取得
    /// </summary>
    public int GetAnimalCount(AnimalType animalType)
    {
        return animals.ContainsKey(animalType) ? animals[animalType] : 0;
    }

    /// <summary>
    /// タイルが空かどうか
    /// </summary>
    public bool IsEmpty()
    {
        return tileType == TileType.Empty && 
               structures.Count == 0 && 
               plants.Count == 0 && 
               animals.Count == 0;
    }

    /// <summary>
    /// カスタムプロパティを設定
    /// </summary>
    public void SetCustomProperty(string key, object value)
    {
        customProperties[key] = value;
    }

    /// <summary>
    /// カスタムプロパティを取得
    /// </summary>
    public T GetCustomProperty<T>(string key, T defaultValue = default(T))
    {
        if (customProperties.ContainsKey(key) && customProperties[key] is T)
            return (T)customProperties[key];
        return defaultValue;
    }

    /// <summary>
    /// タイル情報を文字列で取得
    /// </summary>
    public override string ToString()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"座標: ({position.x}, {position.y})");
        info.AppendLine($"タイル種類: {GetTileTypeName(tileType)}");
        
        if (structures.Count > 0)
        {
            info.AppendLine($"構造物: {string.Join(", ", structures.Select(s => GetStructureTypeName(s)))}");
        }
        
        if (plants.Count > 0)
        {
            var plantInfo = plants.Select(p => $"{GetPlantTypeName(p.Key)}×{p.Value}");
            info.AppendLine($"植物: {string.Join(", ", plantInfo)}");
        }
        
        if (animals.Count > 0)
        {
            var animalInfo = animals.Select(a => $"{GetAnimalTypeName(a.Key)}×{a.Value}");
            info.AppendLine($"動物: {string.Join(", ", animalInfo)}");
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            info.AppendLine($"説明: {description}");
        }
        
        return info.ToString().TrimEnd();
    }

    private string GetTileTypeName(TileType type)
    {
        switch (type)
        {
            case TileType.Empty: return "空き地";
            case TileType.House: return "家";
            case TileType.Field: return "畑";
            case TileType.Pasture: return "牧場";
            case TileType.Forest: return "森";
            case TileType.Mountain: return "山";
            case TileType.River: return "川";
            case TileType.Special: return "特殊";
            default: return type.ToString();
        }
    }

    private string GetStructureTypeName(StructureType type)
    {
        switch (type)
        {
            case StructureType.None: return "なし";
            case StructureType.Stable: return "厩";
            case StructureType.Fence: return "柵";
            case StructureType.Well: return "井戸";
            case StructureType.Mill: return "製粉所";
            case StructureType.Workshop: return "作業場";
            case StructureType.Granary: return "穀物倉庫";
            case StructureType.Warehouse: return "倉庫";
            case StructureType.Bridge: return "橋";
            case StructureType.Tower: return "塔";
            case StructureType.Garden: return "庭園";
            default: return type.ToString();
        }
    }

    private string GetPlantTypeName(PlantType type)
    {
        switch (type)
        {
            case PlantType.None: return "なし";
            case PlantType.Grain: return "穀物";
            case PlantType.Vegetable: return "野菜";
            case PlantType.Fruit: return "果物";
            case PlantType.Flower: return "花";
            case PlantType.Herb: return "ハーブ";
            case PlantType.Tree: return "木";
            case PlantType.Grass: return "草";
            case PlantType.Wheat: return "小麦";
            case PlantType.Corn: return "トウモロコシ";
            case PlantType.Potato: return "ジャガイモ";
            case PlantType.Carrot: return "ニンジン";
            case PlantType.Apple: return "リンゴ";
            case PlantType.Rose: return "バラ";
            case PlantType.Oak: return "オーク";
            case PlantType.Pine: return "松";
            default: return type.ToString();
        }
    }

    private string GetAnimalTypeName(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.None: return "なし";
            case AnimalType.Sheep: return "羊";
            case AnimalType.Boar: return "猪";
            case AnimalType.Cattle: return "牛";
            case AnimalType.Horse: return "馬";
            case AnimalType.Chicken: return "鶏";
            case AnimalType.Pig: return "豚";
            case AnimalType.Goat: return "ヤギ";
            case AnimalType.Duck: return "鴨";
            case AnimalType.Rabbit: return "ウサギ";
            case AnimalType.Dog: return "犬";
            case AnimalType.Cat: return "猫";
            case AnimalType.Bird: return "鳥";
            case AnimalType.Fish: return "魚";
            default: return type.ToString();
        }
    }
}

/// <summary>
/// タイル管理システムのメインクラス
/// </summary>
public class TileManager : MonoBehaviour
{
    [Header("ボード設定")]
    [SerializeField] private int boardWidth = 10;
    [SerializeField] private int boardHeight = 8;
    [SerializeField] private Vector2Int boardOffset = Vector2Int.zero;

    [Header("タイルマップ")]
    [SerializeField] private Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();

    [Header("デバッグ")]
    [SerializeField] private bool enableDebugLog = true;

    // イベント
    public System.Action<Vector2Int, Tile> OnTileChanged;
    public System.Action<Vector2Int, TileType> OnTileTypeChanged;
    public System.Action<Vector2Int, StructureType> OnStructureAdded;
    public System.Action<Vector2Int, StructureType> OnStructureRemoved;

    void Start()
    {
        InitializeTileMap();
    }

    /// <summary>
    /// タイルマップを初期化
    /// </summary>
    private void InitializeTileMap()
    {
        tileMap.Clear();
        
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                Vector2Int position = new Vector2Int(x + boardOffset.x, y + boardOffset.y);
                tileMap[position] = new Tile(position, TileType.Empty);
            }
        }

        if (enableDebugLog)
        {
            Debug.Log($"TileManager: {boardWidth}×{boardHeight}のタイルマップを初期化しました");
        }
    }

    /// <summary>
    /// 座標からタイル情報を取得
    /// </summary>
    public Tile GetTile(Vector2Int position)
    {
        return tileMap.ContainsKey(position) ? tileMap[position] : null;
    }

    /// <summary>
    /// 座標からタイル情報を取得（座標指定版）
    /// </summary>
    public Tile GetTile(int x, int y)
    {
        return GetTile(new Vector2Int(x, y));
    }

    /// <summary>
    /// タイルが存在するかチェック
    /// </summary>
    public bool HasTile(Vector2Int position)
    {
        return tileMap.ContainsKey(position);
    }

    /// <summary>
    /// タイルが存在するかチェック（座標指定版）
    /// </summary>
    public bool HasTile(int x, int y)
    {
        return HasTile(new Vector2Int(x, y));
    }

    /// <summary>
    /// タイルを設定
    /// </summary>
    public bool SetTile(Vector2Int position, Tile tile)
    {
        if (tile == null) return false;

        tile.position = position;
        tileMap[position] = tile;
        
        OnTileChanged?.Invoke(position, tile);
        
        if (enableDebugLog)
        {
            Debug.Log($"TileManager: 座標({position.x}, {position.y})にタイルを設定しました - {tile.tileType}");
        }
        
        return true;
    }

    /// <summary>
    /// タイル種類を設定
    /// </summary>
    public bool SetTileType(Vector2Int position, TileType tileType)
    {
        if (!tileMap.ContainsKey(position))
        {
            tileMap[position] = new Tile(position, tileType);
        }
        else
        {
            tileMap[position].tileType = tileType;
        }

        OnTileTypeChanged?.Invoke(position, tileType);
        
        if (enableDebugLog)
        {
            Debug.Log($"TileManager: 座標({position.x}, {position.y})のタイル種類を{tileType}に設定しました");
        }
        
        return true;
    }

    /// <summary>
    /// タイル種類を設定（座標指定版）
    /// </summary>
    public bool SetTileType(int x, int y, TileType tileType)
    {
        return SetTileType(new Vector2Int(x, y), tileType);
    }

    /// <summary>
    /// 構造物を追加
    /// </summary>
    public bool AddStructure(Vector2Int position, StructureType structureType)
    {
        if (!tileMap.ContainsKey(position))
        {
            tileMap[position] = new Tile(position);
        }

        bool success = tileMap[position].AddStructure(structureType);
        
        if (success)
        {
            OnStructureAdded?.Invoke(position, structureType);
            
            if (enableDebugLog)
            {
                Debug.Log($"TileManager: 座標({position.x}, {position.y})に{structureType}を追加しました");
            }
        }
        
        return success;
    }

    /// <summary>
    /// 構造物を追加（座標指定版）
    /// </summary>
    public bool AddStructure(int x, int y, StructureType structureType)
    {
        return AddStructure(new Vector2Int(x, y), structureType);
    }

    /// <summary>
    /// 構造物を削除
    /// </summary>
    public bool RemoveStructure(Vector2Int position, StructureType structureType)
    {
        if (!tileMap.ContainsKey(position))
            return false;

        bool success = tileMap[position].RemoveStructure(structureType);
        
        if (success)
        {
            OnStructureRemoved?.Invoke(position, structureType);
            
            if (enableDebugLog)
            {
                Debug.Log($"TileManager: 座標({position.x}, {position.y})から{structureType}を削除しました");
            }
        }
        
        return success;
    }

    /// <summary>
    /// 構造物を削除（座標指定版）
    /// </summary>
    public bool RemoveStructure(int x, int y, StructureType structureType)
    {
        return RemoveStructure(new Vector2Int(x, y), structureType);
    }

    /// <summary>
    /// 植物を追加
    /// </summary>
    public bool AddPlant(Vector2Int position, PlantType plantType, int amount = 1)
    {
        if (!tileMap.ContainsKey(position))
        {
            tileMap[position] = new Tile(position);
        }

        bool success = tileMap[position].AddPlant(plantType, amount);
        
        if (success && enableDebugLog)
        {
            Debug.Log($"TileManager: 座標({position.x}, {position.y})に{plantType}を{amount}個追加しました");
        }
        
        return success;
    }

    /// <summary>
    /// 植物を追加（座標指定版）
    /// </summary>
    public bool AddPlant(int x, int y, PlantType plantType, int amount = 1)
    {
        return AddPlant(new Vector2Int(x, y), plantType, amount);
    }

    /// <summary>
    /// 動物を追加
    /// </summary>
    public bool AddAnimal(Vector2Int position, AnimalType animalType, int amount = 1)
    {
        if (!tileMap.ContainsKey(position))
        {
            tileMap[position] = new Tile(position);
        }

        bool success = tileMap[position].AddAnimal(animalType, amount);
        
        if (success && enableDebugLog)
        {
            Debug.Log($"TileManager: 座標({position.x}, {position.y})に{animalType}を{amount}匹追加しました");
        }
        
        return success;
    }

    /// <summary>
    /// 動物を追加（座標指定版）
    /// </summary>
    public bool AddAnimal(int x, int y, AnimalType animalType, int amount = 1)
    {
        return AddAnimal(new Vector2Int(x, y), animalType, amount);
    }

    /// <summary>
    /// 指定タイル種類のタイルをすべて取得
    /// </summary>
    public List<Tile> GetTilesByType(TileType tileType)
    {
        return tileMap.Values.Where(tile => tile.tileType == tileType).ToList();
    }

    /// <summary>
    /// 指定構造物があるタイルをすべて取得
    /// </summary>
    public List<Tile> GetTilesWithStructure(StructureType structureType)
    {
        return tileMap.Values.Where(tile => tile.HasStructure(structureType)).ToList();
    }

    /// <summary>
    /// 指定植物があるタイルをすべて取得
    /// </summary>
    public List<Tile> GetTilesWithPlant(PlantType plantType)
    {
        return tileMap.Values.Where(tile => tile.GetPlantCount(plantType) > 0).ToList();
    }

    /// <summary>
    /// 指定動物がいるタイルをすべて取得
    /// </summary>
    public List<Tile> GetTilesWithAnimal(AnimalType animalType)
    {
        return tileMap.Values.Where(tile => tile.GetAnimalCount(animalType) > 0).ToList();
    }

    /// <summary>
    /// 空のタイルをすべて取得
    /// </summary>
    public List<Tile> GetEmptyTiles()
    {
        return tileMap.Values.Where(tile => tile.IsEmpty()).ToList();
    }

    /// <summary>
    /// すべてのタイルを取得
    /// </summary>
    public List<Tile> GetAllTiles()
    {
        return tileMap.Values.ToList();
    }

    /// <summary>
    /// 指定範囲内のタイルを取得
    /// </summary>
    public List<Tile> GetTilesInRange(Vector2Int center, int range)
    {
        List<Tile> tilesInRange = new List<Tile>();
        
        for (int x = center.x - range; x <= center.x + range; x++)
        {
            for (int y = center.y - range; y <= center.y + range; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (tileMap.ContainsKey(position))
                {
                    tilesInRange.Add(tileMap[position]);
                }
            }
        }
        
        return tilesInRange;
    }

    /// <summary>
    /// タイル情報を文字列で出力
    /// </summary>
    public void PrintTileInfo(Vector2Int position)
    {
        if (tileMap.ContainsKey(position))
        {
            Debug.Log($"=== タイル情報 ===\n{tileMap[position].ToString()}");
        }
        else
        {
            Debug.Log($"座標({position.x}, {position.y})にタイルが存在しません");
        }
    }

    /// <summary>
    /// タイル情報を文字列で出力（座標指定版）
    /// </summary>
    public void PrintTileInfo(int x, int y)
    {
        PrintTileInfo(new Vector2Int(x, y));
    }

    /// <summary>
    /// すべてのタイル情報を出力
    /// </summary>
    public void PrintAllTiles()
    {
        Debug.Log("=== 全タイル情報 ===");
        
        var sortedTiles = tileMap.Values.OrderBy(t => t.position.y).ThenBy(t => t.position.x);
        
        foreach (var tile in sortedTiles)
        {
            if (!tile.IsEmpty())
            {
                Debug.Log(tile.ToString());
                Debug.Log("---");
            }
        }
    }

    /// <summary>
    /// タイルマップをクリア
    /// </summary>
    public void ClearTileMap()
    {
        tileMap.Clear();
        InitializeTileMap();
        
        if (enableDebugLog)
        {
            Debug.Log("TileManager: タイルマップをクリアしました");
        }
    }

    // Unity Editorでのデバッグ表示用
    void OnDrawGizmos()
    {
        if (tileMap == null) return;

        foreach (var kvp in tileMap)
        {
            Vector2Int pos = kvp.Key;
            Tile tile = kvp.Value;
            
            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            
            // タイル種類に応じて色を変更
            switch (tile.tileType)
            {
                case TileType.Empty:
                    Gizmos.color = Color.white;
                    break;
                case TileType.House:
                    Gizmos.color = Color.red;
                    break;
                case TileType.Field:
                    Gizmos.color = Color.green;
                    break;
                case TileType.Pasture:
                    Gizmos.color = Color.yellow;
                    break;
                case TileType.Forest:
                    Gizmos.color = Color.cyan;
                    break;
                case TileType.Mountain:
                    Gizmos.color = Color.gray;
                    break;
                case TileType.River:
                    Gizmos.color = Color.blue;
                    break;
                case TileType.Special:
                    Gizmos.color = Color.magenta;
                    break;
            }
            
            Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
            
            // 構造物がある場合は小さな立方体を描画
            if (tile.structures.Count > 0)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(worldPos + Vector3.up * 0.3f, Vector3.one * 0.2f);
            }
        }
    }
}