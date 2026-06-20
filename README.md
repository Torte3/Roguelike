![title](./docs/images/title.png)

# LogRogue

**LogRogue** は、ターン制・グリッドベースのローグライクゲームです。  
ランダム生成されるダンジョンを探索し、敵との戦闘やアイテム選択を通じて、より深い階層を目指します。

本作では、キャラクターのレベル上げや固定装備による成長よりも、  
**その場で手に入ったアイテム・武器・地形・敵の行動を使って、局面をどう切り抜けるか**  
という戦術的な判断を重視しています。

---

## Quick Links

| 目的 | リンク |
|---|---|
| まずプレイ内容を見る | [Gameplay](#gameplay) |
| 操作を確認する | [Controls](#controls) |
| Windows 版を遊ぶ | [Download](#download) |
| ゲームの特徴を知る | [Features](#features) |
| 設計意図を読む | [Design Highlights](#design-highlights) / [docs/design.md](./docs/design.md) |
| 技術的な工夫を読む | [Technical Highlights](#technical-highlights) / [docs/technical.md](./docs/technical.md) |
| アーキテクチャを見る | [Architecture](#architecture) / [docs/architecture.md](./docs/architecture.md) |

---

## Project Summary

| 項目 | 内容 |
|---|---|
| Genre | ターン制・グリッドベースのローグライク |
| Platform | Windows 10 / 11 |
| Engine / Language | Unity / C# |
| 実装規模 | 約40,000行 |
| 開発範囲 | ゲームルール、ダンジョン生成、敵AI、アイテム効果、UI、セーブ、制作用GUI |
| 公開目的 | ポートフォリオとして、ゲームデザイン・実装・設計判断を確認できるようにするため |

---

## Gameplay

最新ビルドでの、約5分のプレイ動画です。  
タイトルから始まり、通常階の探索・戦闘、店、ボス戦、モンスターハウス、仲間との行動、アイテム合成、設定などの各メニューまで、一通りの要素を確認できます。

<video src="./docs/videos/GamePlay.mp4" controls width="720"></video>

[動画を開く](./docs/videos/GamePlay.mp4)

---

## Download

Windows 向けビルドは GitHub Releases からダウンロードできます。

- Platform: Windows 10 / 11
- Latest version: `v0.1.0`
- Download: [Releases](../../releases)

### 実行方法

1. Releases から `LogRogue_Windows.zip` をダウンロード
2. zip ファイルを任意の場所に解凍
3. `LogRogue.exe` を起動

### Notes

- 初回起動時に Windows の警告が表示される場合があります。
- セーブデータは自動的に保存されます。
- このリポジトリでは、ライセンス上公開できない一部アセットを除外している場合があります。
- プレイ確認には、Releases の Windows 向けビルドを利用してください。

---

## Overview

LogRogue は、毎回異なる地形・敵・アイテムの組み合わせに対して、  
**今ある手札でどう立ち回るか** を判断するローグライクゲームです。

プレイヤーは、探索中に次のような判断を繰り返します。

- どの敵と戦い、どの敵を避けるか
- 強い武器を今使うか、後のために温存するか
- アイテムを使うか、投げるか、合成素材として残すか
- 危険な部屋を探索するか、安全に階段へ向かうか
- 地形や罠を利用して、戦闘を有利に進めるか

目指しているのは、  
**簡単に始められ、毎局面で「どう切り抜けるか」を考えられるローグライク**  
です。

---

## Features

- ターン制・グリッドベースのダンジョン探索
- ランダム生成されるフロア、敵、アイテム、イベント
- 使用回数を持つ武器と、消耗資源としてのアイテム管理
- アイテムの使用・投擲・合成・修理・強化
- ショップ、モンスターハウス、休憩部屋などの特殊部屋
- 敵の行動、地形、罠、草、配置物を利用した戦術判断
- 好感度や所属変化による、敵・味方・中立の関係性
- プレイ統計と条件達成によるプレイヤータイプ解放

---

## How to Play

### 目的

ダンジョンを探索し、敵やイベントを乗り越えながら、より深い階層を目指します。  
まずは 30F 到達を目標としています。30F 到達以降は、無限に生成されるランダムダンジョンとして続けて遊べます。

### 基本ルール

- プレイヤーと敵はターンごとに行動します。
- プレイヤーが 1 回行動すると、敵や環境もそれに応じて行動します。
- HP が 0 になるとゲームオーバーです。
- 武器や多くのアイテムには使用回数があります。
- アイテムは「使う」だけでなく、「投げる」ことで別の効果を発揮する場合があります。
- 一度訪れたマップの状態は保持され、階段などで戻ると同じ状態で再訪できます。

### Controls

| 操作 | キーボード | ゲームパッド（Xbox 配置） |
|---|---|---|
| 移動 | `W` `A` `S` `D` | 左スティック / 十字キー |
| アイテム選択 | `↑` `↓` | 右スティック（上下） |
| 使用 / 攻撃 | `Space` | `A` |
| ダッシュ | `Shift` + 方向 | `B` + 方向 |
| 向き変更のみ | `Ctrl` + 方向 | `Y` + 方向 |
| 斜め移動のみ | `Alt` + 方向 | `RB` + 方向 |
| 最寄りの敵を向く | `Ctrl` | `Y` |
| 入れ替え | `E` | `X` |
| 投げる | `Q` | `RT` |
| 名前変更 | `R` | `View` |
| 待機 | `X` | `A` + `B` |
| メニューを開く / 閉じる | `Tab` | `Start` |
| カーソル移動（メニュー） | 方向キー | 右スティック / 十字キー |
| 決定（メニュー） | `Space` | `A` |
| キャンセル（メニュー） | `Esc` | `B` |

- 使用・投げる・入れ替えなどは、選択中のアイテムに対して実行されます。
- 設定の「A/B・X/Yボタンを入れ替える」で、`A`/`B` と `X`/`Y` の割り当てを変更できます。

---

## Design Highlights

LogRogue では、ローグライクの面白さを  
**限られた手札で局面をどう切り抜けるか**  
という判断に置いています。

そのため、単に機能を増やすのではなく、プレイヤーの判断に直接つながりにくい操作や画面遷移はできるだけ削り、  
一方で、アイテム・敵・地形・罠・関係性による選択肢は残すようにしています。

主な設計判断は次の通りです。

- 武器を装備品ではなく、使用回数を持つ消耗アイテムとして扱う
- インベントリ・装備画面を廃止し、ゲーム画面から直接アイテムを使えるようにする
- 罠チェックのような「最適だが作業的になりやすい行動」を削り、判断だけを残す
- レベル上げより、敵やアイテムを理解してプレイヤー自身が上達する構造を重視する
- アイテム合成により、ラン中の短期目標と武器の性質変化を作る

詳しくは [docs/design.md](./docs/design.md) を参照してください。

---

## Technical Highlights

LogRogue は Unity / C# で開発しています。  
約40,000行規模の実装の中で、ゲームルール、ダンジョン生成、敵AI、アイテム効果、UI、セーブ、制作用GUIを実装しています。

主な技術的特徴は次の通りです。

- グラフと手続き生成を組み合わせたダンジョン生成
- 視界・状態・評価値を使う敵AI
- Field of View のキャッシュ化と `HashSet` 化による処理速度改善
- 発生位置・範囲・効果リストを組み合わせるスキル / アイテム効果システム
- ダンジョン・フロア・アイテム・敵を編集する制作用GUI
- レイヤ分離と Assembly Definition による依存方向の制御

### 技術的に工夫した点

| 項目 | 内容 |
|---|---|
| Dungeon Generation | ダンジョン全体をグラフとして扱い、分岐・再訪・特殊接続・無限区間を表現 |
| Enemy AI | 行動候補ごとに実際の影響を計算し、評価値として比較。敵も視野内情報のみで判断 |
| FOV Optimization | 視野計算のキャッシュ化と `HashSet` 化により、100ms 程度から 10ms 程度まで改善 |
| Skill / Effect System | 発生位置・範囲・効果リストを組み合わせ、攻撃・回復・状態異常・生成・破壊を表現 |
| Content Authoring Tools | Unity 上の専用GUIで敵・アイテム・フロア構造を編集し、調整の反復速度を向上 |

詳しくは [docs/technical.md](./docs/technical.md) を参照してください。

---

## Architecture

LogRogue では、ゲームルール、進行管理、表示、入力、データ定義を分離し、依存方向が大きく崩れないように Assembly Definition で制限しています。

基本方針は、**データ定義やインターフェースを内側に置き、外側の実装が内側の定義に依存する**構造です。

主なレイヤは次の通りです。

| Layer | 役割 |
|---|---|
| Domain.Model | 型、インターフェース、ScriptableObject、Memento などのデータ定義 |
| Domain.Service | 戦闘、敵AI、効果処理、マップ処理などのゲームルール |
| Game | World、Map、Turn、Save など、ゲーム全体の進行管理 |
| View | UI、演出、GameObject の表示制御 |
| Provider | DI、Presenter、入力、View とロジックの接続 |

この構成により、敵AI、効果処理、ターン進行などの中核ロジックが、Unity の表示処理や UI に引きずられないようにしています。

また、R3 によるイベント通知を使い、ロジック側が UI や GameObject を直接操作せず、Presenter や View 同期側が状態変化を購読して追従する形にしています。

詳しくは [docs/architecture.md](./docs/architecture.md) を参照してください。

---

## Development Background

本作は、最初から Unity で作り始めたものではありません。

最初は Rust の練習として、コンソールアプリ版のローグライクを作り始めました。  
その後、画像表現やゲームエンジンの仕組みに関心が広がり、Rust 製ゲームエンジンである Bevy を用いた版を制作しました。

Bevy 版では、ECS による設計やゲームロジックの実装に触れられた一方で、UI 実装の負荷や、開発中ライブラリの破壊的変更への追従が課題になりました。

最終的に、規模が大きくなった後の保守性や制作環境を考え、Unity へ移行しました。

Unity 版では一通り遊べる状態まで実装した後、既存実装への継ぎ足しではなく、設計思想を反映しやすい形に整理するため、再設計を行っています。

---

## Documentation

| Document | 内容 |
|---|---|
| [WIKI.md](./WIKI.md) | ゲームルール、アイテム、敵、セーブ、難易度、統計など |
| [docs/design.md](./docs/design.md) | ゲームデザイン上の判断理由 |
| [docs/technical.md](./docs/technical.md) | ダンジョン生成、敵AI、FOV、スキルシステム、制作GUI |
| [docs/architecture.md](./docs/architecture.md) | レイヤ構成、依存方向、asmdef、DI、Presenter |
| [docs/images/](./docs/images/) | README / docs 用画像・GIF |

---

## Repository Notes

このリポジトリは、ポートフォリオとして公開することを目的としています。

以下の理由により、リポジトリ単体では Unity 上で完全に再現・実行できない可能性があります。

- 有償アセットを除外している
- ライセンス上再配布できない外部アセットを除外している
- ビルド用設定や一部ローカル環境依存ファイルを含めていない場合がある

実際にプレイする場合は、GitHub Releases の Windows 向けビルドを利用してください。

---

## Future Work / Validation Points

今後は、LogRogue の設計意図が実際のプレイ体験として成立しているかを中心に検証・改善していきます。

- 武器・アイテム・地形・罠を組み合わせて局面を切り抜ける遊びが成立しているか
- レベル制や満腹度を廃止した影響として、成長感や探索リスクが不足していないか
- 武器消耗・アイテム供給・インベントリ容量が、適切なリソース管理になっているか
- アイテム合成や未識別アイテムが、固定解や理不尽になっておらず判断要素として機能しているか

---

<details>
<summary>Credits</summary>

本作では、以下のソフトウェア、ライブラリ、アセットを使用しています。

### Engine

| Name | Provider | License |
|---|---|---|
| Unity | Unity Technologies | Unity Terms |
| Universal Render Pipeline | Unity Technologies | Unity Terms |

### Software / Libraries

| Name | Author / Organization | License |
|---|---|---|
| UniTask | Cysharp | MIT License |
| R3 | Cysharp | MIT License |
| ObservableCollections | Cysharp | MIT License |
| VContainer | Cysharp | MIT License |
| xNode | Thor Brigsted | MIT License |
| Fang Auto Tile | ruccho | MIT License |
| SQLiteUnityKit | tetr4lab | MIT License |
| SQLite | SQLite Consortium | Public Domain |
| Unity In-game Debug Console | yasirkula | MIT License |
| LiteNetLib | RevenantX | MIT License |
| DOTween | Demigiant | Unity Asset Store License |
| Odin Inspector | Sirenix | Unity Asset Store License |
| Console Pro | — | Unity Asset Store License |
| Hot Reload | Singularity Group | Commercial License |

### Graphics / Visual Effects

| Asset | Author / Organization | License / Notes |
|---|---|---|
| マップチップ・キャラクター素材 | [ぴぽや](https://pipoya.net/) | 各素材の利用規約に従います |
| UIアイコン | Kenney | CC0 |
| モンスターアニメーション | [Pixel Mob! — Henry Software](https://henrysoftware.itch.io/pixel-mob) | CC0（itch.io版） / Unity Asset Store EULA（Asset Store版） |
| アイテムアイコン | [1000+ Fantasy RPG Icons — finalbossblues](https://finalbossblues.itch.io/icons) | Pro License（商用可・改変可・再配布不可・表記任意） |
| 状態異常エフェクト | 2D Pixel FX: StateEffect (Particle) — DDreamyPixelArt | Unity Asset Store License |

### Fonts

| Font | License |
|---|---|
| DotGothic16 | SIL Open Font License 1.1 |
| Noto Sans JP | SIL Open Font License 1.1 |
| PixelMplus12 | M+ FONT LICENSE |
| Liberation Sans | SIL Open Font License 1.1 |

※ Liberation Sans は TextMesh Pro に含まれるフォントとして使用しています。

### Music / Sound Effects

| Asset | Author / Organization | License / Notes |
|---|---|---|
| BGM / SE | [魔王魂](https://maou.audio/) | 魔王魂の利用規約に従います |
| 効果音 | [効果音ラボ](https://soundeffect-lab.info/) | 該当ファイルのみ使用。効果音ラボの利用規約に従います |

### Notes

一部の有償アセット、商用ライセンス素材、再配布が許可されていない素材は、このリポジトリには含めていません。  
プレイ用ビルドには、各ライセンスの範囲内で必要な素材を組み込んでいます。

---

</details>

---

## License

このリポジトリに含まれるソースコード、画像、音声、フォント、その他アセットの権利は、それぞれの作者・権利者に帰属します。

第三者製のライブラリ・アセット・フォント・音楽・効果音は、それぞれのライセンスまたは利用規約に従います。

ライセンスが明示されていない本作独自のファイルについては、無断での再利用・再配布を許可していません。

有償アセット、商用ライセンス素材、再配布不可の素材は、公開リポジトリには含めていません。
