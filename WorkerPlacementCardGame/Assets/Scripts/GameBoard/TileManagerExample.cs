using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TileManagerの使用方法を示すサンプルクラス
/// </summary>
public class TileManagerExample : MonoBehaviour
{
    [Header("タイル管理システム")]
    public TileManager tileManager;

    void Start()
    {
        // TileManagerが設定されていない場合は自動で取得
        if (tileManager == null)
        {
            tileManager = FindObjectOfType<TileManager>();
        }

        // TileManagerが見つからない場合は作成
        if (tileManager == null)
        {
            GameObject tileManagerObject = new GameObject("TileManager");
            tileManager = tileManagerObject.AddComponent<TileManager>();
        }

        // サンプルタイルを設定
        SetupSampleTiles();

        // 使用例を実行
        RunUsageExamples();
    }

    /// <summary>
    /// サンプルタイルを設定
    /// </summary>
    private void SetupSampleTiles()
    {
        Debug.Log("=== サンプルタイルを設定中 ===");

        // 木の家を設定（座標 2, 3）
        tileManager.SetTileType(2, 3, TileType.WoodenHouse);
        tileManager.AddStructure(2, 3, StructureType.Well);
        tileManager.AddPlant(2, 3, PlantType.Rose, 2);

        // 畑を設定（座標 4, 3）
        tileManager.SetTileType(4, 3, TileType.Field);
        tileManager.AddPlant(4, 3, PlantType.Wheat, 5);
        tileManager.AddPlant(4, 3, PlantType.Corn, 3);

        // 牧場を設定（座標 5, 4）
        tileManager.SetTileType(5, 4, TileType.Pasture);
        tileManager.AddStructure(5, 4, StructureType.Stable);
        tileManager.AddStructure(5, 4, StructureType.Fence);
        tileManager.AddAnimal(5, 4, AnimalType.Sheep, 4);
        tileManager.AddAnimal(5, 4, AnimalType.Cattle, 2);

        // レンガの家を設定（座標 1, 5）
        tileManager.SetTileType(1, 5, TileType.BrickHouse);
        tileManager.AddStructure(1, 5, StructureType.Granary);
        tileManager.AddPlant(1, 5, PlantType.Flower, 3);

        // 石の家を設定（座標 3, 1）
        tileManager.SetTileType(3, 1, TileType.StoneHouse);
        tileManager.AddStructure(3, 1, StructureType.Workshop);
        tileManager.AddStructure(3, 1, StructureType.Mill);

        // 追加の畑を設定（座標 6, 2）
        tileManager.SetTileType(6, 2, TileType.Field);
        tileManager.AddPlant(6, 2, PlantType.Vegetable, 4);
        tileManager.AddPlant(6, 2, PlantType.Potato, 2);

        Debug.Log("サンプルタイルの設定が完了しました");
    }

    /// <summary>
    /// 使用例を実行
    /// </summary>
    private void RunUsageExamples()
    {
        Debug.Log("\n=== TileManager使用例 ===");

        // 例1: 座標からタイル情報を取得
        ExampleGetTileInfo();

        // 例2: 特定の種類のタイルを検索
        ExampleSearchTilesByType();

        // 例3: 構造物があるタイルを検索
        ExampleSearchTilesByStructure();

        // 例4: 植物があるタイルを検索
        ExampleSearchTilesByPlant();

        // 例5: 動物がいるタイルを検索
        ExampleSearchTilesByAnimal();

        // 例6: 範囲内のタイルを取得
        ExampleGetTilesInRange();

        // 例7: タイルの動的変更
        ExampleDynamicTileChanges();
    }

