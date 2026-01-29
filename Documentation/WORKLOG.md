# GENESIS-BESTIARY 作業ログ

> このファイルはリアルタイムで更新する。
> タスク着手時に即座に追記し、完了時にチェックを入れる。
> セッション終了時、完了タスクはPROGRESS.mdに移動。

---

## 使い方

### タスク着手時
```markdown
- [ ] HH:MM タスク名
```

### タスク完了時
```markdown
- [x] HH:MM タスク名
```

### ファイル変更時
```bash
git add .
git commit -m "[Phase X] タスク名: 変更内容"
```

---

## 2025-01-27

### Phase 0: 設計固定

- [x] 23:45 プロジェクト計画開始
- [x] 00:30 コア体験定義
- [x] 01:00 プラットフォーム戦略決定
- [x] 01:30 ネットワーク方式決定
- [x] 02:00 収益モデル調査（MHF失敗要因）
- [x] 02:30 収益モデル決定
- [x] 03:00 仕様書作成開始
- [x] 03:30 全仕様書作成完了

---

## 2025-01-27 (続き)

### Phase 1: 縦切りコア（旧実装）

- [x] 10:30 Unityプロジェクト設定確認（3D設定済み確認）
- [x] 10:35 フォルダ構造作成（既存確認、完了済み）
- [x] 10:40 プレイヤー移動実装（既存確認、完了済み）
- [x] 10:45 プレイヤー攻撃実装（大剣コンボ）
- [x] 11:00 プレイヤーステートマシン更新
- [x] 11:10 プレイヤー回避実装
- [x] 11:15 PlayerData更新
- [x] 11:20 PlayerController更新
- [x] 11:30 モンスター実装
- [x] UI実装（HUD）
- [x] クエストシステム
- [x] 09:15 剥ぎ取り機能実装
- [x] 09:30 結果画面UI実装

---

## 2025-01-28

### アセット導入

- [x] Starter Assets - Third Person 導入
- [x] Cinemachine 導入
- [x] Input System 導入
- [x] DOTween 導入
- [x] TextMeshPro 導入
- [x] UAL2_Standard（アニメーション）追加

### Starter Assets統合

- [x] HunterController.cs 作成（MH固有処理）
- [x] HunterStateMachine.cs 作成（ステートマシン）
- [x] StarterAssets.inputactions 更新（Attack/Dodge/Carve追加）
- [ ] テストシーン構築
- [ ] 🎮 **テストプレイ①～⑤ 一括確認**

### キーバインド

| アクション | キーボード | ゲームパッド |
|-----------|-----------|-------------|
| Move | WASD | 左スティック |
| Look | マウス | 右スティック |
| Attack | マウス左 | X |
| Dodge | Space | B |
| Carve | E | Y |
| Sprint | Shift | LT |

---

## テストプレイチェックリスト

### テストプレイ①: 移動確認
- [ ] WASDで8方向移動できる
- [ ] カメラがプレイヤーを追従する
- [ ] 移動速度が適切

### テストプレイ②: 操作感確認
- [ ] 攻撃ボタンで3段コンボが出る
- [ ] 各攻撃の重みを感じる（硬直）
- [ ] 回避でローリングする
- [ ] スタミナが減る/回復する

### テストプレイ③: モンスター挙動確認
- [ ] モンスターが徘徊している
- [ ] プレイヤーに近づくと追跡してくる
- [ ] 追跡の速度が適切

### テストプレイ④: 戦闘確認 ⭐重要
- [ ] モンスターが攻撃してくる
- [ ] プレイヤーの攻撃でダメージが入る
- [ ] ダメージ数字が表示される
- [ ] 怯みが発生する
- [ ] プレイヤーが被弾でのけぞる
- [ ] 回避で攻撃を避けられる
- [ ] モンスターを倒せる
- [ ] **「狩りの手応え」を感じるか？**

