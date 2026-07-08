#nullable enable
using Domain.Model.Character.Affiliation;
using Domain.Model.Entity;
using UnityEngine;

namespace Domain.Model.Map
{
    /// <summary>
    /// マップ上の1マスに対する各種判定。判定は次の3つの軸の組み合わせで考えると区別しやすい。
    ///
    /// 1) 地形タイプ（そのマスの地形そのものの性質。エンティティは考慮しない。末尾が OnMap）
    ///    - Walkable      … 立ち止まれる地形（床）。
    ///    - Passable      … 通過できる地形（床＋水など）。通れるが立てるとは限らない（水は通れるが立てない）。
    ///                       包含関係は Walkable ⊂ Passable。
    ///    - LightPassable … 光を通す地形（視界=FOV 計算で使う）。
    ///
    /// 2) エンティティの有無（Blank。地形ではなく、そのマスに物やキャラがいないか）
    ///    - 指定した EntityLayer 上にエンティティがいなければ blank。
    ///
    /// 3) 1) と 2) の組み合わせ
    ///    - IsBlank             … 通過できる地形 かつ エンティティ無し。
    ///    - IsBlankAndStandable … 立てる地形     かつ エンティティ無し。
    ///    - CanPlace            … 行動者の能力（飛行・壁抜け・エンティティ無視）に応じて上記を選ぶ統合判定。
    /// </summary>
    public interface IMapPosition
    {
        public Vector2Int Position { get; init; }

        /// <summary>地形を問わず、指定レイヤーにエンティティがいなければ true（壁の上でも可）。</summary>
        public bool IsBlankIgnoreWall(params EntityLayer[] layers);

        /// <summary>通過できる地形（Passable）で、かつ指定レイヤーにエンティティがいなければ true。</summary>
        public bool IsBlank(params EntityLayer[] layers);

        /// <summary>立てる地形（Walkable）で、かつ指定レイヤーにエンティティがいなければ true。</summary>
        public bool IsBlankAndStandable(params EntityLayer[] layers);

        /// <summary>
        /// 行動者の能力に応じて、このマスへ移動・設置できるかを総合判定する。
        /// isFlying: 飛行（水などの通過可能地形にも入れる）／canThroughWalls: 壁を抜けられる／
        /// ignoreEntity: エンティティの有無を無視する。
        /// </summary>
        public bool CanPlace(bool isFlying, bool canThroughWalls, bool ignoreEntity,
            params EntityLayer[] layers);

        /// <summary>
        /// actor から見て歩いて進めるマスか（経路探索用）。立てる地形であり、かつ中央レイヤーに
        /// 進行を妨げる相手（敵対キャラなど）がいないこと。地形だけを見る IsWalkableOnMap とは別物。
        /// </summary>
        public bool IsWalkable(IAffiliation actor);

        /// <summary>地形だけで見て立てるか（エンティティは考慮しない）。</summary>
        public bool IsWalkableOnMap();

        /// <summary>地形だけで見て通過できるか（エンティティは考慮しない）。</summary>
        public bool IsPassableOnMap();

        /// <summary>光を通すマスか（視界=FOV 計算で使う）。</summary>
        public bool IsLightPassable();
    }
}