    /// <summary>
    /// 例1: 座標からタイル情報を取得
    /// </summary>
    private void ExampleGetTileInfo()
    {
        Debug.Log("\n--- 例1: 座標からタイル情報を取得 ---");

        // 座標(5, 4)の牧場情報を取得
        Tile pastureInfo = tileManager.GetTile(5, 4);
        if (pastureInfo != null)
        {
            Debug.Log($"座標(5, 4)のタイル情報:");
            Debug.Log($"  タイル種類: {pastureInfo.tileType}");
            Debug.Log($"  構造物数: {pastureInfo.structures.Count}");
            Debug.Log($"  植物種類数: {pastureInfo.plants.Count}");
            Debug.Log($"  動物種類数: {pastureInfo.animals.Count}");
            
            // 動物の詳細情報
            foreach (var animal in pastureInfo.animals)
            {
                Debug.Log($"  - {animal.Key}: {animal.Value}匹");
            }
        }

        // より詳細な情報を表示
        tileManager.PrintTileInfo(5, 4);
    }

    /// <summary>
    /// 例2: 特定の種類のタイルを検索
    /// </summary>
    private void ExampleSearchTilesByType()
    {
        Debug.Log("\n--- 例2: 特定の種類のタイルを検索 ---");

        // すべての畑を取得
        List<Tile> fields = tileManager.GetTilesByType(TileType.Field);
        Debug.Log($"畑の数: {fields.Count}");
        foreach (var field in fields)
        {
            Debug.Log($"  畑の位置: ({field.position.x}, {field.position.y})");
        }

        // すべての牧場を取得
        List<Tile> pastures = tileManager.GetTilesByType(TileType.Pasture);
        Debug.Log($"牧場の数: {pastures.Count}");
        foreach (var pasture in pastures)
        {
            Debug.Log($"  牧場の位置: ({pasture.position.x}, {pasture.position.y})");
        }
    }

    /// <summary>
    /// 例3: 構造物があるタイルを検索
    /// </summary>
    private void ExampleSearchTilesByStructure()
    {
        Debug.Log("\n--- 例3: 構造物があるタイルを検索 ---");

        // 厩があるタイルを検索
        List<Tile> tilesWithStables = tileManager.GetTilesWithStructure(StructureType.Stable);
        Debug.Log($"厩がある場所: {tilesWithStables.Count}箇所");
        foreach (var tile in tilesWithStables)
        {
            Debug.Log($"  厩の位置: ({tile.position.x}, {tile.position.y}) - タイル種類: {tile.tileType}");
        }

        // 井戸があるタイルを検索
        List<Tile> tilesWithWells = tileManager.GetTilesWithStructure(StructureType.Well);
        Debug.Log($"井戸がある場所: {tilesWithWells.Count}箇所");
        foreach (var tile in tilesWithWells)
        {
            Debug.Log($"  井戸の位置: ({tile.position.x}, {tile.position.y}) - タイル種類: {tile.tileType}");
        }
    }

    /// <summary>
    /// 例4: 植物があるタイルを検索
    /// </summary>
    private void ExampleSearchTilesByPlant()
    {
        Debug.Log("\n--- 例4: 植物があるタイルを検索 ---");

        // 小麦があるタイルを検索
        List<Tile> tilesWithWheat = tileManager.GetTilesWithPlant(PlantType.Wheat);
        Debug.Log($"小麦がある場所: {tilesWithWheat.Count}箇所");
        foreach (var tile in tilesWithWheat)
        {
            int wheatCount = tile.GetPlantCount(PlantType.Wheat);
            Debug.Log($"  小麦の位置: ({tile.position.x}, {tile.position.y}) - 数量: {wheatCount}個");
        }

        // オークの木があるタイルを検索
        List<Tile> tilesWithOak = tileManager.GetTilesWithPlant(PlantType.Oak);
        Debug.Log($"オークがある場所: {tilesWithOak.Count}箇所");
        foreach (var tile in tilesWithOak)
        {
            int oakCount = tile.GetPlantCount(PlantType.Oak);
            Debug.Log($"  オークの位置: ({tile.position.x}, {tile.position.y}) - 数量: {oakCount}本");
        }
    }

