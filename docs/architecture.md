# LogRogue Architecture

[READMEへ戻る](../README.md)

LogRogue では、ゲームルール、進行管理、表示、入力、データ定義を分離し、依存方向が大きく崩れないように Assembly Definition で制限しています。

基本方針は、**データ定義やインターフェースを内側に置き、外側の実装が内側の定義に依存する**構造です。

---

## Layer Overview

```mermaid
flowchart TD
    Provider["Provider<br/>DI / Presenter / Input"]

    View["View<br/>UI / 演出 / GameObject表示"]
    Game["Game<br/>World / Map / Turn / Save"]
    Service["Domain.Service<br/>戦闘 / 敵AI / 効果処理"]

    Model["Domain.Model<br/>型 / Interface / ScriptableObject / Memento"]

    Utilities["Utilities<br/>A* / FOV / 汎用処理"]
    Stats["Stats<br/>統計"]
    Settings["Settings<br/>設定"]

    Provider --> View
    Provider --> Game
    Provider --> Service
    Provider --> Model

    Game --> Service
    Game --> Model

    Service --> Model
    Service --> Settings

    Model --> Utilities
    Model --> Stats

    View --> Settings
    View --> Utilities
```

`Provider` は、DI や Presenter を通じて各層を接続し、WorldEvent を表示の指示へ変換する合成ルートです。  
`View` は UI、演出、GameObject の表示制御と、表示の指示の再生を担当します。  
`Game` は World、Map、Turn、Save など、ゲーム全体の進行を管理します。  
`Domain.Service` は、戦闘、敵AI、効果処理、マップ処理などのゲームルールを実装します。  
`Domain.Model` は、型、インターフェース、ScriptableObject、Memento などのデータ定義を持ちます。  
`Utilities`、`Stats`、`Settings` は、複数の層から利用される共通基盤です。

---

## Dependency Policy

依存方向は Assembly Definition によって制限しています。

特に、次のような依存を避けています。

- `Domain.Model` が `Game` や `View` を参照しない
- `Domain.Service` が `GameObject` や UI に依存しない
- `Game` が `View` を直接操作しない
- `View` が `Domain.Model`、`Domain.Service`、`Game`、`Provider` に直接依存しない

これにより、敵AI、効果処理、ターン進行などの中核ロジックが、Unity の表示処理や UI に引きずられないようにしています。

---

## Provider as Composition Root

`Provider` は、入力、Presenter、DI コンテナなどを通じて、`View`、`Game`、`Domain.Service`、`Domain.Model` を接続する層です。

VContainer を使い、GameManager、Presenter、WorldEvent の変換、デバッグコマンドなどを登録・注入しています。

この層に接続処理を集めることで、各機能が互いに直接参照しすぎないようにしています。

主な実装：

- [Container.cs](../Assets/Scripts/Provider/Container.cs)

---

## WorldEvent

ロジックと表示は、WorldEvent だけでつないでいます。

```mermaid
flowchart LR
    Logic["Domain / Game<br/>ターンを解決し WorldEvent を記録"]
    Stream["WorldEventStream<br/>R3 のストリーム"]
    View["表示側<br/>WorldEvent を読み、演出を再生"]

    Logic --> Stream --> View
```

### この形にした理由

ローグライクの1ターンでは、多くのキャラクターが続けて動きます。ロジックと演出を結びつけたまま進めると、演出の順番がロジックの処理順に縛られ、敵が多い場面では操作に引っかかりを覚えるなど、体験が悪くなります。また、表示の都合にゲームルールが依存してしまいます。

### 記録

ゲームルールは、状態を変えたその場で WorldEvent（移動・攻撃の命中・状態異常・所持品の変化など）を記録して先へ進みます。WorldEvent には、何が起きたかに加えて、ロジックの状態に関する値も載せているので、表示側はロジックの状態を読みに行かずに画面を作れます。

### 再生

表示側は WorldEvent を読み、演出を出します。順序関係のない演出はまとめて再生するため、他のキャラクターの行動を待つ時間がなくなり、テンポよく遊べます。演出を変えてもゲームルールには手を入れずに済みます。また、画面のログ・BGM・プレイ統計も、同じ WorldEvent から作っています。

主な実装：

- [WorldEvent.cs](../Assets/Scripts/Domain/Model/WorldEvents/WorldEvent.cs) /
  [WorldEventStream.cs](../Assets/Scripts/Domain/Model/WorldEvents/WorldEventStream.cs)
- [PlaybackQueue.cs](../Assets/Scripts/View/Playback/PlaybackQueue.cs)

---

## Difficulties and Design Choices

### 1. Unity の GameObject へ依存しすぎないこと

Unity では、MonoBehaviour や GameObject を中心に実装すると、ロジックと表示が密結合になりやすいです。

しかしローグライクでは、敵AI、視界計算、効果処理など、表示に依存しないロジックが多くあります。

そのため、ゲームルールはできるだけ `Domain.Service` や `Game` 側に置き、  
`View` は表示と演出に集中させるようにしました。

### 2. 分離しすぎによる実装コスト

層を細かく分けすぎると、個人開発では中間層や変換処理が増えすぎる問題があります。

そのため、すべてを理想的に抽象化するのではなく、  
`Provider` に接続処理を集めることで、依存関係を整理しつつ実装コストを抑えています。

### 3. データ駆動と型安全性の両立

アイテム、敵、スキル、フロア設定は ScriptableObject や専用エディタから調整できるようにしています。

一方で、スキルや効果は種類が多く、単純な enum や固定クラスだけでは拡張しにくくなります。

そのため、`SerializeReference` を使い、効果、範囲、発生位置などをポリモーフィックに組み合わせられるようにしています。

これにより、データ側で試作しやすくしつつ、コード側では共通インターフェースを通じて処理できるようにしています。
