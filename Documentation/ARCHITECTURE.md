# GENESIS-BESTIARY 設計原則

> このファイルは設計の指針。CONSTITUTIONほど厳格ではないが、
> 変更する場合は十分な理由と記録が必要。

---

## 1. フォルダ構造

```
Assets/
├── Documentation/           # 仕様書・設計書
├── Scripts/
│   ├── Player/              # プレイヤー関連
│   │   ├── PlayerController.cs
│   │   ├── PlayerStateMachine.cs
│   │   ├── PlayerCombat.cs
│   │   └── PlayerInventory.cs
│   ├── Monster/             # モンスター関連
│   │   ├── MonsterBase.cs
│   │   ├── MonsterAI.cs
│   │   ├── MonsterStateMachine.cs
│   │   └── Monsters/        # 個別モンスター
│   ├── Combat/              # 戦闘システム
│   │   ├── DamageCalculator.cs
│   │   ├── HitboxManager.cs
│   │   └── Sharpness.cs
│   ├── Quest/               # クエストシステム
│   │   ├── QuestManager.cs
│   │   ├── QuestObjective.cs
│   │   └── QuestReward.cs
│   ├── Equipment/           # 装備システム
│   │   ├── WeaponBase.cs
│   │   ├── ArmorBase.cs
│   │   └── EquipmentManager.cs
│   ├── UI/                  # UI関連
│   │   ├── HUDManager.cs
│   │   ├── MenuManager.cs
│   │   └── InventoryUI.cs
│   ├── Network/             # ネットワーク（Phase 4）
│   │   ├── NetworkManager.cs
│   │   └── PlayerSync.cs
│   ├── Data/                # データ管理
│   │   ├── GameDatabase.cs
│   │   └── SaveManager.cs
│   └── Utilities/           # 汎用ユーティリティ
│       ├── StateMachine.cs
│       └── ObjectPool.cs
├── ScriptableObjects/
│   ├── Monsters/            # モンスターデータ
│   ├── Weapons/             # 武器データ
│   ├── Armors/              # 防具データ
│   ├── Items/               # アイテムデータ
│   └── Quests/              # クエストデータ
├── Prefabs/
│   ├── Player/
│   ├── Monsters/
│   ├── Weapons/
│   ├── Effects/
│   └── UI/
├── Scenes/
│   ├── Title.unity
│   ├── Village.unity        # 拠点
│   ├── Quest/               # クエストマップ
│   └── Test/                # テスト用
├── Art/
│   ├── Models/
│   ├── Textures/
│   ├── Animations/
│   └── Shaders/
└── ThirdParty/              # 外部アセット
    ├── Mirror/              # Phase 4で追加
    └── PSXEffects/          # Phase 3で追加
```

---

## 2. プレイヤー設計

### ステートマシン

```
PlayerState:
├── Idle          # 待機
├── Move          # 移動
├── Attack        # 攻撃（コンボ対応）
├── Dodge         # 回避
├── UseItem       # アイテム使用（硬直あり）
├── Carve         # 剥ぎ取り
├── Sheathe       # 納刀中
├── Unsheathe     # 抜刀中
├── Stagger       # のけぞり
├── Down          # ダウン
└── Dead          # 死亡
```

### モジュラーメッシュ

```
Player/
├── Head      # 頭（髪型、顔）
├── Torso     # 胴
├── Arms      # 腕
├── Waist     # 腰
└── Legs      # 脚

防具装備 = メッシュ差し替え
重ね着 = 別レイヤーのメッシュ
```

---

## 3. モンスター設計

### ステートマシン

```
MonsterState:
├── Idle          # 待機
├── Roam          # 徘徊
├── Alert         # 警戒（プレイヤー発見）
├── Chase         # 追跡
├── Attack        # 攻撃（行動選択）
├── Enraged       # 怒り状態
├── Fatigued      # 疲労状態
├── Flinch        # 怯み
├── Down          # ダウン
├── Flee          # 逃走（エリア移動）
└── Dead          # 死亡
```

### データ駆動

```csharp
[CreateAssetMenu]
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public int baseHealth;
    public float moveSpeed;
    public float attackPower;
    
    // 肉質（部位ごとのダメージ倍率）
    public Dictionary<BodyPart, float> hitZones;
    
    // 行動パターン
    public List<MonsterAction> actions;
    
    // 状態変化閾値
    public float enrageThreshold;  // 怒り発動HP%
    public float fatigueThreshold; // 疲労発動スタミナ%
}
```

---

## 4. ダメージ計算

```
基本ダメージ = 武器攻撃力 × モーション値 × 切れ味補正
最終ダメージ = 基本ダメージ × 肉質 × 会心補正 × その他補正

切れ味補正:
- 赤: 0.5
- 橙: 0.75
- 黄: 1.0
- 緑: 1.125
- 青: 1.25
- 白: 1.32
```

---

## 5. ネットワーク設計（Phase 4）

### 同期方式

| 対象 | 同期方法 |
|------|----------|
| プレイヤー位置 | NetworkTransform（補間あり） |
| プレイヤー状態 | SyncVar |
| モンスターAI | Host権限、状態同期 |
| アイテム使用 | Command → Server → ClientRpc |
| ダメージ | Host計算 → 結果同期 |

### Host権限

- モンスターのAI判断はHostのみ
- ダメージ計算はHostのみ
- アイテム消費判定はHostのみ

---

## 6. ScriptableObject設計

### 武器データ

```csharp
[CreateAssetMenu]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType type;
    public int attack;
    public int affinity;        // 会心率
    public ElementType element;
    public int elementValue;
    public SharpnessData sharpness;
    public int[] slots;         // スロット数
    
    // 生産・強化素材
    public List<MaterialRequirement> craftMaterials;
    public WeaponData upgradeTo;
}
```

### 防具データ

```csharp
[CreateAssetMenu]
public class ArmorData : ScriptableObject
{
    public string armorName;
    public ArmorPart part;      // 頭/胴/腕/腰/脚
    public int defense;
    public ElementResist resist;
    public List<SkillPoint> skills;
    public int[] slots;
    
    // 見た目
    public Mesh armorMesh;
    public Material armorMaterial;
    
    // 生産素材
    public List<MaterialRequirement> craftMaterials;
}
```

---

## 7. UI設計原則

- **Canvas Scaler**: Scale With Screen Size（1920x1080基準）
- **アンカー**: 画面端基準で配置
- **フォント**: 日本語対応、読みやすさ優先
- **操作**: ゲームパッド前提、マウスも対応

---

## 8. 拡張ポイント

将来の拡張に備えて、以下は差し替え可能に設計:

| 対象 | 差し替え方法 |
|------|--------------|
| 入力 | Input System Action |
| ネットワーク | Transport差し替え |
| 保存 | ISaveSystem インターフェース |
| UI | Prefab差し替え |

---

## 9. パフォーマンス指針

- オブジェクトプール使用（エフェクト、弾丸等）
- LOD設定（モンスター、背景）
- オクルージョンカリング有効化
- テクスチャアトラス化

---

## 更新履歴

| 日付 | 内容 |
|------|------|
| 2025-01-27 | 初版作成 |