    /// <summary>
    /// 例5: 動物がいるタイルを検索
    /// </summary>
    private void ExampleSearchTilesByAnimal()
    {
        Debug.Log("\n--- 例5: 動物がいるタイルを検索 ---");

        // 羊がいるタイルを検索
        List<Tile> tilesWithSheep = tileManager.GetTilesWithAnimal(AnimalType.Sheep);
        Debug.Log($"羊がいる場所: {tilesWithSheep.Count}箇所");
        foreach (var tile in tilesWithSheep)
        {
            int sheepCount = tile.GetAnimalCount(AnimalType.Sheep);
            Debug.Log($"  羊の位置: ({tile.position.x}, {tile.position.y}) - 数量: {sheepCount}匹");
        }

        // 魚がいるタイルを検索
        List<Tile> tilesWithFish = tileManager.GetTilesWithAnimal(AnimalType.Fish);
        Debug.Log($"魚がいる場所: {tilesWithFish.Count}箇所");
        foreach (var tile in tilesWithFish)
        {
            int fishCount = tile.GetAnimalCount(AnimalType.Fish);
            Debug.Log($"  魚の位置: ({tile.position.x}, {tile.position.y}) - 数量: {fishCount}匹");
        }
    }

    /// <summary>
    /// 例6: 範囲内のタイルを取得
    /// </summary>
    private void ExampleGetTilesInRange()
    {
        Debug.Log("\n--- 例6: 範囲内のタイルを取得 ---");

        // 座標(3, 3)を中心とした半径2の範囲内のタイルを取得
        Vector2Int center = new Vector2Int(3, 3);
        int range = 2;
        List<Tile> tilesInRange = tileManager.GetTilesInRange(center, range);
        
        Debug.Log($"座標({center.x}, {center.y})を中心とした半径{range}の範囲内のタイル数: {tilesInRange.Count}");
        
        int nonEmptyCount = 0;
        foreach (var tile in tilesInRange)
        {
            if (!tile.IsEmpty())
            {
                nonEmptyCount++;
                Debug.Log($"  位置({tile.position.x}, {tile.position.y}): {tile.tileType}");
            }
        }
        Debug.Log($"  うち空でないタイル: {nonEmptyCount}個");
    }

    /// <summary>
    /// 例7: タイルの動的変更
    /// </summary>
    private void ExampleDynamicTileChanges()
    {
        Debug.Log("\n--- 例7: タイルの動的変更 ---");

        Vector2Int targetPos = new Vector2Int(7, 3);
        
        // 最初は空き地
        Debug.Log($"座標({targetPos.x}, {targetPos.y})を畑に変更します");
        tileManager.SetTileType(targetPos, TileType.Field);
        
        // 植物を追加
        tileManager.AddPlant(targetPos, PlantType.Potato, 4);
        tileManager.AddPlant(targetPos, PlantType.Carrot, 3);
        Debug.Log("ジャガイモ4個とニンジン3個を植えました");
        
        // 構造物を追加
        tileManager.AddStructure(targetPos, StructureType.Well);
        Debug.Log("井戸を建設しました");
        
        // 現在の状態を確認
        tileManager.PrintTileInfo(targetPos);
        
        // 植物を収穫（削除）
        Tile tile = tileManager.GetTile(targetPos);
        if (tile != null)
        {
            int harvestedPotatoes = tile.GetPlantCount(PlantType.Potato);
            tile.RemovePlant(PlantType.Potato, harvestedPotatoes);
            Debug.Log($"ジャガイモ{harvestedPotatoes}個を収穫しました");
        }
        
        // 変更後の状態を確認
        tileManager.PrintTileInfo(targetPos);
    }

