#nullable enable
using System;
using System.Linq;
using Domain.Model.Character;
using Game;

namespace Provider
{
    /// <summary>
    /// デバッグコマンドで使用する共通ユーティリティ
    /// </summary>
    public static class CommandUtilities
    {
        /// <summary>
        /// ターゲット文字列からキャラクターを取得します
        /// </summary>
        /// <param name="target">ターゲット文字列（"Player"、"player"、またはGUID）</param>
        /// <param name="world">ワールドインスタンス</param>
        /// <returns>キャラクターインスタンス</returns>
        /// <exception cref="Exception">キャラクターが見つからない場合</exception>
        public static ICharacter GetTarget(string target, MapManager map)
        {
            if (target == "Player" || target == "player")
            {
                return map.Player.Character;
            }

            if (Guid.TryParse(target, out var guid))
            {
                var character =
                    map.Characters.FirstOrDefault(character =>
                        character.Entity.Id.ToString() == guid.ToString());
                if (character == null)
                {
                    throw new Exception("指定されたキャラクターが見つかりません。");
                }

                return character;
            }

            throw new Exception("不正なキャラクター指定です。");
        }
    }
}