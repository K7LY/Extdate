# タイル管理システム (TileManager)

このシステムは、座標ベースでタイルの種類、構造物、植物、動物の情報を管理するための包括的なソリューションです。

## 概要

新しいタイル管理システムでは、以下の要素を座標単位で管理できます：

- **タイル種類**: 家、牧場、空き地、畑、森、山、川、特殊タイルなど
- **構造物**: 厩、柵、井戸、製粉所、作業場、倉庫など（複数可）
- **植物**: 穀物、野菜、果物、木、花など（種類と数量）
- **動物**: 羊、牛、豚、鶏、魚など（種類と数量）

## クイックスタート

### 1. TileManagerの設定

```csharp
// GameObjectにTileManagerコンポーネントを追加
GameObject tileManagerObject = new GameObject("TileManager");
TileManager tileManager = tileManagerObject.AddComponent<TileManager>();
```

### 2. 基本的な使用方法

```csharp
// タイル種類を設定
tileManager.SetTileType(2, 3, TileType.Field);

// 構造物を追加
tileManager.AddStructure(2, 3, StructureType.Well);

// 植物を植える
tileManager.AddPlant(2, 3, PlantType.Wheat, 5);

// 動物を追加
tileManager.AddAnimal(2, 3, AnimalType.Sheep, 3);

// 座標からタイル情報を取得
Tile tileInfo = tileManager.GetTile(2, 3);
```

### 3. 情報の取得

```csharp
// 座標からタイル情報を取得
Tile tile = tileManager.GetTile(x, y);
if (tile != null)
{
    // タイル種類
    TileType type = tile.tileType;
    
    // 構造物の確認
    bool hasStable = tile.HasStructure(StructureType.Stable);
    
    // 植物の数量取得
    int wheatCount = tile.GetPlantCount(PlantType.Wheat);
    
    // 動物の数量取得
    int sheepCount = tile.GetAnimalCount(AnimalType.Sheep);
}
```

## 主要クラス

### Tile クラス

個別のタイル情報を管理するクラスです。

#### プロパティ
- `Vector2Int position`: タイルの座標
- `TileType tileType`: タイルの種類
- `List<StructureType> structures`: 構造物リスト
- `Dictionary<PlantType, int> plants`: 植物の種類と数量
- `Dictionary<AnimalType, int> animals`: 動物の種類と数量

#### 主要メソッド
- `AddStructure(StructureType)`: 構造物を追加
- `RemoveStructure(StructureType)`: 構造物を削除
- `HasStructure(StructureType)`: 構造物の存在確認
- `AddPlant(PlantType, int)`: 植物を追加
- `RemovePlant(PlantType, int)`: 植物を削除
- `GetPlantCount(PlantType)`: 植物の数量取得
- `AddAnimal(AnimalType, int)`: 動物を追加
- `RemoveAnimal(AnimalType, int)`: 動物を削除
- `GetAnimalCount(AnimalType)`: 動物の数量取得
- `IsEmpty()`: タイルが空かどうか

### TileManager クラス

タイルマップ全体を管理するメインクラスです。

#### 座標ベース操作
- `GetTile(int x, int y)`: 座標からタイル取得
- `SetTileType(int x, int y, TileType)`: タイル種類設定
- `AddStructure(int x, int y, StructureType)`: 構造物追加
- `AddPlant(int x, int y, PlantType, int)`: 植物追加
- `AddAnimal(int x, int y, AnimalType, int)`: 動物追加

#### 検索機能
- `GetTilesByType(TileType)`: 指定タイプのタイル検索
- `GetTilesWithStructure(StructureType)`: 指定構造物があるタイル検索
- `GetTilesWithPlant(PlantType)`: 指定植物があるタイル検索
- `GetTilesWithAnimal(AnimalType)`: 指定動物がいるタイル検索
- `GetEmptyTiles()`: 空タイル取得
- `GetTilesInRange(Vector2Int center, int range)`: 範囲内タイル取得

## 使用例

### 例1: 農場の設定

```csharp
// 畑を作成
tileManager.SetTileType(3, 4, TileType.Field);
tileManager.AddPlant(3, 4, PlantType.Wheat, 6);
tileManager.AddPlant(3, 4, PlantType.Corn, 4);
tileManager.AddStructure(3, 4, StructureType.Well);

// 牧場を作成
tileManager.SetTileType(5, 4, TileType.Pasture);
tileManager.AddStructure(5, 4, StructureType.Stable);
tileManager.AddStructure(5, 4, StructureType.Fence);
tileManager.AddAnimal(5, 4, AnimalType.Sheep, 8);
tileManager.AddAnimal(5, 4, AnimalType.Cattle, 3);
```

### 例2: 情報の検索と分析

```csharp
// 全ての畑を取得
List<Tile> allFields = tileManager.GetTilesByType(TileType.Field);

// 小麦がある場所を検索
List<Tile> wheatFields = tileManager.GetTilesWithPlant(PlantType.Wheat);

// 厩がある場所を検索
List<Tile> stablesLocations = tileManager.GetTilesWithStructure(StructureType.Stable);

// リソース生産量を計算
int totalWheat = 0;
foreach (var field in wheatFields)
{
    totalWheat += field.GetPlantCount(PlantType.Wheat);
}
```