    /// <summary>
    /// プレイヤーの農場を検索する例
    /// </summary>
    public void SearchPlayerFarm()
    {
        Debug.Log("\n=== プレイヤーの農場検索例 ===");
        
        // プレイヤーの全農場情報を取得
        List<Tile> allFarms = new List<Tile>();
        allFarms.AddRange(tileManager.GetTilesByType(TileType.Field));
        allFarms.AddRange(tileManager.GetTilesByType(TileType.Pasture));
        allFarms.AddRange(tileManager.GetTilesByType(TileType.WoodenHouse));
        allFarms.AddRange(tileManager.GetTilesByType(TileType.BrickHouse));
        allFarms.AddRange(tileManager.GetTilesByType(TileType.StoneHouse));
        
        Debug.Log($"プレイヤーの農場エリア総数: {allFarms.Count}");
        
        // 農場の生産力を計算
        int totalPlants = 0;
        int totalAnimals = 0;
        int totalStructures = 0;
        
        foreach (var farm in allFarms)
        {
            totalPlants += farm.plants.Values.Sum();
            totalAnimals += farm.animals.Values.Sum();
            totalStructures += farm.structures.Count;
        }
        
        Debug.Log($"総植物数: {totalPlants}");
        Debug.Log($"総動物数: {totalAnimals}");
        Debug.Log($"総構造物数: {totalStructures}");
    }

    /// <summary>
    /// リソース生産量を計算する例
    /// </summary>
    public void CalculateResourceProduction()
    {
        Debug.Log("\n=== リソース生産量計算例 ===");
        
        // 食料生産量を計算
        int grainProduction = 0;
        int meatProduction = 0;
        
        // 畑からの穀物生産
        List<Tile> fields = tileManager.GetTilesByType(TileType.Field);
        foreach (var field in fields)
        {
            grainProduction += field.GetPlantCount(PlantType.Wheat);
            grainProduction += field.GetPlantCount(PlantType.Corn);
            grainProduction += field.GetPlantCount(PlantType.Grain);
        }
        
        // 牧場からの肉生産
        List<Tile> pastures = tileManager.GetTilesByType(TileType.Pasture);
        foreach (var pasture in pastures)
        {
            meatProduction += pasture.GetAnimalCount(AnimalType.Cattle) * 3; // 牛1匹=肉3個
            meatProduction += pasture.GetAnimalCount(AnimalType.Sheep) * 2;  // 羊1匹=肉2個
            meatProduction += pasture.GetAnimalCount(AnimalType.Boar) * 2;   // 猪1匹=肉2個
        }
        
        Debug.Log($"穀物生産量: {grainProduction}個/ターン");
        Debug.Log($"肉生産量: {meatProduction}個/ターン");
        Debug.Log($"総食料生産量: {grainProduction + meatProduction}個/ターン");
    }

    /// <summary>
    /// 建設可能エリアを検索する例
    /// </summary>
    public void FindBuildableAreas()
    {
        Debug.Log("\n=== 建設可能エリア検索例 ===");
        
        // 空き地を取得
        List<Tile> emptyTiles = tileManager.GetEmptyTiles();
        Debug.Log($"建設可能な空き地: {emptyTiles.Count}箇所");
        
        // 拡張可能な既存農場を検索
        List<Tile> expandableFields = tileManager.GetTilesByType(TileType.Field)
            .Where(field => field.plants.Values.Sum() < 10) // 植物が10個未満の畑
            .ToList();
        Debug.Log($"拡張可能な畑: {expandableFields.Count}箇所");
        
        // 動物追加可能な牧場を検索
        List<Tile> expandablePastures = tileManager.GetTilesByType(TileType.Pasture)
            .Where(pasture => pasture.animals.Values.Sum() < 8) // 動物が8匹未満の牧場
            .ToList();
        Debug.Log($"動物追加可能な牧場: {expandablePastures.Count}箇所");
    }

    // Update関数でキー入力による例の実行
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SearchPlayerFarm();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CalculateResourceProduction();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            FindBuildableAreas();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            tileManager.PrintAllTiles();
        }
    }
}