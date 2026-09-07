# 引き継ぎ: Unity CLI `eval` 解説動画用 素材アニメーション

最終更新: 2026-09-07 / 状態: **第1バージョン完成・未レンダー**

---

## 1. これは何か

`../bunny-blitz-exp` の Bunny Blitz プロジェクトを題材にした **Unity CLI の `eval` 機能の解説動画**（本編）に差し込むための、無音・ナレーションなしの短尺素材アニメーション。

伝えたいこと（`message`）: **エージェントの C# コードが、そのまま動いているゲームを書き換える**

内容: コーディングエージェントから C# コードが Bunny Blitz に向かって飛んでいき、到着すると主人公キャラの周りにコインが生成される。

ユーザーからの明示指定:

- 背景は**真っ黒** (`#000`)
- Bunny Blitz の画面は実キャプチャではなく、**見やすくした模式図**
- 1024×1024 正方形 / **ProRes MOV**（前作 `unity-logo-sting` と同じ書き出し設定）

---

## 2. どこにあるか

| パス | 中身 |
| --- | --- |
| `videos/eval-code-to-coins/` | プロジェクト本体（HyperFrames） |
| `videos/eval-code-to-coins/index.html` | **コンポジション本体**（単一ファイル、これが唯一の作業対象） |
| `videos/eval-code-to-coins/BRIEF.md` | 確定ブリーフ。`/hyperframes` は新セッションでまずこれを読む |
| `videos/eval-code-to-coins/shot-plan.json` | ショット IR（ビート表・幾何・検証結果・設計判断） |
| `videos/eval-code-to-coins/snapshots/` | 直近のプルーフ（`contact-sheet.jpg` が一覧） |
| `videos/eval-code-to-coins/renders/` | **まだ存在しない**（未レンダー・git 管理外） |
| `.gitignore` / `.gitattributes` | リポジトリ設定（下の §10 参照） |
| `../bunny-blitz-exp/` | 元の Unity プロジェクト。ビジュアル identity の参照元 |

前セッションの残骸 `videos/unity-logo-sting/` はユーザー許可のうえ削除済み。

CLI ピン: `hyperframes@0.8.30`（`package.json` の scripts）。レンダー前に一度
`npx hyperframes@latest upgrade --project . --check` を実行して差分を確認すること。

---

## 3. 現在の状態

- 尺 **7.0s** / **1024×1024** / 30fps 想定 / 単一シーン（カット無し）
- `npx hyperframes check .` **passed**
  - lint 0 error（残り1件は `composition_file_too_large` の warning のみ）
  - layout 0 error / motion 0 error / contrast **78/78 WCAG AA**
- プルーフスナップショットを目視確認済み（`snapshots/contact-sheet.jpg`）
- **レンダーは未実行**。ユーザーは「第1バージョンとしてこれで良い」と判断した段階

Studio プレビューは停止済み。再開は:

```bash
cd videos/eval-code-to-coins
npx hyperframes preview --background   # → http://localhost:3002/#project/eval-code-to-coins
npx hyperframes preview --status
npx hyperframes preview --stop
```

---

## 4. 次にやること

1. **ProRes MOV で書き出す**（ユーザー承認済みの最終形式。承認ゲートはもう通っている）

   ```bash
   cd videos/eval-code-to-coins
   npx hyperframes render . --skill=motion-graphics -q high --format mov -o ./renders/eval-code-to-coins.mov
   ```

   書き出し後、ファイルの存在・非ゼロ・尺 7.0s を確認する。

2. ユーザーからの修正指示があればそれを反映（下の「触るときの注意」を必読）

3. 本編動画に差し込んだうえでの尺・間の調整。特にラスト 5.9→7.0s の
   ホールドは編集用の余白なので、本編に合わせて伸縮させる余地がある。

---

## 5. 構成（レイアウト・幾何）

1024×1024 の中に 2 枚のパネルが浮いている。周囲は真っ黒。

```
                    ┌──────────────────────────────┐
  #agent            │ ●●●  coding agent  [unity eval]│  left 96, top 96, 832×292
  (ダークな          │                               │
   エディタ面)        │  var p = player.transform...  │  ← タイプ後 #code は display:none
                    │  SpawnCoins(p, radius: ...)   │     入れ替わりに #result
                    └──────────────────────────────┘
                              ↓ C# パケットが弧を描いて飛ぶ
                    ┌──────────────────────────────┐
  #game             │ [BUNNY BLITZ]         ◉ 8    │  left 96, top 452, 832×476
  (模式図)           │      ○   ○   ○               │  ← コインのリング
                    │   ○    (🐰)    ○             │
                    │      ○   ○   ○               │
                    │  ▬▬      ████████     ▬▬▬    │  ← 板 (#ledge-l/#ledge-r)
                    │  ██████████████████████████  │  ← 地面 (#ground)
                    └──────────────────────────────┘
```

