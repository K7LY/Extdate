# 収穫時の処理確認レポート

## 概要
このレポートは、ワーカープレイスメントカードゲームの収穫処理システムについて詳細に分析したものです。

## 収穫処理の流れ

### 1. 収穫ラウンドの判定
**場所**: `GameManager.cs`の`IsHarvestRound()`メソッド
```csharp
// 収穫ラウンド: 4, 7, 9, 11, 13, 14
public int[] harvestRounds = { 4, 7, 9, 11, 13, 14 };
```

### 2. 収穫フェーズの実行
**場所**: `GameManager.cs`の`ExecuteHarvest()`メソッド

収穫フェーズでは以下の処理が順番に実行されます：

#### 2.1 作物の収穫
- 各プレイヤーに対して`player.HarvestCrops()`を呼び出し
- 収穫時のカード効果を発動: `ExecuteAllTriggerableCards(OccupationTrigger.OnHarvest, player)`

#### 2.2 家族の餌やり
- `player.FeedFamily()`で食料消費
- 食料不足の場合、乞食カードを受け取る（-3点ペナルティ）

#### 2.3 動物の繁殖
- `player.BreedAnimals()`で動物繁殖
- 繁殖時のカード効果を発動: `ExecuteAllTriggerableCards(OccupationTrigger.OnBreeding, player)`

### 3. プレイヤーレベルの収穫処理
**場所**: `Player.cs`の`HarvestCrops()`メソッド

```csharp
public void HarvestCrops()
{
    if (tileManager == null)
    {
        Debug.LogWarning($"{playerName}: TileManagerが設定されていないため、収穫できません");
        return;
    }
    
    var harvestedCrops = tileManager.HarvestCrops(this);
    
    // 職業効果のトリガー
    TriggerOccupationEffects(OccupationTrigger.OnHarvest);
}
```

### 4. 実際の収穫処理
**場所**: `TileManager.cs`の`HarvestCrops()`メソッド

#### 4.1 処理の詳細
1. **畑の座標取得**: `player.GetAllFieldPositions()`で全畑の座標を取得
2. **各畑の作物確認**: 各座標の畑に植えられた作物を確認
3. **作物の収穫**: 
   - `field.HarvestCrop(cropType, 1)`で畑から1個ずつ収穫
   - `player.ReceiveResourceDirect(cropType, harvestedAmount, null, "harvest")`でリソース追加
4. **タイルマップの更新**: `GetTile(position)?.RemovePlant(plantType, harvestedAmount)`でタイルから植物を削除
5. **統計記録**: 収穫量を記録・ログ出力

#### 4.2 収穫ルール
- 各畑から1個ずつ収穫
- 収穫した作物は自動的にプレイヤーのリソースに追加
- 畑とTileManagerの両方から作物を削除

#### 4.3 デバッグ情報
```csharp
Debug.Log($"🌾 TileManager: {player.playerName}の収穫を開始します");
Debug.Log($"    座標({position.x}, {position.y})から{GetResourceName(cropType)}を{harvestedAmount}個収穫");
Debug.Log($"  {player.playerName}の収穫結果:");
```

## カード効果の発動

### 収穫時トリガー
**トリガータイプ**: `OccupationTrigger.OnHarvest`

収穫時に発動する職業カードの例：
- **農夫**: 収穫時に追加穀物1個を獲得
- その他の職業効果も条件に応じて発動

### 繁殖時トリガー
**トリガータイプ**: `OccupationTrigger.OnBreeding`

動物繁殖時に発動する職業カードの効果

## 食料システム

### 家族の餌やり
**場所**: `Player.cs`の`FeedFamily()`メソッド

#### 必要食料の計算
```csharp
public int GetFoodNeeded()
{
    return familyMembers * 2; // 1人につき2食料
}
```

#### 食料不足時の処理
1. 利用可能な食料を優先使用
2. ResourceConverterで他のリソースから食料を自動変換
3. それでも不足の場合、乞食カードを受け取る

## 動物の繁殖

### 繁殖ルール
**場所**: `Player.cs`の`BreedAnimals()`メソッド

```csharp
// 各動物種で2匹以上いれば1匹増える
if (GetResource(ResourceType.Sheep) >= 2 && CanHouseAnimals(ResourceType.Sheep, 1))
    ReceiveResourceDirect(ResourceType.Sheep, 1, null, "breeding");
```

### 飼育可能数の制限
```csharp
public bool CanHouseAnimals(ResourceType animalType, int count)
{
    int capacity = pastures * 2 + stables; // 牧場1つにつき2匹、小屋1つにつき1匹
    int currentAnimals = GetResource(ResourceType.Sheep) + GetResource(ResourceType.Boar) + GetResource(ResourceType.Cattle);
    
    return currentAnimals + count <= capacity;
}
```

## 収穫の最適化

### 5人プレイでの追加アクション
**場所**: `FivePlayerActionSpaceSetup.cs`

- **葦の収穫**: 葦2個獲得
- **野菜の収穫**: 野菜を収穫
- **収穫の最適化**: 次の収穫で追加の収穫物を得る

### 一時的ボーナス
**場所**: `ActionSpace.cs`の収穫最適化処理

```csharp
case "収穫の最適化":
    player.AddTempBonus("harvest_bonus", 1);
    Debug.Log($"{player.playerName}が収穫最適化効果を得ました");
```

## テスト・デバッグ機能

### 収穫テスト
複数のテストファイルで収穫処理のテストが実装されています：
- `ComprehensiveGameTest.cs`
- `FivePlayerGameTest.cs`
- `CardSystemTest.cs`

### デバッグ出力
- 収穫開始/終了の詳細ログ
- 各畑からの収穫量
- 総収穫量の統計
- カード効果の発動状況

## 統合システム

### TileManagerとの連携
- Player.csの畑システムとTileManager.csのタイルマップが連携
- 種まき時：両方に作物を追加
- 収穫時：両方から作物を削除

### カードトリガーシステム
- CardTriggerManagerが収穫時の職業効果を管理
- 条件チェックと効果実行を自動化

## 今後の改善点

1. **収穫量の調整**: 現在は1個固定、将来的に複数個収穫の可能性
2. **季節システム**: 作物の成長時間や季節による収穫量変動
3. **天候システム**: 収穫に影響を与える天候効果
4. **収穫道具**: 収穫効率を向上させる道具の実装

## まとめ

収穫処理システムは以下の特徴を持ちます：
- **多層構造**: GameManager → Player → TileManager の階層的処理
- **完全統合**: 畑システム、タイルマップ、カード効果が連携
- **自動化**: 収穫から餌やり、繁殖まで自動実行
- **拡張性**: 新しいカード効果や特殊ルールに対応可能
- **デバッグ対応**: 詳細なログ出力でテスト・調整が容易

この仕組みによって、Agricolaスタイルの複雑な収穫システムが実現されています。