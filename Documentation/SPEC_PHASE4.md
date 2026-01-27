# Phase 4: ネットワーク仕様書

> 目標: 最大4人のオンライン協力プレイ実現

## 期間目安: 1週間

## 前提: Phase 3 完了

---

## 1. 実装範囲

| 要素 | 内容 |
|------|------|
| 基盤 | Mirror導入、Steam Relay接続 |
| 同期 | プレイヤー、モンスター、アイテム |
| UI | ロビー、マッチメイキング |
| 機能 | フレンド招待、クイックマッチ |

---

## 2. ネットワーク構成

### 2.1 方式

```
Host-Client方式:
- 1人がHostとして部屋を立てる
- 他プレイヤーはClientとして接続
- Hostが切断 = セッション終了
```

### 2.2 Transport

```
v1.0: Steam Relay（Steamworks.NET経由）
- 無料
- NAT越え自動
- Steamフレンドとの連携

将来: Photon移行
- クロスプレイ対応
- 専用サーバー
```

---

## 3. Mirror導入

### 3.1 パッケージ

```
- Mirror（メイン）
- Steamworks.NET（Steam連携）
- FizzySteamworks（Transport）
```

### 3.2 NetworkManager設定

```csharp
public class GameNetworkManager : NetworkManager
{
    // 最大プレイヤー数
    public override int maxConnections => 4;
    
    // プレイヤープレファブ
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // プレイヤースポーン処理
    }
}
```

---

## 4. 同期対象

### 4.1 プレイヤー

```csharp
public class NetworkPlayer : NetworkBehaviour
{
    // 位置・回転（自動同期）
    [SyncVar] public PlayerState currentState;
    [SyncVar] public int currentHP;
    [SyncVar] public int currentStamina;
    [SyncVar] public WeaponState weaponState;
    
    // アクション（Command経由）
    [Command]
    public void CmdAttack(int comboStep) { }
    
    [Command]
    public void CmdUseItem(int itemId) { }
    
    [Command]
    public void CmdDodge(Vector3 direction) { }
}
```

### 4.2 モンスター

```csharp
public class NetworkMonster : NetworkBehaviour
{
    // Host権限
    [SyncVar] public MonsterState currentState;
    [SyncVar] public int currentHP;
    [SyncVar] public Vector3 targetPosition;
    [SyncVar] public int targetPlayerId;
    
    // AIはHostのみ実行
    void Update()
    {
        if (!isServer) return;
        UpdateAI();
    }
    
    // ダメージはHost計算
    [Command]
    public void CmdTakeDamage(int damage, int part)
    {
        // Host側でダメージ処理
        // 結果をClientに同期
    }
}
```

### 4.3 アイテム・オブジェクト

```csharp
// 罠
public class NetworkTrap : NetworkBehaviour
{
    [SyncVar] public bool isTriggered;
    [SyncVar] public float remainingTime;
}

// 落とし物
public class NetworkDrop : NetworkBehaviour
{
    [SyncVar] public int itemId;
    [SyncVar] public bool isPickedUp;
}
```

---

## 5. 同期ルール

### 5.1 権限

| 対象 | 権限 | 理由 |
|------|------|------|
| プレイヤー移動 | 各Client | レスポンス重視 |
| プレイヤー攻撃 | 各Client → Host検証 | チート対策 |
| モンスターAI | Host | 一貫性 |
| ダメージ計算 | Host | 一貫性 |
| アイテム消費 | Host | 同期保証 |
| クエスト進行 | Host | 一貫性 |

### 5.2 補間

```
位置補間:
- NetworkTransform使用
- 補間時間: 0.1秒
- スナップ距離: 3.0m（瞬間移動）

状態補間:
- 即時反映（補間なし）
```

---

## 6. ロビーシステム

### 6.1 ロビー画面

```
表示内容:
- 部屋名（Host名 or カスタム）
- 現在人数 / 最大人数
- クエスト名
- 準備状態

機能:
- 準備完了ボタン
- チャット（定型文）
- 装備確認
- 退出ボタン
```

