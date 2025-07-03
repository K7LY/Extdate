# 牧場管理システム (PastureManager)

## 概要

牧場管理システムは、タイル間に配置される「柵」によって囲まれた領域を自動的に検出し、「牧場」として管理するシステムです。柵の配置・削除に応じて、囲まれた領域をリアルタイムで判定し、動物の管理も行えます。

## 主要機能

### 1. 柵管理
- **配置**: タイル間の境界に水平・垂直の柵を配置
- **削除**: 指定した柵を削除
- **検証**: 柵の存在確認と重複防止

### 2. 牧場自動検出
- **領域検出**: 柵で囲まれた領域をFlood Fillアルゴリズムで自動検出
- **リアルタイム更新**: 柵の変更に応じて牧場を再計算
- **複数牧場対応**: 複数の独立した牧場を同時管理

### 3. 動物管理
- **容量管理**: 牧場の面積に基づく動物収容数の自動計算
- **動物配置**: 種類別の動物の追加・削除
- **制限チェック**: 容量オーバーの防止

## クラス構成

### `Fence` クラス
```csharp
public class Fence
{
    public Vector2Int position;         // 柵の位置
    public FenceDirection direction;    // 向き（水平/垂直）
    public bool isActive;              // 有効状態
}
```

### `Pasture` クラス
```csharp
public class Pasture
{
    public int id;                              // 牧場ID
    public List<Vector2Int> tiles;              // 含まれるタイル
    public List<string> boundaryFences;         // 境界柵のID
    public int capacity;                        // 動物収容容量
    public Dictionary<AnimalType, int> animals; // 配置された動物
}
```

### `PastureManager` クラス
メインの管理クラス。柵の配置・削除、牧場の自動検出、動物管理を統括します。

## 基本的な使用方法

### 1. セットアップ

```csharp
// TileManagerとPastureManagerをシーンに配置
public class GameController : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;
    [SerializeField] private PastureManager pastureManager;
    
    void Start()
    {
        // PastureManagerはTileManagerを自動で見つけますが、
        // 明示的に設定することも可能です
    }
}
```

### 2. 柵の配置

```csharp
// 水平柵の配置（タイルの上下境界）
pastureManager.AddFence(2, 3, FenceDirection.Horizontal);

// 垂直柵の配置（タイルの左右境界）
pastureManager.AddFence(2, 3, FenceDirection.Vertical);

// 座標版
pastureManager.AddFence(new Vector2Int(2, 3), FenceDirection.Horizontal);
```

### 3. 牧場の作成例

```csharp
// 2x2の正方形牧場を作成
pastureManager.AddFence(1, 1, FenceDirection.Horizontal);  // 下辺
pastureManager.AddFence(1, 3, FenceDirection.Horizontal);  // 上辺
pastureManager.AddFence(1, 1, FenceDirection.Vertical);    // 左辺
pastureManager.AddFence(3, 1, FenceDirection.Vertical);    // 右辺

// 牧場は自動的に検出されます
var pastures = pastureManager.GetAllPastures();
Debug.Log($"検出された牧場数: {pastures.Count}");
```

### 4. 動物の管理

```csharp
// 牧場に動物を追加
int pastureId = 1;
bool success = pastureManager.AddAnimalToPasture(pastureId, AnimalType.Sheep, 3);

// 動物を削除
pastureManager.RemoveAnimalFromPasture(pastureId, AnimalType.Sheep, 1);

// 牧場情報の確認
var pasture = pastureManager.GetAllPastures().FirstOrDefault();
if (pasture != null)
{
    Debug.Log($"面積: {pasture.GetArea()}タイル");
    Debug.Log($"容量: {pasture.capacity}匹");
    Debug.Log($"現在の動物数: {pasture.GetTotalAnimalCount()}匹");
}
```

## 座標系と柵の配置

### 柵の座標系

柵は「基準タイル」の座標と向きで指定します：

```
水平柵 (Horizontal): タイル(x,y)の上側境界
┌─────┬─────┐
│     │     │
│(x,y)│(x+1,y)│ ← 水平柵は(x,y)で指定
│     │     │
├─────┼─────┤
│     │     │
│(x,y-1)│(x+1,y-1)│
└─────┴─────┘

垂直柵 (Vertical): タイル(x,y)の右側境界
┌─────┬─────┐
│     │     │
│(x,y)│(x+1,y)│
│     ┃     │ ← 垂直柵は(x,y)で指定
├─────╂─────┤
│     ┃     │
│(x,y-1)│(x+1,y-1)│
└─────┴─────┘
```

### 完全な牧場の例

