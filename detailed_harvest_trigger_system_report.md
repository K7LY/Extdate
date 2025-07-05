# 細かな収穫トリガーシステム実装レポート

## 概要
収穫処理のカード効果発動タイミングを、従来の2つのトリガーから6つの細かなフェーズに分割し、より戦略的で詳細な収穫システムを実装しました。

## 新しいトリガーシステム

### 1. BeforeHarvest（収穫の直前）
**タイミング**: 収穫作業が始まる直前
**用途**: 畑の準備、道具の手入れ、事前ボーナス
**実装例**:
```csharp
case OccupationTrigger.BeforeHarvest:
    // 農夫: 収穫前に畑の準備で穀物+1
    player.AddResource(ResourceType.Grain, 1);
    Debug.Log($"農夫効果発動: 収穫前の畑準備で穀物1個獲得");
    break;
```

### 2. HarvestStart（収穫の開始時）
**タイミング**: 収穫作業の開始時
**用途**: 収穫効率の向上、全体的な指導効果
**実装例**:
```csharp
case OccupationTrigger.HarvestStart:
    // 学者: 収穫開始時に知識による効率化
    player.AddTempBonus("harvest_efficiency", 1);
    Debug.Log($"学者効果発動: 収穫開始時に効率化ボーナス獲得");
    break;
```

### 3. FieldPhase（畑フェーズ）
**タイミング**: 実際の作物収穫中
**用途**: 収穫量の増加、作物の品質向上
**実装例**:
```csharp
case OccupationTrigger.FieldPhase:
    // 職人: 畑フェーズで野菜の品質向上
    int vegetableCount = player.GetResource(ResourceType.Vegetable);
    if (vegetableCount > 0)
    {
        player.AddResource(ResourceType.Food, vegetableCount);
        Debug.Log($"職人効果発動: 野菜{vegetableCount}個から食料{vegetableCount}個獲得");
    }
    break;
```

### 4. FeedingPhase（食料供給フェーズ）
**タイミング**: 家族への食料供給時
**用途**: 食料不足の補填、変換効率の向上
**実装例**:
```csharp
case OccupationTrigger.FeedingPhase:
    // パン職人: 食料供給フェーズで穀物→食料変換効率UP
    int grainCount = player.GetResource(ResourceType.Grain);
    if (grainCount >= 2)
    {
        player.SpendResource(ResourceType.Grain, 2);
        player.AddResource(ResourceType.Food, 3); // 通常2→3に効率UP
        Debug.Log($"パン職人効果発動: 穀物2個→食料3個に変換（効率UP）");
    }
    break;
```

### 5. BreedingPhase（繁殖フェーズ）
**タイミング**: 動物の繁殖時
**用途**: 繁殖効率の向上、動物の追加獲得
**実装例**:
```csharp
case OccupationTrigger.BreedingPhase:
    // 動物育種家: 繁殖フェーズで全動物の繁殖効率UP
    if (player.GetResource(ResourceType.Sheep) >= 2 && player.CanHouseAnimals(ResourceType.Sheep, 1))
    {
        player.AddResource(ResourceType.Sheep, 1);
        Debug.Log($"動物育種家効果発動: 羊の追加繁殖");
    }
    break;
```

### 6. HarvestEnd（収穫終了時）
**タイミング**: 収穫処理の完了時
**用途**: 収穫後の整理、次ラウンドの準備
**実装例**:
```csharp
case OccupationTrigger.HarvestEnd:
    // 顧問: 収穫終了時に次ラウンドの計画で勝利点
    player.AddVictoryPoints(1);
    Debug.Log($"顧問効果発動: 収穫終了時に勝利点1点獲得");
    break;
```

## 実装された変更点

### 1. OccupationTrigger enumの拡張
**ファイル**: `OccupationCard.cs`
```csharp
public enum OccupationTrigger
{
    // 既存のトリガー
    Immediate, OnAction, OnHarvest, OnBreeding, OnTurnEnd, OnRoundStart, OnTake, OnReceive, Passive,
    
    // 新しい収穫フェーズ用トリガー
    BeforeHarvest,  // 収穫の直前
    HarvestStart,   // 収穫の開始時
    FieldPhase,     // 畑フェーズ（作物収穫）
    FeedingPhase,   // 食料供給フェーズ
    BreedingPhase,  // 繁殖フェーズ
    HarvestEnd      // 収穫終了時
}
```

### 2. GameManager.ExecuteHarvest()の改修
**ファイル**: `GameManager.cs`

新しい収穫フロー:
```csharp
private void ExecuteHarvest()
{
    foreach (Player player in players)
    {
        // 1. 収穫の直前トリガー
        ExecuteAllTriggerableCards(OccupationTrigger.BeforeHarvest, player);
        
        // 2. 収穫の開始時トリガー
        ExecuteAllTriggerableCards(OccupationTrigger.HarvestStart, player);
        
        // 3. 畑フェーズ（作物収穫）
        ExecuteAllTriggerableCards(OccupationTrigger.FieldPhase, player);
        player.HarvestCrops();
        
        // 4. 食料供給フェーズ
        ExecuteAllTriggerableCards(OccupationTrigger.FeedingPhase, player);
        player.FeedFamily();
        
        // 5. 繁殖フェーズ
        ExecuteAllTriggerableCards(OccupationTrigger.BreedingPhase, player);
        player.BreedAnimals();
        
        // 6. 収穫終了時トリガー
        ExecuteAllTriggerableCards(OccupationTrigger.HarvestEnd, player);
    }
}
```

