# Unity UI セットアップガイド

## 🚀 クイックスタート（推奨）

### 最も簡単な方法：
1. **新しい空のシーンを作成**: File > New Scene > Empty
2. **QuickStartSetupスクリプトを作成**: 
   - Hierarchy > Create Empty GameObject
   - 名前を「QuickStartSetup」に変更
   - Inspector > Add Component > QuickStartSetup
3. **自動セットアップ実行**: 
   - Playボタンを押すだけで自動的にUIが作成されます！
   - または、Inspector で「Quick Start Setup」を右クリック > 「Quick Start Setup」を選択

これで基本的なUIが完全に動作します！

---

## 📖 手動セットアップ（詳細版）

手動でUIを作成したい場合は、以下の手順に従ってください：

## 1. 基本シーンの作成

### 1.1 新しいシーンの作成
- File > New Scene > Empty
- シーンを保存: File > Save Scene As... > "MainGame"

### 1.2 GameManagerオブジェクトの作成
- Hierarchy > Create Empty GameObject
- 名前を「GameManager」に変更
- Inspector > Add Component > GameManager

## 2. UIキャンバスの設定

### 2.1 基本キャンバスの作成
- Hierarchy > UI > Canvas
- Canvas名を「GameCanvas」に変更
- Canvas Scaler > UI Scale Mode を「Scale With Screen Size」に設定
- Reference Resolution を 1920x1080 に設定

### 2.2 GameUIオブジェクトの作成
- GameCanvas > Create Empty GameObject
- 名前を「GameUI」に変更
- Inspector > Add Component > GameUI

## 3. 必要なUIプレハブの作成

### 3.1 PlayerInfoプレハブ
- GameCanvas > UI > Panel
- 名前を「PlayerInfoPanel」に変更
- 以下の子要素を追加：
  - UI > Text (TextMeshPro) > 名前を「PlayerNameText」
  - UI > Text (TextMeshPro) > 名前を「VictoryPointsText」
  - UI > Text (TextMeshPro) > 名前を「WorkersText」
- PlayerInfoPanel > Inspector > Add Component > PlayerInfoUI
- プレハブ化: Assets > Create > Prefab > 名前を「PlayerInfoPrefab」

### 3.2 ResourceDisplayプレハブ
- GameCanvas > UI > Panel
- 名前を「ResourceDisplayPanel」に変更
- 以下の子要素を追加：
  - UI > Text (TextMeshPro) > 名前を「ResourceNameText」
  - UI > Text (TextMeshPro) > 名前を「ResourceAmountText」
  - UI > Image > 名前を「ResourceIcon」
- ResourceDisplayPanel > Inspector > Add Component > ResourceDisplayUI
- プレハブ化: Assets > Create > Prefab > 名前を「ResourceDisplayPrefab」

### 3.3 CardDisplayプレハブ
- GameCanvas > UI > Panel
- 名前を「CardDisplayPanel」に変更
- 以下の子要素を追加：
  - UI > Text (TextMeshPro) > 名前を「CardNameText」
  - UI > Text (TextMeshPro) > 名前を「CardDescriptionText」
  - UI > Image > 名前を「CardArtImage」
  - UI > Button > 名前を「CardButton」
- CardDisplayPanel > Inspector > Add Component > CardDisplayUI
- プレハブ化: Assets > Create > Prefab > 名前を「CardDisplayPrefab」

## 4. メインUIレイアウトの作成

### 4.1 ゲーム情報表示エリア
- GameCanvas > UI > Panel > 名前を「GameInfoPanel」
- 以下の子要素を追加：
  - UI > Text (TextMeshPro) > 名前を「CurrentPlayerText」
  - UI > Text (TextMeshPro) > 名前を「CurrentRoundText」
  - UI > Text (TextMeshPro) > 名前を「GameStateText」
  - UI > Text (TextMeshPro) > 名前を「TurnPhaseText」

### 4.2 プレイヤー情報表示エリア
- GameCanvas > UI > Panel > 名前を「PlayerInfoParent」
- Horizontal Layout Group コンポーネントを追加

### 4.3 リソース表示エリア
- GameCanvas > UI > Panel > 名前を「ResourceDisplayParent」
- Horizontal Layout Group コンポーネントを追加

### 4.4 手札表示エリア
- GameCanvas > UI > Panel > 名前を「HandDisplayParent」
- Horizontal Layout Group コンポーネントを追加

### 4.5 ボタンエリア
- GameCanvas > UI > Button > 名前を「EndTurnButton」
- GameCanvas > UI > Button > 名前を「RestartGameButton」

### 4.6 ゲーム終了パネル
- GameCanvas > UI > Panel > 名前を「GameOverPanel」
- 以下の子要素を追加：
  - UI > Text (TextMeshPro) > 名前を「WinnerText」
- 初期状態で非表示にチェック

## 5. GameUIスクリプトの設定

### 5.1 GameUIコンポーネントの参照設定
GameUIオブジェクトを選択し、Inspector で以下を設定：

**ゲーム情報:**
- Current Player Text: CurrentPlayerText
- Current Round Text: CurrentRoundText
- Game State Text: GameStateText
- Turn Phase Text: TurnPhaseText

**プレイヤー情報:**
- Player Info Parent: PlayerInfoParent
- Player Info Prefab: PlayerInfoPrefab

**リソース表示:**
- Resource Display Parent: ResourceDisplayParent
- Resource Display Prefab: ResourceDisplayPrefab

**ボタン:**
- End Turn Button: EndTurnButton
- Restart Game Button: RestartGameButton

**カード表示:**
- Hand Display Parent: HandDisplayParent
- Card Display Prefab: CardDisplayPrefab

**ゲーム終了:**
- Game Over Panel: GameOverPanel
- Winner Text: WinnerText

## 6. 動作確認

### 6.1 プレイボタンで確認
- Unity > Play ボタンをクリック
- Console でエラーが出ていないか確認

### 6.2 基本的な動作テスト
- シーンがロードされること
- UIが正しく表示されること
- ボタンが機能すること

## 7. 追加の最適化

### 7.1 レイアウト調整
- Content Size Fitter を使用してテキストサイズを自動調整
- Anchor を適切に設定してレスポンシブ対応

### 7.2 視覚的改善
- 背景色やフォントサイズを調整
- アイコンや画像を追加
- アニメーション効果を追加

## 8. トラブルシューティング

### よくある問題:
1. **TextMeshProが見つからない** → Window > TextMeshPro > Import TMP Essential Resources
2. **スクリプトが見つからない** → Assets > Scripts の構造を確認
3. **プレハブが機能しない** → プレハブの参照が正しく設定されているか確認

### デバッグ方法:
- Console ウィンドウでエラーメッセージを確認
- Inspector で各コンポーネントの参照が正しく設定されているか確認
- Play モードで実際の動作を確認

## 9. 次のステップ

このセットアップが完了したら、以下の機能を追加できます：
- アクションスペースの視覚的表示
- ワーカー配置のインタラクション
- カードの詳細表示
- ゲーム進行の視覚的フィードバック
- 音響効果
- アニメーション効果

---

このガイドに従って設定すれば、基本的なUIが動作するはずです。問題が発生した場合は、Console ウィンドウのエラーメッセージを確認してください。