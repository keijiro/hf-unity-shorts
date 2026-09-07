---
workflow: motion-graphics
flow: automation
storyboard: no
message: "エージェントの C# コードが、そのまま動いているゲームを書き換える"
destination: standalone clip (解説動画への差し込み素材)
aspect: "1:1"
length: ~7s
language: ja
export: mov (ProRes)
---

## Intent

Unity CLI の `eval` 機能の解説動画に差し込む無音・ナレーションなしの短尺素材。

コーディングエージェント (上) が C# のワンライナーを打ち終えると、そのコードが光る
「パケット」に凝縮され、下の Bunny Blitz へ弧を描いて飛んでいく。着弾した瞬間、
ゲーム側が反応し、主人公 (Bunny Blitz の実キャラクター) の周りにコインが湧き出して着地する。
HUD のコインカウンタが 0 → 8 に上がる。

因果の連鎖が主役: タイプ → 送信 → 飛行 → 着弾 → コイン生成 → カウンタ加算。

## Deliverable

- ネイティブ 1024x1024 (正方形) コンポジション
- ProRes MOV (`hyperframes render --format mov`)
- 背景は真っ黒 (#000) — ユーザー指定

## Assets

Bunny Blitz 画面は実画面キャプチャではなく**模式図**でという指定 —
フラットな板状のプラットフォーム、簡易 HUD。

ただし**主人公だけは実プロジェクトのキャラクターレンダー**を使う (2026-09-07 追加指定)。
`assets/bunny-render.png` = `Prefab_Rabbit` の透過切り抜き静止画 (442x800 RGBA)。
Unity Editor から正射投影・背景アルファ 0 で書き出したもので、
グリーンバックのキーイングは不要。再生成手順は `tools/render-bunny.cs`。
それ以外は引き続きコード/CSS/SVG で作画。

## Customizations

- パレットは Bunny Blitz のロゴ/アートから採取: 紫 #4B1E9E / 青 #1E9BF0 /
  黄 #FFC824 / ゴールド #E8A317。主人公は実レンダーなので色は素材そのまま
  (白い体 + マゼンタの髪 + シアンの目 + 青シャツ + 黄ブーツ)
- エージェント側はエディタ寄りのダークパネル (シアン系アクセント)

## Notes

- ナレーション・BGM なし (素材として編集側で音を付ける)
- `bounce.out` / `elastic.out` は使わない (motion-doctrine)
- 着弾とコイン生成の間に 0.35s の「溜め」を入れる (stillness-before-climax)
