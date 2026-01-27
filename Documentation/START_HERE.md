# GENESIS-BESTIARY 開発ガイド

## 🚀 作業開始時に必ず読むファイル（順番厳守）

1. **このファイル** - 作業フロー確認
2. **CONSTITUTION.md** - 絶対に変更不可のルール
3. **ARCHITECTURE.md** - 設計原則
4. **PROGRESS.md** - 完了済みタスク確認
5. **WORKLOG.md** - 中断箇所の確認

---

## 📋 セッション開始時のチェックリスト

```
□ CONSTITUTION.md を読んだ
□ ARCHITECTURE.md を読んだ
□ PROGRESS.md で現在のPhaseを確認した
□ WORKLOG.md で最後の作業箇所を確認した
□ 現在のPhaseのSPEC_PHASEx.md を読んだ
```

---

## 🔄 作業フロー

### 1. タスク着手時
```markdown
WORKLOG.md に追記:
- [ ] HH:MM タスク名
```

### 2. ファイル変更時
```bash
git add .
git commit -m "タスク名: 変更内容"
```

### 3. タスク完了時
```markdown
WORKLOG.md を更新:
- [x] HH:MM タスク名
```

### 4. セッション終了時
```markdown
PROGRESS.md に完了タスクをまとめて記録
```

---

## 📁 フォルダ構造

```
GENESIS-BESTIARY/
├── Documentation/          ← 今ここ
│   ├── START_HERE.md
│   ├── CONSTITUTION.md
│   ├── ARCHITECTURE.md
│   ├── PROGRESS.md
│   ├── WORKLOG.md
│   ├── SPEC_PHASE1.md
│   ├── SPEC_PHASE2.md
│   ├── SPEC_PHASE3.md
│   └── SPEC_PHASE4.md
├── Assets/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── ScriptableObjects/
│   └── ...
└── ...
```

---

## ⚠️ 重要な注意事項

1. **CONSTITUTIONは変更禁止** - 理由があっても変えない
2. **WORKLOGは即時更新** - 落ちても復帰できるように
3. **こまめにコミット** - 1タスク1コミット
4. **Phaseをスキップしない** - 順番に実装

---

## 🆘 困ったときは

- 設計判断に迷う → CONSTITUTION.md / ARCHITECTURE.md を参照
- 実装方法がわからない → 該当PhaseのSPEC を参照
- 前回の作業がわからない → WORKLOG.md を参照