### 例3: 動的な変更

```csharp
// 植物の収穫
Tile field = tileManager.GetTile(3, 4);
if (field != null)
{
    int wheatAmount = field.GetPlantCount(PlantType.Wheat);
    field.RemovePlant(PlantType.Wheat, wheatAmount); // 全て収穫
    
    // 新しい作物を植える
    field.AddPlant(PlantType.Vegetable, 5);
}
```

### 例4: 範囲検索

```csharp
// 座標(5, 5)を中心とした半径3の範囲内のタイルを取得
Vector2Int center = new Vector2Int(5, 5);
List<Tile> nearbyTiles = tileManager.GetTilesInRange(center, 3);

// 範囲内の動物数を集計
int totalAnimals = 0;
foreach (var tile in nearbyTiles)
{
    totalAnimals += tile.animals.Values.Sum();
}
```

## エディタ機能

### Gizmosによる可視化

Sceneビューで以下の色分けでタイルが表示されます：
- **白**: 空き地
- **赤**: 家
- **緑**: 畑
- **黄**: 牧場
- **シアン**: 森
- **グレー**: 山
- **青**: 川
- **マゼンタ**: 特殊タイル

構造物がある場合は黒い小立方体が表示されます。

### デバッグ機能

```csharp
// 特定タイルの詳細情報を表示
tileManager.PrintTileInfo(x, y);

// 全タイル情報を表示
tileManager.PrintAllTiles();
```

## イベントシステム

TileManagerは以下のイベントを提供します：

```csharp
// イベントの購読
tileManager.OnTileChanged += (position, tile) => {
    Debug.Log($"タイル({position.x}, {position.y})が変更されました");
};

tileManager.OnTileTypeChanged += (position, tileType) => {
    Debug.Log($"座標({position.x}, {position.y})のタイプが{tileType}に変更されました");
};

tileManager.OnStructureAdded += (position, structureType) => {
    Debug.Log($"座標({position.x}, {position.y})に{structureType}が追加されました");
};
```

## カスタムプロパティ

タイルには任意のカスタムプロパティを設定できます：

```csharp
Tile tile = tileManager.GetTile(x, y);
tile.SetCustomProperty("fertility", 85);     // 肥沃度
tile.SetCustomProperty("lastHarvest", DateTime.Now); // 最終収穫日
tile.description = "高品質な小麦畑";           // 説明

// 取得
int fertility = tile.GetCustomProperty<int>("fertility");
DateTime lastHarvest = tile.GetCustomProperty<DateTime>("lastHarvest");
```

## 最適化のヒント

### パフォーマンス
- 大量の検索を行う場合は、結果をキャッシュすることを推奨
- 毎フレーム検索を行わず、必要時のみ実行する
- `GetTilesInRange`は範囲が大きいと重いので注意

### メモリ使用量
- 使用しないタイルはClearTileMap()でクリア
- 大きなボードサイズは必要に応じて調整

## APIリファレンス

### TileType 列挙型
- `Empty`: 空き地
- `House`: 家
- `Field`: 畑
- `Pasture`: 牧場
- `Forest`: 森
- `Mountain`: 山
- `River`: 川
- `Special`: 特殊タイル

### StructureType 列挙型
- `Stable`: 厩
- `Fence`: 柵
- `Well`: 井戸
- `Mill`: 製粉所
- `Workshop`: 作業場
- `Granary`: 穀物倉庫
- `Warehouse`: 倉庫
- `Bridge`: 橋
- `Tower`: 塔
- `Garden`: 庭園

### PlantType 列挙型
- `Grain`: 穀物
- `Vegetable`: 野菜
- `Fruit`: 果物
- `Wheat`: 小麦
- `Corn`: トウモロコシ
- `Potato`: ジャガイモ
- `Carrot`: ニンジン
- `Apple`: リンゴ
- など

### AnimalType 列挙型
- `Sheep`: 羊
- `Cattle`: 牛
- `Boar`: 猪
- `Horse`: 馬
- `Chicken`: 鶏
- `Pig`: 豚
- `Fish`: 魚
- など

## 実行方法

1. `TileManagerExample`コンポーネントをGameObjectにアタッチ
2. プレイモードでF1〜F4キーで各種テスト実行：
   - **F1**: プレイヤー農場検索
   - **F2**: リソース生産量計算
   - **F3**: 建設可能エリア検索
   - **F4**: 全タイル情報表示

## 拡張性

このシステムは以下の方向で拡張可能です：

- 新しいタイル種類の追加
- 新しい構造物/植物/動物タイプの追加
- 複雑な検索条件の実装
- セーブ/ロード機能の追加
- ネットワーク同期対応
- UI統合

詳細な実装例は`TileManagerExample.cs`を参照してください。