```csharp
// 3x2の牧場を作成
// ■■■
// ■■■

// 下辺の水平柵
pastureManager.AddFence(0, 0, FenceDirection.Horizontal);
pastureManager.AddFence(1, 0, FenceDirection.Horizontal);
pastureManager.AddFence(2, 0, FenceDirection.Horizontal);

// 上辺の水平柵
pastureManager.AddFence(0, 2, FenceDirection.Horizontal);
pastureManager.AddFence(1, 2, FenceDirection.Horizontal);
pastureManager.AddFence(2, 2, FenceDirection.Horizontal);

// 左辺の垂直柵
pastureManager.AddFence(-1, 0, FenceDirection.Vertical);
pastureManager.AddFence(-1, 1, FenceDirection.Vertical);

// 右辺の垂直柵
pastureManager.AddFence(2, 0, FenceDirection.Vertical);
pastureManager.AddFence(2, 1, FenceDirection.Vertical);
```

## イベントシステム

PastureManagerは各種イベントを発行します：

```csharp
void Start()
{
    // 柵関連イベント
    pastureManager.OnFenceAdded += (fenceId, fence) => {
        Debug.Log($"柵が追加されました: {fenceId}");
    };
    
    pastureManager.OnFenceRemoved += (fenceId) => {
        Debug.Log($"柵が削除されました: {fenceId}");
    };
    
    // 牧場関連イベント
    pastureManager.OnPastureCreated += (pastureId, pasture) => {
        Debug.Log($"牧場が作成されました: {pastureId}");
    };
    
    pastureManager.OnPastureDestroyed += (pastureId) => {
        Debug.Log($"牧場が削除されました: {pastureId}");
    };
    
    pastureManager.OnPastureUpdated += (pastureId, pasture) => {
        Debug.Log($"牧場が更新されました: {pastureId}");
    };
}
```

## 設定パラメータ

```csharp
[Header("設定")]
[SerializeField] private int defaultCapacityPerTile = 2; // タイル1つあたりの動物収容数
[SerializeField] private bool enableDebugLog = true;     // デバッグログの有効/無効
```

## デバッグ機能

### 1. 情報表示

```csharp
// 特定の牧場情報を表示
pastureManager.PrintPastureInfo(1);

// すべての牧場情報を表示
pastureManager.PrintAllPasturesInfo();
```

### 2. ビジュアルデバッグ

Scene ViewでGizmosを有効にすると：
- **茶色の線**: 配置された柵
- **カラフルなキューブ**: 牧場の領域（牧場ごとに異なる色）

### 3. Context Menu

PastureManagerExampleコンポーネントを使用すると、右クリックメニューから：
- "小さな牧場を作成"
- "大きな牧場を作成" 
- "すべての柵をクリア"
- "動物を追加テスト"

## 利用例とテスト

`PastureManagerExample`クラスには包括的なテストが含まれています：

1. **基本的な柵の配置テスト**
2. **正方形の牧場作成テスト**
3. **L字型の牧場作成テスト**
4. **動物管理テスト**
5. **複数牧場テスト**

### 実行方法

```csharp
// シーンにPastureManagerExampleを配置
// runTestOnStart = true に設定してプレイ
```

## 高度な使用例

### カスタム牧場判定

```csharp
// 特定位置が牧場内かどうか確認
var pasture = pastureManager.GetPastureAtPosition(2, 3);
if (pasture != null)
{
    Debug.Log($"座標(2,3)は牧場{pasture.id}内です");
}
```

### 牧場の動的変更

```csharp
// 柵を追加して牧場を拡張
pastureManager.AddFence(5, 2, FenceDirection.Vertical);

// 柵を削除して牧場を統合
pastureManager.RemoveFence(3, 2, FenceDirection.Vertical);

// 変更は自動的に反映されます
```

### 動物の一括管理

```csharp
// すべての牧場の動物を確認
foreach (var pasture in pastureManager.GetAllPastures())
{
    Debug.Log($"牧場{pasture.id}: {pasture.GetTotalAnimalCount()}匹");
    foreach (var animal in pasture.animals)
    {
        Debug.Log($"  {animal.Key}: {animal.Value}匹");
    }
}
```

## 注意点

1. **座標系**: 柵の座標は基準タイルの座標で指定します
2. **境界判定**: タイルが存在しない場所には柵を配置できません
3. **容量制限**: 動物の追加時は自動的に容量チェックが行われます
4. **リアルタイム更新**: 柵の変更時は自動的に牧場が再計算されます

## トラブルシューティング

### 牧場が検出されない
- すべての境界が柵で囲まれているか確認
- 柵の向きが正しいか確認
- デバッグログを有効にして詳細を確認

### 動物が追加できない
- 牧場の容量を確認 (`pasture.capacity`)
- 現在の動物数を確認 (`pasture.GetTotalAnimalCount()`)

### 柵が配置できない
- 隣接するタイルが存在するか確認
- 同じ位置に柵が既に存在しないか確認