### テストプレイ⑤: 一連の流れ確認
- [ ] クエスト開始できる
- [ ] モンスターを討伐できる
- [ ] 剥ぎ取りができる（3回）
- [ ] クエスト完了が表示される
- [ ] 結果画面で素材が見える
- [ ] **最初から最後まで通してプレイできる**

---

## 次回作業予定

1. テストシーン構築
2. プレイヤーPrefab作成（Starter Assets + HunterController）
3. モンスターPrefab配置
4. テストプレイ実施

---

## 中断時メモ

```
最後に作業していたファイル: HunterStateMachine.cs, StarterAssets.inputactions
最後に作業していた内容: Starter Assets統合スクリプト作成
次にやるべきこと: テストシーン構築、Prefab作成
```

---

## 2026-01-28

### Phase 1: 縦切りコア（テスト/統合）

- [x] 03:21 テストシーン構築
- [x] 11:06 HUD/Quest連携（Hunter対応・モンスターHP/ダメージ表示）
- [x] 11:08 剥ぎ取り報酬を結果画面へ連携
- [x] 11:10 クエスト自動開始/結果UI初期化

### テストプレイ① 結果 (2026-01-28 03:53)
- 合格項目: 移動（WASD/8方向）, 移動速度, カメラ追従
- 不合格項目: 
- 気になった点: カメラの高さが低い
- 調整が必要: カメラ高さ

### コード修正 (2026-01-28)
- [x] PlayerCombat.cs: デフォルト攻撃データ追加（WeaponDataなしでも動作）
- [x] PlayerCombat.cs: ヒット判定をレイヤー非依存に変更
- [x] MonsterController.cs: デフォルト値追加（MonsterDataなしでも動作）
- [x] MonsterController.cs: HunterController対応
- [x] MonsterStateMachine.cs: プロパティ参照に変更
- [x] ScriptableObjects作成（PlayerData, WeaponData, MonsterData等）

### テストプレイ②問題修正 (2026-01-28)
- [x] HunterController.cs: SendMessagesモード対応（OnAttack/OnDodge/OnCarve/OnJump）
- [x] HunterController.cs: Jumpを回避専用に変更（Starter Assetsのジャンプ無効化）
- [x] Cinemachine: カメラ高さ調整（ShoulderOffset.y=0.5, VerticalArmLength=0.4, CameraDistance=5.0）
- [x] MonsterController.cs: デフォルト攻撃追加（Bite: 15ダメージ）
- [x] MonsterStateMachine.cs: デフォルト攻撃サポート

**次: テストプレイ②～⑤再実施**

### 作業 (2026-01-28 続き)
- [x] 23:22 Scene_1 整理・Prefab化・Quest/UIホスト配置

- [x] 05:03 カメラ制御スクリプト削除（CinemachineTargetSetup）
- [x] 05:10 PlayerFollowCamera の Follow/LookAt を PlayerCameraRoot に再設定
- [x] 05:13 PlayerFollowCamera 初期位置/向き調整（背後上から三人称）
- [x] 05:14 PlayerFollowCamera 初期ピッチを5度に調整
- [x] 05:20 CinemachinePOV 縦視野制限を調整（-25〜25）
- [x] 05:22 POV縦可動域を再調整（-40〜25）＆Follow/LookAt再設定
- [x] 05:24 POV縦可動域を元に戻し（-70〜70）
- [x] 05:25 POV縦可動域を前状態に戻し（-40〜25）
- [x] 05:56 FreeLook Orbits 調整（Top 2.6/2.2, Mid 1.4/3.2, Bottom 0.1/4.2）
- [x] 06:00 FreeLook X軸反転を解除
- [x] 08:58 FreeLook X軸入力をMouse Xに設定し反転解除
- [x] 08:59 FreeLook Y軸入力をMouse Yに設定し反転解除
- [x] 09:05 InputManagerのMouse X/Yを反転（FreeLook反転補正）
- [x] 09:10 InputManagerのMouse X/Y反転を元に戻し
- [x] 09:24 UI整理：QuestResultUIを非表示化
- [x] 09:29 UI重なり対策：InputDebugger無効化 & HunterControllerデバッグUI非表示
