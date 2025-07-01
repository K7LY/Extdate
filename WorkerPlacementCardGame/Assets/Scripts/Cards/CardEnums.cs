using UnityEngine;

[System.Serializable]
public enum CardEffectType
{
    ResourceModification,   // リソース修正
    VictoryPointModification, // 勝利点修正
    ActionBonus,           // アクションボーナス
    SpecialAbility,        // 特殊能力
    PassiveEffect,         // 継続効果
    ConditionalEffect      // 条件付き効果
}

[System.Serializable] 
public enum OccupationType
{
    Farmer,              // 農夫
    Carpenter,           // 大工
    Baker,               // パン職人
    Fisherman,           // 漁師
    Shepherd,            // 羊飼い
    StoneMason,          // 石工
    Weaver,              // 織工
    Merchant,            // 商人
    ForestWarden,        // 森林管理人
    AnimalBreeder,       // 動物育種家
    Miller,              // 粉挽き
    Blacksmith,          // 鍛冶屋
    Potter,              // 陶芸家
    Trader,              // 交易商
    Hunter               // 狩人
}

[System.Serializable]
public enum ImprovementType
{
    CookingFacility,     // 料理設備
    StorageFacility,     // 貯蔵設備
    ProductionFacility,  // 生産設備
    ScoringImprovement,  // 得点進歩
    SpecialImprovement   // 特殊進歩
}

[System.Serializable]
public enum ImprovementCategory
{
    Minor,               // 小さな進歩
    Major                // 大きな進歩
}

[System.Serializable]
public enum ActionTriggerType
{
    None,                // トリガーなし
    BeforeAction,        // アクション前
    AfterAction,         // アクション後
    DuringAction,        // アクション中
    OnSpecificAction     // 特定アクション時
}

[System.Serializable]
public enum GamePhase
{
    Setup,               // セットアップ
    WorkPhase,           // 労働フェーズ
    ReturningHome,       // 帰宅フェーズ
    Harvest,             // 収穫フェーズ
    Feeding,             // 食料供給フェーズ
    Breeding,            // 繁殖フェーズ
    EndGame              // ゲーム終了
}

[System.Serializable]
public enum TriggerCondition
{
    Always,              // 常に
    OnlyOnce,            // 一度だけ
    PerRound,            // ラウンド毎
    PerTurn,             // ターン毎
    ConditionalCheck     // 条件チェック
}

// 拡張効果のターゲットタイプ
[System.Serializable]
public enum EffectTarget
{
    Self,                // 自分
    AllPlayers,          // 全プレイヤー
    OtherPlayers,        // 他プレイヤー
    NextPlayer,          // 次のプレイヤー
    SpecificPlayer       // 特定プレイヤー
}

// カードの希少度
[System.Serializable]
public enum CardRarity
{
    Common,              // 一般
    Uncommon,            // 珍しい
    Rare,                // 稀
    Unique               // 唯一
}

// 効果の継続期間
[System.Serializable]
public enum EffectDuration
{
    Instant,             // 即座
    ThisTurn,            // このターン
    ThisRound,           // このラウンド
    UntilHarvest,        // 次の収穫まで
    Permanent,           // 永続
    EndGame              // ゲーム終了まで
}