JS 内の定数（すべて `index.html` の `<script>` 冒頭付近）:

| 定数 | 値 | 意味 |
| --- | --- | --- |
| `RING` | `{cx:416, cy:250, rx:160, ry:88, n:8, a0:-67.5}` | コインの最終リング（`#game-inner` ローカル座標）。`a0` を -67.5° にしているのは、真下にコインが来て主人公の足元と重ならないようにするため |
| `FLY` | `{at:2.76, dur:0.64, dx:-108, dy:436}` | パケットの飛行。`x` に `power1.inOut`、`y` に `power2.in` を別掛けして放物線を作っている（MotionPath は使っていない） |
| `HIT` | `3.4` | 着弾 |
| `POP` | `3.9` | コイン生成 |
| `TICK0`/`GAP` | `4.35` / `0.09` | HUD カウント開始とティック間隔 |
| `LAND` | `5.15` | 総数着地（HUD ポップ＋ゴールドスウィープ＋結果テキスト追記） |
| `CODE` | 2 行のトークン配列 | 表示する C#。ここを書き換えれば文字送りも自動追従する |

パケットの飛行経路は、`#bb-chip`（左上）と `#hud`（右上）のテキストを跨がないように
x 470→362 の帯に収めてある。**始点 `left` や `FLY.dx` を動かすとレイアウト監査の
occlusion に引っかかるので注意。**

---

## 6. ビート表（因果の連鎖が主役）

| 時刻 | 何が起きるか |
| --- | --- |
| 0.00–0.80 | 2 枚のパネルが staggered で登場（agent が先、game が 0.20 遅れ） |
| 0.80–2.28 | C# を 2 行タイプ。キャレットが文字送りの先端に追従 |
| **2.32** | **原因**: `unity eval` ピルがシアンに発火＋コードが発光 |
| 2.42–2.54 | コードが潰れて消え、入れ替わりに光る **C# パケット**が凝縮 |
| 2.76–3.40 | パケットが左下へ弧を描いて飛行（4 点のコメットトレイル付き） |
| **3.40** | **着弾**: パケット破裂／衝撃波リング＋フラッシュ／ウサギがスクワッシュ／パネルが 9px リコイル／枠がシアンに閃光／同フレームで `✓ evaluated` |
| 3.52–3.90 | **溜め 0.38s**（stillness-before-climax）。フラッシュが減衰するだけ |
| **3.90–4.40** | **クライマックス**: コイン 8 枚が主人公から湧き出しリングに着地（`back.out(1.4)`, stagger 0.05）＋ゴールドの光 |
| 4.35–5.07 | HUD が 0→8 をコイン 1 枚ずつカウント。カウントされたコインが呼応してポップ |
| **5.15** | 総数着地: `8` が 1.16 倍にポップ＋パネルをゴールドのスウィープ＋`· 8 coins spawned` を追記 |
| 5.90–7.00 | ホールド（編集用の余白） |

演出方針は motion-doctrine 準拠。単一シーンなので Seam Law／vector ledger は対象外だが、
**因果の連鎖・溜め・アイドルウォブル禁止**は守っている。`bounce.out` / `elastic.out` は未使用。

---

## 7. 触るときの注意（ここでハマった）

### 7.1 `immediateRender` はタイムライン全体で無効化してある

```js
const tl = gsap.timeline({ paused: true, defaults: { immediateRender: false } });
```

GSAP の `fromTo` は既定で from-vars を**生成時に即適用**する。この作品は
「行って戻る」2 段トゥイーン（リコイル、スクワッシュ、スケールポップ等）を多用するので、
既定のままだと**戻り側の開始値がフレーム 0 に焼き付く**（実際にゲーム面が 9px ずれ、
キャレットが冒頭から点灯し、HUD が 1.42 倍で始まっていた）。

**CSS が t=0 の真値**である。新しいトゥイーンを足すときも、この前提を崩さないこと。
CSS 側に初期状態を書き、トゥイーンはそこからの旅程として書く。

### 7.2 隠す要素は `opacity`/`visibility` ではなく `display` で消す

HyperFrames のレイアウト監査は **`opacity: 0` も `visibility: hidden` も無視して
矩形を測る**。そのため「見えていないのに occlusion / overlap エラー」が大量に出る。

この作品では次を `display: none` → トゥイーンで表示に切り替えている:

- タイプ前の 1 文字ずつの `<span class="ch">`（マスク方式だと隠れた文字が測定されて詰む）
- `#packet` と `.trail`（コード領域の真上に置かれているため）
- `#code`（2.54s で退場。以降はパケットと矩形が衝突する）
- `#result-more`

**1 要素につき `display` の `fromTo` は 1 本だけ**にすること。後片付け用の
`display: block → none` を足すと、`immediateRender` の件と合わせて開始値が
フレーム 0 に漏れる（実際に踏んだ）。