### 6.2 部屋作成

```
Host側:
1. 「部屋を作る」選択
2. クエスト選択
3. 公開設定（公開 / フレンドのみ / 非公開）
4. 部屋作成
5. 他プレイヤー待機
6. 全員準備完了で出発
```

### 6.3 部屋参加

```
Client側:
1. 「部屋を探す」選択
2. 部屋一覧表示
3. 部屋選択して参加
4. 準備完了
5. Host出発待ち
```

---

## 7. マッチメイキング

### 7.1 フレンド招待

```
Steam連携:
- Steamフレンドリスト表示
- 招待送信
- 招待受信で直接参加
```

### 7.2 クイックマッチ

```
処理:
1. 「クイックマッチ」選択
2. 条件に合う部屋を検索
3. 見つかれば参加
4. なければ自動で部屋作成
```

### 7.3 条件フィルター

```
フィルター項目:
- クエストランク
- 目標モンスター
- 空き枠あり
```

---

## 8. 切断対応

### 8.1 Client切断

```
処理:
- 該当プレイヤーを削除
- 残りメンバーで継続
- モンスターHP調整なし（そのまま）
```

### 8.2 Host切断

```
処理:
- セッション終了
- 全員拠点に戻る
- クエスト失敗扱い（報酬なし）

※ Host移譲は v1.0 では未実装
```

### 8.3 再接続

```
v1.0: 未対応（切断 = 終了）
将来: セッションID保持して再接続
```

---

## 9. チート対策

### 9.1 基本方針

```
- 重要な計算はHost側
- Client入力は検証
- 異常値は無視
```

### 9.2 検証項目

```
移動速度:
- 上限チェック
- 異常なテレポート検知

ダメージ:
- 最大ダメージ上限
- 攻撃頻度チェック

アイテム:
- 所持数チェック
- 使用可能状態チェック
```

---

## 10. 通信量最適化

### 10.1 送信頻度

```
位置情報: 20回/秒
状態変化: 変化時のみ
HP/スタミナ: 変化時のみ
```

### 10.2 圧縮

```
- 位置: 小数点2桁まで
- 回転: Quaternion圧縮
- 不要データ除外
```

---

## 11. UI追加

### 11.1 マルチプレイ用HUD

```
追加表示:
- 他プレイヤーHP（画面端）
- 他プレイヤー位置（ミニマップ）
- 接続状態アイコン
```

### 11.2 チャット

```
定型文方式:
- 「よろしく！」
- 「ありがとう！」
- 「手伝って！」
- 「罠設置する」
- 等

カスタム入力: なし（v1.0）
```

---

## 12. 完了条件

- [ ] Mirror導入完了
- [ ] Steam Relay接続できる
- [ ] 複数人で同じマップに入れる
- [ ] プレイヤー位置が同期される
- [ ] プレイヤー攻撃が同期される
- [ ] モンスターが同期される
- [ ] ダメージが正しく同期される
- [ ] ロビー画面が機能する
- [ ] 部屋作成できる
- [ ] 部屋参加できる
- [ ] フレンド招待できる
- [ ] クエスト完了が同期される
- [ ] 切断時に正しく処理される

---

## 13. 実装順序

```
Day 1:
- Mirror導入
- 基本NetworkManager設定

Day 2:
- プレイヤー同期
- 位置・状態同期

Day 3:
- モンスター同期
- Host権限実装

Day 4:
- ダメージ同期
- アイテム同期

Day 5:
- ロビーUI
- 部屋作成/参加

Day 6:
- Steam Relay設定
- フレンド招待

Day 7:
- 統合テスト
- バグ修正
```

---

## 14. テスト項目

```
基本:
- [ ] 2人で接続できる
- [ ] 4人で接続できる
- [ ] 同じモンスターを攻撃できる
- [ ] お互いの攻撃が見える

異常系:
- [ ] Client切断後も継続できる
- [ ] Host切断で正しく終了する
- [ ] 高レイテンシでも動作する（200ms）
```
