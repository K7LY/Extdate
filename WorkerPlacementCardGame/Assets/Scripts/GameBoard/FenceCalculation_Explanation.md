# 柵の配置計算 - 2×2牧場に8個の柵が必要な理由

## 概要

2×2の範囲を完全に囲むには、**8個の柵**が必要です。これは数学的に計算できます。

## 座標系の理解

### タイルと柵の関係

```
座標システム:
  0   1   2   3   4
0 +---+---+---+---+
  |   |   |   |   |
1 +---+---+---+---+
  |   | ■ | ■ |   |  ← 牧場タイル (1,1) と (2,1)
2 +---+---+---+---+
  |   | ■ | ■ |   |  ← 牧場タイル (1,2) と (2,2)
3 +---+---+---+---+
  |   |   |   |   |
4 +---+---+---+---+
```

### 柵の配置位置

各柵は「基準タイル」の座標と方向で指定されます：

- **水平柵 (Horizontal)**: タイルの上側境界
- **垂直柵 (Vertical)**: タイルの右側境界

## 2×2牧場に必要な柵の詳細計算

### 牧場タイル
- (1,1), (2,1), (1,2), (2,2)

### 境界分析

#### 1. 下境界（水平柵）
```
  1   2   
+===+===+  ← これらの水平柵が必要
| ■ | ■ |
+---+---+
```
- `(1,1) Horizontal` - タイル(1,1)の下境界
- `(2,1) Horizontal` - タイル(2,1)の下境界

#### 2. 上境界（水平柵）
```
+---+---+
| ■ | ■ |
+===+===+  ← これらの水平柵が必要
  1   2   
```
- `(1,3) Horizontal` - タイル(1,2)の上境界
- `(2,3) Horizontal` - タイル(2,2)の上境界

#### 3. 左境界（垂直柵）
```
‖ ■ |
‖ ■ |
↑
これらの垂直柵が必要
```
- `(0,1) Vertical` - タイル(1,1)の左境界
- `(0,2) Vertical` - タイル(1,2)の左境界

#### 4. 右境界（垂直柵）
```
| ■ ‖
| ■ ‖
    ↑
    これらの垂直柵が必要
```
- `(2,1) Vertical` - タイル(2,1)の右境界
- `(2,2) Vertical` - タイル(2,2)の右境界

## 完全な柵配置コード

```csharp
// 2×2牧場の完全な囲い
// 水平柵（上下境界）- 4個
pastureManager.AddFence(1, 1, FenceDirection.Horizontal);  // 下境界1
pastureManager.AddFence(2, 1, FenceDirection.Horizontal);  // 下境界2
pastureManager.AddFence(1, 3, FenceDirection.Horizontal);  // 上境界1
pastureManager.AddFence(2, 3, FenceDirection.Horizontal);  // 上境界2

// 垂直柵（左右境界）- 4個
pastureManager.AddFence(0, 1, FenceDirection.Vertical);    // 左境界1
pastureManager.AddFence(0, 2, FenceDirection.Vertical);    // 左境界2
pastureManager.AddFence(2, 1, FenceDirection.Vertical);    // 右境界1
pastureManager.AddFence(2, 2, FenceDirection.Vertical);    // 右境界2

// 合計：8個の柵
```

## 一般的な計算式

### n×m の矩形牧場に必要な柵の数

```
水平柵の数 = n × 2        (上辺と下辺)
垂直柵の数 = m × 2        (左辺と右辺)
総柵数 = 2 × (n + m)
```

### 例
- **2×2牧場**: 2×(2+2) = 8個
- **3×2牧場**: 2×(3+2) = 10個
- **1×3牧場**: 2×(1+3) = 8個

## ビジュアル表現

### 完全に囲まれた2×2牧場
```
  0   1   2   3
0 +---+---+---+
  |   |   |   |
1 +---+===+===+---+
  |   ‖ ■ | ■ ‖   |
2 +---+===+===+---+
  |   ‖ ■ | ■ ‖   |
3 +---+===+===+---+
  |   |   |   |
4 +---+---+---+
```

### 柵の種類別表示
```
=== : 水平柵 (Horizontal)
‖   : 垂直柵 (Vertical)
■   : 牧場タイル
```

## よくある間違い

### ❌ 間違った配置（4個の柵のみ）
```csharp
// 不完全な囲い - 牧場として検出されない
pastureManager.AddFence(1, 1, FenceDirection.Horizontal);  // 不十分
pastureManager.AddFence(1, 3, FenceDirection.Horizontal);  // 不十分
pastureManager.AddFence(0, 1, FenceDirection.Vertical);    // 不十分
pastureManager.AddFence(2, 1, FenceDirection.Vertical);    // 不十分
```

この場合、境界が完全に閉じられていないため、Flood Fillアルゴリズムで牧場として認識されません。

### ✅ 正しい配置（8個の柵）
すべての境界が完全に閉じられているため、正しく牧場として検出されます。

## アルゴリズムとの関係

### Flood Fill検出の仕組み
1. 任意のタイルから開始
2. 隣接タイルに移動を試みる
3. **柵があると移動が阻止される**
4. 移動できる範囲が牧場として判定される

### 重要なポイント
- すべての境界に柵がないと「外部」と繋がってしまう
- 一つでも柵が欠けていると、牧場として検出されない
- 8個すべての柵が揃って初めて「囲まれた領域」となる

## デバッグのヒント

### 牧場が検出されない場合のチェックリスト
1. **柵の総数**: 2×(幅+高さ) = 期待値
2. **境界の完全性**: すべての辺が閉じられているか
3. **座標の正確性**: 柵の位置が正しいか
4. **方向の正確性**: Horizontal/Verticalが正しいか

### デバッグコマンド
```csharp
// 配置された柵の確認
Debug.Log($"配置された柵の総数: {pastureManager.GetAllFences().Count}");

// 各柵の詳細確認
foreach (var fence in pastureManager.GetAllFences())
{
    Debug.Log($"柵: {fence.GetFenceId()}");
}

// 牧場検出結果
pastureManager.PrintAllPasturesInfo();
```

## まとめ

2×2の牧場を作るには**必ず8個の柵**が必要です。これは：
- 上下境界：各2個 × 2 = 4個
- 左右境界：各2個 × 2 = 4個
- **合計：8個**

この理解により、任意のサイズの牧場に必要な柵の数を正確に計算できます。