### 7.3 HUD のカウントは 9 個の桁要素を重ねてはいけない

重ねると `content_overlap` エラーになる。プロキシオブジェクトを
`ease: "steps(8)"` で回して `onUpdate` で `textContent` を書き換える、
builder-contract 記載の正攻法にしてある。

### 7.4 残っている info 3 件は意図どおり

- キャレットが隣（`display: none` の）文字セルに被っていると判定される（t=1.17 / 1.94）
- `#game-inner` が t=3.5 で 3.45px はみ出す → **着弾リコイルそのもの**。`#game` の
  `overflow: hidden` が切っている

いずれも潰す必要はない。潰そうとするとリコイルが死ぬ。

---

## 8. パレット（Bunny Blitz のロゴ／アートから採取）

`index.html` の `:root` に CSS 変数として一元化してある。インラインの hex は使わない。

| 用途 | 値 |
| --- | --- |
| ステージ | `#000000` |
| エージェント面 | `#0b0f16` / `#0e141d`、chrome `#8b9bad` |
| シアン（eval / コード / 衝撃） | `#35d3f5`、dim `#6aa9bd` |
| ゲーム面 | `#100826` / `#1a0d3a` |
| 紫（地面・枠） | `#4b1e9e`、edge `#7b4bd8` |
| 青（板） | `#1e9bf0`、edge `#6fc8ff` |
| 黄（HUD / チップ） | `#ffc824` |
| ゴールド（コイン） | `#e8a317` / `#ffd75e` / `#fff3c4`、rim `#a8710c` |
| ウサギ | 白 `#ffffff`、耳の内側 `#ffc2de`、前髪 `#ff2d96`、目 `#35d3f5`、鼻 `#ff7bb0` |

コントラストは 78/78 で AA を通っている。`--agent-chrome` と `--cyan-dim` は
**監査を通すために意図的に明るくした値**なので、暗く戻すと contrast エラーが復活する。

主人公は `#bunny` 内のインライン SVG（viewBox `0 0 120 160`）。
`../bunny-blitz-exp/Assets/BunnyBlitz/Game/UI/Textures/iconUIrabbit.png` を
参考にした簡略化グリフで、実素材は読み込んでいない（外部アセット依存ゼロ）。

---

## 9. 新セッションの入り方

```
/hyperframes
```

`videos/eval-code-to-coins/BRIEF.md` が存在するので、intent interview は走らず
`workflow: motion-graphics` / `flow: automation` として再開される。
既存プロジェクトへの編集・レンダー・プレビューは、ルーティングをスキップして
そのまま実行してよい（`/hyperframes` の state table の 2〜3 行目）。

ユーザーは日本語で応答する。前作 `unity-logo-sting` を含め、書き出しは
**1024×1024 / ProRes MOV** を標準としている。

---

## 10. git の扱い

`.gitignore` / `.gitattributes` はワークスペース直下に配置済み。
**`git init` はまだ実行していない**（ブランチ名・リモートの好みがあるはずなので保留）。

```bash
cd /Users/keijiro/Projects/hf-test
git init && git add . && git commit -m "eval 解説動画用素材アニメーション 第1版"
```

### 追跡するもの

`index.html`（本体）/ `BRIEF.md` / `shot-plan.json` / `HANDOFF.md` /
`hyperframes.json` / `package.json`（CLI ピン）/ `meta.json` /
`assets/` / `.media/` / `compositions/`

`.media/` を意図的に追跡対象にしているのは、media-use が凍結した素材が
**レンダーの入力**であり、追跡しておけばプロバイダに再アクセスせずに
同じ絵を再現できるため。

### 追跡しないもの

| 対象 | 理由 |
| --- | --- |
| `/.agents/`, `/.claude/skills/`, `/skills-lock.json` | スキル本体。`npx hyperframes skills update` で再取得できる |
| `videos/*/AGENTS.md`, `videos/*/CLAUDE.md` | スキルのルーターテキストを CLI がプロジェクトに焼いたもの。CLI 更新ごとに差分が出るだけで情報量がない |
| `renders/` | 最終生成物 |
| `snapshots/`, `.thumbnails/`, `transcript.json`, `*.log` | 中間生成物 |
| `.hyperframes/`, `node_modules/` | CLI のローカルキャッシュ・状態 |
| `.DS_Store` 他 | macOS |

### ハマりどころ

`.gitignore` は**行末コメントを解釈しない**。`snapshots/  # proof frames` と
書くと行全体がパターンになり、何にもマッチしなくなる（実際に一度踏んだ）。
コメントは必ず独立行に書くこと。

ルールを増やしたら、リポジトリを汚さずに検証できる:

```bash
# スクラッチに同じパス構造の空ファイルを作り、.gitignore だけコピーして
# git check-ignore -v <path> で1件ずつ期待どおりか確認する
```