### 3. OccupationCard.csの拡張
新しいトリガーハンドラーメソッドを追加:
- `OnBeforeHarvestTrigger()`
- `OnHarvestStartTrigger()`
- `OnFieldPhaseTrigger()`
- `OnFeedingPhaseTrigger()`
- `OnBreedingPhaseTrigger()`
- `OnHarvestEndTrigger()`

### 4. EnhancedOccupationCard.csの拡張
新しいトリガー条件チェックメソッドを追加:
- `CheckBeforeHarvestTrigger()`
- `CheckHarvestStartTrigger()`
- `CheckFieldPhaseTrigger()`
- `CheckFeedingPhaseTrigger()`
- `CheckBreedingPhaseTrigger()`
- `CheckHarvestEndTrigger()`

## 職業カードの効果例

### 農夫（Farmer）
- **BeforeHarvest**: 収穫前の畑準備で穀物+1
- **FieldPhase**: 畑フェーズで追加穀物+1

### 学者（Scholar）
- **HarvestStart**: 収穫開始時に効率化ボーナス

### 職人（Craftsman）
- **FieldPhase**: 野菜から食料への品質向上変換

### パン職人（Baker）
- **FeedingPhase**: 穀物→食料変換効率UP（2→3）

### 漁師（Fisherman）
- **FeedingPhase**: 食料供給フェーズで追加食料+1

### 大商人（Merchant）
- **FeedingPhase**: 食料不足を他リソースで補填

### 羊飼い（Shepherd）
- **BreedingPhase**: 繁殖フェーズで追加羊+1

### 動物育種家（Breeder）
- **BreedingPhase**: 全動物の繁殖効率UP

### 顧問（Advisor）
- **HarvestEnd**: 収穫終了時に勝利点+1

### 石工（Stonecutter）
- **HarvestEnd**: 収穫終了時に石材整理で石+1

### 織工（Weaver）
- **HarvestEnd**: 羊から追加食料（羊数÷2）

## 条件付きトリガーの実装

### 畑フェーズの条件例
```csharp
// 畑を持っている場合のみ発動
if (effect.triggerCondition.Contains("has_fields"))
{
    return player.GetFields() > 0;
}

// 作物を持っている場合のみ発動
if (effect.triggerCondition.Contains("has_crops"))
{
    return player.GetTotalCropsInFields(ResourceType.Grain) > 0 ||
           player.GetTotalCropsInFields(ResourceType.Vegetable) > 0;
}
```

### 食料供給フェーズの条件例
```csharp
// 食料不足の場合のみ発動
if (effect.triggerCondition.Contains("food_shortage"))
{
    return player.GetFoodNeeded() > player.GetResource(ResourceType.Food);
}

// 穀物を持っている場合のみ発動
if (effect.triggerCondition.Contains("has_grain"))
{
    return player.GetResource(ResourceType.Grain) > 0;
}
```

### 繁殖フェーズの条件例
```csharp
// 羊を2匹以上持っている場合のみ発動
if (effect.triggerCondition.Contains("has_sheep"))
{
    return player.GetResource(ResourceType.Sheep) >= 2;
}
```

## 後方互換性

既存の`OnHarvest`と`OnBreeding`トリガーは互換性のため継続サポートします:
- `OnHarvest`: `FieldPhase`の直後に発動
- `OnBreeding`: `BreedingPhase`の直後に発動

## テストシステム

### DetailedHarvestTriggerTest.cs
新しいトリガーシステムの包括的なテストを実装:
- 各フェーズのトリガーテスト
- 統合収穫フローテスト
- 条件付きトリガーのテスト
- 職業カードの効果テスト

### テスト実行方法
```csharp
// エディタでの手動テスト
[ContextMenu("細かな収穫トリガーテスト実行")]
public void RunTestInEditor()

// 使用例の表示
[ContextMenu("新しいトリガーシステムの使用例")]
public void ShowUsageExamples()
```

## デバッグ出力の改善

詳細なログ出力で収穫フローを可視化:
```
🌾=== 収穫フェーズを実行中... ===
--- プレイヤー1の収穫フェーズ ---
📋 収穫の直前フェーズ
🚀 収穫の開始時フェーズ
🌱 畑フェーズ（作物収穫）
🍞 食料供給フェーズ
🐑 繁殖フェーズ
🏁 収穫終了時フェーズ
✅ プレイヤー1の収穫フェーズ完了
```

## 戦略的な意味

### 戦略の深化
1. **事前準備**: BeforeHarvestで収穫前の準備
2. **効率化**: HarvestStartで全体的な効率向上
3. **収穫最適化**: FieldPhaseで収穫量・品質向上
4. **食料管理**: FeedingPhaseで食料不足対策
5. **繁殖戦略**: BreedingPhaseで動物戦略
6. **次ラウンド準備**: HarvestEndで長期戦略

### カード設計の自由度
- 特定フェーズに特化した職業カード
- 複数フェーズにまたがる効果
- 条件付き発動による戦略性
- プレイヤーの選択による効果の最適化

## 今後の拡張可能性

### 新しい職業カードの可能性
- **収穫監督**: 全フェーズで小さなボーナス
- **季節の賢者**: 収穫時期に応じて効果変動
- **倉庫管理人**: 収穫後の貯蔵効率向上
- **交易商**: 収穫物の価値変換

### システムの拡張
- フェーズ間の相互作用
- 天候システムとの連携
- 季節システムとの統合
- 他プレイヤーとの相互作用

## まとめ

この実装により、収穫処理は：
- **6つの細かなフェーズ**に分割
- **戦略的深度**の大幅な向上
- **カード設計の自由度**拡大
- **後方互換性**の維持
- **詳細なテストシステム**の提供

従来の単純な収穫システムから、Agricola級の複雑で戦略的な収穫システムへと発展させることができました。